using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using theCPU.SharpFtpServer.Commands.Base;
using theCPU.SharpFtpServer.POCO;
using theCPU.SharpFtpServer.Server;
using theCPU.SharpFtpServer.Utils;

namespace theCPU.SharpFtpServer.Commands
{
    internal class DeleCommand : BaseFtpCommand
    {
        public override async Task<FtpCommandResult> Invoke(IFtpServer server, IFtpClientControls client, string? args)
        {
            if (string.IsNullOrWhiteSpace(args))
                return FtpCommandResult.FileActionFailed;

            var path = NormalizePath(client.WorkingDirectory, args);
            if (!server.Callbacks.DeleteFile(client.Username, path))
                return FtpCommandResult.FileActionFailed;

            return FtpCommandResult.FileActionComplete;
        }
    }
}
