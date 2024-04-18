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
    internal class UserCommand : BaseFtpCommand, IFtpAnonymousCommand
    {
        public override async Task<FtpCommandResult> Invoke(IFtpServer server, IFtpClientControls client, string? args)
        {
            if (!server.Callbacks.CheckUsername(args))
                return FtpCommandResult.NotLoggedIn;

            client.Username = args;
            return FtpCommandResult.UsernameOk;
        }
    }
}
