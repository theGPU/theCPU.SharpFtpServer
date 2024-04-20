using System;
using System.Threading.Tasks;
using theCPU.SharpFtpServer.Commands.Base;
using theCPU.SharpFtpServer.POCO;
using theCPU.SharpFtpServer.Server;

namespace Sample_Extension
{
    public class ChanceCommand : BaseFtpCommand
    {
        public override async Task<FtpCommandResult> Invoke(IFtpServer server, IFtpClientControls client, string? args)
        {
            return new FtpCommandResult { Code = 451, Message = $"This command is overwritten. You'll never see this message." };
        }
    }
}
