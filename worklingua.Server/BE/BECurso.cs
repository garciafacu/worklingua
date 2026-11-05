namespace worklingua.Server.BE
{
    public class BECurso
    {
        public int CursoId { get; set; }
        public string Idioma { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Nivel { get; set; }
        public int? DuracionHoras { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaAlta { get; set; }
        public int? EmpresaId { get; set; }
        public string Sector { get; set; }
        public DateOnly? FechaPublicacion { get; set; }
        public DateOnly? FechaFin { get; set; }
        public List<BEEtiqueta> Etiquetas { get; set; } = new List<BEEtiqueta>();
    }

    public class BECursoConProgreso
    {
        public BECurso Curso { get; set; }
        public List<BEModuloConProgreso> Modulos { get; set; } = new List<BEModuloConProgreso>();
        public List<BEActivoPedagogico> Activos { get; set; } = new List<BEActivoPedagogico>();
        public decimal PorcentajeAvance { get; set; }
        public string Estado { get; set; }
        public DateTime? UltimaActividad { get; set; }
    }

    public class BECursoRespuesta
    {
        public int CursoId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Nivel { get; set; }
        public int? DuracionHoras { get; set; }
        public string Idioma { get; set; }
        public bool EsGlobal { get; set; }
        public string Sector { get; set; }
        public List<BEEtiqueta> Etiquetas { get; set; }
    }

    public class BEGuardarCurso
    {
        public string Idioma { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Nivel { get; set; }
        public int? DuracionHoras { get; set; }
        public string Sector { get; set; }
        public DateOnly? FechaPublicacion { get; set; }
        public DateOnly? FechaFin { get; set; }
        public List<int> EtiquetaIds { get; set; }
    }

    public class BEFiltroCurso
    {
        public string Nombre { get; set; }
        public string Idioma { get; set; }
        public string Nivel { get; set; }
        public string Sector { get; set; }
        public int? EtiquetaId { get; set; }
        public int? EmpresaId { get; set; }
        public bool IncluirFueraDeVentana { get; set; }

        public BEFiltroCurso()
        {
        }

        public BEFiltroCurso(string nombre, string idioma, string nivel)
        {
            this.Nombre = nombre;
            this.Idioma = idioma;
            this.Nivel = nivel;
        }
    }

    public class BEModulo
    {
        public int ModuloId { get; set; }
        public int CursoId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Contenido { get; set; }
        public int OrdenModulo { get; set; }
        public bool? Activo { get; set; }

        public BEModulo()
        {
        }

        public BEModulo(
            int moduloId,
            int cursoId,
            string nombre,
            string descripcion,
            string contenido,
            int ordenModulo,
            bool? activo)
        {
            this.ModuloId = moduloId;
            this.CursoId = cursoId;
            this.Nombre = nombre;
            this.Descripcion = descripcion;
            this.Contenido = contenido;
            this.OrdenModulo = ordenModulo;
            this.Activo = activo;
        }
    }

    public class BEModuloConProgreso
    {
        public BEModulo Modulo { get; set; }
        public BEProgreso Progreso { get; set; }

        public BEModuloConProgreso()
        {
        }

        public BEModuloConProgreso(BEModulo modulo, BEProgreso progreso)
        {
            this.Modulo = modulo;
            this.Progreso = progreso;
        }
    }

    public class BEGuardarModulo
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Contenido { get; set; }
        public int OrdenModulo { get; set; }
    }

    public class BEEtiqueta
    {
        public int EtiquetaId { get; set; }
        public string Nombre { get; set; }
        public bool? Activo { get; set; }
        public DateTime? FechaAlta { get; set; }

        public BEEtiqueta()
        {
        }

        public BEEtiqueta(int etiquetaId, string nombre, bool? activo, DateTime? fechaAlta)
        {
            this.EtiquetaId = etiquetaId;
            this.Nombre = nombre;
            this.Activo = activo;
            this.FechaAlta = fechaAlta;
        }
    }

    public class BEEtiquetaCurso
    {
        public int CursoId { get; set; }
        public int EtiquetaId { get; set; }
        public string Nombre { get; set; }
    }

    public class BEProgreso
    {
        public int ProgresoId { get; set; }
        public int UsuarioId { get; set; }
        public int ModuloId { get; set; }
        public decimal? PorcentajeAvance { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFinalizacion { get; set; }
        public string Estado { get; set; }

        public BEProgreso()
        {
        }

        public BEProgreso(
            int progresoId,
            int usuarioId,
            int moduloId,
            decimal? porcentajeAvance,
            DateTime? fechaInicio,
            DateTime? fechaFinalizacion,
            string estado)
        {
            this.ProgresoId = progresoId;
            this.UsuarioId = usuarioId;
            this.ModuloId = moduloId;
            this.PorcentajeAvance = porcentajeAvance;
            this.FechaInicio = fechaInicio;
            this.FechaFinalizacion = fechaFinalizacion;
            this.Estado = estado;
        }
    }

    public class BEProgresoUsuario
    {
        public int UsuarioId { get; set; }
        public string Usuario { get; set; }
        public string Email { get; set; }
        public List<BECursoConProgreso> Cursos { get; set; }

        public BEProgresoUsuario()
        {
            this.Cursos = new List<BECursoConProgreso>();
        }

        public BEProgresoUsuario(int usuarioId, string usuario, string email, List<BECursoConProgreso> cursos)
        {
            this.UsuarioId = usuarioId;
            this.Usuario = usuario;
            this.Email = email;
            this.Cursos = cursos;
        }
    }

    public class BEGuardarProgreso
    {
        public int ModuloId { get; set; }
        public string Accion { get; set; }

        public BEGuardarProgreso()
        {
        }

        public BEGuardarProgreso(int moduloId, string accion)
        {
            this.ModuloId = moduloId;
            this.Accion = accion;
        }
    }

    public class BEActivoPedagogico
    {
        public int ActivoPedagogicoId { get; set; }
        public int CursoId { get; set; }
        public int? ModuloId { get; set; }
        public string Modulo { get; set; }
        public string Nombre { get; set; }
        public string TipoContenido { get; set; }
        public string Descripcion { get; set; }
        public string UrlArchivo { get; set; }
        public bool? Activo { get; set; }
        public string Estado { get; set; }
    }

    public class BEActivoRespuesta
    {
        public int ActivoPedagogicoId { get; set; }
        public int CursoId { get; set; }
        public int? ModuloId { get; set; }
        public string Modulo { get; set; }
        public string Nombre { get; set; }
        public string TipoContenido { get; set; }
        public string Descripcion { get; set; }
        public string UrlArchivo { get; set; }
        public bool Activo { get; set; }
        public string Estado { get; set; }
    }

    public class BEArchivoSubido
    {
        public string NombreOriginal { get; set; }
        public byte[] Contenido { get; set; }

        public BEArchivoSubido()
        {
        }

        public BEArchivoSubido(string nombreOriginal, byte[] contenido)
        {
            this.NombreOriginal = nombreOriginal;
            this.Contenido = contenido;
        }
    }

    public class BEVersionContenido
    {
        public int VersionContenidoId { get; set; }
        public int CursoId { get; set; }
        public string NumeroVersion { get; set; }
        public DateTime? FechaVersion { get; set; }
        public string Observaciones { get; set; }
        public int? UsuarioId { get; set; }
        public string Autor { get; set; }
        public string ContenidoXml { get; set; }
        public int LargoXml { get; set; }
    }

    public class BEDiferenciaVersion
    {
        public string Campo { get; set; }
        public string Izquierda { get; set; }
        public string Derecha { get; set; }

        public BEDiferenciaVersion()
        {
        }

        public BEDiferenciaVersion(string campo, string izquierda, string derecha)
        {
            this.Campo = campo;
            this.Izquierda = izquierda;
            this.Derecha = derecha;
        }
    }

    public class BESnapshotCurso
    {
        public BECurso Curso { get; set; } = new BECurso();
        public List<BEModulo> Modulos { get; set; } = new List<BEModulo>();
        public List<BEActivoPedagogico> Activos { get; set; } = new List<BEActivoPedagogico>();
    }

    public class BEResultadoRestauracion
    {
        public int CursoId { get; set; }
        public int Omitidos { get; set; }
        public List<string> NombresOmitidos { get; set; } = new List<string>();
    }
}
