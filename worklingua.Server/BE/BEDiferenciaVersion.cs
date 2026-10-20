namespace worklingua.Server.BE
{
    public class BEDiferenciaVersion
    {
        #region Propiedades
        public string Campo { get; set; }
        public string Izquierda { get; set; }
        public string Derecha { get; set; }
        #endregion

        public BEDiferenciaVersion()
        {

        }

        public BEDiferenciaVersion(string campo, string izquierda, string derecha)
        {
            this.Campo = campo;
            this.Izquierda = izquierda;
            this.Derecha = derecha;
        }
    }
}
