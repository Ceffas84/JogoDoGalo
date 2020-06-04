using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
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
