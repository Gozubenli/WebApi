using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.DbModels
{
    public class Group : BaseModel
    {
        [JsonProperty("n")]
        public string Name { get; set; }
    }
}
