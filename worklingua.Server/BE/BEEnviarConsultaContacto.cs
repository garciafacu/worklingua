namespace worklingua.Server.BE
{
    public class BEEnviarConsultaContacto
    {
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string Asunto { get; set; }
        public string Mensaje { get; set; }

        public BEEnviarConsultaContacto()
        {
        }

        public BEEnviarConsultaContacto(string nombre, string email, string asunto, string mensaje)
        {
            this.Nombre = nombre;
            this.Email = email;
            this.Asunto = asunto;
            this.Mensaje = mensaje;
        }
    }
}
