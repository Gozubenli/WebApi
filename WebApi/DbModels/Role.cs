using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Utils;

namespace WebApi.DbModels
{
    /// <summary>
    /// Web Crm Kullanıcılarının Rolleri
    /// </summary>
    public class Role : BaseModel
    {
        [JsonProperty("n")]
        public string Name { get; set; }
        [JsonProperty("d")]
        public string Description { get; set; }
    }
}
