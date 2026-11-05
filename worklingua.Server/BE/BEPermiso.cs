namespace worklingua.Server.BE
{
    public abstract class BEPermiso
    {
        public int PermisoId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool? Activo { get; set; }
        public bool EsCompuesto { get; set; }

        public abstract void AgregarPermiso(BEPermiso permiso);

        public abstract List<BEPermiso> ObtenerPermisosHijos();

        public abstract BEPermiso BuscarPermisoPorNombre(string nombre);
    }

    public class BEPermisoCompuesto : BEPermiso
    {
        public List<BEPermiso> Hijos { get; set; }

        public BEPermisoCompuesto()
        {
            this.EsCompuesto = true;
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

    public class BEPermisoSimple : BEPermiso
    {
        public BEPermisoSimple()
        {
            this.EsCompuesto = false;
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

    public class BEPermisoJerarquia
    {
        public int PermisoJerarquiaId { get; set; }
        public int PermisoPadreId { get; set; }
        public int PermisoHijoId { get; set; }

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

    public class BEPermisoRespuesta
    {
        public int PermisoId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool EsCompuesto { get; set; }
        public List<BEPermisoRespuesta> Hijos { get; set; } = new List<BEPermisoRespuesta>();
    }

    public class BEGuardarPermiso
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
    }

    public class BERol
    {
        public int RolId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool? Activo { get; set; }
    }

    public class BERolPermiso
    {
        public int RolPermisoId { get; set; }
        public int RolId { get; set; }
        public int PermisoId { get; set; }

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

    public class BERolRespuesta
    {
        public int RolId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }

        public BERolRespuesta()
        {
        }

        public BERolRespuesta(int rolId, string nombre, string descripcion)
        {
            this.RolId = rolId;
            this.Nombre = nombre;
            this.Descripcion = descripcion;
        }
    }

    public class BEGuardarRol
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
    }
}
