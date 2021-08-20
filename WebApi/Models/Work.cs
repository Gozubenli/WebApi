using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Utils;

namespace WebApi.Models
{
    public class Work : BaseModel
    {
        public int CustomerId { get; set; }
        public int CategoryId { get; set; }
        public int ProjectId { get; set; }
        public WorkType WorkType { get; set; }
        public WorkStatus WorkStatus { get; set; }
        public EmergencyStatus EmergencyStatus { get; set; }
        public WorkTime WorkTime { get; set; }
    }
}
