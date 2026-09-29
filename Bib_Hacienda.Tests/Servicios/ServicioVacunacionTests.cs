using Bib_Hacienda.Aplicacion;
using Bib_Hacienda.Dominio;
using Bib_Hacienda.Dominio.Eventos;
using Moq;
using Xunit;

namespace Bib_Hacienda.Tests.Servicios
{
   
    public class ServicioVacunacionTests
    {
        private static Potrero NuevoPotrero(string identificacion, string tipo = "ternero")
        {
            return new Potrero(identificacion, tipo, new PublisherPotreroMitad(), new PublisherPotreroLleno());
        }

        private static ServicioVacunacion NuevoServicio(
            Mock<IRepositorioPotreros> repoPotrerosMock,
            Mock<IRepositorioVacunas> repoVacunasMock)
        {
            return new ServicioVacunacion(
                repoPotrerosMock.Object,
                repoVacunasMock.Object,
                new PublisherVacunaVencida(),
                new PublisherVacunacionCompletada());
        }

        // ------------------------------------------------------------
        // aplicar_vacuna
        // ------------------------------------------------------------

        [Fact]
        public void aplicar_vacuna_PotreroNoExiste_DebeRetornarFalloYNoMarcarAplicada()
        {
            // Arrange
            var repoPotrerosMock = new Mock<IRepositorioPotreros>();
            repoPotrerosMock.Setup(r => r.ObtenerPorId("PotreroX")).Returns((Potrero)null!);
            var repoVacunasMock = new Mock<IRepositorioVacunas>();
            var servicio = NuevoServicio(repoPotrerosMock, repoVacunasMock);
            var vacuna = new Bacteriana("Aftosa", "L1", DateTime.Now.AddMonths(6), DateTime.Now, 3);

            // Act
            ResultadoOperacion resultado = servicio.aplicar_vacuna(vacuna, "Bessie", "PotreroX");

            // Assert
            Assert.False(resultado.Exito);
            Assert.Contains("No se encontró el potrero", resultado.Mensaje);
            repoVacunasMock.Verify(r => r.MarcarComoAplicada(It.IsAny<Vacuna>(), It.IsAny<Res>()), Times.Never);
        }

        [Fact]
        public void aplicar_vacuna_ResNoExisteEnElPotrero_DebeRetornarFallo()
        {
            // Arrange: Potrero.BuscarRes lanza si no encuentra coincidencias;
            // aplicar_vacuna la atrapa con su catch genérico.
            var potrero = NuevoPotrero("Potrero1");
            var repoPotrerosMock = new Mock<IRepositorioPotreros>();
            repoPotrerosMock.Setup(r => r.ObtenerPorId("Potrero1")).Returns(potrero);
            var repoVacunasMock = new Mock<IRepositorioVacunas>();
            var servicio = NuevoServicio(repoPotrerosMock, repoVacunasMock);
            var vacuna = new Bacteriana("Aftosa", "L1", DateTime.Now.AddMonths(6), DateTime.Now, 3);

            // Act
            ResultadoOperacion resultado = servicio.aplicar_vacuna(vacuna, "Inexistente", "Potrero1");

            // Assert
            Assert.False(resultado.Exito);
            Assert.Contains("Error inesperado en el metodo aplicar_vacuna", resultado.Mensaje);
            repoVacunasMock.Verify(r => r.MarcarComoAplicada(It.IsAny<Vacuna>(), It.IsAny<Res>()), Times.Never);
        }

        [Fact]
        public void aplicar_vacuna_VacunaNula_DebeRetornarFallo()
        {
            // Arrange
            var res = new Ternero("Bessie", 260, 5);
            var potrero = NuevoPotrero("Potrero1");
            potrero.L_reses.Add(res);
            var repoPotrerosMock = new Mock<IRepositorioPotreros>();
            repoPotrerosMock.Setup(r => r.ObtenerPorId("Potrero1")).Returns(potrero);
            var repoVacunasMock = new Mock<IRepositorioVacunas>();
            var servicio = NuevoServicio(repoPotrerosMock, repoVacunasMock);

            // Act
            ResultadoOperacion resultado = servicio.aplicar_vacuna(null!, "Bessie", "Potrero1");

            // Assert
            Assert.False(resultado.Exito);
            Assert.Contains("La vacuna no puede ser nula", resultado.Mensaje);
            repoVacunasMock.Verify(r => r.MarcarComoAplicada(It.IsAny<Vacuna>(), It.IsAny<Res>()), Times.Never);
        }

        [Fact]
        public void aplicar_vacuna_VacunaYaAplicada_DebeRetornarFallo()
        {
            // Arrange: coincide por Nombre (aunque el lote sea distinto).
            var res = new Ternero("Bessie", 260, 5);
            res.L_vacunas_aplicadas.Add(new Bacteriana("Aftosa", "L0", DateTime.Now.AddMonths(6), DateTime.Now, 3));
            var potrero = NuevoPotrero("Potrero1");
            potrero.L_reses.Add(res);
            var repoPotrerosMock = new Mock<IRepositorioPotreros>();
            repoPotrerosMock.Setup(r => r.ObtenerPorId("Potrero1")).Returns(potrero);
            var repoVacunasMock = new Mock<IRepositorioVacunas>();
            var servicio = NuevoServicio(repoPotrerosMock, repoVacunasMock);
            var vacunaNueva = new Bacteriana("Aftosa", "L1", DateTime.Now.AddMonths(6), DateTime.Now, 3);

            // Act
            ResultadoOperacion resultado = servicio.aplicar_vacuna(vacunaNueva, "Bessie", "Potrero1");

            // Assert
            Assert.False(resultado.Exito);
            Assert.Contains("ya fue aplicada", resultado.Mensaje);
            repoVacunasMock.Verify(r => r.MarcarComoAplicada(It.IsAny<Vacuna>(), It.IsAny<Res>()), Times.Never);
        }

        [Fact]
        public void aplicar_vacuna_LimiteMaximoBacterianasAlcanzado_DebeRetornarFallo()
        {
            // Arrange: Cebon tiene max_bac_cebon = 1 (ReglaVacuna).
            var res = new Cebon("Bessie", 320, 15);
            res.L_vacunas_aplicadas.Add(new Bacteriana("Brucelosis", "L0", DateTime.Now.AddMonths(6), DateTime.Now, 3));
            var potrero = NuevoPotrero("Potrero1", "cebon");
            potrero.L_reses.Add(res);
            var repoPotrerosMock = new Mock<IRepositorioPotreros>();
            repoPotrerosMock.Setup(r => r.ObtenerPorId("Potrero1")).Returns(potrero);
            var repoVacunasMock = new Mock<IRepositorioVacunas>();
            var servicio = NuevoServicio(repoPotrerosMock, repoVacunasMock);
            var vacunaNueva = new Bacteriana("Aftosa", "L1", DateTime.Now.AddMonths(6), DateTime.Now, 3);

            // Act
            ResultadoOperacion resultado = servicio.aplicar_vacuna(vacunaNueva, "Bessie", "Potrero1");

            // Assert
            Assert.False(resultado.Exito);
            Assert.Contains("No se puede aplicar más vacunas bacterianas", resultado.Mensaje);
            repoVacunasMock.Verify(r => r.MarcarComoAplicada(It.IsAny<Vacuna>(), It.IsAny<Res>()), Times.Never);
        }

        [Fact]
        public void aplicar_vacuna_LimiteMaximoVivasAlcanzado_DebeRetornarFallo()
        {
            // Arrange: Ternero tiene max_viv_ternero = 1 (ReglaVacuna).
            var res = new Ternero("Bessie", 260, 5);
            res.L_vacunas_aplicadas.Add(new Viva("Rabia", "L0", DateTime.Now.AddMonths(6), DateTime.Now, Viva.enum_l_atenuaciones.Atenuacion10));
            var potrero = NuevoPotrero("Potrero1");
            potrero.L_reses.Add(res);
            var repoPotrerosMock = new Mock<IRepositorioPotreros>();
            repoPotrerosMock.Setup(r => r.ObtenerPorId("Potrero1")).Returns(potrero);
            var repoVacunasMock = new Mock<IRepositorioVacunas>();
            var servicio = NuevoServicio(repoPotrerosMock, repoVacunasMock);
            var vacunaNueva = new Viva("Carbunclo", "L1", DateTime.Now.AddMonths(6), DateTime.Now, Viva.enum_l_atenuaciones.Atenuacion20);

            // Act
            ResultadoOperacion resultado = servicio.aplicar_vacuna(vacunaNueva, "Bessie", "Potrero1");

            // Assert
            Assert.False(resultado.Exito);
            Assert.Contains("No se puede aplicar más vacunas vivas", resultado.Mensaje);
            repoVacunasMock.Verify(r => r.MarcarComoAplicada(It.IsAny<Vacuna>(), It.IsAny<Res>()), Times.Never);
        }

        [Fact]
        public void aplicar_vacuna_VacunaVencida_DebeRetornarFalloYNoMarcarAplicada()
        {
            // Arrange: fecha_vencimiento en el pasado -> PublisherVacunaVencida
            // informa "esta_vencida = true" y aplicar_vacuna corta el flujo.
            var res = new Ternero("Bessie", 260, 5);
            var potrero = NuevoPotrero("Potrero1");
            potrero.L_reses.Add(res);
            var repoPotrerosMock = new Mock<IRepositorioPotreros>();
            repoPotrerosMock.Setup(r => r.ObtenerPorId("Potrero1")).Returns(potrero);
            var repoVacunasMock = new Mock<IRepositorioVacunas>();
            var servicio = NuevoServicio(repoPotrerosMock, repoVacunasMock);
            var vacunaVencida = new Bacteriana("Aftosa", "L1", DateTime.Now.AddDays(-1), DateTime.Now.AddMonths(-2), 3);

            // Act
            ResultadoOperacion resultado = servicio.aplicar_vacuna(vacunaVencida, "Bessie", "Potrero1");

            // Assert
            Assert.False(resultado.Exito);
            Assert.Contains("vencida", resultado.Mensaje);
            repoVacunasMock.Verify(r => r.MarcarComoAplicada(It.IsAny<Vacuna>(), It.IsAny<Res>()), Times.Never);
            Assert.DoesNotContain(vacunaVencida, res.L_vacunas_aplicadas);
        }

        [Fact]
        public void aplicar_vacuna_ExitosaBacteriana_DebeMarcarComoAplicadaYAgregarseALaRes()
        {
            // Arrange
            var res = new Ternero("Bessie", 260, 5);
            var potrero = NuevoPotrero("Potrero1");
            potrero.L_reses.Add(res);
            var repoPotrerosMock = new Mock<IRepositorioPotreros>();
            repoPotrerosMock.Setup(r => r.ObtenerPorId("Potrero1")).Returns(potrero);
            var repoVacunasMock = new Mock<IRepositorioVacunas>();
            var servicio = NuevoServicio(repoPotrerosMock, repoVacunasMock);
            var vacuna = new Bacteriana("Aftosa", "L1", DateTime.Now.AddMonths(6), DateTime.Now, 3);

            // Act
            ResultadoOperacion resultado = servicio.aplicar_vacuna(vacuna, "Bessie", "Potrero1");

            // Assert
            Assert.True(resultado.Exito);
            Assert.Contains("Vacuna aplicada correctamente a la res Bessie", resultado.Mensaje);
            Assert.Contains(vacuna, res.L_vacunas_aplicadas);
            repoVacunasMock.Verify(r => r.MarcarComoAplicada(vacuna, res), Times.Once);
        }

        [Fact]
        public void aplicar_vacuna_ExitosaViva_DebeMarcarComoAplicadaYAgregarseALaRes()
        {
            // Arrange
            var res = new Ternero("Bessie", 260, 5);
            var potrero = NuevoPotrero("Potrero1");
            potrero.L_reses.Add(res);
            var repoPotrerosMock = new Mock<IRepositorioPotreros>();
            repoPotrerosMock.Setup(r => r.ObtenerPorId("Potrero1")).Returns(potrero);
            var repoVacunasMock = new Mock<IRepositorioVacunas>();
            var servicio = NuevoServicio(repoPotrerosMock, repoVacunasMock);
            var vacuna = new Viva("Rabia", "L1", DateTime.Now.AddMonths(6), DateTime.Now, Viva.enum_l_atenuaciones.Atenuacion10);

            // Act
            ResultadoOperacion resultado = servicio.aplicar_vacuna(vacuna, "Bessie", "Potrero1");

            // Assert
            Assert.True(resultado.Exito);
            Assert.Contains(vacuna, res.L_vacunas_aplicadas);
            repoVacunasMock.Verify(r => r.MarcarComoAplicada(vacuna, res), Times.Once);
        }

        // ------------------------------------------------------------
        // ObtenerVacunasAplicadas
        // ------------------------------------------------------------

        [Fact]
        public void ObtenerVacunasAplicadas_PotreroYResExisten_DebeRetornarListaDeLaRes()
        {
            // Arrange
            var res = new Ternero("Bessie", 260, 5);
            var vacunaAplicada = new Bacteriana("Aftosa", "L1", DateTime.Now.AddMonths(6), DateTime.Now, 3);
            res.L_vacunas_aplicadas.Add(vacunaAplicada);
            var potrero = NuevoPotrero("Potrero1");
            potrero.L_reses.Add(res);
            var repoPotrerosMock = new Mock<IRepositorioPotreros>();
            repoPotrerosMock.Setup(r => r.ObtenerPorId("Potrero1")).Returns(potrero);
            var repoVacunasMock = new Mock<IRepositorioVacunas>();
            var servicio = NuevoServicio(repoPotrerosMock, repoVacunasMock);

            // Act
            List<Vacuna> resultado = servicio.ObtenerVacunasAplicadas("Potrero1", "Bessie");

            // Assert
            Assert.Single(resultado);
            Assert.Contains(vacunaAplicada, resultado);
        }

        [Fact]
        public void ObtenerVacunasAplicadas_PotreroNoExiste_DebeRetornarListaVacia()
        {
            // Arrange: potrero == null -> potrero.BuscarRes lanza
            // NullReferenceException, atrapada por el catch genérico del método.
            var repoPotrerosMock = new Mock<IRepositorioPotreros>();
            repoPotrerosMock.Setup(r => r.ObtenerPorId("PotreroX")).Returns((Potrero)null!);
            var repoVacunasMock = new Mock<IRepositorioVacunas>();
            var servicio = NuevoServicio(repoPotrerosMock, repoVacunasMock);

            // Act
            List<Vacuna> resultado = servicio.ObtenerVacunasAplicadas("PotreroX", "Bessie");

            // Assert
            Assert.Empty(resultado);
        }
    }
}
