using System.Collections.Generic;

namespace worklingua.Server.BE
{
    public class BEPaginaBitacoraRespuesta
    {
        #region Propiedades
        public List<BEBitacoraEventoRespuesta> Registros { get; set; }
        public int TotalRegistros { get; set; }
        public int Pagina { get; set; }
        public int TamanioPagina { get; set; }
        public int TotalPaginas { get; set; }
        #endregion

        public BEPaginaBitacoraRespuesta()
        {

        }

        public BEPaginaBitacoraRespuesta(
            List<BEBitacoraEventoRespuesta> registros,
            int totalRegistros,
            int pagina,
            int tamanioPagina,
            int totalPaginas)
        {
            this.Registros = registros;
            this.TotalRegistros = totalRegistros;
            this.Pagina = pagina;
            this.TamanioPagina = tamanioPagina;
            this.TotalPaginas = totalPaginas;
        }
    }
}
