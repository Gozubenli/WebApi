using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Utils
{
    public class LoggerProvider : ILoggerProvider
    {
        string rootPath = "";
        public LoggerProvider(string rootPath)
        {
            this.rootPath = rootPath;
        }
        public ILogger CreateLogger(string categoryName)
        {
            return new Logger(this.rootPath);
        }

        public void Dispose()
        {
        }
    }
}
