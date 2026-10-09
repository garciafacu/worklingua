using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace worklingua.Server.BE
{
    public class BEGuardarEncuesta
    {
        #region Propiedades
        [Range(1, int.MaxValue, ErrorMessage = "Elegí el idioma de la encuesta.")]
        public int IdiomaId { get; set; }

        [Required(ErrorMessage = "La pregunta es obligatoria.")]
        [StringLength(300, ErrorMessage = "La pregunta no puede superar los 300 caracteres.")]
        public string Pregunta { get; set; }

        [StringLength(600, ErrorMessage = "El texto de ayuda no puede superar los 600 caracteres.")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
        public DateOnly? FechaDesde { get; set; }

        [Required(ErrorMessage = "La fecha de vencimiento es obligatoria.")]
        public DateOnly? FechaVencimiento { get; set; }

        public bool Activo { get; set; }

        /// <summary>
        /// Textos de las opciones, en el orden en que se muestran. Solo se envían
        /// cuando la encuesta todavía no tiene respuestas.
        /// </summary>
        public List<string> Opciones { get; set; }
        #endregion

        public BEGuardarEncuesta()
        {
            this.Opciones = new List<string>();
        }

        public BEGuardarEncuesta(
            int idiomaId,
            string pregunta,
            string descripcion,
            DateOnly? fechaDesde,
            DateOnly? fechaVencimiento,
            bool activo,
            List<string> opciones)
        {
            this.IdiomaId = idiomaId;
            this.Pregunta = pregunta;
            this.Descripcion = descripcion;
            this.FechaDesde = fechaDesde;
            this.FechaVencimiento = fechaVencimiento;
            this.Activo = activo;
            this.Opciones = opciones;
        }
    }
}
