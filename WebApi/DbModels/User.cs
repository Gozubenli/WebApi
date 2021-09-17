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
    public class User : BaseModel
    {
        [JsonProperty("u")]
        public string UserName { get; set; }
        [JsonProperty("pa")]
        public string Password { get; set; }
        [JsonProperty("n")]
        public string Name { get; set; }
        [JsonProperty("s")]
        public string SurName{ get; set; }
        [JsonProperty("ph")]
        public string Phone { get; set; }
        [JsonProperty("e")]
        public string Email { get; set; }
        [JsonProperty("st")]
        public int Status { get; set; }
    }
}
