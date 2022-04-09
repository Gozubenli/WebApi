using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.DbModels
{
    public class Category_Option : BaseModel
    {
        public int CategoryId { get; set; }
        public int OptionId { get; set; }
    }
}
