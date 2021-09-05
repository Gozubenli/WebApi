using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.DbModels
{
    /// <summary>
    /// Web Crm Kullanıcılarının Rolleri
    /// </summary>
    public class User_Role : BaseModel
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
    }
}
