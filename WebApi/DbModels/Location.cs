using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.DbModels
{
    /// <summary>
    /// Web Crm Kullanıcılarının Rolleri
    /// </summary>
    public class Location : BaseModel
    {
        [JsonProperty("e")]
        public int EmployeeId { get; set; }
        [JsonProperty("la")]
        public string Latitude { get; set; }
        [JsonProperty("lo")]
        public string Longitude { get; set; }
    }
}
