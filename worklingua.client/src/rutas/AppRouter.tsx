import { Navigate, Route, Routes } from 'react-router-dom';
import { LayoutPrivado } from '../componentes/layout/LayoutPrivado';
import { LayoutAuth, LayoutPublico } from '../componentes/layout/LayoutPublico';
import { BajaNewsletter } from '../paginas/BajaNewsletter';
import { Busqueda } from '../paginas/Busqueda';
import { CambiarClave } from '../paginas/CambiarClave';
import { CompletarInvitacion } from '../paginas/CompletarInvitacion';
import { ConfirmarCuenta } from '../paginas/ConfirmarCuenta';
import { ConfirmarNewsletter } from '../paginas/ConfirmarNewsletter';
import { Login } from '../paginas/Login';
import { AdminAlertas } from '../paginas/privadas/admin/AdminAlertas';
import { AdminBitacora } from '../paginas/privadas/admin/AdminBitacora';
import { AdminCulturas } from '../paginas/privadas/admin/AdminCulturas';
import { AdminCursos } from '../paginas/privadas/admin/AdminCursos';
import { AdminDepartamentos } from '../paginas/privadas/admin/AdminDepartamentos';
import { AdminEmpresas } from '../paginas/privadas/admin/AdminEmpresas';
import { AdminEncuestas } from '../paginas/privadas/admin/AdminEncuestas';
import { AdminIdiomas } from '../paginas/privadas/admin/AdminIdiomas';
import { AdminLicencias } from '../paginas/privadas/admin/AdminLicencias';
import { AdminNewsletter } from '../paginas/privadas/admin/AdminNewsletter';
import { AdminNoticias } from '../paginas/privadas/admin/AdminNoticias';
import { AdminOfertas } from '../paginas/privadas/admin/AdminOfertas';
import { AdminOperadores } from '../paginas/privadas/admin/AdminOperadores';
import { AdminPermisos } from '../paginas/privadas/admin/AdminPermisos';
import { AdminPlanes } from '../paginas/privadas/admin/AdminPlanes';
import { AdminRendimiento } from '../paginas/privadas/admin/AdminRendimiento';
import { AdminReportes } from '../paginas/privadas/admin/AdminReportes';
import { AdminRoles } from '../paginas/privadas/admin/AdminRoles';
import { AdminSoporte } from '../paginas/privadas/admin/AdminSoporte';
import { AdminTablero } from '../paginas/privadas/admin/AdminTablero';
import { AdminTraducciones } from '../paginas/privadas/admin/AdminTraducciones';
import { AdminUsuarios } from '../paginas/privadas/admin/AdminUsuarios';
import { CuentaCorriente } from '../paginas/privadas/CuentaCorriente';
import { Encuestas } from '../paginas/privadas/Encuestas';
import { Leccion } from '../paginas/privadas/Leccion';
import { MiPerfil } from '../paginas/privadas/MiPerfil';
import { MiPlan } from '../paginas/privadas/MiPlan';
import { MisCursos } from '../paginas/privadas/MisCursos';
import { MisOfertas } from '../paginas/privadas/MisOfertas';
import { Opiniones } from '../paginas/privadas/Opiniones';
import { Panel } from '../paginas/privadas/Panel';
import { Soporte } from '../paginas/privadas/Soporte';
import { Contacto } from '../paginas/publicas/Contacto';
import { InicioPublico } from '../paginas/publicas/InicioPublico';
import { Institucional } from '../paginas/publicas/Institucional';
import { NoEncontrado } from '../paginas/publicas/NoEncontrado';
import { NovedadDetalle } from '../paginas/publicas/NovedadDetalle';
import { Novedades } from '../paginas/publicas/Novedades';
import { Planes } from '../paginas/publicas/Planes';
import { PoliticaPrivacidad } from '../paginas/publicas/PoliticaPrivacidad';
import { PreguntasFrecuentes } from '../paginas/publicas/PreguntasFrecuentes';
import { TerminosCondiciones } from '../paginas/publicas/TerminosCondiciones';
import { RecuperarClave } from '../paginas/RecuperarClave';
import { Registro } from '../paginas/Registro';
import { RestablecerClave } from '../paginas/RestablecerClave';
import { PERMISOS } from './itemsMenu';
import { RutaConPermiso } from './RutaConPermiso';
import { RutaProtegida } from './RutaProtegida';

export function AppRouter() {
    return (
        <Routes>
            <Route element={<LayoutPublico />}>
                <Route path="/" element={<InicioPublico />} />
                <Route path="/catalogo" element={<Planes />} />
                <Route path="/novedades" element={<Novedades />} />
                <Route path="/novedades/:noticiaId" element={<NovedadDetalle />} />
                <Route path="/institucional" element={<Institucional />} />
                <Route path="/contacto" element={<Contacto />} />
                <Route path="/preguntas-frecuentes" element={<PreguntasFrecuentes />} />
                <Route path="/terminos" element={<TerminosCondiciones />} />
                <Route path="/privacidad" element={<PoliticaPrivacidad />} />
                <Route path="/buscar" element={<Busqueda publica />} />

                <Route path="/planes" element={<Navigate to="/catalogo" replace />} />
                <Route path="/cursos" element={<Navigate to="/catalogo" replace />} />
                <Route path="/catalogo/comparar" element={<Navigate to="/catalogo" replace />} />
                <Route path="/planes/comparar" element={<Navigate to="/catalogo" replace />} />

                <Route path="*" element={<NoEncontrado />} />
            </Route>

            <Route element={<LayoutAuth />}>
                <Route path="/login" element={<Login />} />
                <Route path="/registro" element={<Registro />} />
                <Route path="/recuperar-clave" element={<RecuperarClave />} />
            </Route>

            <Route path="/confirmar-cuenta" element={<ConfirmarCuenta />} />
            <Route path="/completar-invitacion" element={<CompletarInvitacion />} />
            <Route path="/restablecer-clave" element={<RestablecerClave />} />
            <Route path="/newsletter/confirmar" element={<ConfirmarNewsletter />} />
            <Route path="/newsletter/baja" element={<BajaNewsletter />} />

            <Route
                path="/inicio"
                element={
                    <RutaProtegida>
                        <LayoutPrivado />
                    </RutaProtegida>
                }
            >
                <Route index element={<Panel />} />
                <Route path="cambiar-clave" element={<CambiarClave />} />
                <Route path="perfil" element={<MiPerfil />} />
                <Route path="plan" element={<MiPlan />} />
                <Route
                    path="opiniones"
                    element={
                        <RutaConPermiso permiso={PERMISOS.comentarioParticipar}>
                            <Opiniones />
                        </RutaConPermiso>
                    }
                />
                <Route
                    path="cursos"
                    element={
                        <RutaConPermiso permiso={PERMISOS.cursoVerProgreso}>
                            <MisCursos />
                        </RutaConPermiso>
                    }
                />
                <Route
                    path="cursos/:cursoId/modulos/:moduloId"
                    element={
                        <RutaConPermiso permiso={PERMISOS.cursoVerProgreso}>
                            <Leccion />
                        </RutaConPermiso>
                    }
                />
                <Route
                    path="cuenta-corriente"
                    element={
                        <RutaConPermiso permiso={PERMISOS.cuentaCorrienteConsultar}>
                            <CuentaCorriente />
                        </RutaConPermiso>
                    }
                />
                <Route
                    path="soporte"
                    element={
                        <RutaConPermiso permiso={PERMISOS.ticketCrear}>
                            <Soporte />
                        </RutaConPermiso>
                    }
                />
                <Route
                    path="ofertas"
                    element={
                        <RutaConPermiso permiso={PERMISOS.ofertaConsultar}>
                            <MisOfertas />
                        </RutaConPermiso>
                    }
                />
                <Route
                    path="encuestas"
                    element={
                        <RutaConPermiso permiso={PERMISOS.encuestaResponder}>
                            <Encuestas />
                        </RutaConPermiso>
                    }
                />
                <Route path="buscar" element={<Busqueda />} />

                <Route
                    path="comentarios"
                    element={<Navigate to="/inicio/opiniones" replace />}
                />

                <Route
                    path="admin/licencias"
                    element={
                        <RutaConPermiso permiso={PERMISOS.licenciaListar}>
                            <AdminLicencias />
                        </RutaConPermiso>
                    }
                />
                <Route
                    path="admin/rendimiento"
                    element={
                        <RutaConPermiso permiso={PERMISOS.cursoVerProgresoEmpresa}>
                            <AdminRendimiento />
                        </RutaConPermiso>
                    }
                />
                <Route
                    path="admin/alertas"
                    element={
                        <RutaConPermiso permiso={PERMISOS.alertaListar}>
                            <AdminAlertas />
                        </RutaConPermiso>
                    }
                />
                <Route
                    path="admin/planes"
                    element={
                        <RutaConPermiso permiso={PERMISOS.planListar}>
                            <AdminPlanes />
                        </RutaConPermiso>
                    }
                />
                <Route
                    path="admin/cursos"
                    element={
                        <RutaConPermiso permiso={PERMISOS.cursoListar}>
                            <AdminCursos />
                        </RutaConPermiso>
                    }
                />
                <Route
                    path="admin/idiomas"
                    element={
                        <RutaConPermiso permiso={PERMISOS.idiomaListar}>
                            <AdminIdiomas />
                        </RutaConPermiso>
                    }
                />
                <Route
                    path="admin/traducciones"
                    element={
                        <RutaConPermiso permiso={PERMISOS.traduccionListar}>
                            <AdminTraducciones />
                        </RutaConPermiso>
                    }
                />
                <Route
                    path="admin/culturas"
                    element={
                        <RutaConPermiso permiso={PERMISOS.culturaListar}>
                            <AdminCulturas />
                        </RutaConPermiso>
                    }
                />
                <Route
                    path="admin/departamentos"
                    element={
                        <RutaConPermiso permiso={PERMISOS.departamentoListar}>
                            <AdminDepartamentos />
                        </RutaConPermiso>
                    }
                />
                <Route
                    path="admin/empresas"
                    element={
                        <RutaConPermiso permiso={PERMISOS.empresaListar}>
                            <AdminEmpresas />
                        </RutaConPermiso>
                    }
                />
                <Route
                    path="admin/usuarios"
                    element={
                        <RutaConPermiso permiso={PERMISOS.usuarioListar}>
                            <AdminUsuarios />
                        </RutaConPermiso>
                    }
                />
                <Route
                    path="admin/operadores"
                    element={
                        <RutaConPermiso permiso={PERMISOS.operadorListar}>
                            <AdminOperadores />
                        </RutaConPermiso>
                    }
                />
                <Route
                    path="admin/noticias"
                    element={
                        <RutaConPermiso permiso={PERMISOS.noticiaListar}>
                            <AdminNoticias />
                        </RutaConPermiso>
                    }
                />
                <Route
                    path="admin/newsletter"
                    element={
                        <RutaConPermiso permiso={PERMISOS.newsletterListar}>
                            <AdminNewsletter />
                        </RutaConPermiso>
                    }
                />
                <Route
                    path="admin/ofertas"
                    element={
                        <RutaConPermiso permiso={PERMISOS.ofertaListar}>
                            <AdminOfertas />
                        </RutaConPermiso>
                    }
                />
                <Route
                    path="admin/soporte"
                    element={
                        <RutaConPermiso permiso={PERMISOS.ticketAtender}>
                            <AdminSoporte />
                        </RutaConPermiso>
                    }
                />
                <Route
                    path="admin/encuestas"
                    element={
                        <RutaConPermiso permiso={PERMISOS.encuestaListar}>
                            <AdminEncuestas />
                        </RutaConPermiso>
                    }
                />
                <Route
                    path="admin/tablero"
                    element={
                        <RutaConPermiso permiso={PERMISOS.reporteVer}>
                            <AdminTablero />
                        </RutaConPermiso>
                    }
                />
                <Route
                    path="admin/reportes"
                    element={
                        <RutaConPermiso permiso={PERMISOS.reporteVer}>
                            <AdminReportes />
                        </RutaConPermiso>
                    }
                />
                <Route
                    path="admin/roles"
                    element={
                        <RutaConPermiso permiso={PERMISOS.rolListar}>
                            <AdminRoles />
                        </RutaConPermiso>
                    }
                />
                <Route
                    path="admin/permisos"
                    element={
                        <RutaConPermiso permiso={PERMISOS.permisoListar}>
                            <AdminPermisos />
                        </RutaConPermiso>
                    }
                />
                <Route
                    path="admin/bitacora"
                    element={
                        <RutaConPermiso permiso={PERMISOS.bitacoraListar}>
                            <AdminBitacora />
                        </RutaConPermiso>
                    }
                />
            </Route>

            <Route path="/cambiar-clave" element={<Navigate to="/inicio/cambiar-clave" replace />} />
        </Routes>
    );
}
