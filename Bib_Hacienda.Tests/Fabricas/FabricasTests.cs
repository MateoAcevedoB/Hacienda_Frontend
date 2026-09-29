using System;
using System.Collections.Generic;
using Bib_Hacienda.Aplicacion;
using Bib_Hacienda.Dominio;
using Xunit;
using static Bib_Hacienda.Dominio.Viva;

namespace Bib_Hacienda.Tests.Fabricas
{
    // FASE 4C-2: unit tests de las fábricas concretas (Bib_Hacienda.Aplicacion.Fabricas*),
    // basados en el código REAL de cada Fabrica*.cs. Cobertura previa: 0%
    // (ServicioPotreros/ServicioVacunacion siempre mockean IFabricaRes /
    // IFabricaVacunaBacteriana / IFabricaVacunaViva, nunca se probaba la
    // implementación concreta). Son lógica de negocio (construyen las
    // entidades de dominio correctas según el tipo solicitado).

    public class FabricaTerneroTests
    {
        [Fact]
        public void Crear_DebeRetornarInstanciaDeTernero()
        {
            IFabricaRes fabrica = new FabricaTernero();
            Res res = fabrica.Crear("Vaca1", 200, 5);

            Assert.IsType<Ternero>(res);
            Assert.Equal("Vaca1", res.Nombre);
            Assert.Equal((uint)200, res.Peso);
            Assert.Equal((ushort)5, res.Edad);
        }
    }

    public class FabricaCebonTests
    {
        [Fact]
        public void Crear_DebeRetornarInstanciaDeCebon()
        {
            IFabricaRes fabrica = new FabricaCebon();
            Res res = fabrica.Crear("Vaca2", 300, 20);

            Assert.IsType<Cebon>(res);
            Assert.Equal("Vaca2", res.Nombre);
            Assert.Equal((uint)300, res.Peso);
            Assert.Equal((ushort)20, res.Edad);
        }
    }

    public class FabricaNovilloTests
    {
        [Fact]
        public void Crear_DebeRetornarInstanciaDeNovillo()
        {
            IFabricaRes fabrica = new FabricaNovillo();
            Res res = fabrica.Crear("Vaca3", 450, 60);

            Assert.IsType<Novillo>(res);
            Assert.Equal("Vaca3", res.Nombre);
            Assert.Equal((uint)450, res.Peso);
            Assert.Equal((ushort)60, res.Edad);
        }
    }

    public class FabricaVacunaBacterianaTests
    {
        [Fact]
        public void Crear_DebeRetornarInstanciaDeBacterianaConDatosCorrectos()
        {
            IFabricaVacunaBacteriana fabrica = new FabricaVacunaBacteriana();
            DateTime vencimiento = DateTime.Now.AddMonths(6);
            DateTime aplicacion = DateTime.Now;

            Bacteriana vacuna = fabrica.Crear("Aftosa", "L1", vencimiento, aplicacion, 3);

            Assert.Equal("Aftosa", vacuna.Nombre);
            Assert.Equal("L1", vacuna.Lote);
            Assert.Equal(vencimiento, vacuna.Fecha_vencimiento);
            Assert.Equal(aplicacion, vacuna.Fecha_aplicacion);
            Assert.Equal((uint)3, vacuna.Periodo_aplicacion);
        }
    }

    public class FabricaVacunaVivaTests
    {
        [Fact]
        public void Crear_DebeRetornarInstanciaDeVivaConDatosCorrectos()
        {
            IFabricaVacunaViva fabrica = new FabricaVacunaViva();
            DateTime vencimiento = DateTime.Now.AddMonths(6);
            DateTime aplicacion = DateTime.Now;

            Viva vacuna = fabrica.Crear("Brucelosis", "L2", vencimiento, aplicacion, enum_l_atenuaciones.Atenuacion20);

            Assert.Equal("Brucelosis", vacuna.Nombre);
            Assert.Equal("L2", vacuna.Lote);
            Assert.Equal(vencimiento, vacuna.Fecha_vencimiento);
            Assert.Equal(aplicacion, vacuna.Fecha_aplicacion);
        }
    }

    public class RegistroFabricasResTests
    {
        [Fact]
        public void ObtenerFabrica_TipoRegistrado_DebeRetornarLaFabricaCorrecta()
        {
            IFabricaRes fabricaTernero = new FabricaTernero();
            var fabricas = new Dictionary<string, IFabricaRes>
            {
                { "ternero", fabricaTernero },
                { "cebon", new FabricaCebon() }
            };
            IRegistroFabricasRes registro = new RegistroFabricasRes(fabricas);

            IFabricaRes obtenida = registro.ObtenerFabrica("ternero");

            Assert.Same(fabricaTernero, obtenida);
        }

        [Fact]
        public void ObtenerFabrica_TipoNoRegistrado_DebeLanzarExcepcion()
        {
            var fabricas = new Dictionary<string, IFabricaRes>
            {
                { "ternero", new FabricaTernero() }
            };
            IRegistroFabricasRes registro = new RegistroFabricasRes(fabricas);

            var ex = Assert.Throws<Exception>(() => registro.ObtenerFabrica("inexistente"));
            Assert.Contains("inexistente", ex.Message);
        }

        [Fact]
        public void ObtenerFabrica_TipoNulo_DebeLanzarExcepcion()
        {
            var fabricas = new Dictionary<string, IFabricaRes>();
            IRegistroFabricasRes registro = new RegistroFabricasRes(fabricas);

            Assert.Throws<Exception>(() => registro.ObtenerFabrica(null!));
        }
    }
}
