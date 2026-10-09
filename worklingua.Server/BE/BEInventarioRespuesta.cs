using System.Collections.Generic;

namespace worklingua.Server.BE
{
    /// <summary>
    /// El inventario de licencias de una empresa: el cupo del plan contratado y
    /// la lista de empleados con su situación.
    /// </summary>
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
