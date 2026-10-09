namespace worklingua.Server.BE
{
    public class BEUsuario
    {
        #region Propiedades
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
        /// <summary>Fallos consecutivos de login; un ingreso exitoso lo vuelve a cero.</summary>
        public int IntentosFallidos { get; set; }
        /// <summary>Hasta cuándo está bloqueada. NULL = no bloqueada (CU-003-004).</summary>
        public DateTime? BloqueadoHasta { get; set; }
        #endregion

        public BEUsuario()
        {

        }

        public BEUsuario(
            int usuarioId,
            int empresaId,
            int? departamentoId,
            string idioma,
            string nombre,
            string apellido,
            string documento,
            string email,
            string passwordHash,
            string nivelIdioma,
            DateOnly? fechaNacimiento,
            DateTime? fechaAlta,
            DateTime? ultimoAcceso,
            bool? activo)
        {
            this.UsuarioId = usuarioId;
            this.EmpresaId = empresaId;
            this.DepartamentoId = departamentoId;
            this.Idioma = idioma;
            this.Nombre = nombre;
            this.Apellido = apellido;
            this.Documento = documento;
            this.Email = email;
            this.PasswordHash = passwordHash;
            this.NivelIdioma = nivelIdioma;
            this.FechaNacimiento = fechaNacimiento;
            this.FechaAlta = fechaAlta;
            this.UltimoAcceso = ultimoAcceso;
            this.Activo = activo;
        }
    }
}
