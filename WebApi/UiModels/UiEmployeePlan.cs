using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.DbModels;
using WebApi.Utils;

namespace WebApi.UiModels
{
    public class UiEmployeePlan
    {
        [JsonProperty("e")]
        public Employee Employee { get; set; }
        [JsonProperty("pl")]
        public List<Plan> PlanList { get; set; }
    }

    public class Plan
    {
        [JsonProperty("i")]
        public int index;
        [JsonProperty("wt")]
        public WorkTime workTime;
        [JsonProperty("s")]
        public bool status;
        [JsonProperty("w")]
        public int? workId;
        [JsonProperty("c")]
        public int? customerId;
    }
}
