using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace atraparobjetos
{
    public partial class Form1 : Form
    {
        private readonly Acciones acciones;
        private readonly Random rnd = new Random();

        // ahora guardamos objetos de juego como PictureBox
        private readonly List<PictureBox> fallingBoxes = new List<PictureBox>();

        // opciones de spawn: (clave, imagen)
        private readonly List<Tuple<string, Image>> spawnOptions = new List<Tuple<string, Image>>();

        private int score;
        private int timeRemaining;
        private int targetScore;
        private int fallSpeed;

        // tamaño visual de cada objeto
        private const int ObjectSize = 48; // aumentar un poco para PNG legibles

        public Form1()
        {
            acciones = new Acciones();
            InitializeComponent();

            // Suscribir Paint y double buffering antes de arrancar timers
            panelGame.Paint += PanelGame_Paint;
            typeof(Panel).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(panelGame, true, null);

            // Cargar imágenes antes de iniciar el juego para evitar que el TimerSpawn se dispare sin imágenes
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
            // Eliminar todos los PictureBox creados dinámicamente
            foreach (var pb in fallingBoxes.ToArray())
            {
                if (panelGame.Controls.Contains(pb))
                {
                    panelGame.Controls.Remove(pb);
                }
                // No destruimos las imágenes compartidas en spawnOptions aquí
                pb.Image = null;
                pb.Dispose();
            }
            fallingBoxes.Clear();
            panelGame.Invalidate();
        }

        // Carga imágenes de spawn con su clave. Intenta Resources primero y luego archivos en output/Resources.
        private void LoadSpawnImages()
        {
            spawnOptions.Clear();

            var names = new[]
            {
                "pastel_rosa",
                "pastel_chocolate",
                "cupcake",
                "dona_rosada",
                "galleta_redonda",
                "pastel_malo"
            };

            // 1) Intentar cargar desde Properties.Resources por ResourceManager (más fiable que reflexión directa)
            foreach (var name in names)
            {
                try
                {
                    var obj = Properties.Resources.ResourceManager.GetObject(name);
                    if (obj is Image resImg)
                    {
                        spawnOptions.Add(Tuple.Create(name, (Image)resImg));
                    }
                }
                catch
                {
                    // Ignorar y continuar
                }
            }

            // 2) Si faltan, intentar cargar desde archivos en salida (Application.StartupPath) y subcarpetas
            var basePaths = new[]
            {
                Application.StartupPath,
                Path.Combine(Application.StartupPath, "Resources"),
                Path.Combine(Application.StartupPath, "images"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources")
            };

            foreach (var name in names)
            {
                if (spawnOptions.Any(t => string.Equals(t.Item1, name, StringComparison.OrdinalIgnoreCase)))
                    continue;

                foreach (var basePath in basePaths)
                {
                    var file = Path.Combine(basePath, name + ".png");
                    var img = acciones.LoadImageFromFile(file);
                    if (img != null)
                    {
                        spawnOptions.Add(Tuple.Create(name, img));
                        break;
                    }
                }
            }

            // 3) Si no se cargó ninguna imagen, crear placeholders (rectángulos con texto) en lugar de círculos.
            if (spawnOptions.Count == 0)
            {
                foreach (var name in names.Take(3))
                {
                    var bmp = acciones.CreatePlaceholderImage(ObjectSize, name);
                    spawnOptions.Add(Tuple.Create(name, (Image)bmp));
                }
            }
        }

        // Crear un PictureBox aleatorio arriba del panel
        private void SpawnPictureBox()
        {
            if (spawnOptions.Count == 0) return;

            var x = acciones.GetRandomX(rnd, panelGame.Width, ObjectSize);

            // elegir aleatoriamente una opción (clave + imagen)
            var opt = spawnOptions[rnd.Next(spawnOptions.Count)];
            var key = opt.Item1;
            var img = opt.Item2;

            var pb = new PictureBox
            {
                Width = ObjectSize,
                Height = ObjectSize,
                Left = x,
                Top = -ObjectSize,
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = img,
                Tag = string.Equals(key, "pastel_malo", StringComparison.OrdinalIgnoreCase) ? "bad" : "good",
                BackColor = Color.Transparent
            };

            // Añadir al panel y a la lista de seguimiento
            panelGame.Controls.Add(pb);
            // Asegurar que el catcher permanezca visible encima de las cajas
            pb.BringToFront();
            pbCatcher.BringToFront();

            fallingBoxes.Add(pb);
        }

        // Este handler debe coincidir con lo que hay en Form1.Designer.cs
        private void TimerSpawn_Tick(object sender, EventArgs e)
        {
            SpawnPictureBox();
        }

        // Dejar el Paint del panel para fondos u otros elementos. Ya no dibujamos las bolas manualmente.
        private void PanelGame_Paint(object sender, PaintEventArgs e)
        {
            // Mantener configuración de alta calidad para cualquier otro dibujo
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            // No dibujar las imágenes aquí: se usan PictureBox controls
        }

        // Este handler debe coincidir con lo que hay en Form1.Designer.cs
        private void TimerGame_Tick(object sender, EventArgs e)
        {
            // Avanzar todos los PictureBox
            for (int i = fallingBoxes.Count - 1; i >= 0; i--)
            {
                var pb = fallingBoxes[i];

                // Mover hacia abajo
                pb.Top += fallSpeed;

                // Colisión con el catcher (usar Bounds del PictureBox)
                if (pb.Bounds.IntersectsWith(pbCatcher.Bounds))
                {
                    var tag = pb.Tag as string;
                    if (tag == "bad")
                    {
                        score--;
                    }
                    else // "good"
                    {
                        score++;
                    }

                    lblScore.Text = $"Puntos: {score}";

                    // eliminar el objeto (no eliminamos la imagen compartida en spawnOptions)
                    if (panelGame.Controls.Contains(pb)) panelGame.Controls.Remove(pb);
                    pb.Image = null;
                    pb.Dispose();
                    fallingBoxes.RemoveAt(i);

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
                    if (panelGame.Controls.Contains(pb)) panelGame.Controls.Remove(pb);
                    pb.Image = null;
                    pb.Dispose();
                    fallingBoxes.RemoveAt(i);
                }
            }

            // Repintar por si cambia algo del panel
            panelGame.Invalidate();
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

            // No se disponen aquí las imágenes de spawnOptions porque pueden ser recursos compartidos.
        }
    }
}
