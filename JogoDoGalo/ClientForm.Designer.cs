﻿namespace JogoDoGalo
{
    partial class ClientForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ClientForm));
            this.button1 = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.bt_EnviaMensagem = new System.Windows.Forms.Button();
            this.tb_EscreveMensagem = new System.Windows.Forms.TextBox();
            this.bt_Autenticar = new System.Windows.Forms.Button();
            this.tb_Password = new System.Windows.Forms.TextBox();
            this.tb_Jogador = new System.Windows.Forms.TextBox();
            this.tb_Servidor = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.rtbMensagens = new System.Windows.Forms.RichTextBox();
            this.gBoxServer = new System.Windows.Forms.GroupBox();
            this.gBoxChat = new System.Windows.Forms.GroupBox();
            this.gBoxPlayers = new System.Windows.Forms.GroupBox();
            this.btnAddClient = new System.Windows.Forms.Button();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStrip2 = new System.Windows.Forms.ToolStrip();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.sairToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gBoxServer.SuspendLayout();
            this.gBoxChat.SuspendLayout();
            this.gBoxPlayers.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(623, 26);
            this.button1.Margin = new System.Windows.Forms.Padding(2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(121, 19);
            this.button1.TabIndex = 34;
            this.button1.Text = "Botao Teste Jogo";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(427, 34);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(46, 13);
            this.label8.TabIndex = 33;
            this.label8.Text = "Servidor";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(133, 55);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(43, 13);
            this.label6.TabIndex = 32;
            this.label6.Text = "aaaaaa";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(36, 55);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(43, 13);
            this.label7.TabIndex = 31;
            this.label7.Text = "aaaaaa";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(133, 25);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(54, 13);
            this.label5.TabIndex = 30;
            this.label5.Text = "Jogador 2";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(36, 25);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(54, 13);
            this.label4.TabIndex = 29;
            this.label4.Text = "Jogador 1";
            // 
            // bt_EnviaMensagem
            // 
            this.bt_EnviaMensagem.Location = new System.Drawing.Point(7, 357);
            this.bt_EnviaMensagem.Margin = new System.Windows.Forms.Padding(2);
            this.bt_EnviaMensagem.Name = "bt_EnviaMensagem";
            this.bt_EnviaMensagem.Size = new System.Drawing.Size(237, 35);
            this.bt_EnviaMensagem.TabIndex = 28;
            this.bt_EnviaMensagem.Text = "Enviar Mensagem";
            this.bt_EnviaMensagem.UseVisualStyleBackColor = true;
            this.bt_EnviaMensagem.Click += new System.EventHandler(this.bt_EnviaMensagem_Click);
            // 
            // tb_EscreveMensagem
            // 
            this.tb_EscreveMensagem.Location = new System.Drawing.Point(7, 316);
            this.tb_EscreveMensagem.Margin = new System.Windows.Forms.Padding(2);
            this.tb_EscreveMensagem.Multiline = true;
            this.tb_EscreveMensagem.Name = "tb_EscreveMensagem";
            this.tb_EscreveMensagem.Size = new System.Drawing.Size(236, 37);
            this.tb_EscreveMensagem.TabIndex = 27;
            // 
            // bt_Autenticar
            // 
            this.bt_Autenticar.Location = new System.Drawing.Point(70, 91);
            this.bt_Autenticar.Margin = new System.Windows.Forms.Padding(2);
            this.bt_Autenticar.Name = "bt_Autenticar";
            this.bt_Autenticar.Size = new System.Drawing.Size(173, 29);
            this.bt_Autenticar.TabIndex = 25;
            this.bt_Autenticar.Text = "SIGN IN  / SING UP";
            this.bt_Autenticar.UseVisualStyleBackColor = true;
            this.bt_Autenticar.Click += new System.EventHandler(this.bt_Autenticar_Click);
            // 
            // tb_Password
            // 
            this.tb_Password.Location = new System.Drawing.Point(70, 67);
            this.tb_Password.Margin = new System.Windows.Forms.Padding(2);
            this.tb_Password.Name = "tb_Password";
            this.tb_Password.Size = new System.Drawing.Size(173, 20);
            this.tb_Password.TabIndex = 24;
            // 
            // tb_Jogador
            // 
            this.tb_Jogador.Location = new System.Drawing.Point(70, 43);
            this.tb_Jogador.Margin = new System.Windows.Forms.Padding(2);
            this.tb_Jogador.Name = "tb_Jogador";
            this.tb_Jogador.Size = new System.Drawing.Size(173, 20);
            this.tb_Jogador.TabIndex = 23;
            // 
            // tb_Servidor
            // 
            this.tb_Servidor.Location = new System.Drawing.Point(70, 19);
            this.tb_Servidor.Margin = new System.Windows.Forms.Padding(2);
            this.tb_Servidor.Name = "tb_Servidor";
            this.tb_Servidor.Size = new System.Drawing.Size(173, 20);
            this.tb_Servidor.TabIndex = 22;
            this.tb_Servidor.Text = "127.0.0.1";
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
            // rtbMensagens
            // 
            this.rtbMensagens.BackColor = System.Drawing.SystemColors.ControlLight;
            this.rtbMensagens.Location = new System.Drawing.Point(8, 19);
            this.rtbMensagens.Name = "rtbMensagens";
            this.rtbMensagens.ReadOnly = true;
            this.rtbMensagens.Size = new System.Drawing.Size(236, 292);
            this.rtbMensagens.TabIndex = 35;
            this.rtbMensagens.Text = "";
            // 
            // gBoxServer
            // 
            this.gBoxServer.Controls.Add(this.bt_Autenticar);
            this.gBoxServer.Controls.Add(this.label1);
            this.gBoxServer.Controls.Add(this.label2);
            this.gBoxServer.Controls.Add(this.label3);
            this.gBoxServer.Controls.Add(this.tb_Servidor);
            this.gBoxServer.Controls.Add(this.tb_Jogador);
            this.gBoxServer.Controls.Add(this.tb_Password);
            this.gBoxServer.Location = new System.Drawing.Point(600, 50);
            this.gBoxServer.Name = "gBoxServer";
            this.gBoxServer.Size = new System.Drawing.Size(250, 125);
            this.gBoxServer.TabIndex = 37;
            this.gBoxServer.TabStop = false;
            this.gBoxServer.Text = "Ligação";
            // 
            // gBoxChat
            // 
            this.gBoxChat.Controls.Add(this.rtbMensagens);
            this.gBoxChat.Controls.Add(this.tb_EscreveMensagem);
            this.gBoxChat.Controls.Add(this.bt_EnviaMensagem);
            this.gBoxChat.Location = new System.Drawing.Point(600, 200);
            this.gBoxChat.Name = "gBoxChat";
            this.gBoxChat.Size = new System.Drawing.Size(250, 400);
            this.gBoxChat.TabIndex = 38;
            this.gBoxChat.TabStop = false;
            this.gBoxChat.Text = "Chat Jogadores";
            // 
            // gBoxPlayers
            // 
            this.gBoxPlayers.Controls.Add(this.label6);
            this.gBoxPlayers.Controls.Add(this.label4);
            this.gBoxPlayers.Controls.Add(this.label5);
            this.gBoxPlayers.Controls.Add(this.label7);
            this.gBoxPlayers.Controls.Add(this.label8);
            this.gBoxPlayers.Location = new System.Drawing.Point(50, 500);
            this.gBoxPlayers.Name = "gBoxPlayers";
            this.gBoxPlayers.Size = new System.Drawing.Size(500, 100);
            this.gBoxPlayers.TabIndex = 36;
            this.gBoxPlayers.TabStop = false;
            this.gBoxPlayers.Text = "Jogadores";
            // 
            // btnAddClient
            // 
            this.btnAddClient.Location = new System.Drawing.Point(658, 617);
            this.btnAddClient.Margin = new System.Windows.Forms.Padding(2);
            this.btnAddClient.Name = "btnAddClient";
            this.btnAddClient.Size = new System.Drawing.Size(121, 19);
            this.btnAddClient.TabIndex = 39;
            this.btnAddClient.Text = "Adicionar Cliente";
            this.btnAddClient.UseVisualStyleBackColor = true;
            this.btnAddClient.Click += new System.EventHandler(this.btnAddClient_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripDropDownButton1});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(884, 25);
            this.toolStrip1.TabIndex = 40;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStrip2
            // 
            this.toolStrip2.Location = new System.Drawing.Point(0, 25);
            this.toolStrip2.Name = "toolStrip2";
            this.toolStrip2.Size = new System.Drawing.Size(884, 25);
            this.toolStrip2.TabIndex = 41;
            this.toolStrip2.Text = "toolStrip2";
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sairToolStripMenuItem});
            this.toolStripDropDownButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton1.Image")));
            this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(62, 22);
            this.toolStripDropDownButton1.Text = "Ficheiro";
            // 
            // sairToolStripMenuItem
            // 
            this.sairToolStripMenuItem.Name = "sairToolStripMenuItem";
            this.sairToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.sairToolStripMenuItem.Text = "Sair";
            this.sairToolStripMenuItem.Click += new System.EventHandler(this.sairToolStripMenuItem_Click);
            // 
            // ClientForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(884, 661);
            this.Controls.Add(this.toolStrip2);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.btnAddClient);
            this.Controls.Add(this.gBoxPlayers);
            this.Controls.Add(this.gBoxChat);
            this.Controls.Add(this.gBoxServer);
            this.Controls.Add(this.button1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "ClientForm";
            this.Text = "Jogo do Galo - Cliente";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ClientForm_FormClosing);
            this.gBoxServer.ResumeLayout(false);
            this.gBoxServer.PerformLayout();
            this.gBoxChat.ResumeLayout(false);
            this.gBoxChat.PerformLayout();
            this.gBoxPlayers.ResumeLayout(false);
            this.gBoxPlayers.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button bt_EnviaMensagem;
        private System.Windows.Forms.TextBox tb_EscreveMensagem;
        private System.Windows.Forms.Button bt_Autenticar;
        private System.Windows.Forms.TextBox tb_Password;
        private System.Windows.Forms.TextBox tb_Jogador;
        private System.Windows.Forms.TextBox tb_Servidor;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RichTextBox rtbMensagens;
        private System.Windows.Forms.GroupBox gBoxServer;
        private System.Windows.Forms.GroupBox gBoxChat;
        private System.Windows.Forms.GroupBox gBoxPlayers;
        private System.Windows.Forms.Button btnAddClient;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripMenuItem sairToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStrip2;
    }
}

