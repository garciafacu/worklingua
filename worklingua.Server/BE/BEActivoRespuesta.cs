namespace worklingua.Server.BE
{
    public class BEActivoRespuesta
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
        public bool Activo { get; set; }
        public string Estado { get; set; }
        #endregion

        public BEActivoRespuesta()
        {

        }
    }
}
