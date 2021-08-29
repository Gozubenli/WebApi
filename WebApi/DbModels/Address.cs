using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.DbModels
{
    public class Address : BaseModel
    {
        public string Name { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Detail { get; set; }
        public int CustomerId { get; set; }
    }
}
