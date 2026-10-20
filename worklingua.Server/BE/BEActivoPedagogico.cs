namespace worklingua.Server.BE
{
    public class BEActivoPedagogico
    {
        #region Propiedades
        public int ActivoPedagogicoId { get; set; }
        public int CursoId { get; set; }
        public int? ModuloId { get; set; }
        public string Modulo { get; set; }
        public string Nombre { get; set; }
        public string TipoContenido { get; set; }
        public string Descripcion { get; set; }
        public string UrlArchivo { get; set; }
        public bool? Activo { get; set; }
        public string Estado { get; set; }
        #endregion

        public BEActivoPedagogico()
        {

        }

        public BEActivoPedagogico(
            int activoPedagogicoId,
            int cursoId,
            int? moduloId,
            string modulo,
            string nombre,
            string tipoContenido,
            string descripcion,
            string urlArchivo,
            bool? activo,
            string estado)
        {
            this.ActivoPedagogicoId = activoPedagogicoId;
            this.CursoId = cursoId;
            this.ModuloId = moduloId;
            this.Modulo = modulo;
            this.Nombre = nombre;
            this.TipoContenido = tipoContenido;
            this.Descripcion = descripcion;
            this.UrlArchivo = urlArchivo;
            this.Activo = activo;
            this.Estado = estado;
        }
    }
}
