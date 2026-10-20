namespace worklingua.Server.BE
{
    public class BEFilaPadron
    {
        #region Propiedades
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Documento { get; set; }
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
