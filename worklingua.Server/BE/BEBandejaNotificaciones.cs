using System.Collections.Generic;

namespace worklingua.Server.BE
{
    /// <summary>
    /// Lo que necesita la campana en una sola llamada: el contador que se
    /// pinta siempre y las últimas notificaciones que se muestran al abrirla.
    /// </summary>
    public class BEBandejaNotificaciones
    {
        #region Propiedades
        public int NoLeidas { get; set; }
        public List<BENotificacion> Notificaciones { get; set; }
        #endregion

        public BEBandejaNotificaciones()
        {
            Notificaciones = new List<BENotificacion>();
        }
    }
}
