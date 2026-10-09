namespace worklingua.Server.BE
{
    /// <summary>
    /// Una fila del CSV tal como la leyó el navegador, sin validar. Los valores
    /// vienen como texto: la validación y la normalización son de la BLL.
    /// </summary>
    public class BEFilaPadron
    {
        #region Propiedades
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Documento { get; set; }
        /// <summary>Nombre del departamento, no el id: el CSV lo trae como texto.</summary>
        public string Departamento { get; set; }
        #endregion

        public BEFilaPadron()
        {

        }

        public BEFilaPadron(
            string nombre, string apellido, string email, string documento, string departamento)
        {
            this.Nombre = nombre;
            this.Apellido = apellido;
            this.Email = email;
            this.Documento = documento;
            this.Departamento = departamento;
        }
    }
}
