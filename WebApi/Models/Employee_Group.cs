using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class Employee_Group : BaseModel
    {
        public int EmployeeId { get; set; }
        public int GroupId { get; set; }
    }
}
