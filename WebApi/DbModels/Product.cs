using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.DbModels
{
    public class Product : BaseModel
    {
        public string Name { get; set; }

        public int WarehouseId { get; set; }
    }
}
