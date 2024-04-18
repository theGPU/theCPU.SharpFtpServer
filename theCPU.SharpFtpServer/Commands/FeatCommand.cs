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
    internal class FeatCommand : BaseFtpCommand
    {
        public override async Task<FtpCommandResult?> Invoke(IFtpServer server, IFtpClientControls client, string? args)
        {
            await client.SendCommandMessage(FtpCommandResult.FeatStart);

            var commandsAnnotations = CommandRegistar.Features!.Values;
            foreach (var annotation in commandsAnnotations)
                await client.SendCommandMessage($" {annotation}");

            return FtpCommandResult.FeatEnd;
        }
    }
}
