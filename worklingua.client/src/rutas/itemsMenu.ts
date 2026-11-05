export interface ItemMenu {
    etiqueta: string;
    ruta: string;
    permiso?: string;
    ocultarConPermiso?: string;
    oculto?: boolean;
}

export interface GrupoMenu {
    etiqueta: string;
    items: ItemMenu[];
    fijarAbajo?: boolean;
}

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
    faqListar: 'Faq.Listar',
    faqAlta: 'Faq.Alta',
    faqModificar: 'Faq.Modificar',
    faqBaja: 'Faq.Baja',
    reporteVer: 'Reporte.Ver',
} as const;

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
            {
                etiqueta: 'layout.menu.departamentos',
                ruta: '/inicio/admin/departamentos',
                permiso: PERMISOS.departamentoListar,
            },
            {
                etiqueta: 'layout.menu.licencias',
                ruta: '/inicio/admin/licencias',
                permiso: PERMISOS.licenciaListar,
            },
            {
                etiqueta: 'layout.menu.rendimiento',
                ruta: '/inicio/admin/rendimiento',
                permiso: PERMISOS.cursoVerProgresoEmpresa,
            },
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
            {
                etiqueta: 'layout.menu.faqs',
                ruta: '/inicio/admin/faqs',
                permiso: PERMISOS.faqListar,
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
            { etiqueta: 'layout.menu.miPerfil', ruta: '/inicio/perfil' },
        ],
    },
];

export const ITEMS_PUBLICOS: ItemMenu[] = [
    { etiqueta: 'layout.publico.inicio', ruta: '/' },
    { etiqueta: 'layout.publico.catalogo', ruta: '/catalogo' },
    { etiqueta: 'layout.publico.novedades', ruta: '/novedades' },
    { etiqueta: 'layout.publico.institucional', ruta: '/institucional' },
    { etiqueta: 'layout.publico.faqs', ruta: '/preguntas-frecuentes' },
    { etiqueta: 'layout.publico.contacto', ruta: '/contacto' },
];
