using System.Collections.Generic;
using worklingua.Server.BE;
using worklingua.Server.MPP;
using worklingua.Server.Services;

namespace worklingua.Server.BLL
{
    /// <summary>
    /// Trazabilidad de versiones de un curso (CU-004-004) y clonado de cursos
    /// (CU-004-002).
    ///
    /// Un punto de restauración es el curso entero —datos, clasificación,
    /// ventana, etiquetas, módulos y activos— serializado a XML. El documento
    /// es la versión: restaurar es leerlo y volver a aplicarlo.
    ///
    /// Los dos casos de uso comparten el mismo armado del snapshot, y por eso
    /// viven juntos: clonar es tomar el estado actual y escribirlo en un curso
    /// nuevo; restaurar es tomar un estado viejo y escribirlo sobre el mismo.
    ///
    /// El alcance lo decide el curso: se reusa BLLCurso, así un curso de otra
    /// empresa responde igual que en el ABM.
    /// </summary>
    public class BLLVersionCurso
    {
        private const int LongitudMaximaObservaciones = 500;

        MPPVersionContenido oMPPVer;
        MPPModulo oMPPMod;
        MPPActivoPedagogico oMPPAct;
        MPPCurso oMPPCur;
        BLLCurso oBLLCur;
        BLLEtiqueta oBLLEti;

        public BLLVersionCurso()
        {
            oMPPVer = new MPPVersionContenido();
            oMPPMod = new MPPModulo();
            oMPPAct = new MPPActivoPedagogico();
            oMPPCur = new MPPCurso();
            oBLLCur = new BLLCurso();
            oBLLEti = new BLLEtiqueta();
        }

        public List<BEVersionContenido> ListarPorCurso(int cursoId, BESesion oSesionBE)
        {
            ExigirCursoVisible(cursoId, oSesionBE);

            return oMPPVer.ListarPorCurso(cursoId);
        }

        /// <summary>
        /// Camino alternativo 2: el punto de restauración manual. Congela el
        /// estado actual del curso y lo guarda como XML.
        /// </summary>
        public BEVersionContenido Crear(int cursoId, string observaciones, BESesion oSesionBE)
        {
            BECurso oCursoBE = oBLLCur.ExigirGestion(Filtro(cursoId), oSesionBE);

            return Guardar(oCursoBE, Normalizar(observaciones), oSesionBE);
        }

        public BEVersionContenido Obtener(int versionContenidoId, BESesion oSesionBE)
        {
            BEVersionContenido oVersionBE = oMPPVer.ListarObjeto(versionContenidoId);

            if (oVersionBE == null)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.NoEncontrado, "La versión no existe.");
            }

            ExigirCursoVisible(oVersionBE.CursoId, oSesionBE);

            return oVersionBE;
        }

        /// <summary>
        /// El escenario principal: vuelve el curso al estado de la versión.
        ///
        /// Antes de aplicar nada guarda el estado actual como una versión
        /// automática: restaurar no puede ser el movimiento que pierde lo que
        /// había.
        /// </summary>
        public BEResultadoRestauracion Restaurar(int versionContenidoId, BESesion oSesionBE)
        {
            BEVersionContenido oVersionBE = oMPPVer.ListarObjeto(versionContenidoId);

            if (oVersionBE == null)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.NoEncontrado, "La versión no existe.");
            }

            BECurso oCursoBE = oBLLCur.ExigirGestion(Filtro(oVersionBE.CursoId), oSesionBE);

            BESnapshotCurso oSnapshotBE = LeerObligatorio(oVersionBE);

            Guardar(oCursoBE, "Estado previo a restaurar la versión " + oVersionBE.NumeroVersion + ".", oSesionBE);

            return Aplicar(oCursoBE.CursoId, oSnapshotBE, oSesionBE, true);
        }

        /// <summary>
        /// Elimina un punto de restauración que ya no hace falta.
        ///
        /// Exige poder gestionar el curso, igual que crear o restaurar: una
        /// versión de un curso ajeno responde que no existe.
        /// </summary>
        public BEVersionContenido Eliminar(int versionContenidoId, BESesion oSesionBE)
        {
            BEVersionContenido oVersionBE = oMPPVer.ListarObjeto(versionContenidoId);

            if (oVersionBE == null)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.NoEncontrado, "La versión no existe.");
            }

            oBLLCur.ExigirGestion(Filtro(oVersionBE.CursoId), oSesionBE);

            oMPPVer.Baja(versionContenidoId);

            return oVersionBE;
        }

        /// <summary>
        /// CU-004-002: duplica el curso con sus módulos y activos. Los activos
        /// apuntan al mismo archivo —el caso de uso pide no alterar los
        /// recursos originales— y la copia nace sin programar.
        /// </summary>
        public BEResultadoRestauracion Clonar(int cursoId, string nombre, BESesion oSesionBE)
        {
            BECurso oOrigenBE = oBLLCur.ExigirGestion(Filtro(cursoId), oSesionBE);

            BESnapshotCurso oSnapshotBE = Armar(oOrigenBE);

            oSnapshotBE.Curso.Nombre = Normalizar(nombre);
            // La copia no hereda la programación: se publica cuando esté lista.
            oSnapshotBE.Curso.FechaPublicacion = null;
            oSnapshotBE.Curso.FechaFin = null;

            BECurso oCopiaBE = new BECurso();

            oCopiaBE.CursoId = 0;
            CopiarDatos(oSnapshotBE.Curso, oCopiaBE);
            oCopiaBE.Etiquetas = ResolverEtiquetas(oSnapshotBE.Curso.Etiquetas);

            // Guardar valida el nombre disponible (camino alternativo 1) y el
            // alcance, y asigna la empresa como en cualquier alta.
            oCopiaBE = oBLLCur.Guardar(oCopiaBE, oSesionBE);

            return Aplicar(oCopiaBE.CursoId, oSnapshotBE, oSesionBE, false);
        }

        /// <summary>
        /// Camino alternativo 3: qué cambió entre dos versiones. Devuelve una
        /// fila por campo distinto; las dos versiones tienen que ser del mismo
        /// curso.
        /// </summary>
        public List<BEDiferenciaVersion> Comparar(
            int versionIzquierdaId, int versionDerechaId, BESesion oSesionBE)
        {
            BEVersionContenido oIzquierdaBE = Obtener(versionIzquierdaId, oSesionBE);
            BEVersionContenido oDerechaBE = Obtener(versionDerechaId, oSesionBE);

            if (oIzquierdaBE.CursoId != oDerechaBE.CursoId)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "Las dos versiones tienen que ser del mismo curso.");
            }

            BESnapshotCurso oUnoBE = LeerObligatorio(oIzquierdaBE);
            BESnapshotCurso oOtroBE = LeerObligatorio(oDerechaBE);

            List<BEDiferenciaVersion> ListaBE = new List<BEDiferenciaVersion>();

            Comparar(ListaBE, "nombre", oUnoBE.Curso.Nombre, oOtroBE.Curso.Nombre);
            Comparar(ListaBE, "descripcion", oUnoBE.Curso.Descripcion, oOtroBE.Curso.Descripcion);
            Comparar(ListaBE, "idioma", oUnoBE.Curso.Idioma, oOtroBE.Curso.Idioma);
            Comparar(ListaBE, "nivel", oUnoBE.Curso.Nivel, oOtroBE.Curso.Nivel);
            Comparar(ListaBE, "sector", oUnoBE.Curso.Sector, oOtroBE.Curso.Sector);
            Comparar(ListaBE, "duracionHoras",
                Texto(oUnoBE.Curso.DuracionHoras), Texto(oOtroBE.Curso.DuracionHoras));
            Comparar(ListaBE, "fechaPublicacion",
                Texto(oUnoBE.Curso.FechaPublicacion), Texto(oOtroBE.Curso.FechaPublicacion));
            Comparar(ListaBE, "fechaFin", Texto(oUnoBE.Curso.FechaFin), Texto(oOtroBE.Curso.FechaFin));
            Comparar(ListaBE, "etiquetas",
                NombresEtiqueta(oUnoBE.Curso.Etiquetas), NombresEtiqueta(oOtroBE.Curso.Etiquetas));
            Comparar(ListaBE, "modulos", NombresModulo(oUnoBE.Modulos), NombresModulo(oOtroBE.Modulos));
            Comparar(ListaBE, "activos", NombresActivo(oUnoBE.Activos), NombresActivo(oOtroBE.Activos));

            return ListaBE;
        }

        /// <summary>Arma el snapshot del curso tal como está hoy.</summary>
        private BESnapshotCurso Armar(BECurso oCursoBE)
        {
            BESnapshotCurso oSnapshotBE = new BESnapshotCurso();

            oSnapshotBE.Curso = oCursoBE;
            oSnapshotBE.Modulos = oMPPMod.ListarPorCurso(Filtro(oCursoBE.CursoId)) ?? new List<BEModulo>();

            BEActivoPedagogico oFiltroActivoBE = new BEActivoPedagogico();
            oFiltroActivoBE.CursoId = oCursoBE.CursoId;

            foreach (BEActivoPedagogico oActivoBE in oMPPAct.ListarPorCurso(oFiltroActivoBE))
            {
                // Un activo dado de baja no forma parte del estado del curso.
                if (oActivoBE.Activo == true)
                {
                    oSnapshotBE.Activos.Add(oActivoBE);
                }
            }

            return oSnapshotBE;
        }

        private BEVersionContenido Guardar(BECurso oCursoBE, string observaciones, BESesion oSesionBE)
        {
            BEVersionContenido oVersionBE = new BEVersionContenido();

            oVersionBE.CursoId = oCursoBE.CursoId;
            oVersionBE.ContenidoXml = ServicioXml.Armar(Armar(oCursoBE));
            oVersionBE.Observaciones = observaciones;
            oVersionBE.UsuarioId = oSesionBE.UsuarioId;

            oVersionBE.VersionContenidoId = oMPPVer.Alta(oVersionBE);

            return oMPPVer.ListarObjeto(oVersionBE.VersionContenidoId);
        }

        /// <summary>
        /// Escribe el snapshot sobre un curso: datos, etiquetas, módulos y
        /// activos. Lo usan restaurar y clonar.
        ///
        /// Módulos y activos se reconcilian por nombre: se da de baja lo que no
        /// está en el snapshot y se agrega lo que falta. No se borra nada,
        /// porque los módulos tienen progreso de empleados colgando.
        /// </summary>
        /// <param name="aplicarDatos">
        /// Falso al clonar: el curso recién creado ya nació con los datos del
        /// snapshot, y volver a guardarlos chocaría con la validación de nombre
        /// único contra sí mismo.
        /// </param>
        private BEResultadoRestauracion Aplicar(
            int cursoId, BESnapshotCurso oSnapshotBE, BESesion oSesionBE, bool aplicarDatos)
        {
            BEResultadoRestauracion respuesta = new BEResultadoRestauracion();
            respuesta.CursoId = cursoId;

            if (aplicarDatos)
            {
                BECurso oDestinoBE = new BECurso();

                oDestinoBE.CursoId = cursoId;
                CopiarDatos(oSnapshotBE.Curso, oDestinoBE);
                oDestinoBE.Etiquetas = ResolverEtiquetas(oSnapshotBE.Curso.Etiquetas);

                oBLLCur.Guardar(oDestinoBE, oSesionBE);
            }

            AplicarModulos(cursoId, oSnapshotBE);
            AplicarActivos(cursoId, oSnapshotBE, respuesta);

            return respuesta;
        }

        private void AplicarModulos(int cursoId, BESnapshotCurso oSnapshotBE)
        {
            List<BEModulo> ListaActualBE = oMPPMod.ListarPorCurso(Filtro(cursoId)) ?? new List<BEModulo>();

            foreach (BEModulo oActualBE in ListaActualBE)
            {
                if (oActualBE.Activo == true && Buscar(oSnapshotBE.Modulos, oActualBE.Nombre) == null)
                {
                    oMPPMod.Baja(oActualBE);
                }
            }

            foreach (BEModulo oDelSnapshotBE in oSnapshotBE.Modulos)
            {
                BEModulo oExistenteBE = Buscar(ListaActualBE, oDelSnapshotBE.Nombre);

                if (oExistenteBE != null && oExistenteBE.Activo == true)
                {
                    // El módulo sobrevive —tiene progreso colgando— pero su
                    // objetivo, su contenido y su orden sí vuelven a los de la
                    // versión: si no, restaurar dejaría la lección a medias.
                    ActualizarSiCambio(oExistenteBE, oDelSnapshotBE);

                    continue;
                }

                BEModulo oNuevoBE = new BEModulo();

                oNuevoBE.CursoId = cursoId;
                oNuevoBE.Nombre = oDelSnapshotBE.Nombre;
                oNuevoBE.Descripcion = oDelSnapshotBE.Descripcion;
                oNuevoBE.Contenido = oDelSnapshotBE.Contenido;
                oNuevoBE.OrdenModulo = oDelSnapshotBE.OrdenModulo;
                oNuevoBE.Activo = true;

                oMPPMod.Guardar(oNuevoBE);
            }
        }

        private void ActualizarSiCambio(BEModulo oActualBE, BEModulo oDelSnapshotBE)
        {
            bool igual = Texto(oActualBE.Descripcion) == Texto(oDelSnapshotBE.Descripcion)
                && Texto(oActualBE.Contenido) == Texto(oDelSnapshotBE.Contenido)
                && oActualBE.OrdenModulo == oDelSnapshotBE.OrdenModulo;

            if (igual)
            {
                return;
            }

            oActualBE.Descripcion = oDelSnapshotBE.Descripcion;
            oActualBE.Contenido = oDelSnapshotBE.Contenido;
            oActualBE.OrdenModulo = oDelSnapshotBE.OrdenModulo;

            oMPPMod.ActualizarTextos(oActualBE);
        }

        /// <summary>
        /// Camino alternativo 2 de CU-004-002 y 1 de CU-004-004: el activo cuyo
        /// archivo ya no está en disco se omite en vez de heredar un enlace
        /// roto, y se informa cuántos quedaron afuera.
        /// </summary>
        private void AplicarActivos(
            int cursoId, BESnapshotCurso oSnapshotBE, BEResultadoRestauracion respuesta)
        {
            BEActivoPedagogico oFiltroBE = new BEActivoPedagogico();
            oFiltroBE.CursoId = cursoId;

            List<BEActivoPedagogico> ListaActualBE = oMPPAct.ListarPorCurso(oFiltroBE);

            // Los módulos del destino ya están aplicados, así que acá se puede
            // volver a atar cada activo al suyo por nombre.
            List<BEModulo> ListaModuloBE = oMPPMod.ListarPorCurso(Filtro(cursoId)) ?? new List<BEModulo>();

            foreach (BEActivoPedagogico oActualBE in ListaActualBE)
            {
                if (oActualBE.Activo == true && BuscarActivo(oSnapshotBE.Activos, oActualBE.Nombre) == null)
                {
                    oActualBE.Activo = false;
                    oMPPAct.CambiarActivo(oActualBE);
                }
            }

            foreach (BEActivoPedagogico oDelSnapshotBE in oSnapshotBE.Activos)
            {
                BEActivoPedagogico oExistenteBE = BuscarActivo(ListaActualBE, oDelSnapshotBE.Nombre);

                if (!ServicioArchivos.Existe(oDelSnapshotBE.UrlArchivo))
                {
                    respuesta.Omitidos++;
                    respuesta.NombresOmitidos.Add(oDelSnapshotBE.Nombre);

                    // Si ya estaba vigente, tampoco puede quedarse: el punto de
                    // omitirlo es no dejar el curso con un enlace roto.
                    if (oExistenteBE != null && oExistenteBE.Activo == true)
                    {
                        oExistenteBE.Activo = false;
                        oMPPAct.CambiarActivo(oExistenteBE);
                    }

                    continue;
                }

                if (oExistenteBE != null && oExistenteBE.Activo == true)
                {
                    continue;
                }

                BEModulo oModuloBE = oDelSnapshotBE.Modulo == null
                    ? null
                    : Buscar(ListaModuloBE, oDelSnapshotBE.Modulo);

                BEActivoPedagogico oNuevoBE = new BEActivoPedagogico();

                oNuevoBE.CursoId = cursoId;
                // Si el módulo ya no está, el activo vuelve a ser material
                // general del curso en lugar de perderse.
                oNuevoBE.ModuloId = oModuloBE == null ? (int?)null : oModuloBE.ModuloId;
                oNuevoBE.Nombre = oDelSnapshotBE.Nombre;
                oNuevoBE.TipoContenido = oDelSnapshotBE.TipoContenido;
                oNuevoBE.Descripcion = oDelSnapshotBE.Descripcion;
                oNuevoBE.UrlArchivo = oDelSnapshotBE.UrlArchivo;
                // El estado viaja con el activo: una copia o una restauración que
                // deja todo en borrador es material que el alumno no ve, y desde
                // la pantalla parece que no se copió nada.
                oNuevoBE.Estado = oDelSnapshotBE.Estado;

                oMPPAct.Alta(oNuevoBE);
            }
        }

        /// <summary>
        /// Resuelve las etiquetas del snapshot contra el diccionario de hoy.
        /// Una que ya no existe se descarta: el snapshot guarda nombres, no
        /// ids, justamente para no depender de filas que pudieron cambiar.
        /// </summary>
        private List<BEEtiqueta> ResolverEtiquetas(List<BEEtiqueta> ListaBE)
        {
            List<BEEtiqueta> ListaResueltaBE = new List<BEEtiqueta>();
            List<BEEtiqueta> ListaDiccionarioBE = oBLLEti.Listar();

            foreach (BEEtiqueta oEtiquetaBE in ListaBE)
            {
                foreach (BEEtiqueta oDelDiccionarioBE in ListaDiccionarioBE)
                {
                    bool coincide = oEtiquetaBE.EtiquetaId != 0
                        ? oDelDiccionarioBE.EtiquetaId == oEtiquetaBE.EtiquetaId
                        : string.Equals(oDelDiccionarioBE.Nombre, oEtiquetaBE.Nombre,
                            StringComparison.OrdinalIgnoreCase);

                    if (coincide)
                    {
                        ListaResueltaBE.Add(oDelDiccionarioBE);

                        break;
                    }
                }
            }

            return ListaResueltaBE;
        }

        private BESnapshotCurso LeerObligatorio(BEVersionContenido oVersionBE)
        {
            BESnapshotCurso oSnapshotBE = ServicioXml.Leer(oVersionBE.ContenidoXml ?? string.Empty);

            if (oSnapshotBE == null)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto,
                    "El contenido de la versión está dañado y no se puede leer.");
            }

            return oSnapshotBE;
        }

        private void CopiarDatos(BECurso oOrigenBE, BECurso oDestinoBE)
        {
            oDestinoBE.Nombre = oOrigenBE.Nombre;
            oDestinoBE.Descripcion = oOrigenBE.Descripcion;
            oDestinoBE.Idioma = oOrigenBE.Idioma;
            oDestinoBE.Nivel = oOrigenBE.Nivel;
            oDestinoBE.DuracionHoras = oOrigenBE.DuracionHoras;
            oDestinoBE.Sector = oOrigenBE.Sector;
            oDestinoBE.FechaPublicacion = oOrigenBE.FechaPublicacion;
            oDestinoBE.FechaFin = oOrigenBE.FechaFin;
        }

        private void Comparar(
            List<BEDiferenciaVersion> ListaBE, string campo, string izquierda, string derecha)
        {
            string uno = izquierda ?? string.Empty;
            string otro = derecha ?? string.Empty;

            if (uno != otro)
            {
                ListaBE.Add(new BEDiferenciaVersion(campo, uno, otro));
            }
        }

        private BEModulo Buscar(List<BEModulo> ListaBE, string nombre)
        {
            foreach (BEModulo oModuloBE in ListaBE)
            {
                if (string.Equals(oModuloBE.Nombre, nombre, StringComparison.OrdinalIgnoreCase))
                {
                    return oModuloBE;
                }
            }

            return null;
        }

        private BEActivoPedagogico BuscarActivo(List<BEActivoPedagogico> ListaBE, string nombre)
        {
            foreach (BEActivoPedagogico oActivoBE in ListaBE)
            {
                if (string.Equals(oActivoBE.Nombre, nombre, StringComparison.OrdinalIgnoreCase))
                {
                    return oActivoBE;
                }
            }

            return null;
        }

        private string NombresEtiqueta(List<BEEtiqueta> ListaBE)
        {
            List<string> nombres = new List<string>();

            foreach (BEEtiqueta oEtiquetaBE in ListaBE)
            {
                nombres.Add(oEtiquetaBE.Nombre);
            }

            nombres.Sort(StringComparer.OrdinalIgnoreCase);

            return string.Join(", ", nombres);
        }

        private string NombresModulo(List<BEModulo> ListaBE)
        {
            List<string> nombres = new List<string>();

            foreach (BEModulo oModuloBE in ListaBE)
            {
                nombres.Add(oModuloBE.OrdenModulo + ". " + oModuloBE.Nombre);
            }

            nombres.Sort(StringComparer.OrdinalIgnoreCase);

            return string.Join(", ", nombres);
        }

        private string NombresActivo(List<BEActivoPedagogico> ListaBE)
        {
            List<string> nombres = new List<string>();

            foreach (BEActivoPedagogico oActivoBE in ListaBE)
            {
                nombres.Add(oActivoBE.Nombre);
            }

            nombres.Sort(StringComparer.OrdinalIgnoreCase);

            return string.Join(", ", nombres);
        }

        private void ExigirCursoVisible(int cursoId, BESesion oSesionBE)
        {
            oBLLCur.ListarObjeto(Filtro(cursoId), oSesionBE);
        }

        private BECurso Filtro(int cursoId)
        {
            BECurso oFiltroBE = new BECurso();
            oFiltroBE.CursoId = cursoId;

            return oFiltroBE;
        }

        private string Texto(string valor)
        {
            return valor ?? string.Empty;
        }

        private string Texto(int? valor)
        {
            return valor.HasValue ? valor.Value.ToString() : string.Empty;
        }

        private string Texto(DateOnly? valor)
        {
            return valor.HasValue ? valor.Value.ToString("yyyy-MM-dd") : string.Empty;
        }

        private string Normalizar(string valor)
        {
            if (valor == null)
            {
                return null;
            }

            string limpio = valor.Trim();

            if (limpio.Length > LongitudMaximaObservaciones)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "Las observaciones no pueden superar los " + LongitudMaximaObservaciones + " caracteres.");
            }

            return limpio.Length == 0 ? null : limpio;
        }
    }
}
