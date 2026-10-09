using System.Collections;
using System.Collections.Generic;
using System.Data;
using worklingua.Server.BE;
using worklingua.Server.DAL;
using worklingua.Server.Services;

namespace worklingua.Server.MPP
{
    public class MPPLicencia
    {
        Acceso oDatos;
        Hashtable Hdatos;

        public MPPLicencia()
        {
            oDatos = new Acceso();
        }

        public int Asignar(BELicencia Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@SuscripcionId", Objeto.SuscripcionId);
            Hdatos.Add("@UsuarioId", Objeto.UsuarioId);

            if (Objeto.FechaVencimiento.HasValue)
            {
                Hdatos.Add("@FechaVencimiento", Objeto.FechaVencimiento.Value);
            }

            object identidad = oDatos.LeerEscalar("sp_Licencia_Asignar", Hdatos);

            return Convert.ToInt32(identidad);
        }

        public bool Revocar(BELicencia Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@LicenciaId", Objeto.LicenciaId);

            object filas = oDatos.LeerEscalar("sp_Licencia_Revocar", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        /// <summary>Al cancelar una contratación. Devuelve cuántas revocó.</summary>
        public int RevocarPorSuscripcion(BELicencia Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@SuscripcionId", Objeto.SuscripcionId);

            object filas = oDatos.LeerEscalar("sp_Licencia_RevocarPorSuscripcion", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas);
        }

        /// <summary>Al dar de baja un usuario o moverlo de empresa.</summary>
        public int RevocarPorUsuario(BELicencia Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@UsuarioId", Objeto.UsuarioId);

            object filas = oDatos.LeerEscalar("sp_Licencia_RevocarPorUsuario", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas);
        }

        /// <summary>
        /// Muda las licencias vigentes a la suscripción nueva al cambiar de plan,
        /// hasta donde alcance el cupo destino. Devuelve cuántas se migraron.
        /// </summary>
        public int Migrar(int suscripcionOrigen, int suscripcionDestino, int cupoDestino, DateTime? fechaVencimiento)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@SuscripcionOrigen", suscripcionOrigen);
            Hdatos.Add("@SuscripcionDestino", suscripcionDestino);
            Hdatos.Add("@CupoDestino", cupoDestino);

            if (fechaVencimiento.HasValue)
            {
                Hdatos.Add("@FechaVencimiento", fechaVencimiento.Value);
            }

            DataTable Dt = oDatos.Leer("sp_Licencia_Migrar", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                return ServicioLectorFila.LeerEntero(Dt.Rows[0], "Migradas");
            }

            return 0;
        }

        public BELicencia ObtenerPorUsuario(BELicencia Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@UsuarioId", Objeto.UsuarioId);

            DataTable Dt = oDatos.Leer("sp_Licencia_ObtenerPorUsuario", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                return Mapear(Dt.Rows[0]);
            }
            else
            {
                return null;
            }
        }

        public BELicencia ObtenerPorId(BELicencia Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@LicenciaId", Objeto.LicenciaId);

            DataTable Dt = oDatos.Leer("sp_Licencia_ObtenerPorId", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                return Mapear(Dt.Rows[0]);
            }
            else
            {
                return null;
            }
        }

        public int ContarActivas(BELicencia Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@SuscripcionId", Objeto.SuscripcionId);

            DataTable Dt = oDatos.Leer("sp_Licencia_ContarActivas", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                return ServicioLectorFila.LeerEntero(Dt.Rows[0], "Activas");
            }

            return 0;
        }

        public List<BEInventarioLicencia> ListarPorEmpresa(int empresaId, int suscripcionId, string rol)
        {
            List<BEInventarioLicencia> ListaInventarioBE = new List<BEInventarioLicencia>();

            Hdatos = new Hashtable();
            Hdatos.Add("@EmpresaId", empresaId);
            Hdatos.Add("@SuscripcionId", suscripcionId);
            Hdatos.Add("@Rol", rol);

            DataTable Dt = oDatos.Leer("sp_Licencia_ListarPorEmpresa", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                foreach (DataRow Item in Dt.Rows)
                {
                    ListaInventarioBE.Add(MapearInventario(Item));
                }

                return ListaInventarioBE;
            }
            else
            {
                return null;
            }
        }

        private BELicencia Mapear(DataRow Item)
        {
            BELicencia oLicenciaBE = new BELicencia();

            oLicenciaBE.LicenciaId = ServicioLectorFila.LeerEntero(Item, "LicenciaId");
            oLicenciaBE.SuscripcionId = ServicioLectorFila.LeerEntero(Item, "SuscripcionId");
            oLicenciaBE.UsuarioId = ServicioLectorFila.LeerEnteroNulo(Item, "UsuarioId");
            oLicenciaBE.CodigoLicencia = LeerGuidNulo(Item, "CodigoLicencia");
            oLicenciaBE.FechaAsignacion = ServicioLectorFila.LeerFechaHoraNula(Item, "FechaAsignacion");
            oLicenciaBE.FechaVencimiento = ServicioLectorFila.LeerFechaHoraNula(Item, "FechaVencimiento");
            oLicenciaBE.Estado = ServicioLectorFila.LeerTextoNulo(Item, "Estado");
            oLicenciaBE.EstadoSuscripcion = ServicioLectorFila.LeerTextoNulo(Item, "EstadoSuscripcion");

            return oLicenciaBE;
        }

        private BEInventarioLicencia MapearInventario(DataRow Item)
        {
            BEInventarioLicencia oInventarioBE = new BEInventarioLicencia();

            oInventarioBE.UsuarioId = ServicioLectorFila.LeerEntero(Item, "UsuarioId");
            oInventarioBE.Nombre = ServicioLectorFila.LeerTexto(Item, "Nombre");
            oInventarioBE.Apellido = ServicioLectorFila.LeerTexto(Item, "Apellido");
            oInventarioBE.Email = ServicioLectorFila.LeerTexto(Item, "Email");
            oInventarioBE.Departamento = ServicioLectorFila.LeerTextoNulo(Item, "Departamento");
            oInventarioBE.LicenciaId = ServicioLectorFila.LeerEnteroNulo(Item, "LicenciaId");
            oInventarioBE.CodigoLicencia = LeerGuidNulo(Item, "CodigoLicencia");
            oInventarioBE.FechaAsignacion = ServicioLectorFila.LeerFechaHoraNula(Item, "FechaAsignacion");

            return oInventarioBE;
        }

        /// <summary>
        /// El lector compartido solo tiene LeerGuid, que devuelve Guid.Empty. Acá
        /// hace falta distinguir "sin licencia" de un código vacío.
        /// </summary>
        private Guid? LeerGuidNulo(DataRow Item, string columna)
        {
            return ServicioLectorFila.TieneValor(Item, columna)
                ? (Guid?)Guid.Parse(Convert.ToString(Item[columna]))
                : null;
        }
    }
}
