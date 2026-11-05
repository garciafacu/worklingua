using worklingua.Server.BE;
using worklingua.Server.MPP;
using worklingua.Server.Services;

namespace worklingua.Server.BLL
{
    public class BLLSeguridad
    {
        MPPUsuario oMPPUsu = new MPPUsuario();
        BLLPermiso oBLLPer = new BLLPermiso();
        BLLSesion oBLLSes = new BLLSesion();
        BLLBitacora oBLLBit = new BLLBitacora();

        public List<string> ObtenerPermisos(int usuarioId)
        {
            List<string> codigos = new List<string>();

            foreach (BEPermiso oPermisoBE in ObtenerRaices(usuarioId))
            {
                AgregarNombres(oPermisoBE, codigos);
            }

            codigos.Sort(StringComparer.Ordinal);

            return codigos;
        }

        public bool Tiene(int usuarioId, string permiso)
        {
            foreach (BEPermiso oPermisoBE in ObtenerRaices(usuarioId))
            {
                if (oPermisoBE.BuscarPermisoPorNombre(permiso) != null)
                {
                    return true;
                }
            }

            return false;
        }

        public int? EmpresaQueLimita(BESesion oSesionBE, string permisoTodasLasEmpresas)
        {
            if (Tiene(oSesionBE.UsuarioId, permisoTodasLasEmpresas))
            {
                return null;
            }

            BEUsuario oFiltroBE = new BEUsuario();
            oFiltroBE.UsuarioId = oSesionBE.UsuarioId;

            BEUsuario oUsuarioBE = oMPPUsu.ListarObjeto(oFiltroBE);

            if (oUsuarioBE == null)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.SesionInvalida, "El usuario de la sesión no existe.");
            }

            return oUsuarioBE.EmpresaId;
        }

        private List<BEPermiso> ObtenerRaices(int usuarioId)
        {
            BEUsuario oFiltroBE = new BEUsuario();
            oFiltroBE.UsuarioId = usuarioId;

            return oBLLPer.ObtenerAsignados(oMPPUsu.ObtenerPermisos(oFiltroBE));
        }

        private void AgregarNombres(BEPermiso oPermisoBE, List<string> codigos)
        {
            if (codigos.Contains(oPermisoBE.Nombre))
            {
                return;
            }

            codigos.Add(oPermisoBE.Nombre);

            foreach (BEPermiso oHijoBE in oPermisoBE.ObtenerPermisosHijos())
            {
                AgregarNombres(oHijoBE, codigos);
            }
        }

        public BESesion ExigirPermiso(Guid token, string permiso)
        {
            BESesion oSesionBE = oBLLSes.ObtenerActiva(token);

            if (Tiene(oSesionBE.UsuarioId, permiso))
            {
                return oSesionBE;
            }

            oBLLBit.Guardar(new BEBitacoraEvento(
                0, oSesionBE.UsuarioId, DateTime.Now, BLLBitacora.ModuloSeguridad, "PermisoDenegado",
                "Falta el permiso " + permiso + ".", BLLBitacora.NivelAdvertencia));

            throw new ExcepcionNegocio(
                TipoErrorNegocio.PermisoDenegado, "No tenés permiso para realizar esta acción.");
        }
    }

    public class BLLSesion
    {
        MPPSesion oMPPSes = new MPPSesion();
        BLLBitacora oBLLBit = new BLLBitacora();

        public BESesion Guardar(BESesion Objeto)
        {
            return oMPPSes.Guardar(Objeto);
        }

        public BESesion ObtenerActiva(Guid token)
        {
            BESesion oFiltroBE = new BESesion();
            oFiltroBE.Token = token;

            BESesion oSesionBE = oMPPSes.ListarObjeto(oFiltroBE);

            if (oSesionBE == null || oSesionBE.Activa != true)
            {
                oBLLBit.Guardar(new BEBitacoraEvento(
                    0, oSesionBE == null ? null : (int?)oSesionBE.UsuarioId, DateTime.Now, BLLBitacora.ModuloSeguridad,
                    "SesionInvalida",
                    oSesionBE == null ? "Pedido con un token de sesión que no existe." : "Pedido con un token de una sesión ya cerrada.",
                    BLLBitacora.NivelAdvertencia));

                throw new ExcepcionNegocio(
                    TipoErrorNegocio.SesionInvalida, "La sesión no es válida o ya fue cerrada.");
            }

            return oSesionBE;
        }

        public bool Baja(Guid token)
        {
            BESesion oSesionBE = new BESesion();
            oSesionBE.Token = token;

            return oMPPSes.Baja(oSesionBE);
        }

        public int CerrarPorUsuario(BESesion Objeto)
        {
            int cerradas = oMPPSes.CerrarPorUsuario(Objeto);

            if (cerradas > 0)
            {
                oBLLBit.Guardar(new BEBitacoraEvento(
                    0, Objeto.UsuarioId, DateTime.Now, BLLBitacora.ModuloSeguridad, "SesionesCerradas",
                    "Se invalidaron " + cerradas + " sesiones abiertas de la cuenta.", BLLBitacora.NivelAdvertencia));
            }

            return cerradas;
        }
    }

    public class BLLTokenSeguridad
    {
        public const string TipoConfirmacion = "CONFIRMACION";
        public const string TipoRecupero = "RECUPERO";
        public const string TipoInvitacion = "INVITACION";

        MPPTokenSeguridad oMPPTok = new MPPTokenSeguridad();

        public BETokenSeguridad Guardar(BETokenSeguridad Objeto)
        {
            int horas = HorasVigencia(Objeto.Tipo);

            Objeto.FechaGeneracion = DateTime.Now;
            Objeto.FechaExpiracion = DateTime.Now.AddHours(horas);

            return oMPPTok.Guardar(Objeto);
        }

        private int HorasVigencia(string tipo)
        {
            if (tipo == TipoConfirmacion)
            {
                return Configuracion.HorasVigenciaConfirmacion;
            }

            if (tipo == TipoInvitacion)
            {
                return Configuracion.HorasVigenciaInvitacion;
            }

            return Configuracion.HorasVigenciaRecupero;
        }

        public int InvalidarPorUsuarioYTipo(BETokenSeguridad Objeto)
        {
            return oMPPTok.InvalidarPorUsuarioYTipo(Objeto);
        }

        public BETokenSeguridad Consumir(BETokenSeguridad Objeto)
        {
            BETokenSeguridad oAlmacenadoBE = oMPPTok.ListarObjeto(Objeto);

            bool invalido = oAlmacenadoBE == null
                || oAlmacenadoBE.Tipo != Objeto.Tipo
                || !oAlmacenadoBE.Activo
                || oAlmacenadoBE.FechaUso.HasValue
                || oAlmacenadoBE.FechaExpiracion <= DateTime.Now;

            if (invalido || !oMPPTok.MarcarUsado(Objeto))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El enlace no es válido, ya fue utilizado o venció. Solicitá uno nuevo.");
            }

            return oAlmacenadoBE;
        }
    }

    public enum TipoErrorNegocio
    {
        Validacion,
        CredencialesInvalidas,
        CuentaNoConfirmada,
        CuentaBloqueada,
        SesionInvalida,
        PermisoDenegado,
        NoEncontrado,
        Conflicto
    }

    public class ExcepcionNegocio : Exception
    {
        public ExcepcionNegocio(TipoErrorNegocio tipo, string mensaje) : base(mensaje)
        {
            this.Tipo = tipo;
        }

        public ExcepcionNegocio(TipoErrorNegocio tipo, string mensaje, string sugerencia)
            : this(tipo, mensaje)
        {
            this.Sugerencia = sugerencia;
        }

        public TipoErrorNegocio Tipo { get; }

        public string Sugerencia { get; }
    }

    public static class ReglasNegocio
    {
        public static void ExigirTexto(string valor, string mensaje)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.Validacion, mensaje);
            }
        }

        public static void ExigirLargoMaximo(string valor, int maximo, string mensaje)
        {
            if (valor != null && valor.Length > maximo)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.Validacion, mensaje);
            }
        }

        public static void ExigirConLargoMaximo(string valor, int maximo, string mensaje)
        {
            if (valor == null || valor.Length > maximo)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.Validacion, mensaje);
            }
        }

        public static void ExigirLargo(string valor, int minimo, int maximo, string mensaje)
        {
            if (valor != null && (valor.Length < minimo || valor.Length > maximo))
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.Validacion, mensaje);
            }
        }

        public static void ExigirValor(object valor, string mensaje)
        {
            if (valor == null)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.Validacion, mensaje);
            }
        }

        public static void ExigirRango(int? valor, int minimo, int maximo, string mensaje)
        {
            if (valor.HasValue && (valor.Value < minimo || valor.Value > maximo))
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.Validacion, mensaje);
            }
        }

        public static void ExigirRango(decimal? valor, decimal minimo, decimal maximo, string mensaje)
        {
            if (valor.HasValue && (valor.Value < minimo || valor.Value > maximo))
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.Validacion, mensaje);
            }
        }

        public static void ExigirEmail(string valor, string mensaje)
        {
            if (valor == null)
            {
                return;
            }

            int arroba = valor.IndexOf('@');

            if (valor.Contains('\r') || valor.Contains('\n') || arroba <= 0
                || arroba == valor.Length - 1 || arroba != valor.LastIndexOf('@'))
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.Validacion, mensaje);
            }
        }

        public static void ExigirCantidadMinima(System.Collections.ICollection valor, int minimo, string mensaje)
        {
            if (valor != null && valor.Count < minimo)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.Validacion, mensaje);
            }
        }
    }
}
