namespace worklingua.Server.BE
{
    /// <summary>
    /// Un campo que cambió entre dos versiones (CU-004-004, camino
    /// alternativo 3). El `Campo` es la clave con la que la pantalla busca su
    /// traducción; los valores ya vienen como texto listo para mostrar.
    /// </summary>
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
