using System.Collections.Generic;
using worklingua.Server.BE;
using worklingua.Server.MPP;

namespace worklingua.Server.BLL
{
    public class BLLModulo
    {
        private const int LongitudMaximaNombre = 150;
        private const int LongitudMaximaDescripcion = 500;
        private const int LongitudMaximaContenido = 8000;

        MPPModulo oMPPMod;
        BLLCurso oBLLCur;
        BLLBitacora oBLLBit;

        public BLLModulo()
        {
            oMPPMod = new MPPModulo();
            oBLLCur = new BLLCurso();
            oBLLBit = new BLLBitacora();
        }

        public List<BEModulo> ListarPorCurso(BECurso Objeto, BESesion oSesionBE)
        {
            oBLLCur.ListarObjeto(Objeto, oSesionBE);

            List<BEModulo> ListaModuloBE = oMPPMod.ListarPorCurso(Objeto);

            return ListaModuloBE == null ? new List<BEModulo>() : ListaModuloBE;
        }

        public BEModulo Guardar(BEModulo Objeto, BESesion oSesionBE)
        {
            Objeto.Nombre = Normalizar(Objeto.Nombre);
            Objeto.Descripcion = Normalizar(Objeto.Descripcion);
            Objeto.Contenido = Normalizar(Objeto.Contenido);

            if (Objeto.Nombre == null || Objeto.Nombre.Length > LongitudMaximaNombre)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El nombre del módulo es obligatorio y no puede superar los " + LongitudMaximaNombre + " caracteres.");
            }

            if (Objeto.Descripcion != null && Objeto.Descripcion.Length > LongitudMaximaDescripcion)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El objetivo no puede superar los " + LongitudMaximaDescripcion + " caracteres.");
            }

            if (Objeto.Contenido != null && Objeto.Contenido.Length > LongitudMaximaContenido)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.Validacion,
                    "El contenido no puede superar los " + LongitudMaximaContenido + " caracteres.");
            }

            if (Objeto.OrdenModulo < 1)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.Validacion, "El orden del módulo tiene que ser mayor que cero.");
            }

            BECurso oFiltroBE = new BECurso();
            oFiltroBE.CursoId = Objeto.CursoId;

            BECurso oCursoBE = oBLLCur.ExigirGestion(oFiltroBE, oSesionBE);

            if (!oCursoBE.Activo)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.Conflicto, "El curso está dado de baja.");
            }

            foreach (BEModulo oModuloBE in ListarPorCurso(oFiltroBE, oSesionBE))
            {
                if (oModuloBE.OrdenModulo == Objeto.OrdenModulo)
                {
                    throw new ExcepcionNegocio(
                        TipoErrorNegocio.Conflicto,
                        "Ya hay un módulo con el orden " + Objeto.OrdenModulo + " en este curso.");
                }
            }

            Objeto.ModuloId = oMPPMod.Guardar(Objeto);
            Objeto.Activo = true;

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloCurso,
                "ModuloAlta",
                "Módulo " + Objeto.ModuloId + " (" + Objeto.Nombre + ") en el curso " + oCursoBE.Nombre + ".",
                BLLBitacora.NivelInformativo));

            return Objeto;
        }

        public bool Baja(BEModulo Objeto, BESesion oSesionBE)
        {
            BECurso oFiltroBE = new BECurso();
            oFiltroBE.CursoId = Objeto.CursoId;

            oBLLCur.ExigirGestion(oFiltroBE, oSesionBE);

            BEModulo oModuloBE = null;

            foreach (BEModulo oItemBE in ListarPorCurso(oFiltroBE, oSesionBE))
            {
                if (oItemBE.ModuloId == Objeto.ModuloId)
                {
                    oModuloBE = oItemBE;
                }
            }

            if (oModuloBE == null)
            {
                throw new ExcepcionNegocio(TipoErrorNegocio.NoEncontrado, "El módulo no existe o ya está dado de baja.");
            }

            bool resultado = oMPPMod.Baja(oModuloBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloCurso,
                "ModuloBaja",
                "Baja del módulo " + oModuloBE.ModuloId + " (" + oModuloBE.Nombre + ") del curso " + oModuloBE.CursoId + ".",
                BLLBitacora.NivelInformativo));

            return resultado;
        }

        private string Normalizar(string texto)
        {
            return string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
        }
    }
}
