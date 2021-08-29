using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.DbModels
{
    public class Customer_Project : BaseModel
    {
        public int CustomerId { get; set; }
        public int ProjectId { get; set; }
    }
}
