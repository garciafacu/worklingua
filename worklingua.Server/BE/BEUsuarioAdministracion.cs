using System.Collections.Generic;

namespace worklingua.Server.BE
{
    public class BEUsuarioAdministracion
    {
        #region Propiedades
        public BEUsuario Usuario { get; set; }
        public string Empresa { get; set; }
        public string Departamento { get; set; }
        public int RolId { get; set; }
        public List<string> Roles { get; set; }
        #endregion

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
}
