using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace theCPU.SharpFtpServer.POCO
{
    public class FtpEntryInfo
    {
        public FtpEntryInfo(bool isDirectory, string name, DateTime lastWriteTime, long length )
        {
            IsDirectory = isDirectory;
            Name = name;
            LastWriteTime = lastWriteTime;
            Length = length;
        }

        public bool IsDirectory { get; init; }
        public string Name { get; init; }
        public DateTime LastWriteTime { get; init; }
        public long Length { get; init; }

        public string Serialize() => $"{(IsDirectory ? "drwxr-xr-x" : "-rw-r--r--")}    {(IsDirectory ? 2 : 1)} 1001     1001     {Length} {LastWriteTime.ToString("MMM dd  yyyy", CultureInfo.InvariantCulture)} {Name}";
    }
}
