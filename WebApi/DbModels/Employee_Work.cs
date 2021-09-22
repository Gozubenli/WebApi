using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.DbModels
{
    public class Employee_Work : BaseModel
    {
        [JsonProperty("e")]
        public int EmployeeId { get; set; }
        [JsonProperty("w")]
        public int WorkId { get; set; }
    }
}
