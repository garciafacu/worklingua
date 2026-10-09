using System.ComponentModel.DataAnnotations;

namespace worklingua.Server.BE
{
    public class BEGuardarOperador
    {
        #region Propiedades
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(80, ErrorMessage = "El nombre no puede superar los 80 caracteres.")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(80, ErrorMessage = "El apellido no puede superar los 80 caracteres.")]
        public string Apellido { get; set; }

        [StringLength(20, ErrorMessage = "El documento no puede superar los 20 caracteres.")]
        public string Documento { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [StringLength(150, ErrorMessage = "El correo no puede superar los 150 caracteres.")]
        public string Email { get; set; }

        #endregion

        public BEGuardarOperador()
        {

        }

        public BEGuardarOperador(string nombre, string apellido, string documento, string email)
        {
            this.Nombre = nombre;
            this.Apellido = apellido;
            this.Documento = documento;
            this.Email = email;
        }
    }
}
