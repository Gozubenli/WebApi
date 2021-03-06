using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Data;

namespace WebApi.Utils
{
    public class Logger : ILogger
    {        
        string rootPath = "";
        public Logger(string path)
        {
            this.rootPath = path;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public async void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            using (StreamWriter streamWriter = new StreamWriter(rootPath+"/api.log", true))
            {
                await streamWriter.WriteLineAsync($"{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")} | {logLevel.ToString()} \t| {formatter(state, exception)}");
                streamWriter.Close();
                await streamWriter.DisposeAsync();
            }            
        }
    }
}
