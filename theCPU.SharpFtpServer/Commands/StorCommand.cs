using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using theCPU.SharpFtpServer.POCO;
using theCPU.SharpFtpServer.Server;

namespace theCPU.SharpFtpServer.Commands
{
    internal class StorCommand : BaseFtpCommand
    {
        public override async Task<FtpCommandResult> Invoke(IFtpServer server, IFtpClientControls client, string? args)
        {
            if (string.IsNullOrWhiteSpace(args))
                return FtpCommandResult.FileActionFailed;

            var path = NormalizePath(client.WorkingDirectory, args);
            if (!server.Callbacks.CanCreateFile(client.Username, path))
                return FtpCommandResult.FileActionFailed;

            if (!await client.SetupDataChannel())
                Debugger.Break();

            await client.SendCommandMessage(FtpCommandResult.TransferResponseOpenConnection(client.TransferType, path));
            var dataStream = client.GetDataStream();
            var complete = server.Callbacks.CreateFile(client.Username, path, dataStream);
            await client.CloseDataChannel();

            if (server.Config.AutoGC)
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true);

            if (!complete)
                return FtpCommandResult.ClosingDataConnection;

            return FtpCommandResult.ClosingDataConnection;
        }
    }
}
