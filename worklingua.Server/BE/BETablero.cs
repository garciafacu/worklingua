using System.Collections.Generic;

namespace worklingua.Server.BE
{
    public class BETablero
    {
        #region Propiedades
        public decimal IngresosMes { get; set; }
        public decimal IngresosMesAnterior { get; set; }
        public decimal DeudaTotal { get; set; }
        public int EmpresasConDeuda { get; set; }
        public int EmpresasActivas { get; set; }
        public int ContratacionesActivas { get; set; }
        public int LicenciasAsignadas { get; set; }
        public int LicenciasContratadas { get; set; }
        public int EmpresasSinCupo { get; set; }
        public List<BEContratacionPorPlan> Planes { get; set; }
        #endregion

        public BETablero()
        {
            this.Planes = new List<BEContratacionPorPlan>();
        }

        public BETablero(
            decimal ingresosMes,
            decimal ingresosMesAnterior,
            decimal deudaTotal,
            int empresasConDeuda,
            int empresasActivas,
            int contratacionesActivas,
            List<BEContratacionPorPlan> planes)
        {
            this.IngresosMes = ingresosMes;
            this.IngresosMesAnterior = ingresosMesAnterior;
            this.DeudaTotal = deudaTotal;
            this.EmpresasConDeuda = empresasConDeuda;
            this.EmpresasActivas = empresasActivas;
            this.ContratacionesActivas = contratacionesActivas;
            this.Planes = planes;
        }
    }
}
