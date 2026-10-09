using System.ComponentModel.DataAnnotations;

namespace worklingua.Server.BE
{
    public class BETokenNewsletter
    {
        #region Propiedades
        [Required(ErrorMessage = "Falta el token del enlace.")]
        public Guid Token { get; set; }
        #endregion

        public BETokenNewsletter()
        {

        }

        public BETokenNewsletter(Guid token)
        {
            this.Token = token;
        }
    }
}
