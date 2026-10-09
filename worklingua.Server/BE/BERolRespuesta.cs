namespace worklingua.Server.BE
{
    public class BERolRespuesta
    {
        #region Propiedades
        public int RolId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        #endregion

        public BERolRespuesta()
        {

        }

        public BERolRespuesta(int rolId, string nombre, string descripcion)
        {
            this.RolId = rolId;
            this.Nombre = nombre;
            this.Descripcion = descripcion;
        }
    }
}
