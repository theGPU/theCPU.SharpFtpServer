using Sample.VirtualFS;
using System.Collections.Concurrent;
using System.Net;
using System.Reflection;
using System.Text;
using theCPU.SharpFtpServer.POCO;
using theCPU.SharpFtpServer.Server;

namespace Sample
{
    internal class Program
    {
        static readonly HttpClient _client = new HttpClient();
        static VirtualFsDirectory _virtualFsRoot = null!;

        static async Task Main(string[] args)
        {
            InitVirtualFs();

            var ftpConfig = new FtpServerConfig(new IPEndPoint(IPAddress.Loopback, 2121), 512, true);
            var ftpServer = new FtpServer(ftpConfig);

            Assembly.Load(File.ReadAllBytes("Sample_Extension.dll"), File.ReadAllBytes("Sample_Extension.pdb"));
            CommandRegistar.Init();

            ftpServer.Callbacks.OnCheckUsername += (username) => username.Equals("anonymous", StringComparison.InvariantCultureIgnoreCase);
            ftpServer.Callbacks.OnCheckPassword += (username, password) => true;
            ftpServer.Callbacks.OnGetDefaultWorkingDirectory += (username) => "/";
            ftpServer.Callbacks.OnDirectoryExist += (username, path) => _virtualFsRoot.IsDirectoryExist(path);
            ftpServer.Callbacks.OnFileExist += (username, path) => path != "/NotFound.txt" && _virtualFsRoot.IsFileExist(path);
            ftpServer.Callbacks.OnGetDirectoryEntries += OnGetDirectoryEntries;
            ftpServer.Callbacks.OnDownloadFile += OnDownloadFile;
            ftpServer.Callbacks.OnDeleteFile += (username, path) => _virtualFsRoot.DeleteFile(path);
            ftpServer.Callbacks.OnFileRename += (username, oldPath, newPath) => true;
            ftpServer.Callbacks.OnCanCreateFile += (username, path) => true;
            ftpServer.Callbacks.OnCreateFile += (username, path, bytes) => _virtualFsRoot.TryCreateFile(path, bytes, true, out _);
            ftpServer.Callbacks.OnCreateDirectory += (username, path) => _virtualFsRoot.CreateDirectories(path) != null;
            ftpServer.Callbacks.OnDeleteDirectory += (username, path) => _virtualFsRoot.DeleteDirectory(path);

            ftpServer.Logger.LogLevel = LogLevel.Debug;
            ftpServer.Logger.Callback = (level, message) => Console.WriteLine($"[{level}]: {message}");

            var cts = new CancellationTokenSource();
            var serverTask = ftpServer.Start(cts.Token);

            Console.WriteLine("FTP server running");
            Console.ReadLine();
            cts.Cancel();
            await ftpServer.WaitShutdown();
        }

        private static void InitVirtualFs()
        {
            var normalFileContent = Encoding.UTF8.GetBytes("This is single line test message for \"*/Normal.txt\" files");

            _virtualFsRoot = new() { Name = "" };
            _virtualFsRoot.TryCreateFile("/Error.txt", [], true, out _);
            _virtualFsRoot.TryCreateFile("/NotFound.txt", [], true, out _);
            _virtualFsRoot.TryCreateFile("/Normal.txt", normalFileContent, true, out _);
            _virtualFsRoot.TryCreateFile("/Empty.txt", [], true, out _);
            _virtualFsRoot.TryCreateFile("/Google.txt", [], true, out _);
            _virtualFsRoot.TryCreateFile("/GoogleStream.txt", [], true, out _);
            _virtualFsRoot.TryCreateFile("/NormalDirectory/Normal.txt", normalFileContent, true, out _);
            _virtualFsRoot.TryCreateFile("/NormalDirectory/NormalDirectory/Normal.txt", normalFileContent, true, out _);

            Console.WriteLine("Sample virtualFs structure:");
            foreach (var entry in _virtualFsRoot.GetEntriesRecursive().Skip(1))
                Console.WriteLine($"{entry.Path}{(entry.IsDirectory ? "/" : "")}");
        }

        private static DirectoryListResponse OnGetDirectoryEntries(string username, string path)
        {
            if (_virtualFsRoot.TryGetDirectory(path, out var dir))
                return new DirectoryListResponse(true, dir.GetEntries().Select(x => new FtpEntryInfo(x.IsDirectory, x.Name, DateTime.Now, x.Length)));

            return new DirectoryListResponse(false, null);
        }

        private static DownloadFileResponse OnDownloadFile(string username, string path)
        {
            if (path == "/Error.txt")
                return new DownloadFileResponse(null);

            if (path == "/Google.txt")
            {
                var result = new BlockingCollection<byte[]>();
                result.Add(_client.GetByteArrayAsync("https://www.google.com").Result);
                result.CompleteAdding();
                return new DownloadFileResponse(result, true);
            }

            if (path == "/GoogleStream.txt")
            {
                var result = new BlockingCollection<byte[]>();
                Task.Run(() => DownloadGoogleStream(result));
                return new DownloadFileResponse(result, true);
            }

            if (_virtualFsRoot.TryGetFile(path, out var file))
            {
                var result = new BlockingCollection<byte[]>();
                result.Add(file.Data);
                result.CompleteAdding();
                return new DownloadFileResponse(result);
            }

            return new DownloadFileResponse(null);
        }

        private static async Task DownloadGoogleStream(BlockingCollection<byte[]> result)
        {
            var stream = await _client.GetStreamAsync("https://www.google.com");
            var buffer = new byte[128];
            while (true)
            {
                var readed = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (readed == 0)
                    break;

                result.Add(buffer[0..readed]);
            }

            result.CompleteAdding();
        }
    }
}
