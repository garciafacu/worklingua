using System.ComponentModel.DataAnnotations;

namespace worklingua.Server.BE
{
    public class BEInvitarUsuario
    {
        #region Propiedades
        public int EmpresaId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Seleccioná un rol.")]
        public int RolId { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(80, ErrorMessage = "El nombre no puede superar los 80 caracteres.")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(80, ErrorMessage = "El apellido no puede superar los 80 caracteres.")]
        public string Apellido { get; set; }

        [StringLength(20, ErrorMessage = "El documento no puede superar los 20 caracteres.")]
        public string Documento { get; set; }

        /// <summary>Opcional: cero o null dejan al empleado sin departamento.</summary>
        public int? DepartamentoId { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [StringLength(150, ErrorMessage = "El correo no puede superar los 150 caracteres.")]
        public string Email { get; set; }

        #endregion

        public BEInvitarUsuario()
        {

        }

        public BEInvitarUsuario(
            int empresaId,
            int rolId,
            string nombre,
            string apellido,
            string documento,
            string email)
        {
            this.EmpresaId = empresaId;
            this.RolId = rolId;
            this.Nombre = nombre;
            this.Apellido = apellido;
            this.Documento = documento;
            this.Email = email;
        }
    }
}
