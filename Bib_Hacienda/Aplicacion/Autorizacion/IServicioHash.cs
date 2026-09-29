namespace Bib_Hacienda.Aplicacion
{
    public interface IServicioHash
    {
        string Hash(string textoPlano);
        bool Verificar(string textoPlano, string hash);
    }
}
