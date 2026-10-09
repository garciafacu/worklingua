using System.Collections.Generic;

namespace worklingua.Server.BE
{
    public abstract class BEPermiso
    {
        #region Propiedades
        public int PermisoId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool? Activo { get; set; }
        public bool EsCompuesto { get; set; }
        #endregion

        protected BEPermiso()
        {

        }

        protected BEPermiso(int permisoId, string nombre, string descripcion, bool? activo, bool esCompuesto)
        {
            this.PermisoId = permisoId;
            this.Nombre = nombre;
            this.Descripcion = descripcion;
            this.Activo = activo;
            this.EsCompuesto = esCompuesto;
        }

        public abstract void AgregarPermiso(BEPermiso permiso);

        public abstract List<BEPermiso> ObtenerPermisosHijos();

        public abstract BEPermiso BuscarPermisoPorNombre(string nombre);
    }
}
