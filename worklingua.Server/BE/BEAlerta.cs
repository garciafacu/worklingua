namespace worklingua.Server.BE
{
    /// <summary>
    /// Una regla de alerta preventiva de deserción (CU-001-010).
    ///
    /// La regla se guarda y se emite; desactivarla detiene los envíos sin
    /// borrar el histórico. No está en el diagrama entidad-relación del PDF,
    /// pero sí en el diagrama de clases parcial del caso de uso.
    /// </summary>
    public class BEAlerta
    {
        #region Propiedades
        public int AlertaId { get; set; }
        public int EmpresaId { get; set; }
        public string Empresa { get; set; }
        /// <summary>Null significa toda la empresa.</summary>
        public int? DepartamentoId { get; set; }
        public string Departamento { get; set; }
        public string Titulo { get; set; }
        public string Mensaje { get; set; }
        public int DiasInactividad { get; set; }
        public bool? Activo { get; set; }
        public DateTime? FechaAlta { get; set; }
        /// <summary>Cuándo se emitió por última vez; null si todavía no se emitió.</summary>
        public DateTime? UltimaEmision { get; set; }
        /// <summary>A cuántos empleados alcanzó la última emisión.</summary>
        public int Destinatarios { get; set; }
        public int UsuarioId { get; set; }
        #endregion

        public BEAlerta()
        {

        }

        public BEAlerta(
            int alertaId,
            int empresaId,
            string empresa,
            int? departamentoId,
            string departamento,
            string titulo,
            string mensaje,
            int diasInactividad,
            bool? activo,
            DateTime? fechaAlta,
            DateTime? ultimaEmision,
            int destinatarios,
            int usuarioId)
        {
            this.AlertaId = alertaId;
            this.EmpresaId = empresaId;
            this.Empresa = empresa;
            this.DepartamentoId = departamentoId;
            this.Departamento = departamento;
            this.Titulo = titulo;
            this.Mensaje = mensaje;
            this.DiasInactividad = diasInactividad;
            this.Activo = activo;
            this.FechaAlta = fechaAlta;
            this.UltimaEmision = ultimaEmision;
            this.Destinatarios = destinatarios;
            this.UsuarioId = usuarioId;
        }
    }
}
