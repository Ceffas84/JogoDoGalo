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
            this.networkStream = stream;
            this.protocolSI = new ProtocolSI();
        }
        public void SendPacket(ProtocolSICmdType protocolSICmdType, TSCryptography tsCrypto, NetworkStream stream, byte[] data, string privateKey)
        {
            byte[] encryptedData = tsCrypto.SymetricEncryption(data);
            byte[] digitalSignature = tsCrypto.SignData(data, privateKey);
            Packet packet = new Packet(encryptedData, digitalSignature);
            byte[] objByteArray = TSCryptography.ObjectToByteArray(packet);

            SendProtocol(protocolSICmdType, stream, objByteArray);
        }
        public void SendPacket(ProtocolSICmdType protocolSICmdType, TSCryptography tsCrypto, byte[] data, string privateKey)
        {
            byte[] encryptedData = tsCrypto.SymetricEncryption(data);
            byte[] digitalSignature = tsCrypto.SignData(data, privateKey);
            Packet packet = new Packet(encryptedData, digitalSignature);
            byte[] objByteArray = TSCryptography.ObjectToByteArray(packet);

            SendProtocol(protocolSICmdType, objByteArray);
        }
        public void SendProtocol(ProtocolSICmdType protocolSICmdType, int number)
        {
            byte[] packet = protocolSI.Make(protocolSICmdType, number);
            networkStream.Write(packet, 0, packet.Length);
            networkStream.Flush();
        }
        public void SendProtocol(ProtocolSICmdType protocolSICmdType, string str)
        {
            byte[] packet = protocolSI.Make(protocolSICmdType, str);
            networkStream.Write(packet, 0, packet.Length);
            networkStream.Flush();
        }
        public void SendProtocol(ProtocolSICmdType protocolSICmdType, byte[] data)
        {
            byte[] packet = protocolSI.Make(protocolSICmdType, data);
            networkStream.Write(packet, 0, packet.Length);
            networkStream.Flush();
        }
        public void SendProtocol(ProtocolSICmdType protocolSICmdType, NetworkStream stream, int number)
        {
            byte[] packet = protocolSI.Make(protocolSICmdType, number);
            stream.Write(packet, 0, packet.Length);
            stream.Flush();
        }
        public void SendProtocol(ProtocolSICmdType protocolSICmdType, NetworkStream stream, string str)
        {
            byte[] packet = protocolSI.Make(protocolSICmdType, str);
            stream.Write(packet, 0, packet.Length);
            stream.Flush();
        }
        public void SendProtocol(ProtocolSICmdType protocolSICmdType, NetworkStream stream, byte[] data)
        {
            byte[] packet = protocolSI.Make(protocolSICmdType, data);
            stream.Write(packet, 0, packet.Length);
            stream.Flush();
        }
        public void SendProtocol(ProtocolSICmdType protocolSICmdType)
        {
            byte[] packet = protocolSI.Make(protocolSICmdType);
            networkStream.Write(packet, 0, packet.Length);
            networkStream.Flush();
        }
        public void SendProtocol(ProtocolSICmdType protocolSICmdType, NetworkStream stream)
        {
            byte[] packet = protocolSI.Make(protocolSICmdType);
            stream.Write(packet, 0, packet.Length);
            stream.Flush();
        }
        public void SendAck()
        {
            byte[] packet = protocolSI.Make(ProtocolSICmdType.ACK);
            networkStream.Write(packet, 0, packet.Length);
            networkStream.Flush();
        }
        public void SendAck(NetworkStream stream)
        {
            byte[] packet = protocolSI.Make(ProtocolSICmdType.ACK);
            stream.Write(packet, 0, packet.Length);
            stream.Flush();
        }
        public void WaitForAck()
        {
            while (protocolSI.GetCmdType() != ProtocolSICmdType.ACK)
            {
                networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
            }
        }
        public void WaitForAck(NetworkStream stream)
        {
            while (protocolSI.GetCmdType() != ProtocolSICmdType.ACK)
            {
                stream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
            }
        }
    }
}
