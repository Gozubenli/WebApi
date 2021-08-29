using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.DbModels
{
    public class WebSettings : BaseModel
    {
        public string PrimaryColor { get; set; }
        public string SecondaryColor { get; set; }

    }
}
