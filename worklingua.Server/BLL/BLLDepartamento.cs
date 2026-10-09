using System.Collections.Generic;
using worklingua.Server.BE;
using worklingua.Server.MPP;

namespace worklingua.Server.BLL
{
    /// <summary>
    /// El organigrama departamental (CU-001-009). Sin
    /// Departamento.VerTodasLasEmpresas se ve y se gestiona únicamente el
    /// organigrama de la empresa del usuario, igual que "Mi empresa".
    /// </summary>
    public class BLLDepartamento
    {
        private const int LongitudMaximaNombre = 100;
        private const int LongitudMaximaDescripcion = 250;

        MPPDepartamento oMPPDep;
        MPPUsuario oMPPUsu;
        BLLSeguridad oBLLSeg;
        BLLEmpresa oBLLEmp;

        public BLLDepartamento()
        {
            oMPPDep = new MPPDepartamento();
            oMPPUsu = new MPPUsuario();
            oBLLSeg = new BLLSeguridad();
            oBLLEmp = new BLLEmpresa();
        }

        public List<BEDepartamento> ListarAdministracion(BESesion oSesionBE)
        {
            BEDepartamento oFiltroBE = new BEDepartamento();
            int? empresaQueLimita = EmpresaQueLimita(oSesionBE);

            if (empresaQueLimita.HasValue)
            {
                oFiltroBE.EmpresaId = empresaQueLimita.Value;
            }

            List<BEDepartamento> ListaDepartamentoBE = oMPPDep.ListarTodoConBajas(oFiltroBE);

            return ListaDepartamentoBE == null ? new List<BEDepartamento>() : ListaDepartamentoBE;
        }

        /// <summary>
        /// Los departamentos que se le pueden asignar a un usuario de una
        /// empresa: solo los activos, porque uno dado de baja ya no es parte del
        /// organigrama.
        ///
        /// Quien no tiene alcance sobre todas las empresas recibe siempre los de
        /// la suya, sin importar qué empresa haya pedido: es el mismo criterio
        /// con el que el backend le fuerza la empresa al guardar el usuario.
        /// </summary>
        public List<BEDepartamento> ListarAsignables(BEDepartamento Objeto, BESesion oSesionBE)
        {
            int? empresaQueLimita = EmpresaQueLimita(oSesionBE);

            if (empresaQueLimita.HasValue)
            {
                Objeto.EmpresaId = empresaQueLimita.Value;
            }

            if (Objeto.EmpresaId == 0)
            {
                return new List<BEDepartamento>();
            }

            return ListarActivos(Objeto);
        }

        private List<BEDepartamento> ListarActivos(BEDepartamento Objeto)
        {
            List<BEDepartamento> ListaDepartamentoBE = oMPPDep.ListarTodo(Objeto);

            return ListaDepartamentoBE == null ? new List<BEDepartamento>() : ListaDepartamentoBE;
        }

        /// <summary>Un departamento de otra empresa se informa como inexistente.</summary>
        public BEDepartamento ListarObjeto(BEDepartamento Objeto, BESesion oSesionBE)
        {
            BEDepartamento oDepartamentoBE = ListarObjeto(Objeto);
            int? empresaQueLimita = EmpresaQueLimita(oSesionBE);

            if (empresaQueLimita.HasValue && oDepartamentoBE.EmpresaId != empresaQueLimita.Value)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.NoEncontrado, "El departamento no existe.");
            }

            return oDepartamentoBE;
        }

        public BEDepartamento Guardar(BEDepartamento Objeto, BESesion oSesionBE)
        {
            Validar(Objeto);

            bool esAlta = Objeto.DepartamentoId == 0;
            int? empresaQueLimita = EmpresaQueLimita(oSesionBE);

            if (esAlta)
            {
                if (empresaQueLimita.HasValue)
                {
                    Objeto.EmpresaId = empresaQueLimita.Value;
                }

                if (Objeto.EmpresaId == 0)
                {
                    throw new ExcepcionNegocio(
                        TipoErrorNegocio.Validacion, "Elegí la empresa del departamento.");
                }

                ExigirEmpresaExistente(Objeto.EmpresaId);
            }
            else
            {
                // La empresa de un departamento no se cambia: mover el sector de
                // empresa dejaría a sus empleados en un organigrama ajeno.
                Objeto.EmpresaId = ListarObjeto(Objeto, oSesionBE).EmpresaId;
            }

            if (ExisteOtroConNombre(Objeto))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto, "Ya existe un departamento con ese nombre.");
            }

            Objeto.Activo = true;
            Objeto.DepartamentoId = oMPPDep.Guardar(Objeto);

            return ListarObjeto(Objeto);
        }

        public bool Baja(BEDepartamento Objeto, BESesion oSesionBE)
        {
            BEDepartamento oDepartamentoBE = ListarObjeto(Objeto, oSesionBE);

            if (oDepartamentoBE.Activo != true)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto, "El departamento ya está dado de baja.");
            }

            ExigirSinEmpleados(oDepartamentoBE);

            return oMPPDep.Baja(Objeto);
        }

        public bool CambiarEstado(BEDepartamento Objeto, BESesion oSesionBE)
        {
            BEDepartamento oDepartamentoBE = ListarObjeto(Objeto, oSesionBE);

            bool activar = Objeto.Activo.HasValue && Objeto.Activo.Value;

            if (oDepartamentoBE.Activo == activar)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto,
                    activar ? "El departamento ya está activo." : "El departamento ya está desactivado.");
            }

            if (!activar)
            {
                ExigirSinEmpleados(oDepartamentoBE);
            }

            if (activar && ExisteOtroConNombre(oDepartamentoBE))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto, "Ya existe un departamento con ese nombre.");
            }

            return oMPPDep.CambiarEstado(Objeto);
        }

        /// <summary>
        /// Valida que un departamento se pueda asignar a un usuario de esa
        /// empresa. Devuelve null cuando el usuario queda sin departamento.
        /// </summary>
        public int? ResolverParaUsuario(int? departamentoId, int empresaId)
        {
            if (!departamentoId.HasValue || departamentoId.Value == 0)
            {
                return null;
            }

            BEDepartamento oFiltroBE = new BEDepartamento();
            oFiltroBE.DepartamentoId = departamentoId.Value;

            BEDepartamento oDepartamentoBE = oMPPDep.ListarObjeto(oFiltroBE);

            if (oDepartamentoBE == null || oDepartamentoBE.Activo != true ||
                oDepartamentoBE.EmpresaId != empresaId)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El departamento elegido no existe en el organigrama de la empresa.");
            }

            return oDepartamentoBE.DepartamentoId;
        }

        public BEDepartamento ListarObjeto(BEDepartamento Objeto)
        {
            BEDepartamento oDepartamentoBE = oMPPDep.ListarObjeto(Objeto);

            if (oDepartamentoBE == null)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.NoEncontrado, "El departamento no existe.");
            }

            return oDepartamentoBE;
        }

        /// <summary>
        /// Sin esto una empresa inexistente llega al INSERT y la FK revienta como
        /// error 500 en vez de un 400 con un mensaje entendible.
        /// </summary>
        private void ExigirEmpresaExistente(int empresaId)
        {
            foreach (BEEmpresa oEmpresaBE in oBLLEmp.ListarTodo())
            {
                if (oEmpresaBE.EmpresaId == empresaId)
                {
                    return;
                }
            }

            throw new ExcepcionNegocio(
                TipoErrorNegocio.Validacion, "La empresa seleccionada no existe.");
        }

        private void ExigirSinEmpleados(BEDepartamento oDepartamentoBE)
        {
            if (oDepartamentoBE.Empleados > 0)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto,
                    "No se puede dar de baja un departamento con empleados asignados: " +
                    "primero reasignalos desde Usuarios.");
            }
        }

        private int? EmpresaQueLimita(BESesion oSesionBE)
        {
            if (oBLLSeg.Tiene(oSesionBE.UsuarioId, Permisos.DepartamentoVerTodasLasEmpresas))
            {
                return null;
            }

            BEUsuario oFiltroBE = new BEUsuario();
            oFiltroBE.UsuarioId = oSesionBE.UsuarioId;

            BEUsuario oUsuarioBE = oMPPUsu.ListarObjeto(oFiltroBE);

            if (oUsuarioBE == null)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.SesionInvalida, "El usuario de la sesión no existe.");
            }

            return oUsuarioBE.EmpresaId;
        }

        /// <summary>
        /// El nombre es único dentro de la empresa y solo entre los activos: un
        /// sector dado de baja no tiene por qué bloquear el nombre.
        /// </summary>
        private bool ExisteOtroConNombre(BEDepartamento Objeto)
        {
            BEDepartamento oFiltroBE = new BEDepartamento();
            oFiltroBE.EmpresaId = Objeto.EmpresaId;

            foreach (BEDepartamento oDepartamentoBE in ListarActivos(oFiltroBE))
            {
                if (oDepartamentoBE.DepartamentoId == Objeto.DepartamentoId)
                {
                    continue;
                }

                if (string.Equals(oDepartamentoBE.Nombre, Objeto.Nombre, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private void Validar(BEDepartamento Objeto)
        {
            Objeto.Nombre = Normalizar(Objeto.Nombre);
            Objeto.Descripcion = Normalizar(Objeto.Descripcion);

            if (string.IsNullOrEmpty(Objeto.Nombre))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "El nombre del departamento es obligatorio.");
            }

            if (Objeto.Nombre.Length > LongitudMaximaNombre)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El nombre no puede superar los " + LongitudMaximaNombre + " caracteres.");
            }

            if (Objeto.Descripcion != null && Objeto.Descripcion.Length > LongitudMaximaDescripcion)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "La descripción no puede superar los " + LongitudMaximaDescripcion + " caracteres.");
            }
        }

        private string Normalizar(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return null;
            }

            return valor.Trim();
        }
    }
}
