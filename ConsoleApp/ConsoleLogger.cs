using System;
using Botbin;

namespace ConsoleApp {
    public class ConsoleLogger : ILogger {
        public void Log<T>(T item) {
            Console.WriteLine(item.ToString());
        }
    }
}