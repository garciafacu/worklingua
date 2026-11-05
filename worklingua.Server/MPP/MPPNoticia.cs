using System.Collections;
using System.Data;
using worklingua.Server.BE;
using worklingua.Server.DAL;
using worklingua.Server.Services;

namespace worklingua.Server.MPP
{
    public class MPPNoticia
    {
        Acceso oDatos = new Acceso();
        Hashtable Hdatos;

        public List<BENoticia> ListarAdministracion(BEFiltroNoticia Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@IdiomaId", Objeto.IdiomaId);

            return ServicioLectorFila.ALista<BENoticia>(oDatos.Leer("sp_Noticia_ListarAdministracion", Hdatos), Mapear);
        }

        public List<BENoticia> ListarPublicadas(BEIdioma Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@IdiomaId", Objeto.IdiomaId);

            return ServicioLectorFila.ALista<BENoticia>(oDatos.Leer("sp_Noticia_ListarPublicadas", Hdatos), Mapear);
        }

        public BENoticia ListarObjeto(BENoticia Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@NoticiaId", Objeto.NoticiaId);

            return ServicioLectorFila.AObjeto(oDatos.Leer("sp_Noticia_ObtenerPorId", Hdatos), Mapear);
        }

        public BENoticia ListarObjetoPublicada(BENoticia Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@NoticiaId", Objeto.NoticiaId);

            return ServicioLectorFila.AObjeto(oDatos.Leer("sp_Noticia_ObtenerPublicada", Hdatos), Mapear);
        }

        public int Guardar(BENoticia Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@IdiomaId", Objeto.IdiomaId);
            Hdatos.Add("@Titulo", Objeto.Titulo);
            Hdatos.Add("@Resumen", Objeto.Resumen);
            Hdatos.Add("@Contenido", Objeto.Contenido);
            Hdatos.Add("@FechaPublicacion", Objeto.FechaPublicacion);

            if (Objeto.NoticiaId != 0)
            {
                Hdatos.Add("@NoticiaId", Objeto.NoticiaId);

                oDatos.LeerEscalar("sp_Noticia_Modificar", Hdatos);

                return Objeto.NoticiaId;
            }

            Hdatos.Add("@UsuarioId", Objeto.UsuarioId);

            object identidad = oDatos.LeerEscalar("sp_Noticia_Alta", Hdatos);

            return Convert.ToInt32(identidad);
        }

        public bool Baja(BENoticia Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@NoticiaId", Objeto.NoticiaId);

            object filas = oDatos.LeerEscalar("sp_Noticia_Baja", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        private BENoticia Mapear(DataRow Item)
        {
            BENoticia oNoticiaBE = new BENoticia();

            oNoticiaBE.NoticiaId = ServicioLectorFila.LeerEntero(Item, "NoticiaId");
            oNoticiaBE.IdiomaId = ServicioLectorFila.LeerEntero(Item, "IdiomaId");
            oNoticiaBE.UsuarioId = ServicioLectorFila.LeerEntero(Item, "UsuarioId");
            oNoticiaBE.Titulo = ServicioLectorFila.LeerTexto(Item, "Titulo");
            oNoticiaBE.Resumen = ServicioLectorFila.LeerTextoNulo(Item, "Resumen");
            oNoticiaBE.Contenido = ServicioLectorFila.LeerTexto(Item, "Contenido");
            oNoticiaBE.FechaPublicacion = ServicioLectorFila.LeerFechaHora(Item, "FechaPublicacion");
            oNoticiaBE.FechaAlta = ServicioLectorFila.LeerFechaHora(Item, "FechaAlta");
            oNoticiaBE.Activo = ServicioLectorFila.LeerBooleano(Item, "Activo");

            return oNoticiaBE;
        }
    }

    public class MPPSuscriptorNewsletter
    {
        Acceso oDatos = new Acceso();
        Hashtable Hdatos;

        public List<BESuscriptorNewsletter> ListarTodo()
        {
            return ServicioLectorFila.ALista<BESuscriptorNewsletter>(oDatos.Leer("sp_SuscriptorNewsletter_Listar", null), Mapear);
        }

        public List<BESuscriptorNewsletter> ListarConfirmados(BEIdioma Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@IdiomaId", Objeto.IdiomaId);

            return ServicioLectorFila.ALista<BESuscriptorNewsletter>(oDatos.Leer("sp_SuscriptorNewsletter_ListarConfirmadosPorIdioma", Hdatos), Mapear);
        }

        public BESuscriptorNewsletter ListarObjeto(BESuscriptorNewsletter Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@SuscriptorId", Objeto.SuscriptorId);

            return ServicioLectorFila.AObjeto<BESuscriptorNewsletter>(oDatos.Leer("sp_SuscriptorNewsletter_ObtenerPorId", Hdatos), Mapear);
        }

        public BESuscriptorNewsletter ListarPorEmail(BESuscriptorNewsletter Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@Email", Objeto.Email);

            return ServicioLectorFila.AObjeto<BESuscriptorNewsletter>(oDatos.Leer("sp_SuscriptorNewsletter_ObtenerPorEmail", Hdatos), Mapear);
        }

        public BESuscriptorNewsletter ListarPorToken(BESuscriptorNewsletter Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@Token", Objeto.Token);

            return ServicioLectorFila.AObjeto<BESuscriptorNewsletter>(oDatos.Leer("sp_SuscriptorNewsletter_ObtenerPorToken", Hdatos), Mapear);
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

    public class MPPEnvioNewsletter
    {
        Acceso oDatos = new Acceso();
        Hashtable Hdatos;

        public List<BEEnvioNewsletter> ListarTodo()
        {
            return ServicioLectorFila.ALista<BEEnvioNewsletter>(oDatos.Leer("sp_EnvioNewsletter_Listar", null), Mapear);
        }

        public int Guardar(BEEnvioNewsletter Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@IdiomaId", Objeto.IdiomaId);
            Hdatos.Add("@UsuarioId", Objeto.UsuarioId);
            Hdatos.Add("@Asunto", Objeto.Asunto);
            Hdatos.Add("@CantidadEnviados", Objeto.CantidadEnviados);
            Hdatos.Add("@CantidadFallidos", Objeto.CantidadFallidos);

            object identidad = oDatos.LeerEscalar("sp_EnvioNewsletter_Alta", Hdatos);

            return Convert.ToInt32(identidad);
        }

        private BEEnvioNewsletter Mapear(DataRow Item)
        {
            BEEnvioNewsletter oEnvioBE = new BEEnvioNewsletter();

            oEnvioBE.EnvioId = ServicioLectorFila.LeerEntero(Item, "EnvioId");
            oEnvioBE.IdiomaId = ServicioLectorFila.LeerEntero(Item, "IdiomaId");
            oEnvioBE.UsuarioId = ServicioLectorFila.LeerEntero(Item, "UsuarioId");
            oEnvioBE.Asunto = ServicioLectorFila.LeerTexto(Item, "Asunto");
            oEnvioBE.FechaEnvio = ServicioLectorFila.LeerFechaHora(Item, "FechaEnvio");
            oEnvioBE.CantidadEnviados = ServicioLectorFila.LeerEntero(Item, "CantidadEnviados");
            oEnvioBE.CantidadFallidos = ServicioLectorFila.LeerEntero(Item, "CantidadFallidos");

            return oEnvioBE;
        }
    }
}
