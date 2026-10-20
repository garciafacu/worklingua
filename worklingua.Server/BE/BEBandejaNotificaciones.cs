using System.Collections.Generic;

namespace worklingua.Server.BE
{
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
