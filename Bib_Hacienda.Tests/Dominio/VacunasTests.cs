using System;
using Bib_Hacienda.Dominio;
using Xunit;
using static Bib_Hacienda.Dominio.Viva;

namespace Bib_Hacienda.Tests.Dominio
{

    public class BacterianaTests
    {
        private static Bacteriana Nueva(uint periodoAplicacion)
        {
            return new Bacteriana("Aftosa", "L1", DateTime.Now.AddMonths(6), DateTime.Now, periodoAplicacion);
        }

        [Theory]
        [InlineData(2u)]
        [InlineData(3u)]
        [InlineData(4u)]
        public void PeriodoAplicacion_DentroDelRango_DebeAsignarse(uint periodo)
        {
            var vacuna = Nueva(periodo);
            Assert.Equal(periodo, vacuna.Periodo_aplicacion);
        }

        [Fact]
        public void PeriodoAplicacion_MenorAlMinimo_DebeLanzarExcepcion()
        {
            var ex = Assert.Throws<Exception>(() => Nueva(1));
            Assert.Contains("periodo de aplicacion", ex.Message);
        }

        [Fact]
        public void PeriodoAplicacion_MayorAlMaximo_DebeLanzarExcepcion()
        {
            var ex = Assert.Throws<Exception>(() => Nueva(5));
            Assert.Contains("periodo de aplicacion", ex.Message);
        }

        [Fact]
        public void Constructor_DebeAsignarCamposHeredadosDeVacuna()
        {
            DateTime vencimiento = DateTime.Now.AddMonths(6);
            DateTime aplicacion = DateTime.Now;
            var vacuna = new Bacteriana("Aftosa", "L1", vencimiento, aplicacion, 3);

            Assert.Equal("Aftosa", vacuna.Nombre);
            Assert.Equal("L1", vacuna.Lote);
            Assert.Equal(vencimiento, vacuna.Fecha_vencimiento);
            Assert.Equal(aplicacion, vacuna.Fecha_aplicacion);
        }
    }

    public class VivaTests
    {
        [Theory]
        [InlineData(enum_l_atenuaciones.Atenuacion10)]
        [InlineData(enum_l_atenuaciones.Atenuacion20)]
        [InlineData(enum_l_atenuaciones.Atenuacion30)]
        public void Constructor_DebeAsignarCamposHeredadosDeVacuna(enum_l_atenuaciones atenuacion)
        {
            DateTime vencimiento = DateTime.Now.AddMonths(6);
            DateTime aplicacion = DateTime.Now;
            var vacuna = new Viva("Brucelosis", "L2", vencimiento, aplicacion, atenuacion);

            Assert.Equal("Brucelosis", vacuna.Nombre);
            Assert.Equal("L2", vacuna.Lote);
            Assert.Equal(vencimiento, vacuna.Fecha_vencimiento);
            Assert.Equal(aplicacion, vacuna.Fecha_aplicacion);
        }
    }
}
