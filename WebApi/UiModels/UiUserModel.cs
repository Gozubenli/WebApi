using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.DbModels;

namespace WebApi.UiModels
{
    public class UiUserModel
    {
        public User User { get; set; }
        public List<Role> RoleList { get; set; }
    }
}
