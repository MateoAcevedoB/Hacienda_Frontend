namespace Bib_Hacienda.Infraestructura
{
    public interface ISerializador<T>
    {
        string Serializar(T entidad);

        T Deserializar(string linea);
    }
}
