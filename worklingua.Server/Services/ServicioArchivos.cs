using System.IO;

namespace worklingua.Server.Services
{
    /// <summary>
    /// Guarda en disco los archivos que sube el Backoffice (CU-004-001).
    ///
    /// El caso de uso los aloja en la nube; acá se guardan bajo
    /// wwwroot/activos y la aplicación los sirve como archivos estáticos, que
    /// es el equivalente local sin infraestructura externa.
    ///
    /// El nombre en disco es un GUID: dos archivos con el mismo nombre
    /// original no se pisan, y el nombre que eligió el usuario no llega nunca
    /// al sistema de archivos.
    /// </summary>
    public static class ServicioArchivos
    {
        /// <summary>Ruta pública con la que se sirven los activos.</summary>
        public const string CarpetaActivos = "activos";

        /// <summary>
        /// Los archivos subidos viven FUERA de wwwroot, a propósito.
        ///
        /// wwwroot lo gobierna MapStaticAssets, que arma un manifiesto al
        /// compilar: todo lo que aparece ahí después queda fuera del manifiesto
        /// y la petición termina en 404 aunque el archivo exista. Además, el
        /// contenido que sube un usuario no es parte del build.
        /// </summary>
        public static string RutaFisicaActivos()
        {
            return Path.Combine(Configuracion.RutaContenido, "Archivos", CarpetaActivos);
        }

        /// <summary>
        /// Si el archivo de un activo sigue en disco. Lo usan el clonado y la
        /// restauración para saltear los que perdieron su archivo, en vez de
        /// heredar un enlace roto (CU-004-002 y CU-004-004).
        /// </summary>
        public static bool Existe(string urlArchivo)
        {
            if (string.IsNullOrWhiteSpace(urlArchivo))
            {
                return false;
            }

            string nombre = Path.GetFileName(urlArchivo);

            return nombre.Length > 0 && File.Exists(Path.Combine(RutaFisicaActivos(), nombre));
        }

        /// <summary>
        /// La crea si no existe. Hace falta al arrancar: el proveedor de
        /// archivos estáticos falla si apunta a una carpeta inexistente, y en
        /// una instalación nueva todavía no se subió ningún activo.
        /// </summary>
        public static void AsegurarCarpeta()
        {
            Directory.CreateDirectory(RutaFisicaActivos());
        }

        /// <summary>
        /// Escribe el archivo y devuelve la ruta pública con la que se lo
        /// referencia, por ejemplo /activos/0f3c....png
        /// </summary>
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
