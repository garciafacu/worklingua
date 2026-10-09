export interface ItemMenu {
    /**
     * Clave de traducción de la etiqueta, no el texto.
     *
     * `MenuLateral` y `EncabezadoPublico` la resuelven con `t()`, así que el
     * menú cambia de idioma con el resto de la interfaz sin tocar este archivo.
     */
    etiqueta: string;
    ruta: string;
    /**
     * Permiso que habilita el ítem. Sin permiso declarado, el ítem se muestra
     * siempre; con permiso, `MenuLateral` lo filtra contra los permisos de la
     * sesión.
     */
    permiso?: string;
    /**
     * Oculta el ítem a quien tiene este permiso. Sirve para la versión acotada
     * de una pantalla (por ejemplo "Mi empresa") que no tiene que aparecer junto
     * a la completa.
     */
    ocultarConPermiso?: string;
    /**
     * Saca el ítem del menú sin borrarlo.
     *
     * La ruta, la pantalla y sus traducciones siguen existiendo y se puede
     * entrar escribiendo la dirección: lo único que desaparece es el enlace.
     * Se usa para funcionalidad terminada que todavía no se quiere ofrecer, y
     * se vuelve a mostrar quitando esta línea.
     */
    oculto?: boolean;
}

/** Un bloque del menú lateral, con su título y sus ítems. */
export interface GrupoMenu {
    /** Clave de traducción del encabezado del bloque. */
    etiqueta: string;
    items: ItemMenu[];
    /**
     * Ancla el bloque al pie del menú, separado del resto por una línea.
     *
     * Es la zona de cuenta y sesión, que en la navegación de un SaaS va abajo y
     * aparte de las secciones de trabajo. `MenuLateral` lo empuja con `mt-auto`.
     */
    fijarAbajo?: boolean;
}

/**
 * Códigos de permiso que usa el frontend.
 *
 * Espejo de `worklingua.Server/BLL/Permisos.cs`, que es la fuente de verdad:
 * los valores tienen que coincidir con `Permiso.Nombre` en la base.
 */
export const PERMISOS = {
    planListar: 'Plan.Listar',
    cursoListar: 'Curso.Listar',
    caracteristicaListar: 'Caracteristica.Listar',
    idiomaListar: 'Idioma.Listar',
    traduccionListar: 'Traduccion.Listar',
    culturaListar: 'Cultura.Listar',
    empresaListar: 'Empresa.Listar',
    empresaVerTodasLasEmpresas: 'Empresa.VerTodasLasEmpresas',
    departamentoListar: 'Departamento.Listar',
    departamentoAlta: 'Departamento.Alta',
    departamentoModificar: 'Departamento.Modificar',
    departamentoBaja: 'Departamento.Baja',
    departamentoVerTodasLasEmpresas: 'Departamento.VerTodasLasEmpresas',
    alertaListar: 'Alerta.Listar',
    alertaAlta: 'Alerta.Alta',
    alertaEmitir: 'Alerta.Emitir',
    alertaBaja: 'Alerta.Baja',
    alertaVerTodasLasEmpresas: 'Alerta.VerTodasLasEmpresas',
    licenciaListar: 'Licencia.Listar',
    licenciaAsignar: 'Licencia.Asignar',
    licenciaRevocar: 'Licencia.Revocar',
    licenciaVerTodasLasEmpresas: 'Licencia.VerTodasLasEmpresas',
    usuarioListar: 'Usuario.Listar',
    comentarioParticipar: 'Comentario.Participar',
    bitacoraListar: 'Bitacora.Listar',
    usuarioVerTodasLasEmpresas: 'Usuario.VerTodasLasEmpresas',
    rolListar: 'Rol.Listar',
    permisoListar: 'Permiso.Listar',
    operadorListar: 'Operador.Listar',
    noticiaListar: 'Noticia.Listar',
    noticiaAlta: 'Noticia.Alta',
    noticiaModificar: 'Noticia.Modificar',
    noticiaBaja: 'Noticia.Baja',
    newsletterListar: 'Newsletter.Listar',
    newsletterEnviar: 'Newsletter.Enviar',
    suscripcionContratar: 'Suscripcion.Contratar',
    cursoVerProgreso: 'Curso.VerProgreso',
    cursoVerProgresoEmpresa: 'Curso.VerProgresoEmpresa',
    cursoVerTodasLasEmpresas: 'Curso.VerTodasLasEmpresas',
    cuentaCorrienteConsultar: 'CuentaCorriente.Consultar',
    ticketCrear: 'Ticket.Crear',
    ticketAtender: 'Ticket.Atender',
    ofertaConsultar: 'Oferta.Consultar',
    ofertaListar: 'Oferta.Listar',
    ofertaAlta: 'Oferta.Alta',
    ofertaModificar: 'Oferta.Modificar',
    ofertaBaja: 'Oferta.Baja',
    encuestaResponder: 'Encuesta.Responder',
    encuestaListar: 'Encuesta.Listar',
    encuestaAlta: 'Encuesta.Alta',
    encuestaModificar: 'Encuesta.Modificar',
    encuestaBaja: 'Encuesta.Baja',
    reporteVer: 'Reporte.Ver',
} as const;

/**
 * Menú lateral del área privada, en bloques, con la jerarquía típica de un SaaS.
 *
 * Arriba lo que se ADMINISTRA de la plataforma, con Usuarios de primero como
 * sección principal. Después COMUNICACIÓN: noticias y newsletter. Después
 * SEGURIDAD: operadores, roles, permisos y bitácora,
 * que solo ve el Administrador de Plataforma. Después MI WORKLINGUA, el trabajo
 * del día a día. Y anclada al pie, la cuenta: perfil y contraseña.
 *
 * Solo se listan pantallas que existen y funcionan: un ítem sin ruta real sería
 * un enlace muerto. Los permisos y las rutas son los mismos de siempre; agrupar
 * y reordenar es presentación y no cambia quién ve qué.
 *
 * `MenuLateral` oculta un bloque entero cuando ninguno de sus ítems sobrevive al
 * filtro, así que Administración y Seguridad desaparecen solas para quien no
 * tiene sus permisos. Los permisos se releen del backend en cada navegación
 * (`LayoutPrivado`), así que el menú sigue a los cambios de roles y familias.
 */
export const GRUPOS_MENU: GrupoMenu[] = [
    {
        etiqueta: 'layout.menu.grupo.administracion',
        items: [
            {
                etiqueta: 'layout.menu.tablero',
                ruta: '/inicio/admin/tablero',
                permiso: PERMISOS.reporteVer,
            },
            {
                etiqueta: 'layout.menu.reportes',
                ruta: '/inicio/admin/reportes',
                permiso: PERMISOS.reporteVer,
            },
            {
                etiqueta: 'layout.menu.usuarios',
                ruta: '/inicio/admin/usuarios',
                permiso: PERMISOS.usuarioListar,
            },
            {
                etiqueta: 'layout.menu.empresas',
                ruta: '/inicio/admin/empresas',
                permiso: PERMISOS.empresaVerTodasLasEmpresas,
            },
            // La misma pantalla para los dos alcances: sin
            // Departamento.VerTodasLasEmpresas el backend la acota a la empresa
            // del usuario, así que el Administrador Empresa ve acá su
            // organigrama y no hace falta un ítem aparte.
            {
                etiqueta: 'layout.menu.departamentos',
                ruta: '/inicio/admin/departamentos',
                permiso: PERMISOS.departamentoListar,
            },
            // Misma lógica que Departamentos: una sola pantalla, acotada por
            // el backend a la empresa del usuario cuando no tiene alcance total.
            {
                etiqueta: 'layout.menu.licencias',
                ruta: '/inicio/admin/licencias',
                permiso: PERMISOS.licenciaListar,
            },
            // Ídem: el panel de rendimiento es el de la empresa propia salvo
            // que la sesión tenga Curso.VerTodasLasEmpresas.
            {
                etiqueta: 'layout.menu.rendimiento',
                ruta: '/inicio/admin/rendimiento',
                permiso: PERMISOS.cursoVerProgresoEmpresa,
            },
            // Ídem: las alertas preventivas son las de la empresa propia sin
            // Alerta.VerTodasLasEmpresas.
            {
                etiqueta: 'layout.menu.alertas',
                ruta: '/inicio/admin/alertas',
                permiso: PERMISOS.alertaListar,
            },
            {
                etiqueta: 'layout.menu.planes',
                ruta: '/inicio/admin/planes',
                permiso: PERMISOS.planListar,
            },
            {
                etiqueta: 'layout.menu.idiomas',
                ruta: '/inicio/admin/idiomas',
                permiso: PERMISOS.idiomaListar,
            },
            {
                etiqueta: 'layout.menu.culturas',
                ruta: '/inicio/admin/culturas',
                permiso: PERMISOS.culturaListar,
            },
            {
                etiqueta: 'layout.menu.traducciones',
                ruta: '/inicio/admin/traducciones',
                permiso: PERMISOS.traduccionListar,
            },
        ],
    },
    {
        etiqueta: 'layout.menu.grupo.comunicacion',
        items: [
            {
                etiqueta: 'layout.menu.noticias',
                ruta: '/inicio/admin/noticias',
                permiso: PERMISOS.noticiaListar,
            },
            {
                etiqueta: 'layout.menu.newsletter',
                ruta: '/inicio/admin/newsletter',
                permiso: PERMISOS.newsletterListar,
            },
            {
                etiqueta: 'layout.menu.adminOfertas',
                ruta: '/inicio/admin/ofertas',
                permiso: PERMISOS.ofertaListar,
            },
            {
                etiqueta: 'layout.menu.adminEncuestas',
                ruta: '/inicio/admin/encuestas',
                permiso: PERMISOS.encuestaListar,
            },
        ],
    },
    {
        etiqueta: 'layout.menu.grupo.atencion',
        items: [
            {
                etiqueta: 'layout.menu.adminSoporte',
                ruta: '/inicio/admin/soporte',
                permiso: PERMISOS.ticketAtender,
            },
        ],
    },
    {
        etiqueta: 'layout.menu.grupo.seguridad',
        items: [
            {
                etiqueta: 'layout.menu.operadores',
                ruta: '/inicio/admin/operadores',
                permiso: PERMISOS.operadorListar,
            },
            {
                etiqueta: 'layout.menu.roles',
                ruta: '/inicio/admin/roles',
                permiso: PERMISOS.rolListar,
            },
            {
                etiqueta: 'layout.menu.permisos',
                ruta: '/inicio/admin/permisos',
                permiso: PERMISOS.permisoListar,
            },
            {
                etiqueta: 'layout.menu.bitacora',
                ruta: '/inicio/admin/bitacora',
                permiso: PERMISOS.bitacoraListar,
            },
        ],
    },
    {
        etiqueta: 'layout.menu.grupo.miWorklingua',
        items: [
            {
                etiqueta: 'layout.menu.misCursos',
                ruta: '/inicio/cursos',
                permiso: PERMISOS.cursoVerProgreso,
            },
            {
                etiqueta: 'layout.menu.cursos',
                ruta: '/inicio/admin/cursos',
                permiso: PERMISOS.cursoListar,
            },
            // La misma pantalla que Empresas, acotada por el backend a la
            // empresa del usuario.
            {
                etiqueta: 'layout.menu.miEmpresa',
                ruta: '/inicio/admin/empresas',
                permiso: PERMISOS.empresaListar,
                ocultarConPermiso: PERMISOS.empresaVerTodasLasEmpresas,
            },
            {
                etiqueta: 'layout.menu.opiniones',
                ruta: '/inicio/opiniones',
                permiso: PERMISOS.comentarioParticipar,
            },
            // Lo ve quien puede contratar: la ruta sigue abierta a cualquier
            // sesión, que la usa en modo consulta.
            {
                etiqueta: 'layout.menu.miPlan',
                ruta: '/inicio/plan',
                permiso: PERMISOS.suscripcionContratar,
            },
            {
                etiqueta: 'layout.menu.cuentaCorriente',
                ruta: '/inicio/cuenta-corriente',
                permiso: PERMISOS.cuentaCorrienteConsultar,
            },
            {
                etiqueta: 'layout.menu.misOfertas',
                ruta: '/inicio/ofertas',
                permiso: PERMISOS.ofertaConsultar,
            },
            {
                etiqueta: 'layout.menu.soporte',
                ruta: '/inicio/soporte',
                permiso: PERMISOS.ticketCrear,
            },
            {
                etiqueta: 'layout.menu.encuestas',
                ruta: '/inicio/encuestas',
                permiso: PERMISOS.encuestaResponder,
            },
        ],
    },
    {
        etiqueta: 'layout.menu.grupo.cuenta',
        fijarAbajo: true,
        items: [
            // El panel (/inicio) no va en el menú: es la pantalla de llegada tras
            // el login y el logo de la barra lleva ahí. Lo que mostraba de la
            // cuenta lo cubre Mi perfil, que además ofrece el cambio de clave.
            { etiqueta: 'layout.menu.miPerfil', ruta: '/inicio/perfil' },
        ],
    },
];

/**
 * Enlaces de la navegación pública, en el orden en que se muestran.
 *
 * Términos y Privacidad no están acá: viven en el pie (`PieDePagina`), como es
 * habitual en un SaaS, y así el encabezado entra en una sola línea.
 */
export const ITEMS_PUBLICOS: ItemMenu[] = [
    { etiqueta: 'layout.publico.inicio', ruta: '/' },
    { etiqueta: 'layout.publico.catalogo', ruta: '/catalogo' },
    { etiqueta: 'layout.publico.novedades', ruta: '/novedades' },
    { etiqueta: 'layout.publico.institucional', ruta: '/institucional' },
    { etiqueta: 'layout.publico.faqs', ruta: '/preguntas-frecuentes' },
    { etiqueta: 'layout.publico.contacto', ruta: '/contacto' },
];
