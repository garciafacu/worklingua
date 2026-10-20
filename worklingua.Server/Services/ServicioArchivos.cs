using System.IO;

namespace worklingua.Server.Services
{
    public static class ServicioArchivos
    {
        public const string CarpetaActivos = "activos";

        public static string RutaFisicaActivos()
        {
            return Path.Combine(Configuracion.RutaContenido, "Archivos", CarpetaActivos);
        }

        public static bool Existe(string urlArchivo)
        {
            if (string.IsNullOrWhiteSpace(urlArchivo))
            {
                return false;
            }

            string nombre = Path.GetFileName(urlArchivo);

            return nombre.Length > 0 && File.Exists(Path.Combine(RutaFisicaActivos(), nombre));
        }

        public static void AsegurarCarpeta()
        {
            Directory.CreateDirectory(RutaFisicaActivos());
        }

        public static string Guardar(byte[] contenido, string extension)
        {
            AsegurarCarpeta();

            string carpeta = RutaFisicaActivos();

            string nombre = Guid.NewGuid().ToString("N") + extension;

            File.WriteAllBytes(Path.Combine(carpeta, nombre), contenido);

            return "/" + CarpetaActivos + "/" + nombre;
        }
    }
}
