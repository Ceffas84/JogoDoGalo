namespace JogoDoGaloV1._0
{
    partial class JogoDoGalo_ClientForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.gBoxServer = new System.Windows.Forms.GroupBox();
            this.btnSignup = new System.Windows.Forms.Button();
            this.btnLogin = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tbServidor = new System.Windows.Forms.TextBox();
            this.tbUsername = new System.Windows.Forms.TextBox();
            this.tbPassword = new System.Windows.Forms.TextBox();
            this.gBoxChat = new System.Windows.Forms.GroupBox();
            this.rtbMensagens = new System.Windows.Forms.RichTextBox();
            this.tbEscreverMensagem = new System.Windows.Forms.TextBox();
            this.bt_EnviaMensagem = new System.Windows.Forms.Button();
            this.tbChat = new System.Windows.Forms.TextBox();
            this.tbServer = new System.Windows.Forms.TextBox();
            this.gameDisplay = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnGameStart = new System.Windows.Forms.Button();
            this.gbPlayersBoard = new System.Windows.Forms.GroupBox();
            this.lbLoggedClients = new System.Windows.Forms.ListBox();
            this.gBoxServer.SuspendLayout();
            this.gBoxChat.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.gbPlayersBoard.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(919, 371);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(899, 440);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(142, 23);
            this.button2.TabIndex = 3;
            this.button2.Text = "Add New Client";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // gBoxServer
            // 
            this.gBoxServer.Controls.Add(this.btnSignup);
            this.gBoxServer.Controls.Add(this.btnLogin);
            this.gBoxServer.Controls.Add(this.label1);
            this.gBoxServer.Controls.Add(this.label2);
            this.gBoxServer.Controls.Add(this.label3);
            this.gBoxServer.Controls.Add(this.tbServidor);
            this.gBoxServer.Controls.Add(this.tbUsername);
            this.gBoxServer.Controls.Add(this.tbPassword);
            this.gBoxServer.Location = new System.Drawing.Point(600, 12);
            this.gBoxServer.Name = "gBoxServer";
            this.gBoxServer.Size = new System.Drawing.Size(250, 154);
            this.gBoxServer.TabIndex = 38;
            this.gBoxServer.TabStop = false;
            this.gBoxServer.Text = "Ligação";
            // 
            // btnSignup
            // 
            this.btnSignup.Location = new System.Drawing.Point(169, 91);
            this.btnSignup.Margin = new System.Windows.Forms.Padding(2);
            this.btnSignup.Name = "btnSignup";
            this.btnSignup.Size = new System.Drawing.Size(74, 29);
            this.btnSignup.TabIndex = 26;
            this.btnSignup.Text = "SINGUP";
            this.btnSignup.UseVisualStyleBackColor = true;
            this.btnSignup.Click += new System.EventHandler(this.btnSignup_Click);
            // 
            // btnLogin
            // 
            this.btnLogin.Location = new System.Drawing.Point(70, 91);
            this.btnLogin.Margin = new System.Windows.Forms.Padding(2);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(74, 29);
            this.btnLogin.TabIndex = 25;
            this.btnLogin.Text = "LOGIN";
            this.btnLogin.UseVisualStyleBackColor = true;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 22);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 13);
            this.label1.TabIndex = 19;
            this.label1.Text = "Servidor";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(5, 46);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 13);
            this.label2.TabIndex = 20;
            this.label2.Text = "Jogador";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 70);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 13);
            this.label3.TabIndex = 21;
            this.label3.Text = "Password";
            // 
            // tbServidor
            // 
            this.tbServidor.Location = new System.Drawing.Point(70, 19);
            this.tbServidor.Margin = new System.Windows.Forms.Padding(2);
            this.tbServidor.Name = "tbServidor";
            this.tbServidor.Size = new System.Drawing.Size(173, 20);
            this.tbServidor.TabIndex = 22;
            this.tbServidor.Text = "127.0.0.1";
            // 
            // tbUsername
            // 
            this.tbUsername.Location = new System.Drawing.Point(70, 43);
            this.tbUsername.Margin = new System.Windows.Forms.Padding(2);
            this.tbUsername.Name = "tbUsername";
            this.tbUsername.Size = new System.Drawing.Size(173, 20);
            this.tbUsername.TabIndex = 23;
            // 
            // tbPassword
            // 
            this.tbPassword.Location = new System.Drawing.Point(70, 67);
            this.tbPassword.Margin = new System.Windows.Forms.Padding(2);
            this.tbPassword.Name = "tbPassword";
            this.tbPassword.Size = new System.Drawing.Size(173, 20);
            this.tbPassword.TabIndex = 24;
            // 
            // gBoxChat
            // 
            this.gBoxChat.Controls.Add(this.rtbMensagens);
            this.gBoxChat.Controls.Add(this.tbEscreverMensagem);
            this.gBoxChat.Controls.Add(this.bt_EnviaMensagem);
            this.gBoxChat.Location = new System.Drawing.Point(600, 175);
            this.gBoxChat.Name = "gBoxChat";
            this.gBoxChat.Size = new System.Drawing.Size(250, 449);
            this.gBoxChat.TabIndex = 39;
            this.gBoxChat.TabStop = false;
            this.gBoxChat.Text = "Chat Jogadores";
            // 
            // rtbMensagens
            // 
            this.rtbMensagens.BackColor = System.Drawing.SystemColors.ControlLight;
            this.rtbMensagens.Location = new System.Drawing.Point(8, 19);
            this.rtbMensagens.Name = "rtbMensagens";
            this.rtbMensagens.ReadOnly = true;
            this.rtbMensagens.Size = new System.Drawing.Size(236, 344);
            this.rtbMensagens.TabIndex = 35;
            this.rtbMensagens.Text = "";
            // 
            // tbEscreverMensagem
            // 
            this.tbEscreverMensagem.Location = new System.Drawing.Point(8, 368);
            this.tbEscreverMensagem.Margin = new System.Windows.Forms.Padding(2);
            this.tbEscreverMensagem.Multiline = true;
            this.tbEscreverMensagem.Name = "tbEscreverMensagem";
            this.tbEscreverMensagem.Size = new System.Drawing.Size(236, 37);
            this.tbEscreverMensagem.TabIndex = 27;
            // 
            // bt_EnviaMensagem
            // 
            this.bt_EnviaMensagem.Location = new System.Drawing.Point(8, 409);
            this.bt_EnviaMensagem.Margin = new System.Windows.Forms.Padding(2);
            this.bt_EnviaMensagem.Name = "bt_EnviaMensagem";
            this.bt_EnviaMensagem.Size = new System.Drawing.Size(237, 35);
            this.bt_EnviaMensagem.TabIndex = 28;
            this.bt_EnviaMensagem.Text = "Enviar Mensagem";
            this.bt_EnviaMensagem.UseVisualStyleBackColor = true;
            this.bt_EnviaMensagem.Click += new System.EventHandler(this.bt_EnviaMensagem_Click);
            // 
            // tbChat
            // 
            this.tbChat.Location = new System.Drawing.Point(919, 325);
            this.tbChat.Name = "tbChat";
            this.tbChat.Size = new System.Drawing.Size(155, 20);
            this.tbChat.TabIndex = 1;
            // 
            // tbServer
            // 
            this.tbServer.Location = new System.Drawing.Point(919, 194);
            this.tbServer.Name = "tbServer";
            this.tbServer.Size = new System.Drawing.Size(142, 20);
            this.tbServer.TabIndex = 2;
            this.tbServer.Text = "127.0.0.1";
            // 
            // gameDisplay
            // 
            this.gameDisplay.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.gameDisplay.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gameDisplay.ForeColor = System.Drawing.Color.White;
            this.gameDisplay.Location = new System.Drawing.Point(6, 19);
            this.gameDisplay.Name = "gameDisplay";
            this.gameDisplay.Size = new System.Drawing.Size(259, 91);
            this.gameDisplay.TabIndex = 46;
            this.gameDisplay.Text = "Faça Login";
            this.gameDisplay.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.gameDisplay);
            this.groupBox2.Controls.Add(this.btnGameStart);
            this.groupBox2.Location = new System.Drawing.Point(309, 470);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(270, 154);
            this.groupBox2.TabIndex = 45;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Informação de Jogo";
            // 
            // btnGameStart
            // 
            this.btnGameStart.Location = new System.Drawing.Point(5, 114);
            this.btnGameStart.Margin = new System.Windows.Forms.Padding(2);
            this.btnGameStart.Name = "btnGameStart";
            this.btnGameStart.Size = new System.Drawing.Size(260, 35);
            this.btnGameStart.TabIndex = 36;
            this.btnGameStart.Text = "START";
            this.btnGameStart.UseVisualStyleBackColor = true;
            // 
            // gbPlayersBoard
            // 
            this.gbPlayersBoard.Controls.Add(this.lbLoggedClients);
            this.gbPlayersBoard.Location = new System.Drawing.Point(18, 470);
            this.gbPlayersBoard.Name = "gbPlayersBoard";
            this.gbPlayersBoard.Size = new System.Drawing.Size(270, 154);
            this.gbPlayersBoard.TabIndex = 44;
            this.gbPlayersBoard.TabStop = false;
            this.gbPlayersBoard.Text = "Jogadores Ligados";
            // 
            // lbLoggedClients
            // 
            this.lbLoggedClients.FormattingEnabled = true;
            this.lbLoggedClients.Location = new System.Drawing.Point(6, 19);
            this.lbLoggedClients.Name = "lbLoggedClients";
            this.lbLoggedClients.Size = new System.Drawing.Size(258, 121);
            this.lbLoggedClients.TabIndex = 36;
            // 
            // JogoDoGalo_ClientForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1086, 703);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.gbPlayersBoard);
            this.Controls.Add(this.gBoxChat);
            this.Controls.Add(this.gBoxServer);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.tbServer);
            this.Controls.Add(this.tbChat);
            this.Controls.Add(this.button1);
            this.Name = "JogoDoGalo_ClientForm";
            this.Text = "Form1";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.gBoxServer.ResumeLayout(false);
            this.gBoxServer.PerformLayout();
            this.gBoxChat.ResumeLayout(false);
            this.gBoxChat.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.gbPlayersBoard.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.GroupBox gBoxServer;
        private System.Windows.Forms.Button btnSignup;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbServidor;
        private System.Windows.Forms.TextBox tbUsername;
        private System.Windows.Forms.TextBox tbPassword;
        private System.Windows.Forms.GroupBox gBoxChat;
        private System.Windows.Forms.RichTextBox rtbMensagens;
        private System.Windows.Forms.TextBox tbEscreverMensagem;
        private System.Windows.Forms.Button bt_EnviaMensagem;
        private System.Windows.Forms.TextBox tbChat;
        private System.Windows.Forms.TextBox tbServer;
        private System.Windows.Forms.Label gameDisplay;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnGameStart;
        private System.Windows.Forms.GroupBox gbPlayersBoard;
        private System.Windows.Forms.ListBox lbLoggedClients;
    }
}

