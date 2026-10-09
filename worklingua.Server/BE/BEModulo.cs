namespace worklingua.Server.BE
{
    /// <summary>
    /// Una unidad de un curso, que para el empleado es una lección.
    ///
    /// Descripcion y Contenido son cosas distintas: Descripcion es el objetivo
    /// —una línea de qué se va a lograr— y Contenido es la lección en sí, el
    /// texto que el empleado lee al abrir el módulo.
    /// </summary>
    public class BEModulo
    {
        #region Propiedades
        public int ModuloId { get; set; }
        public int CursoId { get; set; }
        public string Nombre { get; set; }
        /// <summary>El objetivo del módulo.</summary>
        public string Descripcion { get; set; }
        /// <summary>El texto de la lección.</summary>
        public string Contenido { get; set; }
        public int OrdenModulo { get; set; }
        public bool? Activo { get; set; }
        #endregion

        public BEModulo()
        {

        }

        public BEModulo(
            int moduloId,
            int cursoId,
            string nombre,
            string descripcion,
            string contenido,
            int ordenModulo,
            bool? activo)
        {
            this.ModuloId = moduloId;
            this.CursoId = cursoId;
            this.Nombre = nombre;
            this.Descripcion = descripcion;
            this.Contenido = contenido;
            this.OrdenModulo = ordenModulo;
            this.Activo = activo;
        }
    }
}
