using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inb4.SvnClient
{
    public class User
    {
        public string Name { get; set; }
        public string Password { get; set; }

        public User() { }

        public User(string username, string password)
        {
            this.Name = username;
            this.Password = password;
        }
    }
}
