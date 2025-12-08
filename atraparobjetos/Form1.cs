using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace atraparobjetos
{
    public partial class Form1 : Form
    {
        private readonly Acciones acciones;
        private readonly Random rnd = new Random();

        // Lista dinámica de PictureBox que caen
        private readonly List<PictureBox> fallingObjects = new List<PictureBox>();

        // Nombres de recursos disponibles para spawn (se llenan desde Dificultad y Resources)
        private readonly List<string> availableResourceNames = new List<string>();

        // Botones del menú de selección de dificultad
        private Button btnFacil;
        private Button btnMedio;
        private Button btnDificil;

        // Labels de estado que muestran malos atrapados y buenos dejados
        private Label lblMalosAtrapados;
        private Label lblBuenosDejados;

        // Contadores y estado del juego
        private int score;
        private int timeRemaining;
        private int targetScore;
        private int fallSpeed;

        // Contadores para condiciones de derrota
        private int malosAtrapados;
        private int buenosDejados;

        // Tamaño visual de cada objeto
        private const int ObjectSize = 50;

        public Form1()
        {
            acciones = new Acciones();
            InitializeComponent();

            // Doble buffer para suavizar repintado
            this.DoubleBuffered = true;
            panelGame.Paint += PanelGame_Paint;
            typeof(Panel).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(panelGame, true, null);

            // Crear y configurar los labels de estado (siempre encima del panel)
            CreateStatusLabels();

            // Cargar imágenes permitidas según el nivel activo en Dificultad
            LoadSpawnImagesFromDificultad();

            // Crear y mostrar el menú de selección de dificultad al iniciar
            CreateDifficultyMenu();
            ShowDifficultyMenu();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CenterCatcher();
            PositionStatusLabels();
        }

        private void CenterCatcher()
        {
            pbCatcher.Location = new Point(
                (panelGame.Width - pbCatcher.Width) / 2,
                panelGame.Height - 30
            );
        }

        // ------------------- STATUS LABELS -------------------
        // Crear los labels que muestran malos atrapados y buenos dejados
        private void CreateStatusLabels()
        {
            // Label malos atrapados (rojo)
            lblMalosAtrapados = new Label
            {
                Name = "lblMalosAtrapados",
                Text = "Malos atrapados: 0",
                AutoSize = false,
                Size = new Size(160, 22),
                BackColor = Color.Transparent,
                ForeColor = Color.DarkRed,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            // Label buenos dejados (verde)
            lblBuenosDejados = new Label
            {
                Name = "lblBuenosDejados",
                Text = "Buenos dejados: 0",
                AutoSize = false,
                Size = new Size(160, 22),
                BackColor = Color.Transparent,
                ForeColor = Color.DarkGreen,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            // Añadir al panel de juego y traer al frente (siempre encima de objetos)
            panelGame.Controls.Add(lblMalosAtrapados);
            panelGame.Controls.Add(lblBuenosDejados);
            lblMalosAtrapados.BringToFront();
            lblBuenosDejados.BringToFront();
        }

        // Posicionar los labels relativo al tamaño del panel
        private void PositionStatusLabels()
        {
            if (lblMalosAtrapados == null || lblBuenosDejados == null) return;

            // margen interior
            const int margin = 8;

            // posición arriba izquierda
            lblMalosAtrapados.Left = margin;
            lblMalosAtrapados.Top = margin;

            // posición arriba derecha
            lblBuenosDejados.Left = Math.Max(0, panelGame.ClientSize.Width - lblBuenosDejados.Width - margin);
            lblBuenosDejados.Top = margin;

            // Asegurar que estén por encima
            lblMalosAtrapados.BringToFront();
            lblBuenosDejados.BringToFront();
        }

        // ------------------- CARGA DE IMÁGENES SEGÚN DIFICULTAD -------------------
        // Lee la lista de objetos permitidos desde Dificultad y comprueba si existen como recursos.
        private void LoadSpawnImagesFromDificultad()
        {
            availableResourceNames.Clear();

            // Obtener lista de objetos permitidos por el nivel actual (normalizados)
            var objetos = Dificultad.ObtenerObjetosPermitidos();

            foreach (var name in objetos)
            {
                if (string.IsNullOrWhiteSpace(name)) continue;

                // Normalizar nombre para búsqueda de propiedad en Resources
                var key = name.Trim();

                // Buscar propiedad en Properties.Resources
                var prop = typeof(Properties.Resources).GetProperty(key, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (prop != null)
                {
                    var img = prop.GetValue(null) as Image;
                    if (img != null)
                    {
                        availableResourceNames.Add(key);
                        continue;
                    }
                }

                // Intentar con mapeos frecuentes (por ejemplo "dona" -> "dona_rosada")
                if (string.Equals(key, "dona", StringComparison.OrdinalIgnoreCase))
                {
                    var alt = "dona_rosada";
                    var propAlt = typeof(Properties.Resources).GetProperty(alt, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    if (propAlt != null && propAlt.GetValue(null) is Image)
                    {
                        availableResourceNames.Add(alt);
                        continue;
                    }
                }

                // Si no existe en Resources, añadimos el nombre para que create use fallback placeholder
                availableResourceNames.Add(key);
            }

            // Si por alguna razón la lista queda vacía, añadir placeholders genéricos
            if (availableResourceNames.Count == 0)
            {
                availableResourceNames.AddRange(new[] { "fallback1", "fallback2", "fallback3" });
            }
        }

        // ------------------- INICIO / FIN DE PARTIDA -------------------

        // Inicia la partida leyendo parámetros desde Dificultad
        private void StartGame()
        {
            ClearBalls();

            // Reiniciar contadores
            score = 0;
            malosAtrapados = 0;
            buenosDejados = 0;

            // Leer parámetros desde la clase Dificultad
            timeRemaining = Dificultad.TiempoActual;
            targetScore = Dificultad.ObjetivoActual;
            fallSpeed = Dificultad.VelocidadInicialActual;

            // Actualizar UI inicial
            lblScore.Text = $"Puntos: {score}";
            lblTime.Text = $"Tiempo: {timeRemaining}";
            lblResult.Text = "";

            // Actualizar status labels
            UpdateStatusLabels();

            pbCatcher.Left = (panelGame.Width - pbCatcher.Width) / 2;

            // timerGame fijo (actualiza movimiento con fallSpeed)
            if (timerGame != null)
                timerGame.Interval = 20; // tick cada 20ms para movimiento suave

            if (timerSpawn != null)
                timerSpawn.Interval = Math.Max(1, Dificultad.SpawnRateActual);

            // Sincronizar lista de recursos con nivel
            LoadSpawnImagesFromDificultad();

            timerGame?.Start();
            timerSpawn?.Start();
            timerTime?.Start();
        }

        // Finaliza la partida y muestra resultado
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
                MessageBox.Show($"Has perdido. Puntos: {score}.", "Perdiste", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            // Volver a pedir nivel
            CreateDifficultyMenu();
            ShowDifficultyMenu();
        }

        private void ClearBalls()
        {
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

        // ------------------- CREAR Y DIBUJAR OBJETOS -------------------

        // Crea un PictureBox con la imagen correspondiente (o fallback) y lo agrega al panel.
        private void CreateFallingObject()
        {
            // Elegir nombre de recurso disponible
            var name = availableResourceNames[rnd.Next(availableResourceNames.Count)];

            Image img = null;

            // Intentar cargar desde Properties.Resources por nombre exacto
            var prop = typeof(Properties.Resources).GetProperty(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop != null)
            {
                img = prop.GetValue(null) as Image;
            }

            // Si nombre es "dona", intentar "dona_rosada"
            if (img == null && string.Equals(name, "dona", StringComparison.OrdinalIgnoreCase))
            {
                var prop2 = typeof(Properties.Resources).GetProperty("dona_rosada", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (prop2 != null) img = prop2.GetValue(null) as Image;
            }

            // Si no hay imagen en recursos, usar placeholder generada
            if (img == null)
            {
                img = acciones.CreatePlaceholderImage(ObjectSize, name);
            }

            // Crear PictureBox dinámico; Tag almacena el nombre lógico del objeto (para puntuar)
            var pb = new PictureBox
            {
                Size = new Size(ObjectSize, ObjectSize),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = img,
                Left = acciones.GetRandomX(rnd, panelGame.Width, ObjectSize),
                Top = -ObjectSize,
                Tag = name, // nombre del objeto usado para puntuar / clasificar
                BackColor = Color.Transparent
            };

            panelGame.Controls.Add(pb);
            pb.BringToFront();
            // Asegurar labels siguen por encima
            lblMalosAtrapados.BringToFront();
            lblBuenosDejados.BringToFront();
            fallingObjects.Add(pb);
        }

        private void PanelGame_Paint(object sender, PaintEventArgs e)
        {
            // Mantener suavizado por si se añade dibujo adicional en el futuro.
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        }

        // ------------------- TIMERS: SPAWN / MOVIMIENTO / TIEMPO -------------------

        // Spawn timer: crea objetos según spawn rate configurado por Dificultad
        private void TimerSpawn_Tick(object sender, EventArgs e)
        {
            CreateFallingObject();
        }

        // Movimiento y colisiones; usa Dificultad para puntuar y chequear victoria/derrota
        private void TimerGame_Tick(object sender, EventArgs e)
        {
            // Mover todos los objetos y gestionar colisiones / conteos
            for (int i = fallingObjects.Count - 1; i >= 0; i--)
            {
                var pb = fallingObjects[i];
                pb.Top += fallSpeed;

                // Colisión con catcher
                if (pb.Bounds.IntersectsWith(pbCatcher.Bounds))
                {
                    var name = (pb.Tag as string) ?? string.Empty;
                    var puntos = Dificultad.ObtenerPuntosPorObjeto(name);

                    // Actualizar score usando Dificultad
                    score += puntos;
                    lblScore.Text = $"Puntos: {score}";

                    // Contar malos atrapados: si objeto es pastel_malo o puntos negativos
                    if (puntos < 0 || string.Equals(name, "pastel_malo", StringComparison.OrdinalIgnoreCase))
                    {
                        malosAtrapados++;
                    }

                    // Eliminar objeto
                    panelGame.Controls.Remove(pb);
                    pb.Image?.Dispose();
                    pb.Dispose();
                    fallingObjects.RemoveAt(i);

                    // Actualizar status labels en tiempo real
                    UpdateStatusLabels();

                    // Ajustar velocidad progresiva si procede (delegado a Acciones)
                    fallSpeed = acciones.ScaleFallSpeed(fallSpeed, score, increaseEvery: 3, maxSpeed: 20);

                    // Comprobar victoria / derrota consultando Dificultad
                    if (Dificultad.ChecarVictoria(score))
                    {
                        EndGame(true);
                        return;
                    }

                    if (Dificultad.ChecarDerrota(malosAtrapados, buenosDejados))
                    {
                        EndGame(false);
                        return;
                    }

                    continue;
                }

                // Si el objeto sale por abajo del panel: contar buenos dejados si corresponde
                if (pb.Top > panelGame.Height)
                {
                    var name = (pb.Tag as string) ?? string.Empty;
                    var puntos = Dificultad.ObtenerPuntosPorObjeto(name);

                    if (puntos > 0)
                    {
                        buenosDejados++;
                    }

                    panelGame.Controls.Remove(pb);
                    pb.Image?.Dispose();
                    pb.Dispose();
                    fallingObjects.RemoveAt(i);

                    // Actualizar status labels
                    UpdateStatusLabels();

                    // Comprobar derrota
                    if (Dificultad.ChecarDerrota(malosAtrapados, buenosDejados))
                    {
                        EndGame(false);
                        return;
                    }
                }
            }
        }

        // Reloj de tiempo restante
        private void TimerTime_Tick(object sender, EventArgs e)
        {
            timeRemaining--;
            lblTime.Text = $"Tiempo: {timeRemaining}";

            // Si tiempo llega a 0, chequear victoria por puntos y si no, derrota
            if (timeRemaining <= 0)
            {
                // Si alcanzó objetivo, victoria; si no, derrota
                if (Dificultad.ChecarVictoria(score))
                    EndGame(true);
                else
                    EndGame(false);
            }
        }

        // ------------------- MENÚ DE DIFICULTAD (BOTONES) -------------------

        private void CreateDifficultyMenu()
        {
            // Evitar crear duplicados
            if (btnFacil != null || btnMedio != null || btnDificil != null) return;

            btnFacil = new Button
            {
                Text = "Fácil",
                Size = new Size(120, 40),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.LightGreen,
                FlatStyle = FlatStyle.Flat
            };
            btnFacil.Click += BtnFacil_Click;

            btnMedio = new Button
            {
                Text = "Medio",
                Size = new Size(120, 40),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.Gold,
                FlatStyle = FlatStyle.Flat
            };
            btnMedio.Click += BtnMedio_Click;

            btnDificil = new Button
            {
                Text = "Difícil",
                Size = new Size(120, 40),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.IndianRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnDificil.Click += BtnDificil_Click;
        }

        private void ShowDifficultyMenu()
        {
            CreateDifficultyMenu();

            // Posicionar centrado
            var spacing = 16;
            var totalWidth = btnFacil.Width + btnMedio.Width + btnDificil.Width + spacing * 2;
            var startX = Math.Max(0, (panelGame.Width - totalWidth) / 2);
            var y = Math.Max(10, (panelGame.Height - btnFacil.Height) / 2 - 40);

            btnFacil.Left = startX;
            btnMedio.Left = startX + btnFacil.Width + spacing;
            btnDificil.Left = startX + btnFacil.Width + spacing + btnMedio.Width + spacing;

            btnFacil.Top = y;
            btnMedio.Top = y;
            btnDificil.Top = y;

            if (!panelGame.Controls.Contains(btnFacil)) panelGame.Controls.Add(btnFacil);
            if (!panelGame.Controls.Contains(btnMedio)) panelGame.Controls.Add(btnMedio);
            if (!panelGame.Controls.Contains(btnDificil)) panelGame.Controls.Add(btnDificil);

            btnFacil.BringToFront();
            btnMedio.BringToFront();
            btnDificil.BringToFront();

            // actualizar status labels posición y visibilidad
            PositionStatusLabels();
            lblMalosAtrapados.BringToFront();
            lblBuenosDejados.BringToFront();

            // Detener timers mientras se selecciona nivel
            timerGame?.Stop();
            timerSpawn?.Stop();
            timerTime?.Stop();
        }

        private void HideDifficultyMenu()
        {
            if (btnFacil != null && panelGame.Controls.Contains(btnFacil))
            {
                panelGame.Controls.Remove(btnFacil);
                btnFacil.Click -= BtnFacil_Click;
                btnFacil.Dispose();
                btnFacil = null;
            }

            if (btnMedio != null && panelGame.Controls.Contains(btnMedio))
            {
                panelGame.Controls.Remove(btnMedio);
                btnMedio.Click -= BtnMedio_Click;
                btnMedio.Dispose();
                btnMedio = null;
            }

            if (btnDificil != null && panelGame.Controls.Contains(btnDificil))
            {
                panelGame.Controls.Remove(btnDificil);
                btnDificil.Click -= BtnDificil_Click;
                btnDificil.Dispose();
                btnDificil = null;
            }
        }

        // Botones: llaman a Dificultad para seleccionar nivel y aplican los parámetros
        private void BtnFacil_Click(object sender, EventArgs e)
        {
            Dificultad.SetFacil();
            ApplyNivelFromDificultad();
            HideDifficultyMenu();
            StartGame();
        }

        private void BtnMedio_Click(object sender, EventArgs e)
        {
            Dificultad.SetMedio();
            ApplyNivelFromDificultad();
            HideDifficultyMenu();
            StartGame();
        }

        private void BtnDificil_Click(object sender, EventArgs e)
        {
            Dificultad.SetDificil();
            ApplyNivelFromDificultad();
            HideDifficultyMenu();
            StartGame();
        }

        // Aplica parámetros leídos desde Dificultad (sin contener lógica de niveles en Form1)
        private void ApplyNivelFromDificultad()
        {
            if (timerGame != null)
                timerGame.Interval = 20; // mantener tick suave

            if (timerSpawn != null)
                timerSpawn.Interval = Math.Max(1, Dificultad.SpawnRateActual);

            timeRemaining = Dificultad.TiempoActual;
            targetScore = Dificultad.ObjetivoActual;
            fallSpeed = Dificultad.VelocidadInicialActual;

            lblTime.Text = $"Tiempo: {timeRemaining}";
            lblScore.Text = $"Puntos: {score}";

            LoadSpawnImagesFromDificultad();
            PositionStatusLabels();
        }

        // ------------------- CONTROLES: REINICIAR -------------------

        // Reiniciar: mostrar el menú para elegir nivel otra vez
        private void BtnRestart_Click(object sender, EventArgs e)
        {
            timerGame?.Stop();
            timerSpawn?.Stop();
            timerTime?.Stop();
            ClearBalls();

            score = 0;
            malosAtrapados = 0;
            buenosDejados = 0;
            UpdateStatusLabels();

            CreateDifficultyMenu();
            ShowDifficultyMenu();
        }

        // ------------------- UTILS: STATUS UPDATE -------------------

        // Actualiza textos de los labels de estado en tiempo real
        private void UpdateStatusLabels()
        {
            if (lblMalosAtrapados != null)
                lblMalosAtrapados.Text = $"Malos atrapados: {malosAtrapados} / {Dificultad.LimiteMalosAtrapadosActual}";

            if (lblBuenosDejados != null)
                lblBuenosDejados.Text = $"Buenos dejados: {buenosDejados} / {Dificultad.LimiteBuenosDejadosActual}";

            lblMalosAtrapados.BringToFront();
            lblBuenosDejados.BringToFront();
        }

        // ------------------- INPUT Y MOVIMIENTO DEL CATCHER -------------------

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
