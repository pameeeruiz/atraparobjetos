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

        // timers adicionales (no tocar Designer.cs)
        private readonly Timer timerCountdown;
        private readonly Timer timerSlowEffect;

        private int score;
        private int timeRemaining;
        private int fallSpeed;

        // conteo de pasteles buenos perdidos
        private int missedGoodCount;

        // tamaño visual de cada objeto
        private const int ObjectSize = 48; // tamaño normal
        private const int MiniObjectSize = 32; // mini pastel

        // estado inicial para reinicios/ajustes
        private int initialCatcherWidth;
        private int initialSpawnInterval;

        // parámetros del modo difícil
        private const int InitialTime = 40; // 40 segundos
        private const int CatcherShrinkPer5Points = 5;
        private const int MinCatcherWidth = 30;

        // control extra para mostrar perdidos
        private readonly Label lblMissed;

        // control de efecto slow
        private bool slowActive;
        private int savedFallSpeed;

        // control para aumentos periódicos por tiempo
        private int lastSpeedIncreaseElapsed = 0; // en segundos

        public Form1()
        {
            acciones = new Acciones();

            InitializeComponent();

            // Activar double buffering del Form y del panel para reducir parpadeo
            this.DoubleBuffered = true;
            typeof(Panel).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(panelGame, true, null);

            // crear timers que no están en el diseñador
            timerCountdown = new Timer { Interval = 1000 }; // 1s countdown
            timerCountdown.Tick += TimerCountdown_Tick;
            timerSlowEffect = new Timer { Interval = 3000 }; // 3s slow duration
            timerSlowEffect.Tick += TimerSlowEffect_Tick;

            // Guardar estado inicial del catcher y spawn timer
            initialCatcherWidth = pbCatcher.Width;
            initialSpawnInterval = timerSpawn.Interval;

            // Ajustes visuales
            panelGame.Paint += PanelGame_Paint;

            // Ajustar texto del botón para indicar "Iniciar Juego"
            btnRestart.Text = "Iniciar Juego";

            // crear label de perdidos (no tocar Designer)
            lblMissed = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Location = new Point(lblTime.Right + 10, lblTime.Top),
                ForeColor = Color.DarkRed,
                Text = "Perdidos: 0"
            };
            this.Controls.Add(lblMissed);

            // Cargar imágenes antes de inciar el juego
            LoadSpawnImages();

            // No arrancar automáticamente: dejar que el usuario pulse Iniciar
            // Si prefieres arrancar al cargar, descomenta la siguiente línea:
            // StartGame();
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

        // Reiniciar TODO el estado del juego
        private void StartGame()
        {
            ClearBalls();

            score = 0;
            timeRemaining = InitialTime;
            fallSpeed = 6; // velocidad inicial rápida
            missedGoodCount = 0;
            slowActive = false;
            savedFallSpeed = fallSpeed;
            lastSpeedIncreaseElapsed = 0;

            // restaurar catcher y spawn interval
            pbCatcher.Width = initialCatcherWidth;
            CenterCatcher();
            timerSpawn.Interval = initialSpawnInterval;

            UpdateUiLabels();

            lblResult.Text = "";

            // arrancar timers
            timerGame.Start();
            timerSpawn.Start();
            timerCountdown.Start();
            timerSlowEffect.Stop(); // asegurar parado
        }

        private void EndGame(bool showMessage = true)
        {
            timerGame.Stop();
            timerSpawn.Stop();
            timerCountdown.Stop();
            timerSlowEffect.Stop();

            ClearBalls();

            if (showMessage)
            {
                lblResult.ForeColor = Color.DarkBlue;
                lblResult.Text = "Juego finalizado";
                MessageBox.Show($"Puntos finales: {score}\nPasteles perdidos: {missedGoodCount}", "Fin del juego", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ClearBalls()
        {
            foreach (var pb in fallingBoxes.ToArray())
            {
                if (panelGame.Controls.Contains(pb))
                {
                    panelGame.Controls.Remove(pb);
                }
                pb.Image = null;
                pb.Dispose();
            }
            fallingBoxes.Clear();
            panelGame.Invalidate();
        }

        private void UpdateUiLabels()
        {
            lblScore.Text = $"Puntos: {score}";
            lblTime.Text = $"Tiempo: {timeRemaining}";
            lblMissed.Text = $"Perdidos: {missedGoodCount}";
        }

        // Carga imágenes de spawn (no cambiase la estructura del proyecto)
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
                "pastel_malo",
                "power_slow" // imagen opcional para power-up
            };

            var rm = Properties.Resources.ResourceManager;

            foreach (var key in names)
            {
                Image img = null;
                var variants = GetNameVariants(key);

                foreach (var v in variants)
                {
                    try
                    {
                        var obj = rm.GetObject(v);
                        if (obj is Image resImg)
                        {
                            img = (Image)resImg;
                            break;
                        }
                    }
                    catch { }
                }

                if (img == null)
                {
                    var basePaths = new[]
                    {
                        Application.StartupPath,
                        Path.Combine(Application.StartupPath, "Resources"),
                        Path.Combine(Application.StartupPath, "images"),
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources")
                    };

                    foreach (var v in variants)
                    {
                        foreach (var basePath in basePaths)
                        {
                            var file = Path.Combine(basePath, v + ".png");
                            var loaded = acciones.LoadImageFromFile(file);
                            if (loaded != null)
                            {
                                img = loaded;
                                break;
                            }
                        }
                        if (img != null) break;
                    }
                }

                if (img != null)
                {
                    spawnOptions.Add(Tuple.Create(key, img));
                }
            }

            // si no hay nada, fallback
            if (spawnOptions.Count == 0)
            {
                var fallbackNames = new[] { "pastel_rosa", "pastel_chocolate", "cupcake" };
                foreach (var fn in fallbackNames)
                {
                    var bmp = acciones.CreatePlaceholderImage(ObjectSize, fn);
                    spawnOptions.Add(Tuple.Create(fn, (Image)bmp));
                }
            }
        }

        private static IEnumerable<string> GetNameVariants(string key)
        {
            var list = new List<string>();
            list.Add(key);
            list.Add(key.Replace('_', '.'));
            list.Add(key.Replace('_', '-'));
            list.Add(key.Replace("_", ""));
            var parts = key.Split(new[] { '_', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var pascal = string.Concat(parts.Select(p => char.ToUpperInvariant(p[0]) + (p.Length > 1 ? p.Substring(1) : "")));
            list.Add(pascal);
            if (pascal.Length > 0) list.Add(char.ToLowerInvariant(pascal[0]) + pascal.Substring(1));
            list.Add(string.Join("", parts).ToLowerInvariant());
            list.Add(key.ToUpperInvariant());
            return list.Distinct(StringComparer.OrdinalIgnoreCase);
        }

        // Crear un PictureBox aleatorio arriba del panel (incluye mini pasteles y power-up)
        private void SpawnPictureBox()
        {
            if (spawnOptions.Count == 0) return;

            var x = acciones.GetRandomX(rnd, panelGame.Width, ObjectSize);

            // decidir tipo: power-up raro, mini o normal
            double powerChance = 0.06; // 6% power-up
            bool spawnPower = rnd.NextDouble() < powerChance;
            bool makeMini = !spawnPower && rnd.NextDouble() < 0.25; // 25% mini si no es power

            Tuple<string, Image> opt;
            if (spawnPower)
            {
                // buscar power_slow en spawnOptions si existe, si no, fallback a pastel
                opt = spawnOptions.FirstOrDefault(t => t.Item1.IndexOf("power", StringComparison.OrdinalIgnoreCase) >= 0)
                      ?? spawnOptions[rnd.Next(spawnOptions.Count)];
            }
            else
            {
                opt = spawnOptions[rnd.Next(spawnOptions.Count)];
            }

            var key = opt.Item1;
            var img = opt.Item2;

            var size = makeMini ? MiniObjectSize : ObjectSize;
            var left = acciones.Clamp(x, 0, Math.Max(0, panelGame.Width - size));

            var pb = new PictureBox
            {
                Width = size,
                Height = size,
                Left = left,
                Top = -size,
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = img,
                BackColor = Color.Transparent
            };

            // tags:
            if (spawnPower && key.IndexOf("power", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                pb.Tag = "power_slow";
            }
            else if (makeMini)
            {
                pb.Tag = string.Equals(key, "pastel_malo", StringComparison.OrdinalIgnoreCase) ? "mini_bad" : "mini_good";
            }
            else
            {
                pb.Tag = string.Equals(key, "pastel_malo", StringComparison.OrdinalIgnoreCase) ? "bad" : "good";
            }

            panelGame.Controls.Add(pb);
            pb.BringToFront();
            pbCatcher.BringToFront();

            fallingBoxes.Add(pb);
        }

        // movimiento y comprobación de colisiones
        private void TimerGame_Tick(object sender, EventArgs e)
        {
            for (int i = fallingBoxes.Count - 1; i >= 0; i--)
            {
                var pb = fallingBoxes[i];
                pb.Top += fallSpeed;

                // colisión con la bandeja
                if (pb.Bounds.IntersectsWith(pbCatcher.Bounds))
                {
                    var tag = pb.Tag as string;
                    if (tag != null && tag.Contains("power"))
                    {
                        // activar slow time por 3s
                        ActivateSlowTime();
                    }
                    else if (tag != null && tag.Contains("bad"))
                    {
                        // pastel malo: -3 puntos
                        score = Math.Max(0, score - 3);
                    }
                    else if (tag != null && tag.Contains("mini"))
                    {
                        // mini bueno: +2
                        score += 2;
                    }
                    else
                    {
                        // normal bueno +1
                        score += 1;
                    }

                    ApplyCatcherShrink();
                    // no usar ScaleFallSpeed automático aquí porque controlamos por tiempo; pero mantener pequeña adaptación por puntos:
                    fallSpeed = acciones.ScaleFallSpeed(fallSpeed, score, increaseEvery: 5, maxSpeed: 30);

                    UpdateUiLabels();

                    if (panelGame.Controls.Contains(pb)) panelGame.Controls.Remove(pb);
                    pb.Image = null;
                    pb.Dispose();
                    fallingBoxes.RemoveAt(i);

                    continue;
                }

                // si sale por debajo sin atrapar: si es pastel bueno incrementa missed
                if (pb.Top > panelGame.Height)
                {
                    var tag = pb.Tag as string;
                    if (tag != null && (tag.Contains("good") || tag.Contains("mini")))
                    {
                        missedGoodCount++;
                        UpdateUiLabels();
                        if (missedGoodCount >= 10)
                        {
                            lblResult.ForeColor = Color.DarkRed;
                            lblResult.Text = "Demasiados perdidos";
                            EndGame(true);
                            return;
                        }
                    }

                    if (panelGame.Controls.Contains(pb)) panelGame.Controls.Remove(pb);
                    pb.Image = null;
                    pb.Dispose();
                    fallingBoxes.RemoveAt(i);
                }
            }

            panelGame.Invalidate();
        }

        // spawn timer (desde Designer)
        private void TimerSpawn_Tick(object sender, EventArgs e)
        {
            SpawnPictureBox();
        }

        // timerCountdown maneja el countdown de 1s y subidas de dificultad cada 10s
        private void TimerCountdown_Tick(object sender, EventArgs e)
        {
            if (timeRemaining <= 0) return;

            timeRemaining--;
            UpdateUiLabels();

            int elapsed = InitialTime - timeRemaining;

            // cada 10 segundos aumentar velocidad de caída
            if (elapsed > 0 && elapsed % 10 == 0 && elapsed != lastSpeedIncreaseElapsed)
            {
                lastSpeedIncreaseElapsed = elapsed;
                fallSpeed = Math.Min(30, fallSpeed + 2);
            }

            if (timeRemaining <= 0)
            {
                lblResult.ForeColor = Color.DarkBlue;
                lblResult.Text = "Tiempo agotado";
                EndGame(true);
            }
        }

        // activar efecto slow (3s)
        private void ActivateSlowTime()
        {
            if (slowActive) return;
            slowActive = true;
            savedFallSpeed = fallSpeed;
            // reducir la velocidad a la mitad (mínimo 1)
            fallSpeed = Math.Max(1, fallSpeed / 2);
            timerSlowEffect.Stop();
            timerSlowEffect.Start();
        }

        // fin del efecto slow
        private void TimerSlowEffect_Tick(object sender, EventArgs e)
        {
            timerSlowEffect.Stop();
            slowActive = false;
            // restaurar velocidad (si la guardada es menor que actual, dejar la más alta)
            fallSpeed = Math.Max(savedFallSpeed, fallSpeed);
        }

        private void PanelGame_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
        }

        // botón iniciar (designer wired)
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
                    timerCountdown.Stop();
                    timerSlowEffect.Stop();
                    lblResult.Text = "Pausa";
                    lblResult.ForeColor = Color.Orange;
                }
                else
                {
                    timerGame.Start();
                    timerSpawn.Start();
                    timerCountdown.Start();
                    if (slowActive) timerSlowEffect.Start();
                    lblResult.Text = "";
                }
            }
        }

        private void ApplyCatcherShrink()
        {
            var reductions = score / 5;
            var newWidth = initialCatcherWidth - reductions * CatcherShrinkPer5Points;
            newWidth = Math.Max(MinCatcherWidth, newWidth);
            pbCatcher.Width = newWidth;
            pbCatcher.Left = acciones.Clamp(pbCatcher.Left, 0, panelGame.Width - pbCatcher.Width);
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
            timerCountdown?.Stop();
            timerSlowEffect?.Stop();
            ClearBalls();
            timerGame?.Dispose();
            timerSpawn?.Dispose();
            timerCountdown?.Dispose();
            timerSlowEffect?.Dispose();
        }

        // Añadir este método dentro de la clase `Form1` (p.ej. cerca de otros handlers de timers)
        private void TimerTime_Tick(object sender, EventArgs e)
        {
            // Compatibilidad con el evento conectado desde el diseñador:
            // delega al handler del countdown que ya implementaste.
            TimerCountdown_Tick(sender, e);
        }
    }
}
