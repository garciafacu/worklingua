using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using worklingua.Server.BE;

namespace worklingua.Server.Services
{
    public class ServicioValidacionTarjeta
    {
        private const string MarcaVisa = "VISA";
        private const string MarcaMastercard = "MASTERCARD";
        private const string MarcaAmex = "AMEX";

        private static readonly Dictionary<string, string> TarjetasRechazadas = new Dictionary<string, string>
        {
            { "4000000000000002", "La tarjeta fue rechazada por el emisor." },
            { "4000000000009995", "La tarjeta no tiene fondos suficientes." }
        };

        public BEResultadoValidacionTarjeta Validar(BETarjeta Objeto)
        {
            if (Objeto == null)
            {
                return Invalida("Completá los datos de la tarjeta.");
            }

            string numero = SoloDigitos(Objeto.Numero);

            if (numero.Length < 13 || numero.Length > 19 || !CumpleLuhn(numero))
            {
                return Invalida("El número de tarjeta no es válido.");
            }

            string marca = ResolverMarca(numero);

            if (marca == null)
            {
                return Invalida("Solo se aceptan tarjetas Visa, Mastercard o American Express.");
            }

            if (string.IsNullOrWhiteSpace(Objeto.Titular) || Objeto.Titular.Trim().Length < 3)
            {
                return Invalida("Completá el nombre del titular como figura en la tarjeta.");
            }

            if (!VencimientoVigente(Objeto.Vencimiento))
            {
                return Invalida("El vencimiento no es válido o la tarjeta está vencida.");
            }

            string codigo = Objeto.CodigoSeguridad == null ? string.Empty : Objeto.CodigoSeguridad.Trim();
            int largoCodigo = marca == MarcaAmex ? 4 : 3;

            if (codigo.Length != largoCodigo || SoloDigitos(codigo).Length != largoCodigo)
            {
                return Invalida("El código de seguridad tiene que tener " + largoCodigo + " dígitos.");
            }

            string ultimosDigitos = numero.Substring(numero.Length - 4);
            string motivoRechazo;

            if (TarjetasRechazadas.TryGetValue(numero, out motivoRechazo))
            {
                return new BEResultadoValidacionTarjeta(true, false, marca, ultimosDigitos, null, motivoRechazo);
            }

            return new BEResultadoValidacionTarjeta(true, true, marca, ultimosDigitos, GenerarCodigoAutorizacion(), null);
        }

        private BEResultadoValidacionTarjeta Invalida(string motivo)
        {
            return new BEResultadoValidacionTarjeta(false, false, null, null, null, motivo);
        }

        private string SoloDigitos(string texto)
        {
            if (string.IsNullOrEmpty(texto))
            {
                return string.Empty;
            }

            StringBuilder resultado = new StringBuilder();

            foreach (char caracter in texto)
            {
                if (char.IsDigit(caracter))
                {
                    resultado.Append(caracter);
                }
                else if (caracter != ' ' && caracter != '-')
                {
                    return string.Empty;
                }
            }

            return resultado.ToString();
        }

        private bool CumpleLuhn(string numero)
        {
            int suma = 0;
            bool duplicar = false;

            for (int indice = numero.Length - 1; indice >= 0; indice--)
            {
                int digito = numero[indice] - '0';

                if (duplicar)
                {
                    digito = digito * 2;

                    if (digito > 9)
                    {
                        digito = digito - 9;
                    }
                }

                suma = suma + digito;
                duplicar = !duplicar;
            }

            return suma % 10 == 0;
        }

        private string ResolverMarca(string numero)
        {
            if (numero.StartsWith("4"))
            {
                return MarcaVisa;
            }

            if ((numero.StartsWith("34") || numero.StartsWith("37")) && numero.Length == 15)
            {
                return MarcaAmex;
            }

            int prefijoDos = int.Parse(numero.Substring(0, 2));
            int prefijoCuatro = int.Parse(numero.Substring(0, 4));

            if ((prefijoDos >= 51 && prefijoDos <= 55) || (prefijoCuatro >= 2221 && prefijoCuatro <= 2720))
            {
                return MarcaMastercard;
            }

            return null;
        }

        private bool VencimientoVigente(string vencimiento)
        {
            if (string.IsNullOrWhiteSpace(vencimiento))
            {
                return false;
            }

            string[] partes = vencimiento.Trim().Split('/');
            int mes;
            int anio;

            if (partes.Length != 2
                || !int.TryParse(partes[0], out mes)
                || !int.TryParse(partes[1], out anio)
                || mes < 1
                || mes > 12)
            {
                return false;
            }

            if (partes[1].Length == 2)
            {
                anio = anio + 2000;
            }
            else if (partes[1].Length != 4)
            {
                return false;
            }

            DateTime primerDiaMesSiguiente = new DateTime(anio, mes, 1).AddMonths(1);

            return primerDiaMesSiguiente > DateTime.Today;
        }

        private string GenerarCodigoAutorizacion()
        {
            return "AUT-" + RandomNumberGenerator.GetInt32(100000, 1000000);
        }
    }
}
