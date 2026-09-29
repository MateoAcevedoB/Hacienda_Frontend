using Bib_Hacienda.Dominio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bib_Hacienda.Interfaces
{
    public interface IAutenticacion
    {
        ResultadoOperacion AutorizarOperacion(Usuario usuario, string operacion);
    }
}
