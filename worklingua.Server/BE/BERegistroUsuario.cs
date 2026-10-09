using System.ComponentModel.DataAnnotations;

namespace worklingua.Server.BE
{
    public class BERegistroUsuario
    {
        #region Propiedades
        [Required(ErrorMessage = "La razón social es obligatoria.")]
        [StringLength(150, ErrorMessage = "La razón social no puede superar los 150 caracteres.")]
        public string RazonSocial { get; set; }

        [Required(ErrorMessage = "El CUIT es obligatorio.")]
        [StringLength(11, MinimumLength = 11,
            ErrorMessage = "El CUIT debe tener 11 dígitos, sin guiones.")]
        public string CUIT { get; set; }

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

        [Required(ErrorMessage = "La clave es obligatoria.")]
        [StringLength(200, MinimumLength = 8, ErrorMessage = "La clave debe tener al menos 8 caracteres.")]
        public string Clave { get; set; }


        public DateOnly? FechaNacimiento { get; set; }

        [Required(ErrorMessage = "Confirmá que no sos un robot.")]
        public string TokenCaptcha { get; set; }
        #endregion

        public BERegistroUsuario()
        {

        }

        public BERegistroUsuario(
            string razonSocial,
            string cuit,
            string nombre,
            string apellido,
            string documento,
            string email,
            string clave,
            DateOnly? fechaNacimiento,
            string tokenCaptcha)
        {
            this.RazonSocial = razonSocial;
            this.CUIT = cuit;
            this.Nombre = nombre;
            this.Apellido = apellido;
            this.Documento = documento;
            this.Email = email;
            this.Clave = clave;
            this.FechaNacimiento = fechaNacimiento;
            this.TokenCaptcha = tokenCaptcha;
        }
    }
}
