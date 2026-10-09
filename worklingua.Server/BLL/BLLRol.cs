using System.Collections.Generic;
using worklingua.Server.BE;
using worklingua.Server.MPP;
using worklingua.Server.Services;

namespace worklingua.Server.BLL
{
    public class BLLRol
    {
        private const int LongitudMaximaNombre = 60;
        private const int LongitudMaximaDescripcion = 250;

        static readonly TimeSpan DuracionCache = TimeSpan.FromMinutes(10);
        static List<BERol> CacheRoles;
        static DateTime VencimientoCache;

        MPPRol oMPPRol;
        BLLPermiso oBLLPer;

        public BLLRol()
        {
            oMPPRol = new MPPRol();
            oBLLPer = new BLLPermiso();
        }

        public List<BERol> ListarTodo()
        {
            if (CacheRoles != null && DateTime.Now < VencimientoCache)
            {
                return CacheRoles;
            }

            List<BERol> ListaRolBE = oMPPRol.ListarTodo();

            if (ListaRolBE == null)
            {
                ListaRolBE = new List<BERol>();
            }

            CacheRoles = ListaRolBE;
            VencimientoCache = DateTime.Now.Add(DuracionCache);

            return ListaRolBE;
        }

        public BERol ObtenerPorNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return null;
            }

            List<BERol> ListaRolBE = ListarTodo();

            foreach (BERol oRolBE in ListaRolBE)
            {
                if (string.Equals(oRolBE.Nombre.Trim(), nombre.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return oRolBE;
                }
            }

            return null;
        }

        public BERol ListarObjeto(BERol Objeto)
        {
            foreach (BERol oRolBE in ListarTodo())
            {
                if (oRolBE.RolId == Objeto.RolId)
                {
                    return oRolBE;
                }
            }

            throw new ExcepcionNegocio(TipoErrorNegocio.NoEncontrado, "El rol no existe.");
        }

        public BERol Guardar(BERol Objeto)
        {
            Validar(Objeto);

            if (Objeto.RolId != 0)
            {
                BERol oActualBE = ListarObjeto(Objeto);

                ExigirEditable(oActualBE);

                if (EsRolPorDefecto(oActualBE)
                    && !string.Equals(oActualBE.Nombre.Trim(), Objeto.Nombre, StringComparison.OrdinalIgnoreCase))
                {
                    throw new ExcepcionNegocio(
                        TipoErrorNegocio.Conflicto,
                        "El rol " + oActualBE.Nombre + " es el que reciben los registros públicos: no se puede renombrar.");
                }
            }

            if (ExisteOtroConNombre(Objeto))
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.Conflicto, "Ya existe un rol con ese nombre.");
            }

            Objeto.RolId = oMPPRol.Guardar(Objeto);

            InvalidarCache();

            return ListarObjeto(Objeto);
        }

        public bool Baja(BERol Objeto)
        {
            BERol oRolBE = ListarObjeto(Objeto);

            ExigirEditable(oRolBE);

            if (EsRolPorDefecto(oRolBE))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto,
                    "El rol " + oRolBE.Nombre + " es el que reciben los registros públicos: no se puede eliminar.");
            }

            if (oMPPRol.ContarUsuarios(oRolBE) > 0)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto,
                    "El rol " + oRolBE.Nombre + " tiene usuarios asignados. Cambiales el rol antes de eliminarlo.");
            }

            bool resultado = oMPPRol.Baja(oRolBE);

            InvalidarCache();

            return resultado;
        }

        public List<BEPermiso> ObtenerPermisos(BERol Objeto)
        {
            BERol oRolBE = ListarObjeto(Objeto);

            return oBLLPer.ObtenerAsignados(oMPPRol.ObtenerPermisos(oRolBE));
        }

        public bool AsignarPermiso(BERolPermiso Objeto)
        {
            BERol oRolBE = ObtenerRolObligatorio(Objeto.RolId);

            ExigirEditable(oRolBE);

            List<BEPermiso> ListaPermisoBE = oBLLPer.ObtenerAsignados(new List<BERolPermiso> { Objeto });

            if (ListaPermisoBE.Count == 0)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.NoEncontrado, "El permiso no existe.");
            }

            BEPermiso oPermisoBE = ListaPermisoBE[0];

            if (TienePermisoAsignado(oRolBE, oPermisoBE.PermisoId))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto,
                    "El rol " + oRolBE.Nombre + " ya tiene asignado " + oPermisoBE.Nombre + ".");
            }

            if (oBLLPer.ContienePermisoReservado(oPermisoBE))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto,
                    "Los permisos de administración de operadores, roles y permisos son exclusivos del Administrador de Plataforma.");
            }

            return oMPPRol.AsignarPermiso(Objeto);
        }

        public bool QuitarPermiso(BERolPermiso Objeto)
        {
            BERol oRolBE = ObtenerRolObligatorio(Objeto.RolId);

            ExigirEditable(oRolBE);

            if (!TienePermisoAsignado(oRolBE, Objeto.PermisoId))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.NoEncontrado, "El rol " + oRolBE.Nombre + " no tiene asignado ese permiso.");
            }

            return oMPPRol.QuitarPermiso(Objeto);
        }

        private BERol ObtenerRolObligatorio(int rolId)
        {
            BERol oFiltroBE = new BERol();
            oFiltroBE.RolId = rolId;

            return ListarObjeto(oFiltroBE);
        }

        private bool TienePermisoAsignado(BERol oRolBE, int permisoId)
        {
            List<BERolPermiso> ListaRolPermisoBE = oMPPRol.ObtenerPermisos(oRolBE);

            if (ListaRolPermisoBE == null)
            {
                return false;
            }

            foreach (BERolPermiso oRolPermisoBE in ListaRolPermisoBE)
            {
                if (oRolPermisoBE.PermisoId == permisoId)
                {
                    return true;
                }
            }

            return false;
        }

        private void ExigirEditable(BERol oRolBE)
        {
            if (string.Equals(oRolBE.Nombre.Trim(), Configuracion.SuperAdminRol.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto,
                    "El rol " + oRolBE.Nombre + " es el de la plataforma: recibe siempre todos los permisos y no se modifica desde el ABM.");
            }
        }

        private bool EsRolPorDefecto(BERol oRolBE)
        {
            return string.Equals(oRolBE.Nombre.Trim(), Configuracion.RolPorDefecto.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        private bool ExisteOtroConNombre(BERol Objeto)
        {
            foreach (BERol oRolBE in ListarTodo())
            {
                if (oRolBE.RolId != Objeto.RolId
                    && string.Equals(oRolBE.Nombre.Trim(), Objeto.Nombre, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private void InvalidarCache()
        {
            CacheRoles = null;
            VencimientoCache = DateTime.MinValue;
        }

        private void Validar(BERol Objeto)
        {
            Objeto.Nombre = string.IsNullOrWhiteSpace(Objeto.Nombre) ? null : Objeto.Nombre.Trim();
            Objeto.Descripcion = string.IsNullOrWhiteSpace(Objeto.Descripcion) ? null : Objeto.Descripcion.Trim();

            if (Objeto.Nombre == null)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.Validacion, "El nombre del rol es obligatorio.");
            }

            if (Objeto.Nombre.Length > LongitudMaximaNombre)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El nombre no puede superar los " + LongitudMaximaNombre + " caracteres.");
            }

            if (Objeto.Descripcion != null && Objeto.Descripcion.Length > LongitudMaximaDescripcion)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "La descripción no puede superar los " + LongitudMaximaDescripcion + " caracteres.");
            }
        }
    }
}
