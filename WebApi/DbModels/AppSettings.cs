﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.DbModels
{
    public class AppSettings : BaseModel
    {
        public string MainColor { get; set; }
        public string SecondaryColor { get; set; }
    }
}