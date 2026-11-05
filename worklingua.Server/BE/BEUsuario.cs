namespace worklingua.Server.BE
{
    public class BEUsuario
    {
        public int UsuarioId { get; set; }
        public int EmpresaId { get; set; }
        public int? DepartamentoId { get; set; }
        public string Idioma { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Documento { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string NivelIdioma { get; set; }
        public DateOnly? FechaNacimiento { get; set; }
        public DateTime? FechaAlta { get; set; }
        public DateTime? UltimoAcceso { get; set; }
        public bool? Activo { get; set; }
        public int IntentosFallidos { get; set; }
        public DateTime? BloqueadoHasta { get; set; }
    }

    public class BEUsuarioAdminRespuesta
    {
        public int UsuarioId { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Documento { get; set; }
        public string Email { get; set; }
        public int EmpresaId { get; set; }
        public string Empresa { get; set; }
        public int? DepartamentoId { get; set; }
        public string Departamento { get; set; }
        public int RolId { get; set; }
        public List<string> Roles { get; set; }
        public DateTime? FechaAlta { get; set; }
        public DateTime? UltimoAcceso { get; set; }
        public bool Activo { get; set; }
        public DateTime? BloqueadoHasta { get; set; }
    }

    public class BEUsuarioAdministracion
    {
        public BEUsuario Usuario { get; set; }
        public string Empresa { get; set; }
        public string Departamento { get; set; }
        public int RolId { get; set; }
        public List<string> Roles { get; set; }

        public BEUsuarioAdministracion()
        {
        }

        public BEUsuarioAdministracion(BEUsuario usuario, string empresa, int rolId, List<string> roles)
        {
            this.Usuario = usuario;
            this.Empresa = empresa;
            this.RolId = rolId;
            this.Roles = roles;
        }
    }

    public class BEUsuarioRol
    {
        public int UsuarioRolId { get; set; }
        public int UsuarioId { get; set; }
        public int RolId { get; set; }
        public DateTime? FechaAsignacion { get; set; }

        public BEUsuarioRol()
        {
        }

        public BEUsuarioRol(int usuarioRolId, int usuarioId, int rolId, DateTime? fechaAsignacion)
        {
            this.UsuarioRolId = usuarioRolId;
            this.UsuarioId = usuarioId;
            this.RolId = rolId;
            this.FechaAsignacion = fechaAsignacion;
        }
    }

    public class BESesion
    {
        public int SesionId { get; set; }
        public int UsuarioId { get; set; }
        public Guid Token { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public bool? Activa { get; set; }
    }

    public class BELogin
    {
        public string Email { get; set; }
        public string Clave { get; set; }
    }

    public class BEResultadoLogin
    {
        public Guid Token { get; set; }
        public int UsuarioId { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }
        public List<string> Permisos { get; set; }

        public BEResultadoLogin()
        {
        }

        public BEResultadoLogin(
            Guid token,
            int usuarioId,
            string nombre,
            string apellido,
            string email,
            List<string> roles,
            List<string> permisos)
        {
            this.Token = token;
            this.UsuarioId = usuarioId;
            this.Nombre = nombre;
            this.Apellido = apellido;
            this.Email = email;
            this.Roles = roles;
            this.Permisos = permisos;
        }
    }

    public class BECambiarClave
    {
        public string ClaveActual { get; set; }
        public string ClaveNueva { get; set; }
    }

    public class BERecuperarClave
    {
        public string Email { get; set; }
    }

    public class BERestablecerClave
    {
        public Guid Token { get; set; }
        public string ClaveNueva { get; set; }
    }

    public class BERegistroUsuario
    {
        public string RazonSocial { get; set; }
        public string CUIT { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Documento { get; set; }
        public string Email { get; set; }
        public string Clave { get; set; }
        public DateOnly? FechaNacimiento { get; set; }
        public string TokenCaptcha { get; set; }
    }

    public class BECompletarInvitacion
    {
        public Guid Token { get; set; }
        public string Clave { get; set; }
    }

    public class BEInvitarUsuario
    {
        public int EmpresaId { get; set; }
        public int RolId { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Documento { get; set; }
        public int? DepartamentoId { get; set; }
        public string Email { get; set; }
    }

    public class BEModificarPerfil
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public DateOnly? FechaNacimiento { get; set; }
    }

    public class BEPerfilRespuesta
    {
        public int UsuarioId { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public DateOnly? FechaNacimiento { get; set; }
        public string Email { get; set; }
        public string Documento { get; set; }
        public string Empresa { get; set; }
        public string Departamento { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }

    public class BETokenSeguridad
    {
        public int TokenId { get; set; }
        public int UsuarioId { get; set; }
        public Guid Token { get; set; }
        public string Tipo { get; set; }
        public DateTime FechaGeneracion { get; set; }
        public DateTime FechaExpiracion { get; set; }
        public DateTime? FechaUso { get; set; }
        public bool Activo { get; set; }
    }

    public class BEGuardarOperador
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Documento { get; set; }
        public string Email { get; set; }
    }
}
