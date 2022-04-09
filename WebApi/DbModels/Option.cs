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
    public class Option : BaseModel
    {
        [JsonProperty("t")]
        public string Title { get; set; }

        [JsonProperty("m")]
        public int Max { get; set; }
    }
}
