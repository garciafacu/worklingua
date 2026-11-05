using worklingua.Server.BE;
using worklingua.Server.MPP;
using worklingua.Server.Services;

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

        MPPPermiso oMPPPer = new MPPPermiso();

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

            ReglasNegocio.ExigirLargoMaximo(Objeto.Nombre, LongitudMaximaNombre,
                "El nombre no puede superar los " + LongitudMaximaNombre + " caracteres.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Descripcion, LongitudMaximaDescripcion,
                "La descripción no puede superar los " + LongitudMaximaDescripcion + " caracteres.");
        }

        public void Validar(BEGuardarPermiso Objeto)
        {
            ReglasNegocio.ExigirTexto(Objeto.Nombre, "El nombre de la familia es obligatorio.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Nombre, 80, "El nombre no puede superar los 80 caracteres.");

            ReglasNegocio.ExigirLargoMaximo(Objeto.Descripcion, 300,
                "La descripción no puede superar los 300 caracteres.");
        }
    }

    public class BLLRol
    {
        private const int LongitudMaximaNombre = 60;
        private const int LongitudMaximaDescripcion = 250;

        static readonly TimeSpan DuracionCache = TimeSpan.FromMinutes(10);
        static List<BERol> CacheRoles;
        static DateTime VencimientoCache;

        MPPRol oMPPRol = new MPPRol();
        BLLPermiso oBLLPer = new BLLPermiso();

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

            ReglasNegocio.ExigirLargoMaximo(Objeto.Nombre, LongitudMaximaNombre,
                "El nombre no puede superar los " + LongitudMaximaNombre + " caracteres.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Descripcion, LongitudMaximaDescripcion,
                "La descripción no puede superar los " + LongitudMaximaDescripcion + " caracteres.");
        }

        public void Validar(BEGuardarRol Objeto)
        {
            ReglasNegocio.ExigirTexto(Objeto.Nombre, "El nombre del rol es obligatorio.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Nombre, 60, "El nombre no puede superar los 60 caracteres.");

            ReglasNegocio.ExigirLargoMaximo(Objeto.Descripcion, 250,
                "La descripción no puede superar los 250 caracteres.");
        }
    }

    public static class Permisos
    {
        public const string BackofficeAcceder = "Backoffice.Acceder";

        public const string PlanListar = "Plan.Listar";
        public const string PlanAlta = "Plan.Alta";
        public const string PlanModificar = "Plan.Modificar";
        public const string PlanBaja = "Plan.Baja";

        public const string CursoListar = "Curso.Listar";
        public const string CursoAlta = "Curso.Alta";
        public const string CursoModificar = "Curso.Modificar";
        public const string CursoBaja = "Curso.Baja";
        public const string CursoVerTodasLasEmpresas = "Curso.VerTodasLasEmpresas";

        public const string CaracteristicaListar = "Caracteristica.Listar";
        public const string CaracteristicaAlta = "Caracteristica.Alta";
        public const string CaracteristicaModificar = "Caracteristica.Modificar";
        public const string CaracteristicaBaja = "Caracteristica.Baja";

        public const string IdiomaListar = "Idioma.Listar";
        public const string IdiomaAlta = "Idioma.Alta";
        public const string IdiomaModificar = "Idioma.Modificar";
        public const string IdiomaBaja = "Idioma.Baja";

        public const string EmpresaListar = "Empresa.Listar";
        public const string EmpresaAlta = "Empresa.Alta";
        public const string EmpresaModificar = "Empresa.Modificar";
        public const string EmpresaBaja = "Empresa.Baja";
        public const string EmpresaVerTodasLasEmpresas = "Empresa.VerTodasLasEmpresas";

        public const string DepartamentoListar = "Departamento.Listar";
        public const string DepartamentoAlta = "Departamento.Alta";
        public const string DepartamentoModificar = "Departamento.Modificar";
        public const string DepartamentoBaja = "Departamento.Baja";
        public const string DepartamentoVerTodasLasEmpresas = "Departamento.VerTodasLasEmpresas";

        public const string AlertaListar = "Alerta.Listar";
        public const string AlertaAlta = "Alerta.Alta";
        public const string AlertaEmitir = "Alerta.Emitir";
        public const string AlertaBaja = "Alerta.Baja";
        public const string AlertaVerTodasLasEmpresas = "Alerta.VerTodasLasEmpresas";

        public const string LicenciaListar = "Licencia.Listar";
        public const string LicenciaAsignar = "Licencia.Asignar";
        public const string LicenciaRevocar = "Licencia.Revocar";
        public const string LicenciaVerTodasLasEmpresas = "Licencia.VerTodasLasEmpresas";

        public const string UsuarioListar = "Usuario.Listar";
        public const string UsuarioInvitar = "Usuario.Invitar";
        public const string UsuarioModificar = "Usuario.Modificar";
        public const string UsuarioBaja = "Usuario.Baja";

        public const string UsuarioVerTodasLasEmpresas = "Usuario.VerTodasLasEmpresas";

        public const string ComentarioParticipar = "Comentario.Participar";
        public const string ComentarioModerar = "Comentario.Moderar";

        public const string TraduccionListar = "Traduccion.Listar";
        public const string TraduccionModificar = "Traduccion.Modificar";

        public const string CulturaListar = "Cultura.Listar";
        public const string CulturaAlta = "Cultura.Alta";
        public const string CulturaModificar = "Cultura.Modificar";
        public const string CulturaBaja = "Cultura.Baja";

        public const string BitacoraListar = "Bitacora.Listar";

        public const string RolListar = "Rol.Listar";
        public const string RolAlta = "Rol.Alta";
        public const string RolModificar = "Rol.Modificar";
        public const string RolBaja = "Rol.Baja";

        public const string PermisoListar = "Permiso.Listar";
        public const string PermisoAlta = "Permiso.Alta";
        public const string PermisoModificar = "Permiso.Modificar";
        public const string PermisoBaja = "Permiso.Baja";

        public const string OperadorListar = "Operador.Listar";
        public const string OperadorAlta = "Operador.Alta";
        public const string OperadorModificar = "Operador.Modificar";
        public const string OperadorBaja = "Operador.Baja";

        public const string NoticiaListar = "Noticia.Listar";
        public const string NoticiaAlta = "Noticia.Alta";
        public const string NoticiaModificar = "Noticia.Modificar";
        public const string NoticiaBaja = "Noticia.Baja";

        public const string NewsletterListar = "Newsletter.Listar";
        public const string NewsletterEnviar = "Newsletter.Enviar";

        public const string SuscripcionContratar = "Suscripcion.Contratar";

        public const string CursoVerProgreso = "Curso.VerProgreso";
        public const string CursoVerProgresoEmpresa = "Curso.VerProgresoEmpresa";

        public const string CuentaCorrienteConsultar = "CuentaCorriente.Consultar";

        public const string TicketCrear = "Ticket.Crear";
        public const string TicketVerEmpresa = "Ticket.VerEmpresa";
        public const string TicketAtender = "Ticket.Atender";

        public const string OfertaConsultar = "Oferta.Consultar";
        public const string OfertaListar = "Oferta.Listar";
        public const string OfertaAlta = "Oferta.Alta";
        public const string OfertaModificar = "Oferta.Modificar";
        public const string OfertaBaja = "Oferta.Baja";

        public const string EncuestaResponder = "Encuesta.Responder";
        public const string EncuestaListar = "Encuesta.Listar";
        public const string EncuestaAlta = "Encuesta.Alta";
        public const string EncuestaModificar = "Encuesta.Modificar";
        public const string EncuestaBaja = "Encuesta.Baja";

        public const string ReporteVer = "Reporte.Ver";

        public const string FaqListar = "Faq.Listar";
        public const string FaqAlta = "Faq.Alta";
        public const string FaqModificar = "Faq.Modificar";
        public const string FaqBaja = "Faq.Baja";
    }
}
