using Bib_Hacienda.Dominio;
using System.Collections.Generic;

namespace Bib_Hacienda.Aplicacion
{
    // DIP (H-02, H-03, H-04, H-11, H-13): un repositorio por entidad. Los
    // servicios de aplicación dependen de esta abstracción; la implementación
    // concreta vive en Bib_Hacienda.Infraestructura (fuera de este bloque).
    public interface IRepositorioPotreros
    {
        List<Potrero> ObtenerTodos();
        Potrero ObtenerPorId(string identificacion);
        void Agregar(Potrero potrero);
        bool Existe(string identificacion);

        // FASE 2C: operación mínima necesaria para volver a persistir un
        // Potrero ya existente después de mutar su colección de reses en
        // memoria (p. ej. ServicioPotreros.AnadirResAPotrero ->
        // Potrero.AgregarRes). Antes de esta fase, IRepositorioPotreros no
        // tenía forma de reflejar esa mutación en el almacenamiento
        // (ver RepositorioPotrerosTexto/RepositorioPotrerosEfCore, "INCONSISTENCIA
        // REGISTRADA"). No sustituye a Agregar (que sigue siendo el alta de
        // un Potrero nuevo); Actualizar solo vuelve a guardar el estado de
        // L_reses de un Potrero que ya existe.
        void Actualizar(Potrero potrero);
    }
}
