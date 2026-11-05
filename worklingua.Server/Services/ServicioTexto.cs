namespace worklingua.Server.Services
{
    public static class ServicioTexto
    {
        public static string Normalizar(string texto)
        {
            return string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
        }
    }
}
