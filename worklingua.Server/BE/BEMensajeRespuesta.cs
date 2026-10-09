namespace worklingua.Server.BE
{
    public class BEMensajeRespuesta
    {
        #region Propiedades
        public string Mensaje { get; set; }
        #endregion

        public BEMensajeRespuesta()
        {

        }

        public BEMensajeRespuesta(string mensaje)
        {
            this.Mensaje = mensaje;
        }
    }
}
