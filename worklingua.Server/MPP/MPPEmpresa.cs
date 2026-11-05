using System.Collections;
using System.Data;
using worklingua.Server.BE;
using worklingua.Server.DAL;
using worklingua.Server.Services;

namespace worklingua.Server.MPP
{
    public class MPPEmpresa
    {
        Acceso oDatos = new Acceso();
        Hashtable Hdatos;

        public List<BEEmpresa> ListarTodo()
        {
            return Listar("sp_Empresa_Listar");
        }

        public List<BEEmpresa> ListarSeleccionables()
        {
            return Listar("sp_Empresa_ListarSeleccionables");
        }

        public BEEmpresa ListarObjeto(BEEmpresa Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@EmpresaId", Objeto.EmpresaId);

            return ServicioLectorFila.AObjeto(oDatos.Leer("sp_Empresa_ObtenerPorId", Hdatos), Mapear);
        }

        public int Guardar(BEEmpresa Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@RazonSocial", Objeto.RazonSocial);
            Hdatos.Add("@Email", Objeto.Email);
            Hdatos.Add("@Telefono", Objeto.Telefono);
            Hdatos.Add("@Direccion", Objeto.Direccion);
            Hdatos.Add("@Ciudad", Objeto.Ciudad);
            Hdatos.Add("@Provincia", Objeto.Provincia);
            Hdatos.Add("@Pais", Objeto.Pais);

            if (Objeto.EmpresaId != 0)
            {
                Hdatos.Add("@EmpresaId", Objeto.EmpresaId);

                oDatos.LeerEscalar("sp_Empresa_Modificar", Hdatos);

                return Objeto.EmpresaId;
            }

            Hdatos.Add("@CUIT", Objeto.CUIT);
            Hdatos.Add("@Seleccionable", Objeto.Seleccionable);

            object identidad = oDatos.LeerEscalar("sp_Empresa_Alta", Hdatos);

            return Convert.ToInt32(identidad);
        }

        public bool Baja(BEEmpresa Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@EmpresaId", Objeto.EmpresaId);

            object filas = oDatos.LeerEscalar("sp_Empresa_Baja", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        private List<BEEmpresa> Listar(string Consulta)
        {
            return ServicioLectorFila.ALista<BEEmpresa>(oDatos.Leer(Consulta, null), Mapear);
        }

        private BEEmpresa Mapear(DataRow Item)
        {
            BEEmpresa oEmpresaBE = new BEEmpresa();

            oEmpresaBE.EmpresaId = ServicioLectorFila.LeerEntero(Item, "EmpresaId");
            oEmpresaBE.RazonSocial = ServicioLectorFila.LeerTexto(Item, "RazonSocial");
            oEmpresaBE.CUIT = ServicioLectorFila.LeerTexto(Item, "CUIT").Trim();
            oEmpresaBE.Email = ServicioLectorFila.LeerTexto(Item, "Email");
            oEmpresaBE.Telefono = ServicioLectorFila.LeerTextoNulo(Item, "Telefono");
            oEmpresaBE.Direccion = ServicioLectorFila.LeerTextoNulo(Item, "Direccion");
            oEmpresaBE.Ciudad = ServicioLectorFila.LeerTextoNulo(Item, "Ciudad");
            oEmpresaBE.Provincia = ServicioLectorFila.LeerTextoNulo(Item, "Provincia");
            oEmpresaBE.Pais = ServicioLectorFila.LeerTextoNulo(Item, "Pais");
            oEmpresaBE.FechaAlta = ServicioLectorFila.LeerFechaHora(Item, "FechaAlta");
            oEmpresaBE.Activo = ServicioLectorFila.LeerBooleano(Item, "Activo");
            oEmpresaBE.Protegido = ServicioLectorFila.LeerBooleano(Item, "Protegido");
            oEmpresaBE.Seleccionable = ServicioLectorFila.LeerBooleano(Item, "Seleccionable");

            return oEmpresaBE;
        }
    }

    public class MPPDepartamento
    {
        Acceso oDatos = new Acceso();
        Hashtable Hdatos;

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

            return ServicioLectorFila.AObjeto(oDatos.Leer("sp_Departamento_ObtenerPorId", Hdatos), Mapear);
        }

        public int Guardar(BEDepartamento Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@Nombre", Objeto.Nombre);
            Hdatos.Add("@Descripcion", Objeto.Descripcion);

            if (Objeto.DepartamentoId != 0)
            {
                Hdatos.Add("@DepartamentoId", Objeto.DepartamentoId);

                oDatos.LeerEscalar("sp_Departamento_Modificar", Hdatos);

                return Objeto.DepartamentoId;
            }

            Hdatos.Add("@EmpresaId", Objeto.EmpresaId);
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
            Hdatos = new Hashtable();
            Hdatos.Add("@EmpresaId", Objeto.EmpresaId == 0 ? null : (object)Objeto.EmpresaId);

            return ServicioLectorFila.ALista(oDatos.Leer(procedimiento, Hdatos), Mapear);
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
