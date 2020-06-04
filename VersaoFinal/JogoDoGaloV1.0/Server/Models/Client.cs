using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    [Serializable]

    /**
     * <summary>    (Serializable) Class que representa o client. </summary>
     *
     * <remarks>    Ricardo Lopes, 04/06/2020. </remarks>
     */

    public class Client
    {
        public TcpClient TcpClient { get; set; }
        public int ClientID { get; set; }
        public string PublicKey { get; set; }
        public byte[] SymKey { get; set; }
        public byte[] IV { get; set; }
        public string username { set; get; }
        public int playerID { get; set; }
        public byte[] password { set; get; }
        public byte[] salt { get; set; }
        public byte[] saltedPasswordHash { set; get; }
        public Client()
        {

        }
        public int GetPlayerId()
        {
            return playerID;
        }
    }
}
