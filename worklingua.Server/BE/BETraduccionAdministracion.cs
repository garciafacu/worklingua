namespace worklingua.Server.BE
{
    public class BETraduccionAdministracion
    {
        #region Propiedades
        public string Clave { get; set; }

        public string TextoEspanol { get; set; }

        public string Texto { get; set; }

        public bool Pendiente { get; set; }
        #endregion

        public BETraduccionAdministracion()
        {

        }

        public BETraduccionAdministracion(string clave, string textoEspanol, string texto, bool pendiente)
        {
            this.Clave = clave;
            this.TextoEspanol = textoEspanol;
            this.Texto = texto;
            this.Pendiente = pendiente;
        }
    }
}
