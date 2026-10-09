using System.Collections.Generic;

namespace worklingua.Server.BE
{
    public class BEResultadoLogin
    {
        #region Propiedades
        public Guid Token { get; set; }
        public int UsuarioId { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }
        public List<string> Permisos { get; set; }
        #endregion

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
}
