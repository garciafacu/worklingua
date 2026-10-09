using System.Collections;
using System.Collections.Generic;
using System.Data;
using worklingua.Server.BE;
using worklingua.Server.DAL;
using worklingua.Server.Services;

namespace worklingua.Server.MPP
{
    public class MPPUsuario
    {
        Acceso oDatos;
        Hashtable Hdatos;
        ServicioCifrado oServicioCifrado;

        public MPPUsuario()
        {
            oDatos = new Acceso();
            oServicioCifrado = new ServicioCifrado();
        }

        public int Guardar(BEUsuario Objeto)
        {
            if (Objeto.UsuarioId != 0)
            {
                Hdatos = new Hashtable();
                Hdatos.Add("@UsuarioId", Objeto.UsuarioId);
                Hdatos.Add("@EmpresaId", Objeto.EmpresaId);
                Hdatos.Add("@DepartamentoId", Objeto.DepartamentoId);
                Hdatos.Add("@Idioma", Objeto.Idioma);
                Hdatos.Add("@Nombre", Objeto.Nombre);
                Hdatos.Add("@Apellido", Objeto.Apellido);
                AgregarDocumento(Hdatos, Objeto.Documento);
                Hdatos.Add("@Email", Objeto.Email);
                Hdatos.Add("@NivelIdioma", Objeto.NivelIdioma);
                Hdatos.Add("@FechaNacimiento", Objeto.FechaNacimiento);

                oDatos.LeerEscalar("sp_Usuario_Modificar", Hdatos);

                return Objeto.UsuarioId;
            }

            Hdatos = new Hashtable();
            Hdatos.Add("@EmpresaId", Objeto.EmpresaId);
            Hdatos.Add("@DepartamentoId", Objeto.DepartamentoId);
            Hdatos.Add("@Idioma", Objeto.Idioma);
            Hdatos.Add("@Nombre", Objeto.Nombre);
            Hdatos.Add("@Apellido", Objeto.Apellido);
            AgregarDocumento(Hdatos, Objeto.Documento);
            Hdatos.Add("@Email", Objeto.Email);
            Hdatos.Add("@PasswordHash", Objeto.PasswordHash);
            Hdatos.Add("@NivelIdioma", Objeto.NivelIdioma);
            Hdatos.Add("@FechaNacimiento", Objeto.FechaNacimiento);
            Hdatos.Add("@Activo", Objeto.Activo.HasValue && Objeto.Activo.Value);

            object identidad = oDatos.LeerEscalar("sp_Usuario_Alta", Hdatos);

            return Convert.ToInt32(identidad);
        }

        /// <summary>
        /// El documento cifrado y su huella, que van siempre juntos.
        ///
        /// Cuando no hay documento el parámetro de la huella no se manda: la DAL
        /// arma los parámetros con AddWithValue y un DBNull suelto se infiere
        /// como nvarchar, que SQL Server no convierte a varbinary. Los dos SP
        /// declaran @DocumentoHash con default NULL, así que omitirlo graba NULL,
        /// que es justo lo que corresponde.
        /// </summary>
        private void AgregarDocumento(Hashtable Hdatos, string documento)
        {
            Hdatos.Add("@Documento", oServicioCifrado.Cifrar(documento));

            byte[] huella = oServicioCifrado.HashDeterministico(documento);

            if (huella != null)
            {
                Hdatos.Add("@DocumentoHash", huella);
            }
        }

        /// <summary>
        /// Busca por la huella determinística del documento: el valor cifrado no
        /// se puede comparar en SQL. Ver <see cref="ServicioCifrado.HashDeterministico"/>.
        /// </summary>
        public BEUsuario ObtenerPorDocumento(BEUsuario Objeto)
        {
            byte[] huella = oServicioCifrado.HashDeterministico(Objeto.Documento);

            if (huella == null)
            {
                return null;
            }

            Hdatos = new Hashtable();
            Hdatos.Add("@DocumentoHash", huella);

            DataTable Dt = oDatos.Leer("sp_Usuario_ObtenerPorDocumentoHash", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                return Mapear(Dt.Rows[0]);
            }
            else
            {
                return null;
            }
        }

        public BEUsuario ObtenerPorEmail(BEUsuario Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@Email", Objeto.Email);

            DataTable Dt = oDatos.Leer("sp_Usuario_ObtenerPorEmail", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                return Mapear(Dt.Rows[0]);
            }
            else
            {
                return null;
            }
        }

        public BEUsuario ListarObjeto(BEUsuario Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@UsuarioId", Objeto.UsuarioId);

            DataTable Dt = oDatos.Leer("sp_Usuario_ObtenerPorId", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                return Mapear(Dt.Rows[0]);
            }
            else
            {
                return null;
            }
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

            List<BERol> ListaRolBE = new List<BERol>();
            DataTable Dt = oDatos.Leer("sp_Usuario_ObtenerRoles", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                foreach (DataRow Item in Dt.Rows)
                {
                    BERol oRolBE = new BERol();

                    oRolBE.RolId = ServicioLectorFila.LeerEntero(Item, "RolId");
                    oRolBE.Nombre = ServicioLectorFila.LeerTexto(Item, "Nombre");
                    oRolBE.Descripcion = ServicioLectorFila.LeerTextoNulo(Item, "Descripcion");
                    oRolBE.Activo = true;

                    ListaRolBE.Add(oRolBE);
                }

                return ListaRolBE;
            }
            else
            {
                return null;
            }
        }

        public List<BEUsuarioAdministracion> ListarAdministracion(BEUsuario Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@EmpresaId", Objeto.EmpresaId == 0 ? null : (object)Objeto.EmpresaId);

            List<BEUsuarioAdministracion> ListaUsuarioBE = new List<BEUsuarioAdministracion>();
            DataTable Dt = oDatos.Leer("sp_Usuario_ListarAdministracion", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                foreach (DataRow Item in Dt.Rows)
                {
                    ListaUsuarioBE.Add(MapearAdministracion(Item));
                }

                return ListaUsuarioBE;
            }
            else
            {
                return null;
            }
        }

        public List<BEUsuarioAdministracion> ListarOperadores(BEUsuario Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@EmpresaId", Objeto.EmpresaId);

            List<BEUsuarioAdministracion> ListaUsuarioBE = new List<BEUsuarioAdministracion>();
            DataTable Dt = oDatos.Leer("sp_Usuario_ListarOperadores", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                foreach (DataRow Item in Dt.Rows)
                {
                    ListaUsuarioBE.Add(MapearAdministracion(Item));
                }

                return ListaUsuarioBE;
            }
            else
            {
                return null;
            }
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

            List<BERolPermiso> ListaRolPermisoBE = new List<BERolPermiso>();
            DataTable Dt = oDatos.Leer("sp_Usuario_ObtenerPermisos", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                foreach (DataRow Item in Dt.Rows)
                {
                    BERolPermiso oRolPermisoBE = new BERolPermiso();

                    oRolPermisoBE.RolPermisoId = ServicioLectorFila.LeerEntero(Item, "RolPermisoId");
                    oRolPermisoBE.RolId = ServicioLectorFila.LeerEntero(Item, "RolId");
                    oRolPermisoBE.PermisoId = ServicioLectorFila.LeerEntero(Item, "PermisoId");

                    ListaRolPermisoBE.Add(oRolPermisoBE);
                }

                return ListaRolPermisoBE;
            }
            else
            {
                return null;
            }
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

        /// <summary>
        /// Suma un intento fallido y bloquea la cuenta si llega al tope. El SP
        /// hace las dos cosas en un solo UPDATE para que dos intentos
        /// simultáneos no se pisen el contador.
        ///
        /// Devuelve el estado resultante, que es lo que la BLL necesita para
        /// saber si tiene que informar credenciales inválidas o bloqueo.
        /// </summary>
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
            BEUsuario oUsuarioBE = new BEUsuario();

            oUsuarioBE.UsuarioId = ServicioLectorFila.LeerEntero(Item, "UsuarioId");
            oUsuarioBE.EmpresaId = ServicioLectorFila.LeerEntero(Item, "EmpresaId");
            // Los operadores comparten este mapeo y su SP no trae departamento:
            // el lector devuelve null cuando la columna no viene.
            oUsuarioBE.DepartamentoId = ServicioLectorFila.LeerEnteroNulo(Item, "DepartamentoId");
            oUsuarioBE.Nombre = ServicioLectorFila.LeerTexto(Item, "Nombre");
            oUsuarioBE.Apellido = ServicioLectorFila.LeerTexto(Item, "Apellido");
            oUsuarioBE.Documento = oServicioCifrado.Descifrar(ServicioLectorFila.LeerTextoNulo(Item, "Documento"));
            oUsuarioBE.Email = ServicioLectorFila.LeerTexto(Item, "Email");
            oUsuarioBE.FechaAlta = ServicioLectorFila.LeerFechaHoraNula(Item, "FechaAlta");
            oUsuarioBE.UltimoAcceso = ServicioLectorFila.LeerFechaHoraNula(Item, "UltimoAcceso");
            oUsuarioBE.Activo = ServicioLectorFila.LeerBooleanoNulo(Item, "Activo");
            oUsuarioBE.BloqueadoHasta = ServicioLectorFila.LeerFechaHoraNula(Item, "BloqueadoHasta");

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
}
