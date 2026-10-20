using System.Collections.Generic;

namespace worklingua.Server.BE
{
    public class BEAlertaRespuesta
    {
        #region Propiedades
        public int AlertaId { get; set; }
        public int EmpresaId { get; set; }
        public string Empresa { get; set; }
        public int? DepartamentoId { get; set; }
        public string Departamento { get; set; }
        public string Titulo { get; set; }
        public string Mensaje { get; set; }
        public int DiasInactividad { get; set; }
        public bool Activo { get; set; }
        public DateTime? FechaAlta { get; set; }
        public DateTime? UltimaEmision { get; set; }
        public int Destinatarios { get; set; }
        #endregion

        public BEAlertaRespuesta()
        {

        }
    }
}
