using System.Security.Cryptography;
using System.Text;

namespace worklingua.Server.Services
{
    public class ServicioCifrado
    {
        public const int LongitudClaveBytes = 32;

        private const string Prefijo = "aes";
        private const int LongitudVectorBytes = 16;
        private const char Separador = '$';

        public string Cifrar(string texto)
        {
            if (string.IsNullOrEmpty(texto))
            {
                return texto;
            }

            byte[] vector = RandomNumberGenerator.GetBytes(LongitudVectorBytes);

            using (Aes algoritmo = Aes.Create())
            {
                algoritmo.Key = ObtenerClave();

                byte[] cifrado = algoritmo.EncryptCbc(Encoding.UTF8.GetBytes(texto), vector, PaddingMode.PKCS7);

                return string.Join(Separador, Prefijo, Convert.ToBase64String(vector), Convert.ToBase64String(cifrado));
            }
        }

        /// <summary>
        /// Huella determinística de un dato cifrado, para poder exigir unicidad
        /// sobre él. <see cref="Cifrar"/> usa un vector de inicialización
        /// aleatorio, así que el mismo texto da resultados distintos y no se
        /// puede comparar ni indexar en SQL: esta huella sí.
        ///
        /// Es HMAC y no un hash pelado porque el dato que protege es corto (un
        /// documento son pocos dígitos) y sin clave se recuperaría probando
        /// todas las combinaciones contra una copia de la base.
        /// </summary>
        public byte[] HashDeterministico(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return null;
            }

            using (HMACSHA256 algoritmo = new HMACSHA256(ObtenerClave()))
            {
                return algoritmo.ComputeHash(Encoding.UTF8.GetBytes(Normalizar(texto)));
            }
        }

        private string Normalizar(string texto)
        {
            return texto.Trim().ToUpperInvariant();
        }

        public string Descifrar(string valorAlmacenado)
        {
            if (string.IsNullOrEmpty(valorAlmacenado))
            {
                return valorAlmacenado;
            }

            string[] partes = valorAlmacenado.Split(Separador);

            if (partes.Length != 3 || partes[0] != Prefijo)
            {
                return valorAlmacenado;
            }

            byte[] clave = ObtenerClave();

            try
            {
                byte[] vector = Convert.FromBase64String(partes[1]);
                byte[] cifrado = Convert.FromBase64String(partes[2]);

                using (Aes algoritmo = Aes.Create())
                {
                    algoritmo.Key = clave;

                    return Encoding.UTF8.GetString(algoritmo.DecryptCbc(cifrado, vector, PaddingMode.PKCS7));
                }
            }
            catch (Exception ex)
            {
                ServicioLog.Advertencia("No se pudo descifrar un valor almacenado. Revisar que 'Cifrado:Clave' sea la " + "misma con la que se cifró. Detalle: " + ex.Message);
                return valorAlmacenado;
            }
        }

        public static byte[] DecodificarClave(string claveBase64)
        {
            if (string.IsNullOrWhiteSpace(claveBase64))
            {
                return null;
            }

            try
            {
                byte[] clave = Convert.FromBase64String(claveBase64);

                return clave.Length == LongitudClaveBytes ? clave : null;
            }
            catch (FormatException)
            {
                return null;
            }
        }

        private byte[] ObtenerClave()
        {
            byte[] clave = DecodificarClave(Configuracion.CifradoClave);

            if (clave == null)
            {
                throw new InvalidOperationException("La clave de cifrado 'Cifrado:Clave' falta o no es una cadena Base64 de " + LongitudClaveBytes + " bytes.");
            }

            return clave;
        }
    }
}
