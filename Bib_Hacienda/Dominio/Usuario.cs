using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bib_Hacienda.Dominio
{
    public class Usuario
    {
        private string nombre;
        private string contrasenaHash;

        public Usuario(string nombre, string contrasenaHash)
        {
            this.Nombre = nombre;
            this.ContrasenaHash = contrasenaHash;
        }

        public string Nombre { get => nombre; set => nombre = value; }
        public string ContrasenaHash { get => contrasenaHash; set => contrasenaHash = value; }
    }
}
