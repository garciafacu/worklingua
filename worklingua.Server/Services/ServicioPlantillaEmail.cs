using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Text.Unicode;

namespace worklingua.Server.Services
{
    public class ServicioPlantillaEmail
    {
        private const string PlantillaBase = "base";

        private static readonly HtmlEncoder Codificador =
            HtmlEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement);

        public string Renderizar(string nombrePlantilla, Dictionary<string, string> valores)
        {
            return Renderizar(nombrePlantilla, valores, new Dictionary<string, string>());
        }

        public string Renderizar(
            string nombrePlantilla,
            Dictionary<string, string> valores,
            Dictionary<string, string> valoresHtml)
        {
            Dictionary<string, string> codificados = Codificar(valores);

            foreach (KeyValuePair<string, string> par in valoresHtml)
            {
                codificados[par.Key] = par.Value == null ? string.Empty : par.Value;
            }

            string cuerpo = Sustituir(LeerPlantilla(nombrePlantilla), codificados);

            Dictionary<string, string> valoresLayout = new Dictionary<string, string>(codificados);
            valoresLayout["Cuerpo"] = cuerpo;
            valoresLayout["Anio"] = DateTime.Now.Year.ToString();

            return Sustituir(LeerPlantilla(PlantillaBase), valoresLayout);
        }

        public string RenderizarFragmento(string nombrePlantilla, Dictionary<string, string> valores)
        {
            return Sustituir(LeerPlantilla(nombrePlantilla), Codificar(valores));
        }

        private Dictionary<string, string> Codificar(Dictionary<string, string> valores)
        {
            Dictionary<string, string> codificados = new Dictionary<string, string>();

            foreach (KeyValuePair<string, string> par in valores)
            {
                codificados.Add(par.Key, Codificador.Encode(par.Value == null ? string.Empty : par.Value));
            }

            return codificados;
        }

        private string LeerPlantilla(string nombre)
        {
            string ruta = Path.Combine(
                Configuracion.RutaContenido, "Services", "Plantillas", nombre + ".html");

            return Regex.Replace(File.ReadAllText(ruta, Encoding.UTF8), @"<!--.*?-->\s*", string.Empty, RegexOptions.Singleline);
        }

        private string Sustituir(string plantilla, Dictionary<string, string> valores)
        {
            StringBuilder resultado = new StringBuilder(plantilla);

            foreach (KeyValuePair<string, string> par in valores)
            {
                resultado.Replace("{{" + par.Key + "}}", par.Value);
            }

            return Regex.Replace(resultado.ToString(), @"\{\{\s*\w+\s*\}\}", string.Empty);
        }
    }
}
