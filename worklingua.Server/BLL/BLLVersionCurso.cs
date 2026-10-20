using System.Collections.Generic;
using worklingua.Server.BE;
using worklingua.Server.MPP;
using worklingua.Server.Services;

namespace worklingua.Server.BLL
{
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

        public BEResultadoRestauracion Clonar(int cursoId, string nombre, BESesion oSesionBE)
        {
            BECurso oOrigenBE = oBLLCur.ExigirGestion(Filtro(cursoId), oSesionBE);

            BESnapshotCurso oSnapshotBE = Armar(oOrigenBE);

            oSnapshotBE.Curso.Nombre = Normalizar(nombre);
            oSnapshotBE.Curso.FechaPublicacion = null;
            oSnapshotBE.Curso.FechaFin = null;

            BECurso oCopiaBE = new BECurso();

            oCopiaBE.CursoId = 0;
            CopiarDatos(oSnapshotBE.Curso, oCopiaBE);
            oCopiaBE.Etiquetas = ResolverEtiquetas(oSnapshotBE.Curso.Etiquetas);

            oCopiaBE = oBLLCur.Guardar(oCopiaBE, oSesionBE);

            return Aplicar(oCopiaBE.CursoId, oSnapshotBE, oSesionBE, false);
        }

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

        private BESnapshotCurso Armar(BECurso oCursoBE)
        {
            BESnapshotCurso oSnapshotBE = new BESnapshotCurso();

            oSnapshotBE.Curso = oCursoBE;
            oSnapshotBE.Modulos = oMPPMod.ListarPorCurso(Filtro(oCursoBE.CursoId)) ?? new List<BEModulo>();

            BEActivoPedagogico oFiltroActivoBE = new BEActivoPedagogico();
            oFiltroActivoBE.CursoId = oCursoBE.CursoId;

            foreach (BEActivoPedagogico oActivoBE in oMPPAct.ListarPorCurso(oFiltroActivoBE))
            {
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

        private void AplicarActivos(
            int cursoId, BESnapshotCurso oSnapshotBE, BEResultadoRestauracion respuesta)
        {
            BEActivoPedagogico oFiltroBE = new BEActivoPedagogico();
            oFiltroBE.CursoId = cursoId;

            List<BEActivoPedagogico> ListaActualBE = oMPPAct.ListarPorCurso(oFiltroBE);

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
                oNuevoBE.ModuloId = oModuloBE == null ? (int?)null : oModuloBE.ModuloId;
                oNuevoBE.Nombre = oDelSnapshotBE.Nombre;
                oNuevoBE.TipoContenido = oDelSnapshotBE.TipoContenido;
                oNuevoBE.Descripcion = oDelSnapshotBE.Descripcion;
                oNuevoBE.UrlArchivo = oDelSnapshotBE.UrlArchivo;
                oNuevoBE.Estado = oDelSnapshotBE.Estado;

                oMPPAct.Alta(oNuevoBE);
            }
        }

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
