using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        /// </summary>
        public Bitmap CreateBallBitmap(int size, Color color)
        {
            var bmp = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
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
}
