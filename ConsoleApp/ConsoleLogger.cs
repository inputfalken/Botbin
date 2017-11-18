using System;
using Botbin;
using Botbin.Services.Interfaces;

namespace ConsoleApp {
    public class ConsoleLogger : ILogger {
        public void Log<T>(T item) {
            Console.WriteLine(item.ToString());
        }
    }
}