using System.Collections;
using System.Collections.Generic;
using System.Data;
using worklingua.Server.BE;
using worklingua.Server.DAL;
using worklingua.Server.Services;

namespace worklingua.Server.MPP
{
    public class MPPCultura
    {
        Acceso oDatos;
        Hashtable Hdatos;

        public MPPCultura()
        {
            oDatos = new Acceso();
        }

        public List<BECulturaConIdioma> ListarTodo()
        {
            return Leer("sp_Cultura_Listar", null);
        }

        public List<BECulturaConIdioma> ListarTodoConBajas()
        {
            return Leer("sp_Cultura_ListarTodos", null);
        }

        public BECulturaConIdioma ListarObjeto(BECultura Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@CulturaId", Objeto.CulturaId);

            DataTable Dt = oDatos.Leer("sp_Cultura_ObtenerPorId", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                return Mapear(Dt.Rows[0]);
            }
            else
            {
                return null;
            }
        }

        public int Guardar(BECultura Objeto)
        {
            Hdatos = new Hashtable();

            if (Objeto.CulturaId != 0)
            {
                Hdatos.Add("@CulturaId", Objeto.CulturaId);
                CargarParametrosComunes(Objeto);

                oDatos.LeerEscalar("sp_Cultura_Modificar", Hdatos);

                return Objeto.CulturaId;
            }

            CargarParametrosComunes(Objeto);
            Hdatos.Add("@Activo", Objeto.Activo);

            object identidad = oDatos.LeerEscalar("sp_Cultura_Alta", Hdatos);

            return Convert.ToInt32(identidad);
        }

        public bool Baja(BECultura Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@CulturaId", Objeto.CulturaId);

            object filas = oDatos.LeerEscalar("sp_Cultura_Baja", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        public bool CambiarEstado(BECultura Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@CulturaId", Objeto.CulturaId);
            Hdatos.Add("@Activo", Objeto.Activo);

            object filas = oDatos.LeerEscalar("sp_Cultura_CambiarEstado", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        private void CargarParametrosComunes(BECultura Objeto)
        {
            Hdatos.Add("@Codigo", Objeto.Codigo);
            Hdatos.Add("@Nombre", Objeto.Nombre);
            Hdatos.Add("@IdiomaId", Objeto.IdiomaId);
            Hdatos.Add("@Moneda", Objeto.Moneda);
            Hdatos.Add("@SimboloMoneda", Objeto.SimboloMoneda);
            Hdatos.Add("@FormatoFecha", Objeto.FormatoFecha);
            Hdatos.Add("@SeparadorDecimal", Objeto.SeparadorDecimal);
            Hdatos.Add("@SeparadorMiles", Objeto.SeparadorMiles);
            Hdatos.Add("@TasaConversion", Objeto.TasaConversion);
            Hdatos.Add("@EsPredeterminada", Objeto.EsPredeterminada);
        }

        private List<BECulturaConIdioma> Leer(string nombreSp, Hashtable parametros)
        {
            List<BECulturaConIdioma> ListaCulturaBE = new List<BECulturaConIdioma>();
            DataTable Dt = oDatos.Leer(nombreSp, parametros);

            if (Dt.Rows.Count > 0)
            {
                foreach (DataRow Item in Dt.Rows)
                {
                    ListaCulturaBE.Add(Mapear(Item));
                }

                return ListaCulturaBE;
            }
            else
            {
                return null;
            }
        }

        private BECulturaConIdioma Mapear(DataRow Item)
        {
            BECultura oCulturaBE = new BECultura();

            oCulturaBE.CulturaId = ServicioLectorFila.LeerEntero(Item, "CulturaId");
            oCulturaBE.Codigo = ServicioLectorFila.LeerTexto(Item, "Codigo").Trim();
            oCulturaBE.Nombre = ServicioLectorFila.LeerTexto(Item, "Nombre");
            oCulturaBE.IdiomaId = ServicioLectorFila.LeerEntero(Item, "IdiomaId");
            oCulturaBE.Moneda = ServicioLectorFila.LeerTexto(Item, "Moneda").Trim();
            oCulturaBE.SimboloMoneda = ServicioLectorFila.LeerTexto(Item, "SimboloMoneda").Trim();
            oCulturaBE.FormatoFecha = ServicioLectorFila.LeerTexto(Item, "FormatoFecha").Trim();
            oCulturaBE.SeparadorDecimal = ServicioLectorFila.LeerTexto(Item, "SeparadorDecimal");
            oCulturaBE.SeparadorMiles = ServicioLectorFila.LeerTexto(Item, "SeparadorMiles");
            oCulturaBE.TasaConversion = ServicioLectorFila.LeerDecimal(Item, "TasaConversion");
            oCulturaBE.EsPredeterminada = ServicioLectorFila.LeerBooleano(Item, "EsPredeterminada");
            oCulturaBE.Activo = ServicioLectorFila.LeerBooleano(Item, "Activo");

            return new BECulturaConIdioma(
                oCulturaBE,
                ServicioLectorFila.LeerTexto(Item, "Idioma"),
                ServicioLectorFila.LeerTexto(Item, "CodigoIdioma").Trim());
        }
    }
}
