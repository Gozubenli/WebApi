using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.DbModels
{
    public class Project : BaseModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
