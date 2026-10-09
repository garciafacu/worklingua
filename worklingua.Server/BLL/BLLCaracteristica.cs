using System.Collections.Generic;
using worklingua.Server.BE;
using worklingua.Server.MPP;

namespace worklingua.Server.BLL
{
    public class BLLCaracteristica
    {
        private const int LongitudMaximaNombre = 100;
        private const int OrdenMaximo = 100000;

        MPPCaracteristica oMPPCar;

        public BLLCaracteristica()
        {
            oMPPCar = new MPPCaracteristica();
        }

        public List<BECaracteristica> ListarTodo()
        {
            List<BECaracteristica> ListaCaracteristicaBE = oMPPCar.ListarTodo();

            return ListaCaracteristicaBE == null
                ? new List<BECaracteristica>()
                : ListaCaracteristicaBE;
        }

        public List<BECaracteristica> ListarTodoConBajas()
        {
            List<BECaracteristica> ListaCaracteristicaBE = oMPPCar.ListarTodoConBajas();

            return ListaCaracteristicaBE == null
                ? new List<BECaracteristica>()
                : ListaCaracteristicaBE;
        }

        public BECaracteristica ListarObjeto(BECaracteristica Objeto)
        {
            BECaracteristica oCaracteristicaBE = oMPPCar.ListarObjeto(Objeto);

            if (oCaracteristicaBE == null)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.NoEncontrado, "La característica no existe.");
            }

            return oCaracteristicaBE;
        }

        public BECaracteristica Guardar(BECaracteristica Objeto)
        {
            Validar(Objeto);

            if (Objeto.CaracteristicaId != 0)
            {
                ListarObjeto(Objeto);
            }

            ValidarNombreDisponible(Objeto);

            Objeto.CaracteristicaId = oMPPCar.Guardar(Objeto);

            return ListarObjeto(Objeto);
        }

        public bool Baja(BECaracteristica Objeto)
        {
            BECaracteristica oCaracteristicaBE = ListarObjeto(Objeto);

            if (!oCaracteristicaBE.Activo)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Conflicto, "La característica ya está dada de baja.");
            }

            return oMPPCar.Baja(Objeto);
        }

        private void Validar(BECaracteristica Objeto)
        {
            Objeto.Nombre = Normalizar(Objeto.Nombre);

            if (string.IsNullOrWhiteSpace(Objeto.Nombre))
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion, "El nombre de la característica es obligatorio.");
            }

            if (Objeto.Nombre.Length > LongitudMaximaNombre)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El nombre no puede superar los " + LongitudMaximaNombre + " caracteres.");
            }

            if (Objeto.Orden < 0 || Objeto.Orden > OrdenMaximo)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El orden debe estar entre 0 y " + OrdenMaximo + ".");
            }
        }

        private void ValidarNombreDisponible(BECaracteristica Objeto)
        {
            List<BECaracteristica> ListaCaracteristicaBE = ListarTodoConBajas();

            foreach (BECaracteristica oCaracteristicaBE in ListaCaracteristicaBE)
            {
                bool mismoNombre = string.Equals(
                    oCaracteristicaBE.Nombre.Trim(), Objeto.Nombre, StringComparison.OrdinalIgnoreCase);

                if (mismoNombre && oCaracteristicaBE.CaracteristicaId != Objeto.CaracteristicaId)
                {
                    throw new ExcepcionNegocio(
                        TipoErrorNegocio.Conflicto, "Ya existe una característica con ese nombre.");
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
