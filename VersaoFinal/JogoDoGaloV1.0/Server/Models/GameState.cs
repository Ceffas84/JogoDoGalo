using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    /**
     * <summary>    Enumeração que representa os estados do jogo. </summary>
     *
     * <remarks>    Ricardo Lopes, 04/06/2020. </remarks>
     */

    public enum GameState
    {
        Standby,
        OnGoing,
        GameOver
    }
}
