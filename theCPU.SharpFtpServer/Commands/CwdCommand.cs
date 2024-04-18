using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using theCPU.SharpFtpServer.Commands.Base;
using theCPU.SharpFtpServer.POCO;
using theCPU.SharpFtpServer.Server;

namespace theCPU.SharpFtpServer.Commands
{
    internal class CwdCommand : BaseFtpCommand
    {
        public override async Task<FtpCommandResult> Invoke(IFtpServer server, IFtpClientControls client, string? args)
        {
            if (string.IsNullOrWhiteSpace(args))
                return FtpCommandResult.FileActionFailed;

            args = NormalizePath(Path.Join(args.StartsWith('\\') || args.StartsWith('/') ? "" : client.WorkingDirectory, args));
            if (!server.Callbacks.DirectoryExist(client.Username, args))
                return FtpCommandResult.FileActionFailed;

            client.WorkingDirectory = args;
            return FtpCommandResult.FileActionComplete;
        }
    }
}
