using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    [Serializable]
    public class Packet
    {
        public byte[] EncryptedData;
        public byte[] DigitalSignature;
        public Packet(byte[] encryptedData, byte[] digitalSignature)
        {
            this.EncryptedData = encryptedData;
            this.DigitalSignature = digitalSignature;
        }
        public static byte[] CreateEncrypDigSignPacket(TSCryptography tsCrypto, byte[] data, string privateKey)
        {
            byte[] encryptedData = tsCrypto.SymetricEncryption(data);
            byte[] digitalSignature = tsCrypto.SignData(data, Program.SERVERPRIVATEKEY);
            Packet packet = new Packet(encryptedData, digitalSignature);
            byte[] objectArrayBytes = TSCryptography.ObjectToByteArray(packet);
            return objectArrayBytes;
        }
    }
}
