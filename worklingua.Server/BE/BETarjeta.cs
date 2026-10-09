using System.ComponentModel.DataAnnotations;

namespace worklingua.Server.BE
{
    public class BETarjeta
    {
        #region Propiedades
        [StringLength(25, ErrorMessage = "El número de tarjeta no puede superar los 25 caracteres.")]
        public string Numero { get; set; }

        [StringLength(100, ErrorMessage = "El titular no puede superar los 100 caracteres.")]
        public string Titular { get; set; }

        [StringLength(7, ErrorMessage = "El vencimiento tiene que tener el formato MM/AA.")]
        public string Vencimiento { get; set; }

        [StringLength(4, ErrorMessage = "El código de seguridad tiene 3 o 4 dígitos.")]
        public string CodigoSeguridad { get; set; }
        #endregion

        public BETarjeta()
        {

        }

        public BETarjeta(string numero, string titular, string vencimiento, string codigoSeguridad)
        {
            this.Numero = numero;
            this.Titular = titular;
            this.Vencimiento = vencimiento;
            this.CodigoSeguridad = codigoSeguridad;
        }
    }
}
