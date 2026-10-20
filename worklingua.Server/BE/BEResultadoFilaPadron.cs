namespace worklingua.Server.BE
{
    public class BEResultadoFilaPadron
    {
        #region Propiedades
        public int Fila { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Departamento { get; set; }
        public bool Valida { get; set; }
        public string Motivo { get; set; }
        #endregion

        public BEResultadoFilaPadron()
        {

        }
    }
}
