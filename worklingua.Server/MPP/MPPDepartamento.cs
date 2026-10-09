using System.Collections;
using System.Collections.Generic;
using System.Data;
using worklingua.Server.BE;
using worklingua.Server.DAL;
using worklingua.Server.Services;

namespace worklingua.Server.MPP
{
    public class MPPDepartamento
    {
        Acceso oDatos;
        Hashtable Hdatos;

        public MPPDepartamento()
        {
            oDatos = new Acceso();
        }

        /// <summary>
        /// Departamentos activos. Con EmpresaId en cero trae los de todas las
        /// empresas: el alcance lo decide la BLL.
        /// </summary>
        public List<BEDepartamento> ListarTodo(BEDepartamento Objeto)
        {
            return Listar("sp_Departamento_Listar", Objeto);
        }

        public List<BEDepartamento> ListarTodoConBajas(BEDepartamento Objeto)
        {
            return Listar("sp_Departamento_ListarTodos", Objeto);
        }

        public BEDepartamento ListarObjeto(BEDepartamento Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@DepartamentoId", Objeto.DepartamentoId);

            DataTable Dt = oDatos.Leer("sp_Departamento_ObtenerPorId", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                return Mapear(Dt.Rows[0]);
            }
            else
            {
                return null;
            }
        }

        public int Guardar(BEDepartamento Objeto)
        {
            Hdatos = new Hashtable();

            if (Objeto.DepartamentoId != 0)
            {
                Hdatos.Add("@DepartamentoId", Objeto.DepartamentoId);
                Hdatos.Add("@Nombre", Objeto.Nombre);
                Hdatos.Add("@Descripcion", Objeto.Descripcion);

                oDatos.LeerEscalar("sp_Departamento_Modificar", Hdatos);

                return Objeto.DepartamentoId;
            }

            Hdatos.Add("@EmpresaId", Objeto.EmpresaId);
            Hdatos.Add("@Nombre", Objeto.Nombre);
            Hdatos.Add("@Descripcion", Objeto.Descripcion);
            Hdatos.Add("@Activo", Objeto.Activo.HasValue && Objeto.Activo.Value);

            object identidad = oDatos.LeerEscalar("sp_Departamento_Alta", Hdatos);

            return Convert.ToInt32(identidad);
        }

        public bool Baja(BEDepartamento Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@DepartamentoId", Objeto.DepartamentoId);

            object filas = oDatos.LeerEscalar("sp_Departamento_Baja", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        public bool CambiarEstado(BEDepartamento Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@DepartamentoId", Objeto.DepartamentoId);
            Hdatos.Add("@Activo", Objeto.Activo.HasValue && Objeto.Activo.Value);

            object filas = oDatos.LeerEscalar("sp_Departamento_CambiarEstado", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        private List<BEDepartamento> Listar(string procedimiento, BEDepartamento Objeto)
        {
            List<BEDepartamento> ListaDepartamentoBE = new List<BEDepartamento>();

            Hdatos = new Hashtable();
            Hdatos.Add("@EmpresaId", Objeto.EmpresaId == 0 ? null : (object)Objeto.EmpresaId);

            DataTable Dt = oDatos.Leer(procedimiento, Hdatos);

            if (Dt.Rows.Count > 0)
            {
                foreach (DataRow Item in Dt.Rows)
                {
                    ListaDepartamentoBE.Add(Mapear(Item));
                }

                return ListaDepartamentoBE;
            }
            else
            {
                return null;
            }
        }

        private BEDepartamento Mapear(DataRow Item)
        {
            BEDepartamento oDepartamentoBE = new BEDepartamento();

            oDepartamentoBE.DepartamentoId = ServicioLectorFila.LeerEntero(Item, "DepartamentoId");
            oDepartamentoBE.EmpresaId = ServicioLectorFila.LeerEntero(Item, "EmpresaId");
            oDepartamentoBE.Empresa = ServicioLectorFila.LeerTextoNulo(Item, "Empresa");
            oDepartamentoBE.Nombre = ServicioLectorFila.LeerTexto(Item, "Nombre");
            oDepartamentoBE.Descripcion = ServicioLectorFila.LeerTextoNulo(Item, "Descripcion");
            oDepartamentoBE.FechaAlta = ServicioLectorFila.LeerFechaHoraNula(Item, "FechaAlta");
            oDepartamentoBE.Activo = ServicioLectorFila.LeerBooleanoNulo(Item, "Activo");
            oDepartamentoBE.Empleados = ServicioLectorFila.LeerEntero(Item, "Empleados");

            return oDepartamentoBE;
        }
    }
}
