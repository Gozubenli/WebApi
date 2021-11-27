using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.DbModels
{
    /// <summary>
    /// Web Crm Kullanıcılarının Rolleri
    /// </summary>
    public class Employee_Role : BaseModel
    {
        public int EmployeeId { get; set; }
        public int RoleId { get; set; }
    }
}
