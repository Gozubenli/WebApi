using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Utils
{
    public sealed class Singleton
    {
        private static readonly Singleton instance = new Singleton();
        static Singleton()
        {
        }
        private Singleton()
        {
        }
        public static Singleton Instance
        {
            get
            {
                return instance;
            }
        }
        public Dictionary<string, string> ApiKey { get; set; } = new Dictionary<string, string>();
    }
}
