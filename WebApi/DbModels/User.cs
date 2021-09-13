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
        public string UserName { get; set; }
        public string Password { get; set; }
        //[JsonProperty("name")]
        public string Name { get; set; }
       // [JsonProperty("surname")]
        public string SurName{ get; set; }
        //[JsonProperty("phone")]
        public string Phone { get; set; }
       // [JsonProperty("email")]
        public string Email { get; set; }
        public int Status { get; set; }
    }
}
