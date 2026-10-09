using System.Collections.Generic;

namespace worklingua.Server.BE
{
    public class BEPermisoRespuesta
    {
        #region Propiedades
        public int PermisoId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool EsCompuesto { get; set; }
        public List<BEPermisoRespuesta> Hijos { get; set; }
        #endregion

        public BEPermisoRespuesta()
        {
            this.Hijos = new List<BEPermisoRespuesta>();
        }

        public BEPermisoRespuesta(int permisoId, string nombre, string descripcion, bool esCompuesto)
        {
            this.PermisoId = permisoId;
            this.Nombre = nombre;
            this.Descripcion = descripcion;
            this.EsCompuesto = esCompuesto;
            this.Hijos = new List<BEPermisoRespuesta>();
        }
    }
}
