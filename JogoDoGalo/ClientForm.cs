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
using System.Security.Cryptography;
using JogoDoGalo_Server.Models;
using JogoDoGalo.Models;

namespace JogoDoGalo
{
    public partial class ClientForm : Form
    {
        private static int dimensaoTabuleiro = 3;
        private const int PORT = 10000;
        private TcpClient tcpClient;
        private IPEndPoint ipEndPoint;
        private NetworkStream networkStream;
        private ProtocolSI protocolSI;
        private TSCryptography tsCrypto;
        private List<Button> gameBoard;
        private Thread thread;

        private string publicKey;
        private string privateKey;

        private byte[] decryptedSymKey;
        private byte[] decryptedIV;

        private string username;

        byte[] packet;
        byte[] encryptedData;
        byte[] decryptedData;
        
        private bool Acknoledged = false;

        private bool isYourTurn;
        public ClientForm()
        {
            InitializeComponent();

            ipEndPoint = new IPEndPoint(IPAddress.Parse(tb_Servidor.Text), PORT);
            tcpClient = new TcpClient();

            tcpClient.Connect(ipEndPoint);
            networkStream = tcpClient.GetStream();

            protocolSI = new ProtocolSI();
            tsCrypto = new TSCryptography();

            thread = new Thread(ServerLisneter);
            thread.Name = "ServerLisneter";
            thread.Start(tcpClient);

            publicKey = tsCrypto.GetPublicKey();
            privateKey = tsCrypto.GetPrivateKey();
            //Console.WriteLine("Chave publica: {0}", publicKey);

            byte[] packet = protocolSI.Make(ProtocolSICmdType.PUBLIC_KEY, publicKey);
            networkStream.Write(packet, 0, packet.Length);

            //ServerHandler serverHandler = new ServerHandler(tcpClient);
            //serverHandler.Handle();
        }
        private void ServerLisneter(object obj)
        {
            TcpClient tcpClient = (TcpClient)obj;
            NetworkStream networkStream = tcpClient.GetStream();
            int result;

            while (protocolSI.GetCmdType() != ProtocolSICmdType.EOT)
            {
                try
                {
                    networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                    switch (protocolSI.GetCmdType())
                    {
                        case ProtocolSICmdType.SECRET_KEY:
                            //Recebe a Chave Simétrica do ProtocolSI
                            byte[] encryptedSymKey = protocolSI.GetData();

                            //Desencripta e atribui a key ao objecto aes
                            decryptedSymKey = tsCrypto.RsaDecryption(encryptedSymKey, tsCrypto.GetPrivateKey());
                            tsCrypto.SetAesSymKey(decryptedSymKey);

                            SendAcknowledged(protocolSI, networkStream);
                            break;
                        case ProtocolSICmdType.IV:
                            //Recebe o Vetor de Inicialização do ProtocolSI
                            byte[] ivEncrypted = protocolSI.GetData();

                            //Desencripta e atribui o iv ao objecto aes
                            decryptedIV = tsCrypto.RsaDecryption(ivEncrypted, tsCrypto.GetPrivateKey());
                            tsCrypto.SetAesIV(decryptedIV);

                            SendAcknowledged(protocolSI, networkStream);
                            break;
                        case ProtocolSICmdType.DATA:
                            //recebe a mensagem
                            byte[] encriptedMsg = protocolSI.GetData();

                            //Desencripta a mensagem recebida
                            byte[] decryptedMsg = tsCrypto.SymetricDecryption(encriptedMsg);

                            //O código abaixo serve para utilizar elementos do ClientForm apartir de outra thread.
                            //Explicação: Uma vez que os elementos do form só podem ser chamados pela thread que executa o form.
                            string message = Encoding.UTF8.GetString(decryptedMsg, 0, decryptedMsg.Length);
                            Invoke(new Action(() =>
                            {
                                rtbMensagens.AppendText(message); rtbMensagens.AppendText(Environment.NewLine);
                            }));

                            SendAcknowledged(protocolSI, networkStream);
                            break;
                            //Receção de resposta so servidor ao pedido de Login

                        case ProtocolSICmdType.USER_OPTION_1:
                            result = protocolSI.GetIntFromData();
                            Invoke(new Action(() => { ReturnError(result); }));
                            break;

                            //Receção de resposta do servidor ao pedido de registo
                        case ProtocolSICmdType.USER_OPTION_2:
                            result = protocolSI.GetIntFromData();
                            Invoke(new Action(() => { VerifyRegister(result); }));
                            break;

                            //Receção de Brodcast do servidor com um novo user logged in
                        case ProtocolSICmdType.USER_OPTION_3:
                            byte[] encryptedData = protocolSI.GetData();
                            byte[] decryptedData = tsCrypto.SymetricDecryption(encryptedData);

                            Invoke(new Action(() => { PlayersBoardUpdate(decryptedData); }));
                            break;
                        case ProtocolSICmdType.USER_OPTION_4:
                            Invoke(new Action(() => { MessageBox.Show("Comunicação corrompida!"); }));
                            break;

                        case ProtocolSICmdType.EOT:
                            SendAcknowledged(protocolSI, networkStream);
                            Acknoledged = true;
                            break;
                        case ProtocolSICmdType.USER_OPTION_9:
                            encryptedData = protocolSI.GetData();
                            byte[] gameStart = tsCrypto.SymetricDecryption(encryptedData);
                            Invoke(new Action(() => { StartGame(gameStart); }));
                            break;
                        case ProtocolSICmdType.ACK:
                            Acknoledged = true;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK);
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.HResult);
                    break;
                }
            }
            //SendAcknowledged(protocolSI, networkStream);
            networkStream.Close();
            tcpClient.Close();
        }
        public void SendEncryptedProtocol(ProtocolSICmdType protocolSICmdType, byte[] data)
        {
            byte[] encrypteData = tsCrypto.SymetricEncryption(data);
            byte[] packet = protocolSI.Make(protocolSICmdType, encrypteData);
            networkStream.Write(packet, 0, packet.Length);
            networkStream.Flush();
        }
        public void SendAcknowledged(ProtocolSI propotcolSi, NetworkStream networkStream)
        {
            byte[] ack = protocolSI.Make(ProtocolSICmdType.ACK);
            networkStream.Write(ack, 0, ack.Length);
            networkStream.Flush();
        }
        //------------------------------------------------------------------------------------------------------------------------------
        //Código para teste do jogo no tabuleiro

        private void bt_EnviaMensagem_Click(object sender, EventArgs e)
        {
            if (tcpClient.Connected)
            {
                networkStream = tcpClient.GetStream();
                string msg = tb_EscreveMensagem.Text;
                tb_EscreveMensagem.Clear();

                //Enviamos um pacote com uma mensagem e a assinatura digital
                Send_Data_DigitalSignature(Encoding.UTF8.GetBytes(msg));

                //Enviamos a infomação ao servidor que o pacote recebido foi uma mensagem do chat
                byte[] packet = protocolSI.Make(ProtocolSICmdType.DATA);
                networkStream.Write(packet, 0, packet.Length);
                // Aguarda confirmação da receção do username
                while (!Acknoledged) { }
                Acknoledged = false;

                networkStream.Flush();
            }
        }
        private void btnAddClient_Click(object sender, EventArgs e)
        {
            ClientForm newcliente = new ClientForm();
            newcliente.Show();
        }
        public void IniciarJogo()
        {
            DesenharTabuleiro(100, 50, 400, 3);
        }
        private void DesenharTabuleiro(int offset_x, int offset_y, int size, int numButtons)
        {
            gameBoard = new List<Button>();
            int padding = Convert.ToInt32(Math.Round(size * 0.02, MidpointRounding.AwayFromZero));
            int buttonSize = (size - (padding * (numButtons + 1))) / numButtons;
            for (int I = 0; I < dimensaoTabuleiro; I++)
            {
                for (int J = 0; J < dimensaoTabuleiro; J++)
                {
                    Button newButton = new Button();
                    newButton.Text = "";
                    newButton.Name = I + "_" + J;
                    newButton.Location = new Point(offset_x + padding + (I * (padding + buttonSize)), offset_y + padding + (J * (padding + buttonSize)));
                    newButton.Width = buttonSize;
                    newButton.Height = buttonSize;
                    newButton.BackColor = System.Drawing.Color.LightGray;
                    newButton.Click += BoardClick;
                    gameBoard.Add(newButton);
                    this.Controls.Add(newButton);
                }
            }
        }
        private void BoardClick(object sender, EventArgs e)
        {
            Button clickedButton = sender as Button;
            List<int> coord = GetClickedButton(clickedButton);
        }
        private Button GetButton(int x, int y)
        {

            foreach(Button bytton in gameBoard)
            {

                if()
            }
            return null;
        }
        private List<int> GetClickedButton(Button clickedButton)
        {
            string[] a = clickedButton.Name.Split('_');
            List<int> coord = new List<int>();
            coord.Add(int.Parse(a[1]));
            coord.Add(int.Parse(a[0]));
            return coord;
        }
        private void ClientForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseClient();
        }
        private void CloseClient()
        {
            //Preparar o envio da mensagem para desligar as ligações
            byte[] eot = protocolSI.Make(ProtocolSICmdType.EOT);
            networkStream.Write(eot, 0, eot.Length);
                        
            //Aguarda confirmação da receção do username
            while (!Acknoledged) { }
            Acknoledged = false;
            Thread.Sleep(2000);

            networkStream.Close();
            tcpClient.Close();
        }
        private void btnLogin_Click(object sender, EventArgs e)
        {
            Send_Username_Password();
            byte[] packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_3);
            networkStream.Write(packet, 0, packet.Length);

            //Aguarda confirmação da receção do pedido de login
            while (!Acknoledged) { }
            Acknoledged = false;
        }
        private void btnSignup_Click(object sender, EventArgs e)
        {
            Send_Username_Password();
            byte[] packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_4);
            networkStream.Write(packet, 0, packet.Length);

            //Aguarda confirmação da receção do pedido de signup
            while (!Acknoledged) { }
            Acknoledged = false;
        }
        private void ReturnError(int serverResponse)
        {
            
            switch (serverResponse)
            {
                case 0:
                    tb_Jogador.Clear();
                    tb_Password.Clear();
                    MessageBox.Show("Crendeciais incorretas");
                    break;
                case 1:
                    tb_Password.Text = "UTILIZADOR LIGADO";
                    tb_Password.BackColor = Color.LightGreen;
                    username = tb_Jogador.Text;
                    break;
                case 2:
                    MessageBox.Show("Não está autenticado, faça login!", "Erro de autenticação", MessageBoxButtons.OK);
                    break;
                case 9:
                    MessageBox.Show("Não existem jogadores suficientes loggados", "Erro ao iniciar jogo", MessageBoxButtons.OK);
                    break;
            }
        }
        private void VerifyRegister(int serverResponse)
        {
            switch (serverResponse)
            {
                case 0:
                    tb_Jogador.Clear();
                    tb_Password.Clear();
                    MessageBox.Show("Verifique o username e password introduzidos!", "Erro ao registar utilizador");
                    break;
                case 1:
                    tb_Password.Clear();
                    MessageBox.Show("Faça login!", "Utilizador Registado com sucesso");
                    break;
            }
        }
        private void PlayersBoardUpdate(byte[] data)
        {
            lbPlayersBoard.Items.Clear();

            List<string> usersLogged = (List<string>)TSCryptography.ByteArrayToObject(data);
            foreach(string user in usersLogged)
            {
                lbPlayersBoard.Items.Add(user);
            }
            
        }
        private void Send_Username_Password()
        {
            byte[] packet;

            //Enviamos encriptado os dados do username
            byte[] username = Encoding.UTF8.GetBytes(tb_Jogador.Text);
            byte[] encryptedData = tsCrypto.SymetricEncryption(username);
            packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_1, encryptedData);
            networkStream.Write(packet, 0, packet.Length);

            //Aguarda confirmação da receção do username
            while (!Acknoledged) { } 
            Acknoledged = false;
            //Thread.Sleep(1000);

            //Enviamos encriptado os dados da password
            byte[] password = Encoding.UTF8.GetBytes(tb_Password.Text);
            encryptedData = tsCrypto.SymetricEncryption(password);
            packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_2, encryptedData);
            networkStream.Write(packet, 0, packet.Length);

            //Aguarda confirmação da receção do username
            while (!Acknoledged) { }
            Acknoledged = false;

        }
        private void Send_Data_DigitalSignature(byte[] data)
        {
            byte[] packet;
            Console.WriteLine("Dados enviados: {0}", Encoding.UTF8.GetString(data));

            //Enviamos os dados encriptados
            byte[] encryptedData = tsCrypto.SymetricEncryption(data);
            Console.WriteLine("Dados encriptados: {0}", Convert.ToBase64String(encryptedData));

            packet = protocolSI.Make(ProtocolSICmdType.SYM_CIPHER_DATA, encryptedData);
            networkStream.Write(packet, 0, packet.Length);

            //Aguarda confirmação da receção dos dados
            while (!Acknoledged) { }
            Acknoledged = false;

            //Enviamos a assinatura digital
            byte[] digitalSignature = tsCrypto.SignData(tsCrypto.SymetricDecryption(encryptedData), tsCrypto.GetPrivateKey());
            Console.WriteLine("Assinatura digital: {0}",Convert.ToBase64String(digitalSignature));

            packet = protocolSI.Make(ProtocolSICmdType.DIGITAL_SIGNATURE, digitalSignature);
            networkStream.Write(packet, 0, packet.Length);

            //Aguarda confirmação da receção do username
            while (!Acknoledged) { }
            Acknoledged = false;
        }

        private void sairToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnGameStart_Click(object sender, EventArgs e)
        {
            encryptedData = tsCrypto.SymetricEncryption(Encoding.UTF8.GetBytes(dimensaoTabuleiro.ToString()));
            packet = protocolSI.Make(ProtocolSICmdType.USER_OPTION_9, encryptedData);
            networkStream.Write(packet, 0, packet.Length);
        }
        private void StartGame(byte[] data)
        {
            List<object> gameStart = (List<object>)TSCryptography.ByteArrayToObject(data);
            GamePlayer gamePlayer = (GamePlayer)gameStart[1];

            DesenharTabuleiro(100, 50, 400,(int) gameStart[0]);

            if(gamePlayer.GetPlayerUsername() == tb_Jogador.Text)
            {
                isYourTurn = true;
                gameDisplay.Text = "Jogue!";
                gameDisplay.BackColor = Color.Green;
                EnableGameBoard(true);
            }
            else
            {
                isYourTurn = true;
                gameDisplay.Text = string.Format("Espere que {0} faça a sua jogada!", gamePlayer.GetPlayerUsername());
                gameDisplay.BackColor = Color.Gray;
                EnableGameBoard(false);      
            }
        }
        private void EnableGameBoard(bool state)
        {
            foreach (Button button in gameBoard)
            {
                button.Enabled = state;
            }
        }
        private void UpdateGameBoard(byte[] data)
        {

        }

    }
}
