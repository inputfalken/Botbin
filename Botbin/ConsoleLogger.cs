using System;
using System.Threading.Tasks;
using Discord;

namespace Botbin {
    class ConsoleLogger : ILogger {
        public void Log(string log) {
            Console.WriteLine(log);
        }
    }
}