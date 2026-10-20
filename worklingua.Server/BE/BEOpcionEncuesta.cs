namespace worklingua.Server.BE
{
    public class BEOpcionEncuesta
    {
        #region Propiedades
        public int OpcionEncuestaId { get; set; }
        public int EncuestaId { get; set; }
        public string Texto { get; set; }
        public int Orden { get; set; }
        public int Total { get; set; }
        #endregion

        public BEOpcionEncuesta()
        {

        }

        public BEOpcionEncuesta(int opcionEncuestaId, int encuestaId, string texto, int orden, int total)
        {
            this.OpcionEncuestaId = opcionEncuestaId;
            this.EncuestaId = encuestaId;
            this.Texto = texto;
            this.Orden = orden;
            this.Total = total;
        }
    }
}
