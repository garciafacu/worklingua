using System.Collections.Generic;

namespace worklingua.Server.BE
{
    public class BEPermisoSimple : BEPermiso
    {
        public BEPermisoSimple()
        {
            this.EsCompuesto = false;
        }

        public BEPermisoSimple(int permisoId, string nombre, string descripcion, bool? activo)
            : base(permisoId, nombre, descripcion, activo, false)
        {

        }

        public override void AgregarPermiso(BEPermiso permiso)
        {
            throw new InvalidOperationException("Un permiso simple no puede contener otros permisos.");
        }

        public override List<BEPermiso> ObtenerPermisosHijos()
        {
            return new List<BEPermiso>();
        }

        public override BEPermiso BuscarPermisoPorNombre(string nombre)
        {
            if (string.Equals(this.Nombre, nombre, StringComparison.OrdinalIgnoreCase))
            {
                return this;
            }

            return null;
        }
    }
}
