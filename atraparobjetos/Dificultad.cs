using System;
using System.Collections.Generic;
using System.Linq;

namespace atraparobjetos
{
    /// <summary>
    /// Clase gestora de niveles y parámetros de dificultad.
    /// </summary>
    public static class Dificultad
    {
        private class Nivel
        {
            public string Nombre { get; }
            public List<string> ObjetosPermitidos { get; }
            public int VelocidadInicial { get; }   // píxeles por tick
            public int SpawnRate { get; }         // ms entre spawn
            public int LimiteMalosAtrapados { get; }
            public int LimiteBuenosDejados { get; }
            public int ObjetivoPuntos { get; }

            public Nivel(string nombre,
                         IEnumerable<string> objetosPermitidos,
                         int velocidadInicial,
                         int spawnRate,
                         int limiteMalosAtrapados,
                         int limiteBuenosDejados,
                         int objetivoPuntos)
            {
                Nombre = nombre;
                ObjetosPermitidos = new List<string>((objetosPermitidos ?? Enumerable.Empty<string>()).Select(s => s?.Trim().ToLowerInvariant()));
                VelocidadInicial = velocidadInicial;
                SpawnRate = spawnRate;
                LimiteMalosAtrapados = limiteMalosAtrapados;
                LimiteBuenosDejados = limiteBuenosDejados;
                ObjetivoPuntos = objetivoPuntos;
            }
        }

        private static readonly Nivel facil;
        private static readonly Nivel medio;
        private static readonly Nivel dificil;
        private static Nivel actual;

        static Dificultad()
        {
            var baseObjects = new[]
            {
                "pastel_rosa",
                "pastel_chocolate",
                "cupcake",
                "dona_rosada",
                "galleta_redonda",
                "pastel_malo"
            };

            facil = new Nivel(
                "Fácil",
                new[] { "pastel_rosa", "pastel_chocolate", "dona_rosada", "pastel_malo" },
                velocidadInicial: 3,
                spawnRate: 1000,
                limiteMalosAtrapados: 5,
                limiteBuenosDejados: 6,
                objetivoPuntos: 33
            );

            medio = new Nivel(
                "Medio",
                baseObjects,
                velocidadInicial: 4,
                spawnRate: 800,
                limiteMalosAtrapados: 4,
                limiteBuenosDejados: 4,
                objetivoPuntos: 33
            );

            var dificilObjects = new List<string>(baseObjects) { "cupcake_dorado", "pastel_quemado" };

            dificil = new Nivel(
                "Difícil",
                dificilObjects,
                velocidadInicial: 5,
                spawnRate: 600,
                limiteMalosAtrapados: 2,
                limiteBuenosDejados: 3,
                objetivoPuntos: 33
            );

            actual = facil;
        }

        // ------------------ API pública ------------------

        public static string NivelActualNombre => actual?.Nombre ?? "Fácil";

        public static void SetFacil() => actual = facil;
        public static void SetMedio() => actual = medio;
        public static void SetDificil() => actual = dificil;

        public static int VelocidadInicialActual => actual?.VelocidadInicial ?? 3;
        public static int SpawnRateActual => actual?.SpawnRate ?? 1000;
        public static int LimiteMalosAtrapadosActual => actual?.LimiteMalosAtrapados ?? int.MaxValue;
        public static int LimiteBuenosDejadosActual => actual?.LimiteBuenosDejados ?? int.MaxValue;
        public static int ObjetivoActual => actual?.ObjetivoPuntos ?? 33;
        public static int TiempoActual => 60; // tiempo base fijo si quieres

        public static List<string> ObtenerObjetosPermitidos() => actual?.ObjetosPermitidos.Select(NormalizarNombre).ToList() ?? new List<string>();

        public static bool ChecarVictoria(int puntos) => puntos >= ObjetivoActual;

        public static bool ChecarDerrota(int malosAtrapados, int buenosDejados)
        {
            return malosAtrapados >= LimiteMalosAtrapadosActual || buenosDejados >= LimiteBuenosDejadosActual;
        }

        public static int ObtenerPuntosPorObjeto(string objeto)
        {
            if (string.IsNullOrWhiteSpace(objeto)) return 0;
            var key = NormalizarNombre(objeto);
            if (key == "cupcake_dorado") return 5;
            if (key == "pastel_quemado") return -5;
            if (ObtenerObjetosPermitidos().Contains(key)) return 1;
            return 0;
        }

        public static bool EsObjetoPermitido(string objeto)
        {
            if (string.IsNullOrWhiteSpace(objeto)) return false;
            var key = NormalizarNombre(objeto);
            return ObtenerObjetosPermitidos().Contains(key);
        }

        private static string NormalizarNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return string.Empty;
            var n = nombre.Trim().ToLowerInvariant();
            if (n == "dona") return "dona_rosada";
            return n;
        }
    }
}
