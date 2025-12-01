using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace atraparobjetos
{
    public partial class Form1 : Form
    {
        private readonly Acciones acciones;
        private readonly Random rnd = new Random();
        private readonly List<PictureBox> balls = new List<PictureBox>();

        private int score;
        private int timeRemaining;
        private int targetScore;
        private int fallSpeed;

        public Form1()
        {
            acciones = new Acciones();
            InitializeComponent();
            StartGame();
        }

        private void StartGame()
        {
            ClearBalls();
            score = 0;
            timeRemaining = 30;
            targetScore = 10;
            fallSpeed = 4;

            lblScore.Text = $"Puntos: {score}";
            lblTime.Text = $"Tiempo: {timeRemaining}";
            lblResult.Text = "";

            // colocar catcher al centro (control declarado en el diseñador)
            pbCatcher.Left = (panelGame.Width - pbCatcher.Width) / 2;

            // arrancar timers definidos en el diseñador
            timerGame.Start();
            timerSpawn.Start();
            timerTime.Start();
        }

        private void EndGame(bool won)
        {
            timerGame.Stop();
            timerSpawn.Stop();
            timerTime.Stop();
            ClearBalls();

            if (won)
            {
                lblResult.ForeColor = Color.DarkGreen;
                lblResult.Text = "¡Has ganado!";
                MessageBox.Show($"¡Felicidades! Has conseguido {score} puntos.", "Ganaste", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                lblResult.ForeColor = Color.DarkRed;
                lblResult.Text = "Has perdido";
                MessageBox.Show($"Tiempo agotado. Has conseguido {score} puntos.", "Perdiste", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void ClearBalls()
        {
            foreach (var b in balls.ToList())
            {
                panelGame.Controls.Remove(b);
                b.Dispose();
            }
            balls.Clear();
        }

        // Este handler debe coincidir con lo que hay en Form1.Designer.cs
        private void TimerSpawn_Tick(object sender, EventArgs e)
        {
            const int size = 28;
            var x = acciones.GetRandomX(rnd, panelGame.Width, size);
            var color = Color.FromArgb(rnd.Next(50, 256), rnd.Next(50, 256), rnd.Next(50, 256));

            var pb = new PictureBox
            {
                Size = new Size(size, size),
                Location = new Point(x, -size),
                Tag = "ball",
                Image = acciones.CreateBallBitmap(size, color),
                SizeMode = PictureBoxSizeMode.CenterImage,
                BackColor = Color.Transparent
            };

            panelGame.Controls.Add(pb);
            pb.BringToFront();
            balls.Add(pb);
        }

        // Este handler debe coincidir con lo que hay en Form1.Designer.cs
        private void TimerGame_Tick(object sender, EventArgs e)
        {
            foreach (var ball in balls.ToList())
            {
                ball.Top += fallSpeed;

                if (acciones.IsCollision(ball, pbCatcher))
                {
                    score++;
                    lblScore.Text = $"Puntos: {score}";

                    panelGame.Controls.Remove(ball);
                    balls.Remove(ball);
                    ball.Dispose();

                    // ajustar dificultad con la ayuda de Acciones
                    fallSpeed = acciones.ScaleFallSpeed(fallSpeed, score, increaseEvery: 3, maxSpeed: 12);

                    if (score >= targetScore)
                    {
                        EndGame(true);
                        return;
                    }
                }
                else if (ball.Top > panelGame.Height)
                {
                    panelGame.Controls.Remove(ball);
                    balls.Remove(ball);
                    ball.Dispose();
                }
            }
        }

        // Este handler debe coincidir con lo que hay en Form1.Designer.cs
        private void TimerTime_Tick(object sender, EventArgs e)
        {
            timeRemaining--;
            lblTime.Text = $"Tiempo: {timeRemaining}";
            if (timeRemaining <= 0)
            {
                EndGame(false);
            }
        }

        private void BtnRestart_Click(object sender, EventArgs e)
        {
            StartGame();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            const int move = 20;
            if (e.KeyCode == Keys.Left)
            {
                pbCatcher.Left = acciones.Clamp(pbCatcher.Left - move, 0, panelGame.Width - pbCatcher.Width);
            }
            else if (e.KeyCode == Keys.Right)
            {
                pbCatcher.Left = acciones.Clamp(pbCatcher.Left + move, 0, panelGame.Width - pbCatcher.Width);
            }
            else if (e.KeyCode == Keys.Space)
            {
                if (timerGame.Enabled)
                {
                    timerGame.Stop();
                    timerSpawn.Stop();
                    timerTime.Stop();
                    lblResult.Text = "Pausa";
                    lblResult.ForeColor = Color.Orange;
                }
                else
                {
                    timerGame.Start();
                    timerSpawn.Start();
                    timerTime.Start();
                    lblResult.Text = "";
                }
            }
        }

        private void PanelGame_MouseMove(object sender, MouseEventArgs e)
        {
            var x = e.X - pbCatcher.Width / 2;
            x = acciones.Clamp(x, 0, panelGame.Width - pbCatcher.Width);
            pbCatcher.Left = x;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            timerGame?.Stop();
            timerSpawn?.Stop();
            timerTime?.Stop();
            ClearBalls();
            timerGame?.Dispose();
            timerSpawn?.Dispose();
            timerTime?.Dispose();
        }
    }
}
