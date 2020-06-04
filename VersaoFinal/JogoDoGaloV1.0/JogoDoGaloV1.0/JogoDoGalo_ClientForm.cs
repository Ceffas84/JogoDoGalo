using EI.SI;
using Server.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JogoDoGaloV1._0
{
    public partial class JogoDoGalo_ClientForm : Form
    {
        private TcpClient tcpClient;
        private IPEndPoint ipEndPoint;
        private NetworkStream stream;

        private TSCryptography tsCryptography;
        private ProtocolSI protocolSI;
        private TSProtocol tsProtocol;

        private string publicKey;       
        private string privateKey;      
        private string serverPublicKey; 

        private delegate void SafeCallDelegate(string text);
        
        private List<Button> gameBoard;
        private int playerId;

        private Thread thread;
        public JogoDoGalo_ClientForm()
        {
            InitializeComponent();

            ipEndPoint = new IPEndPoint(IPAddress.Parse(tbServidor.Text), 10000);
            tcpClient = new TcpClient();

            tcpClient.Connect(ipEndPoint);
            stream = tcpClient.GetStream();

            tsCryptography = new TSCryptography();
            tsProtocol = new TSProtocol(stream);
            protocolSI = new ProtocolSI();
            

            thread = new Thread(ServerListener);
            thread.Name = "ServerLisneter";
            thread.Start(tcpClient);

            publicKey = tsCryptography.GetPublicKey();
            privateKey = tsCryptography.GetPrivateKey();

            byte[] packet = protocolSI.Make(ProtocolSICmdType.PUBLIC_KEY, publicKey);
            stream.Write(packet, 0, packet.Length);

            dupSymbol.Items.Add("X");
            dupSymbol.Items.Add("$");
            dupSymbol.Items.Add(":)");
        }

        /**
         * <summary>    Server listener.
         *              Método que corre na thread e recebe as respostas do servidor </summary>
         *
         * <remarks>    Simão Pedro, 04/06/2020. </remarks>
         *
         * <param name="obj">   The object. </param>
         */
        private void ServerListener(object obj)
        {
            TcpClient tcpClient = (TcpClient)obj;
            NetworkStream stream = tcpClient.GetStream();
            SafeCallDelegate safeCallDelegate;
            Packet packet;
            byte[] packetByteArray;
            byte[] digitalSignature;
            byte[] decryptedData;

            //
            //  **** TABELA DE UTILIZAÇÃO DE COMANDOS DO PROTOCOLSI ****
            //
            //  USER_OPTION_1         => Receção do Start Game
            //  USER_OPTION_2         => Receção do Próximo jogador
            //  USER_OPTION_3         => Receção de Jogadas
            //  USER_OPTION_4         => Receção de Game Over
            //  USER_OPTION_5         => Receção de Jogadores Logados
            //  USER_OPTION_8         => Receção de mensagens no chat
            //  USER_OPTION_9         => Receção de mensagens de Erro ou Sucesso
            //  
            //  PUBLIC_KEY            => Receção da PublicKey do servidor
            //  SECRET_KEY            => Receção da SecretKey
            //  IV                    => Receção do vetor de inicialização
            // 
            
            while (protocolSI.GetCmdType() != ProtocolSICmdType.EOT)
            {
                try
                {
                    stream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);        
                    switch (protocolSI.GetCmdType())
                    {
                        case ProtocolSICmdType.USER_OPTION_1:
                            //Receção do Start Game
                            tsProtocol.SendAck();

                            packetByteArray = protocolSI.GetData();
                            packet = (Packet)TSCryptography.ByteArrayToObject(packetByteArray);
                            decryptedData = tsCryptography.SymetricDecryption(packet.EncryptedData);
                            digitalSignature = packet.DigitalSignature;
                            
                            if (tsCryptography.VerifyData(decryptedData, digitalSignature, serverPublicKey))
                            {
                                StartGame start = (StartGame)TSCryptography.ByteArrayToObject(decryptedData);
                                playerId = start.PlayerId;

                                Invoke(new Action(() => { DrawBoard(80, 79, 400, start.BoardDimension); }));
                            }
                            break;
                           
                        case ProtocolSICmdType.USER_OPTION_2:
                            //Receção do Próximo jogador
                            tsProtocol.SendAck();

                            packetByteArray = protocolSI.GetData();
                            packet = (Packet)TSCryptography.ByteArrayToObject(packetByteArray);

                            decryptedData = tsCryptography.SymetricDecryption(packet.EncryptedData);
                            digitalSignature = packet.DigitalSignature;

                            if (tsCryptography.VerifyData(decryptedData, digitalSignature, serverPublicKey))
                            {
                                GamePlayer gamePlayer = (GamePlayer)TSCryptography.ByteArrayToObject(decryptedData);

                                Invoke(new Action(() => { ShowActivePlayer(gamePlayer.PlayerId); }));

                            }
                            break;

                        case ProtocolSICmdType.USER_OPTION_3:
                            //Receção de Jogadas
                            tsProtocol.SendAck();

                            packetByteArray = protocolSI.GetData();
                            packet = (Packet)TSCryptography.ByteArrayToObject(packetByteArray);
                            decryptedData = tsCryptography.SymetricDecryption(packet.EncryptedData);
                            digitalSignature = packet.DigitalSignature;

                            if (tsCryptography.VerifyData(decryptedData, digitalSignature, serverPublicKey))
                            {
                                List<GamePlay> listOfPlays = (List<GamePlay>)TSCryptography.ByteArrayToObject(decryptedData);
                                Invoke(new Action(() => { DrawPlays(listOfPlays); }));
                            }
                            break;

                        case ProtocolSICmdType.USER_OPTION_4:
                            //Receção de Game Over
                            tsProtocol.SendAck();

                            packetByteArray = protocolSI.GetData();
                            packet = (Packet)TSCryptography.ByteArrayToObject(packetByteArray);
                            decryptedData = tsCryptography.SymetricDecryption(packet.EncryptedData);
                            digitalSignature = packet.DigitalSignature;

                            if (tsCryptography.VerifyData(decryptedData, digitalSignature, serverPublicKey))
                            {
                                GameOver gameOver = (GameOver)TSCryptography.ByteArrayToObject(decryptedData);
                                Invoke(new Action(() => { ShowGameOver(gameOver); }));
                            }
                            break;

                        case ProtocolSICmdType.USER_OPTION_5:
                            //Receção de Jogadores Logados
                            tsProtocol.SendAck();

                            packetByteArray = protocolSI.GetData();
                            packet = (Packet) TSCryptography.ByteArrayToObject(packetByteArray);
                            decryptedData = tsCryptography.SymetricDecryption(packet.EncryptedData);
                            digitalSignature = packet.DigitalSignature;
                            

                            if (tsCryptography.VerifyData(decryptedData, digitalSignature, serverPublicKey))
                            {
                                List<string> gameRoomLoggedPlayers = (List<string>)TSCryptography.ByteArrayToObject(decryptedData);
                                Invoke(new Action(() => { UpdateLoggedUsers(gameRoomLoggedPlayers); }));
                            }
                            break;

                        case ProtocolSICmdType.USER_OPTION_8:
                            //Receção de mensagens no chat
                            tsProtocol.SendAck();

                            packetByteArray = protocolSI.GetData();
                            packet = (Packet)TSCryptography.ByteArrayToObject(packetByteArray);
                            decryptedData = tsCryptography.SymetricDecryption(packet.EncryptedData);
                            digitalSignature = packet.DigitalSignature;

                            if (tsCryptography.VerifyData(decryptedData, digitalSignature, serverPublicKey))
                            {
                                string msgRecebida = Encoding.UTF8.GetString(decryptedData);
                                safeCallDelegate = new SafeCallDelegate((message) => { receiveChatMessage(message); });
                                rtbMensagens.Invoke(safeCallDelegate, new object[] { msgRecebida });
                            }
                            else
                            {
                                Console.WriteLine("Mensagem corrompida");
                            }
                            break;

                        case ProtocolSICmdType.USER_OPTION_9:
                            //Receção de mensagens de Erro ou Sucesso
                            packetByteArray = protocolSI.GetData();
                            packet = (Packet)TSCryptography.ByteArrayToObject(packetByteArray);
                            decryptedData = tsCryptography.SymetricDecryption(packet.EncryptedData);
                            digitalSignature = packet.DigitalSignature;

                            if (tsCryptography.VerifyData(decryptedData, digitalSignature, serverPublicKey))
                            {
                                Response response = (Response)TSCryptography.ByteArrayToObject(decryptedData);
                                Invoke(new Action(() => { checkServerResponse(response.ResponseId); }));
                            }
                            else
                            {
                                Console.WriteLine("Mensagem corrompida");
                            }

                            tsProtocol.SendAck();
                            break;

                        //******************   ABRETURA DE COMUNICAÇÃO ENCRIPTADA SIMÉTRICA   ******************
                        case ProtocolSICmdType.PUBLIC_KEY:
                            //Receção da PublicKey do servidor
                            tsProtocol.SendAck();
                            serverPublicKey = protocolSI.GetStringFromData();
                            break;

                        case ProtocolSICmdType.SECRET_KEY:
                            //Receção da SecretKey
                            tsProtocol.SendAck();
                            byte[] encryptedSymKey = protocolSI.GetData();

                            //Desencripta e atribui a key ao objecto aes
                            byte[] decryptedSymKey = this.tsCryptography.RsaDecryption(encryptedSymKey, privateKey);
                            this.tsCryptography.SetAesSymKey(decryptedSymKey);
                            Console.WriteLine("Recebido SymKey: {0}", Convert.ToBase64String(decryptedSymKey));
                            break;

                        case ProtocolSICmdType.IV:
                            //Receção do vetor de inicialização
                            tsProtocol.SendAck();
                            byte[] encryptedIV = protocolSI.GetData();

                            //Desencripta e atribui o iv ao objecto aes
                            byte[] decryptedIV = tsCryptography.RsaDecryption(encryptedIV, tsCryptography.GetPrivateKey());
                            this.tsCryptography.SetAesIV(decryptedIV);
                            Console.WriteLine("Recebido IV: {0}", Convert.ToBase64String(decryptedIV));
                            break;
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            tsProtocol.SendAck();
            Thread.Sleep(1000);

            tcpClient.Close();
            stream.Close();
        }

        /**
         * <summary>    Updates the logged users na janela. </summary>
         *
         * <remarks>    Simão Pedro, 04/06/2020. </remarks>
         *
         * <param name="gameRoomLoggedPlayers"> The game room logged players. </param>
         */
        private void UpdateLoggedUsers(List<string> gameRoomLoggedPlayers)
        {
            lbLoggedClients.Items.Clear();
            foreach (string username in gameRoomLoggedPlayers)
            {
                lbLoggedClients.Items.Add(username);
            }
            if (gameRoomLoggedPlayers.Count < 2)
            {
                gameDisplay.Text = "Aguarde que se ligue outro jogador na sala!";
            }
            else
            {
                gameDisplay.Text= "Estão dois jogadores na sala! Já podem começar comunicar pelo chat e iniciar um jogo!!!";
            }
            gameDisplay.BackColor = SystemColors.ActiveCaption;
        }

        /**
         * <summary>    Shows the game over. </summary>
         *
         * <remarks>    Simão Pedro, 04/06/2020. </remarks>
         *
         * <param name="gameOver">  The game over. </param>
         */
        private void ShowGameOver(GameOver gameOver)
        {
            switch (gameOver.TypeGameOver)
            {
                case TypeGameOver.Winner:
                    if (this.playerId == gameOver.PlayerId)
                    {
                        gameDisplay.Text = string.Format("Parabéns, {0}! Ganhaste!", gameOver.PlayerUsername);
                        gameDisplay.BackColor = Color.Green;
                    }
                    else
                    {
                        gameDisplay.Text = string.Format("Ups! Parece que o(a) {0} ganhou esta partida.", gameOver.PlayerUsername);
                        gameDisplay.BackColor = Color.Red;
                    }
                    break;

                case TypeGameOver.Draw:
                    gameDisplay.Text = "Este foi renhido! Terminou empatado.";
                    gameDisplay.BackColor = Color.Gray;
                    break;

                case TypeGameOver.Abandon:
                    if (this.playerId == gameOver.PlayerId)
                    {
                        gameDisplay.Text = string.Format("{0}, abandonou o jogo.", gameOver.PlayerUsername);
                    }
                    else
                    {
                        gameDisplay.Text = string.Format("O seu adversário, {0}, abandonou o jogo.", gameOver.PlayerUsername);
                    }
                    gameDisplay.BackColor = Color.Gray;
                    DeleteBoard();
                    break;
            }
            nudBoardDimension.Enabled = true;
            
        }

        /**
         * <summary>    Check server response.
         *              Avalia qual o código de resposta que o servidor enviou.
         *              Dependendo do código mostra uma mensagem ao utilizador.</summary>
         *
         * <remarks>    Simão Pedro, 04/06/2020. </remarks>
         *
         * <param name="resultCode">    The result code. </param>
         */
        private void checkServerResponse(int resultCode)
        {
            switch (resultCode)
            {
                case 00:
                    MessageBox.Show("Não foi possível fazer o seu registo.", "Erro de Registo");
                    break;
                case 01:
                    MessageBox.Show("Login inválido.", "Erro de Login");
                    break;
                case 02:
                    MessageBox.Show("Para jogar tem de estar logado.", "Erro de Login");
                    break;
                case 03:
                    MessageBox.Show("Já está logado. Para fazer novo registo tem de fazer logout e registar um novo utilizador.", "Erro de Login");
                    break;
                case 04:
                    MessageBox.Show("O username e a password têm de ter pelo menos 8 caractéres.", "Erro de introdução de dados");
                    break;
                case 05:
                    MessageBox.Show("Assinatura diginal inválida.", "Erro de Assinatura digital.");
                    break;
                case 06:
                    MessageBox.Show("O utilizador já se encontra logado noutra aplicação cliente.", "Erro de utilizador.");
                    break;
                case 07:
                    MessageBox.Show("Para usar o chat tem de estar logado.", "Erro de Login.");
                    break;
                case 08:
                    MessageBox.Show("Erro ao tentar fazer logout no servidor.", "Erro do servidor.");
                    break;
                case 09:
                    MessageBox.Show("Sala de jogo cheia, tente novamente mais tarde.", "Erro do servidor.");
                    break;
                case 10:
                    MessageBox.Show("Registado com sucesso! Faça login para se juntar a uma sala de jogo.", "Sucesso de Registo");
                    break;
                case 11:
                    lbBoasVindas.Text = string.Format("{0}, bem-vindo(a) ao Jogo do Galo!", tbUsername.Text);
                    lbBoasVindas.BackColor = Color.DarkGreen;
                    gameDisplay.Text = string.Format("{0}, Já está logado!", tbUsername.Text);
                    btnLogin.Text = "LOGOUT";
                    break;
                case 12:
                    lbBoasVindas.Text = string.Format("Bem-vindo ao Jogo do Galo!", tbUsername.Text);
                    lbBoasVindas.BackColor = SystemColors.ActiveCaption;
                    gameDisplay.Text = "Faça Login";
                    gameDisplay.BackColor = SystemColors.ActiveCaption;
                    btnLogin.Text = "LOGIN";
                    lbLoggedClients.Items.Clear();
                    rtbMensagens.Clear();
                    dupSymbol.Enabled = true;
                    dupSymbol.SelectedIndex = 0;
                    nudBoardDimension.Enabled = true;
                    nudBoardDimension.Value = 3;
                    tbUsername.Clear();
                    tbPassword.Clear();
                    lbWinningCondition.Visible = false;
                    DeleteBoard();
                    break;
                case 20:
                    MessageBox.Show("O jogo ainda não começou! Espere que estejam duas pessoas na sala para começar o jogo.", "Erro de utilizador");
                    break;
                case 21:
                    MessageBox.Show("O jogo já começou! Não é possível iniciar um novo jogo enquanto outro está em curso.", "Erro de utilizador");
                    break;
                case 22:
                    MessageBox.Show("Jogada inválida. Jogue novamente.", "Erro de utilizador");
                    break;
                case 23:
                    MessageBox.Show("Não é a sua vez de jogar. Espere que o seu adversário jogue.", "Erro de utilizador");
                    break;
            }
        }

        /**
         * <summary>    Event handler. Called by Form1 for form closed events. </summary>
         *
         * <remarks>    Simão Pedro, 04/06/2020. </remarks>
         *
         * <param name="sender">    Source of the event. </param>
         * <param name="e">         Form closed event information. </param>
         */
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            CloseClient();
        }

        /**
         * <summary>    Closes the client.
         *              Manda para o servidor um protocolSI do tipo End Of Transmission (EOT)
         *              Fecha a stream e o tcpClient. </summary>
         *
         * <remarks>    Simão Pedro, 04/06/2020. </remarks>
         */
        private void CloseClient()
        {
            //Preparar o envio da mensagem para desligar as ligações
            byte[] eot = protocolSI.Make(ProtocolSICmdType.EOT);
            stream.Write(eot, 0, eot.Length);

            //Aguarda confirmação da receção do username
            Thread.Sleep(2000);

            stream.Close();
            tcpClient.Close();
        }

        /**
         * <summary>    Event handler. Called by btnLogin for click events. 
         *              Caso o utilizador não esteja logado, faz o pedido de login ao servidor.
         *              Caso esteja logado, faz o pedido de logout ao servidor. </summary>
         *
         * <remarks>    Simão Pedro, 04/06/2020. </remarks>
         *
         * <param name="sender">    Source of the event. </param>
         * <param name="e">         Event information. </param>
         */
        private void btnLogin_Click(object sender, EventArgs e)
        {
            if(btnLogin.Text == "LOGIN")
            {
                Credentials credentials = new Credentials(tbUsername.Text, tbPassword.Text);
                byte[] objByteArray = TSCryptography.ObjectToByteArray(credentials);

                byte[] encryptedData = tsCryptography.SymetricEncryption(objByteArray);
                byte[] digitalSignature = tsCryptography.SignData(objByteArray, privateKey);
                Packet packet = new Packet(encryptedData, digitalSignature);
                byte[] packetByteArray = TSCryptography.ObjectToByteArray(packet);
                tsProtocol.SendProtocol(ProtocolSICmdType.USER_OPTION_3, packetByteArray);

                tbPassword.Clear();
            }
            else
            {
                string logoutRequest = "Deixem-me sair!";
                byte[] logoutRequestBytes = Encoding.UTF8.GetBytes(logoutRequest);

                byte[] encryptedData = tsCryptography.SymetricEncryption(logoutRequestBytes);
                byte[] digitalSignature = tsCryptography.SignData(logoutRequestBytes, privateKey);
                Packet packet = new Packet(encryptedData, digitalSignature);
                byte[] packetByteArray = TSCryptography.ObjectToByteArray(packet);
                tsProtocol.SendProtocol(ProtocolSICmdType.USER_OPTION_5, packetByteArray);
            }
        }

        /**
         * <summary>    Event handler. Called by btnSignup for click events. 
         *              Faz o pedido de SignUp (registo) ao servidor. </summary>
         *
         * <remarks>    Simão Pedro, 04/06/2020. </remarks>
         *
         * <param name="sender">    Source of the event. </param>
         * <param name="e">         Event information. </param>
         */
        private void btnSignup_Click(object sender, EventArgs e)
        {
            Credentials credentials = new Credentials(tbUsername.Text, tbPassword.Text);
            byte[] objByteArray = TSCryptography.ObjectToByteArray(credentials);

            byte[] encryptedData = tsCryptography.SymetricEncryption(objByteArray);
            byte[] digitalSignature = tsCryptography.SignData(objByteArray, privateKey);
            Packet packet = new Packet(encryptedData, digitalSignature);
            byte[] packetByteArray = TSCryptography.ObjectToByteArray(packet);
            tsProtocol.SendProtocol(ProtocolSICmdType.USER_OPTION_4, packetByteArray);

            tbPassword.Clear();
        }

        /**
         * <summary>    Event handler. Called by bt_EnviaMensagem for click events. 
         *              Envia uma mensagem para o servidor. </summary>
         *
         * <remarks>    Simão Pedro, 04/06/2020. </remarks>
         *
         * <param name="sender">    Source of the event. </param>
         * <param name="e">         Event information. </param>
         */
        private void bt_EnviaMensagem_Click(object sender, EventArgs e)
        {
            byte[] decryptedData = Encoding.UTF8.GetBytes(tbEscreverMensagem.Text);
            
            byte[] encryptedData = tsCryptography.SymetricEncryption(decryptedData);
            byte[] digitalSignature = tsCryptography.SignData(decryptedData, privateKey);
            Packet packet = new Packet(encryptedData, digitalSignature);
            byte[] packetByteArray = TSCryptography.ObjectToByteArray(packet);
            tsProtocol.SendProtocol(ProtocolSICmdType.USER_OPTION_8, packetByteArray);

            tbEscreverMensagem.Clear();
        }

        /**
         * <summary>    Receive chat message. 
         *              Recebe as mensagens do servidor que se destinam ao chat. </summary>
         *
         * <remarks>    Simão Pedro, 04/06/2020. </remarks>
         *
         * <param name="message">   The message. </param>
         */
        private void receiveChatMessage(string message)
        {
            message = message + Environment.NewLine;
            rtbMensagens.AppendText(message);
        }

        /**
         * <summary>    Determine winning condition. 
         *              Dependendo do tamanho do tabuleiro, determina qual a condição de vitória. </summary>
         *
         * <remarks>    Simão Pedro, 04/06/2020. </remarks>
         *
         * <param name="boardsize"> The boardsize. </param>
         *
         * <returns>    A string. </returns>
         */
        private string DetermineWinningCondition(int boardsize)
        {
            
            int winningCondition = 0;
            switch (boardsize)
            {
                case 3:
                    winningCondition = boardsize;
                    break;
                case 4:
                case 5:
                case 6:
                    winningCondition = 4;
                    break;
                case 7:
                case 8:
                    winningCondition = 5;
                    break;
                case 9:
                    winningCondition = 6;
                    break;
            }
            string infoWinningCondition = string.Format("Num tabuleiro com {0} x {0} de dimensão, a condição de vitória é fazer {1} em linha.", boardsize, winningCondition);
            return infoWinningCondition;
        }

        /**
         * <summary>    Draw board. 
         *              Desenha na janela:
         *                  - o tabuleuro de jogo na janela;   
         *                  - o numeric up and down do tamanho do tabuleiro  
         *                  - informação sobre a condição de vitória. </summary>
         *
         * <remarks>    Simão Pedro, 04/06/2020. </remarks>
         *
         * <param name="offset_x">  The offset x coordinate. </param>
         * <param name="offset_y">  The offset y coordinate. </param>
         * <param name="size">      The size. </param>
         * <param name="boardsize"> The boardsize. </param>
         */
        private void DrawBoard(int offset_x, int offset_y, int size, int boardsize)
        {
            DeleteBoard();
            
            nudBoardDimension.Value = boardsize;
            nudBoardDimension.Enabled = false;

            lbWinningCondition.Text = DetermineWinningCondition(boardsize);
            lbWinningCondition.Visible = true;

            gameBoard = new List<Button>();
            int padding = Convert.ToInt32(Math.Round(size * 0.02, MidpointRounding.AwayFromZero));
            int buttonSize = (size - (padding * (boardsize + 1))) / boardsize;
            int buttonCaptionSize = (buttonSize * 40) / 122;
            for (int J = 0; J < boardsize; J++)
            {
                for (int I = 0; I < boardsize; I++)
                {
                    Button newButton = new Button();
                    newButton.Text = "";
                    newButton.Name = I + "_" + J;
                    newButton.Location = new Point(offset_y + padding + (J * (padding + buttonSize)), offset_x + padding + (I * (padding + buttonSize)));
                    newButton.Width = buttonSize;
                    newButton.Height = buttonSize;
                    newButton.BackColor = System.Drawing.Color.LightGray;
                    newButton.Font = new Font(newButton.Font.FontFamily, buttonCaptionSize);
                    newButton.Click += BoardClick;
                    gameBoard.Add(newButton);
                    this.Controls.Add(newButton);
                }
            }  
        }

        /**
         * <summary>    Shows the active player. 
         *              Altera os elementos gráficos
         *              de modo a alertar o utilizador
         *              para quando é ou não a sua vez de jogar. </summary>
         *
         * <remarks>    Simão Pedro, 04/06/2020. </remarks>
         *
         * <param name="number">    Number of. </param>
         */
        private void ShowActivePlayer(int number)
        {
            bool isPlayerTurn = playerId == number ? true : false;

            if(isPlayerTurn)
            {
                gameDisplay.Text = "É a sua vez de jogar.";
                gameDisplay.ForeColor = Color.White;
                gameDisplay.BackColor = Color.Green;
                BlockBoard(isPlayerTurn);
            }
            else
            {
                gameDisplay.Text = "Aguarde pela sua vez de jogar.";
                gameDisplay.ForeColor = Color.White;
                gameDisplay.BackColor = Color.Gray;
                BlockBoard(isPlayerTurn);
            }
        }

        /**
         * <summary>    Block board. 
         *              Bloqueia o tabuleiro de jogo. </summary>
         *
         * <remarks>    Simão Pedro, 04/06/2020. </remarks>
         *
         * <param name="isPlayerTurn">  True if is player turn, false if not. </param>
         */
        private void BlockBoard(bool isPlayerTurn)
        {
            foreach(Button btn in gameBoard)
            {
                btn.Enabled = isPlayerTurn;

                if (btn.Text == dupSymbol.Text || btn.Text == "O")
                {
                    btn.Enabled = false;
                }
            }
        }

        /**
         * <summary>    Board click. 
         *              Regista a jogada do utilizador e envia-a para o servidor. </summary>
         *
         * <remarks>    Simão Pedro, 04/06/2020. </remarks>
         *
         * <param name="sender">    Source of the event. </param>
         * <param name="e">         Event information. </param>
         */
        private void BoardClick(object sender, EventArgs e)
        {
            Button clickedButton = sender as Button;
            List<int> coord = GetClickedButton(clickedButton);

            GamePlay gamePlay = new GamePlay(coord[0], coord[1], playerId);
            byte[] objByteArray = TSCryptography.ObjectToByteArray(gamePlay);

            byte[] encryptedData = tsCryptography.SymetricEncryption(objByteArray);
            byte[] digitalSignature = tsCryptography.SignData(objByteArray, privateKey);

            Packet packet = new Packet(encryptedData, digitalSignature);
            byte[] packetByteArray = TSCryptography.ObjectToByteArray(packet);


            tsProtocol.SendProtocol(ProtocolSICmdType.USER_OPTION_7, packetByteArray);
        }

        /**
         * <summary>    Gets clicked button. 
         *              Devolve as coordenadas do butão (do tabuleiro de jogo) que foi clicada pelo utilizador. </summary>
         *
         * <remarks>    Simão Pedro, 04/06/2020. </remarks>
         *
         * <param name="clickedButton"> The clicked control. </param>
         *
         * <returns>    The clicked button. </returns>
         */
        private List<int> GetClickedButton(Button clickedButton)
        {
            string[] a = clickedButton.Name.Split('_');
            List<int> coord = new List<int>();
            coord.Add(int.Parse(a[0]));
            coord.Add(int.Parse(a[1]));
            return coord;
        }

        /**
         * <summary>    Event handler. Called by btnGameStart for click events. 
         *              Envia para o servido o pedido para iniciar uma partida. </summary>
         *
         * <remarks>    Simão Pedro, 04/06/2020. </remarks>
         *
         * <param name="sender">    Source of the event. </param>
         * <param name="e">         Event information. </param>
         */
        private void btnGameStart_Click(object sender, EventArgs e)
        {
            int boardSize = decimal.ToInt32(nudBoardDimension.Value);
            StartGame startGame= new StartGame(boardSize, 0);
            byte[] objByteArray = TSCryptography.ObjectToByteArray(startGame);

            byte[] encryptedData = tsCryptography.SymetricEncryption(objByteArray);
            byte[] digitalSignature = tsCryptography.SignData(objByteArray, privateKey);

            Packet packet = new Packet(encryptedData, digitalSignature);
            byte[] packetByteArray = TSCryptography.ObjectToByteArray(packet);

            tsProtocol.SendProtocol(ProtocolSICmdType.USER_OPTION_6, packetByteArray);

            tbPassword.Clear();
        }

        /**
         * <summary>    Draw plays. 
         *              Desenha no tabuleiro de jogo todas as jogadas que já foram feitas durante o jogo. </summary>
         *
         * <remarks>    Simão Pedro, 04/06/2020. </remarks>
         *
         * <param name="listOfGamePlays">   The list of game plays. </param>
         */
        private void DrawPlays(List<GamePlay> listOfGamePlays)
        {

            foreach (GamePlay gamePlay in listOfGamePlays)
            {
                string btnName = gamePlay.Coord_x.ToString() + "_" + gamePlay.Coord_y.ToString();
                
                if (gameBoard.Exists((btn) => btn.Name == btnName))
                {
                    Button btnAModificar = gameBoard.Find((x) => x.Name == btnName);

                    if (gamePlay.playerId == playerId)
                    {
                        btnAModificar.Text = dupSymbol.Text;
                        btnAModificar.BackColor = Color.Honeydew;
                    }
                    else
                    {
                        btnAModificar.Text = "O";
                    }
                }
            }
        }

        /**
         * <summary>    Deletes the board. 
         *              Apaga o tabuleiro de jogo. </summary>
         *
         * <remarks>    Simão Pedro, 04/06/2020. </remarks>
         */
        private void DeleteBoard()
        {
            if(gameBoard != null && gameBoard.Count > 0)
            {
                foreach (Button btn in gameBoard)
                {
                    btn.Dispose();
                }
                gameBoard.Clear();
            }
        }
    }
}
