using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace atraparobjetos
{
    public partial class Form1 : Form
    {
        private readonly Acciones acciones;
        private readonly Random rnd = new Random();

        // ahora guardamos objetos de juego como PictureBox dinámicos
        private readonly List<PictureBox> fallingObjects = new List<PictureBox>();

        // nombres esperados en Resources (se filtran en LoadSpawnImages)
        private readonly string[] expectedResourceNames = new[]
        {
            "pastel_rosa",
            "pastel_chocolate",
            "cupcake",
            "dona_rosada",
            "galleta_redonda",
            "pastel_malo"
        };

        private readonly List<string> availableResourceNames = new List<string>();

        private int score;
        private int timeRemaining;
        private int targetScore;
        private int fallSpeed;

        // tamaño visual de cada objeto (usar 50x50 según requisito)
        private const int ObjectSize = 50;

        public Form1()
        {
            acciones = new Acciones();
            InitializeComponent();
            this.DoubleBuffered = true;

            // Suscribir Paint y double buffering antes de arrancar timers
            panelGame.Paint += PanelGame_Paint;
            typeof(Panel).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(panelGame, true, null);

            // Cargar lista de nombres de Resources disponibles
            LoadSpawnImages();

            StartGame();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CenterCatcher();
        }

        private void CenterCatcher()
        {
            pbCatcher.Location = new Point(
                (panelGame.Width - pbCatcher.Width) / 2,
                panelGame.Height - 30
            );
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
            // No disponer imágenes compartidas; eliminar PictureBoxes dinámicos del panel
            for (int i = fallingObjects.Count - 1; i >= 0; i--)
            {
                var pb = fallingObjects[i];
                panelGame.Controls.Remove(pb);
                pb.Image?.Dispose();
                pb.Dispose();
            }

            fallingObjects.Clear();
            panelGame.Invalidate();
        }

        // Carga nombres de imágenes disponibles en Properties.Resources (no carga archivos externos)
        private void LoadSpawnImages()
        {
            availableResourceNames.Clear();

            foreach (var name in expectedResourceNames)
            {
                var prop = typeof(Properties.Resources).GetProperty(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (prop != null)
                {
                    var img = prop.GetValue(null) as Image;
                    if (img != null)
                    {
                        availableResourceNames.Add(name);
                    }
                }
            }

            // Si no hay imágenes en Resources, dejar al menos algunos "nombres" para fallback.
            if (availableResourceNames.Count == 0)
            {
                // no se intentan cargar archivos externos: seguiremos con fallback dinámico en CreateFallingObject
                availableResourceNames.AddRange(new[] { "fallback1", "fallback2", "fallback3" });
            }
        }

        // Crea un PictureBox que cae, de 50x50, con imagen desde Resources si existe.
        private void CreateFallingObject()
        {
            // elegir nombre disponible
            var name = availableResourceNames[rnd.Next(availableResourceNames.Count)];

            Image img = null;
            string tag = "good";

            // intentar tomar de Resources
            var prop = typeof(Properties.Resources).GetProperty(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop != null)
            {
                img = prop.GetValue(null) as Image;
            }

            // si nombre corresponde al malo conocido, marcar tag
            if (string.Equals(name, "pastel_malo", StringComparison.OrdinalIgnoreCase))
            {
                tag = "bad";
            }

            // fallback: si no hay imagen en Resources, crear bitmap simple
            if (img == null)
            {
                img = acciones.CreateBallBitmap(ObjectSize, Color.FromArgb(rnd.Next(50, 256), rnd.Next(50, 256), rnd.Next(50, 256)));
            }

            // generar PictureBox dinámico
            var pb = new PictureBox
            {
                Size = new Size(ObjectSize, ObjectSize),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = img,
                Left = acciones.GetRandomX(rnd, panelGame.Width, ObjectSize),
                Top = -ObjectSize,
                Tag = tag,
                BackColor = Color.Transparent
            };

            // Añadir al panel y a la lista de objetos que caen
            panelGame.Controls.Add(pb);
            // Asegurar que el nuevo PictureBox se pinte sobre el panel
            pb.BringToFront();
            fallingObjects.Add(pb);
        }

        // Este handler debe coincidir con lo que hay en Form1.Designer.cs
        private void TimerSpawn_Tick(object sender, EventArgs e)
        {
            CreateFallingObject();
        }

        // El Paint ahora ya no dibuja objetos manualmente, pero conserva mejoras gráficas
        private void PanelGame_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            // ya no dibujamos objetos manualmente; los PictureBox los muestran
        }

        // Este handler debe coincidir con lo que hay en Form1.Designer.cs
        private void TimerGame_Tick(object sender, EventArgs e)
        {
            // Mover todos los PictureBox que caen
            for (int i = fallingObjects.Count - 1; i >= 0; i--)
            {
                var pb = fallingObjects[i];
                pb.Top += fallSpeed;

                // Colisión con el catcher (usar Bounds del PictureBox)
                if (pb.Bounds.IntersectsWith(pbCatcher.Bounds))
                {
                    if ((pb.Tag as string) == "bad")
                    {
                        score--;
                    }
                    else // "good"
                    {
                        score++;
                    }

                    lblScore.Text = $"Puntos: {score}";

                    // eliminar el objeto visual
                    panelGame.Controls.Remove(pb);
                    pb.Image?.Dispose();
                    pb.Dispose();
                    fallingObjects.RemoveAt(i);

                    // ajustar dificultad con la ayuda de Acciones
                    fallSpeed = acciones.ScaleFallSpeed(fallSpeed, score, increaseEvery: 3, maxSpeed: 12);

                    if (score >= targetScore)
                    {
                        EndGame(true);
                        return;
                    }

                    continue;
                }

                // Si sale del panel, eliminar
                if (pb.Top > panelGame.Height)
                {
                    panelGame.Controls.Remove(pb);
                    pb.Image?.Dispose();
                    pb.Dispose();
                    fallingObjects.RemoveAt(i);
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
