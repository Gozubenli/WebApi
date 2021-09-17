using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.DbModels
{
    public class Project : BaseModel
    {
        [JsonProperty("n")]
        public string Name { get; set; }
        [JsonProperty("d")]
        public string Description { get; set; }
    }
}
