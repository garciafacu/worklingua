using System.ComponentModel.DataAnnotations;

namespace worklingua.Server.BE
{
    /// <summary>
    /// Lo que el propio usuario puede cambiar de su perfil (CU-001-003). El resto
    /// de sus datos lo modifica un administrador.
    /// </summary>
    public class BEModificarPerfil
    {
        #region Propiedades
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(80, ErrorMessage = "El nombre no puede superar los 80 caracteres.")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(80, ErrorMessage = "El apellido no puede superar los 80 caracteres.")]
        public string Apellido { get; set; }

        public DateOnly? FechaNacimiento { get; set; }
        #endregion

        public BEModificarPerfil()
        {

        }

        public BEModificarPerfil(string nombre, string apellido, DateOnly? fechaNacimiento)
        {
            this.Nombre = nombre;
            this.Apellido = apellido;
            this.FechaNacimiento = fechaNacimiento;
        }
    }
}
