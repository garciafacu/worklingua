using System.Collections;
using System.Data;
using worklingua.Server.BE;
using worklingua.Server.DAL;
using worklingua.Server.Services;

namespace worklingua.Server.MPP
{
    public class MPPCurso
    {
        Acceso oDatos = new Acceso();
        Hashtable Hdatos;

        public List<BECurso> Buscar(BEFiltroCurso Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@Nombre", Objeto.Nombre);
            Hdatos.Add("@Idioma", Objeto.Idioma);
            Hdatos.Add("@Nivel", Objeto.Nivel);
            Hdatos.Add("@EmpresaId", Objeto.EmpresaId);
            Hdatos.Add("@Sector", Objeto.Sector);
            Hdatos.Add("@IncluirFueraDeVentana", Objeto.IncluirFueraDeVentana);

            if (Objeto.EtiquetaId.HasValue && Objeto.EtiquetaId.Value != 0)
            {
                Hdatos.Add("@EtiquetaId", Objeto.EtiquetaId.Value);
            }

            return ServicioLectorFila.ALista<BECurso>(oDatos.Leer("sp_Curso_Buscar", Hdatos), Mapear);
        }

        public List<BECurso> ListarTodoConBajas(BEFiltroCurso Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@EmpresaId", Objeto.EmpresaId);

            return ServicioLectorFila.ALista<BECurso>(oDatos.Leer("sp_Curso_ListarTodos", Hdatos), Mapear);
        }

        public List<string> ListarIdiomas(BEFiltroCurso Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@EmpresaId", Objeto.EmpresaId);

            List<string> ListaIdioma = new List<string>();
            DataTable Dt = oDatos.Leer("sp_Curso_ListarIdiomas", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                foreach (DataRow Item in Dt.Rows)
                {
                    ListaIdioma.Add(ServicioLectorFila.LeerTexto(Item, "Idioma"));
                }

                return ListaIdioma;
            }
            else
            {
                return null;
            }
        }

        public BECurso ListarObjeto(BECurso Objeto, BEFiltroCurso oAlcance)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@CursoId", Objeto.CursoId);
            Hdatos.Add("@EmpresaId", oAlcance.EmpresaId);

            return ServicioLectorFila.AObjeto(oDatos.Leer("sp_Curso_ObtenerPorId", Hdatos), Mapear);
        }

        public int Guardar(BECurso Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@Idioma", Objeto.Idioma);
            Hdatos.Add("@Nombre", Objeto.Nombre);
            Hdatos.Add("@Descripcion", Objeto.Descripcion);
            Hdatos.Add("@Nivel", Objeto.Nivel);
            Hdatos.Add("@DuracionHoras", Objeto.DuracionHoras);
            AgregarClasificacion(Objeto);

            if (Objeto.CursoId != 0)
            {
                Hdatos.Add("@CursoId", Objeto.CursoId);

                oDatos.LeerEscalar("sp_Curso_Modificar", Hdatos);

                return Objeto.CursoId;
            }

            Hdatos.Add("@EmpresaId", Objeto.EmpresaId);

            object identidad = oDatos.LeerEscalar("sp_Curso_Alta", Hdatos);

            return Convert.ToInt32(identidad);
        }

        private void AgregarClasificacion(BECurso Objeto)
        {
            if (!string.IsNullOrEmpty(Objeto.Sector))
            {
                Hdatos.Add("@Sector", Objeto.Sector);
            }

            if (Objeto.FechaPublicacion.HasValue)
            {
                Hdatos.Add("@FechaPublicacion", Objeto.FechaPublicacion.Value.ToDateTime(TimeOnly.MinValue));
            }

            if (Objeto.FechaFin.HasValue)
            {
                Hdatos.Add("@FechaFin", Objeto.FechaFin.Value.ToDateTime(TimeOnly.MinValue));
            }
        }

        public bool Baja(BECurso Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@CursoId", Objeto.CursoId);

            object filas = oDatos.LeerEscalar("sp_Curso_Baja", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        private BECurso Mapear(DataRow Item)
        {
            BECurso oCursoBE = new BECurso();

            oCursoBE.CursoId = ServicioLectorFila.LeerEntero(Item, "CursoId");
            oCursoBE.Idioma = ServicioLectorFila.LeerTexto(Item, "Idioma");
            oCursoBE.Nombre = ServicioLectorFila.LeerTexto(Item, "Nombre");
            oCursoBE.Descripcion = ServicioLectorFila.LeerTextoNulo(Item, "Descripcion");
            oCursoBE.Nivel = ServicioLectorFila.LeerTexto(Item, "Nivel");
            oCursoBE.DuracionHoras = ServicioLectorFila.LeerEnteroNulo(Item, "DuracionHoras");
            oCursoBE.Activo = ServicioLectorFila.LeerBooleano(Item, "Activo");
            oCursoBE.FechaAlta = ServicioLectorFila.LeerFechaHora(Item, "FechaAlta");
            oCursoBE.EmpresaId = ServicioLectorFila.LeerEnteroNulo(Item, "EmpresaId");
            oCursoBE.Sector = ServicioLectorFila.LeerTextoNulo(Item, "Sector");
            oCursoBE.FechaPublicacion = ServicioLectorFila.LeerFechaNula(Item, "FechaPublicacion");
            oCursoBE.FechaFin = ServicioLectorFila.LeerFechaNula(Item, "FechaFin");

            return oCursoBE;
        }
    }

    public class MPPModulo
    {
        Acceso oDatos = new Acceso();
        Hashtable Hdatos;

        public List<BEModulo> ListarPorCurso(BECurso Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@CursoId", Objeto.CursoId);

            return ServicioLectorFila.ALista(oDatos.Leer("sp_Modulo_Listar", Hdatos), Mapear);
        }

        private BEModulo Mapear(DataRow Item)
        {
            return new BEModulo(
                ServicioLectorFila.LeerEntero(Item, "ModuloId"),
                ServicioLectorFila.LeerEntero(Item, "CursoId"),
                ServicioLectorFila.LeerTexto(Item, "Nombre"),
                ServicioLectorFila.LeerTextoNulo(Item, "Descripcion"),
                ServicioLectorFila.LeerTextoNulo(Item, "Contenido"),
                ServicioLectorFila.LeerEntero(Item, "OrdenModulo"),
                ServicioLectorFila.LeerBooleanoNulo(Item, "Activo"));
        }

        public int Guardar(BEModulo Objeto)
        {
            if (Objeto.ModuloId != 0)
            {
                throw new NotImplementedException(
                    "Un módulo se da de baja y se vuelve a cargar; para restaurar una versión usar ActualizarTextos.");
            }

            Hdatos = new Hashtable();
            Hdatos.Add("@CursoId", Objeto.CursoId);
            Hdatos.Add("@Nombre", Objeto.Nombre);
            Hdatos.Add("@Descripcion", Objeto.Descripcion);
            Hdatos.Add("@Contenido", Objeto.Contenido);
            Hdatos.Add("@OrdenModulo", Objeto.OrdenModulo);

            object identidad = oDatos.LeerEscalar("sp_Modulo_Alta", Hdatos);

            return Convert.ToInt32(identidad);
        }

        public bool ActualizarTextos(BEModulo Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@ModuloId", Objeto.ModuloId);
            Hdatos.Add("@Descripcion", Objeto.Descripcion);
            Hdatos.Add("@Contenido", Objeto.Contenido);
            Hdatos.Add("@OrdenModulo", Objeto.OrdenModulo);

            object filas = oDatos.LeerEscalar("sp_Modulo_Modificar", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        public bool Baja(BEModulo Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@ModuloId", Objeto.ModuloId);

            object filas = oDatos.LeerEscalar("sp_Modulo_Baja", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }
    }

    public class MPPEtiqueta
    {
        Acceso oDatos = new Acceso();
        Hashtable Hdatos;

        public List<BEEtiqueta> Listar()
        {
            List<BEEtiqueta> ListaBE = new List<BEEtiqueta>();

            Hdatos = new Hashtable();

            DataTable Dt = oDatos.Leer("sp_Etiqueta_Listar", Hdatos);

            foreach (DataRow Item in Dt.Rows)
            {
                BEEtiqueta oEtiquetaBE = new BEEtiqueta();

                oEtiquetaBE.EtiquetaId = ServicioLectorFila.LeerEntero(Item, "EtiquetaId");
                oEtiquetaBE.Nombre = ServicioLectorFila.LeerTexto(Item, "Nombre");
                oEtiquetaBE.Activo = ServicioLectorFila.LeerBooleanoNulo(Item, "Activo");
                oEtiquetaBE.FechaAlta = ServicioLectorFila.LeerFechaHoraNula(Item, "FechaAlta");

                ListaBE.Add(oEtiquetaBE);
            }

            return ListaBE;
        }

        public int Alta(BEEtiqueta Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@Nombre", Objeto.Nombre);

            object identidad = oDatos.LeerEscalar("sp_Etiqueta_Alta", Hdatos);

            return Convert.ToInt32(identidad);
        }

        public List<BEEtiquetaCurso> ListarAsignadas(int cursoId)
        {
            List<BEEtiquetaCurso> ListaBE = new List<BEEtiquetaCurso>();

            Hdatos = new Hashtable();

            if (cursoId != 0)
            {
                Hdatos.Add("@CursoId", cursoId);
            }

            DataTable Dt = oDatos.Leer("sp_CursoEtiqueta_Listar", Hdatos);

            foreach (DataRow Item in Dt.Rows)
            {
                BEEtiquetaCurso oAsignadaBE = new BEEtiquetaCurso();

                oAsignadaBE.CursoId = ServicioLectorFila.LeerEntero(Item, "CursoId");
                oAsignadaBE.EtiquetaId = ServicioLectorFila.LeerEntero(Item, "EtiquetaId");
                oAsignadaBE.Nombre = ServicioLectorFila.LeerTexto(Item, "Nombre");

                ListaBE.Add(oAsignadaBE);
            }

            return ListaBE;
        }

        public void Reemplazar(int cursoId, List<int> etiquetaIds)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@CursoId", cursoId);
            Hdatos.Add("@EtiquetaIds", string.Join(",", etiquetaIds));

            oDatos.LeerEscalar("sp_CursoEtiqueta_Reemplazar", Hdatos);
        }
    }

    public class MPPProgreso
    {
        Acceso oDatos = new Acceso();
        Hashtable Hdatos;

        public List<BECursoConProgreso> ListarPorUsuario(BEUsuario Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@UsuarioId", Objeto.UsuarioId);

            DataTable Dt = oDatos.Leer("sp_Progreso_ListarPorUsuario", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                List<BECursoConProgreso> ListaCursoBE = new List<BECursoConProgreso>();

                foreach (DataRow Item in Dt.Rows)
                {
                    AgregarFila(ListaCursoBE, Item);
                }

                return ListaCursoBE;
            }
            else
            {
                return null;
            }
        }

        public List<BEProgresoUsuario> ListarPorEmpresa(BEEmpresa Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@EmpresaId", Objeto.EmpresaId);

            DataTable Dt = oDatos.Leer("sp_Progreso_ListarPorEmpresa", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                List<BEProgresoUsuario> ListaUsuarioBE = new List<BEProgresoUsuario>();
                BEProgresoUsuario oActual = null;

                foreach (DataRow Item in Dt.Rows)
                {
                    int usuarioId = ServicioLectorFila.LeerEntero(Item, "UsuarioId");

                    if (oActual == null || oActual.UsuarioId != usuarioId)
                    {
                        oActual = new BEProgresoUsuario(
                            usuarioId,
                            ServicioLectorFila.LeerTexto(Item, "Usuario"),
                            ServicioLectorFila.LeerTexto(Item, "Email"),
                            new List<BECursoConProgreso>());

                        ListaUsuarioBE.Add(oActual);
                    }

                    AgregarFila(oActual.Cursos, Item);
                }

                return ListaUsuarioBE;
            }
            else
            {
                return null;
            }
        }

        public int Guardar(BEProgreso Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@UsuarioId", Objeto.UsuarioId);
            Hdatos.Add("@ModuloId", Objeto.ModuloId);
            Hdatos.Add("@PorcentajeAvance", Objeto.PorcentajeAvance);
            Hdatos.Add("@Estado", Objeto.Estado);
            Hdatos.Add("@FechaInicio", Objeto.FechaInicio);
            Hdatos.Add("@FechaFinalizacion", Objeto.FechaFinalizacion);

            object identidad = oDatos.LeerEscalar("sp_Progreso_Guardar", Hdatos);

            return Convert.ToInt32(identidad);
        }

        private void AgregarFila(List<BECursoConProgreso> ListaCursoBE, DataRow Item)
        {
            int cursoId = ServicioLectorFila.LeerEntero(Item, "CursoId");
            BECursoConProgreso oCurso = ListaCursoBE.Count > 0 ? ListaCursoBE[ListaCursoBE.Count - 1] : null;

            if (oCurso == null || oCurso.Curso.CursoId != cursoId)
            {
                BECurso oCursoBE = new BECurso();
                oCursoBE.CursoId = cursoId;
                oCursoBE.Nombre = ServicioLectorFila.LeerTexto(Item, "Curso");
                oCursoBE.Idioma = ServicioLectorFila.LeerTexto(Item, "Idioma");
                oCursoBE.Nivel = ServicioLectorFila.LeerTexto(Item, "Nivel");
                oCursoBE.DuracionHoras = ServicioLectorFila.LeerEnteroNulo(Item, "DuracionHoras");
                oCursoBE.Descripcion = ServicioLectorFila.LeerTextoNulo(Item, "Descripcion");
                oCursoBE.Sector = ServicioLectorFila.LeerTextoNulo(Item, "Sector");
                oCursoBE.FechaPublicacion = ServicioLectorFila.LeerFechaNula(Item, "FechaPublicacion");
                oCursoBE.FechaFin = ServicioLectorFila.LeerFechaNula(Item, "FechaFin");
                oCursoBE.Activo = true;

                oCurso = new BECursoConProgreso();
                oCurso.Curso = oCursoBE;
                ListaCursoBE.Add(oCurso);
            }

            if (!ServicioLectorFila.TieneValor(Item, "ModuloId"))
            {
                return;
            }

            BEModulo oModuloBE = new BEModulo(
                ServicioLectorFila.LeerEntero(Item, "ModuloId"),
                cursoId,
                ServicioLectorFila.LeerTexto(Item, "Modulo"),
                ServicioLectorFila.LeerTextoNulo(Item, "ModuloDescripcion"),
                ServicioLectorFila.LeerTextoNulo(Item, "ModuloContenido"),
                ServicioLectorFila.LeerEntero(Item, "OrdenModulo"),
                true);

            BEProgreso oProgresoBE = null;

            if (ServicioLectorFila.TieneValor(Item, "ProgresoId"))
            {
                oProgresoBE = new BEProgreso(
                    ServicioLectorFila.LeerEntero(Item, "ProgresoId"),
                    ServicioLectorFila.LeerEntero(Item, "UsuarioId"),
                    oModuloBE.ModuloId,
                    ServicioLectorFila.TieneValor(Item, "PorcentajeAvance")
                        ? ServicioLectorFila.LeerDecimal(Item, "PorcentajeAvance")
                        : (decimal?)null,
                    ServicioLectorFila.LeerFechaHoraNula(Item, "FechaInicio"),
                    ServicioLectorFila.LeerFechaHoraNula(Item, "FechaFinalizacion"),
                    ServicioLectorFila.LeerTextoNulo(Item, "Estado"));
            }

            oCurso.Modulos.Add(new BEModuloConProgreso(oModuloBE, oProgresoBE));
        }
    }

    public class MPPActivoPedagogico
    {
        Acceso oDatos = new Acceso();
        Hashtable Hdatos;

        public List<BEActivoPedagogico> ListarPorCurso(BEActivoPedagogico Objeto)
        {
            List<BEActivoPedagogico> ListaBE = new List<BEActivoPedagogico>();

            Hdatos = new Hashtable();
            Hdatos.Add("@CursoId", Objeto.CursoId);

            DataTable Dt = oDatos.Leer("sp_ActivoPedagogico_Listar", Hdatos);

            foreach (DataRow Item in Dt.Rows)
            {
                ListaBE.Add(Mapear(Item));
            }

            return ListaBE;
        }

        public BEActivoPedagogico ListarObjeto(BEActivoPedagogico Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@ActivoPedagogicoId", Objeto.ActivoPedagogicoId);

            return ServicioLectorFila.AObjeto(oDatos.Leer("sp_ActivoPedagogico_ObtenerPorId", Hdatos), Mapear);
        }

        public int Alta(BEActivoPedagogico Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@CursoId", Objeto.CursoId);
            Hdatos.Add("@ModuloId", Objeto.ModuloId);
            Hdatos.Add("@Nombre", Objeto.Nombre);
            Hdatos.Add("@TipoContenido", Objeto.TipoContenido);
            Hdatos.Add("@Descripcion", Objeto.Descripcion);
            Hdatos.Add("@UrlArchivo", Objeto.UrlArchivo);

            if (!string.IsNullOrEmpty(Objeto.Estado))
            {
                Hdatos.Add("@Estado", Objeto.Estado);
            }

            object identidad = oDatos.LeerEscalar("sp_ActivoPedagogico_Alta", Hdatos);

            return Convert.ToInt32(identidad);
        }

        public bool CambiarEstado(BEActivoPedagogico Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@ActivoPedagogicoId", Objeto.ActivoPedagogicoId);
            Hdatos.Add("@Estado", Objeto.Estado);

            object filas = oDatos.LeerEscalar("sp_ActivoPedagogico_CambiarEstado", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        public bool CambiarActivo(BEActivoPedagogico Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@ActivoPedagogicoId", Objeto.ActivoPedagogicoId);
            Hdatos.Add("@Activo", Objeto.Activo.HasValue && Objeto.Activo.Value);

            object filas = oDatos.LeerEscalar("sp_ActivoPedagogico_CambiarActivo", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        private BEActivoPedagogico Mapear(DataRow Item)
        {
            BEActivoPedagogico oActivoBE = new BEActivoPedagogico();

            oActivoBE.ActivoPedagogicoId = ServicioLectorFila.LeerEntero(Item, "ActivoPedagogicoId");
            oActivoBE.CursoId = ServicioLectorFila.LeerEntero(Item, "CursoId");
            oActivoBE.ModuloId = ServicioLectorFila.LeerEnteroNulo(Item, "ModuloId");
            oActivoBE.Modulo = ServicioLectorFila.LeerTextoNulo(Item, "Modulo");
            oActivoBE.Nombre = ServicioLectorFila.LeerTexto(Item, "Nombre");
            oActivoBE.TipoContenido = ServicioLectorFila.LeerTextoNulo(Item, "TipoContenido");
            oActivoBE.Descripcion = ServicioLectorFila.LeerTextoNulo(Item, "Descripcion");
            oActivoBE.UrlArchivo = ServicioLectorFila.LeerTextoNulo(Item, "UrlArchivo");
            oActivoBE.Activo = ServicioLectorFila.LeerBooleanoNulo(Item, "Activo");
            oActivoBE.Estado = ServicioLectorFila.LeerTextoNulo(Item, "Estado");

            return oActivoBE;
        }
    }

    public class MPPVersionContenido
    {
        Acceso oDatos = new Acceso();
        Hashtable Hdatos;

        public int Alta(BEVersionContenido Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@CursoId", Objeto.CursoId);
            Hdatos.Add("@ContenidoXml", Objeto.ContenidoXml);
            Hdatos.Add("@Observaciones", Objeto.Observaciones);
            Hdatos.Add("@UsuarioId", Objeto.UsuarioId);

            object identidad = oDatos.LeerEscalar("sp_VersionContenido_Alta", Hdatos);

            return Convert.ToInt32(identidad);
        }

        public List<BEVersionContenido> ListarPorCurso(int cursoId)
        {
            List<BEVersionContenido> ListaBE = new List<BEVersionContenido>();

            Hdatos = new Hashtable();
            Hdatos.Add("@CursoId", cursoId);

            DataTable Dt = oDatos.Leer("sp_VersionContenido_ListarPorCurso", Hdatos);

            foreach (DataRow Item in Dt.Rows)
            {
                ListaBE.Add(Mapear(Item));
            }

            return ListaBE;
        }

        public BEVersionContenido ListarObjeto(int versionContenidoId)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@VersionContenidoId", versionContenidoId);

            DataTable Dt = oDatos.Leer("sp_VersionContenido_ObtenerPorId", Hdatos);

            if (Dt.Rows.Count == 0)
            {
                return null;
            }

            BEVersionContenido oVersionBE = Mapear(Dt.Rows[0]);

            oVersionBE.ContenidoXml = ServicioLectorFila.LeerTextoNulo(Dt.Rows[0], "ContenidoXml");

            return oVersionBE;
        }

        public bool Baja(int versionContenidoId)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@VersionContenidoId", versionContenidoId);

            object filas = oDatos.LeerEscalar("sp_VersionContenido_Baja", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        private BEVersionContenido Mapear(DataRow Item)
        {
            BEVersionContenido oVersionBE = new BEVersionContenido();

            oVersionBE.VersionContenidoId = ServicioLectorFila.LeerEntero(Item, "VersionContenidoId");
            oVersionBE.CursoId = ServicioLectorFila.LeerEntero(Item, "CursoId");
            oVersionBE.NumeroVersion = ServicioLectorFila.LeerTextoNulo(Item, "NumeroVersion");
            oVersionBE.FechaVersion = ServicioLectorFila.LeerFechaHoraNula(Item, "FechaVersion");
            oVersionBE.Observaciones = ServicioLectorFila.LeerTextoNulo(Item, "Observaciones");
            oVersionBE.UsuarioId = ServicioLectorFila.LeerEnteroNulo(Item, "UsuarioId");
            oVersionBE.Autor = ServicioLectorFila.LeerTextoNulo(Item, "Autor");
            oVersionBE.LargoXml = ServicioLectorFila.LeerEntero(Item, "LargoXml");

            return oVersionBE;
        }
    }
}
