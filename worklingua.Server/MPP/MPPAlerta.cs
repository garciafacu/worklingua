using System.Collections;
using System.Collections.Generic;
using System.Data;
using worklingua.Server.BE;
using worklingua.Server.DAL;
using worklingua.Server.Services;

namespace worklingua.Server.MPP
{
    public class MPPAlerta
    {
        Acceso oDatos;
        Hashtable Hdatos;

        public MPPAlerta()
        {
            oDatos = new Acceso();
        }

        /// <summary>
        /// Todas las alertas, incluidas las desactivadas. Con EmpresaId en cero
        /// trae las de todas las empresas: el alcance lo decide la BLL.
        /// </summary>
        public List<BEAlerta> Listar(BEAlerta Objeto)
        {
            List<BEAlerta> ListaAlertaBE = new List<BEAlerta>();

            Hdatos = new Hashtable();
            Hdatos.Add("@EmpresaId", Objeto.EmpresaId == 0 ? null : (object)Objeto.EmpresaId);

            DataTable Dt = oDatos.Leer("sp_Alerta_Listar", Hdatos);

            foreach (DataRow Item in Dt.Rows)
            {
                ListaAlertaBE.Add(Mapear(Item));
            }

            return ListaAlertaBE;
        }

        public BEAlerta ListarObjeto(BEAlerta Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@AlertaId", Objeto.AlertaId);

            DataTable Dt = oDatos.Leer("sp_Alerta_ObtenerPorId", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                return Mapear(Dt.Rows[0]);
            }
            else
            {
                return null;
            }
        }

        public int Alta(BEAlerta Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@EmpresaId", Objeto.EmpresaId);
            Hdatos.Add("@Titulo", Objeto.Titulo);
            Hdatos.Add("@Mensaje", Objeto.Mensaje);
            Hdatos.Add("@DiasInactividad", Objeto.DiasInactividad);
            Hdatos.Add("@UsuarioId", Objeto.UsuarioId);

            if (Objeto.DepartamentoId.HasValue)
            {
                Hdatos.Add("@DepartamentoId", Objeto.DepartamentoId.Value);
            }

            object identidad = oDatos.LeerEscalar("sp_Alerta_Alta", Hdatos);

            return Convert.ToInt32(identidad);
        }

        public bool CambiarEstado(BEAlerta Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@AlertaId", Objeto.AlertaId);
            Hdatos.Add("@Activo", Objeto.Activo.HasValue && Objeto.Activo.Value);

            object filas = oDatos.LeerEscalar("sp_Alerta_CambiarEstado", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        /// <summary>
        /// Los empleados que cumplen la condición de inactividad. Es la única
        /// consulta que define qué significa estar inactivo.
        /// </summary>
        public List<BEEmpleadoInactivo> ListarInactivos(BEAlerta Objeto, string rol)
        {
            List<BEEmpleadoInactivo> ListaBE = new List<BEEmpleadoInactivo>();

            Hdatos = new Hashtable();
            Hdatos.Add("@EmpresaId", Objeto.EmpresaId);
            Hdatos.Add("@DiasInactividad", Objeto.DiasInactividad);
            Hdatos.Add("@Rol", rol);

            if (Objeto.DepartamentoId.HasValue)
            {
                Hdatos.Add("@DepartamentoId", Objeto.DepartamentoId.Value);
            }

            DataTable Dt = oDatos.Leer("sp_Alerta_ListarInactivos", Hdatos);

            foreach (DataRow Item in Dt.Rows)
            {
                BEEmpleadoInactivo oEmpleadoBE = new BEEmpleadoInactivo();

                oEmpleadoBE.UsuarioId = ServicioLectorFila.LeerEntero(Item, "UsuarioId");
                oEmpleadoBE.Empleado = ServicioLectorFila.LeerTexto(Item, "Empleado");
                oEmpleadoBE.Email = ServicioLectorFila.LeerTexto(Item, "Email");
                oEmpleadoBE.Departamento = ServicioLectorFila.LeerTextoNulo(Item, "Departamento");
                oEmpleadoBE.DiasSinActividad = ServicioLectorFila.LeerEntero(Item, "DiasSinActividad");

                ListaBE.Add(oEmpleadoBE);
            }

            return ListaBE;
        }

        /// <summary>
        /// Emite la alerta a los usuarios indicados y devuelve cuántas
        /// notificaciones se crearon. Los ids son los que devolvió
        /// <see cref="ListarInactivos" />: el SP no recalcula la condición.
        /// </summary>
        public int Emitir(int alertaId, List<int> usuarioIds)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@AlertaId", alertaId);
            Hdatos.Add("@UsuarioIds", string.Join(",", usuarioIds));

            object destinatarios = oDatos.LeerEscalar("sp_Alerta_Emitir", Hdatos);

            return ServicioLectorFila.ATotalFilas(destinatarios);
        }

        private BEAlerta Mapear(DataRow Item)
        {
            BEAlerta oAlertaBE = new BEAlerta();

            oAlertaBE.AlertaId = ServicioLectorFila.LeerEntero(Item, "AlertaId");
            oAlertaBE.EmpresaId = ServicioLectorFila.LeerEntero(Item, "EmpresaId");
            oAlertaBE.Empresa = ServicioLectorFila.LeerTextoNulo(Item, "Empresa");
            oAlertaBE.DepartamentoId = ServicioLectorFila.LeerEnteroNulo(Item, "DepartamentoId");
            oAlertaBE.Departamento = ServicioLectorFila.LeerTextoNulo(Item, "Departamento");
            oAlertaBE.Titulo = ServicioLectorFila.LeerTexto(Item, "Titulo");
            oAlertaBE.Mensaje = ServicioLectorFila.LeerTexto(Item, "Mensaje");
            oAlertaBE.DiasInactividad = ServicioLectorFila.LeerEntero(Item, "DiasInactividad");
            oAlertaBE.Activo = ServicioLectorFila.LeerBooleanoNulo(Item, "Activo");
            oAlertaBE.FechaAlta = ServicioLectorFila.LeerFechaHoraNula(Item, "FechaAlta");
            oAlertaBE.UltimaEmision = ServicioLectorFila.LeerFechaHoraNula(Item, "UltimaEmision");
            oAlertaBE.Destinatarios = ServicioLectorFila.LeerEntero(Item, "Destinatarios");

            return oAlertaBE;
        }
    }
}
