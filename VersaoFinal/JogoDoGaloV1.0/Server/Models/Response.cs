using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    /**
     * <summary>    (Serializable) a response. 
     *              Class que representa uma resposta do servidor que leva um código de ServerResponse. </summary>
     *
     * <remarks>    Simão Pedro, 04/06/2020. </remarks>
     */

    [Serializable]
    public class Response
    {
        public int ResponseId { get; }
        public Response(int responseId)
        {
            this.ResponseId = responseId;
        }
    }
}
