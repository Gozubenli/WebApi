using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.DbModels
{
    public class Category : BaseModel
    {
        [JsonProperty("pi")]
        public int ParentId { get; set; }
        [JsonProperty("n")]
        public string Name { get; set; }        
        [JsonProperty("d")]
        public string Description { get; set; }
        [JsonProperty("i")]
        public string Image { get; set; }
    }
}
