namespace worklingua.Server.BLL
{
    public enum TipoErrorNegocio
    {
        Validacion,
        CredencialesInvalidas,
        CuentaNoConfirmada,
        CuentaBloqueada,
        SesionInvalida,
        PermisoDenegado,
        NoEncontrado,
        Conflicto
    }

    public class ExcepcionNegocio : Exception
    {
        public ExcepcionNegocio(TipoErrorNegocio tipo, string mensaje) : base(mensaje)
        {
            this.Tipo = tipo;
        }

        public ExcepcionNegocio(TipoErrorNegocio tipo, string mensaje, string sugerencia)
            : this(tipo, mensaje)
        {
            this.Sugerencia = sugerencia;
        }

        public TipoErrorNegocio Tipo { get; }

        public string Sugerencia { get; }
    }
}
