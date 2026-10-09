namespace worklingua.Server.BE
{
    public class BEPermisoJerarquia
    {
        #region Propiedades
        public int PermisoJerarquiaId { get; set; }
        public int PermisoPadreId { get; set; }
        public int PermisoHijoId { get; set; }
        #endregion

        public BEPermisoJerarquia()
        {

        }

        public BEPermisoJerarquia(int permisoJerarquiaId, int permisoPadreId, int permisoHijoId)
        {
            this.PermisoJerarquiaId = permisoJerarquiaId;
            this.PermisoPadreId = permisoPadreId;
            this.PermisoHijoId = permisoHijoId;
        }
    }
}
