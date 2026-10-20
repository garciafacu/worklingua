namespace worklingua.Server.BE
{
    public class BEAlerta
    {
        #region Propiedades
        public int AlertaId { get; set; }
        public int EmpresaId { get; set; }
        public string Empresa { get; set; }
        public int? DepartamentoId { get; set; }
        public string Departamento { get; set; }
        public string Titulo { get; set; }
        public string Mensaje { get; set; }
        public int DiasInactividad { get; set; }
        public bool? Activo { get; set; }
        public DateTime? FechaAlta { get; set; }
        public DateTime? UltimaEmision { get; set; }
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
