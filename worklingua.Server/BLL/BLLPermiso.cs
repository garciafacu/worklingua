using System.Collections.Generic;
using worklingua.Server.BE;
using worklingua.Server.MPP;

namespace worklingua.Server.BLL
{
    public class BLLPermiso
    {
        private const int LongitudMaximaNombre = 80;
        private const int LongitudMaximaDescripcion = 300;

        static readonly string[] PermisosReservados =
        {
            Permisos.RolListar,
            Permisos.RolAlta,
            Permisos.RolModificar,
            Permisos.RolBaja,
            Permisos.PermisoListar,
            Permisos.PermisoAlta,
            Permisos.PermisoModificar,
            Permisos.PermisoBaja,
            Permisos.OperadorListar,
            Permisos.OperadorAlta,
            Permisos.OperadorModificar,
            Permisos.OperadorBaja
        };

        MPPPermiso oMPPPer;

        public BLLPermiso()
        {
            oMPPPer = new MPPPermiso();
        }

        public List<BEPermiso> ListarTodo()
        {
            List<BEPermiso> ListaPermisoBE = oMPPPer.ListarTodo();

            return ListaPermisoBE == null ? new List<BEPermiso>() : ListaPermisoBE;
        }

        public List<BEPermiso> ListarJerarquia()
        {
            List<BEPermiso> ListaPermisoBE = ListarTodo();
            List<BEPermiso> ListaRaizBE = new List<BEPermiso>();

            foreach (BEPermiso oPermisoBE in ListaPermisoBE)
            {
                if (!EsHijoDeAlgunaFamilia(ListaPermisoBE, oPermisoBE))
                {
                    ListaRaizBE.Add(oPermisoBE);
                }
            }

            return ListaRaizBE;
        }

        public List<BEPermiso> ObtenerAsignados(List<BERolPermiso> ListaRolPermisoBE)
        {
            List<BEPermiso> ListaAsignadoBE = new List<BEPermiso>();

            if (ListaRolPermisoBE == null || ListaRolPermisoBE.Count == 0)
            {
                return ListaAsignadoBE;
            }

            foreach (BEPermiso oPermisoBE in ListarTodo())
            {
                foreach (BERolPermiso oRolPermisoBE in ListaRolPermisoBE)
                {
                    if (oRolPermisoBE.PermisoId == oPermisoBE.PermisoId)
                    {
                        ListaAsignadoBE.Add(oPermisoBE);
                        break;
                    }
                }
            }

            return ListaAsignadoBE;
        }

        public BEPermiso Guardar(BEPermiso Objeto)
        {
            Validar(Objeto);

            List<BEPermiso> ListaPermisoBE = ListarTodo();

            if (Objeto.PermisoId != 0)
            {
                BEPermiso oActualBE = ObtenerObligatorio(ListaPermisoBE, Objeto.PermisoId);

                if (!oActualBE.EsCompuesto)
                {
                    throw new ExcepcionNegocio(
                        TipoErrorNegocio.Conflicto,
                        "El permiso " + oActualBE.Nombre + " está definido por el sistema y no se puede modificar.");
                }
            }

            if (ExisteOtroConNombre(ListaPermisoBE, Objeto))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto, "Ya existe un permiso o una familia con ese nombre.");
            }

            int permisoId = oMPPPer.Guardar(Objeto);

            return ObtenerObligatorio(ListarTodo(), permisoId);
        }

        public bool Baja(BEPermiso Objeto)
        {
            List<BEPermiso> ListaPermisoBE = ListarTodo();
            BEPermiso oPermisoBE = ObtenerObligatorio(ListaPermisoBE, Objeto.PermisoId);

            if (!oPermisoBE.EsCompuesto)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto,
                    "El permiso " + oPermisoBE.Nombre + " está definido por el sistema: solo se pueden eliminar familias.");
            }

            if (EsHijoDeAlgunaFamilia(ListaPermisoBE, oPermisoBE))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto,
                    "La familia " + oPermisoBE.Nombre + " forma parte de otra familia. Quitala de allí antes de eliminarla.");
            }

            if (oMPPPer.ContarRoles(oPermisoBE) > 0)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto,
                    "La familia " + oPermisoBE.Nombre + " está asignada a uno o más roles. Quitala de esos roles antes de eliminarla.");
            }

            return oMPPPer.Baja(oPermisoBE);
        }

        public bool AgregarHijo(BEPermisoJerarquia Objeto)
        {
            List<BEPermiso> ListaPermisoBE = ListarTodo();
            BEPermiso oPadreBE = ObtenerObligatorio(ListaPermisoBE, Objeto.PermisoPadreId);
            BEPermiso oHijoBE = ObtenerObligatorio(ListaPermisoBE, Objeto.PermisoHijoId);

            if (!oPadreBE.EsCompuesto)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "Solo una familia puede contener otros permisos.");
            }

            if (oPadreBE.PermisoId == oHijoBE.PermisoId)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto, "Una familia no puede contenerse a sí misma.");
            }

            if (EsHijoDirecto(oPadreBE, oHijoBE))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto, oHijoBE.Nombre + " ya forma parte de " + oPadreBE.Nombre + ".");
            }

            if (oHijoBE.BuscarPermisoPorNombre(oPadreBE.Nombre) != null)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto,
                    "No se puede agregar " + oHijoBE.Nombre + " a " + oPadreBE.Nombre + ": " +
                    oHijoBE.Nombre + " ya contiene a " + oPadreBE.Nombre + " y se formaría un ciclo.");
            }

            if (ContienePermisoReservado(oHijoBE))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto,
                    "Los permisos de administración de operadores, roles y permisos son exclusivos del Administrador de Plataforma y no se pueden incluir en familias.");
            }

            return oMPPPer.AgregarHijo(Objeto);
        }

        public bool QuitarHijo(BEPermisoJerarquia Objeto)
        {
            List<BEPermiso> ListaPermisoBE = ListarTodo();
            BEPermiso oPadreBE = ObtenerObligatorio(ListaPermisoBE, Objeto.PermisoPadreId);
            BEPermiso oHijoBE = ObtenerObligatorio(ListaPermisoBE, Objeto.PermisoHijoId);

            if (!EsHijoDirecto(oPadreBE, oHijoBE))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.NoEncontrado, oHijoBE.Nombre + " no forma parte de " + oPadreBE.Nombre + ".");
            }

            return oMPPPer.QuitarHijo(Objeto);
        }

        public bool ContienePermisoReservado(BEPermiso Objeto)
        {
            foreach (string reservado in PermisosReservados)
            {
                if (Objeto.BuscarPermisoPorNombre(reservado) != null)
                {
                    return true;
                }
            }

            return false;
        }

        private BEPermiso ObtenerObligatorio(List<BEPermiso> ListaPermisoBE, int permisoId)
        {
            foreach (BEPermiso oPermisoBE in ListaPermisoBE)
            {
                if (oPermisoBE.PermisoId == permisoId)
                {
                    return oPermisoBE;
                }
            }

            throw new ExcepcionNegocio(TipoErrorNegocio.NoEncontrado, "El permiso no existe.");
        }

        private bool EsHijoDirecto(BEPermiso oPadreBE, BEPermiso oHijoBE)
        {
            foreach (BEPermiso oPermisoBE in oPadreBE.ObtenerPermisosHijos())
            {
                if (oPermisoBE.PermisoId == oHijoBE.PermisoId)
                {
                    return true;
                }
            }

            return false;
        }

        private bool EsHijoDeAlgunaFamilia(List<BEPermiso> ListaPermisoBE, BEPermiso oHijoBE)
        {
            foreach (BEPermiso oPermisoBE in ListaPermisoBE)
            {
                if (EsHijoDirecto(oPermisoBE, oHijoBE))
                {
                    return true;
                }
            }

            return false;
        }

        private bool ExisteOtroConNombre(List<BEPermiso> ListaPermisoBE, BEPermiso Objeto)
        {
            foreach (BEPermiso oPermisoBE in ListaPermisoBE)
            {
                if (oPermisoBE.PermisoId != Objeto.PermisoId
                    && string.Equals(oPermisoBE.Nombre, Objeto.Nombre, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private void Validar(BEPermiso Objeto)
        {
            Objeto.Nombre = string.IsNullOrWhiteSpace(Objeto.Nombre) ? null : Objeto.Nombre.Trim();
            Objeto.Descripcion = string.IsNullOrWhiteSpace(Objeto.Descripcion) ? null : Objeto.Descripcion.Trim();

            if (Objeto.Nombre == null)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "El nombre de la familia es obligatorio.");
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
