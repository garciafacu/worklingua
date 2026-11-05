using System.Collections;
using System.Data;
using worklingua.Server.BE;
using worklingua.Server.DAL;
using worklingua.Server.Services;

namespace worklingua.Server.MPP
{
    public class MPPIdioma
    {
        Acceso oDatos = new Acceso();
        Hashtable Hdatos;

        public List<BEIdioma> ListarTodo()
        {
            return ServicioLectorFila.ALista<BEIdioma>(oDatos.Leer("sp_Idioma_Listar", null), Mapear);
        }

        public List<BEIdioma> ListarTodoConBajas()
        {
            return ServicioLectorFila.ALista<BEIdioma>(oDatos.Leer("sp_Idioma_ListarTodos", null), Mapear);
        }

        public BEIdioma ListarObjeto(BEIdioma Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@IdiomaId", Objeto.IdiomaId);

            return ServicioLectorFila.AObjeto(oDatos.Leer("sp_Idioma_ObtenerPorId", Hdatos), Mapear);
        }

        public int Guardar(BEIdioma Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@Nombre", Objeto.Nombre);
            Hdatos.Add("@CodigoISO", Objeto.CodigoISO);

            if (Objeto.IdiomaId != 0)
            {
                Hdatos.Add("@IdiomaId", Objeto.IdiomaId);

                oDatos.LeerEscalar("sp_Idioma_Modificar", Hdatos);

                return Objeto.IdiomaId;
            }

            Hdatos.Add("@Activo", Objeto.Activo.HasValue && Objeto.Activo.Value);

            object identidad = oDatos.LeerEscalar("sp_Idioma_Alta", Hdatos);

            return Convert.ToInt32(identidad);
        }

        public bool Baja(BEIdioma Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@IdiomaId", Objeto.IdiomaId);

            object filas = oDatos.LeerEscalar("sp_Idioma_Baja", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        public bool CambiarEstado(BEIdioma Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@IdiomaId", Objeto.IdiomaId);
            Hdatos.Add("@Activo", Objeto.Activo.HasValue && Objeto.Activo.Value);

            object filas = oDatos.LeerEscalar("sp_Idioma_CambiarEstado", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        private BEIdioma Mapear(DataRow Item)
        {
            BEIdioma oIdiomaBE = new BEIdioma();

            oIdiomaBE.IdiomaId = ServicioLectorFila.LeerEntero(Item, "IdiomaId");
            oIdiomaBE.Nombre = ServicioLectorFila.LeerTexto(Item, "Nombre");
            oIdiomaBE.CodigoISO = ServicioLectorFila.LeerTexto(Item, "CodigoISO").Trim();
            oIdiomaBE.Activo = ServicioLectorFila.LeerBooleanoNulo(Item, "Activo");

            return oIdiomaBE;
        }
    }

    public class MPPCultura
    {
        Acceso oDatos = new Acceso();
        Hashtable Hdatos;

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

            return ServicioLectorFila.AObjeto(oDatos.Leer("sp_Cultura_ObtenerPorId", Hdatos), Mapear);
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
            return ServicioLectorFila.ALista<BECulturaConIdioma>(oDatos.Leer(nombreSp, parametros), Mapear);
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

    public class MPPTraduccion
    {
        Acceso oDatos = new Acceso();
        Hashtable Hdatos;

        public Dictionary<string, string> ListarPorCodigo(BEIdioma Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@CodigoISO", Objeto.CodigoISO);

            Dictionary<string, string> Bundle = new Dictionary<string, string>();
            DataTable Dt = oDatos.Leer("sp_Traduccion_ListarPorCodigo", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                foreach (DataRow Item in Dt.Rows)
                {
                    string clave = ServicioLectorFila.LeerTexto(Item, "Clave");

                    if (!string.IsNullOrEmpty(clave))
                    {
                        Bundle[clave] = ServicioLectorFila.LeerTexto(Item, "Texto");
                    }
                }

                return Bundle;
            }
            else
            {
                return null;
            }
        }

        public List<BETraduccionAdministracion> ListarAdministracion(BEFiltroTraduccion Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@IdiomaId", Objeto.IdiomaId);
            Hdatos.Add("@Clave", Objeto.Clave);
            Hdatos.Add("@Texto", Objeto.Texto);
            Hdatos.Add("@SoloPendientes", Objeto.SoloPendientes);

            return ServicioLectorFila.ALista<BETraduccionAdministracion>(oDatos.Leer("sp_Traduccion_ListarAdministracion", Hdatos), Mapear);
        }

        public bool Guardar(BETraduccion Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@IdiomaId", Objeto.IdiomaId);
            Hdatos.Add("@Clave", Objeto.Clave);
            Hdatos.Add("@Texto", Objeto.Texto);

            object filas = oDatos.LeerEscalar("sp_Traduccion_Guardar", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        public int GenerarClavesFaltantes(BEIdioma Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@IdiomaId", Objeto.IdiomaId);

            object filas = oDatos.LeerEscalar("sp_Traduccion_GenerarClavesFaltantes", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas);
        }

        private BETraduccionAdministracion Mapear(DataRow Item)
        {
            BETraduccionAdministracion oTraduccionBE = new BETraduccionAdministracion();

            oTraduccionBE.Clave = ServicioLectorFila.LeerTexto(Item, "Clave");
            oTraduccionBE.TextoEspanol = ServicioLectorFila.LeerTexto(Item, "TextoEspanol");
            oTraduccionBE.Texto = ServicioLectorFila.LeerTexto(Item, "Texto");
            oTraduccionBE.Pendiente = ServicioLectorFila.LeerBooleano(Item, "Pendiente");

            return oTraduccionBE;
        }
    }
}
