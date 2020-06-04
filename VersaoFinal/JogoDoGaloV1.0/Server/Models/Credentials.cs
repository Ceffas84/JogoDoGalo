using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    [Serializable]

    /**
     * <summary>    (Serializable) Class que recebe um envio de credenciais de
     *              login ou de registo. </summary>
     *
     * <remarks>    Ricardo Lopes, 04/06/2020. </remarks>
     */

    public class Credentials
    {
        public string Username { get; }
        public string Password { get; }
        public Credentials(string username, string password)
        {
            this.Username = username;
            this.Password = password;
        }
    }
}
