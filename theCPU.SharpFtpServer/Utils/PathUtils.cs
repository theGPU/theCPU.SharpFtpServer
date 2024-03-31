using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace theCPU.SharpFtpServer.Utils
{
    public static class PathUtils
    {
        public static string NormalizePath(string path) 
        {
            path = path.Trim().Replace("\\\\", "/").Replace('\\', '/').Replace("//", "/");

            if (path.Length > 1)
                path = path.TrimEnd('/');

            if (path.EndsWith(".."))
                path = string.Join('/', path.Split('/').SkipLast(2));

            if (string.IsNullOrWhiteSpace(path))
                return "/";

            return path;
        }

        public static bool IsRoot(string path) => path == "/";
    }
}
