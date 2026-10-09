using System.Collections.Generic;
using worklingua.Server.BE;
using worklingua.Server.MPP;

namespace worklingua.Server.BLL
{
    /// <summary>
    /// Reportes de ganancias y tablero del negocio (Formulario, puntos 15.b y
    /// 15.c). La ganancia es el cobrado neto: los pagos con tarjeta aprobados
    /// menos las notas de crédito emitidas, en la moneda base.
    /// </summary>
    public class BLLReporte
    {
        public const string AgrupacionDia = "DIA";
        public const string AgrupacionSemana = "SEMANA";
        public const string AgrupacionMes = "MES";
        public const string AgrupacionAnio = "ANIO";

        private const int AniosMaximos = 5;

        MPPReporte oMPPRep;

        public BLLReporte()
        {
            oMPPRep = new MPPReporte();
        }

        public List<BEGananciaPeriodo> ListarGanancias(BEFiltroReporte Objeto)
        {
            Normalizar(Objeto);

            return oMPPRep.ListarGanancias(Objeto);
        }

        public List<BEGananciaZona> ListarGananciasPorZona(BEFiltroReporte Objeto)
        {
            Normalizar(Objeto);

            return oMPPRep.ListarGananciasPorZona(Objeto);
        }

        public BETablero ObtenerTablero()
        {
            BETablero oTableroBE = oMPPRep.ObtenerTablero();

            oTableroBE.Planes = oMPPRep.ListarContratacionesPorPlan();

            return oTableroBE;
        }

        /// <summary>
        /// Completa el período con el año en curso cuando no llega, acota la
        /// ventana y deja la agrupación en uno de los cuatro valores válidos.
        /// </summary>
        private void Normalizar(BEFiltroReporte Objeto)
        {
            DateOnly hoy = DateOnly.FromDateTime(DateTime.Now);

            if (!Objeto.Desde.HasValue)
            {
                Objeto.Desde = new DateOnly(hoy.Year, 1, 1);
            }

            if (!Objeto.Hasta.HasValue)
            {
                Objeto.Hasta = hoy;
            }

            if (Objeto.Hasta.Value < Objeto.Desde.Value)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "La fecha de fin no puede ser anterior a la de inicio.");
            }

            if (Objeto.Desde.Value < Objeto.Hasta.Value.AddYears(-AniosMaximos))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El reporte abarca como máximo " + AniosMaximos + " años.");
            }

            Objeto.Agrupacion = NormalizarAgrupacion(Objeto.Agrupacion);
            Objeto.Provincia = string.IsNullOrWhiteSpace(Objeto.Provincia) ? null : Objeto.Provincia.Trim();
        }

        private string NormalizarAgrupacion(string agrupacion)
        {
            if (string.IsNullOrWhiteSpace(agrupacion))
            {
                return AgrupacionMes;
            }

            string valor = agrupacion.Trim().ToUpperInvariant();

            if (valor == AgrupacionDia || valor == AgrupacionSemana
                || valor == AgrupacionMes || valor == AgrupacionAnio)
            {
                return valor;
            }

            throw new ExcepcionNegocio(
                TipoErrorNegocio.Validacion,
                "La agrupación tiene que ser " + AgrupacionDia + ", " + AgrupacionSemana + ", "
                    + AgrupacionMes + " o " + AgrupacionAnio + ".");
        }
    }
}
