using System.ComponentModel.DataAnnotations;

namespace worklingua.Server.BE
{
    public class BEGuardarEmpresa
    {
        #region Propiedades
        [Required(ErrorMessage = "La razón social es obligatoria.")]
        [StringLength(150, ErrorMessage = "La razón social no puede superar los 150 caracteres.")]
        public string RazonSocial { get; set; }

        [Required(ErrorMessage = "El CUIT es obligatorio.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "El CUIT debe tener 11 dígitos, sin guiones.")]
        public string CUIT { get; set; }

        [Required(ErrorMessage = "El correo de la empresa es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [StringLength(120, ErrorMessage = "El correo no puede superar los 120 caracteres.")]
        public string Email { get; set; }

        [StringLength(30, ErrorMessage = "El teléfono no puede superar los 30 caracteres.")]
        public string Telefono { get; set; }

        [StringLength(250, ErrorMessage = "La dirección no puede superar los 250 caracteres.")]
        public string Direccion { get; set; }

        [StringLength(80, ErrorMessage = "La ciudad no puede superar los 80 caracteres.")]
        public string Ciudad { get; set; }

        [StringLength(80, ErrorMessage = "La provincia no puede superar los 80 caracteres.")]
        public string Provincia { get; set; }

        [StringLength(80, ErrorMessage = "El país no puede superar los 80 caracteres.")]
        public string Pais { get; set; }
        #endregion

        public BEGuardarEmpresa()
        {

        }

        public BEGuardarEmpresa(
            string razonSocial,
            string cuit,
            string email,
            string telefono,
            string direccion,
            string ciudad,
            string provincia,
            string pais)
        {
            this.RazonSocial = razonSocial;
            this.CUIT = cuit;
            this.Email = email;
            this.Telefono = telefono;
            this.Direccion = direccion;
            this.Ciudad = ciudad;
            this.Provincia = provincia;
            this.Pais = pais;
        }
    }
}
