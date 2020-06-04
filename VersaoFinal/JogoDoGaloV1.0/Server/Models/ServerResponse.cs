using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    /**
     * <summary>    Values that represent server responses. 
     *              Enumeração com códigos que representam mensagens de erro ou sucesso a enviar aos clientes. </summary>
     *
     * <remarks>    Simão Pedro, 04/06/2020. </remarks>
     */
    public enum ServerResponse
    {
        REGISTER_ERROR = 00,
        LOGIN_ERROR = 01,
        NOT_LOGGED_TO_START_GAME = 02,
        ALREADY_LOGGED = 03,
        USERNAME_OR_PASSWORD_INVALID_LENGTH = 04,
        INVALID_DIGITAL_SIGNATURE = 05,
        LOGGED_IN_ANOTHER_CLIENT = 06,
        NOT_LOGGED_TO_CHAT = 07,
        LOGOUT_ERROR = 08,

        REGISTER_SUCCESS = 10,
        LOGIN_SUCCESS = 11,
        LOGOUT_SUCCESS = 12,

        NOT_ENOUGH_PLAYERS = 20,
        GAME_ALREADY_RUNNING = 21, 
        INVALID_MOVE = 22,
        NOT_YOUR_TURN = 23
    }
}
