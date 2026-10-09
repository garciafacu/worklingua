namespace worklingua.Server.BE
{
    public class BERol
    {
        #region Propiedades
        public int RolId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool? Activo { get; set; }
        #endregion

        public BERol()
        {

        }

        public BERol(int rolId, string nombre, string descripcion, bool? activo)
        {
            this.RolId = rolId;
            this.Nombre = nombre;
            this.Descripcion = descripcion;
            this.Activo = activo;
        }
    }
}
