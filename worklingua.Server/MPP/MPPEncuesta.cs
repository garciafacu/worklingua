using System.Collections;
using System.Data;
using worklingua.Server.BE;
using worklingua.Server.DAL;
using worklingua.Server.Services;

namespace worklingua.Server.MPP
{
    public class MPPEncuesta
    {
        Acceso oDatos = new Acceso();
        Hashtable Hdatos;

        public int Guardar(BEEncuesta Objeto)
        {
            Hdatos = new Hashtable();
            string Consulta = "sp_Encuesta_Alta";

            if (Objeto.EncuestaId != 0)
            {
                Hdatos.Add("@EncuestaId", Objeto.EncuestaId);
                Consulta = "sp_Encuesta_Modificar";
            }
            else
            {
                Hdatos.Add("@UsuarioId", Objeto.UsuarioId);
            }

            Hdatos.Add("@IdiomaId", Objeto.IdiomaId);
            Hdatos.Add("@Pregunta", Objeto.Pregunta);
            Hdatos.Add("@Descripcion", Objeto.Descripcion);
            Hdatos.Add("@FechaDesde", Objeto.FechaDesde.ToDateTime(TimeOnly.MinValue));
            Hdatos.Add("@FechaVencimiento", Objeto.FechaVencimiento.ToDateTime(TimeOnly.MinValue));
            Hdatos.Add("@Activo", Objeto.Activo);
            Hdatos.Add("@EsPublica", Objeto.EsPublica);

            object resultado = oDatos.LeerEscalar(Consulta, Hdatos);

            return Objeto.EncuestaId != 0 ? Objeto.EncuestaId : Convert.ToInt32(resultado);
        }

        public bool Baja(BEEncuesta Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@EncuestaId", Objeto.EncuestaId);

            object filas = oDatos.LeerEscalar("sp_Encuesta_Baja", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        public List<BEEncuestaConDetalle> Listar(BEFiltroEncuesta Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@EncuestaId", Objeto.EncuestaId);
            Hdatos.Add("@IdiomaId", Objeto.IdiomaId);
            Hdatos.Add(
                "@VigentesAl",
                Objeto.VigentesAl.HasValue
                    ? (object)Objeto.VigentesAl.Value.ToDateTime(TimeOnly.MinValue)
                    : null);
            Hdatos.Add("@SoloPublicas", Objeto.SoloPublicas);

            List<BEEncuestaConDetalle> ListaEncuestaBE = new List<BEEncuestaConDetalle>();
            DataTable Dt = oDatos.Leer("sp_Encuesta_Listar", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                foreach (DataRow Item in Dt.Rows)
                {
                    BEEncuesta oEncuestaBE = new BEEncuesta(
                        ServicioLectorFila.LeerEntero(Item, "EncuestaId"),
                        ServicioLectorFila.LeerEntero(Item, "IdiomaId"),
                        ServicioLectorFila.LeerTexto(Item, "Pregunta"),
                        ServicioLectorFila.LeerTextoNulo(Item, "Descripcion"),
                        ServicioLectorFila.LeerFechaNula(Item, "FechaDesde") ?? default,
                        ServicioLectorFila.LeerFechaNula(Item, "FechaVencimiento") ?? default,
                        ServicioLectorFila.LeerBooleano(Item, "Activo"),
                        ServicioLectorFila.LeerBooleano(Item, "EsPublica"),
                        ServicioLectorFila.LeerEntero(Item, "UsuarioId"),
                        ServicioLectorFila.LeerFechaHora(Item, "FechaAlta"));

                    BEEncuestaConDetalle oDetalleBE = new BEEncuestaConDetalle();

                    oDetalleBE.Encuesta = oEncuestaBE;
                    oDetalleBE.Idioma = ServicioLectorFila.LeerTexto(Item, "Idioma");
                    oDetalleBE.CodigoISO = ServicioLectorFila.LeerTexto(Item, "CodigoISO");
                    oDetalleBE.TotalRespuestas = ServicioLectorFila.LeerEntero(Item, "TotalRespuestas");

                    ListaEncuestaBE.Add(oDetalleBE);
                }

                return ListaEncuestaBE;
            }
            else
            {
                return null;
            }
        }

        public int GuardarOpcion(BEOpcionEncuesta Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@EncuestaId", Objeto.EncuestaId);
            Hdatos.Add("@Texto", Objeto.Texto);
            Hdatos.Add("@Orden", Objeto.Orden);

            object resultado = oDatos.LeerEscalar("sp_OpcionEncuesta_Alta", Hdatos);

            return Convert.ToInt32(resultado);
        }

        public bool BajaOpciones(BEEncuesta Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@EncuestaId", Objeto.EncuestaId);

            object filas = oDatos.LeerEscalar("sp_OpcionEncuesta_BajaPorEncuesta", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        public List<BEOpcionEncuesta> ListarOpciones(BEFiltroEncuesta Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@EncuestaId", Objeto.EncuestaId);

            List<BEOpcionEncuesta> ListaOpcionBE = new List<BEOpcionEncuesta>();
            DataTable Dt = oDatos.Leer("sp_OpcionEncuesta_Listar", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                foreach (DataRow Item in Dt.Rows)
                {
                    ListaOpcionBE.Add(new BEOpcionEncuesta(
                        ServicioLectorFila.LeerEntero(Item, "OpcionEncuestaId"),
                        ServicioLectorFila.LeerEntero(Item, "EncuestaId"),
                        ServicioLectorFila.LeerTexto(Item, "Texto"),
                        ServicioLectorFila.LeerEntero(Item, "Orden"),
                        ServicioLectorFila.LeerEntero(Item, "Total")));
                }

                return ListaOpcionBE;
            }
            else
            {
                return null;
            }
        }

        public int GuardarRespuesta(BEResponderEncuesta Objeto, BEVotanteEncuesta oVotanteBE)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@EncuestaId", Objeto.EncuestaId);
            Hdatos.Add("@OpcionEncuestaId", Objeto.OpcionEncuestaId);
            Hdatos.Add("@UsuarioId", oVotanteBE.UsuarioId);
            Hdatos.Add("@TokenVisitante", oVotanteBE.TokenVisitante);

            object resultado = oDatos.LeerEscalar("sp_RespuestaEncuesta_Alta", Hdatos);

            return Convert.ToInt32(resultado);
        }

        public Dictionary<int, int> ListarRespuestasDeVotante(BEFiltroEncuesta Objeto, BEVotanteEncuesta oVotanteBE)
        {
            Dictionary<int, int> Respuestas = new Dictionary<int, int>();

            if (oVotanteBE == null || (!oVotanteBE.UsuarioId.HasValue && !oVotanteBE.TokenVisitante.HasValue))
            {
                return Respuestas;
            }

            Hdatos = new Hashtable();
            Hdatos.Add("@EncuestaId", Objeto.EncuestaId);

            string Consulta;

            if (oVotanteBE.UsuarioId.HasValue)
            {
                Hdatos.Add("@UsuarioId", oVotanteBE.UsuarioId.Value);
                Consulta = "sp_RespuestaEncuesta_ListarPorUsuario";
            }
            else
            {
                Hdatos.Add("@TokenVisitante", oVotanteBE.TokenVisitante.Value);
                Consulta = "sp_RespuestaEncuesta_ListarPorVisitante";
            }

            DataTable Dt = oDatos.Leer(Consulta, Hdatos);

            foreach (DataRow Item in Dt.Rows)
            {
                Respuestas[ServicioLectorFila.LeerEntero(Item, "EncuestaId")] =
                    ServicioLectorFila.LeerEntero(Item, "OpcionEncuestaId");
            }

            return Respuestas;
        }
    }
}
