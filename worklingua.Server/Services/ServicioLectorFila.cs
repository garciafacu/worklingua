using System.Data;

namespace worklingua.Server.Services
{
    public static class ServicioLectorFila
    {
        public static bool TieneValor(DataRow fila, string columna)
        {
            return fila.Table.Columns.Contains(columna) && fila[columna] != DBNull.Value;
        }

        public static string LeerTexto(DataRow fila, string columna)
        {
            if (!TieneValor(fila, columna))
            {
                return string.Empty;
            }

            string valor = Convert.ToString(fila[columna]);

            return valor == null ? string.Empty : valor;
        }

        public static string LeerTextoNulo(DataRow fila, string columna)
        {
            return TieneValor(fila, columna) ? Convert.ToString(fila[columna]) : null;
        }

        public static int LeerEntero(DataRow fila, string columna)
        {
            return TieneValor(fila, columna) ? Convert.ToInt32(fila[columna]) : 0;
        }

        public static int? LeerEnteroNulo(DataRow fila, string columna)
        {
            return TieneValor(fila, columna) ? Convert.ToInt32(fila[columna]) : (int?)null;
        }

        public static decimal LeerDecimal(DataRow fila, string columna)
        {
            return TieneValor(fila, columna) ? Convert.ToDecimal(fila[columna]) : 0m;
        }

        public static bool LeerBooleano(DataRow fila, string columna)
        {
            return TieneValor(fila, columna) && Convert.ToBoolean(fila[columna]);
        }

        public static bool? LeerBooleanoNulo(DataRow fila, string columna)
        {
            return TieneValor(fila, columna) ? Convert.ToBoolean(fila[columna]) : (bool?)null;
        }

        public static DateTime LeerFechaHora(DataRow fila, string columna)
        {
            return TieneValor(fila, columna) ? Convert.ToDateTime(fila[columna]) : default(DateTime);
        }

        public static DateTime? LeerFechaHoraNula(DataRow fila, string columna)
        {
            return TieneValor(fila, columna) ? Convert.ToDateTime(fila[columna]) : (DateTime?)null;
        }

        public static DateOnly? LeerFechaNula(DataRow fila, string columna)
        {
            if (!TieneValor(fila, columna))
            {
                return null;
            }

            return DateOnly.FromDateTime(Convert.ToDateTime(fila[columna]));
        }

        public static Guid LeerGuid(DataRow fila, string columna)
        {
            return TieneValor(fila, columna) ? (Guid)fila[columna] : Guid.Empty;
        }

        public static int ATotalFilas(object escalar)
        {
            return escalar == null ? 0 : Convert.ToInt32(escalar);
        }
    }
}
