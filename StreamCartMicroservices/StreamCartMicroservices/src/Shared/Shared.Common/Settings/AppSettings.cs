﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Common.Settings
{
    public class AppSettings
    {
        public ConnectionStrings ConnectionStrings { get; set; }
        public JwtSettings JwtSettings { get; set; }
        public CorsSettings CorsSettings { get; set; }
    }
}
