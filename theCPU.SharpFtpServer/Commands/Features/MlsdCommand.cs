using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using theCPU.SharpFtpServer.Commands.Base;
using theCPU.SharpFtpServer.POCO;
using theCPU.SharpFtpServer.Server;

namespace theCPU.SharpFtpServer.Commands.Features
{
    internal class MlsdCommand : BaseFtpCommand, IFtpFeatureCommand
    {
        public string Annotation => $"{this.Name}";

        public override async Task<FtpCommandResult> Invoke(IFtpServer server, IFtpClientControls client, string? args)
        {
            var path = NormalizePath(args ?? client.WorkingDirectory);
            var directoryData = server.Callbacks.GetDirectoryEntries(client.Username, path);
            if (!directoryData.IsDirectoryExist)
                return FtpCommandResult.FileActionFailed;

            if (!await client.SetupDataChannel())
                Debugger.Break();

            var data = String.Join('\n', directoryData.Entries!.Select(x => x.MSerialize()));
            await client.SendCommandMessage(FtpCommandResult.TransferResponseOpenConnection(client.TransferType, this.Name, Encoding.UTF8.GetByteCount(data)));
            await client.SendData(data);
            server.Logger.LogDebug(this, $"Sending List data to client {client}\n{data}");
            await client.CloseDataChannel();
            return FtpCommandResult.ClosingDataConnection;
        }
    }
}
