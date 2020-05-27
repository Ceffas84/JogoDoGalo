using EI.SI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    public class TSProtocol
    {
        NetworkStream networkStream;
        ProtocolSI protocolSI;
        public TSProtocol(NetworkStream stream)
        {
            networkStream = stream;
            protocolSI = new ProtocolSI();
        }
        public void SendProtocol(NetworkStream stream, ProtocolSICmdType protocolSICmdType, byte[] data)
        {
            byte[] packet = protocolSI.Make(protocolSICmdType, data);
            stream.Write(packet, 0, packet.Length);
        }
        public void SendProtocol(NetworkStream stream, ProtocolSICmdType protocolSICmdType)
        {
            byte[] packet = protocolSI.Make(protocolSICmdType);
            stream.Write(packet, 0, packet.Length);
        }
        public void SendEncrDigSignProtocol(NetworkStream stream, TSCryptography tsCryptoObj, ProtocolSICmdType protocolSICmdType, User activeUser, byte[] data)
        {
            SendProtocol(stream, ProtocolSICmdType.SYM_CIPHER_DATA, tsCryptoObj.SymetricEncryption(data));
            WaitForAck();

            //tsCryptoObj.SetRsaPrivateKeyCryptography(privateKey);
            byte[] digitalSignature = tsCryptoObj.SignData(data, activeUser.PrivateKey);

            SendProtocol(stream, ProtocolSICmdType.DIGITAL_SIGNATURE, digitalSignature);
            WaitForAck();

            SendProtocol(stream, protocolSICmdType);
            WaitForAck();
        }
        public void SendAcknowledged(ProtocolSI protocolSI, NetworkStream networkStream)
        {
            byte[] ack = protocolSI.Make(ProtocolSICmdType.ACK);
            networkStream.Write(ack, 0, ack.Length);
        }
        public void WaitForAck()
        {
            while (protocolSI.GetCmdType() != ProtocolSICmdType.ACK)
            {
                networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
            }
        }
    }
}
