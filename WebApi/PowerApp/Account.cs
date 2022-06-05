using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RentReadyWebApi.Model
{
    public class Account
    {
        public string accountid { get; set; }
        public string accountnumber { get; set; }
        public string name { get; set; }
        public int statecode { get; set; }
        public string stateOrProvince { get; set; }
    }
}
