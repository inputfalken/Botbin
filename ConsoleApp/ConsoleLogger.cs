using System;
using Botbin;

namespace ConsoleApp {
    public class ConsoleLogger : ILogger {
        public void Log(string log) {
            Console.WriteLine(log);
        }
    }
}