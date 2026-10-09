namespace worklingua.Server.Services
{
    public static class ServicioLog
    {
        public static void Informacion(string mensaje)
        {
            Escribir("INFO", mensaje);
        }

        public static void Advertencia(string mensaje)
        {
            Escribir("WARN", mensaje);
        }

        public static void Error(string mensaje)
        {
            Escribir("ERROR", mensaje);
        }

        public static void Error(string mensaje, Exception excepcion)
        {
            Escribir("ERROR", mensaje + " " + excepcion);
        }

        private static void Escribir(string nivel, string mensaje)
        {
            Console.WriteLine(
                string.Format("{0:dd/MM/yyyy HH:mm:ss} [{1}] {2}", DateTime.Now, nivel, mensaje));
        }
    }
}
