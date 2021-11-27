using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.DbModels
{
    /// <summary>
    /// Web Crm Staff
    /// Signs In from Employee Mobile Application
    /// Username = PhoneNumber
    /// </summary>
    public class Employee : BaseModel
    {
        [JsonProperty("n")]
        public string Name { get; set; }
        [JsonProperty("s")]
        public string Surname { get; set; }
        [JsonProperty("pa")]
        public string Password { get; set; }
        [JsonProperty("e")]
        public string Email { get; set; }
        [JsonProperty("p")]
        public string Telephone { get; set; }
        [JsonProperty("t")]
        public int TitleId { get; set; }
        [JsonProperty("i")]
        public string ImageName { get; set; }
    }
}