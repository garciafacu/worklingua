using System.Collections.Generic;

namespace worklingua.Server.BE
{
    /// <summary>
    /// El resultado de emitir una alerta: la regla como quedó y a quiénes se
    /// notificó. La lista viaja para que la pantalla pueda mostrar a quién le
    /// llegó, no solo cuántos.
    /// </summary>
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
