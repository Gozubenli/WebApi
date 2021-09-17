using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.DbModels
{
    public class Address : BaseModel
    {
        [JsonProperty("n")]
        public string Name { get; set; }
        [JsonProperty("ci")]
        public string City { get; set; }
        [JsonProperty("co")]
        public string Country { get; set; }
        [JsonProperty("d")]
        public string Detail { get; set; }
        [JsonProperty("lo")]
        public double Longitude { get; set; }
        [JsonProperty("la")]
        public double Latidude { get; set; }
        [JsonProperty("cu")]
        public int CustomerId { get; set; }
    }
}
