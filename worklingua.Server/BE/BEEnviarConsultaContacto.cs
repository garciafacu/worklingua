using System.ComponentModel.DataAnnotations;

namespace worklingua.Server.BE
{
    public class BEEnviarConsultaContacto
    {
        #region Propiedades
        [Required(ErrorMessage = "Ingresá tu nombre.")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "Ingresá tu correo.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [StringLength(150, ErrorMessage = "El correo no puede superar los 150 caracteres.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Ingresá un asunto.")]
        [StringLength(150, ErrorMessage = "El asunto no puede superar los 150 caracteres.")]
        public string Asunto { get; set; }

        [Required(ErrorMessage = "Ingresá tu mensaje.")]
        [StringLength(2000, ErrorMessage = "El mensaje no puede superar los 2000 caracteres.")]
        public string Mensaje { get; set; }
        #endregion

        public BEEnviarConsultaContacto()
        {

        }

        public BEEnviarConsultaContacto(string nombre, string email, string asunto, string mensaje)
        {
            this.Nombre = nombre;
            this.Email = email;
            this.Asunto = asunto;
            this.Mensaje = mensaje;
        }
    }
}
