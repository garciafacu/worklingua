using System.Collections.Generic;
using worklingua.Server.BE;
using worklingua.Server.MPP;

namespace worklingua.Server.BLL
{
    public class BLLProgreso
    {
        public const string EstadoNoIniciado = "NO_INICIADO";
        public const string EstadoEnCurso = "EN_CURSO";
        public const string EstadoCompletado = "COMPLETADO";

        public const string AccionIniciar = "INICIAR";
        public const string AccionCompletar = "COMPLETAR";

        MPPProgreso oMPPPro;
        BLLUsuario oBLLUsu;
        BLLLicencia oBLLLic;
        BLLBitacora oBLLBit;
        BLLActivoPedagogico oBLLAct;
        BLLEtiqueta oBLLEti;

        public BLLProgreso()
        {
            oMPPPro = new MPPProgreso();
            oBLLUsu = new BLLUsuario();
            oBLLLic = new BLLLicencia();
            oBLLBit = new BLLBitacora();
            oBLLAct = new BLLActivoPedagogico();
            oBLLEti = new BLLEtiqueta();
        }

        /// <summary>
        /// Mis cursos. Es el punto por el que pasa tambien Guardar, asi que el
        /// control de licencia vive aca una sola vez (CU-001-005).
        /// </summary>
        public List<BECursoConProgreso> ListarPorUsuario(BESesion oSesionBE)
        {
            BEUsuario oUsuarioBE = oBLLUsu.ObtenerPorId(oSesionBE.UsuarioId);

            oBLLLic.ExigirLicenciaVigente(oUsuarioBE);

            List<BECursoConProgreso> ListaCursoBE = Calcular(oMPPPro.ListarPorUsuario(oUsuarioBE));

            AgregarActivosYClasificacion(ListaCursoBE);

            return ListaCursoBE;
        }

        /// <summary>
        /// Suma a cada curso su material de apoyo (CU-004-001).
        ///
        /// Va acá y no en el SP de progreso porque la DAL devuelve un DataTable
        /// por llamada, y qué activo está disponible lo decide
        /// BLLActivoPedagogico: los cursos ya los filtró el alcance de arriba.
        /// </summary>
        private void AgregarActivosYClasificacion(List<BECursoConProgreso> ListaCursoBE)
        {
            List<BECurso> ListaSoloCursoBE = new List<BECurso>();

            foreach (BECursoConProgreso oCursoBE in ListaCursoBE)
            {
                oCursoBE.Activos = oBLLAct.ListarPublicados(oCursoBE.Curso.CursoId);
                ListaSoloCursoBE.Add(oCursoBE.Curso);
            }

            // La clasificación se arma en BLLEtiqueta, el mismo lugar que usa
            // el ABM de Cursos (CU-004-005).
            oBLLEti.AgregarACursos(ListaSoloCursoBE);
        }

        public List<BEProgresoUsuario> ListarPorEmpresa(BESesion oSesionBE)
        {
            BEUsuario oUsuarioBE = oBLLUsu.ObtenerPorId(oSesionBE.UsuarioId);

            BEEmpresa oEmpresaBE = new BEEmpresa();
            oEmpresaBE.EmpresaId = oUsuarioBE.EmpresaId;

            List<BEProgresoUsuario> ListaUsuarioBE = oMPPPro.ListarPorEmpresa(oEmpresaBE);

            if (ListaUsuarioBE == null)
            {
                return new List<BEProgresoUsuario>();
            }

            foreach (BEProgresoUsuario oUsuario in ListaUsuarioBE)
            {
                Calcular(oUsuario.Cursos);
            }

            return ListaUsuarioBE;
        }

        public List<BECursoConProgreso> Guardar(BEGuardarProgreso Objeto, BESesion oSesionBE)
        {
            string accion = Objeto.Accion == null ? string.Empty : Objeto.Accion.Trim().ToUpperInvariant();

            if (accion != AccionIniciar && accion != AccionCompletar)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "La acción tiene que ser " + AccionIniciar + " o " + AccionCompletar + ".");
            }

            List<BECursoConProgreso> ListaCursoBE = ListarPorUsuario(oSesionBE);
            BECursoConProgreso oCurso = null;
            BEModuloConProgreso oModulo = null;

            foreach (BECursoConProgreso oItemCurso in ListaCursoBE)
            {
                foreach (BEModuloConProgreso oItemModulo in oItemCurso.Modulos)
                {
                    if (oItemModulo.Modulo.ModuloId == Objeto.ModuloId)
                    {
                        oCurso = oItemCurso;
                        oModulo = oItemModulo;
                    }
                }
            }

            if (oModulo == null)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.NoEncontrado, "El módulo no existe o no está disponible.");
            }

            BEProgreso oActualBE = oModulo.Progreso;
            DateTime ahora = DateTime.Now;

            if (accion == AccionIniciar && oActualBE != null)
            {
                return ListaCursoBE;
            }

            if (accion == AccionCompletar && oActualBE != null && oActualBE.Estado == EstadoCompletado)
            {
                return ListaCursoBE;
            }

            BEProgreso oProgresoBE = accion == AccionIniciar
                ? new BEProgreso(0, oSesionBE.UsuarioId, Objeto.ModuloId, 0m, ahora, null, EstadoEnCurso)
                : new BEProgreso(
                    oActualBE == null ? 0 : oActualBE.ProgresoId,
                    oSesionBE.UsuarioId,
                    Objeto.ModuloId,
                    100m,
                    oActualBE == null || !oActualBE.FechaInicio.HasValue ? ahora : oActualBE.FechaInicio,
                    ahora,
                    EstadoCompletado);

            oMPPPro.Guardar(oProgresoBE);

            Registrar(
                oSesionBE.UsuarioId,
                accion == AccionIniciar ? "ModuloIniciado" : "ModuloCompletado",
                "Módulo " + oModulo.Modulo.Nombre + " del curso " + oCurso.Curso.Nombre + ".");

            List<BECursoConProgreso> ListaActualizadaBE = ListarPorUsuario(oSesionBE);

            foreach (BECursoConProgreso oItemCurso in ListaActualizadaBE)
            {
                if (oItemCurso.Curso.CursoId == oCurso.Curso.CursoId
                    && oItemCurso.Estado == EstadoCompletado
                    && oCurso.Estado != EstadoCompletado)
                {
                    Registrar(oSesionBE.UsuarioId, "CursoCompletado", "Curso " + oCurso.Curso.Nombre + " completado.");
                }
            }

            return ListaActualizadaBE;
        }

        private List<BECursoConProgreso> Calcular(List<BECursoConProgreso> ListaCursoBE)
        {
            if (ListaCursoBE == null)
            {
                return new List<BECursoConProgreso>();
            }

            foreach (BECursoConProgreso oCurso in ListaCursoBE)
            {
                int total = oCurso.Modulos.Count;
                int iniciados = 0;
                int completados = 0;
                DateTime? ultimaActividad = null;

                foreach (BEModuloConProgreso oModulo in oCurso.Modulos)
                {
                    if (oModulo.Progreso == null)
                    {
                        continue;
                    }

                    iniciados++;

                    if (oModulo.Progreso.Estado == EstadoCompletado)
                    {
                        completados++;
                    }

                    DateTime? fecha = oModulo.Progreso.FechaFinalizacion ?? oModulo.Progreso.FechaInicio;

                    if (fecha.HasValue && (!ultimaActividad.HasValue || fecha.Value > ultimaActividad.Value))
                    {
                        ultimaActividad = fecha;
                    }
                }

                oCurso.PorcentajeAvance = total == 0 ? 0m : Math.Round(completados * 100m / total, 0);
                oCurso.UltimaActividad = ultimaActividad;

                if (iniciados == 0)
                {
                    oCurso.Estado = EstadoNoIniciado;
                }
                else if (completados == total)
                {
                    oCurso.Estado = EstadoCompletado;
                }
                else
                {
                    oCurso.Estado = EstadoEnCurso;
                }
            }

            return ListaCursoBE;
        }

        private void Registrar(int usuarioId, string accion, string descripcion)
        {
            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                usuarioId,
                DateTime.Now,
                BLLBitacora.ModuloCurso,
                accion,
                descripcion,
                BLLBitacora.NivelInformativo));
        }
    }
}
