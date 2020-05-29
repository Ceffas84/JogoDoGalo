using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    [Serializable]
    public class Client
    {
        public TcpClient TcpClient { get; set; }
        public int ClientID { get; set; }
        public string PublicKey { get; set; }
        public string ServerPrivateKey { get; set; }
        public byte[] SymKey { get; set; }
        public byte[] IV { get; set; }
        public string username { set; get; }
        public int userId { get; set; }
        public byte[] password { set; get; }
        public byte[] salt { get; set; }
        public byte[] saltedPasswordHash { set; get; }
        public bool isLogged { get; set; }
        public int playerID { get; set; }
        public Client()
        {

        }
        public int GetUserId()
        {
            return userId;
        }
        public override string ToString()
        {
            return string.Concat(ClientID);
        }
    }
}
