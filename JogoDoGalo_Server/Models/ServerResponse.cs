using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JogoDoGalo_Server.Models
{
    public enum ServerResponse
    {
        LOGIN_ERROR = 00,
        REGISTER_ERROR = 01,
        NOT_LOGGED = 02,
        LOGIN_SUCCESS = 10,
        REGISTER_SUCCESS = 11,
        NOT_ENOUGH_PLAYERS = 30
    }
}
