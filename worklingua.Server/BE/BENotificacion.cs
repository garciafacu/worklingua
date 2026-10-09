namespace worklingua.Server.BE
{
    /// <summary>
    /// Una notificación en la bandeja del usuario. La emite una alerta
    /// preventiva (CU-001-010); el caso de uso habla de dispositivos móviles,
    /// que esta plataforma no tiene, así que llega a la campana.
    /// </summary>
    public class BENotificacion
    {
        #region Propiedades
        public int NotificacionId { get; set; }
        public int UsuarioId { get; set; }
        public string Titulo { get; set; }
        public string Mensaje { get; set; }
        public bool? Leida { get; set; }
        public DateTime? FechaEnvio { get; set; }
        #endregion

        public BENotificacion()
        {

        }

        public BENotificacion(
            int notificacionId, int usuarioId, string titulo, string mensaje, bool? leida, DateTime? fechaEnvio)
        {
            this.NotificacionId = notificacionId;
            this.UsuarioId = usuarioId;
            this.Titulo = titulo;
            this.Mensaje = mensaje;
            this.Leida = leida;
            this.FechaEnvio = fechaEnvio;
        }
    }
}
