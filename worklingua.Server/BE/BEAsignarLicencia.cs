using System.ComponentModel.DataAnnotations;

namespace worklingua.Server.BE
{
    public class BEAsignarLicencia
    {
        #region Propiedades
        [Range(1, int.MaxValue, ErrorMessage = "Elegí el empleado al que asignarle la licencia.")]
        public int UsuarioId { get; set; }
        #endregion

        public BEAsignarLicencia()
        {

        }

        public BEAsignarLicencia(int usuarioId)
        {
            this.UsuarioId = usuarioId;
        }
    }
}
