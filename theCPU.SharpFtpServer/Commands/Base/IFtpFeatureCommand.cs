using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace theCPU.SharpFtpServer.Commands.Base
{
    public interface IFtpFeatureCommand : IFtpCommand
    {
        public string Annotation { get; }
    }
}
