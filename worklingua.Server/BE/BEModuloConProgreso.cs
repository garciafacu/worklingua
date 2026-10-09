namespace worklingua.Server.BE
{
    public class BEModuloConProgreso
    {
        #region Propiedades
        public BEModulo Modulo { get; set; }
        public BEProgreso Progreso { get; set; }
        #endregion

        public BEModuloConProgreso()
        {

        }

        public BEModuloConProgreso(BEModulo modulo, BEProgreso progreso)
        {
            this.Modulo = modulo;
            this.Progreso = progreso;
        }
    }
}
