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
    internal class TypeCommand : BaseFtpCommand
    {
        public override async Task<FtpCommandResult> Invoke(IFtpServer server, IFtpClientControls client, string? args)
        {
            if (args == "A")
            {
                client.TransferType = ClientTransferType.Ascii;
                return FtpCommandResult.TypeAResponse;
            }
            else if (args == "I")
            {
                client.TransferType = ClientTransferType.Image;
                return FtpCommandResult.TypeIResponse;
            }

            return FtpCommandResult.CommandNotImplementedForThatParam;
        }
    }
}
