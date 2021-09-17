using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.DbModels;

namespace WebApi.UiModels
{
    public class UiCustomerModel
    {
        [JsonProperty("cu")]
        public Customer Customer { get; set; }
        [JsonProperty("pl")]
        public List<Project> ProjectList { get; set; }
        [JsonProperty("al")]
        public List<Address> AddressList { get; set; }
    }
}
