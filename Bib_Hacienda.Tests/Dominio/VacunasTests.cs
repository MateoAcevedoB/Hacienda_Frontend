using System;
using Bib_Hacienda.Dominio;
using Xunit;
using static Bib_Hacienda.Dominio.Viva;

namespace Bib_Hacienda.Tests.Dominio
{
    // FASE 4C-2: unit tests de Bacteriana y Viva (Bib_Hacienda.Dominio),
    // basados en el código REAL de Bacteriana.cs / Viva.cs / ReglaVacuna.cs.
    // Cobertura previa: Bacteriana 85.7% (faltaba la rama de excepción del
    // setter Periodo_aplicacion), Viva 57% (solo se probaba a través de
    // ServicioVacunacion con mocks del validador).

    public class BacterianaTests
    {
        private static Bacteriana Nueva(uint periodoAplicacion)
        {
            return new Bacteriana("Aftosa", "L1", DateTime.Now.AddMonths(6), DateTime.Now, periodoAplicacion);
        }

        [Theory]
        [InlineData(2u)] // límite inferior
        [InlineData(3u)]
        [InlineData(4u)] // límite superior
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
