namespace worklingua.Server.BE
{
    /// <summary>El veredicto de una fila: entra, o no entra y por qué.</summary>
    public class BEResultadoFilaPadron
    {
        #region Propiedades
        /// <summary>Número de fila en el archivo, contando el encabezado.</summary>
        public int Fila { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Departamento { get; set; }
        public bool Valida { get; set; }
        /// <summary>Por qué se rechaza. Null cuando la fila es válida.</summary>
        public string Motivo { get; set; }
        #endregion

        public BEResultadoFilaPadron()
        {

        }
    }
}
