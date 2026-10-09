namespace worklingua.Server.BE
{
    public class BEUsuarioRol
    {
        #region Propiedades
        public int UsuarioRolId { get; set; }
        public int UsuarioId { get; set; }
        public int RolId { get; set; }
        public DateTime? FechaAsignacion { get; set; }
        #endregion

        public BEUsuarioRol()
        {

        }

        public BEUsuarioRol(int usuarioRolId, int usuarioId, int rolId, DateTime? fechaAsignacion)
        {
            this.UsuarioRolId = usuarioRolId;
            this.UsuarioId = usuarioId;
            this.RolId = rolId;
            this.FechaAsignacion = fechaAsignacion;
        }
    }
}
