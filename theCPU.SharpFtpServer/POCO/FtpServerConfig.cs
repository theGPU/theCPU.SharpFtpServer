using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace theCPU.SharpFtpServer.POCO
{
    public class FtpServerConfig
    {
        public EndPoint EndPoint { get; init; }
        public int ClientsBufferSize { get; init; }
        public bool AutoGC { get; init; }

        public FtpServerConfig(EndPoint endPoint, int clientsBufferSize, bool autoGC = true)
        {
            EndPoint = endPoint;
            ClientsBufferSize = clientsBufferSize;
            AutoGC = autoGC;
        }
    }
}
