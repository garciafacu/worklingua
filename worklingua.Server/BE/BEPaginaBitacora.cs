using System.Collections.Generic;

namespace worklingua.Server.BE
{
    public class BEPaginaBitacora
    {
        #region Propiedades
        public List<BEBitacoraEventoConUsuario> Registros { get; set; }
        public int TotalRegistros { get; set; }
        public int Pagina { get; set; }
        public int TamanioPagina { get; set; }
        #endregion

        public BEPaginaBitacora()
        {
            this.Registros = new List<BEBitacoraEventoConUsuario>();
        }

        public BEPaginaBitacora(
            List<BEBitacoraEventoConUsuario> registros,
            int totalRegistros,
            int pagina,
            int tamanioPagina)
        {
            this.Registros = registros;
            this.TotalRegistros = totalRegistros;
            this.Pagina = pagina;
            this.TamanioPagina = tamanioPagina;
        }
    }
}
