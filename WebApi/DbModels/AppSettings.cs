using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.DbModels
{
    public class AppSettings : BaseModel
    {
        [JsonProperty("pc")]
        public string PrimaryColor { get; set; }
        [JsonProperty("sc")]
        public string SecondaryColor { get; set; }
        [JsonProperty("mc1")]
        public string MenuColor1 { get; set; }
        [JsonProperty("mc2")]
        public string MenuColor2 { get; set; }
        [JsonProperty("tc")]
        public string TextColor { get; set; }
        [JsonProperty("dp")]
        public int DefaultPadding { get; set; }
    }
}
