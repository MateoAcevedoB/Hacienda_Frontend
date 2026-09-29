using System;
using Bib_Hacienda.Dominio;
using Bib_Hacienda.Dominio.Eventos;
using Bib_Hacienda.Dominio.Validacion;
using Xunit;

namespace Bib_Hacienda.Tests.Validacion
{
    

    public class ValidadorResTests
    {
        private readonly ValidadorRes validador = new ValidadorRes();

        private static Res NuevaRes(string nombre, uint peso, ushort edad)
        {
            return new Ternero(nombre, peso, edad);
        }

        [Fact]
        public void EsValido_ResNula_DebeRetornarFalse()
        {
            Assert.False(validador.EsValido(null!));
        }

        [Fact]
        public void EsValido_NombreVacio_DebeRetornarFalse()
        {
            Res res = NuevaRes("   ", 200, 5);
            Assert.False(validador.EsValido(res));
        }

        [Fact]
        public void EsValido_NombreNulo_DebeRetornarFalse()
        {
            Res res = NuevaRes(null!, 200, 5);
            Assert.False(validador.EsValido(res));
        }

        [Fact]
        public void EsValido_PesoCero_DebeRetornarFalse()
        {
            Res res = NuevaRes("Vaca1", 0, 5);
            Assert.False(validador.EsValido(res));
        }

        [Fact]
        public void EsValido_EdadCero_DebeRetornarFalse()
        {
            Res res = NuevaRes("Vaca1", 200, 0);
            Assert.False(validador.EsValido(res));
        }

        [Fact]
        public void EsValido_DatosValidos_DebeRetornarTrue()
        {
            Res res = NuevaRes("Vaca1", 200, 5);
            Assert.True(validador.EsValido(res));
        }
    }

    public class ValidadorVacunaTests
    {
        private readonly ValidadorVacuna validador = new ValidadorVacuna();

        private static Vacuna NuevaVacuna(string nombre, string lote)
        {
            return new Bacteriana(nombre, lote, DateTime.Now.AddMonths(6), DateTime.Now, 3);
        }

        [Fact]
        public void EsValido_VacunaNula_DebeRetornarFalse()
        {
            Assert.False(validador.EsValido(null!));
        }

        [Fact]
        public void EsValido_NombreVacio_DebeRetornarFalse()
        {
            Assert.False(validador.EsValido(NuevaVacuna("   ", "L1")));
        }

        [Fact]
        public void EsValido_LoteVacio_DebeRetornarFalse()
        {
            Assert.False(validador.EsValido(NuevaVacuna("Aftosa", "   ")));
        }

        [Fact]
        public void EsValido_DatosValidos_DebeRetornarTrue()
        {
            Assert.True(validador.EsValido(NuevaVacuna("Aftosa", "L1")));
        }
    }

    public class ValidadorVentaTests
    {
        private readonly ValidadorVenta validador = new ValidadorVenta();

        private static Potrero NuevoPotrero(string id = "P1")
        {
            return new Potrero(id, "ternero", new PublisherPotreroMitad(), new PublisherPotreroLleno());
        }

        [Fact]
        public void EsValido_VentaNula_DebeRetornarFalse()
        {
            Assert.False(validador.EsValido(null!));
        }

        [Fact]
        public void EsValido_PotreroNulo_DebeRetornarFalse()
        {
            IActivoVendible activo = new Ternero("Vaca1", 200, 5);
            Venta venta = new Venta(null!, DateTime.Now, activo, 1000);
            Assert.False(validador.EsValido(venta));
        }

        [Fact]
        public void EsValido_ActivoNulo_DebeRetornarFalse()
        {
            Venta venta = new Venta(NuevoPotrero(), DateTime.Now, null!, 1000);
            Assert.False(validador.EsValido(venta));
        }

        [Fact]
        public void EsValido_MontoCero_DebeRetornarFalse()
        {
            IActivoVendible activo = new Ternero("Vaca1", 200, 5);
            Venta venta = new Venta(NuevoPotrero(), DateTime.Now, activo, 0);
            Assert.False(validador.EsValido(venta));
        }

        [Fact]
        public void EsValido_DatosValidos_DebeRetornarTrue()
        {
            IActivoVendible activo = new Ternero("Vaca1", 200, 5);
            Venta venta = new Venta(NuevoPotrero(), DateTime.Now, activo, 1000);
            Assert.True(validador.EsValido(venta));
        }
    }

    public class ValidadorPotreroTests
    {
        private readonly ValidadorPotrero validador = new ValidadorPotrero();

        [Fact]
        public void EsValido_PotreroNulo_DebeRetornarFalse()
        {
            Assert.False(validador.EsValido(null!));
        }

        [Fact]
        public void EsValido_IdentificacionVacia_DebeRetornarFalse()
        {
            Potrero potrero = new Potrero("   ", "ternero", new PublisherPotreroMitad(), new PublisherPotreroLleno());
            Assert.False(validador.EsValido(potrero));
        }

        [Fact]
        public void EsValido_DatosValidos_DebeRetornarTrue()
        {
            Potrero potrero = new Potrero("Potrero1", "ternero", new PublisherPotreroMitad(), new PublisherPotreroLleno());
            Assert.True(validador.EsValido(potrero));
        }
    }

    public class ValidadorEventoClinicoTests
    {
        private readonly ValidadorEventoClinico validador = new ValidadorEventoClinico();

        private static EventoClinico NuevoEvento(string potreroId, string nombreRes, string tipoEvento, string descripcion)
        {
            return new EventoClinico(potreroId, nombreRes, DateTime.Now, tipoEvento, descripcion);
        }

        [Fact]
        public void EsValido_EventoNulo_DebeRetornarFalse()
        {
            Assert.False(validador.EsValido(null!));
        }

        [Fact]
        public void EsValido_PotreroIdVacio_DebeRetornarFalse()
        {
            Assert.False(validador.EsValido(NuevoEvento("   ", "Vaca1", "Revision", "Chequeo general")));
        }

        [Fact]
        public void EsValido_NombreResVacio_DebeRetornarFalse()
        {
            Assert.False(validador.EsValido(NuevoEvento("P1", "   ", "Revision", "Chequeo general")));
        }

        [Fact]
        public void EsValido_TipoEventoVacio_DebeRetornarFalse()
        {
            Assert.False(validador.EsValido(NuevoEvento("P1", "Vaca1", "   ", "Chequeo general")));
        }

        [Fact]
        public void EsValido_DescripcionVacia_DebeRetornarFalse()
        {
            Assert.False(validador.EsValido(NuevoEvento("P1", "Vaca1", "Revision", "   ")));
        }

        [Fact]
        public void EsValido_DatosValidos_DebeRetornarTrue()
        {
            Assert.True(validador.EsValido(NuevoEvento("P1", "Vaca1", "Revision", "Chequeo general")));
        }
    }
}
