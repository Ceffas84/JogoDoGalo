using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    [Serializable]
    public class Credentials
    {
        public string Username { get; }
        public string Password { get; }
        public Credentials(string username, string password)
        {
            this.Username = username;
            this.Password = password;
        }
    }
}
