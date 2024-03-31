using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using theCPU.SharpFtpServer.POCO;
using theCPU.SharpFtpServer.Server;
using theCPU.SharpFtpServer.Utils;

namespace theCPU.SharpFtpServer.Commands
{
    internal abstract class BaseFtpCommand : IFtpCommand
    {
        public virtual string Name => this.GetType().Name.Replace("Command", "").ToUpper();
        public virtual byte Priority => 0;

        public abstract Task<FtpCommandResult> Invoke(IFtpServer server, IFtpClientControls client, string? args);

        protected string NormalizePath(string path) => PathUtils.NormalizePath(path);
        protected string NormalizePath(string workingDirectory, string path) => NormalizePath(Path.Join(path.StartsWith('\\') || path.StartsWith('/') ? "" : workingDirectory, path));
    }
}
