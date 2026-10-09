using System.Collections;
using System.Collections.Generic;
using System.Data;
using worklingua.Server.BE;
using worklingua.Server.DAL;
using worklingua.Server.Services;

namespace worklingua.Server.MPP
{
    public class MPPRol
    {
        Acceso oDatos;
        Hashtable Hdatos;

        public MPPRol()
        {
            oDatos = new Acceso();
        }

        public List<BERol> ListarTodo()
        {
            List<BERol> ListaRolBE = new List<BERol>();
            DataTable Dt = oDatos.Leer("sp_Rol_Listar", null);

            if (Dt.Rows.Count > 0)
            {
                foreach (DataRow Item in Dt.Rows)
                {
                    ListaRolBE.Add(Mapear(Item));
                }

                return ListaRolBE;
            }
            else
            {
                return null;
            }
        }

        public int Guardar(BERol Objeto)
        {
            Hdatos = new Hashtable();

            if (Objeto.RolId != 0)
            {
                Hdatos.Add("@RolId", Objeto.RolId);
                Hdatos.Add("@Nombre", Objeto.Nombre);
                Hdatos.Add("@Descripcion", Objeto.Descripcion);

                oDatos.LeerEscalar("sp_Rol_Modificar", Hdatos);

                return Objeto.RolId;
            }

            Hdatos.Add("@Nombre", Objeto.Nombre);
            Hdatos.Add("@Descripcion", Objeto.Descripcion);

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

            List<BERolPermiso> ListaRolPermisoBE = new List<BERolPermiso>();
            DataTable Dt = oDatos.Leer("sp_Rol_ObtenerPermisos", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                foreach (DataRow Item in Dt.Rows)
                {
                    ListaRolPermisoBE.Add(MapearRolPermiso(Item));
                }

                return ListaRolPermisoBE;
            }
            else
            {
                return null;
            }
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
