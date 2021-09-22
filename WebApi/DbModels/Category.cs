using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.DbModels
{
    public class Category : BaseModel
    {
        [JsonProperty("n")]
        public string Name { get; set; }
        [JsonProperty("pi")]
        public int ParentId { get; set; }
    }
}
