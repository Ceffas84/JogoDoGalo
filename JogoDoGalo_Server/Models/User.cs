using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace JogoDoGalo_Server.Models
{
    public class User
    {
        public TcpClient TcpClient { get; set; }
        public int UserID { get; set; }
        public string PublicKey { get; set; }
        public byte[] SymKey { get; set; }
        public byte[] IV { get; set; }
        public byte[] username { set; get; }
        public byte[] password { set; get; }
        public byte[] salt { get; set; }
        public byte[] saltedPasswordHash { set; get; }
        public bool isLogged { get; set; }
        public User()
        {

        }

        public override string ToString()
        {
            return string.Concat(UserID);
        }
    }
}
