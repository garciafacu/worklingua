using System.Collections.Generic;

namespace worklingua.Server.BE
{
    public class BEEmisionRespuesta
    {
        #region Propiedades
        public BEAlertaRespuesta Alerta { get; set; }
        public int Destinatarios { get; set; }
        public List<BEEmpleadoInactivo> Empleados { get; set; }
        #endregion

        public BEEmisionRespuesta()
        {
            Empleados = new List<BEEmpleadoInactivo>();
        }
    }
}
