namespace worklingua.Server.BE
{
    public class BEModulo
    {
        #region Propiedades
        public int ModuloId { get; set; }
        public int CursoId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Contenido { get; set; }
        public int OrdenModulo { get; set; }
        public bool? Activo { get; set; }
        #endregion

        public BEModulo()
        {

        }

        public BEModulo(
            int moduloId,
            int cursoId,
            string nombre,
            string descripcion,
            string contenido,
            int ordenModulo,
            bool? activo)
        {
            this.ModuloId = moduloId;
            this.CursoId = cursoId;
            this.Nombre = nombre;
            this.Descripcion = descripcion;
            this.Contenido = contenido;
            this.OrdenModulo = ordenModulo;
            this.Activo = activo;
        }
    }
}
