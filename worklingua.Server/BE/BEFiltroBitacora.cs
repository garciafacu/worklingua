using System.ComponentModel.DataAnnotations;

namespace worklingua.Server.BE
{
    public class BEFiltroBitacora
    {
        #region Propiedades
        [StringLength(200, ErrorMessage = "El término de búsqueda no puede superar los 200 caracteres.")]
        public string Texto { get; set; }

        [StringLength(200, ErrorMessage = "El usuario buscado no puede superar los 200 caracteres.")]
        public string Usuario { get; set; }

        [StringLength(80, ErrorMessage = "El módulo no puede superar los 80 caracteres.")]
        public string Modulo { get; set; }

        [StringLength(120, ErrorMessage = "La acción no puede superar los 120 caracteres.")]
        public string Accion { get; set; }

        [StringLength(20, ErrorMessage = "El nivel no puede superar los 20 caracteres.")]
        public string Nivel { get; set; }

        public DateTime? Desde { get; set; }

        public DateTime? Hasta { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La página tiene que ser mayor o igual a 1.")]
        public int? Pagina { get; set; }

        [Range(1, 100, ErrorMessage = "El tamaño de página tiene que estar entre 1 y 100.")]
        public int? TamanioPagina { get; set; }
        #endregion

        public BEFiltroBitacora()
        {

        }

        public BEFiltroBitacora(
            string texto,
            string usuario,
            string modulo,
            string accion,
            string nivel,
            DateTime? desde,
            DateTime? hasta,
            int? pagina,
            int? tamanioPagina)
        {
            this.Texto = texto;
            this.Usuario = usuario;
            this.Modulo = modulo;
            this.Accion = accion;
            this.Nivel = nivel;
            this.Desde = desde;
            this.Hasta = hasta;
            this.Pagina = pagina;
            this.TamanioPagina = tamanioPagina;
        }
    }
}
