using System.Collections.Generic;
using System.Globalization;
using System.Text;
using worklingua.Server.BE;
using worklingua.Server.MPP;
using worklingua.Server.Services;

namespace worklingua.Server.BLL
{
    /// <summary>
    /// Alta masiva de empleados desde el padrón (CU-001-002).
    ///
    /// El procesamiento tiene dos pasos y los dos pasan por acá: primero se
    /// valida todo sin escribir nada (previsualización) y después se confirma.
    /// El archivo entero viaja las dos veces, así que la confirmación revalida
    /// de cero y no confía en lo que el cliente dice que ya estaba bien.
    ///
    /// La importación es parcial: las filas con error se descartan y las válidas
    /// entran igual, como pide el caso de prueba CP-001.
    /// </summary>
    public class BLLPadron
    {
        /// <summary>
        /// Tope de filas por archivo. Cada invitación manda un correo de forma
        /// síncrona, con un cliente SMTP nuevo y 20 s de timeout: sin tope, un
        /// archivo grande con el correo lento deja la petición colgada.
        /// </summary>
        public const int FilasMaximas = 25;

        private const int LongitudMaximaNombre = 80;
        private const int LongitudMaximaApellido = 80;
        private const int LongitudMaximaEmail = 150;
        private const int LongitudMaximaDocumento = 20;

        BLLUsuario oBLLUsu;
        BLLRol oBLLRol;
        BLLEmpresa oBLLEmp;
        BLLLicencia oBLLLic;
        BLLSeguridad oBLLSeg;
        BLLBitacora oBLLBit;
        MPPUsuario oMPPUsu;
        MPPDepartamento oMPPDep;
        ServicioHash oServicioHash;

        public BLLPadron()
        {
            oBLLUsu = new BLLUsuario();
            oBLLRol = new BLLRol();
            oBLLEmp = new BLLEmpresa();
            oBLLLic = new BLLLicencia();
            oBLLSeg = new BLLSeguridad();
            oBLLBit = new BLLBitacora();
            oMPPUsu = new MPPUsuario();
            oMPPDep = new MPPDepartamento();
            oServicioHash = new ServicioHash();
        }

        public BEResultadoPadron Procesar(BEPadron Objeto, BESesion oSesionBE)
        {
            int empresaId = ResolverEmpresa(Objeto.EmpresaId, oSesionBE);

            ExigirArchivoProcesable(Objeto);

            BEEmpresa oEmpresaBE = ObtenerEmpresaSeleccionable(empresaId);
            BERol oRolBE = ObtenerRolEmpleado();

            // Empresa, rol y organigrama se resuelven una sola vez para todo el
            // archivo: resolverlos por fila reléería los catálogos completos.
            Dictionary<string, int> departamentos = ObtenerDepartamentos(empresaId);

            int cupoDisponible = oBLLLic.ObtenerCupoDisponible(empresaId);

            BEResultadoPadron respuesta = Validar(Objeto, departamentos);

            respuesta.CupoDisponible = cupoDisponible;

            ExigirCupoSuficiente(respuesta.Validas, cupoDisponible);

            if (!Objeto.Confirmar)
            {
                return respuesta;
            }

            Invitar(Objeto, respuesta, oEmpresaBE, oRolBE, departamentos);

            respuesta.Confirmado = true;

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloUsuario,
                "Padron",
                "Padrón procesado para " + oEmpresaBE.RazonSocial + ": " + respuesta.Leidas +
                " filas leídas, " + respuesta.Validas + " invitaciones y " + respuesta.ConError +
                " con error.",
                BLLBitacora.NivelInformativo));

            return respuesta;
        }

        /// <summary>
        /// Valida fila por fila y arma el detalle. No escribe nada: lo usan
        /// tanto la previsualización como la confirmación.
        /// </summary>
        private BEResultadoPadron Validar(BEPadron Objeto, Dictionary<string, int> departamentos)
        {
            BEResultadoPadron respuesta = new BEResultadoPadron();

            // Los correos repetidos DENTRO del archivo también son un error: si
            // no, la primera fila entra y la segunda explota contra la base.
            HashSet<string> emailsDelArchivo = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> documentosDelArchivo = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int indice = 0; indice < Objeto.Filas.Count; indice++)
            {
                BEFilaPadron oFilaBE = Objeto.Filas[indice];

                BEResultadoFilaPadron oResultadoBE = new BEResultadoFilaPadron();

                // +2: la fila 1 es el encabezado y el índice arranca en cero.
                oResultadoBE.Fila = indice + 2;
                oResultadoBE.Nombre = Normalizar(oFilaBE.Nombre);
                oResultadoBE.Apellido = Normalizar(oFilaBE.Apellido);
                oResultadoBE.Email = NormalizarEmail(oFilaBE.Email);
                oResultadoBE.Departamento = Normalizar(oFilaBE.Departamento);

                oResultadoBE.Motivo = Revisar(
                    oResultadoBE,
                    Normalizar(oFilaBE.Documento),
                    departamentos,
                    emailsDelArchivo,
                    documentosDelArchivo);

                oResultadoBE.Valida = oResultadoBE.Motivo == null;

                if (oResultadoBE.Valida)
                {
                    respuesta.Validas++;
                    emailsDelArchivo.Add(oResultadoBE.Email);

                    if (!string.IsNullOrEmpty(Normalizar(oFilaBE.Documento)))
                    {
                        documentosDelArchivo.Add(Normalizar(oFilaBE.Documento));
                    }
                }
                else
                {
                    respuesta.ConError++;
                }

                respuesta.Filas.Add(oResultadoBE);
            }

            respuesta.Leidas = Objeto.Filas.Count;

            return respuesta;
        }

        /// <summary>
        /// Devuelve el motivo del rechazo, o null si la fila está bien. Se
        /// informa un solo motivo por fila: es el primero que aparece, para que
        /// el mensaje sea accionable en vez de una lista.
        /// </summary>
        private string Revisar(
            BEResultadoFilaPadron oFilaBE,
            string documento,
            Dictionary<string, int> departamentos,
            HashSet<string> emailsDelArchivo,
            HashSet<string> documentosDelArchivo)
        {
            if (string.IsNullOrEmpty(oFilaBE.Nombre) || string.IsNullOrEmpty(oFilaBE.Apellido))
            {
                return "El nombre y el apellido son obligatorios.";
            }

            if (string.IsNullOrEmpty(oFilaBE.Email))
            {
                return "El correo es obligatorio.";
            }

            if (oFilaBE.Nombre.Length > LongitudMaximaNombre)
            {
                return "El nombre no puede superar los " + LongitudMaximaNombre + " caracteres.";
            }

            if (oFilaBE.Apellido.Length > LongitudMaximaApellido)
            {
                return "El apellido no puede superar los " + LongitudMaximaApellido + " caracteres.";
            }

            if (oFilaBE.Email.Length > LongitudMaximaEmail || !EsEmailValido(oFilaBE.Email))
            {
                return "El correo no tiene un formato válido.";
            }

            if (documento != null && documento.Length > LongitudMaximaDocumento)
            {
                return "El documento no puede superar los " + LongitudMaximaDocumento + " caracteres.";
            }

            if (documento != null && !EsDocumentoValido(documento))
            {
                return "El documento solo puede tener números, letras y guiones.";
            }

            if (emailsDelArchivo.Contains(oFilaBE.Email))
            {
                return "El correo está repetido dentro del archivo.";
            }

            if (documento != null && documentosDelArchivo.Contains(documento))
            {
                return "El documento está repetido dentro del archivo.";
            }

            if (oBLLUsu.ExisteEmail(oFilaBE.Email))
            {
                return "Ya existe una cuenta registrada con ese correo.";
            }

            if (oBLLUsu.ExisteDocumento(documento))
            {
                return "Ya existe una cuenta registrada con ese documento.";
            }

            if (oFilaBE.Departamento != null && !departamentos.ContainsKey(Clave(oFilaBE.Departamento)))
            {
                return "El departamento " + oFilaBE.Departamento +
                    " no existe en el organigrama de la empresa.";
            }

            return null;
        }

        /// <summary>
        /// Manda las invitaciones de las filas válidas. Si una falla, se anota
        /// en su fila y el resto del lote sigue: un correo roto no tira el
        /// archivo entero.
        /// </summary>
        private void Invitar(
            BEPadron Objeto,
            BEResultadoPadron respuesta,
            BEEmpresa oEmpresaBE,
            BERol oRolBE,
            Dictionary<string, int> departamentos)
        {
            for (int indice = 0; indice < respuesta.Filas.Count; indice++)
            {
                BEResultadoFilaPadron oResultadoBE = respuesta.Filas[indice];

                if (!oResultadoBE.Valida)
                {
                    continue;
                }

                BEUsuario oUsuarioBE = new BEUsuario();

                oUsuarioBE.EmpresaId = oEmpresaBE.EmpresaId;
                oUsuarioBE.Nombre = oResultadoBE.Nombre;
                oUsuarioBE.Apellido = oResultadoBE.Apellido;
                oUsuarioBE.Email = oResultadoBE.Email;
                oUsuarioBE.Documento = Normalizar(Objeto.Filas[indice].Documento);
                oUsuarioBE.DepartamentoId = ResolverDepartamento(oResultadoBE.Departamento, departamentos);

                // La clave es descartable: la verdadera la define la persona
                // desde el enlace de la invitación. Es el mismo criterio que
                // usa UsuarioController al invitar de a uno.
                oUsuarioBE.PasswordHash = oServicioHash.Hashear(Guid.NewGuid().ToString());

                try
                {
                    oBLLUsu.InvitarDelPadron(oUsuarioBE, oRolBE, oEmpresaBE.RazonSocial);
                }
                catch (Exception ex)
                {
                    oResultadoBE.Valida = false;
                    oResultadoBE.Motivo = "No se pudo dar de alta: " + ex.Message;

                    respuesta.Validas--;
                    respuesta.ConError++;

                    ServicioLog.Error(
                        "El padrón no pudo dar de alta a " + oResultadoBE.Email + ".", ex);
                }
            }
        }

        private int? ResolverDepartamento(string nombre, Dictionary<string, int> departamentos)
        {
            if (nombre == null)
            {
                return null;
            }

            int departamentoId;

            return departamentos.TryGetValue(Clave(nombre), out departamentoId)
                ? (int?)departamentoId
                : null;
        }

        /// <summary>
        /// El organigrama de la empresa indexado por nombre. El CSV trae el
        /// departamento como texto, y comparar en memoria evita una consulta
        /// por fila. Case-insensitive, igual que el ABM de Departamentos.
        /// </summary>
        private Dictionary<string, int> ObtenerDepartamentos(int empresaId)
        {
            Dictionary<string, int> departamentos = new Dictionary<string, int>();

            BEDepartamento oFiltroBE = new BEDepartamento();
            oFiltroBE.EmpresaId = empresaId;

            List<BEDepartamento> ListaDepartamentoBE = oMPPDep.ListarTodo(oFiltroBE);

            if (ListaDepartamentoBE == null)
            {
                return departamentos;
            }

            foreach (BEDepartamento oDepartamentoBE in ListaDepartamentoBE)
            {
                departamentos[Clave(oDepartamentoBE.Nombre)] = oDepartamentoBE.DepartamentoId;
            }

            return departamentos;
        }

        private void ExigirArchivoProcesable(BEPadron Objeto)
        {
            if (Objeto.Filas == null || Objeto.Filas.Count == 0)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El archivo no tiene ninguna fila de empleados.");
            }

            if (Objeto.Filas.Count > FilasMaximas)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El archivo tiene " + Objeto.Filas.Count + " filas y el máximo es " +
                    FilasMaximas + ". Dividilo en varios archivos.");
            }
        }

        /// <summary>
        /// CA3 del caso de uso: si no hay cupo para todas las altas, no se
        /// importa nada. Es a todo o nada a propósito: importar la mitad dejaría
        /// al resto del padrón sin poder acceder y sin aviso.
        /// </summary>
        private void ExigirCupoSuficiente(int validas, int cupoDisponible)
        {
            if (validas > cupoDisponible)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto,
                    "El padrón necesita " + validas + " licencias y solo quedan " + cupoDisponible +
                    " disponibles. Debe realizar un Upgrade de su plan para obtener más cupos.");
            }
        }

        private int ResolverEmpresa(int empresaId, BESesion oSesionBE)
        {
            if (!oBLLSeg.Tiene(oSesionBE.UsuarioId, Permisos.UsuarioVerTodasLasEmpresas))
            {
                BEUsuario oFiltroBE = new BEUsuario();
                oFiltroBE.UsuarioId = oSesionBE.UsuarioId;

                BEUsuario oUsuarioBE = oMPPUsu.ListarObjeto(oFiltroBE);

                if (oUsuarioBE == null)
                {
                    throw new ExcepcionNegocio(
                        TipoErrorNegocio.SesionInvalida, "El usuario de la sesión no existe.");
                }

                return oUsuarioBE.EmpresaId;
            }

            if (empresaId == 0)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.Validacion, "Elegí la empresa.");
            }

            return empresaId;
        }

        private BEEmpresa ObtenerEmpresaSeleccionable(int empresaId)
        {
            foreach (BEEmpresa oEmpresaBE in oBLLEmp.ListarSeleccionables())
            {
                if (oEmpresaBE.EmpresaId == empresaId)
                {
                    return oEmpresaBE;
                }
            }

            throw new ExcepcionNegocio(
                TipoErrorNegocio.Validacion,
                "La empresa seleccionada no existe o no está disponible para asignar usuarios.");
        }

        /// <summary>
        /// El padrón da de alta empleados, no administradores: el rol es siempre
        /// el alcanzado por las licencias (Licencias:RolAlcanzado).
        /// </summary>
        private BERol ObtenerRolEmpleado()
        {
            BERol oRolBE = oBLLRol.ObtenerPorNombre(Configuracion.LicenciasRolAlcanzado);

            if (oRolBE == null)
            {
                throw new InvalidOperationException(
                    "El rol '" + Configuracion.LicenciasRolAlcanzado + "' configurado en " +
                    "Licencias:RolAlcanzado no existe entre los roles activos de la tabla Rol.");
            }

            return oRolBE;
        }

        private bool EsEmailValido(string email)
        {
            int arroba = email.IndexOf('@');

            return arroba > 0 &&
                   arroba < email.Length - 1 &&
                   email.IndexOf('@', arroba + 1) < 0 &&
                   email.IndexOf(' ') < 0;
        }

        private bool EsDocumentoValido(string documento)
        {
            foreach (char caracter in documento)
            {
                if (!char.IsLetterOrDigit(caracter) && caracter != '-' && caracter != '.')
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Clave de comparación de nombres de departamento: sin mayúsculas y sin
        /// acentos. Un padrón escrito en Excel trae "recepcion" tanto como
        /// "Recepción", y la base ya compara así por su propia intercalación.
        /// </summary>
        private string Clave(string nombre)
        {
            string descompuesto = nombre.Trim().ToUpperInvariant().Normalize(NormalizationForm.FormD);

            StringBuilder limpio = new StringBuilder(descompuesto.Length);

            foreach (char caracter in descompuesto)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(caracter) != UnicodeCategory.NonSpacingMark)
                {
                    limpio.Append(caracter);
                }
            }

            return limpio.ToString().Normalize(NormalizationForm.FormC);
        }

        private string Normalizar(string valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }

        private string NormalizarEmail(string valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim().ToLowerInvariant();
        }
    }
}
