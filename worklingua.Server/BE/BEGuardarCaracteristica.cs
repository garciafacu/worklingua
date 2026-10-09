using System.ComponentModel.DataAnnotations;

namespace worklingua.Server.BE
{
    public class BEGuardarCaracteristica
    {
        #region Propiedades
        [Required(ErrorMessage = "El nombre de la característica es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        public string Nombre { get; set; }

        [Range(0, 100000, ErrorMessage = "El orden debe estar entre 0 y 100000.")]
        public int Orden { get; set; }
        #endregion

        public BEGuardarCaracteristica()
        {

        }

        public BEGuardarCaracteristica(string nombre, int orden)
        {
            this.Nombre = nombre;
            this.Orden = orden;
        }
    }
}
