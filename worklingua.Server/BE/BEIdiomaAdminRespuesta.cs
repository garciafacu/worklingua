namespace worklingua.Server.BE
{
    public class BEIdiomaAdminRespuesta : BEIdiomaRespuesta
    {
        #region Propiedades
        public bool Activo { get; set; }
        #endregion

        public BEIdiomaAdminRespuesta()
        {

        }

        public BEIdiomaAdminRespuesta(bool activo)
        {
            this.Activo = activo;
        }
    }
}
