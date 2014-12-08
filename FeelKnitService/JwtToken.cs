﻿using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace FeelKnitService
{
    public class JwtToken
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public IEnumerable<Claim> Claims { get; set; }
        public DateTime Expiry { get; set; }
    }
}