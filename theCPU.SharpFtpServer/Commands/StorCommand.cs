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
            await client.ReadData(out var dataStream, out var cts);
            var bytes = dataStream.ToArray();
            dataStream.Dispose();
            dataStream = null;
            await client.CloseDataChannel();

            if (server.Config.AutoGC)
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true);

            if (!server.Callbacks.CreateFile(client.Username, path, bytes))
                return FtpCommandResult.ClosingDataConnection;

            return FtpCommandResult.ClosingDataConnection;
        }
    }
}
