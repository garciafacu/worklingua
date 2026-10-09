using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using worklingua.Server.BE;

namespace worklingua.Server.Services
{
    /// <summary>
    /// Convierte el estado de un curso en un documento XML y vuelve a leerlo
    /// (CU-004-004 y punto 23 del Formulario de Avances).
    ///
    /// Es el único uso de XML del proyecto. Se eligió `XDocument` y no
    /// `XmlSerializer` porque el documento es un contrato que se guarda en la
    /// base y se lee años después: acá se ve exactamente qué se escribe y qué
    /// se espera, sin depender de cómo estén formadas las BE hoy.
    ///
    /// El documento guarda **nombres, no ids**: los ids de etiquetas, módulos y
    /// activos no significan nada al restaurar, porque esas filas pueden haber
    /// cambiado. El archivo de un activo sí viaja por su ruta, que es lo único
    /// que lo identifica en disco.
    /// </summary>
    public static class ServicioXml
    {
        /// <summary>Versión del formato, por si el documento cambia de forma.</summary>
        public const string FormatoActual = "1";

        public static string Armar(BESnapshotCurso Objeto)
        {
            XElement raiz = new XElement("curso",
                new XAttribute("formato", FormatoActual),
                new XAttribute("nombre", Texto(Objeto.Curso.Nombre)),
                new XAttribute("idioma", Texto(Objeto.Curso.Idioma)),
                new XAttribute("nivel", Texto(Objeto.Curso.Nivel)));

            Agregar(raiz, "sector", Objeto.Curso.Sector);
            Agregar(raiz, "duracionHoras", Numero(Objeto.Curso.DuracionHoras));
            Agregar(raiz, "fechaPublicacion", Fecha(Objeto.Curso.FechaPublicacion));
            Agregar(raiz, "fechaFin", Fecha(Objeto.Curso.FechaFin));

            raiz.Add(new XElement("descripcion", Texto(Objeto.Curso.Descripcion)));

            XElement etiquetas = new XElement("etiquetas");

            foreach (BEEtiqueta oEtiquetaBE in Objeto.Curso.Etiquetas)
            {
                etiquetas.Add(new XElement("etiqueta", Texto(oEtiquetaBE.Nombre)));
            }

            raiz.Add(etiquetas);

            XElement modulos = new XElement("modulos");

            foreach (BEModulo oModuloBE in Objeto.Modulos)
            {
                modulos.Add(new XElement("modulo",
                    new XAttribute("nombre", Texto(oModuloBE.Nombre)),
                    new XAttribute("orden", oModuloBE.OrdenModulo),
                    new XElement("descripcion", Texto(oModuloBE.Descripcion)),
                    new XElement("contenido", Texto(oModuloBE.Contenido))));
            }

            raiz.Add(modulos);

            XElement activos = new XElement("activos");

            foreach (BEActivoPedagogico oActivoBE in Objeto.Activos)
            {
                XElement activo = new XElement("activo",
                    new XAttribute("nombre", Texto(oActivoBE.Nombre)),
                    new XAttribute("tipo", Texto(oActivoBE.TipoContenido)),
                    new XAttribute("archivo", Texto(oActivoBE.UrlArchivo)),
                    new XAttribute("estado", Texto(oActivoBE.Estado)),
                    new XElement("descripcion", Texto(oActivoBE.Descripcion)));

                // El módulo viaja por nombre, como todo lo demás: al restaurar,
                // el módulo del snapshot puede ser una fila distinta.
                Agregar(activo, "modulo", oActivoBE.Modulo);

                activos.Add(activo);
            }

            raiz.Add(activos);

            return new XDocument(new XDeclaration("1.0", "utf-8", null), raiz).ToString();
        }

        /// <summary>
        /// Lee el documento. Un XML que no se puede parsear es un error de
        /// negocio, no una excepción cruda: la versión está corrupta.
        /// </summary>
        public static BESnapshotCurso Leer(string xml)
        {
            XDocument documento;

            try
            {
                documento = XDocument.Parse(xml);
            }
            catch (System.Xml.XmlException)
            {
                return null;
            }

            XElement raiz = documento.Root;

            if (raiz == null || raiz.Name != "curso")
            {
                return null;
            }

            BESnapshotCurso Objeto = new BESnapshotCurso();

            Objeto.Curso.Nombre = Atributo(raiz, "nombre");
            Objeto.Curso.Idioma = Atributo(raiz, "idioma");
            Objeto.Curso.Nivel = Atributo(raiz, "nivel");
            Objeto.Curso.Sector = Atributo(raiz, "sector");
            Objeto.Curso.DuracionHoras = AEntero(Atributo(raiz, "duracionHoras"));
            Objeto.Curso.FechaPublicacion = AFecha(Atributo(raiz, "fechaPublicacion"));
            Objeto.Curso.FechaFin = AFecha(Atributo(raiz, "fechaFin"));
            Objeto.Curso.Descripcion = Elemento(raiz, "descripcion");

            foreach (XElement etiqueta in Hijos(raiz, "etiquetas", "etiqueta"))
            {
                BEEtiqueta oEtiquetaBE = new BEEtiqueta();
                oEtiquetaBE.Nombre = etiqueta.Value;

                Objeto.Curso.Etiquetas.Add(oEtiquetaBE);
            }

            foreach (XElement modulo in Hijos(raiz, "modulos", "modulo"))
            {
                BEModulo oModuloBE = new BEModulo();

                oModuloBE.Nombre = Atributo(modulo, "nombre");
                oModuloBE.OrdenModulo = AEntero(Atributo(modulo, "orden")) ?? 1;
                oModuloBE.Descripcion = Elemento(modulo, "descripcion");
                oModuloBE.Contenido = Elemento(modulo, "contenido");

                Objeto.Modulos.Add(oModuloBE);
            }

            foreach (XElement activo in Hijos(raiz, "activos", "activo"))
            {
                BEActivoPedagogico oActivoBE = new BEActivoPedagogico();

                oActivoBE.Nombre = Atributo(activo, "nombre");
                oActivoBE.TipoContenido = Atributo(activo, "tipo");
                oActivoBE.UrlArchivo = Atributo(activo, "archivo");
                oActivoBE.Estado = Atributo(activo, "estado");
                oActivoBE.Modulo = Atributo(activo, "modulo");
                oActivoBE.Descripcion = Elemento(activo, "descripcion");

                Objeto.Activos.Add(oActivoBE);
            }

            return Objeto;
        }

        private static void Agregar(XElement raiz, string nombre, string valor)
        {
            if (!string.IsNullOrEmpty(valor))
            {
                raiz.Add(new XAttribute(nombre, valor));
            }
        }

        private static IEnumerable<XElement> Hijos(XElement raiz, string contenedor, string hijo)
        {
            XElement elemento = raiz.Element(contenedor);

            return elemento == null ? new List<XElement>() : elemento.Elements(hijo);
        }

        private static string Atributo(XElement elemento, string nombre)
        {
            XAttribute atributo = elemento.Attribute(nombre);

            return atributo == null || atributo.Value.Length == 0 ? null : atributo.Value;
        }

        private static string Elemento(XElement raiz, string nombre)
        {
            XElement elemento = raiz.Element(nombre);

            return elemento == null || elemento.Value.Length == 0 ? null : elemento.Value;
        }

        private static string Texto(string valor)
        {
            return valor ?? string.Empty;
        }

        private static string Numero(int? valor)
        {
            return valor.HasValue ? valor.Value.ToString(CultureInfo.InvariantCulture) : null;
        }

        private static string Fecha(DateOnly? valor)
        {
            return valor.HasValue ? valor.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : null;
        }

        private static int? AEntero(string valor)
        {
            int numero;

            return int.TryParse(valor, NumberStyles.Integer, CultureInfo.InvariantCulture, out numero)
                ? numero
                : (int?)null;
        }

        private static DateOnly? AFecha(string valor)
        {
            DateOnly fecha;

            return DateOnly.TryParseExact(valor, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out fecha)
                ? fecha
                : (DateOnly?)null;
        }
    }
}
