using System.Collections;
using System.Collections.Generic;
using System.Data;
using worklingua.Server.BE;
using worklingua.Server.DAL;
using worklingua.Server.Services;

namespace worklingua.Server.MPP
{
    public class MPPBitacora
    {
        Acceso oDatos;
        Hashtable Hdatos;

        public MPPBitacora()
        {
            oDatos = new Acceso();
        }

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
            BEBitacoraEvento oEventoBE = new BEBitacoraEvento();

            oEventoBE.BitacoraId = ServicioLectorFila.LeerEntero(Item, "BitacoraId");
            oEventoBE.UsuarioId = ServicioLectorFila.LeerEnteroNulo(Item, "UsuarioId");
            oEventoBE.FechaEvento = ServicioLectorFila.LeerFechaHora(Item, "FechaEvento");
            oEventoBE.Modulo = ServicioLectorFila.LeerTextoNulo(Item, "Modulo");
            oEventoBE.Accion = ServicioLectorFila.LeerTextoNulo(Item, "Accion");
            oEventoBE.Descripcion = ServicioLectorFila.LeerTextoNulo(Item, "Descripcion");
            oEventoBE.Nivel = ServicioLectorFila.LeerTextoNulo(Item, "Nivel");

            return new BEBitacoraEventoConUsuario(
                oEventoBE,
                ServicioLectorFila.LeerTexto(Item, "Usuario"),
                ServicioLectorFila.LeerTextoNulo(Item, "Email"));
        }
    }
}
