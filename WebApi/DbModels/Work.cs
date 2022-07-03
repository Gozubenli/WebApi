using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Utils;

namespace WebApi.DbModels
{
    public class Work : BaseModel
    {
        [JsonProperty("cu")]
        public int CustomerId { get; set; }
        [JsonProperty("ca")]
        public int CategoryId { get; set; }
        [JsonProperty("p")]
        public int ProjectId { get; set; }
        [JsonProperty("ti")]
        public string Title { get; set; }
        [JsonProperty("d")]
        public string Detail { get; set; }
        [JsonProperty("t")]
        public WorkType WorkType { get; set; }
        [JsonProperty("ws")]
        public WorkStatus WorkStatus { get; set; }
        [JsonProperty("es")]
        public EmergencyStatus EmergencyStatus { get; set; }
        [JsonProperty("wt")]
        public WorkTime WorkTime { get; set; }
        [JsonProperty("pd")]
        public DateTime? PlannedDateTime { get; set; }
        [JsonProperty("ph")]
        public int? PlannedHours { get; set; }
        [JsonProperty("wpt")]
        public WorkPeriod? WorkPeriodType { get; set; }
        [JsonProperty("wpn")]
        public int? WorkPeriodNumber { get; set; }
        [JsonProperty("wpe")]
        public DateTime? WorkPeriodEndDate { get; set; }
        [JsonProperty("wpr")]
        public int? WorkPeriodRootId { get; set; }
        [JsonProperty("wprt")]
        public int? WorkPeriodRecurringType { get; set; }
        [JsonProperty("wprd")]
        public string WorkPeriodRecurringDays { get; set; }
        [JsonProperty("in")]
        public string Notes { get; set; }
    }
}
