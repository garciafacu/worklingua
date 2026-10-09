namespace worklingua.Server.BE
{
    /// <summary>Criterios del panel de rendimiento (CU-001-004).</summary>
    public class BEFiltroRendimiento
    {
        #region Propiedades
        /// <summary>
        /// Solo la manda quien tiene alcance sobre todas las empresas; al resto
        /// la BLL le impone la propia.
        /// </summary>
        public int EmpresaId { get; set; }
        public DateOnly? Desde { get; set; }
        public DateOnly? Hasta { get; set; }
        /// <summary>Cero o null significa todos los departamentos.</summary>
        public int? DepartamentoId { get; set; }
        #endregion

        public BEFiltroRendimiento()
        {

        }
    }
}
