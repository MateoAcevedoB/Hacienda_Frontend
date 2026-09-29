using Bib_Hacienda.Dominio;
using System.Collections.Generic;

namespace Bib_Hacienda.Aplicacion
{
    public interface IRepositorioUsuarios
    {
        List<Usuario> ObtenerTodos();
        Usuario BuscarPorNombre(string nombre);
        void Agregar(Usuario usuario);
    }
}
