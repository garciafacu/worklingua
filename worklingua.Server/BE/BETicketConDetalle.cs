using System.Collections.Generic;

namespace worklingua.Server.BE
{
    public class BETicketConDetalle
    {
        #region Propiedades
        public BETicket Ticket { get; set; }
        public string CodigoIdioma { get; set; }
        public string Empresa { get; set; }
        public string Autor { get; set; }
        public string AutorNombre { get; set; }
        public string AutorEmail { get; set; }
        public string Plan { get; set; }
        public string EstadoSuscripcion { get; set; }
        public string Curso { get; set; }
        public int CantidadMensajes { get; set; }
        public List<BEMensajeTicketConAutor> Mensajes { get; set; }
        #endregion

        public BETicketConDetalle()
        {
            this.Mensajes = new List<BEMensajeTicketConAutor>();
        }

        public BETicketConDetalle(
            BETicket ticket,
            string codigoIdioma,
            string empresa,
            string autor,
            string autorNombre,
            string autorEmail,
            string plan,
            string estadoSuscripcion,
            string curso,
            int cantidadMensajes,
            List<BEMensajeTicketConAutor> mensajes)
        {
            this.Ticket = ticket;
            this.CodigoIdioma = codigoIdioma;
            this.Empresa = empresa;
            this.Autor = autor;
            this.AutorNombre = autorNombre;
            this.AutorEmail = autorEmail;
            this.Plan = plan;
            this.EstadoSuscripcion = estadoSuscripcion;
            this.Curso = curso;
            this.CantidadMensajes = cantidadMensajes;
            this.Mensajes = mensajes;
        }
    }
}
