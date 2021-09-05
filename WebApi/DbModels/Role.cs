using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Utils;

namespace WebApi.DbModels
{
    /// <summary>
    /// Web Crm Kullanıcılarının Rolleri
    /// </summary>
    public class Role : BaseModel
    {
        public string Name { get; set; }
    }
}
