using worklingua.Server.BE;
using worklingua.Server.MPP;
using worklingua.Server.Services;

namespace worklingua.Server.BLL
{
    public class BLLEmpresa
    {
        private const int LongitudCuit = 11;
        private const int LongitudMaximaRazonSocial = 150;
        private const int LongitudMaximaEmail = 120;

        MPPEmpresa oMPPEmp = new MPPEmpresa();
        MPPUsuario oMPPUsu = new MPPUsuario();
        BLLSeguridad oBLLSeg = new BLLSeguridad();

        public List<BEEmpresa> ListarAdministracion(BESesion oSesionBE)
        {
            int? empresaQueLimita = EmpresaQueLimita(oSesionBE);

            if (!empresaQueLimita.HasValue)
            {
                return ListarTodo();
            }

            BEEmpresa oFiltroBE = new BEEmpresa();
            oFiltroBE.EmpresaId = empresaQueLimita.Value;

            return new List<BEEmpresa> { ListarObjeto(oFiltroBE) };
        }

        public BEEmpresa ListarObjeto(BEEmpresa Objeto, BESesion oSesionBE)
        {
            int? empresaQueLimita = EmpresaQueLimita(oSesionBE);

            if (empresaQueLimita.HasValue && Objeto.EmpresaId != empresaQueLimita.Value)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.NoEncontrado, "La empresa no existe.");
            }

            return ListarObjeto(Objeto);
        }

        public BEEmpresa Guardar(BEEmpresa Objeto, BESesion oSesionBE)
        {
            if (Objeto.EmpresaId == 0)
            {
                ExigirAlcanceTotal(oSesionBE, "Solo la plataforma puede dar de alta empresas.");
            }
            else
            {
                ListarObjeto(Objeto, oSesionBE);
            }

            return Guardar(Objeto);
        }

        public bool Baja(BEEmpresa Objeto, BESesion oSesionBE)
        {
            ExigirAlcanceTotal(oSesionBE, "Solo la plataforma puede dar de baja empresas.");

            return Baja(Objeto);
        }

        private int? EmpresaQueLimita(BESesion oSesionBE)
        {
            return oBLLSeg.EmpresaQueLimita(oSesionBE, Permisos.EmpresaVerTodasLasEmpresas);
        }

        private void ExigirAlcanceTotal(BESesion oSesionBE, string mensaje)
        {
            if (EmpresaQueLimita(oSesionBE).HasValue)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.PermisoDenegado, mensaje);
            }
        }

        public List<BEEmpresa> ListarTodo()
        {
            List<BEEmpresa> ListaEmpresaBE = oMPPEmp.ListarTodo();

            return ListaEmpresaBE == null ? new List<BEEmpresa>() : ListaEmpresaBE;
        }

        public List<BEEmpresa> ListarSeleccionables()
        {
            List<BEEmpresa> ListaEmpresaBE = oMPPEmp.ListarSeleccionables();

            return ListaEmpresaBE == null ? new List<BEEmpresa>() : ListaEmpresaBE;
        }

        public BEEmpresa ObtenerSeleccionable(BEEmpresa Objeto)
        {
            foreach (BEEmpresa oEmpresaBE in ListarSeleccionables())
            {
                if (oEmpresaBE.EmpresaId == Objeto.EmpresaId)
                {
                    return oEmpresaBE;
                }
            }

            throw new ExcepcionNegocio(
                TipoErrorNegocio.Validacion,
                "La empresa seleccionada no existe o no está disponible para asignar usuarios.");
        }

        public BEEmpresa ListarObjeto(BEEmpresa Objeto)
        {
            BEEmpresa oEmpresaBE = oMPPEmp.ListarObjeto(Objeto);

            if (oEmpresaBE == null)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.NoEncontrado, "La empresa no existe.");
            }

            return oEmpresaBE;
        }

        public BEEmpresa ObtenerPorCuit(string cuit)
        {
            if (string.IsNullOrWhiteSpace(cuit))
            {
                return null;
            }

            string buscado = cuit.Trim();
            List<BEEmpresa> ListaEmpresaBE = ListarTodo();

            foreach (BEEmpresa oEmpresaBE in ListaEmpresaBE)
            {
                if (oEmpresaBE.CUIT != null && oEmpresaBE.CUIT.Trim() == buscado)
                {
                    return oEmpresaBE;
                }
            }

            return null;
        }

        public BEEmpresa Guardar(BEEmpresa Objeto)
        {
            Validar(Objeto);

            if (Objeto.EmpresaId != 0)
            {
                ListarObjeto(Objeto);
            }

            if (ExisteOtraConCuitOEmail(Objeto))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto,
                    "Ya existe una empresa con ese CUIT o con ese correo.");
            }

            Objeto.EmpresaId = oMPPEmp.Guardar(Objeto);

            return ListarObjeto(Objeto);
        }

        public bool Baja(BEEmpresa Objeto)
        {
            BEEmpresa oEmpresaBE = ListarObjeto(Objeto);

            if (oEmpresaBE.Protegido)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto,
                    "La empresa " + oEmpresaBE.RazonSocial + " está protegida y no se puede dar de baja.");
            }

            if (!oEmpresaBE.Activo)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto, "La empresa ya está dada de baja.");
            }

            int usuariosActivos = ContarUsuariosActivos(oEmpresaBE.EmpresaId);

            if (usuariosActivos > 0)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto,
                    "La empresa tiene " + usuariosActivos + " usuario(s) activo(s). " +
                    "Desactivalos o movelos a otra empresa antes de darla de baja.");
            }

            return oMPPEmp.Baja(Objeto);
        }

        private int ContarUsuariosActivos(int empresaId)
        {
            BEUsuario oFiltroBE = new BEUsuario();
            oFiltroBE.EmpresaId = empresaId;

            List<BEUsuarioAdministracion> ListaUsuarioBE = oMPPUsu.ListarAdministracion(oFiltroBE);

            return ListaUsuarioBE == null ? 0 : ListaUsuarioBE.Count;
        }

        private bool ExisteOtraConCuitOEmail(BEEmpresa Objeto)
        {
            List<BEEmpresa> ListaEmpresaBE = ListarTodo();

            foreach (BEEmpresa oEmpresaBE in ListaEmpresaBE)
            {
                if (oEmpresaBE.EmpresaId == Objeto.EmpresaId)
                {
                    continue;
                }

                if (Coincide(oEmpresaBE.CUIT, Objeto.CUIT) ||
                    Coincide(oEmpresaBE.Email, Objeto.Email))
                {
                    return true;
                }
            }

            return false;
        }

        private bool Coincide(string valorExistente, string valorNuevo)
        {
            if (string.IsNullOrWhiteSpace(valorExistente) || string.IsNullOrWhiteSpace(valorNuevo))
            {
                return false;
            }

            return string.Equals(
                valorExistente.Trim(), valorNuevo.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        private void Validar(BEEmpresa Objeto)
        {
            Objeto.RazonSocial = ServicioTexto.Normalizar(Objeto.RazonSocial);
            Objeto.Email = ServicioTexto.Normalizar(Objeto.Email);
            Objeto.CUIT = ServicioTexto.Normalizar(Objeto.CUIT);
            Objeto.Telefono = ServicioTexto.Normalizar(Objeto.Telefono);
            Objeto.Direccion = ServicioTexto.Normalizar(Objeto.Direccion);
            Objeto.Ciudad = ServicioTexto.Normalizar(Objeto.Ciudad);
            Objeto.Provincia = ServicioTexto.Normalizar(Objeto.Provincia);
            Objeto.Pais = ServicioTexto.Normalizar(Objeto.Pais);

            ReglasNegocio.ExigirTexto(Objeto.RazonSocial, "La razón social es obligatoria.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.RazonSocial, LongitudMaximaRazonSocial,
                "La razón social no puede superar los " + LongitudMaximaRazonSocial + " caracteres.");
            ReglasNegocio.ExigirTexto(Objeto.Email, "El correo de la empresa es obligatorio.");

            if (Objeto.Email.Length > LongitudMaximaEmail || !Objeto.Email.Contains('@'))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "El correo de la empresa no tiene un formato válido.");
            }

            Objeto.Email = Objeto.Email.ToLowerInvariant();

            if (Objeto.EmpresaId == 0)
            {
                ValidarCuit(Objeto.CUIT);
            }
        }

        private void ValidarCuit(string cuit)
        {
            ReglasNegocio.ExigirTexto(cuit, "El CUIT es obligatorio.");

            if (cuit.Length != LongitudCuit)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El CUIT debe tener " + LongitudCuit + " dígitos, sin guiones.");
            }

            foreach (char caracter in cuit)
            {
                if (!char.IsDigit(caracter))
                {
                    throw new ExcepcionNegocio(
                        TipoErrorNegocio.Validacion, "El CUIT solo admite dígitos, sin guiones.");
                }
            }
        }

        public void Validar(BEGuardarEmpresa Objeto)
        {
            ReglasNegocio.ExigirTexto(Objeto.RazonSocial, "La razón social es obligatoria.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.RazonSocial, 150,
                "La razón social no puede superar los 150 caracteres.");

            ReglasNegocio.ExigirTexto(Objeto.CUIT, "El CUIT es obligatorio.");
            ReglasNegocio.ExigirLargo(Objeto.CUIT, 11, 11, "El CUIT debe tener 11 dígitos, sin guiones.");

            ReglasNegocio.ExigirTexto(Objeto.Email, "El correo de la empresa es obligatorio.");
            ReglasNegocio.ExigirEmail(Objeto.Email, "El correo no tiene un formato válido.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Email, 120, "El correo no puede superar los 120 caracteres.");

            ReglasNegocio.ExigirLargoMaximo(Objeto.Telefono, 30, "El teléfono no puede superar los 30 caracteres.");

            ReglasNegocio.ExigirLargoMaximo(Objeto.Direccion, 250, "La dirección no puede superar los 250 caracteres.");

            ReglasNegocio.ExigirLargoMaximo(Objeto.Ciudad, 80, "La ciudad no puede superar los 80 caracteres.");

            ReglasNegocio.ExigirLargoMaximo(Objeto.Provincia, 80, "La provincia no puede superar los 80 caracteres.");

            ReglasNegocio.ExigirLargoMaximo(Objeto.Pais, 80, "El país no puede superar los 80 caracteres.");
        }
    }

    public class BLLDepartamento
    {
        private const int LongitudMaximaNombre = 100;
        private const int LongitudMaximaDescripcion = 250;

        MPPDepartamento oMPPDep = new MPPDepartamento();
        BLLSeguridad oBLLSeg = new BLLSeguridad();
        BLLEmpresa oBLLEmp = new BLLEmpresa();

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
            return oBLLSeg.EmpresaQueLimita(oSesionBE, Permisos.DepartamentoVerTodasLasEmpresas);
        }

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
            Objeto.Nombre = ServicioTexto.Normalizar(Objeto.Nombre);
            Objeto.Descripcion = ServicioTexto.Normalizar(Objeto.Descripcion);

            if (string.IsNullOrEmpty(Objeto.Nombre))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "El nombre del departamento es obligatorio.");
            }

            ReglasNegocio.ExigirLargoMaximo(Objeto.Nombre, LongitudMaximaNombre,
                "El nombre no puede superar los " + LongitudMaximaNombre + " caracteres.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Descripcion, LongitudMaximaDescripcion,
                "La descripción no puede superar los " + LongitudMaximaDescripcion + " caracteres.");
        }

        public void Validar(BEGuardarDepartamento Objeto)
        {
            ReglasNegocio.ExigirTexto(Objeto.Nombre, "El nombre del departamento es obligatorio.");
            ReglasNegocio.ExigirLargoMaximo(Objeto.Nombre, 100, "El nombre no puede superar los 100 caracteres.");

            ReglasNegocio.ExigirLargoMaximo(Objeto.Descripcion, 250,
                "La descripción no puede superar los 250 caracteres.");
        }
    }
}
