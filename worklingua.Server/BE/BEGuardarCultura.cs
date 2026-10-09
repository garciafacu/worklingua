using System.ComponentModel.DataAnnotations;

namespace worklingua.Server.BE
{
    public class BEGuardarCultura
    {
        #region Propiedades
        [Required(ErrorMessage = "El código de cultura es obligatorio.")]
        [StringLength(10, MinimumLength = 2,
            ErrorMessage = "El código debe tener entre 2 y 10 caracteres, por ejemplo es-AR.")]
        public string Codigo { get; set; }

        [Required(ErrorMessage = "El nombre de la cultura es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        public string Nombre { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Seleccioná el idioma de plataforma.")]
        public int IdiomaId { get; set; }

        [Required(ErrorMessage = "La moneda es obligatoria.")]
        [StringLength(50, ErrorMessage = "La moneda no puede superar los 50 caracteres.")]
        public string Moneda { get; set; }

        [Required(ErrorMessage = "El símbolo de la moneda es obligatorio.")]
        [StringLength(10, ErrorMessage = "El símbolo no puede superar los 10 caracteres.")]
        public string SimboloMoneda { get; set; }

        [Required(ErrorMessage = "El formato de fecha es obligatorio.")]
        [StringLength(30, ErrorMessage = "El formato de fecha no puede superar los 30 caracteres.")]
        public string FormatoFecha { get; set; }

        [Required(ErrorMessage = "El separador decimal es obligatorio.")]
        [StringLength(1, MinimumLength = 1, ErrorMessage = "El separador decimal es un solo carácter.")]
        public string SeparadorDecimal { get; set; }

        [Required(ErrorMessage = "El separador de miles es obligatorio.")]
        [StringLength(1, MinimumLength = 1, ErrorMessage = "El separador de miles es un solo carácter.")]
        public string SeparadorMiles { get; set; }

        [Range(0.000001, 1000000, ErrorMessage = "La tasa de conversión debe ser mayor que cero.")]
        public decimal TasaConversion { get; set; }

        public bool EsPredeterminada { get; set; }
        #endregion

        public BEGuardarCultura()
        {

        }

        public BEGuardarCultura(
            string codigo,
            string nombre,
            int idiomaId,
            string moneda,
            string simboloMoneda,
            string formatoFecha,
            string separadorDecimal,
            string separadorMiles,
            decimal tasaConversion,
            bool esPredeterminada)
        {
            this.Codigo = codigo;
            this.Nombre = nombre;
            this.IdiomaId = idiomaId;
            this.Moneda = moneda;
            this.SimboloMoneda = simboloMoneda;
            this.FormatoFecha = formatoFecha;
            this.SeparadorDecimal = separadorDecimal;
            this.SeparadorMiles = separadorMiles;
            this.TasaConversion = tasaConversion;
            this.EsPredeterminada = esPredeterminada;
        }
    }
}
