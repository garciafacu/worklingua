namespace worklingua.Server.BE
{
    public class BEMensajeRespuesta
    {
        public string Mensaje { get; set; }

        public BEMensajeRespuesta()
        {
        }

        public BEMensajeRespuesta(string mensaje)
        {
            this.Mensaje = mensaje;
        }
    }
}
