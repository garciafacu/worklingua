using System.Collections.Generic;

namespace worklingua.Server.BE
{
    public class BEProgresoUsuario
    {
        #region Propiedades
        public int UsuarioId { get; set; }
        public string Usuario { get; set; }
        public string Email { get; set; }
        public List<BECursoConProgreso> Cursos { get; set; }
        #endregion

        public BEProgresoUsuario()
        {
            this.Cursos = new List<BECursoConProgreso>();
        }

        public BEProgresoUsuario(int usuarioId, string usuario, string email, List<BECursoConProgreso> cursos)
        {
            this.UsuarioId = usuarioId;
            this.Usuario = usuario;
            this.Email = email;
            this.Cursos = cursos;
        }
    }
}
