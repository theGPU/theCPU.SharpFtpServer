﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using theCPU.SharpFtpServer.Commands;

namespace theCPU.SharpFtpServer.Server
{
    public static class CommandRegistar
    {
        public static bool Inited => Registar != null;
        public static ImmutableDictionary<string, ImmutableArray<IFtpCommand>>? Registar { get; private set; }
        public static ImmutableDictionary<string, IFtpCommand>? Handlers { get; private set; }

        private static readonly object InitLocker = new object();

        public static void Init()
        {
            lock (InitLocker)
            {
                var ftpCommands = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(p => typeof(IFtpCommand).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract);
                Registar = ftpCommands.Select(x => (IFtpCommand)Activator.CreateInstance(x)!).GroupBy(x => x.Name).ToImmutableDictionary(x => x.Key, x => x.ToImmutableArray());
                Handlers = Registar.ToImmutableDictionary(x => x.Key, x => x.Value.OrderByDescending(y => y.Priority).First());
            }
        }
    }
}
