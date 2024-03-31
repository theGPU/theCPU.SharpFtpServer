using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using theCPU.SharpFtpServer.POCO;
using theCPU.SharpFtpServer.Server;

namespace theCPU.SharpFtpServer.Commands
{
    internal class RestCommand : BaseFtpCommand
    {
        public override async Task<FtpCommandResult> Invoke(IFtpServer server, IFtpClientControls client, string? args)
        {
            return FtpCommandResult.CommandNotImplemented;
        }
    }
}
