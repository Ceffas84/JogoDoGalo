using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    [Serializable]
    public class GamePlayer
    {
        public int PlayerId { get; }
        public string Username { get; }
        public GamePlayer()
        {
        }
        public GamePlayer(int playerId, string username)
        {
            this.PlayerId = playerId;
            this.Username = username;
        }
    }
}
