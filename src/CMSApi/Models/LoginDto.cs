using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CMSApi.Models
{
    public class LoginDto
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}