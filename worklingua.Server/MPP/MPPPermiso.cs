using System.Collections;
using System.Data;
using worklingua.Server.BE;
using worklingua.Server.DAL;
using worklingua.Server.Services;

namespace worklingua.Server.MPP
{
    public class MPPPermiso
    {
        Acceso oDatos = new Acceso();
        Hashtable Hdatos;

        public List<BEPermiso> ListarTodo()
        {
            DataTable Dt = oDatos.Leer("sp_Permiso_Listar", null);

            if (Dt.Rows.Count == 0)
            {
                return null;
            }

            Dictionary<int, BEPermiso> PermisosPorId = new Dictionary<int, BEPermiso>();
            List<BEPermiso> ListaPermisoBE = new List<BEPermiso>();

            foreach (DataRow Item in Dt.Rows)
            {
                BEPermiso oPermisoBE = Mapear(Item);

                PermisosPorId.Add(oPermisoBE.PermisoId, oPermisoBE);
                ListaPermisoBE.Add(oPermisoBE);
            }

            DataTable DtJerarquia = oDatos.Leer("sp_Permiso_ListarJerarquia", null);

            foreach (DataRow Item in DtJerarquia.Rows)
            {
                int padreId = ServicioLectorFila.LeerEntero(Item, "PermisoPadreId");
                int hijoId = ServicioLectorFila.LeerEntero(Item, "PermisoHijoId");

                if (PermisosPorId.ContainsKey(padreId)
                    && PermisosPorId.ContainsKey(hijoId)
                    && PermisosPorId[padreId].EsCompuesto)
                {
                    PermisosPorId[padreId].AgregarPermiso(PermisosPorId[hijoId]);
                }
            }

            return ListaPermisoBE;
        }

        public int Guardar(BEPermiso Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@Nombre", Objeto.Nombre);
            Hdatos.Add("@Descripcion", Objeto.Descripcion);

            if (Objeto.PermisoId != 0)
            {
                Hdatos.Add("@PermisoId", Objeto.PermisoId);

                oDatos.LeerEscalar("sp_Permiso_Modificar", Hdatos);

                return Objeto.PermisoId;
            }

            object identidad = oDatos.LeerEscalar("sp_Permiso_Alta", Hdatos);

            return Convert.ToInt32(identidad);
        }

        public bool Baja(BEPermiso Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@PermisoId", Objeto.PermisoId);

            object filas = oDatos.LeerEscalar("sp_Permiso_Baja", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        public bool AgregarHijo(BEPermisoJerarquia Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@PermisoPadreId", Objeto.PermisoPadreId);
            Hdatos.Add("@PermisoHijoId", Objeto.PermisoHijoId);

            return oDatos.Escribir("sp_Permiso_AgregarHijo", Hdatos);
        }

        public bool QuitarHijo(BEPermisoJerarquia Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@PermisoPadreId", Objeto.PermisoPadreId);
            Hdatos.Add("@PermisoHijoId", Objeto.PermisoHijoId);

            object filas = oDatos.LeerEscalar("sp_Permiso_QuitarHijo", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        public int ContarRoles(BEPermiso Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@PermisoId", Objeto.PermisoId);

            object cantidad = oDatos.LeerEscalar("sp_Permiso_ContarRoles", Hdatos);

            return ServicioLectorFila.ATotalFilas(cantidad);
        }

        private BEPermiso Mapear(DataRow Item)
        {
            BEPermiso oPermisoBE;

            if (ServicioLectorFila.LeerBooleano(Item, "EsCompuesto"))
            {
                oPermisoBE = new BEPermisoCompuesto();
            }
            else
            {
                oPermisoBE = new BEPermisoSimple();
            }

            oPermisoBE.PermisoId = ServicioLectorFila.LeerEntero(Item, "PermisoId");
            oPermisoBE.Nombre = ServicioLectorFila.LeerTexto(Item, "Nombre");
            oPermisoBE.Descripcion = ServicioLectorFila.LeerTextoNulo(Item, "Descripcion");
            oPermisoBE.Activo = ServicioLectorFila.LeerBooleanoNulo(Item, "Activo");

            return oPermisoBE;
        }
    }

    public class MPPRol
    {
        Acceso oDatos = new Acceso();
        Hashtable Hdatos;

        public List<BERol> ListarTodo()
        {
            return ServicioLectorFila.ALista<BERol>(oDatos.Leer("sp_Rol_Listar", null), Mapear);
        }

        public int Guardar(BERol Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@Nombre", Objeto.Nombre);
            Hdatos.Add("@Descripcion", Objeto.Descripcion);

            if (Objeto.RolId != 0)
            {
                Hdatos.Add("@RolId", Objeto.RolId);

                oDatos.LeerEscalar("sp_Rol_Modificar", Hdatos);

                return Objeto.RolId;
            }

            object identidad = oDatos.LeerEscalar("sp_Rol_Alta", Hdatos);

            return Convert.ToInt32(identidad);
        }

        public bool Baja(BERol Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@RolId", Objeto.RolId);

            object filas = oDatos.LeerEscalar("sp_Rol_Baja", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        public int ContarUsuarios(BERol Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@RolId", Objeto.RolId);

            object cantidad = oDatos.LeerEscalar("sp_Rol_ContarUsuarios", Hdatos);

            return ServicioLectorFila.ATotalFilas(cantidad);
        }

        public List<BERolPermiso> ObtenerPermisos(BERol Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@RolId", Objeto.RolId);

            return ServicioLectorFila.ALista<BERolPermiso>(oDatos.Leer("sp_Rol_ObtenerPermisos", Hdatos), MapearRolPermiso);
        }

        public bool AsignarPermiso(BERolPermiso Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@RolId", Objeto.RolId);
            Hdatos.Add("@PermisoId", Objeto.PermisoId);

            return oDatos.Escribir("sp_Rol_AsignarPermiso", Hdatos);
        }

        public bool QuitarPermiso(BERolPermiso Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@RolId", Objeto.RolId);
            Hdatos.Add("@PermisoId", Objeto.PermisoId);

            object filas = oDatos.LeerEscalar("sp_Rol_QuitarPermiso", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        public int SincronizarPermisos(BERol Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@RolId", Objeto.RolId);

            object filas = oDatos.LeerEscalar("sp_Rol_SincronizarPermisos", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas);
        }

        private BERol Mapear(DataRow Item)
        {
            BERol oRolBE = new BERol();

            oRolBE.RolId = ServicioLectorFila.LeerEntero(Item, "RolId");
            oRolBE.Nombre = ServicioLectorFila.LeerTexto(Item, "Nombre");
            oRolBE.Descripcion = ServicioLectorFila.LeerTextoNulo(Item, "Descripcion");
            oRolBE.Activo = ServicioLectorFila.LeerBooleanoNulo(Item, "Activo");

            return oRolBE;
        }

        private BERolPermiso MapearRolPermiso(DataRow Item)
        {
            BERolPermiso oRolPermisoBE = new BERolPermiso();

            oRolPermisoBE.RolPermisoId = ServicioLectorFila.LeerEntero(Item, "RolPermisoId");
            oRolPermisoBE.RolId = ServicioLectorFila.LeerEntero(Item, "RolId");
            oRolPermisoBE.PermisoId = ServicioLectorFila.LeerEntero(Item, "PermisoId");

            return oRolPermisoBE;
        }
    }
}
