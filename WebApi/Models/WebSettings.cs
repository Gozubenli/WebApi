using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class WebSettings : BaseModel
    {
        public string MainColor { get; set; }
        public string SecondaryColor { get; set; }
    }
}
