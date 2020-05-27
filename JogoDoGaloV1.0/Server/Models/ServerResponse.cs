using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    public enum ServerResponse
    {
        REGISTER_ERROR = 00,
        LOGIN_ERROR = 01,
        NOT_LOGGED = 02,
        ALREADY_LOGGED = 03,
        USERNAME_OR_PASSWORD_INVALID_LENGTH = 04,
        INVALID_DIGITAL_SIGNATURE = 05,
        
        REGISTER_SUCCESS = 10,
        LOGIN_SUCCESS = 11,

        GAME_NOT_YET_STARTED = 20,
        GAME_ALREADY_RUNNUNG = 21, 
        INVALID_MOVE = 22,
        NOT_YOUR_TURN = 23
    }
}
