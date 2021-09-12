using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.DbModels
{
    public class Log : BaseModel
    {
        public string Level { get; set; }
        public string UserName { get; set; }
        public string Message { get; set; }
    }
}
