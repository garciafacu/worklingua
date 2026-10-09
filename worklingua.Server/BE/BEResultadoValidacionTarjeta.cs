namespace worklingua.Server.BE
{
    public class BEResultadoValidacionTarjeta
    {
        #region Propiedades
        public bool FormatoValido { get; set; }
        public bool Aprobada { get; set; }
        public string Marca { get; set; }
        public string UltimosDigitos { get; set; }
        public string CodigoAutorizacion { get; set; }
        public string Motivo { get; set; }
        #endregion

        public BEResultadoValidacionTarjeta()
        {

        }

        public BEResultadoValidacionTarjeta(
            bool formatoValido,
            bool aprobada,
            string marca,
            string ultimosDigitos,
            string codigoAutorizacion,
            string motivo)
        {
            this.FormatoValido = formatoValido;
            this.Aprobada = aprobada;
            this.Marca = marca;
            this.UltimosDigitos = ultimosDigitos;
            this.CodigoAutorizacion = codigoAutorizacion;
            this.Motivo = motivo;
        }
    }
}
