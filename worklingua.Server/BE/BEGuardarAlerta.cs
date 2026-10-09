using System.ComponentModel.DataAnnotations;

namespace worklingua.Server.BE
{
    public class BEGuardarAlerta
    {
        #region Propiedades
        /// <summary>
        /// Solo lo manda quien tiene alcance sobre todas las empresas. Al resto
        /// la BLL le impone la propia, así que acá puede venir en cero.
        /// </summary>
        public int EmpresaId { get; set; }

        /// <summary>Null o cero significa toda la empresa.</summary>
        public int? DepartamentoId { get; set; }

        [Required(ErrorMessage = "Debe completar el título y el contenido del mensaje.")]
        [StringLength(150, ErrorMessage = "El título no puede superar los 150 caracteres.")]
        public string Titulo { get; set; }

        [Required(ErrorMessage = "Debe completar el título y el contenido del mensaje.")]
        [StringLength(2000, ErrorMessage = "El mensaje no puede superar los 2000 caracteres.")]
        public string Mensaje { get; set; }

        [Range(1, 365, ErrorMessage = "La condición de inactividad va de 1 a 365 días.")]
        public int DiasInactividad { get; set; }
        #endregion

        public BEGuardarAlerta()
        {

        }

        public BEGuardarAlerta(
            int empresaId, int? departamentoId, string titulo, string mensaje, int diasInactividad)
        {
            this.EmpresaId = empresaId;
            this.DepartamentoId = departamentoId;
            this.Titulo = titulo;
            this.Mensaje = mensaje;
            this.DiasInactividad = diasInactividad;
        }
    }
}
