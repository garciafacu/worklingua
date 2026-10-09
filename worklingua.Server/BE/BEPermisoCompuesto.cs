using System.Collections.Generic;

namespace worklingua.Server.BE
{
    public class BEPermisoCompuesto : BEPermiso
    {
        #region Propiedades
        public List<BEPermiso> Hijos { get; set; }
        #endregion

        public BEPermisoCompuesto()
        {
            this.EsCompuesto = true;
            this.Hijos = new List<BEPermiso>();
        }

        public BEPermisoCompuesto(int permisoId, string nombre, string descripcion, bool? activo)
            : base(permisoId, nombre, descripcion, activo, true)
        {
            this.Hijos = new List<BEPermiso>();
        }

        public override void AgregarPermiso(BEPermiso permiso)
        {
            this.Hijos.Add(permiso);
        }

        public override List<BEPermiso> ObtenerPermisosHijos()
        {
            return this.Hijos;
        }

        public override BEPermiso BuscarPermisoPorNombre(string nombre)
        {
            if (string.Equals(this.Nombre, nombre, StringComparison.OrdinalIgnoreCase))
            {
                return this;
            }

            foreach (BEPermiso oHijoBE in this.Hijos)
            {
                BEPermiso oEncontradoBE = oHijoBE.BuscarPermisoPorNombre(nombre);

                if (oEncontradoBE != null)
                {
                    return oEncontradoBE;
                }
            }

            return null;
        }
    }
}
