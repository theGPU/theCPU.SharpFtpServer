# theCPU.SharpFtpServer
## Description

Fast minimalistic FTP server writed in pure sharp.

Works fine with FileZilla/WinSCP.

Maximum upload speed (in sample): ~750mb/s (10 threads) or ~350mb/s (single thread).

This library is intended to be used as an adapter for a virtual file system (memory, archives, remote storage, etc.) and for LAN use only.

You write your own [logic](https://github.com/theGPU/theCPU.SharpFtpServer/blob/master/theCPU.SharpFtpServer/Server/FtpServerCallbacks.cs) for interacting with files (Check the [sample](https://github.com/theGPU/theCPU.SharpFtpServer/blob/master/Sample/Program.cs)).

There is no active mode here and only the normal FTP protocol is supported.
## Usage

```C#
#Load all assemblyes with commands before invoke Init
CommandRegistar.Init();

#Create FTP server
var ftpConfig = new FtpServerConfig(new IPEndPoint(IPAddress.Loopback, 2121), 512, true);
var ftpServer = new FtpServer(ftpConfig);

ftpServer.Callbacks.OnCheckUsername += (username) => ...;
ftpServer.Callbacks.OnCheckPassword += (username, password) => ...;
ftpServer.Callbacks.OnGetDefaultWorkingDirectory += (username) => "/";
ftpServer.Callbacks.OnDirectoryExist += (username, path) => ...;
ftpServer.Callbacks.OnFileExist += (username, path) => ...;
ftpServer.Callbacks.OnGetDirectoryEntries += ...;
ftpServer.Callbacks.OnDownloadFile += ...;
ftpServer.Callbacks.OnDeleteFile += (username, path) => ...;
ftpServer.Callbacks.OnFileRename += (username, oldPath, newPath) => ...;
ftpServer.Callbacks.OnCanCreateFile += (username, path) => ...;
ftpServer.Callbacks.OnCreateFile += (username, path, bytes) => ...;
ftpServer.Callbacks.OnCreateDirectory += (username, path) => ...;
ftpServer.Callbacks.OnDeleteDirectory += (username, path) => ...;

var cts = new CancellationTokenSource();
var serverTask = ftpServer.Start(cts.Token);

Console.ReadLine();
cts.Cancel();
await ftpServer.WaitShutdown();
```


## Extending
You can take a look at this [example](https://github.com/theGPU/theCPU.SharpFtpServer/tree/master/Sample_Extension)

Simply create an assembly and use [BaseFtpCommand](https://github.com/theGPU/theCPU.SharpFtpServer/blob/master/theCPU.SharpFtpServer/Commands/Base/BaseFtpCommand.cs) or [IFtpCommand](https://github.com/theGPU/theCPU.SharpFtpServer/blob/master/theCPU.SharpFtpServer/Commands/Base/IFtpCommand.cs) to create commands. 

Add [IFtpAnonymousCommand](https://github.com/theGPU/theCPU.SharpFtpServer/blob/master/theCPU.SharpFtpServer/Commands/Base/IFtpAnonymousCommand.cs) to make the command available to be invoked without authorization ([USER](https://github.com/theGPU/theCPU.SharpFtpServer/blob/master/theCPU.SharpFtpServer/Commands/UserCommand.cs) and [PASS](https://github.com/theGPU/theCPU.SharpFtpServer/blob/master/theCPU.SharpFtpServer/Commands/PassCommand.cs)).

Add [IFtpFeatureCommand](https://github.com/theGPU/theCPU.SharpFtpServer/blob/master/theCPU.SharpFtpServer/Commands/Base/IFtpFeatureCommand.cs) if it is an extension command that should be present in the result [FEAT](https://github.com/theGPU/theCPU.SharpFtpServer/blob/master/theCPU.SharpFtpServer/Commands/FeatCommand.cs) ([MLSD](https://github.com/theGPU/theCPU.SharpFtpServer/blob/master/theCPU.SharpFtpServer/Commands/Features/MlsdCommand.cs)).

Note that when using the [BaseFtpCommand](https://github.com/theGPU/theCPU.SharpFtpServer/blob/master/theCPU.SharpFtpServer/Commands/Base/BaseFtpCommand.cs) class, the name of the command class must end with "[Command](https://github.com/theGPU/theCPU.SharpFtpServer/blob/master/theCPU.SharpFtpServer/Commands/Base/BaseFtpCommand.cs#L14)", if it does not, you will have to override the [Name](https://github.com/theGPU/theCPU.SharpFtpServer/blob/master/theCPU.SharpFtpServer/Commands/Base/BaseFtpCommand.cs#L14) getter

Note that any default command can be overwritten using [BaseFtpCommand.Priority](https://github.com/theGPU/theCPU.SharpFtpServer/blob/master/theCPU.SharpFtpServer/Commands/Base/BaseFtpCommand.cs#L15)([IFtpCommand.Priority](https://github.com/theGPU/theCPU.SharpFtpServer/blob/master/theCPU.SharpFtpServer/Commands/Base/IFtpCommand.cs#L14)) >0

Note that [CommandRegistar.Init](https://github.com/theGPU/theCPU.SharpFtpServer/blob/master/theCPU.SharpFtpServer/Server/CommandRegistar.cs#L21) should be called after all assemblies with commands have been loaded ([Sample](https://github.com/theGPU/theCPU.SharpFtpServer/blob/master/Sample/Program.cs#L23))
## License

[MIT](https://choosealicense.com/licenses/mit/)