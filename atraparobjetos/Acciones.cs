using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace atraparobjetos
{
    internal class Acciones
    {
        /// <summary>
        /// Devuelve una posición X aleatoria dentro del panel para una bola de tamaño dado.
        /// </summary>
        public int GetRandomX(Random rnd, int panelWidth, int ballSize)
        {
            if (panelWidth <= ballSize) return 0;
            return rnd.Next(0, panelWidth - ballSize);
        }

        /// <summary>
        /// Crea un bitmap circular (bola) del color y tamaño especificados.
        /// Mantengo pero no se usa como fallback principal.
        /// </summary>
        public Bitmap CreateBallBitmap(int size, Color color)
        {
            var bmp = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                using (var brush = new SolidBrush(color))
                {
                    g.FillEllipse(brush, 0, 0, size - 1, size - 1);
                }
                using (var pen = new Pen(Color.Black, 1))
                {
                    g.DrawEllipse(pen, 0, 0, size - 1, size - 1);
                }
            }
            return bmp;
        }

        /// <summary>
        /// Crea una imagen placeholder rectangular con texto (se usa si no hay recursos ni archivos).
        /// Evita que el fallback sean círculos visualmente iguales a la versión antigua.
        /// </summary>
        public Bitmap CreatePlaceholderImage(int size, string label)
        {
            var bmp = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.Clear(Color.White);
                var rect = new Rectangle(0, 0, size, size);
                using (var brush = new SolidBrush(Color.FromArgb(220, 180, 240)))
                    g.FillRoundedRectangle(brush, rect, 8);
                using (var pen = new Pen(Color.Gray, 1))
                    g.DrawRectangle(pen, 0, 0, size - 1, size - 1);

                var font = new Font("Segoe UI", 7, FontStyle.Bold);
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(label, font, Brushes.Black, rect, sf);
            }
            return bmp;
        }

        /// <summary>
        /// Carga una imagen desde un archivo y retorna un objeto Image (no mantiene el archivo bloqueado).
        /// </summary>
        public Image LoadImageFromFile(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath)) return null;
                if (!File.Exists(filePath)) return null;

                // Leer bytes y crear imagen desde memoria para no bloquear el archivo en disco
                var bytes = File.ReadAllBytes(filePath);
                using (var ms = new MemoryStream(bytes))
                {
                    return Image.FromStream(ms);
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Comprueba colisión entre dos controles (Bounds intersect).
        /// </summary>
        public bool IsCollision(Control a, Control b)
        {
            return a != null && b != null && a.Bounds.IntersectsWith(b.Bounds);
        }

        /// <summary>
        /// Ajusta la velocidad de caída en función de la puntuación.
        /// </summary>
        public int ScaleFallSpeed(int currentSpeed, int score, int increaseEvery = 3, int maxSpeed = 12)
        {
            if (increaseEvery <= 0) return currentSpeed;
            var increments = score / increaseEvery;
            var target = currentSpeed + increments;
            return Math.Min(target, maxSpeed);
        }

        /// <summary>
        /// Clamp utilitario.
        /// </summary>
        public int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }

    // Extensiones de dibujo pequeñas para rounded rectangle
    internal static class GraphicsExtensions
    {
        public static void FillRoundedRectangle(this Graphics g, Brush brush, Rectangle rect, int radius)
        {
            using (var path = RoundedRect(rect, radius))
            {
                g.FillPath(brush, path);
            }
        }

        private static GraphicsPath RoundedRect(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(rect.Left, rect.Top, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Top, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.Left, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
