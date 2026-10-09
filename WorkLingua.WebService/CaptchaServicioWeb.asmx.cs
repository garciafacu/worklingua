using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Script.Serialization;
using System.Web.Services;

namespace WorkLingua.WebService
{
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    public class CaptchaServicioWeb : System.Web.Services.WebService
    {

        [WebMethod]
        public bool ValidarCaptcha(string token, string claveSecreta)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string url = "https://www.google.com/recaptcha/api/siteverify";

                    // Limpiar encabezados previos
                    client.DefaultRequestHeaders.Clear();

                    // Google espera los datos como formulario, no como JSON
                    Dictionary<string, string> datos = new Dictionary<string, string>();
                    datos.Add("secret", claveSecreta);
                    datos.Add("response", token);

                    FormUrlEncodedContent contenido = new FormUrlEncodedContent(datos);

                    // Llamada sincrónica al API
                    HttpResponseMessage response = client.PostAsync(url, contenido).Result; // Aquí estamos bloqueando el hilo

                    // Verificar si la respuesta es exitosa
                    if (response.IsSuccessStatusCode)
                    {
                        string res = response.Content.ReadAsStringAsync().Result; // Bloquea nuevamente el hilo

                        // La respuesta es un objeto JSON: { "success": true/false, ... }
                        JavaScriptSerializer serializador = new JavaScriptSerializer();
                        Dictionary<string, object> json = serializador.Deserialize<Dictionary<string, object>>(res);

                        // Retornar si Google dio por válido el token
                        return (bool)json["success"];
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
