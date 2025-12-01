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
            this.components = new Container();
            this.panelGame = new Panel();
            this.pbCatcher = new PictureBox();
            this.lblScore = new Label();
            this.lblTime = new Label();
            this.lblResult = new Label();
            this.btnRestart = new Button();
            this.timerGame = new Timer(this.components);
            this.timerSpawn = new Timer(this.components);
            this.timerTime = new Timer(this.components);

            // Form1
            this.SuspendLayout();
            this.ClientSize = new Size(700, 520);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "Atrapa Objetos - Catch the Ball";
            this.KeyPreview = true;
            this.BackColor = Color.White;

            // panelGame
            this.panelGame.Location = new Point(20, 20);
            this.panelGame.Size = new Size(660, 380);
            this.panelGame.BorderStyle = BorderStyle.FixedSingle;
            this.panelGame.BackColor = Color.AliceBlue;
            this.panelGame.Name = "panelGame";
            this.panelGame.TabIndex = 0;
            this.panelGame.MouseMove += new MouseEventHandler(this.PanelGame_MouseMove);
            this.Controls.Add(this.panelGame);

            // pbCatcher
            this.pbCatcher.Size = new Size(100, 20);
            this.pbCatcher.BackColor = Color.DimGray;
            this.pbCatcher.Location = new Point((this.panelGame.Width - this.pbCatcher.Width) / 2, this.panelGame.Height - 30);
            this.pbCatcher.Name = "pbCatcher";
            this.pbCatcher.TabIndex = 1;
            this.pbCatcher.TabStop = false;
            this.panelGame.Controls.Add(this.pbCatcher);

            // lblScore
            this.lblScore.AutoSize = true;
            this.lblScore.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.lblScore.Location = new Point(20, 420);
            this.lblScore.Name = "lblScore";
            this.lblScore.Size = new Size(70, 19);
            this.lblScore.Text = "Puntos: 0";
            this.Controls.Add(this.lblScore);

            // lblTime
            this.lblTime.AutoSize = true;
            this.lblTime.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.lblTime.Location = new Point(140, 420);
            this.lblTime.Name = "lblTime";
            this.lblTime.Size = new Size(80, 19);
            this.lblTime.Text = "Tiempo: 0";
            this.Controls.Add(this.lblTime);

            // lblResult
            this.lblResult.AutoSize = true;
            this.lblResult.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.lblResult.Location = new Point(260, 420);
            this.lblResult.Name = "lblResult";
            this.lblResult.Size = new Size(0, 19);
            this.lblResult.Text = "";
            this.Controls.Add(this.lblResult);

            // btnRestart
            this.btnRestart.Location = new Point(560, 415);
            this.btnRestart.Name = "btnRestart";
            this.btnRestart.Size = new Size(120, 30);
            this.btnRestart.Text = "Reiniciar";
            this.btnRestart.UseVisualStyleBackColor = true;
            this.btnRestart.Click += new EventHandler(this.BtnRestart_Click);
            this.Controls.Add(this.btnRestart);

            // timerGame
            this.timerGame.Interval = 20;
            this.timerGame.Tick += new EventHandler(this.TimerGame_Tick);

            // timerSpawn
            this.timerSpawn.Interval = 800;
            this.timerSpawn.Tick += new EventHandler(this.TimerSpawn_Tick);

            // timerTime
            this.timerTime.Interval = 1000;
            this.timerTime.Tick += new EventHandler(this.TimerTime_Tick);

            // Eventos del formulario
            this.FormClosing += new FormClosingEventHandler(this.Form1_FormClosing);
            this.KeyDown += new KeyEventHandler(this.Form1_KeyDown);

            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}

