using System.Collections.Generic;
using worklingua.Server.BE;
using worklingua.Server.MPP;

namespace worklingua.Server.BLL
{
    public class BLLEmpresa
    {
        private const int LongitudCuit = 11;
        private const int LongitudMaximaRazonSocial = 150;
        private const int LongitudMaximaEmail = 120;

        MPPEmpresa oMPPEmp;
        MPPUsuario oMPPUsu;
        BLLSeguridad oBLLSeg;

        public BLLEmpresa()
        {
            oMPPEmp = new MPPEmpresa();
            oMPPUsu = new MPPUsuario();
            oBLLSeg = new BLLSeguridad();
        }

        /// <summary>
        /// El ABM de Empresas. Sin Empresa.VerTodasLasEmpresas es "Mi empresa":
        /// solo la empresa del usuario de la sesión.
        /// </summary>
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

        /// <summary>Una empresa ajena se informa como inexistente.</summary>
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
            if (oBLLSeg.Tiene(oSesionBE.UsuarioId, Permisos.EmpresaVerTodasLasEmpresas))
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
            Objeto.RazonSocial = Normalizar(Objeto.RazonSocial);
            Objeto.Email = Normalizar(Objeto.Email);
            Objeto.CUIT = Normalizar(Objeto.CUIT);
            Objeto.Telefono = Normalizar(Objeto.Telefono);
            Objeto.Direccion = Normalizar(Objeto.Direccion);
            Objeto.Ciudad = Normalizar(Objeto.Ciudad);
            Objeto.Provincia = Normalizar(Objeto.Provincia);
            Objeto.Pais = Normalizar(Objeto.Pais);

            if (string.IsNullOrWhiteSpace(Objeto.RazonSocial))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "La razón social es obligatoria.");
            }

            if (Objeto.RazonSocial.Length > LongitudMaximaRazonSocial)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "La razón social no puede superar los " + LongitudMaximaRazonSocial + " caracteres.");
            }

            if (string.IsNullOrWhiteSpace(Objeto.Email))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "El correo de la empresa es obligatorio.");
            }

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
            if (string.IsNullOrWhiteSpace(cuit))
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.Validacion, "El CUIT es obligatorio.");
            }

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

        private string Normalizar(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return null;
            }

            return texto.Trim();
        }
    }
}
