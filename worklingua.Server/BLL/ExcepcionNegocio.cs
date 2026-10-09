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

        /// <summary>
        /// Un conflicto que la pantalla puede resolver sola proponiendo un
        /// valor. Lo usa el nombre duplicado de un activo pedagógico
        /// (CU-004-001, camino alternativo 1): el backend decide la
        /// nomenclatura sugerida y el usuario solo acepta o la cambia.
        /// </summary>
        public ExcepcionNegocio(TipoErrorNegocio tipo, string mensaje, string sugerencia)
            : this(tipo, mensaje)
        {
            this.Sugerencia = sugerencia;
        }

        public TipoErrorNegocio Tipo { get; }

        public string Sugerencia { get; }
    }
}
