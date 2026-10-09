using System.Collections;
using System.Collections.Generic;
using System.Data;
using worklingua.Server.BE;
using worklingua.Server.DAL;
using worklingua.Server.Services;

namespace worklingua.Server.MPP
{
    public class MPPSuscriptorNewsletter
    {
        Acceso oDatos;
        Hashtable Hdatos;

        public MPPSuscriptorNewsletter()
        {
            oDatos = new Acceso();
        }

        public List<BESuscriptorNewsletter> ListarTodo()
        {
            return MapearLista(oDatos.Leer("sp_SuscriptorNewsletter_Listar", null));
        }

        public List<BESuscriptorNewsletter> ListarConfirmados(BEIdioma Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@IdiomaId", Objeto.IdiomaId);

            return MapearLista(oDatos.Leer("sp_SuscriptorNewsletter_ListarConfirmadosPorIdioma", Hdatos));
        }

        public BESuscriptorNewsletter ListarObjeto(BESuscriptorNewsletter Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@SuscriptorId", Objeto.SuscriptorId);

            return MapearPrimero(oDatos.Leer("sp_SuscriptorNewsletter_ObtenerPorId", Hdatos));
        }

        public BESuscriptorNewsletter ListarPorEmail(BESuscriptorNewsletter Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@Email", Objeto.Email);

            return MapearPrimero(oDatos.Leer("sp_SuscriptorNewsletter_ObtenerPorEmail", Hdatos));
        }

        public BESuscriptorNewsletter ListarPorToken(BESuscriptorNewsletter Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@Token", Objeto.Token);

            return MapearPrimero(oDatos.Leer("sp_SuscriptorNewsletter_ObtenerPorToken", Hdatos));
        }

        public int Guardar(BESuscriptorNewsletter Objeto)
        {
            Hdatos = new Hashtable();

            if (Objeto.SuscriptorId != 0)
            {
                Hdatos.Add("@SuscriptorId", Objeto.SuscriptorId);
                Hdatos.Add("@IdiomaId", Objeto.IdiomaId);

                oDatos.LeerEscalar("sp_SuscriptorNewsletter_Reactivar", Hdatos);

                return Objeto.SuscriptorId;
            }

            Hdatos.Add("@Email", Objeto.Email);
            Hdatos.Add("@IdiomaId", Objeto.IdiomaId);

            object identidad = oDatos.LeerEscalar("sp_SuscriptorNewsletter_Alta", Hdatos);

            return Convert.ToInt32(identidad);
        }

        public bool Confirmar(BESuscriptorNewsletter Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@SuscriptorId", Objeto.SuscriptorId);

            object filas = oDatos.LeerEscalar("sp_SuscriptorNewsletter_Confirmar", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        public bool Baja(BESuscriptorNewsletter Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@SuscriptorId", Objeto.SuscriptorId);

            object filas = oDatos.LeerEscalar("sp_SuscriptorNewsletter_Baja", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        private BESuscriptorNewsletter MapearPrimero(DataTable Dt)
        {
            if (Dt.Rows.Count > 0)
            {
                return Mapear(Dt.Rows[0]);
            }
            else
            {
                return null;
            }
        }

        private List<BESuscriptorNewsletter> MapearLista(DataTable Dt)
        {
            List<BESuscriptorNewsletter> ListaSuscriptorBE = new List<BESuscriptorNewsletter>();

            if (Dt.Rows.Count > 0)
            {
                foreach (DataRow Item in Dt.Rows)
                {
                    ListaSuscriptorBE.Add(Mapear(Item));
                }

                return ListaSuscriptorBE;
            }
            else
            {
                return null;
            }
        }

        private BESuscriptorNewsletter Mapear(DataRow Item)
        {
            BESuscriptorNewsletter oSuscriptorBE = new BESuscriptorNewsletter();

            oSuscriptorBE.SuscriptorId = ServicioLectorFila.LeerEntero(Item, "SuscriptorId");
            oSuscriptorBE.Email = ServicioLectorFila.LeerTexto(Item, "Email");
            oSuscriptorBE.IdiomaId = ServicioLectorFila.LeerEntero(Item, "IdiomaId");
            oSuscriptorBE.Token = ServicioLectorFila.LeerGuid(Item, "Token");
            oSuscriptorBE.Confirmado = ServicioLectorFila.LeerBooleano(Item, "Confirmado");
            oSuscriptorBE.FechaAlta = ServicioLectorFila.LeerFechaHora(Item, "FechaAlta");
            oSuscriptorBE.FechaConfirmacion = ServicioLectorFila.LeerFechaHoraNula(Item, "FechaConfirmacion");
            oSuscriptorBE.FechaBaja = ServicioLectorFila.LeerFechaHoraNula(Item, "FechaBaja");
            oSuscriptorBE.Activo = ServicioLectorFila.LeerBooleano(Item, "Activo");

            return oSuscriptorBE;
        }
    }
}
