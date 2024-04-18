using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace theCPU.SharpFtpServer.POCO
{
    public class FtpCommandResult
    {
        private const string HelloMessage = "SharpFtpServer by theCPU";

        public int Code { get; set; }
        public string? Message { get; set; }

        public static FtpCommandResult ReadyForNewUser = new() { Code = 220, Message = HelloMessage };

        public static FtpCommandResult FeatStart = new() { Code = 211, Message = "-Features supported" };
        public static FtpCommandResult FeatEnd = new() { Code = 211, Message = "End" };

        public static FtpCommandResult UsernameOk = new() { Code = 331, Message = "User name okay, need password" };
        public static FtpCommandResult NotLoggedIn = new() { Code = 530, Message = "Not logged in" };
        public static FtpCommandResult UserLoggedIn = new() { Code = 230, Message = "User logged in" };

        public static FtpCommandResult DefaultSystCommandResult = new() { Code = 215, Message = HelloMessage };
        public static FtpCommandResult TypeAResponse = new() { Code = 200, Message = "Transfer type A" };
        public static FtpCommandResult TypeIResponse = new() { Code = 200, Message = "Transfer type I" };
        public static FtpCommandResult FileActionFailed = new() { Code = 450, Message = "Requested file action not taken" };
        public static FtpCommandResult FileActionComplete = new() { Code = 250, Message = "Requested file action okay, completed" };
        public static FtpCommandResult FileActionWaitMoreInfo = new() { Code = 350, Message = "Requested file action pending further information" };

        public static FtpCommandResult CommandNotImplemented = new() { Code = 502, Message = "Command not implemented" };
        public static FtpCommandResult CommandNotImplementedForThatParam = new() { Code = 504, Message = "Command not implemented for that parameter" };
        public static FtpCommandResult ActionAborted = new() { Code = 451, Message = "Requested action aborted. Local error in processing" };

        public static FtpCommandResult ClosingDataConnection = new() { Code = 226, Message = "Closing data connection. Requested file action successful" };

        public static FtpCommandResult PwdResponse(string currentWorkingDir) => new() { Code = 257, Message = $"\"{currentWorkingDir}\" is current working directory" };
        public static FtpCommandResult PasvResponse(IPEndPoint endpoint) => new() { Code = 227, Message = $"Entering Passive Mode ({string.Join(',', endpoint.Address.GetAddressBytes().TakeLast(4))},{string.Join(',', BitConverter.IsLittleEndian ? BitConverter.GetBytes((short)endpoint.Port).Reverse() : BitConverter.GetBytes((short)endpoint.Port))})" };
        public static FtpCommandResult PasvResponseLong(IPEndPoint endPoint) => new() { Code = 228, Message = $"Entering Long Passive Mode ({endPoint.Address.Address},{endPoint.Port})" };
        public static FtpCommandResult PasvResponseExtended(int port) => new() { Code = 229, Message = $"Entering Extended Passive Mode ({port})" };
        public static FtpCommandResult TransferResponseOpenConnection(ClientTransferType connectionType, string path) => new() { Code = 150, Message = $"Opening {connectionType} mode data connection for {path}" };
        public static FtpCommandResult TransferResponseOpenConnection(ClientTransferType connectionType, string path, int bytes) => new() { Code = 150, Message = $"Opening {connectionType} mode data connection for {path} ({bytes} bytes)" };
    }
}
