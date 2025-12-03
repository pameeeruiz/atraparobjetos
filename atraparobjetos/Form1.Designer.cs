using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace atraparobjetos
{
    partial class Form1
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private IContainer components = null;

        private Panel panelGame;
        private PictureBox pbCatcher;
        private Label lblScore;
        private Label lblTime;
        private Label lblResult;
        private Button btnRestart;
        private Timer timerGame;
        private Timer timerSpawn;
        private Timer timerTime;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Método necesario para admitir el Diseñador. No modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.panelGame = new System.Windows.Forms.Panel();
            this.pbCatcher = new System.Windows.Forms.PictureBox();
            this.lblScore = new System.Windows.Forms.Label();
            this.lblTime = new System.Windows.Forms.Label();
            this.lblResult = new System.Windows.Forms.Label();
            this.btnRestart = new System.Windows.Forms.Button();
            this.timerGame = new System.Windows.Forms.Timer(this.components);
            this.timerSpawn = new System.Windows.Forms.Timer(this.components);
            this.timerTime = new System.Windows.Forms.Timer(this.components);
            this.panelGame.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbCatcher)).BeginInit();
            this.SuspendLayout();
            // 
            // panelGame
            // 
            this.panelGame.BackColor = System.Drawing.Color.AliceBlue;
            this.panelGame.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelGame.Controls.Add(this.pbCatcher);
            this.panelGame.Location = new System.Drawing.Point(20, 20);
            this.panelGame.Name = "panelGame";
            this.panelGame.Size = new System.Drawing.Size(660, 380);
            this.panelGame.TabIndex = 0;
            this.panelGame.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PanelGame_MouseMove);
            // 
            // pbCatcher
            // 
            this.pbCatcher.BackColor = System.Drawing.Color.DimGray;
            this.pbCatcher.Location = new System.Drawing.Point(280, 350);
            this.pbCatcher.Name = "pbCatcher";
            this.pbCatcher.Size = new System.Drawing.Size(100, 20);
            this.pbCatcher.TabIndex = 1;
            this.pbCatcher.TabStop = false;
            // 
            // lblScore
            // 
            this.lblScore.AutoSize = true;
            this.lblScore.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblScore.Location = new System.Drawing.Point(20, 420);
            this.lblScore.Name = "lblScore";
            this.lblScore.Size = new System.Drawing.Size(84, 23);
            this.lblScore.TabIndex = 1;
            this.lblScore.Text = "Puntos: 0";
            // 
            // lblTime
            // 
            this.lblTime.AutoSize = true;
            this.lblTime.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblTime.Location = new System.Drawing.Point(140, 420);
            this.lblTime.Name = "lblTime";
            this.lblTime.Size = new System.Drawing.Size(91, 23);
            this.lblTime.TabIndex = 2;
            this.lblTime.Text = "Tiempo: 0";
            // 
            // lblResult
            // 
            this.lblResult.AutoSize = true;
            this.lblResult.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblResult.Location = new System.Drawing.Point(260, 420);
            this.lblResult.Name = "lblResult";
            this.lblResult.Size = new System.Drawing.Size(0, 23);
            this.lblResult.TabIndex = 3;
            // 
            // btnRestart
            // 
            this.btnRestart.Location = new System.Drawing.Point(560, 415);
            this.btnRestart.Name = "btnRestart";
            this.btnRestart.Size = new System.Drawing.Size(120, 30);
            this.btnRestart.TabIndex = 4;
            this.btnRestart.Text = "Reiniciar";
            this.btnRestart.UseVisualStyleBackColor = true;
            this.btnRestart.Click += new System.EventHandler(this.BtnRestart_Click);
            // 
            // timerGame
            // 
            this.timerGame.Interval = 20;
            this.timerGame.Tick += new System.EventHandler(this.TimerGame_Tick);
            // 
            // timerSpawn
            // 
            this.timerSpawn.Interval = 800;
            this.timerSpawn.Tick += new System.EventHandler(this.TimerSpawn_Tick);
            // 
            // timerTime
            // 
            this.timerTime.Interval = 1000;
            this.timerTime.Tick += new System.EventHandler(this.TimerTime_Tick);
            // 
            // Form1
            // 
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(700, 520);
            this.Controls.Add(this.panelGame);
            this.Controls.Add(this.lblScore);
            this.Controls.Add(this.lblTime);
            this.Controls.Add(this.lblResult);
            this.Controls.Add(this.btnRestart);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "Atrapa Objetos - Catch the Ball";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            this.panelGame.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbCatcher)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}

