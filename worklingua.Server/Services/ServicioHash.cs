using BCryptNet = BCrypt.Net.BCrypt;

namespace worklingua.Server.Services
{
    public class ServicioHash
    {
        private const int FactorTrabajo = 12;

        public string Hashear(string clave)
        {
            if (string.IsNullOrWhiteSpace(clave))
            {
                throw new ArgumentException("La clave no puede estar vacía.", nameof(clave));
            }

            return BCryptNet.HashPassword(clave, FactorTrabajo);
        }

        public bool Verificar(string clave, string hashAlmacenado)
        {
            if (string.IsNullOrWhiteSpace(clave) || string.IsNullOrWhiteSpace(hashAlmacenado))
            {
                return false;
            }

            try
            {
                return BCryptNet.Verify(clave, hashAlmacenado);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
