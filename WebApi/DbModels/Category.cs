using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.DbModels
{
    public class Category : BaseModel
    {
        public string Name { get; set; }
        public int ParentId { get; set; }
    }
}
