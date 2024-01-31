using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ApiAuthentication.Models
{
    public class UserModel
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Fname { get; set; }
        public string Lname { get; set; }
    }
}