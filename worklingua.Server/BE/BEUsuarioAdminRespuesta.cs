using System.Collections.Generic;

namespace worklingua.Server.BE
{
    public class BEUsuarioAdminRespuesta
    {
        #region Propiedades
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
        /// <summary>Hasta cuándo está bloqueada por intentos fallidos; null si no lo está.</summary>
        public DateTime? BloqueadoHasta { get; set; }
        #endregion

        public BEUsuarioAdminRespuesta()
        {

        }

        public BEUsuarioAdminRespuesta(
            int usuarioId,
            string nombre,
            string apellido,
            string documento,
            string email,
            int empresaId,
            string empresa,
            int rolId,
            List<string> roles,
            DateTime? fechaAlta,
            DateTime? ultimoAcceso,
            bool activo)
        {
            this.UsuarioId = usuarioId;
            this.Nombre = nombre;
            this.Apellido = apellido;
            this.Documento = documento;
            this.Email = email;
            this.EmpresaId = empresaId;
            this.Empresa = empresa;
            this.RolId = rolId;
            this.Roles = roles;
            this.FechaAlta = fechaAlta;
            this.UltimoAcceso = ultimoAcceso;
            this.Activo = activo;
        }
    }
}
