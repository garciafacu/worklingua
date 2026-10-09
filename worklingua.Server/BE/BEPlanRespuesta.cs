using System.Collections.Generic;

namespace worklingua.Server.BE
{
    public class BEPlanRespuesta
    {
        #region Propiedades
        public int PlanId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal PrecioMensual { get; set; }
        public int CantidadLicencias { get; set; }
        public bool Destacado { get; set; }
        public decimal PromedioValoracion { get; set; }
        public int CantidadValoraciones { get; set; }
        public List<BEPlanCaracteristicaRespuesta> Caracteristicas { get; set; }
        #endregion

        public BEPlanRespuesta()
        {

        }

        public BEPlanRespuesta(
            int planId,
            string nombre,
            string descripcion,
            decimal precioMensual,
            int cantidadLicencias,
            bool destacado,
            decimal promedioValoracion,
            int cantidadValoraciones,
            List<BEPlanCaracteristicaRespuesta> caracteristicas)
        {
            this.PlanId = planId;
            this.Nombre = nombre;
            this.Descripcion = descripcion;
            this.PrecioMensual = precioMensual;
            this.CantidadLicencias = cantidadLicencias;
            this.Destacado = destacado;
            this.PromedioValoracion = promedioValoracion;
            this.CantidadValoraciones = cantidadValoraciones;
            this.Caracteristicas = caracteristicas;
        }
    }
}
