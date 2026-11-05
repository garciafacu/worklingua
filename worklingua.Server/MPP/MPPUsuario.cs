using System.Collections;
using System.Data;
using worklingua.Server.BE;
using worklingua.Server.DAL;
using worklingua.Server.Services;

namespace worklingua.Server.MPP
{
    public class MPPUsuario
    {
        Acceso oDatos = new Acceso();
        Hashtable Hdatos;
        ServicioCifrado oServicioCifrado = new ServicioCifrado();

        public int Guardar(BEUsuario Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@EmpresaId", Objeto.EmpresaId);
            Hdatos.Add("@DepartamentoId", Objeto.DepartamentoId);
            Hdatos.Add("@Idioma", Objeto.Idioma);
            Hdatos.Add("@Nombre", Objeto.Nombre);
            Hdatos.Add("@Apellido", Objeto.Apellido);
            AgregarDocumento(Hdatos, Objeto.Documento);
            Hdatos.Add("@Email", Objeto.Email);
            Hdatos.Add("@NivelIdioma", Objeto.NivelIdioma);
            Hdatos.Add("@FechaNacimiento", Objeto.FechaNacimiento);

            if (Objeto.UsuarioId != 0)
            {
                Hdatos.Add("@UsuarioId", Objeto.UsuarioId);

                oDatos.LeerEscalar("sp_Usuario_Modificar", Hdatos);

                return Objeto.UsuarioId;
            }

            Hdatos.Add("@PasswordHash", Objeto.PasswordHash);
            Hdatos.Add("@Activo", Objeto.Activo.HasValue && Objeto.Activo.Value);

            object identidad = oDatos.LeerEscalar("sp_Usuario_Alta", Hdatos);

            return Convert.ToInt32(identidad);
        }

        private void AgregarDocumento(Hashtable Hdatos, string documento)
        {
            Hdatos.Add("@Documento", oServicioCifrado.Cifrar(documento));

            byte[] huella = oServicioCifrado.HashDeterministico(documento);

            if (huella != null)
            {
                Hdatos.Add("@DocumentoHash", huella);
            }
        }

        public BEUsuario ObtenerPorDocumento(BEUsuario Objeto)
        {
            byte[] huella = oServicioCifrado.HashDeterministico(Objeto.Documento);

            if (huella == null)
            {
                return null;
            }

            Hdatos = new Hashtable();
            Hdatos.Add("@DocumentoHash", huella);

            return ServicioLectorFila.AObjeto(oDatos.Leer("sp_Usuario_ObtenerPorDocumentoHash", Hdatos), Mapear);
        }

        public BEUsuario ObtenerPorEmail(BEUsuario Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@Email", Objeto.Email);

            return ServicioLectorFila.AObjeto(oDatos.Leer("sp_Usuario_ObtenerPorEmail", Hdatos), Mapear);
        }

        public BEUsuario ListarObjeto(BEUsuario Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@UsuarioId", Objeto.UsuarioId);

            return ServicioLectorFila.AObjeto(oDatos.Leer("sp_Usuario_ObtenerPorId", Hdatos), Mapear);
        }

        public bool AsignarRol(BEUsuarioRol Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@UsuarioId", Objeto.UsuarioId);
            Hdatos.Add("@RolId", Objeto.RolId);

            return oDatos.Escribir("sp_Usuario_AsignarRol", Hdatos);
        }

        public List<BERol> ObtenerRoles(BEUsuario Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@UsuarioId", Objeto.UsuarioId);

            return ServicioLectorFila.ALista(oDatos.Leer("sp_Usuario_ObtenerRoles", Hdatos), MapearRol);
        }

        private BERol MapearRol(DataRow Item)
        {
            BERol oRolBE = new BERol();

            oRolBE.RolId = ServicioLectorFila.LeerEntero(Item, "RolId");
            oRolBE.Nombre = ServicioLectorFila.LeerTexto(Item, "Nombre");
            oRolBE.Descripcion = ServicioLectorFila.LeerTextoNulo(Item, "Descripcion");
            oRolBE.Activo = true;

            return oRolBE;
        }

        public List<BEUsuarioAdministracion> ListarAdministracion(BEUsuario Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@EmpresaId", Objeto.EmpresaId == 0 ? null : (object)Objeto.EmpresaId);

            return ServicioLectorFila.ALista<BEUsuarioAdministracion>(oDatos.Leer("sp_Usuario_ListarAdministracion", Hdatos), MapearAdministracion);
        }

        public List<BEUsuarioAdministracion> ListarOperadores(BEUsuario Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@EmpresaId", Objeto.EmpresaId);

            return ServicioLectorFila.ALista<BEUsuarioAdministracion>(oDatos.Leer("sp_Usuario_ListarOperadores", Hdatos), MapearAdministracion);
        }

        public bool QuitarRol(BEUsuarioRol Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@UsuarioId", Objeto.UsuarioId);
            Hdatos.Add("@RolId", Objeto.RolId);

            object filas = oDatos.LeerEscalar("sp_Usuario_QuitarRol", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        public bool ReemplazarRol(BEUsuarioRol Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@UsuarioId", Objeto.UsuarioId);
            Hdatos.Add("@RolId", Objeto.RolId);

            object filas = oDatos.LeerEscalar("sp_Usuario_ReemplazarRol", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        public bool Baja(BEUsuario Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@UsuarioId", Objeto.UsuarioId);

            object filas = oDatos.LeerEscalar("sp_Usuario_Baja", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        public List<BERolPermiso> ObtenerPermisos(BEUsuario Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@UsuarioId", Objeto.UsuarioId);

            return ServicioLectorFila.ALista(oDatos.Leer("sp_Usuario_ObtenerPermisos", Hdatos), MapearRolPermiso);
        }

        private BERolPermiso MapearRolPermiso(DataRow Item)
        {
            return new BERolPermiso(
                ServicioLectorFila.LeerEntero(Item, "RolPermisoId"),
                ServicioLectorFila.LeerEntero(Item, "RolId"),
                ServicioLectorFila.LeerEntero(Item, "PermisoId"));
        }

        public bool ActivarCuenta(BEUsuario Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@UsuarioId", Objeto.UsuarioId);

            object filas = oDatos.LeerEscalar("sp_Usuario_ActivarCuenta", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        public bool ActualizarPasswordHash(BEUsuario Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@UsuarioId", Objeto.UsuarioId);
            Hdatos.Add("@PasswordHash", Objeto.PasswordHash);

            object filas = oDatos.LeerEscalar("sp_Usuario_ActualizarPasswordHash", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        public bool ActualizarUltimoAcceso(BEUsuario Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@UsuarioId", Objeto.UsuarioId);

            return oDatos.Escribir("sp_Usuario_ActualizarUltimoAcceso", Hdatos);
        }

        public BEUsuario RegistrarIntentoFallido(BEUsuario Objeto, int intentosMaximos, int minutosBloqueo)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@UsuarioId", Objeto.UsuarioId);
            Hdatos.Add("@IntentosMaximos", intentosMaximos);
            Hdatos.Add("@MinutosBloqueo", minutosBloqueo);

            DataTable Dt = oDatos.Leer("sp_Usuario_RegistrarIntentoFallido", Hdatos);

            if (Dt.Rows.Count == 0)
            {
                return null;
            }

            BEUsuario oEstadoBE = new BEUsuario();

            oEstadoBE.IntentosFallidos = ServicioLectorFila.LeerEntero(Dt.Rows[0], "IntentosFallidos");
            oEstadoBE.BloqueadoHasta = ServicioLectorFila.LeerFechaHoraNula(Dt.Rows[0], "BloqueadoHasta");

            return oEstadoBE;
        }

        public bool Desbloquear(BEUsuario Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@UsuarioId", Objeto.UsuarioId);

            object filas = oDatos.LeerEscalar("sp_Usuario_Desbloquear", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        private BEUsuarioAdministracion MapearAdministracion(DataRow Item)
        {
            BEUsuario oUsuarioBE = Mapear(Item);

            List<string> roles = new List<string>();
            string concatenados = ServicioLectorFila.LeerTexto(Item, "Roles");

            if (!string.IsNullOrWhiteSpace(concatenados))
            {
                foreach (string nombre in concatenados.Split(','))
                {
                    roles.Add(nombre.Trim());
                }
            }

            BEUsuarioAdministracion oUsuarioAdministracionBE = new BEUsuarioAdministracion(
                oUsuarioBE,
                ServicioLectorFila.LeerTexto(Item, "Empresa"),
                ServicioLectorFila.LeerEntero(Item, "RolId"),
                roles);

            oUsuarioAdministracionBE.Departamento = ServicioLectorFila.LeerTextoNulo(Item, "Departamento");

            return oUsuarioAdministracionBE;
        }

        private BEUsuario Mapear(DataRow Item)
        {
            BEUsuario oUsuarioBE = new BEUsuario();

            oUsuarioBE.UsuarioId = ServicioLectorFila.LeerEntero(Item, "UsuarioId");
            oUsuarioBE.EmpresaId = ServicioLectorFila.LeerEntero(Item, "EmpresaId");
            oUsuarioBE.DepartamentoId = ServicioLectorFila.LeerEnteroNulo(Item, "DepartamentoId");
            oUsuarioBE.Idioma = ServicioLectorFila.LeerTextoNulo(Item, "Idioma");
            oUsuarioBE.Nombre = ServicioLectorFila.LeerTexto(Item, "Nombre");
            oUsuarioBE.Apellido = ServicioLectorFila.LeerTexto(Item, "Apellido");
            oUsuarioBE.Documento = oServicioCifrado.Descifrar(ServicioLectorFila.LeerTextoNulo(Item, "Documento"));
            oUsuarioBE.Email = ServicioLectorFila.LeerTexto(Item, "Email");
            oUsuarioBE.PasswordHash = ServicioLectorFila.LeerTexto(Item, "PasswordHash");
            oUsuarioBE.NivelIdioma = ServicioLectorFila.LeerTextoNulo(Item, "NivelIdioma");
            oUsuarioBE.FechaNacimiento = ServicioLectorFila.LeerFechaNula(Item, "FechaNacimiento");
            oUsuarioBE.FechaAlta = ServicioLectorFila.LeerFechaHoraNula(Item, "FechaAlta");
            oUsuarioBE.UltimoAcceso = ServicioLectorFila.LeerFechaHoraNula(Item, "UltimoAcceso");
            oUsuarioBE.Activo = ServicioLectorFila.LeerBooleanoNulo(Item, "Activo");
            oUsuarioBE.IntentosFallidos = ServicioLectorFila.LeerEntero(Item, "IntentosFallidos");
            oUsuarioBE.BloqueadoHasta = ServicioLectorFila.LeerFechaHoraNula(Item, "BloqueadoHasta");

            return oUsuarioBE;
        }
    }

    public class MPPSesion
    {
        Acceso oDatos = new Acceso();
        Hashtable Hdatos;

        public BESesion Guardar(BESesion Objeto)
        {
            if (Objeto.SesionId != 0)
            {
                throw new NotImplementedException("No existe sp_Sesion_Modificar: una sesión se abre o se cierra, no se modifica.");
            }

            Hdatos = new Hashtable();
            Hdatos.Add("@UsuarioId", Objeto.UsuarioId);

            DataTable Dt = oDatos.Leer("sp_Sesion_Alta", Hdatos);

            if (Dt.Rows.Count == 0)
            {
                throw new InvalidOperationException("sp_Sesion_Alta no devolvió la sesión creada.");
            }

            return Mapear(Dt.Rows[0]);
        }

        public BESesion ListarObjeto(BESesion Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@Token", Objeto.Token);

            return ServicioLectorFila.AObjeto(oDatos.Leer("sp_Sesion_ObtenerPorToken", Hdatos), Mapear);
        }

        public bool Baja(BESesion Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@Token", Objeto.Token);

            object filas = oDatos.LeerEscalar("sp_Sesion_Cerrar", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        public int CerrarPorUsuario(BESesion Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@UsuarioId", Objeto.UsuarioId);
            Hdatos.Add("@TokenExcluido", Objeto.Token == Guid.Empty ? null : (object)Objeto.Token);

            object filas = oDatos.LeerEscalar("sp_Sesion_CerrarPorUsuario", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas);
        }

        private BESesion Mapear(DataRow Item)
        {
            BESesion oSesionBE = new BESesion();

            oSesionBE.SesionId = ServicioLectorFila.LeerEntero(Item, "SesionId");
            oSesionBE.UsuarioId = ServicioLectorFila.LeerEntero(Item, "UsuarioId");
            oSesionBE.Token = ServicioLectorFila.LeerGuid(Item, "Token");
            oSesionBE.FechaInicio = ServicioLectorFila.LeerFechaHora(Item, "FechaInicio");
            oSesionBE.FechaFin = ServicioLectorFila.LeerFechaHoraNula(Item, "FechaFin");
            oSesionBE.Activa = ServicioLectorFila.LeerBooleanoNulo(Item, "Activa");

            return oSesionBE;
        }
    }

    public class MPPTokenSeguridad
    {
        Acceso oDatos = new Acceso();
        Hashtable Hdatos;

        public BETokenSeguridad Guardar(BETokenSeguridad Objeto)
        {
            if (Objeto.TokenId != 0)
            {
                throw new NotImplementedException("No existe sp_TokenSeguridad_Modificar: un token se genera, se usa o se invalida.");
            }

            int MinutosVigencia = (int)(Objeto.FechaExpiracion - Objeto.FechaGeneracion).TotalMinutes;

            Hdatos = new Hashtable();
            Hdatos.Add("@UsuarioId", Objeto.UsuarioId);
            Hdatos.Add("@Tipo", Objeto.Tipo);
            Hdatos.Add("@MinutosVigencia", MinutosVigencia);

            DataTable Dt = oDatos.Leer("sp_TokenSeguridad_Alta", Hdatos);

            if (Dt.Rows.Count == 0)
            {
                throw new InvalidOperationException("sp_TokenSeguridad_Alta no devolvió el token creado.");
            }

            return Mapear(Dt.Rows[0]);
        }

        public BETokenSeguridad ListarObjeto(BETokenSeguridad Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@Token", Objeto.Token);

            return ServicioLectorFila.AObjeto(oDatos.Leer("sp_TokenSeguridad_ObtenerPorToken", Hdatos), Mapear);
        }

        public bool MarcarUsado(BETokenSeguridad Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@Token", Objeto.Token);

            object filas = oDatos.LeerEscalar("sp_TokenSeguridad_MarcarUsado", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        public int InvalidarPorUsuarioYTipo(BETokenSeguridad Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@UsuarioId", Objeto.UsuarioId);
            Hdatos.Add("@Tipo", Objeto.Tipo);

            object filas = oDatos.LeerEscalar("sp_TokenSeguridad_InvalidarPorUsuarioYTipo", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas);
        }

        private BETokenSeguridad Mapear(DataRow Item)
        {
            BETokenSeguridad oTokenBE = new BETokenSeguridad();

            oTokenBE.TokenId = ServicioLectorFila.LeerEntero(Item, "TokenId");
            oTokenBE.UsuarioId = ServicioLectorFila.LeerEntero(Item, "UsuarioId");
            oTokenBE.Token = ServicioLectorFila.LeerGuid(Item, "Token");
            oTokenBE.Tipo = ServicioLectorFila.LeerTexto(Item, "Tipo");
            oTokenBE.FechaGeneracion = ServicioLectorFila.LeerFechaHora(Item, "FechaGeneracion");
            oTokenBE.FechaExpiracion = ServicioLectorFila.LeerFechaHora(Item, "FechaExpiracion");
            oTokenBE.FechaUso = ServicioLectorFila.LeerFechaHoraNula(Item, "FechaUso");
            oTokenBE.Activo = ServicioLectorFila.LeerBooleano(Item, "Activo");

            return oTokenBE;
        }
    }
}
