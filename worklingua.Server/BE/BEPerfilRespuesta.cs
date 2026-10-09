using System.Collections.Generic;

namespace worklingua.Server.BE
{
    public class BEPerfilRespuesta
    {
        #region Propiedades
        public int UsuarioId { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public DateOnly? FechaNacimiento { get; set; }
        public string Email { get; set; }
        public string Documento { get; set; }
        public string Empresa { get; set; }
        public string Departamento { get; set; }
        public List<string> Roles { get; set; }
        #endregion

        public BEPerfilRespuesta()
        {
            this.Roles = new List<string>();
        }
    }
}
