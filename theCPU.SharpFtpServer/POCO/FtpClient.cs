using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using theCPU.SharpFtpServer.Server;

namespace theCPU.SharpFtpServer.POCO
{
    public enum ClientTransferType
    {
        Ascii,
        Image
    }

    public enum ClientConnectionType
    {
        Active,
        Passive
    }

    public interface IFtpClient
    {
        FtpServer FtpServer { get; }
        Task ReaderTask { get; }

        string Username { get; }
        string WorkingDirectory { get; }

        ClientTransferType TransferType { get; }
        ClientConnectionType ConnectionType { get; }

        IPEndPoint? PassiveSocketEndpoint { get; }
    }

    public interface IFtpClientControls : IFtpClient
    {
        string Username { get; set; }
        string WorkingDirectory { get; set; }

        ClientTransferType TransferType { get; set; }
        ClientConnectionType ConnectionType { get; set; }

        string RenameFromPath { get; set; }

        Task<Socket> EnterPassiveMode();
        Task ExitPassiveMode();

        Task<bool> SetupDataChannel();
        Task CloseDataChannel();

        Task ReadData(out MemoryStream bufferStream, out CancellationTokenSource cts);

        Task SendData(Stream stream, out CancellationTokenSource cts);
        Task SendData(FtpCommandResult result);
        Task SendData(string message);
        Task SendData(IEnumerable<byte> data);

        Task SendCommandMessage(FtpCommandResult result);
        Task SendCommandMessage(string message);
        Task SendCommandMessage(IEnumerable<byte> data);
    }

    internal class FtpClient : IFtpClientControls, IDisposable
    {
        public FtpServer FtpServer { get; init; }
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly CancellationToken _cancellationToken;

        private Socket _socket = null!;
        public EndPoint LocalEndPoint => _socket.LocalEndPoint!;
        public EndPoint RemoteEndPoint => _socket.RemoteEndPoint!;

        private Socket? _passiveSocket;
        private Socket? _passiveChannel;
        public IPEndPoint? PassiveSocketEndpoint => _passiveSocket?.LocalEndPoint as IPEndPoint;

        public Task ReaderTask { get; init; }
        private byte[] _readBuffer;
        private string _commandBuffer;
        private string _command;

        public string Username { get; set; }
        public string WorkingDirectory { get; set; }
        public ClientTransferType TransferType { get; set; } = ClientTransferType.Ascii;
        public ClientConnectionType ConnectionType { get; set; } = ClientConnectionType.Active;

        public string RenameFromPath { get; set; }

        internal FtpClient(FtpServer server, Socket socket, CancellationTokenSource cts)
        {
            FtpServer = server;
            _socket = socket;
            _cancellationTokenSource = cts;
            _cancellationToken = cts.Token;
            _readBuffer = new byte[server.Config.ClientsBufferSize];
            ReaderTask = Task.Run(ReaderWorker);
        }

        public async Task WaitShutdown() => await Task.WhenAll(ReaderTask);

        private async Task ReaderWorker()
        {
            await SendCommandMessage(FtpCommandResult.ReadyForNewUser);

            while (!_cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var readed = await _socket.ReceiveAsync(_readBuffer, _cancellationToken);
                    var readedMessage = Encoding.UTF8.GetString(_readBuffer, 0, readed);
                    _commandBuffer = _commandBuffer + readedMessage;

                    var nextMessageEndIndex = _commandBuffer.IndexOf('\n');
                    while (nextMessageEndIndex >= 0)
                    {
                        _command = _commandBuffer.Substring(0, _commandBuffer[nextMessageEndIndex] == '\r' ? nextMessageEndIndex - 2 : nextMessageEndIndex - 1);
                        _commandBuffer = _commandBuffer.Remove(0, nextMessageEndIndex + 1);
                        nextMessageEndIndex = _commandBuffer.IndexOf('\n');
                        await ProcessCommand();
                    }

                    await Task.Delay(10);
                } 
                catch (Exception ex)
                {
                    _cancellationTokenSource.Cancel();
                    return;
                }
            }
        }

        private async Task ProcessCommand()
        {
            Console.WriteLine(_command);
            var commandData = _command.Split(' ', 2);
            if (!CommandRegistar.Handlers!.TryGetValue(commandData[0], out var commandHandler))
            {
                await SendCommandMessage(FtpCommandResult.CommandNotImplemented);
                return;
            }

            var commandResult = await commandHandler.Invoke(FtpServer, this, commandData.Length > 1 ? commandData[1] : null);
            await SendCommandMessage(commandResult);
        }

        public async Task<Socket> EnterPassiveMode()
        {
            ConnectionType = ClientConnectionType.Passive;
            if (_passiveSocket != null)
                return _passiveSocket;

            _passiveSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);

            _passiveSocket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
            _passiveSocket.Listen();

            return _passiveSocket;
        }

        public async Task ExitPassiveMode()
        {
            ConnectionType = ClientConnectionType.Active;

            _passiveSocket!.Dispose();
            _passiveSocket = null;
        }

        public async Task<bool> SetupDataChannel()
        {
            _passiveChannel?.Dispose();
            var cts = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken);
            cts.CancelAfter(1000);
            var socket = await _passiveSocket!.AcceptAsync(cts.Token);
            _passiveChannel = socket;
            return _passiveChannel != null;
        }

        public async Task CloseDataChannel()
        {
            _passiveChannel?.Shutdown(SocketShutdown.Send);
            _passiveChannel?.Close();
        }

        public Task ReadData(out MemoryStream bufferStream, out CancellationTokenSource cts)
        {
            var ctsInternal = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken);
            cts = ctsInternal;
            var internalBufferStream = new MemoryStream();
            bufferStream = internalBufferStream;
            return Task.Run(() => ReadDataInternal(internalBufferStream, ctsInternal));
        }
        internal async Task ReadDataInternal(MemoryStream bufferStream, CancellationTokenSource cts)
        {
            try
            {
                var ct = cts.Token;
                var buffer = new byte[4096];
                while (!ct.IsCancellationRequested)
                {
                    cts.CancelAfter(5000);
                    var readedBytes = await _passiveChannel!.ReceiveAsync(buffer, ct);
                    cts.CancelAfter(-1);
                    if (readedBytes == 0)
                        return;
                    bufferStream.Write(buffer, 0, readedBytes);
                }
            } catch (OperationCanceledException)
            {
                return;
            }
        }

        public Task SendData(Stream stream, out CancellationTokenSource cts)
        {
            var ctsInternal = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken);
            cts = ctsInternal;
            return Task.Run(() => SendDataInternal(stream, ctsInternal));
        }

        internal async Task SendDataInternal(Stream stream, CancellationTokenSource cts)
        {
            try
            {
                var ct = cts.Token;
                var buffer = new byte[4096];
                while (!ct.IsCancellationRequested)
                {
                    cts.CancelAfter(5000);
                    var readedBytes = await stream.ReadAsync(buffer, 0, 4096, ct);
                    cts.CancelAfter(-1);
                    if (readedBytes == 0)
                        return;
                    await _passiveChannel!.SendAsync(buffer.AsMemory(0, readedBytes), ct);
                    //_passiveChannel!.Send(buffer, 0, readedBytes, SocketFlags.None);
                }
            } 
            catch (Exception) { }
        }
        public async Task SendData(FtpCommandResult result) => await SendData($"{result.Code} {result.Message}");
        public async Task SendData(string message) => await SendData(Encoding.UTF8.GetBytes(message+'\n'));
        public async Task SendData(IEnumerable<byte> bytes) => await _passiveChannel!.SendAsync(bytes.ToArray(), _cancellationToken); //_passiveChannel!.Send(bytes.ToArray()); //await _passiveChannel!.SendAsync(bytes.ToArray(), _cancellationToken);

        public async Task SendCommandMessage(FtpCommandResult result) => await SendCommandMessage($"{result.Code} {result.Message}");
        public async Task SendCommandMessage(string message) => await SendCommandMessage(Encoding.UTF8.GetBytes(message+'\n'));
        public async Task SendCommandMessage(IEnumerable<byte> bytes) => await _socket.SendAsync(bytes.ToArray(), _cancellationToken); //_socket.Send(bytes.ToArray()); //await _socket.SendAsync(bytes.ToArray(), _cancellationToken);
        public override string ToString() => this.Username;

        public void Dispose()
        {
            _socket?.Close();
            _socket?.Dispose();
            _passiveSocket?.Dispose();
            _passiveChannel?.Dispose();
        }
    }
}
