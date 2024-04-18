using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using theCPU.SharpFtpServer.POCO;
using theCPU.SharpFtpServer.Server;

namespace theCPU.SharpFtpServer.Commands.Base
{
    public interface IFtpCommand
    {
        string Name { get; }
        byte Priority { get; }

        Task<FtpCommandResult> Invoke(IFtpServer server, IFtpClientControls client, string? args);
        Task<bool> PreInvoke(IFtpServer server, IFtpClientControls client, string? args);
        Task PostInvoke(IFtpServer server, IFtpClientControls client, string? args);
    }
}
