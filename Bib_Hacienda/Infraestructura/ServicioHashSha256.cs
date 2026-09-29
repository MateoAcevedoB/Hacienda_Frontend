using System;
using System.Security.Cryptography;
using System.Text;
using Bib_Hacienda.Aplicacion;

namespace Bib_Hacienda.Infraestructura
{
    public class ServicioHashSha256 : IServicioHash
    {
        public string Hash(string textoPlano)
        {
            if (textoPlano == null)
            {
                throw new ArgumentNullException(nameof(textoPlano));
            }

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytesHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(textoPlano));
                StringBuilder sb = new StringBuilder(bytesHash.Length * 2);
                foreach (byte b in bytesHash)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        public bool Verificar(string textoPlano, string hash)
        {
            if (textoPlano == null || hash == null)
            {
                return false;
            }

            string hashCalculado = Hash(textoPlano);
            return string.Equals(hashCalculado, hash, StringComparison.OrdinalIgnoreCase);
        }
    }
}
