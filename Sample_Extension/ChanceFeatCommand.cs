using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using theCPU.SharpFtpServer.Commands.Base;
using theCPU.SharpFtpServer.POCO;
using theCPU.SharpFtpServer.Server;

namespace Sample_Extension
{
    public class ChanceFeatCommand : BaseFtpCommand, IFtpFeatureCommand
    {
        public string Annotation => $"{base.Name}";

        public override async Task<FtpCommandResult> Invoke(IFtpServer server, IFtpClientControls client, string? args)
        {
            return new FtpCommandResult { Code = 250, Message = $"{new Random().Next(0, 2) == 1}" };
        }
    }
}
