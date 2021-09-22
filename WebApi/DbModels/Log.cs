using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.DbModels
{
    public class Log : BaseModel
    {
        [JsonProperty("l")]
        public string Level { get; set; }
        [JsonProperty("u")]
        public string UserName { get; set; }
        [JsonProperty("m")]
        public string Message { get; set; }
    }
}
