using System.Collections;
using System.Collections.Generic;
using System.Data;
using worklingua.Server.BE;
using worklingua.Server.DAL;
using worklingua.Server.Services;

namespace worklingua.Server.MPP
{
    public class MPPRendimiento
    {
        Acceso oDatos;
        Hashtable Hdatos;

        public MPPRendimiento()
        {
            oDatos = new Acceso();
        }

        public BERendimientoResumen ObtenerResumen(BEFiltroRendimiento Objeto)
        {
            DataTable Dt = oDatos.Leer("sp_Progreso_RendimientoResumen", ArmarFiltro(Objeto, true));

            if (Dt.Rows.Count == 0)
            {
                return new BERendimientoResumen();
            }

            DataRow Item = Dt.Rows[0];

            BERendimientoResumen oResumenBE = new BERendimientoResumen();

            oResumenBE.EmpleadosConActividad = ServicioLectorFila.LeerEntero(Item, "EmpleadosConActividad");
            oResumenBE.ModulosIniciados = ServicioLectorFila.LeerEntero(Item, "ModulosIniciados");
            oResumenBE.ModulosCompletados = ServicioLectorFila.LeerEntero(Item, "ModulosCompletados");
            oResumenBE.CursosCompletados = ServicioLectorFila.LeerEntero(Item, "CursosCompletados");
            oResumenBE.AvancePromedio = ServicioLectorFila.LeerDecimal(Item, "AvancePromedio");
            oResumenBE.Facturado = ServicioLectorFila.LeerDecimal(Item, "Facturado");

            return oResumenBE;
        }

        public List<BERendimientoDepartamento> ListarPorDepartamento(BEFiltroRendimiento Objeto)
        {
            List<BERendimientoDepartamento> ListaBE = new List<BERendimientoDepartamento>();

            // Este SP no filtra por departamento: justamente compara entre todos.
            DataTable Dt = oDatos.Leer("sp_Progreso_RendimientoPorDepartamento", ArmarFiltro(Objeto, false));

            foreach (DataRow Item in Dt.Rows)
            {
                BERendimientoDepartamento oDepartamentoBE = new BERendimientoDepartamento();

                oDepartamentoBE.DepartamentoId = ServicioLectorFila.LeerEnteroNulo(Item, "DepartamentoId");
                oDepartamentoBE.Departamento = ServicioLectorFila.LeerTextoNulo(Item, "Departamento");
                oDepartamentoBE.Empleados = ServicioLectorFila.LeerEntero(Item, "Empleados");
                oDepartamentoBE.EmpleadosConActividad = ServicioLectorFila.LeerEntero(Item, "EmpleadosConActividad");
                oDepartamentoBE.ModulosCompletados = ServicioLectorFila.LeerEntero(Item, "ModulosCompletados");
                oDepartamentoBE.AvancePromedio = ServicioLectorFila.LeerDecimal(Item, "AvancePromedio");

                ListaBE.Add(oDepartamentoBE);
            }

            return ListaBE;
        }

        public List<BERendimientoEmpleado> ListarPorEmpleado(BEFiltroRendimiento Objeto)
        {
            List<BERendimientoEmpleado> ListaBE = new List<BERendimientoEmpleado>();

            DataTable Dt = oDatos.Leer("sp_Progreso_RendimientoPorEmpleado", ArmarFiltro(Objeto, true));

            foreach (DataRow Item in Dt.Rows)
            {
                BERendimientoEmpleado oEmpleadoBE = new BERendimientoEmpleado();

                oEmpleadoBE.UsuarioId = ServicioLectorFila.LeerEntero(Item, "UsuarioId");
                oEmpleadoBE.Empleado = ServicioLectorFila.LeerTexto(Item, "Empleado");
                oEmpleadoBE.Email = ServicioLectorFila.LeerTexto(Item, "Email");
                oEmpleadoBE.Departamento = ServicioLectorFila.LeerTextoNulo(Item, "Departamento");
                oEmpleadoBE.ModulosIniciados = ServicioLectorFila.LeerEntero(Item, "ModulosIniciados");
                oEmpleadoBE.ModulosCompletados = ServicioLectorFila.LeerEntero(Item, "ModulosCompletados");
                oEmpleadoBE.AvancePromedio = ServicioLectorFila.LeerDecimal(Item, "AvancePromedio");
                oEmpleadoBE.UltimaActividad = ServicioLectorFila.LeerFechaHoraNula(Item, "UltimaActividad");

                ListaBE.Add(oEmpleadoBE);
            }

            return ListaBE;
        }

        /// <summary>
        /// Los tres SP comparten empresa y fechas; solo dos aceptan departamento.
        /// Las fechas viajan como DateOnly y el SP las recibe como DATE.
        /// </summary>
        private Hashtable ArmarFiltro(BEFiltroRendimiento Objeto, bool conDepartamento)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@EmpresaId", Objeto.EmpresaId);

            if (Objeto.Desde.HasValue)
            {
                Hdatos.Add("@Desde", Objeto.Desde.Value.ToDateTime(TimeOnly.MinValue));
            }

            if (Objeto.Hasta.HasValue)
            {
                Hdatos.Add("@Hasta", Objeto.Hasta.Value.ToDateTime(TimeOnly.MinValue));
            }

            if (conDepartamento && Objeto.DepartamentoId.HasValue && Objeto.DepartamentoId.Value != 0)
            {
                Hdatos.Add("@DepartamentoId", Objeto.DepartamentoId.Value);
            }

            return Hdatos;
        }
    }
}
