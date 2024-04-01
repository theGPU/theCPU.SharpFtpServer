using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using theCPU.SharpFtpServer.POCO;
using theCPU.SharpFtpServer.Server;

namespace theCPU.SharpFtpServer.Commands
{
    internal class RetrCommand : BaseFtpCommand
    {
        public override async Task<FtpCommandResult> Invoke(IFtpServer server, IFtpClientControls client, string? args)
        {
            if (string.IsNullOrWhiteSpace(args))
                return FtpCommandResult.FileActionFailed;

            var path = NormalizePath(client.WorkingDirectory, args);
            if (!server.Callbacks.FileExist(client.Username, path))
                return FtpCommandResult.FileActionFailed;


            if (!await client.SetupDataChannel())
                Debugger.Break();

            await client.SendCommandMessage(FtpCommandResult.TransferResponseOpenConnection(client.TransferType, path));

            var fileStreamData = server.Callbacks.GetDownloadStream(client.Username, path);
            if (fileStreamData.Bytes == null)
                return FtpCommandResult.FileActionFailed;

            var uploadTask = client.SendData(fileStreamData.Bytes);
            await uploadTask;

            if (fileStreamData.AutoDispose)
                fileStreamData.Bytes.Dispose();

            await client.CloseDataChannel();

            if (server.Config.AutoGC)
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true);

            return FtpCommandResult.ClosingDataConnection;
        }
    }
}
