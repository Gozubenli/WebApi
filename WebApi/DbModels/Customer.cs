using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Utils;

namespace WebApi.DbModels
{
    public class Customer : BaseModel
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
        public RecordBase RecordBase { get; set; }
    }
}
