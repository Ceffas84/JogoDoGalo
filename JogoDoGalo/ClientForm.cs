using EI.SI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JogoDoGalo
{
    public partial class ClientForm : Form
    {
        private const int PORT = 10000;
        TcpClient tcpClient;
        IPEndPoint ipEndPoint;
        NetworkStream networkStream;
        ProtocolSI protocolSI;
        private string recivedData;
        public ClientForm()
        {
            InitializeComponent();
            ipEndPoint = new IPEndPoint(IPAddress.Loopback, PORT);
            tcpClient = new TcpClient();
            tcpClient.Connect(ipEndPoint);
            networkStream = tcpClient.GetStream();

            //preparação da comunicação utilizando a class ProtocolSI
            protocolSI = new ProtocolSI();

            Thread thread = new Thread(Read);
            thread.Start(tcpClient);
        }



        private void Read(object obj)
        {
            TcpClient tcpClient = (TcpClient)obj;
            StreamReader reader = new StreamReader(tcpClient.GetStream());

            while (true)
            {
                try
                {
                    string message = reader.ReadLine();
                    //Console.WriteLine(message);

                    //O código abaixo serve para utilizar elementos do ClientForm apartir de outra thread.
                    //Explicação: Uma vez
                    Invoke(new Action(() =>
                    {
                        tb_Mensagens.AppendText(message + Environment.NewLine);
                    }));
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    break;
                }
            }
        }

        private void bt_EnviaMensagem_Click(object sender, EventArgs e)
        {
            if (tcpClient.Connected)
            {
                StreamWriter writer = new StreamWriter(tcpClient.GetStream());
                string msg = tb_EscreveMensagem.Text;
                tb_EscreveMensagem.Clear();
                writer.WriteLine(msg);
                writer.Flush();
            }
            //preparar a mensagem para ser enviada
            
            //byte[] packet = protocolSI.Make(ProtocolSICmdType.DATA, msg);

            //enviar a mensagem
        }
    }
}
