namespace worklingua.Server.BE
{
    public class BEVersionContenido
    {
        #region Propiedades
        public int VersionContenidoId { get; set; }
        public int CursoId { get; set; }
        public string NumeroVersion { get; set; }
        public DateTime? FechaVersion { get; set; }
        public string Observaciones { get; set; }
        public int? UsuarioId { get; set; }
        public string Autor { get; set; }
        public string ContenidoXml { get; set; }
        public int LargoXml { get; set; }
        #endregion

        public BEVersionContenido()
        {

        }
    }
}
