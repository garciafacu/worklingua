using System.Collections.Generic;

namespace worklingua.Server.BE
{
    /// <summary>
    /// El resultado del padrón: las cifras y el detalle fila por fila.
    ///
    /// Viaja con HTTP 200 aunque haya filas rechazadas. El middleware de errores
    /// convierte una excepción en una respuesta, y acá hacen falta N errores en
    /// la misma respuesta: ExcepcionNegocio queda para los fallos globales
    /// (archivo sin columnas, sin contratación activa, cupo insuficiente).
    /// </summary>
    public class BEResultadoPadron
    {
        #region Propiedades
        public int Leidas { get; set; }
        /// <summary>Filas que se pueden invitar; ya invitadas si Confirmado.</summary>
        public int Validas { get; set; }
        public int ConError { get; set; }
        /// <summary>False en la previsualización: todavía no se escribió nada.</summary>
        public bool Confirmado { get; set; }
        public int CupoDisponible { get; set; }
        public List<BEResultadoFilaPadron> Filas { get; set; }
        #endregion

        public BEResultadoPadron()
        {
            this.Filas = new List<BEResultadoFilaPadron>();
        }
    }
}
