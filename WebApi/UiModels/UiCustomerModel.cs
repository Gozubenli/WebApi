using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.DbModels;

namespace WebApi.UiModels
{
    public class UiCustomerModel
    {
        public Customer Customer { get; set; }
        public List<Project> ProjectList { get; set; }
        public List<Address> AddressList { get; set; }
    }
}
