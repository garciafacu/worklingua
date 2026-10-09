using System.Collections.Generic;
using worklingua.Server.BE;
using worklingua.Server.MPP;
using worklingua.Server.Services;

namespace worklingua.Server.BLL
{
    public class BLLUsuario
    {
        public const int LongitudMaximaNombre = 80;
        private static readonly DateOnly FechaNacimientoMinima = new DateOnly(1900, 1, 1);

        MPPUsuario oMPPUsu;
        BLLRol oBLLRol;
        BLLSesion oBLLSes;
        BLLTokenSeguridad oBLLTok;
        BLLBitacora oBLLBit;
        BLLSeguridad oBLLSeg;
        BLLEmpresa oBLLEmp;
        BLLSuscripcion oBLLSus;
        BLLDepartamento oBLLDep;
        BLLLicencia oBLLLic;
        ServicioEmailCuenta oServicioEmail;
        ServicioCaptcha oServicioCaptcha;

        public BLLUsuario()
        {
            oMPPUsu = new MPPUsuario();
            oBLLRol = new BLLRol();
            oBLLSes = new BLLSesion();
            oBLLTok = new BLLTokenSeguridad();
            oBLLBit = new BLLBitacora();
            oBLLSeg = new BLLSeguridad();
            oBLLEmp = new BLLEmpresa();
            oBLLSus = new BLLSuscripcion();
            oBLLDep = new BLLDepartamento();
            oBLLLic = new BLLLicencia();
            oServicioEmail = new ServicioEmailCuenta();
            oServicioCaptcha = new ServicioCaptcha();
        }

        public async Task<BEUsuario> Registrar(BERegistroUsuario Objeto)
        {
            BEUsuario oUsuarioBE = new BEUsuario();

            oUsuarioBE.Nombre = Objeto.Nombre;
            oUsuarioBE.Apellido = Objeto.Apellido;
            oUsuarioBE.Documento = Objeto.Documento;
            oUsuarioBE.Email = Objeto.Email;
            oUsuarioBE.FechaNacimiento = Objeto.FechaNacimiento;
            oUsuarioBE.PasswordHash = Objeto.Clave;

            BEEmpresa oEmpresaBE = new BEEmpresa();

            oEmpresaBE.RazonSocial = Objeto.RazonSocial;
            oEmpresaBE.CUIT = Objeto.CUIT;

            if (!await oServicioCaptcha.ValidarAsync(Objeto.TokenCaptcha))
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.Validacion, "No pudimos verificar el CAPTCHA. Volvé a intentarlo.");
            }

            if (ObtenerPorEmail(oUsuarioBE.Email) != null)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.Conflicto, "Ya existe una cuenta registrada con ese correo.");
            }

            ExigirDocumentoDisponible(oUsuarioBE);

            BERol oRolBE = oBLLRol.ObtenerPorNombre(Configuracion.RolPorDefecto);

            if (oRolBE == null)
            {
                throw new InvalidOperationException("El rol por defecto '" + Configuracion.RolPorDefecto + "' configurado en " + "Registro:RolPorDefecto no existe entre los roles activos de la tabla Rol.");
            }

            oEmpresaBE.Email = oUsuarioBE.Email;
            oEmpresaBE.Seleccionable = true;

            oBLLEmp.Guardar(oEmpresaBE);

            oUsuarioBE.EmpresaId = oEmpresaBE.EmpresaId;
            oUsuarioBE.Activo = false;

            oUsuarioBE.UsuarioId = oMPPUsu.Guardar(oUsuarioBE);

            AsignarRol(oUsuarioBE.UsuarioId, oRolBE.RolId);

            oBLLSus.AsignarPlanInicial(oUsuarioBE);

            BETokenSeguridad oTokenBE = GenerarToken(oUsuarioBE.UsuarioId, BLLTokenSeguridad.TipoConfirmacion);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oUsuarioBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloEmpresa,
                "Registro",
                "Alta de " + oEmpresaBE.RazonSocial + " (CUIT " + oEmpresaBE.CUIT + ") desde el registro público.",
                BLLBitacora.NivelInformativo));

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oUsuarioBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloUsuario,
                "Registro",
                "Alta de " + oUsuarioBE.Email + " con rol " + oRolBE.Nombre + " en " + oEmpresaBE.RazonSocial + ".",
                BLLBitacora.NivelInformativo));

            EnviarConfirmacionRegistro(oUsuarioBE, oTokenBE.Token);

            return oUsuarioBE;
        }

        public List<BEUsuarioAdministracion> ListarAdministracion(BESesion oSesionBE)
        {
            BEUsuario oFiltroBE = new BEUsuario();

            int? empresaQueLimita = EmpresaQueLimita(oSesionBE.UsuarioId);

            if (empresaQueLimita.HasValue)
            {
                oFiltroBE.EmpresaId = empresaQueLimita.Value;
            }

            List<BEUsuarioAdministracion> ListaUsuarioBE = oMPPUsu.ListarAdministracion(oFiltroBE);
            List<BEUsuarioAdministracion> ListaClienteBE = new List<BEUsuarioAdministracion>();

            if (ListaUsuarioBE == null)
            {
                return ListaClienteBE;
            }

            BEEmpresa oEmpresaInternaBE = ObtenerEmpresaInterna();

            foreach (BEUsuarioAdministracion oUsuarioAdministracionBE in ListaUsuarioBE)
            {
                if (oEmpresaInternaBE == null || oUsuarioAdministracionBE.Usuario.EmpresaId != oEmpresaInternaBE.EmpresaId)
                {
                    ListaClienteBE.Add(oUsuarioAdministracionBE);
                }
            }

            return ListaClienteBE;
        }

        public BEUsuario Invitar(BEUsuarioAdministracion Objeto, BESesion oSesionBE)
        {
            BEUsuario oUsuarioBE = Objeto.Usuario;

            ValidarDatos(oUsuarioBE);
            ExigirEmailDisponible(oUsuarioBE);
            ExigirDocumentoDisponible(oUsuarioBE);

            int? empresaQueLimita = EmpresaQueLimita(oSesionBE.UsuarioId);

            if (empresaQueLimita.HasValue)
            {
                oUsuarioBE.EmpresaId = empresaQueLimita.Value;
            }

            BEEmpresa oEmpresaBE = ObtenerEmpresaSeleccionable(oUsuarioBE.EmpresaId);
            BERol oRolBE = ObtenerRolObligatorio(Objeto.RolId);

            ValidarRolAsignable(oRolBE);

            oUsuarioBE.DepartamentoId = oBLLDep.ResolverParaUsuario(
                oUsuarioBE.DepartamentoId, oUsuarioBE.EmpresaId);

            CrearInvitacion(
                oUsuarioBE,
                oRolBE,
                BLLBitacora.ModuloUsuario,
                "Invitación enviada a " + oUsuarioBE.Email + " con rol " + oRolBE.Nombre +
                " en " + oEmpresaBE.RazonSocial + ".");

            return oUsuarioBE;
        }

        public void Reinvitar(BEUsuario Objeto, BESesion oSesionBE)
        {
            BEUsuario oUsuarioBE = ObtenerObligatorio(Objeto.UsuarioId);

            ExigirAlcanceSobre(oUsuarioBE, oSesionBE.UsuarioId);
            ExigirNoOperador(oUsuarioBE);

            ReenviarInvitacion(oUsuarioBE, BLLBitacora.ModuloUsuario);
        }

        /// <summary>
        /// ¿Ese correo ya está tomado? Devuelve true o false en vez de lanzar,
        /// porque el padrón necesita informar la fila, no abortar el archivo.
        /// </summary>
        public bool ExisteEmail(string email)
        {
            return ObtenerPorEmail(email) != null;
        }

        /// <summary>
        /// ¿Ese documento ya está tomado? Un documento vacío nunca lo está: es
        /// opcional y la unicidad solo se exige cuando viene cargado.
        /// </summary>
        public bool ExisteDocumento(string documento)
        {
            if (string.IsNullOrWhiteSpace(documento))
            {
                return false;
            }

            BEUsuario oFiltroBE = new BEUsuario();
            oFiltroBE.Documento = documento;

            return oMPPUsu.ObtenerPorDocumento(oFiltroBE) != null;
        }

        /// <summary>
        /// Alta por invitación de una fila del padrón (CU-001-002).
        ///
        /// A diferencia de <see cref="Invitar"/>, recibe la empresa y el rol ya
        /// resueltos: el padrón los resuelve una sola vez para todo el archivo,
        /// porque ObtenerEmpresaSeleccionable y ObtenerRolObligatorio releen el
        /// catálogo completo en cada llamada. Las validaciones de fila ya las
        /// hizo la BLL del padrón, que necesita reportarlas y no cortar.
        /// </summary>
        public BEUsuario InvitarDelPadron(BEUsuario oUsuarioBE, BERol oRolBE, string razonSocial)
        {
            CrearInvitacion(
                oUsuarioBE,
                oRolBE,
                BLLBitacora.ModuloUsuario,
                "Invitación enviada a " + oUsuarioBE.Email + " con rol " + oRolBE.Nombre +
                " en " + razonSocial + " desde el padrón.");

            return oUsuarioBE;
        }

        private void CrearInvitacion(BEUsuario oUsuarioBE, BERol oRolBE, string modulo, string descripcion)
        {
            oUsuarioBE.Activo = false;

            oUsuarioBE.UsuarioId = oMPPUsu.Guardar(oUsuarioBE);

            if (oRolBE != null)
            {
                AsignarRol(oUsuarioBE.UsuarioId, oRolBE.RolId);
            }

            BETokenSeguridad oTokenBE = GenerarToken(oUsuarioBE.UsuarioId, BLLTokenSeguridad.TipoInvitacion);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oUsuarioBE.UsuarioId,
                DateTime.Now,
                modulo,
                "Invitacion",
                descripcion,
                BLLBitacora.NivelInformativo));

            EnviarInvitacion(oUsuarioBE, oTokenBE.Token);
        }

        private void ReenviarInvitacion(BEUsuario oUsuarioBE, string modulo)
        {
            if (oUsuarioBE.Activo == true)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto,
                    "La cuenta de " + oUsuarioBE.Email + " ya está activa. " +
                    "Si olvidó su clave, puede pedir un enlace de recupero desde el ingreso.");
            }

            InvalidarTokensPendientes(oUsuarioBE.UsuarioId, BLLTokenSeguridad.TipoInvitacion);

            BETokenSeguridad oTokenBE = GenerarToken(oUsuarioBE.UsuarioId, BLLTokenSeguridad.TipoInvitacion);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oUsuarioBE.UsuarioId,
                DateTime.Now,
                modulo,
                "InvitacionReenviada",
                "Se reenvió la invitación a " + oUsuarioBE.Email + ".",
                BLLBitacora.NivelInformativo));

            EnviarInvitacion(oUsuarioBE, oTokenBE.Token);
        }

        public void CompletarInvitacion(BECompletarInvitacion Objeto)
        {
            BETokenSeguridad oConsumidoBE = ConsumirToken(Objeto.Token, BLLTokenSeguridad.TipoInvitacion);
            BEUsuario oUsuarioBE = ObtenerObligatorio(oConsumidoBE.UsuarioId);

            oUsuarioBE.PasswordHash = Objeto.Clave;

            oMPPUsu.ActualizarPasswordHash(oUsuarioBE);
            oMPPUsu.ActivarCuenta(oUsuarioBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oUsuarioBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloUsuario,
                "InvitacionCompletada",
                "La cuenta de " + oUsuarioBE.Email + " quedó activa.",
                BLLBitacora.NivelInformativo));

            EnviarBienvenida(oUsuarioBE);
        }

        public BEUsuario ModificarDesdeAdministracion(BEUsuarioAdministracion Objeto, BESesion oSesionBE)
        {
            BEUsuario oUsuarioBE = Objeto.Usuario;

            ValidarDatos(oUsuarioBE);

            BEUsuario oActualBE = ObtenerObligatorio(oUsuarioBE.UsuarioId);

            int? empresaQueLimita = ExigirAlcanceSobre(oActualBE, oSesionBE.UsuarioId);

            ExigirNoOperador(oActualBE);
            ExigirEmailDisponible(oUsuarioBE);
            ExigirDocumentoDisponible(oUsuarioBE);

            if (empresaQueLimita.HasValue)
            {
                oUsuarioBE.EmpresaId = empresaQueLimita.Value;
            }

            ObtenerEmpresaSeleccionable(oUsuarioBE.EmpresaId);
            BERol oRolBE = ObtenerRolObligatorio(Objeto.RolId);

            ValidarRolAsignable(oRolBE);

            oUsuarioBE.DepartamentoId = oBLLDep.ResolverParaUsuario(
                oUsuarioBE.DepartamentoId, oUsuarioBE.EmpresaId);
            oUsuarioBE.Idioma = oActualBE.Idioma;
            oUsuarioBE.NivelIdioma = oActualBE.NivelIdioma;

            // La licencia pertenece a la contratación de la empresa anterior: si
            // el usuario se muda, hay que revocarla y que la empresa nueva le
            // asigne una de su propio cupo (CU-001-005).
            if (oActualBE.EmpresaId != oUsuarioBE.EmpresaId)
            {
                oBLLLic.RevocarPorUsuario(oUsuarioBE.UsuarioId);
            }

            oMPPUsu.Guardar(oUsuarioBE);

            BEUsuarioRol oUsuarioRolBE = new BEUsuarioRol();
            oUsuarioRolBE.UsuarioId = oUsuarioBE.UsuarioId;
            oUsuarioRolBE.RolId = oRolBE.RolId;

            oMPPUsu.ReemplazarRol(oUsuarioRolBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oUsuarioBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloUsuario,
                "Modificacion",
                "Se actualizaron los datos de " + oUsuarioBE.Email + " (rol " + oRolBE.Nombre + ").",
                BLLBitacora.NivelInformativo));

            return ObtenerObligatorio(oUsuarioBE.UsuarioId);
        }

        public bool Baja(BEUsuario Objeto, BESesion oSesionBE)
        {
            BEUsuario oUsuarioBE = ObtenerObligatorio(Objeto.UsuarioId);

            ExigirAlcanceSobre(oUsuarioBE, oSesionBE.UsuarioId);
            ExigirNoOperador(oUsuarioBE);

            return Desactivar(oUsuarioBE, oSesionBE, BLLBitacora.ModuloUsuario);
        }

        public List<BEUsuarioAdministracion> ListarOperadores()
        {
            BEEmpresa oEmpresaBE = ObtenerEmpresaInterna();

            if (oEmpresaBE == null)
            {
                return new List<BEUsuarioAdministracion>();
            }

            BEUsuario oFiltroBE = new BEUsuario();
            oFiltroBE.EmpresaId = oEmpresaBE.EmpresaId;

            List<BEUsuarioAdministracion> ListaOperadorBE = oMPPUsu.ListarOperadores(oFiltroBE);

            return ListaOperadorBE == null ? new List<BEUsuarioAdministracion>() : ListaOperadorBE;
        }

        public BEUsuario InvitarOperador(BEUsuario Objeto, BESesion oSesionBE)
        {
            ValidarDatos(Objeto);
            ExigirEmailDisponible(Objeto);
            ExigirDocumentoDisponible(Objeto);

            BEEmpresa oEmpresaBE = ObtenerEmpresaInterna();

            if (oEmpresaBE == null)
            {
                throw new InvalidOperationException(
                    "No existe una empresa activa con el CUIT '" + Configuracion.SuperAdminEmpresaCuit +
                    "' configurado en SuperAdmin:EmpresaCuit: no se pueden crear operadores.");
            }

            Objeto.EmpresaId = oEmpresaBE.EmpresaId;

            CrearInvitacion(
                Objeto,
                null,
                BLLBitacora.ModuloOperador,
                "Invitación enviada al operador " + Objeto.Email + ".");

            return ObtenerObligatorio(Objeto.UsuarioId);
        }

        public BEUsuario ModificarOperador(BEUsuario Objeto, BESesion oSesionBE)
        {
            ValidarDatos(Objeto);

            BEUsuario oActualBE = ObtenerOperadorObligatorio(Objeto.UsuarioId);

            ExigirEmailDisponible(Objeto);
            ExigirDocumentoDisponible(Objeto);

            Objeto.EmpresaId = oActualBE.EmpresaId;
            Objeto.DepartamentoId = oActualBE.DepartamentoId;
            Objeto.Idioma = oActualBE.Idioma;
            Objeto.NivelIdioma = oActualBE.NivelIdioma;
            Objeto.FechaNacimiento = oActualBE.FechaNacimiento;

            oMPPUsu.Guardar(Objeto);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                Objeto.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloOperador,
                "Modificacion",
                "Se actualizaron los datos del operador " + Objeto.Email + ".",
                BLLBitacora.NivelInformativo));

            return ObtenerObligatorio(Objeto.UsuarioId);
        }

        public void ReinvitarOperador(BEUsuario Objeto, BESesion oSesionBE)
        {
            BEUsuario oUsuarioBE = ObtenerOperadorObligatorio(Objeto.UsuarioId);

            ReenviarInvitacion(oUsuarioBE, BLLBitacora.ModuloOperador);
        }

        public bool BajaOperador(BEUsuario Objeto, BESesion oSesionBE)
        {
            BEUsuario oUsuarioBE = ObtenerOperadorObligatorio(Objeto.UsuarioId);

            if (EsCuentaDePlataforma(oUsuarioBE))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto,
                    "La cuenta de plataforma configurada en SuperAdmin:Email no se puede desactivar.");
            }

            return Desactivar(oUsuarioBE, oSesionBE, BLLBitacora.ModuloOperador);
        }

        public List<BERol> ObtenerRoles(BEUsuario Objeto)
        {
            BEUsuario oUsuarioBE = ObtenerOperadorObligatorio(Objeto.UsuarioId);

            List<BERol> ListaRolBE = oMPPUsu.ObtenerRoles(oUsuarioBE);

            return ListaRolBE == null ? new List<BERol>() : ListaRolBE;
        }

        public bool AsignarRol(BEUsuarioRol Objeto, BESesion oSesionBE)
        {
            BEUsuario oUsuarioBE = ObtenerOperadorObligatorio(Objeto.UsuarioId);

            ExigirRolesAjenos(oUsuarioBE, oSesionBE);

            BERol oRolBE = ObtenerRolObligatorio(Objeto.RolId);

            if (TieneRol(oUsuarioBE, oRolBE.RolId))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto,
                    "El operador " + oUsuarioBE.Email + " ya tiene el rol " + oRolBE.Nombre + ".");
            }

            bool resultado = oMPPUsu.AsignarRol(Objeto);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oUsuarioBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloOperador,
                "AsignarRol",
                "Se asignó el rol " + oRolBE.Nombre + " al operador " + oUsuarioBE.Email + ".",
                BLLBitacora.NivelInformativo));

            return resultado;
        }

        public bool QuitarRol(BEUsuarioRol Objeto, BESesion oSesionBE)
        {
            BEUsuario oUsuarioBE = ObtenerOperadorObligatorio(Objeto.UsuarioId);

            ExigirRolesAjenos(oUsuarioBE, oSesionBE);

            BERol oRolBE = ObtenerRolObligatorio(Objeto.RolId);

            if (!TieneRol(oUsuarioBE, oRolBE.RolId))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.NoEncontrado,
                    "El operador " + oUsuarioBE.Email + " no tiene el rol " + oRolBE.Nombre + ".");
            }

            if (EsCuentaDePlataforma(oUsuarioBE) && EsRolDePlataforma(oRolBE))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto,
                    "La cuenta de plataforma configurada en SuperAdmin:Email no puede perder el rol " + oRolBE.Nombre + ".");
            }

            bool resultado = oMPPUsu.QuitarRol(Objeto);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oUsuarioBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloOperador,
                "QuitarRol",
                "Se quitó el rol " + oRolBE.Nombre + " al operador " + oUsuarioBE.Email + ".",
                BLLBitacora.NivelInformativo));

            return resultado;
        }

        private bool Desactivar(BEUsuario oUsuarioBE, BESesion oSesionBE, string modulo)
        {
            if (oUsuarioBE.UsuarioId == oSesionBE.UsuarioId)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto, "No podés desactivar tu propia cuenta.");
            }

            if (oUsuarioBE.Activo != true)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto, "La cuenta ya está desactivada.");
            }

            bool resultado = oMPPUsu.Baja(oUsuarioBE);

            BESesion oCerrarBE = new BESesion();
            oCerrarBE.UsuarioId = oUsuarioBE.UsuarioId;
            oCerrarBE.Token = Guid.Empty;

            oBLLSes.CerrarPorUsuario(oCerrarBE);

            // Una cuenta dada de baja no puede seguir ocupando un cupo del plan
            // (CU-001-005).
            int licenciasRevocadas = oBLLLic.RevocarPorUsuario(oUsuarioBE.UsuarioId);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oUsuarioBE.UsuarioId,
                DateTime.Now,
                modulo,
                "Baja",
                "Se desactivó la cuenta de " + oUsuarioBE.Email + " y se cerraron sus sesiones." +
                (licenciasRevocadas > 0 ? " Se liberó su licencia." : ""),
                BLLBitacora.NivelInformativo));

            return resultado;
        }

        private int? EmpresaQueLimita(int usuarioQueOpera)
        {
            if (oBLLSeg.Tiene(usuarioQueOpera, Permisos.UsuarioVerTodasLasEmpresas))
            {
                return null;
            }

            return ObtenerObligatorio(usuarioQueOpera).EmpresaId;
        }

        private int? ExigirAlcanceSobre(BEUsuario oUsuarioBE, int usuarioQueOpera)
        {
            int? empresaQueLimita = EmpresaQueLimita(usuarioQueOpera);

            if (empresaQueLimita.HasValue && oUsuarioBE.EmpresaId != empresaQueLimita.Value)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.NoEncontrado, "El usuario no existe.");
            }

            return empresaQueLimita;
        }

        private void ValidarRolAsignable(BERol oRolBE)
        {
            if (EsRolDePlataforma(oRolBE))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto,
                    "El rol " + oRolBE.Nombre + " es exclusivo de los operadores de la plataforma: se asigna desde Operadores.");
            }
        }

        private bool EsRolDePlataforma(BERol oRolBE)
        {
            return string.Equals(
                oRolBE.Nombre.Trim(),
                Configuracion.SuperAdminRol.Trim(),
                StringComparison.OrdinalIgnoreCase);
        }

        private BEEmpresa ObtenerEmpresaInterna()
        {
            return oBLLEmp.ObtenerPorCuit(Configuracion.SuperAdminEmpresaCuit);
        }

        private bool EsOperador(BEUsuario oUsuarioBE)
        {
            BEEmpresa oEmpresaBE = ObtenerEmpresaInterna();

            return oEmpresaBE != null && oUsuarioBE.EmpresaId == oEmpresaBE.EmpresaId;
        }

        private void ExigirNoOperador(BEUsuario oUsuarioBE)
        {
            if (EsOperador(oUsuarioBE))
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.NoEncontrado, "El usuario no existe.");
            }
        }

        private BEUsuario ObtenerOperadorObligatorio(int usuarioId)
        {
            BEUsuario oUsuarioBE = ObtenerObligatorio(usuarioId);

            if (!EsOperador(oUsuarioBE))
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.NoEncontrado, "El operador no existe.");
            }

            return oUsuarioBE;
        }

        private bool EsCuentaDePlataforma(BEUsuario oUsuarioBE)
        {
            return !string.IsNullOrWhiteSpace(Configuracion.SuperAdminEmail)
                && string.Equals(
                    oUsuarioBE.Email.Trim(),
                    Configuracion.SuperAdminEmail.Trim(),
                    StringComparison.OrdinalIgnoreCase);
        }

        private void ExigirRolesAjenos(BEUsuario oUsuarioBE, BESesion oSesionBE)
        {
            if (oUsuarioBE.UsuarioId == oSesionBE.UsuarioId)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto,
                    "No podés modificar tus propios roles: pedíselo a otro operador de la plataforma.");
            }
        }

        private bool TieneRol(BEUsuario oUsuarioBE, int rolId)
        {
            List<BERol> ListaRolBE = oMPPUsu.ObtenerRoles(oUsuarioBE);

            if (ListaRolBE == null)
            {
                return false;
            }

            foreach (BERol oRolBE in ListaRolBE)
            {
                if (oRolBE.RolId == rolId)
                {
                    return true;
                }
            }

            return false;
        }

        private void ExigirEmailDisponible(BEUsuario Objeto)
        {
            BEUsuario oDelEmailBE = ObtenerPorEmail(Objeto.Email);

            if (oDelEmailBE != null && oDelEmailBE.UsuarioId != Objeto.UsuarioId)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto, "Ya existe una cuenta registrada con ese correo.");
            }
        }

        /// <summary>
        /// El documento es opcional, así que solo se exige unicidad cuando viene
        /// cargado. La búsqueda va por la huella determinística: el documento se
        /// guarda cifrado y no se puede comparar en SQL.
        /// </summary>
        private void ExigirDocumentoDisponible(BEUsuario Objeto)
        {
            if (string.IsNullOrWhiteSpace(Objeto.Documento))
            {
                return;
            }

            BEUsuario oFiltroBE = new BEUsuario();
            oFiltroBE.Documento = Objeto.Documento;

            BEUsuario oDelDocumentoBE = oMPPUsu.ObtenerPorDocumento(oFiltroBE);

            if (oDelDocumentoBE != null && oDelDocumentoBE.UsuarioId != Objeto.UsuarioId)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto, "Ya existe una cuenta registrada con ese documento.");
            }
        }

        private BEEmpresa ObtenerEmpresaSeleccionable(int empresaId)
        {
            List<BEEmpresa> ListaEmpresaBE = oBLLEmp.ListarSeleccionables();

            foreach (BEEmpresa oEmpresaBE in ListaEmpresaBE)
            {
                if (oEmpresaBE.EmpresaId == empresaId)
                {
                    return oEmpresaBE;
                }
            }

            throw new ExcepcionNegocio(
                TipoErrorNegocio.Validacion,
                "La empresa seleccionada no existe o no está disponible para asignar usuarios.");
        }

        private BERol ObtenerRolObligatorio(int rolId)
        {
            List<BERol> ListaRolBE = oBLLRol.ListarTodo();

            foreach (BERol oRolBE in ListaRolBE)
            {
                if (oRolBE.RolId == rolId)
                {
                    return oRolBE;
                }
            }

            throw new ExcepcionNegocio(
                TipoErrorNegocio.Validacion, "El rol seleccionado no existe.");
        }

        private void AsignarRol(int usuarioId, int rolId)
        {
            BEUsuarioRol oUsuarioRolBE = new BEUsuarioRol();

            oUsuarioRolBE.UsuarioId = usuarioId;
            oUsuarioRolBE.RolId = rolId;

            oMPPUsu.AsignarRol(oUsuarioRolBE);
        }

        private void ValidarDatos(BEUsuario Objeto)
        {
            if (string.IsNullOrWhiteSpace(Objeto.Nombre) || string.IsNullOrWhiteSpace(Objeto.Apellido))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "El nombre y el apellido son obligatorios.");
            }

            if (string.IsNullOrWhiteSpace(Objeto.Email) || !Objeto.Email.Contains('@'))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "El correo no tiene un formato válido.");
            }
        }

        private void EnviarInvitacion(BEUsuario Objeto, Guid token)
        {
            try
            {
                oServicioEmail.EnviarInvitacion(Objeto, token);
            }
            catch (Exception ex)
            {
                RegistrarFalloEmail("Invitacion", Objeto.UsuarioId, ex);
            }
        }

        public void ConfirmarCuenta(BETokenSeguridad Objeto)
        {
            BETokenSeguridad oConsumidoBE = ConsumirToken(Objeto.Token, BLLTokenSeguridad.TipoConfirmacion);
            BEUsuario oUsuarioBE = ObtenerObligatorio(oConsumidoBE.UsuarioId);

            oMPPUsu.ActivarCuenta(oUsuarioBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oUsuarioBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloUsuario,
                "ConfirmacionCuenta",
                "Cuenta de " + oUsuarioBE.Email + " confirmada.",
                BLLBitacora.NivelInformativo));

            EnviarBienvenida(oUsuarioBE);
        }

        public BEResultadoLogin Login(BELogin Objeto, BESesion oSesionBE)
        {
            BEUsuario oUsuarioBE = ObtenerPorEmail(Objeto.Email);

            // El bloqueo se mira ANTES de validar la clave: una cuenta bloqueada
            // no entra ni con la clave correcta, que es justamente el punto
            // (CU-003-004).
            ExigirNoBloqueada(oUsuarioBE);

            if (oUsuarioBE == null || oSesionBE.UsuarioId != oUsuarioBE.UsuarioId)
            {
                oBLLBit.Guardar(new BEBitacoraEvento(
                    0,
                    oUsuarioBE == null ? null : (int?)oUsuarioBE.UsuarioId,
                    DateTime.Now,
                    BLLBitacora.ModuloAutenticacion,
                    "LoginFallido",
                    "Intento fallido para " + Objeto.Email + ".",
                    BLLBitacora.NivelAdvertencia));

                // Si el correo no existe no hay fila donde contar, y la
                // respuesta es la misma de siempre para no delatar qué cuentas
                // existen.
                if (oUsuarioBE != null)
                {
                    ContarIntentoFallido(oUsuarioBE);
                }

                throw new ExcepcionNegocio(
                    TipoErrorNegocio.CredencialesInvalidas, "Correo o clave incorrectos.");
            }

            if (oUsuarioBE.Activo != true)
            {
                oBLLBit.Guardar(new BEBitacoraEvento(
                    0,
                    oUsuarioBE.UsuarioId,
                    DateTime.Now,
                    BLLBitacora.ModuloAutenticacion,
                    "LoginSinConfirmar",
                    "Login rechazado: " + Objeto.Email + " no confirmó su cuenta.",
                    "WARN"));

                throw new ExcepcionNegocio(
                    TipoErrorNegocio.CuentaNoConfirmada,
                    "Tu cuenta todavía no fue confirmada. Revisá el correo de confirmación que te enviamos.");
            }

            BESesion oSesionCreadaBE = oBLLSes.Guardar(oSesionBE);

            oMPPUsu.ActualizarUltimoAcceso(oUsuarioBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oUsuarioBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloAutenticacion,
                "Login",
                "Inicio de sesión de " + oUsuarioBE.Email + ".",
                BLLBitacora.NivelInformativo));

            return ArmarResultado(oUsuarioBE, oSesionCreadaBE.Token);
        }

        /// <summary>
        /// Corta el login si la cuenta está bloqueada y el bloqueo sigue vigente.
        ///
        /// Un bloqueo vencido no lo limpia ningún proceso de fondo: simplemente
        /// deja de cumplirse esta condición y el login sigue normal. El contador
        /// se reinicia solo cuando la persona entra bien.
        /// </summary>
        private void ExigirNoBloqueada(BEUsuario oUsuarioBE)
        {
            if (oUsuarioBE == null || !EstaBloqueada(oUsuarioBE))
            {
                return;
            }

            throw new ExcepcionNegocio(TipoErrorNegocio.CuentaBloqueada, MensajeBloqueo(oUsuarioBE));
        }

        private bool EstaBloqueada(BEUsuario oUsuarioBE)
        {
            return oUsuarioBE.BloqueadoHasta.HasValue && oUsuarioBE.BloqueadoHasta.Value > DateTime.Now;
        }

        /// <summary>
        /// El mensaje del paso 13 del caso de uso, sin la parte del correo: en
        /// este alcance no se manda alerta al titular.
        /// </summary>
        private string MensajeBloqueo(BEUsuario oUsuarioBE)
        {
            int minutos = (int)Math.Ceiling((oUsuarioBE.BloqueadoHasta.Value - DateTime.Now).TotalMinutes);

            if (minutos < 1)
            {
                minutos = 1;
            }

            return "Cuenta bloqueada temporalmente por múltiples intentos fallidos. " +
                "Volvé a intentar en " + minutos + " minuto" + (minutos == 1 ? "" : "s") +
                " o pedile a un administrador que la desbloquee.";
        }

        /// <summary>
        /// Suma el intento y, si ese intento fue el que disparó el bloqueo, lo
        /// informa como tal en vez de como credenciales inválidas: si no, la
        /// persona se entera recién en el intento siguiente.
        /// </summary>
        private void ContarIntentoFallido(BEUsuario oUsuarioBE)
        {
            BEUsuario oEstadoBE = oMPPUsu.RegistrarIntentoFallido(
                oUsuarioBE, Configuracion.IntentosMaximos, Configuracion.MinutosBloqueo);

            if (oEstadoBE == null || !EstaBloqueada(oEstadoBE))
            {
                return;
            }

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oUsuarioBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloSeguridad,
                "CuentaBloqueada",
                "Se bloqueó la cuenta de " + oUsuarioBE.Email + " tras " + oEstadoBE.IntentosFallidos +
                " intentos fallidos. Queda bloqueada hasta " + oEstadoBE.BloqueadoHasta.Value + ".",
                BLLBitacora.NivelAdvertencia));

            throw new ExcepcionNegocio(TipoErrorNegocio.CuentaBloqueada, MensajeBloqueo(oEstadoBE));
        }

        /// <summary>
        /// Desbloqueo manual por un administrador (camino alternativo 4):
        /// reinicia el contador y rehabilita el ingreso.
        /// </summary>
        public BEUsuario Desbloquear(BEUsuario Objeto, BESesion oSesionBE)
        {
            BEUsuario oUsuarioBE = ObtenerObligatorio(Objeto.UsuarioId);

            ExigirAlcanceSobre(oUsuarioBE, oSesionBE.UsuarioId);
            ExigirNoOperador(oUsuarioBE);

            return Liberar(oUsuarioBE, BLLBitacora.ModuloUsuario);
        }

        /// <summary>Lo mismo para los operadores, que tienen su propia pantalla.</summary>
        public BEUsuario DesbloquearOperador(BEUsuario Objeto, BESesion oSesionBE)
        {
            BEUsuario oUsuarioBE = ObtenerOperadorObligatorio(Objeto.UsuarioId);

            return Liberar(oUsuarioBE, BLLBitacora.ModuloOperador);
        }

        private BEUsuario Liberar(BEUsuario oUsuarioBE, string modulo)
        {
            if (!EstaBloqueada(oUsuarioBE) && oUsuarioBE.IntentosFallidos == 0)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto, "La cuenta no está bloqueada.");
            }

            oMPPUsu.Desbloquear(oUsuarioBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oUsuarioBE.UsuarioId,
                DateTime.Now,
                modulo,
                "Desbloqueo",
                "Se restauró el acceso de " + oUsuarioBE.Email + " y se reinició el contador de intentos.",
                BLLBitacora.NivelInformativo));

            return ObtenerObligatorio(oUsuarioBE.UsuarioId);
        }

        public BEResultadoLogin ObtenerSesion(BESesion Objeto)
        {
            BESesion oSesionBE = oBLLSes.ObtenerActiva(Objeto.Token);
            BEUsuario oUsuarioBE = ObtenerObligatorio(oSesionBE.UsuarioId);

            if (oUsuarioBE.Activo != true)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.SesionInvalida, "La sesión ya no es válida. Volvé a iniciar sesión.");
            }

            return ArmarResultado(oUsuarioBE, oSesionBE.Token);
        }

        private BEResultadoLogin ArmarResultado(BEUsuario oUsuarioBE, Guid token)
        {
            List<BERol> ListaRolBE = oMPPUsu.ObtenerRoles(oUsuarioBE);
            List<string> roles = new List<string>();

            if (ListaRolBE != null)
            {
                foreach (BERol oRolBE in ListaRolBE)
                {
                    roles.Add(oRolBE.Nombre);
                }
            }

            return new BEResultadoLogin(
                token,
                oUsuarioBE.UsuarioId,
                oUsuarioBE.Nombre,
                oUsuarioBE.Apellido,
                oUsuarioBE.Email,
                roles,
                oBLLSeg.ObtenerPermisos(oUsuarioBE.UsuarioId));
        }

        /// <summary>
        /// Perfil del usuario de la sesión (CU-001-003). El id sale siempre de la
        /// sesión, así que nadie puede consultar el perfil de otro por acá.
        /// </summary>
        public BEPerfilRespuesta ObtenerPerfil(BESesion oSesionBE)
        {
            return ArmarPerfil(ObtenerObligatorio(oSesionBE.UsuarioId));
        }

        /// <summary>
        /// Autogestión de los datos personales (CU-001-003). Solo cambian nombre,
        /// apellido y fecha de nacimiento: el correo es el usuario de ingreso y el
        /// documento es único, así que esos los modifica un administrador.
        /// </summary>
        public void ModificarPerfil(BEUsuario Objeto, BESesion oSesionBE)
        {
            BEUsuario oActualBE = ObtenerObligatorio(oSesionBE.UsuarioId);

            ValidarDatosPerfil(Objeto);

            oActualBE.Nombre = Objeto.Nombre.Trim();
            oActualBE.Apellido = Objeto.Apellido.Trim();
            oActualBE.FechaNacimiento = Objeto.FechaNacimiento;

            oMPPUsu.Guardar(oActualBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oActualBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloUsuario,
                "ModificacionPerfil",
                "El usuario " + oActualBE.Email + " actualizó sus datos de perfil.",
                BLLBitacora.NivelInformativo));
        }

        private void ValidarDatosPerfil(BEUsuario Objeto)
        {
            if (string.IsNullOrWhiteSpace(Objeto.Nombre) || string.IsNullOrWhiteSpace(Objeto.Apellido))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "El nombre y el apellido son obligatorios.");
            }

            if (Objeto.Nombre.Trim().Length > LongitudMaximaNombre ||
                Objeto.Apellido.Trim().Length > LongitudMaximaNombre)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El nombre y el apellido no pueden superar los " + LongitudMaximaNombre + " caracteres.");
            }

            if (Objeto.FechaNacimiento.HasValue)
            {
                DateOnly fecha = Objeto.FechaNacimiento.Value;

                if (fecha > DateOnly.FromDateTime(DateTime.Today) || fecha < FechaNacimientoMinima)
                {
                    throw new ExcepcionNegocio(
                        TipoErrorNegocio.Validacion, "La fecha de nacimiento no es válida.");
                }
            }
        }

        private BEPerfilRespuesta ArmarPerfil(BEUsuario oUsuarioBE)
        {
            BEPerfilRespuesta oPerfil = new BEPerfilRespuesta();

            oPerfil.UsuarioId = oUsuarioBE.UsuarioId;
            oPerfil.Nombre = oUsuarioBE.Nombre;
            oPerfil.Apellido = oUsuarioBE.Apellido;
            oPerfil.FechaNacimiento = oUsuarioBE.FechaNacimiento;
            oPerfil.Email = oUsuarioBE.Email;
            oPerfil.Documento = oUsuarioBE.Documento;

            BEEmpresa oFiltroEmpresaBE = new BEEmpresa();
            oFiltroEmpresaBE.EmpresaId = oUsuarioBE.EmpresaId;
            oPerfil.Empresa = oBLLEmp.ListarObjeto(oFiltroEmpresaBE).RazonSocial;

            if (oUsuarioBE.DepartamentoId.HasValue)
            {
                BEDepartamento oFiltroDepartamentoBE = new BEDepartamento();
                oFiltroDepartamentoBE.DepartamentoId = oUsuarioBE.DepartamentoId.Value;
                oPerfil.Departamento = oBLLDep.ListarObjeto(oFiltroDepartamentoBE).Nombre;
            }

            List<BERol> ListaRolBE = oMPPUsu.ObtenerRoles(oUsuarioBE);

            if (ListaRolBE != null)
            {
                foreach (BERol oRolBE in ListaRolBE)
                {
                    oPerfil.Roles.Add(oRolBE.Nombre);
                }
            }

            return oPerfil;
        }

        public void CerrarSesion(Guid token)
        {
            BESesion oSesionBE = oBLLSes.ObtenerActiva(token);

            oBLLSes.Baja(token);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloAutenticacion,
                "Logout",
                "Cierre de sesión.",
                BLLBitacora.NivelInformativo));
        }

        public void SolicitarRecuperoClave(BERecuperarClave Objeto)
        {
            BEUsuario oUsuarioBE = ObtenerPorEmail(Objeto.Email);

            if (oUsuarioBE == null || oUsuarioBE.Activo != true)
            {
                oBLLBit.Guardar(new BEBitacoraEvento(
                    0,
                    oUsuarioBE == null ? null : (int?)oUsuarioBE.UsuarioId,
                    DateTime.Now,
                    BLLBitacora.ModuloAutenticacion,
                    "RecuperoClaveIgnorado",
                    "Pedido para " + Objeto.Email + ": la cuenta no existe o no está activa.",
                    "WARN"));

                return;
            }

            InvalidarTokensPendientes(oUsuarioBE.UsuarioId, BLLTokenSeguridad.TipoRecupero);

            BETokenSeguridad oTokenBE = GenerarToken(oUsuarioBE.UsuarioId, BLLTokenSeguridad.TipoRecupero);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oUsuarioBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloAutenticacion,
                "RecuperoClaveSolicitado",
                "Enlace de recupero generado para " + oUsuarioBE.Email + ".",
                BLLBitacora.NivelInformativo));

            EnviarRecuperoClave(oUsuarioBE, oTokenBE.Token);
        }

        public void RestablecerClave(BERestablecerClave Objeto)
        {
            BETokenSeguridad oConsumidoBE = ConsumirToken(Objeto.Token, BLLTokenSeguridad.TipoRecupero);
            BEUsuario oUsuarioBE = ObtenerObligatorio(oConsumidoBE.UsuarioId);

            AplicarClaveNueva(oUsuarioBE, Objeto.ClaveNueva, Guid.Empty, "RestablecerClave");
        }

        public void CambiarClave(BECambiarClave Objeto, BESesion oSesionBE)
        {
            BEUsuario oUsuarioBE = ObtenerObligatorio(oSesionBE.UsuarioId);

            if (Objeto.ClaveActual == null)
            {
                oBLLBit.Guardar(new BEBitacoraEvento(
                    0,
                    oUsuarioBE.UsuarioId,
                    DateTime.Now,
                    BLLBitacora.ModuloAutenticacion,
                    "CambioClaveRechazado",
                    "La clave actual no coincide.",
                    "WARN"));

                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "La clave actual no es correcta.");
            }

            if (Objeto.ClaveNueva == null)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "La clave nueva debe ser distinta de la actual.");
            }

            AplicarClaveNueva(oUsuarioBE, Objeto.ClaveNueva, oSesionBE.Token, "CambiarClave");
        }

        private void AplicarClaveNueva(
            BEUsuario Objeto,
            string passwordHash,
            Guid sesionAConservar,
            string accion)
        {
            Objeto.PasswordHash = passwordHash;

            oMPPUsu.ActualizarPasswordHash(Objeto);
            InvalidarTokensPendientes(Objeto.UsuarioId, BLLTokenSeguridad.TipoRecupero);

            BESesion oCerrarBE = new BESesion();
            oCerrarBE.UsuarioId = Objeto.UsuarioId;
            oCerrarBE.Token = sesionAConservar;

            int sesionesCerradas = oBLLSes.CerrarPorUsuario(oCerrarBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                Objeto.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloAutenticacion,
                accion,
                "Clave modificada. Sesiones cerradas: " + sesionesCerradas + ".",
                BLLBitacora.NivelInformativo));

            EnviarClaveModificada(Objeto, accion);
        }

        private BETokenSeguridad GenerarToken(int usuarioId, string tipo)
        {
            BETokenSeguridad oTokenBE = new BETokenSeguridad();

            oTokenBE.UsuarioId = usuarioId;
            oTokenBE.Tipo = tipo;

            return oBLLTok.Guardar(oTokenBE);
        }

        private int InvalidarTokensPendientes(int usuarioId, string tipo)
        {
            BETokenSeguridad oTokenBE = new BETokenSeguridad();

            oTokenBE.UsuarioId = usuarioId;
            oTokenBE.Tipo = tipo;

            return oBLLTok.InvalidarPorUsuarioYTipo(oTokenBE);
        }

        private BETokenSeguridad ConsumirToken(Guid token, string tipoEsperado)
        {
            BETokenSeguridad oTokenBE = new BETokenSeguridad();

            oTokenBE.Token = token;
            oTokenBE.Tipo = tipoEsperado;

            return oBLLTok.Consumir(oTokenBE);
        }

        public BEUsuario ObtenerPorEmail(BEUsuario Objeto)
        {
            return oMPPUsu.ObtenerPorEmail(Objeto);
        }

        private BEUsuario ObtenerPorEmail(string email)
        {
            BEUsuario oFiltroBE = new BEUsuario();
            oFiltroBE.Email = email;

            return ObtenerPorEmail(oFiltroBE);
        }

        public BEUsuario ObtenerPorId(int usuarioId)
        {
            return ObtenerObligatorio(usuarioId);
        }

        private BEUsuario ObtenerObligatorio(int usuarioId)
        {
            BEUsuario oFiltroBE = new BEUsuario();
            oFiltroBE.UsuarioId = usuarioId;

            BEUsuario oUsuarioBE = oMPPUsu.ListarObjeto(oFiltroBE);

            if (oUsuarioBE == null)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.NoEncontrado, "El usuario no existe.");
            }

            return oUsuarioBE;
        }

        public void ValidarClave(string clave)
        {
            if (string.IsNullOrWhiteSpace(clave) || clave.Length < Configuracion.LongitudMinimaClave)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "La clave debe tener al menos " + Configuracion.LongitudMinimaClave + " caracteres.");
            }

            bool tieneLetra = false;
            bool tieneDigito = false;

            foreach (char caracter in clave)
            {
                if (char.IsLetter(caracter))
                {
                    tieneLetra = true;
                }

                if (char.IsDigit(caracter))
                {
                    tieneDigito = true;
                }
            }

            if (!tieneLetra || !tieneDigito)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "La clave debe combinar letras y números.");
            }
        }

        private void EnviarConfirmacionRegistro(BEUsuario Objeto, Guid token)
        {
            try
            {
                oServicioEmail.EnviarConfirmacionRegistro(Objeto, token);
            }
            catch (Exception ex)
            {
                RegistrarFalloEmail("Registro", Objeto.UsuarioId, ex);
            }
        }

        private void EnviarBienvenida(BEUsuario Objeto)
        {
            try
            {
                oServicioEmail.EnviarBienvenida(Objeto);
            }
            catch (Exception ex)
            {
                RegistrarFalloEmail("ConfirmacionCuenta", Objeto.UsuarioId, ex);
            }
        }

        private void EnviarRecuperoClave(BEUsuario Objeto, Guid token)
        {
            try
            {
                oServicioEmail.EnviarRecuperoClave(Objeto, token);
            }
            catch (Exception ex)
            {
                RegistrarFalloEmail("RecuperoClaveSolicitado", Objeto.UsuarioId, ex);
            }
        }

        private void EnviarClaveModificada(BEUsuario Objeto, string accion)
        {
            try
            {
                oServicioEmail.EnviarClaveModificada(Objeto);
            }
            catch (Exception ex)
            {
                RegistrarFalloEmail(accion, Objeto.UsuarioId, ex);
            }
        }

        private void RegistrarFalloEmail(string accion, int usuarioId, Exception ex)
        {
            ServicioLog.Error(
                "Falló el envío del email de '" + accion + "' para el usuario " + usuarioId + ".", ex);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                usuarioId,
                DateTime.Now,
                BLLBitacora.ModuloAutenticacion,
                accion + ":EmailNoEnviado",
                ex.Message,
                "ERROR"));
        }
    }
}
