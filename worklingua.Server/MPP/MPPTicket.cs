using System.Collections;
using System.Collections.Generic;
using System.Data;
using worklingua.Server.BE;
using worklingua.Server.DAL;
using worklingua.Server.Services;

namespace worklingua.Server.MPP
{
    public class MPPTicket
    {
        Acceso oDatos;
        Hashtable Hdatos;

        public MPPTicket()
        {
            oDatos = new Acceso();
        }

        public int Guardar(BETicket Objeto, BEMensajeTicket oMensajeBE)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@EmpresaId", Objeto.EmpresaId);
            Hdatos.Add("@UsuarioId", Objeto.UsuarioId);
            Hdatos.Add("@SuscripcionId", Objeto.SuscripcionId);
            Hdatos.Add("@CursoId", Objeto.CursoId);
            Hdatos.Add("@Asunto", Objeto.Asunto);
            Hdatos.Add("@Estado", Objeto.Estado);
            Hdatos.Add("@IdiomaId", Objeto.IdiomaId);
            Hdatos.Add("@Texto", oMensajeBE.Texto);

            object identidad = oDatos.LeerEscalar("sp_Ticket_Alta", Hdatos);

            return Convert.ToInt32(identidad);
        }

        public List<BETicketConDetalle> Listar(BEFiltroTicket Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@EmpresaId", Objeto.EmpresaId);
            Hdatos.Add("@UsuarioId", Objeto.UsuarioId);
            Hdatos.Add("@Estado", Objeto.Estado);
            Hdatos.Add("@TicketId", Objeto.TicketId);

            List<BETicketConDetalle> ListaTicketBE = new List<BETicketConDetalle>();
            DataTable Dt = oDatos.Leer("sp_Ticket_Listar", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                foreach (DataRow Item in Dt.Rows)
                {
                    ListaTicketBE.Add(Mapear(Item));
                }

                return ListaTicketBE;
            }
            else
            {
                return null;
            }
        }

        public BETicketConDetalle ListarObjeto(BETicket Objeto)
        {
            BEFiltroTicket oFiltroBE = new BEFiltroTicket();
            oFiltroBE.TicketId = Objeto.TicketId;

            List<BETicketConDetalle> ListaTicketBE = Listar(oFiltroBE);

            return ListaTicketBE == null ? null : ListaTicketBE[0];
        }

        public bool CambiarEstado(BETicket Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@TicketId", Objeto.TicketId);
            Hdatos.Add("@Estado", Objeto.Estado);
            Hdatos.Add("@FechaCierre", Objeto.FechaCierre);

            object filas = oDatos.LeerEscalar("sp_Ticket_CambiarEstado", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        public int GuardarMensaje(BEMensajeTicket Objeto, BETicket oTicketBE)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@TicketId", Objeto.TicketId);
            Hdatos.Add("@UsuarioId", Objeto.UsuarioId);
            Hdatos.Add("@EsOperador", Objeto.EsOperador);
            Hdatos.Add("@Texto", Objeto.Texto);
            Hdatos.Add("@EstadoTicket", oTicketBE.Estado);

            object identidad = oDatos.LeerEscalar("sp_MensajeTicket_Alta", Hdatos);

            return Convert.ToInt32(identidad);
        }

        public List<BEMensajeTicketConAutor> ListarMensajes(BETicket Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@TicketId", Objeto.TicketId);

            List<BEMensajeTicketConAutor> ListaMensajeBE = new List<BEMensajeTicketConAutor>();
            DataTable Dt = oDatos.Leer("sp_MensajeTicket_ListarPorTicket", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                foreach (DataRow Item in Dt.Rows)
                {
                    BEMensajeTicket oMensajeBE = new BEMensajeTicket(
                        ServicioLectorFila.LeerEntero(Item, "MensajeId"),
                        ServicioLectorFila.LeerEntero(Item, "TicketId"),
                        ServicioLectorFila.LeerEntero(Item, "UsuarioId"),
                        ServicioLectorFila.LeerBooleano(Item, "EsOperador"),
                        ServicioLectorFila.LeerTexto(Item, "Texto"),
                        ServicioLectorFila.LeerFechaHora(Item, "FechaEnvio"));

                    ListaMensajeBE.Add(new BEMensajeTicketConAutor(oMensajeBE, ServicioLectorFila.LeerTexto(Item, "Autor")));
                }

                return ListaMensajeBE;
            }
            else
            {
                return null;
            }
        }

        private BETicketConDetalle Mapear(DataRow Item)
        {
            BETicket oTicketBE = new BETicket(
                ServicioLectorFila.LeerEntero(Item, "TicketId"),
                ServicioLectorFila.LeerEntero(Item, "EmpresaId"),
                ServicioLectorFila.LeerEntero(Item, "UsuarioId"),
                ServicioLectorFila.LeerEntero(Item, "SuscripcionId"),
                ServicioLectorFila.LeerEnteroNulo(Item, "CursoId"),
                ServicioLectorFila.LeerTexto(Item, "Asunto"),
                ServicioLectorFila.LeerTexto(Item, "Estado"),
                ServicioLectorFila.LeerFechaHora(Item, "FechaAlta"),
                ServicioLectorFila.LeerFechaHora(Item, "FechaUltimoMovimiento"),
                ServicioLectorFila.LeerFechaHoraNula(Item, "FechaCierre"),
                ServicioLectorFila.LeerEnteroNulo(Item, "IdiomaId"));

            return new BETicketConDetalle(
                oTicketBE,
                ServicioLectorFila.LeerTextoNulo(Item, "CodigoIdioma") == null
                    ? null
                    : ServicioLectorFila.LeerTexto(Item, "CodigoIdioma").Trim(),
                ServicioLectorFila.LeerTexto(Item, "Empresa"),
                ServicioLectorFila.LeerTexto(Item, "Autor"),
                ServicioLectorFila.LeerTexto(Item, "AutorNombre"),
                ServicioLectorFila.LeerTexto(Item, "AutorEmail"),
                ServicioLectorFila.LeerTexto(Item, "Plan"),
                ServicioLectorFila.LeerTexto(Item, "EstadoSuscripcion"),
                ServicioLectorFila.LeerTextoNulo(Item, "Curso"),
                ServicioLectorFila.LeerEntero(Item, "CantidadMensajes"),
                new List<BEMensajeTicketConAutor>());
        }
    }
}
