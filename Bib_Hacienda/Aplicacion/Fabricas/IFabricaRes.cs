using Bib_Hacienda.Dominio;

namespace Bib_Hacienda.Aplicacion
{
    public interface IFabricaRes
    {
        Res Crear(string nombre, uint peso, ushort edad);
    }
}
