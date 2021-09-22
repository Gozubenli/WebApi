using Newtonsoft.Json;
using WebApi.Utils;

namespace WebApi.DbModels
{
    /// <summary>
    /// Customers created both from Mobile and Web Crm
    /// Signs In from Customer Mobile Application
    /// Username = PhoneNumber
    /// </summary>
    public class Customer : BaseModel
    {
        [JsonProperty("n")]
        public string Name { get; set; }
        [JsonProperty("s")]
        public string Surname { get; set; }
        //[JsonProperty("u")]
        //public string UserName { get; set; }
        //[JsonProperty("p")]
        //public string Password { get; set; }
        [JsonProperty("e")]
        public string Email { get; set; }
        [JsonProperty("t")]
        public string Telephone { get; set; }
        [JsonProperty("r")]
        public RecordBase RecordBase { get; set; }
    }
}
