using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace JogoDoGalo_Server.Models
{
    public class Player
    {
        public TcpClient TcpClient { get; set; }
        public string PublicKey { get; set; }
        public byte[] Salt { get; set; }
        public byte[] HashUsername { set; get; }
        public byte[] HashPassword { set; get; }
        public Player()
        {

        }
    }
}
