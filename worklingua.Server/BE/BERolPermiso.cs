namespace worklingua.Server.BE
{
    public class BERolPermiso
    {
        #region Propiedades
        public int RolPermisoId { get; set; }
        public int RolId { get; set; }
        public int PermisoId { get; set; }
        #endregion

        public BERolPermiso()
        {

        }

        public BERolPermiso(int rolPermisoId, int rolId, int permisoId)
        {
            this.RolPermisoId = rolPermisoId;
            this.RolId = rolId;
            this.PermisoId = permisoId;
        }
    }
}
