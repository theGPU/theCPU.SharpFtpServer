using System.Net.Sockets;
using theCPU.SharpFtpServer.Exceptions;
using theCPU.SharpFtpServer.POCO;

namespace theCPU.SharpFtpServer.Server
{
    public enum FtpServerStage
    {
        Stopped,
        Stopping,
        Working
    }

    public interface IFtpServer
    {
        FtpServerStage Stage { get; }
        FtpServerConfig Config { get; }
        FtpServerCallbacks Callbacks { get; }

        Task Start(CancellationToken ct);
    }

    public class FtpServer : IFtpServer
    {
        public const string ServerHello = "SharpFtpServer v0.0.1 by theCPU";

        public FtpServerConfig Config { get; init; }
        private readonly Socket _serverSocket;

        private CancellationToken _cancellationToken;
        private Task _serverTask = null!;
        public FtpServerCallbacks Callbacks { get; init; }

        private readonly List<FtpClient> _activeClients = new List<FtpClient>();

        public FtpServerStage Stage { get; private set; } = FtpServerStage.Stopped;

        public FtpServer(FtpServerConfig config)
        {
            Config = config;
            Callbacks = new FtpServerCallbacks();

            _serverSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            _serverSocket.Bind(Config.EndPoint);

            _serverSocket.Listen();
        }

        public Task Start(CancellationToken ct)
        {
            var unlinkedCallbacks = Callbacks.GetUnlinkedCallbacks();
            if (unlinkedCallbacks.Any())
                throw new CallbacksUnlinkedException($"Unlinked callbacks found: {string.Join(',', unlinkedCallbacks)}");

            _cancellationToken = ct;
            _serverTask = Task.Run(MainWorker);
            return _serverTask;
        }

        public async Task WaitShutdown() => await _serverTask;

        private async Task MainWorker()
        {
            Stage = FtpServerStage.Working;
            while (!_cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var clientSocket = await _serverSocket.AcceptAsync(_cancellationToken);
                    await ProcessClient(clientSocket);
                } catch (System.OperationCanceledException) { }
            }

            Stage = FtpServerStage.Stopping;
            _activeClients.ForEach(x => x.Dispose());
            await Task.WhenAll(_activeClients.Select(x => x.WaitShutdown()));

            Stage = FtpServerStage.Stopped;
        }

        private async Task ProcessClient(Socket socket)
        {
            var clientCts = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken);
            var client = new FtpClient(this, socket, clientCts);
            _activeClients.Add(client);
        }
    }
}
