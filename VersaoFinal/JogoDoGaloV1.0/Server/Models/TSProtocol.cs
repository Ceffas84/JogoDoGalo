using EI.SI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    /**
     * <summary>    Class que reune várias funções para auxiliar as comunicações de 
     *              ProtocolSI, com diversas assinaturas, usando ou não a stream da class. </summary>
     *
     * <remarks>    Ricardo Lopes, 04/06/2020. </remarks>
     */

    public class TSProtocol
    {
        NetworkStream networkStream;
        ProtocolSI protocolSI;

        /**
         * <summary>    Construtor da Class TSProtocol, a qual inicializa um objeto de ProtocolSi 
         *              e um objeto de NetworkStream </summary>
         *
         * <remarks>    Ricardo Lopes, 04/06/2020. </remarks>
         *
         * <param name="stream">    The stream. </param>
         */

        public TSProtocol(NetworkStream stream)
        {
            this.networkStream = stream;
            this.protocolSI = new ProtocolSI();
        }

        /**
         * <summary>    Função que envia um Packet (informação encriptada e assinatura digital)
         *              através de um stream especifico, e com um objeto criptográfico especifico</summary>
         *
         * <remarks>    Ricardo Lopes, 04/06/2020. </remarks>
         *
         * <param name="protocolSICmdType"> Recebe ProtocolSICmdType a enviar. </param>
         * <param name="tsCrypto">          Recebe um objeto criptográfico especifico. </param>
         * <param name="stream">            Recebe a stream a utilizar. </param>
         * <param name="data">              Receba a informação a enviarThe data. </param>
         * <param name="privateKey">        Recebe a chave privada para assinar a informação. </param>
         */

        public void SendPacket(ProtocolSICmdType protocolSICmdType, TSCryptography tsCrypto, NetworkStream stream, byte[] data, string privateKey)
        {
            byte[] encryptedData = tsCrypto.SymetricEncryption(data);
            byte[] digitalSignature = tsCrypto.SignData(data, privateKey);
            Packet packet = new Packet(encryptedData, digitalSignature);
            byte[] objByteArray = TSCryptography.ObjectToByteArray(packet);

            SendProtocol(protocolSICmdType, stream, objByteArray);
        }

        /**
         * <summary>    Função que envia um Packet (informação encriptada e assinatura digital)
         *              através do stream da Class, com um objeto criptográfico especifico. </summary>
         *
         * <remarks>    Ricardo Lopes, 04/06/2020. </remarks>
         *
         * <param name="protocolSICmdType"> Recebe ProtocolSICmdType a enviar. </param>
         * <param name="tsCrypto">          Recebe um objeto criptográfico especifico. </param>
         * <param name="data">              Receba a informação a enviarThe data. </param>
         * <param name="privateKey">        Recebe a chave privada para assinar a informação. </param>
         */

        public void SendPacket(ProtocolSICmdType protocolSICmdType, TSCryptography tsCrypto, byte[] data, string privateKey)
        {
            byte[] encryptedData = tsCrypto.SymetricEncryption(data);
            byte[] digitalSignature = tsCrypto.SignData(data, privateKey);
            Packet packet = new Packet(encryptedData, digitalSignature);
            byte[] objByteArray = TSCryptography.ObjectToByteArray(packet);

            SendProtocol(protocolSICmdType, objByteArray);
        }

        /**
         * <summary>    Função que envia um Protocol com um inteiro através da stream da Class  </summary>
         *
         * <remarks>    Ricardo Lopes, 04/06/2020. </remarks>
         *
         * <param name="protocolSICmdType"> Recebe o tipo de ProtocolSI a enviar. </param>
         * <param name="number">            Recebe o inteiro a enviar. </param>
         */

        public void SendProtocol(ProtocolSICmdType protocolSICmdType, int number)
        {
            byte[] packet = protocolSI.Make(protocolSICmdType, number);
            networkStream.Write(packet, 0, packet.Length);
            networkStream.Flush();
        }

        /**
         * <summary>    Função que envia um Protocol com uma string através da stream da Class  </summary>
         *
         * <remarks>    Ricardo Lopes, 04/06/2020. </remarks>
         *
         * <param name="protocolSICmdType"> Recebe o tipo de ProtocolSI a enviar. </param>
         * <param name="str">               Recebe a string a enviar. </param>
         */

        public void SendProtocol(ProtocolSICmdType protocolSICmdType, string str)
        {
            byte[] packet = protocolSI.Make(protocolSICmdType, str);
            networkStream.Write(packet, 0, packet.Length);
            networkStream.Flush();
        }

        /**
         * <summary>    Função que envia um Protocol com um array de bytes através da stream da Class </summary>
         *
         * <remarks>    Ricardo Lopes, 04/06/2020. </remarks>
         *
         * <param name="protocolSICmdType"> Recebe o tipo de ProtocolSI a enviar. </param>
         * <param name="data">              Recebe o array de bytes a enviar. </param>
         */

        public void SendProtocol(ProtocolSICmdType protocolSICmdType, byte[] data)
        {
            byte[] packet = protocolSI.Make(protocolSICmdType, data);
            networkStream.Write(packet, 0, packet.Length);
            networkStream.Flush();
        }

        /**
         * <summary>    Função que envia um Protocol com um array de bytes através de uma stream especifica. </summary>
         *
         * <remarks>    Ricardo Lopes, 04/06/2020. </remarks>
         *
         * <param name="protocolSICmdType"> Recebe o tipo de ProtocolSI a enviar. </param>
         * <param name="stream">            Recebe a stream a utilizar. </param>
         * <param name="data">              Recebe o array de bytes a enviar. </param>
         */

        public void SendProtocol(ProtocolSICmdType protocolSICmdType, NetworkStream stream, byte[] data)
        {
            byte[] packet = protocolSI.Make(protocolSICmdType, data);
            stream.Write(packet, 0, packet.Length);
            stream.Flush();
        }

        /**
         * <summary>    Função que envia um Protocol através da stream da Class. </summary>
         *
         * <remarks>    Ricardo Lopes, 04/06/2020. </remarks>
         *
         * <param name="protocolSICmdType"> Recebe o tipo de ProtocolSI a enviar. </param>
         */

        public void SendProtocol(ProtocolSICmdType protocolSICmdType)
        {
            byte[] packet = protocolSI.Make(protocolSICmdType);
            networkStream.Write(packet, 0, packet.Length);
            networkStream.Flush();
        }

        /**
         * <summary>    Função que envia um Protocol através de uma stream especifica. </summary>
         *
         * <remarks>    Ricardo Lopes, 04/06/2020. </remarks>
         *
         * <param name="protocolSICmdType"> Recebe o tipo de ProtocolSI a enviar. </param>
         * <param name="stream">            Recebe a stream a utilizar. </param>
         */

        public void SendProtocol(ProtocolSICmdType protocolSICmdType, NetworkStream stream)
        {
            byte[] packet = protocolSI.Make(protocolSICmdType);
            stream.Write(packet, 0, packet.Length);
            stream.Flush();
        }

        /**
         * <summary>    Função que envia um protocol de acknoledged através
         *              da stream da Class. </summary>
         *
         * <remarks>    Ricardo Lopes, 04/06/2020. </remarks>
         */

        public void SendAck()
        {
            byte[] packet = protocolSI.Make(ProtocolSICmdType.ACK);
            networkStream.Write(packet, 0, packet.Length);
            networkStream.Flush();
        }

        /**
         * <summary>    Função que envia um protocol de acknoledged através
         *              de uma stream especifica. </summary>
         *              
         * <remarks>    Ricardo Lopes, 04/06/2020. </remarks>
         *
         * <param name="stream">            Recebe a stream a utilizar. </param>
         */

        public void SendAck(NetworkStream stream)
        {
            byte[] packet = protocolSI.Make(ProtocolSICmdType.ACK);
            stream.Write(packet, 0, packet.Length);
            stream.Flush();
        }

        /**
         * <summary>    Função que espera por um protocol acknoledged através
         *              da stream da class. </summary>
         *
         * <remarks>    Ricardo Lopes, 04/06/2020. </remarks>
         */

        public void WaitForAck()
        {
            while (protocolSI.GetCmdType() != ProtocolSICmdType.ACK)
            {
                networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
            }
        }

        /**
         * <summary>    Função que espera por um protocol acknoledged através
         *              de uma stream especifica. </summary>
         * 
         * <remarks>    Ricardo Lopes, 04/06/2020. </remarks>
         *
         * <param name="stream">            Recebe a stream a utilizar. </param>
         */

        public void WaitForAck(NetworkStream stream)
        {
            while (protocolSI.GetCmdType() != ProtocolSICmdType.ACK)
            {
                stream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
            }
        }
    }
}
