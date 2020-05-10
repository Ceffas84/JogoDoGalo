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
        public string PublicKey { get; set; }
        public byte[] Salt { get; set; }
        public byte[] Username { set; get; }
        public byte[] HashPassword { set; get; }
        public int UserID { get; set; }
        public User()
        {

        }

        public override string ToString()
        {
            return string.Concat(UserID);
        }
    }
}
