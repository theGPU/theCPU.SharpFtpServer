using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using theCPU.SharpFtpServer.POCO;

namespace theCPU.SharpFtpServer.Server
{
    public record DirectoryListResponse(bool IsDirectoryExist, IEnumerable<FtpEntryInfo>? Entries);
    public record DownloadFileResponse(BlockingCollection<byte[]>? Bytes, bool AutoDispose = true);

    public delegate bool CheckUsernameDelegte(string username);
    public delegate bool CheckPasswordDelegte(string username, string password);

    public delegate bool DirectoryExistDelegate(string username, string path);
    public delegate bool FileExistDelegate(string username, string path);

    public delegate string GetDefaultWorkingDirectoryDelegate(string username);
    public delegate DirectoryListResponse GetDirectoryEntriesDelegate(string username, string path);

    public delegate DownloadFileResponse DownloadFileDelegate(string username, string path);
    public delegate bool DeleteFileDelegate(string username, string path);
    public delegate bool FileRenameDelegate(string username, string oldPath, string newPath);
    public delegate bool CanCreateFileDelegate(string username, string path);
    public delegate bool CreateFileDelegate(string username, string path, NetworkStream stream);
    public delegate bool CreateDirectoryDelegate(string username, string path);
    public delegate bool DeleteDirectoryDelegate(string username, string path);

    public class FtpServerCallbacks
    {
        public event CheckUsernameDelegte OnCheckUsername = null!;
        public event CheckPasswordDelegte OnCheckPassword = null!;

        public event DirectoryExistDelegate OnDirectoryExist = null!;
        public event FileExistDelegate OnFileExist = null!;

        public event GetDefaultWorkingDirectoryDelegate OnGetDefaultWorkingDirectory = null!;
        public event GetDirectoryEntriesDelegate OnGetDirectoryEntries = null!;

        public event DownloadFileDelegate OnDownloadFile = null!;
        public event DeleteFileDelegate OnDeleteFile = null!;
        public event FileRenameDelegate OnFileRename = null!;
        public event CanCreateFileDelegate OnCanCreateFile = null!;
        public event CreateFileDelegate OnCreateFile = null!;
        public event CreateDirectoryDelegate OnCreateDirectory = null!;
        public event DeleteDirectoryDelegate OnDeleteDirectory = null!;

        public bool CheckUsername(string username) => OnCheckUsername.Invoke(username);
        public bool CheckPassword(string username, string password) => OnCheckPassword.Invoke(username, password);

        public bool DirectoryExist(string username, string path) => OnDirectoryExist.Invoke(username, path);
        public bool FileExist(string username, string path) => OnFileExist.Invoke(username, path);

        public string GetDefaultWorkingDirectory(string username) => OnGetDefaultWorkingDirectory.Invoke(username);
        public DirectoryListResponse GetDirectoryEntries(string username, string path) => OnGetDirectoryEntries.Invoke(username, path);

        public DownloadFileResponse GetDownloadStream(string username, string path) => OnDownloadFile.Invoke(username, path);
        public bool DeleteFile(string username, string path) => OnDeleteFile.Invoke(username, path);
        public bool RenameFile(string username, string oldPath, string newPath) => OnFileRename(username, oldPath, newPath);
        public bool CanCreateFile(string username, string path) => OnCanCreateFile(username, path);
        public bool CreateFile(string username, string path, NetworkStream stream) => OnCreateFile(username, path, stream);
        public bool CreateDirectory(string username, string path) => OnCreateDirectory(username, path);
        public bool DeleteDirectory(string username, string path) => OnDeleteDirectory(username, path);

        public IEnumerable<string> GetUnlinkedCallbacks() => this.GetType()
            .GetEvents((BindingFlags)60)
            .Where(x => this.GetType().GetField(x.Name, (BindingFlags)60)!.GetValue(this) == null)
            .Select(x => x.Name);
    }
}
