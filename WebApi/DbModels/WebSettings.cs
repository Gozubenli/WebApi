using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.DbModels
{
    public class WebSettings : BaseModel
    {
        [JsonProperty("pc")]
        public string PrimaryColor { get; set; }
        [JsonProperty("sc")]
        public string SecondaryColor { get; set; }
        [JsonProperty("tic")]
        public string TitleColor { get; set; }
        [JsonProperty("mc")]
        public string MenuColor { get; set; }
        [JsonProperty("tec")]
        public string TextColor { get; set; }
        [JsonProperty("dp")]
        public int DefaultPadding { get; set; }

    }
}
