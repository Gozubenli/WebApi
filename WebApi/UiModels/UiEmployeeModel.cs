using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.DbModels;

namespace WebApi.UiModels
{
    public class UiEmployeeModel
    {
        [JsonProperty("e")]
        public Employee Employee { get; set; }
        [JsonProperty("gl")]
        public List<Group> GroupList { get; set; }
        [JsonProperty("wl")]
        public List<Work> WorkList { get; set; }
        [JsonProperty("rl")]
        public List<Role> RoleList { get; set; }
    }
}
