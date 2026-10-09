using System.ComponentModel.DataAnnotations;

namespace worklingua.Server.BE
{
    public class BEGuardarDepartamento
    {
        #region Propiedades
        /// <summary>
        /// Solo lo manda quien tiene alcance sobre todas las empresas. Al resto
        /// la BLL le impone la propia, así que acá puede venir en cero.
        /// </summary>
        public int EmpresaId { get; set; }

        [Required(ErrorMessage = "El nombre del departamento es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        public string Nombre { get; set; }

        [StringLength(250, ErrorMessage = "La descripción no puede superar los 250 caracteres.")]
        public string Descripcion { get; set; }
        #endregion

        public BEGuardarDepartamento()
        {

        }

        public BEGuardarDepartamento(int empresaId, string nombre, string descripcion)
        {
            this.EmpresaId = empresaId;
            this.Nombre = nombre;
            this.Descripcion = descripcion;
        }
    }
}
