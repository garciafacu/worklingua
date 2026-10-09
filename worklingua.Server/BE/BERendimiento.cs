using System.Collections.Generic;

namespace worklingua.Server.BE
{
    /// <summary>El panel completo: cifras, comparación y detalle.</summary>
    public class BERendimiento
    {
        #region Propiedades
        public int EmpresaId { get; set; }
        public string Empresa { get; set; }
        public BERendimientoResumen Resumen { get; set; }
        public List<BERendimientoDepartamento> Departamentos { get; set; }
        public List<BERendimientoEmpleado> Empleados { get; set; }
        #endregion

        public BERendimiento()
        {
            this.Resumen = new BERendimientoResumen();
            this.Departamentos = new List<BERendimientoDepartamento>();
            this.Empleados = new List<BERendimientoEmpleado>();
        }
    }
}
