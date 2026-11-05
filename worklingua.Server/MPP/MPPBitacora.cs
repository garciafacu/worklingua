using System.Collections;
using System.Data;
using worklingua.Server.BE;
using worklingua.Server.DAL;
using worklingua.Server.Services;

namespace worklingua.Server.MPP
{
    public class MPPBitacora
    {
        Acceso oDatos = new Acceso();
        Hashtable Hdatos;

        public bool Guardar(BEBitacoraEvento Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@UsuarioId", Objeto.UsuarioId);
            Hdatos.Add("@Modulo", Objeto.Modulo);
            Hdatos.Add("@Accion", Objeto.Accion);
            Hdatos.Add("@Descripcion", Objeto.Descripcion);
            Hdatos.Add("@Nivel", Objeto.Nivel);

            return oDatos.Escribir("sp_Bitacora_Registrar", Hdatos);
        }

        public BEPaginaBitacora Buscar(BEFiltroBitacora Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@Texto", Objeto.Texto);
            Hdatos.Add("@Usuario", Objeto.Usuario);
            Hdatos.Add("@Modulo", Objeto.Modulo);
            Hdatos.Add("@Accion", Objeto.Accion);
            Hdatos.Add("@Nivel", Objeto.Nivel);
            Hdatos.Add("@FechaDesde", Objeto.Desde);
            Hdatos.Add("@FechaHasta", Objeto.Hasta);
            Hdatos.Add("@Pagina", Objeto.Pagina);
            Hdatos.Add("@TamanioPagina", Objeto.TamanioPagina);

            BEPaginaBitacora oPaginaBE = new BEPaginaBitacora();

            oPaginaBE.Pagina = Objeto.Pagina.Value;
            oPaginaBE.TamanioPagina = Objeto.TamanioPagina.Value;

            DataTable Dt = oDatos.Leer("sp_Bitacora_Buscar", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                foreach (DataRow Item in Dt.Rows)
                {
                    oPaginaBE.Registros.Add(Mapear(Item));
                }

                oPaginaBE.TotalRegistros = ServicioLectorFila.LeerEntero(Dt.Rows[0], "TotalRegistros");
            }

            return oPaginaBE;
        }

        private BEBitacoraEventoConUsuario Mapear(DataRow Item)
        {
            BEBitacoraEventoConUsuario oRegistroBE = new BEBitacoraEventoConUsuario();

            oRegistroBE.BitacoraId = ServicioLectorFila.LeerEntero(Item, "BitacoraId");
            oRegistroBE.UsuarioId = ServicioLectorFila.LeerEnteroNulo(Item, "UsuarioId");
            oRegistroBE.FechaEvento = ServicioLectorFila.LeerFechaHora(Item, "FechaEvento");
            oRegistroBE.Modulo = ServicioLectorFila.LeerTextoNulo(Item, "Modulo");
            oRegistroBE.Accion = ServicioLectorFila.LeerTextoNulo(Item, "Accion");
            oRegistroBE.Descripcion = ServicioLectorFila.LeerTextoNulo(Item, "Descripcion");
            oRegistroBE.Nivel = ServicioLectorFila.LeerTextoNulo(Item, "Nivel");
            oRegistroBE.Usuario = ServicioLectorFila.LeerTexto(Item, "Usuario");
            oRegistroBE.Email = ServicioLectorFila.LeerTextoNulo(Item, "Email");

            return oRegistroBE;
        }
    }
}
