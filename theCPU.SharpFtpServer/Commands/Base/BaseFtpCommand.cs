using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using theCPU.SharpFtpServer.POCO;
using theCPU.SharpFtpServer.Server;
using theCPU.SharpFtpServer.Utils;

namespace theCPU.SharpFtpServer.Commands.Base
{
    internal abstract class BaseFtpCommand : IFtpCommand
    {
        public virtual string Name => GetType().Name.Replace("Command", "").ToUpper();
        public virtual byte Priority => 0;

        public abstract Task<FtpCommandResult> Invoke(IFtpServer server, IFtpClientControls client, string? args);

        public virtual Task<bool> PreInvoke(IFtpServer server, IFtpClientControls client, string? args)
        {
            server.Logger.LogTrace(this, $"PreInvoke on client {client} with args: {args}");
            return Task.FromResult(true);
        }

        public virtual Task PostInvoke(IFtpServer server, IFtpClientControls client, string? args)
        {
            server.Logger.LogTrace(this, $"PostInvoke on client {client}");
            return Task.CompletedTask;
        }

        protected string NormalizePath(string path) => PathUtils.NormalizePath(path);
        protected string NormalizePath(string workingDirectory, string path) => NormalizePath(Path.Join(path.StartsWith('\\') || path.StartsWith('/') ? "" : workingDirectory, path));
    }
}
