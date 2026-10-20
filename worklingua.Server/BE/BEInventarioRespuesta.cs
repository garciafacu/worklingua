using System.Collections.Generic;

namespace worklingua.Server.BE
{
    public class BEInventarioRespuesta
    {
        #region Propiedades
        public int EmpresaId { get; set; }
        public string Empresa { get; set; }
        public string Plan { get; set; }
        public int Contratadas { get; set; }
        public int Asignadas { get; set; }
        public int Disponibles { get; set; }
        public List<BEInventarioLicencia> Empleados { get; set; }
        #endregion

        public BEInventarioRespuesta()
        {
            this.Empleados = new List<BEInventarioLicencia>();
        }
    }
}
