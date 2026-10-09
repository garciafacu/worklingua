using System.Collections;
using System.Collections.Generic;
using System.Data;
using worklingua.Server.BE;
using worklingua.Server.DAL;
using worklingua.Server.Services;

namespace worklingua.Server.MPP
{
    public class MPPOferta
    {
        Acceso oDatos;
        Hashtable Hdatos;

        public MPPOferta()
        {
            oDatos = new Acceso();
        }

        public int Guardar(BEOferta Objeto)
        {
            Hdatos = new Hashtable();
            string Consulta = "sp_Oferta_Alta";

            if (Objeto.OfertaId != 0)
            {
                Hdatos.Add("@OfertaId", Objeto.OfertaId);
                Consulta = "sp_Oferta_Modificar";
            }
            else
            {
                Hdatos.Add("@UsuarioId", Objeto.UsuarioId);
            }

            Hdatos.Add("@EmpresaId", Objeto.EmpresaId);
            Hdatos.Add("@PlanId", Objeto.PlanId);
            Hdatos.Add("@Titulo", Objeto.Titulo);
            Hdatos.Add("@Descripcion", Objeto.Descripcion);
            Hdatos.Add("@FechaDesde", Objeto.FechaDesde.ToDateTime(TimeOnly.MinValue));
            Hdatos.Add(
                "@FechaHasta",
                Objeto.FechaHasta.HasValue
                    ? (object)Objeto.FechaHasta.Value.ToDateTime(TimeOnly.MinValue)
                    : null);
            Hdatos.Add("@Activo", Objeto.Activo);

            object resultado = oDatos.LeerEscalar(Consulta, Hdatos);

            return Objeto.OfertaId != 0 ? Objeto.OfertaId : Convert.ToInt32(resultado);
        }

        public bool Baja(BEOferta Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@OfertaId", Objeto.OfertaId);

            object filas = oDatos.LeerEscalar("sp_Oferta_Baja", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        public List<BEOfertaConDetalle> Listar(BEFiltroOferta Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@OfertaId", Objeto.OfertaId);
            Hdatos.Add("@EmpresaId", Objeto.EmpresaId);
            Hdatos.Add(
                "@SoloVigentesAl",
                Objeto.VigentesAl.HasValue
                    ? (object)Objeto.VigentesAl.Value.ToDateTime(TimeOnly.MinValue)
                    : null);

            List<BEOfertaConDetalle> ListaOfertaBE = new List<BEOfertaConDetalle>();
            DataTable Dt = oDatos.Leer("sp_Oferta_Listar", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                foreach (DataRow Item in Dt.Rows)
                {
                    BEOferta oOfertaBE = new BEOferta(
                        ServicioLectorFila.LeerEntero(Item, "OfertaId"),
                        ServicioLectorFila.LeerEntero(Item, "EmpresaId"),
                        ServicioLectorFila.LeerEnteroNulo(Item, "PlanId"),
                        ServicioLectorFila.LeerTexto(Item, "Titulo"),
                        ServicioLectorFila.LeerTexto(Item, "Descripcion"),
                        ServicioLectorFila.LeerFechaNula(Item, "FechaDesde") ?? default,
                        ServicioLectorFila.LeerFechaNula(Item, "FechaHasta"),
                        ServicioLectorFila.LeerBooleano(Item, "Activo"),
                        ServicioLectorFila.LeerEntero(Item, "UsuarioId"),
                        ServicioLectorFila.LeerFechaHora(Item, "FechaAlta"));

                    ListaOfertaBE.Add(new BEOfertaConDetalle(
                        oOfertaBE,
                        ServicioLectorFila.LeerTexto(Item, "Empresa"),
                        ServicioLectorFila.LeerTextoNulo(Item, "Plan"),
                        ServicioLectorFila.LeerBooleano(Item, "PlanActivo")));
                }

                return ListaOfertaBE;
            }
            else
            {
                return null;
            }
        }
    }
}
