using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    [Serializable]

    /**
     * <summary>    (Serializable) Class que representa um pacote que contém dados encryptados e 
     *              a assinatura digital dos dados antes da encryptação. </summary>
     *
     * <remarks>    Ricardo Lopes, 04/06/2020. </remarks>
     */

    public class Packet
    {
        public byte[] EncryptedData;
        public byte[] DigitalSignature;

        /**
         * <summary>    Constructor da Class Packet. </summary>
         *
         * <remarks>    Ricardo Lopes, 04/06/2020. </remarks>
         *
         * <param name="encryptedData">     Recebe a informação encriptada. </param>
         * <param name="digitalSignature">  Recebe a assinatura digital da informação. </param>
         */

        public Packet(byte[] encryptedData, byte[] digitalSignature)
        {
            this.EncryptedData = encryptedData;
            this.DigitalSignature = digitalSignature;
        }
    }
}
