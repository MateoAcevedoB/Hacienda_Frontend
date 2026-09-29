using Bib_Hacienda.Aplicacion;
using Bib_Hacienda.Dominio;
using Bib_Hacienda.Dominio.Eventos;
using Moq;
using Xunit;

namespace Bib_Hacienda.Tests.Servicios
{
    
    public class ServicioVentasTests
    {
        private static Potrero NuevoPotrero(string identificacion, string tipo = "ternero")
        {
            return new Potrero(identificacion, tipo, new PublisherPotreroMitad(), new PublisherPotreroLleno());
        }

        // ------------------------------------------------------------
        // vender_res
        // ------------------------------------------------------------

        [Fact]
        public void vender_res_PotreroNoExiste_DebeRetornarFalloYNoAgregarVenta()
        {
            // Arrange
            var repoPotrerosMock = new Mock<IRepositorioPotreros>();
            repoPotrerosMock.Setup(r => r.ObtenerPorId("PotreroX")).Returns((Potrero)null!);
            var repoVentasMock = new Mock<IRepositorioVentas>();
            var servicio = new ServicioVentas(repoPotrerosMock.Object, repoVentasMock.Object);

            // Act
            ResultadoOperacion resultado = servicio.vender_res("PotreroX", "Bessie", 1000);

            // Assert
            Assert.False(resultado.Exito);
            Assert.Contains("No se encontró el potrero", resultado.Mensaje);
            repoVentasMock.Verify(r => r.Agregar(It.IsAny<Venta>()), Times.Never);
        }

        [Fact]
        public void vender_res_ResNoExisteEnElPotrero_DebeRetornarFalloYNoAgregarVenta()
        {
            // Arrange: Potrero.BuscarRes lanza si no encuentra coincidencias;
            // vender_res la atrapa con su catch genérico.
            var potrero = NuevoPotrero("Potrero1");
            var repoPotrerosMock = new Mock<IRepositorioPotreros>();
            repoPotrerosMock.Setup(r => r.ObtenerPorId("Potrero1")).Returns(potrero);
            var repoVentasMock = new Mock<IRepositorioVentas>();
            var servicio = new ServicioVentas(repoPotrerosMock.Object, repoVentasMock.Object);

            // Act
            ResultadoOperacion resultado = servicio.vender_res("Potrero1", "Inexistente", 1000);

            // Assert
            Assert.False(resultado.Exito);
            Assert.Contains("Error inesperado en el metodo vender_res", resultado.Mensaje);
            repoVentasMock.Verify(r => r.Agregar(It.IsAny<Venta>()), Times.Never);
        }

        [Fact]
        public void vender_res_Exitosa_DebeCrearVentaPersistirlaYRemoverResDelPotrero()
        {
            // Arrange
            var res = new Ternero("Bessie", 260, 5);
            var potrero = NuevoPotrero("Potrero1");
            potrero.L_reses.Add(res);

            var repoPotrerosMock = new Mock<IRepositorioPotreros>();
            repoPotrerosMock.Setup(r => r.ObtenerPorId("Potrero1")).Returns(potrero);
            var repoVentasMock = new Mock<IRepositorioVentas>();
            var servicio = new ServicioVentas(repoPotrerosMock.Object, repoVentasMock.Object);

            // Act
            ResultadoOperacion resultado = servicio.vender_res("Potrero1", "Bessie", 1500);

            // Assert
            Assert.True(resultado.Exito);
            Assert.Contains("Bessie", resultado.Mensaje);
            repoVentasMock.Verify(v => v.Agregar(It.Is<Venta>(
                venta => venta.Potrero == potrero && venta.Activo == res && venta.Monto == 1500)), Times.Once);
            Assert.DoesNotContain(res, potrero.L_reses);
        }

        // ------------------------------------------------------------
        // Consultas (mocks de IRepositorioVentas, sin PostgreSQL/EF Core)
        // ------------------------------------------------------------

        [Fact]
        public void ObtenerTodasLasVentas_DebeRetornarOrdenadasDescendentePorFecha()
        {
            // Arrange
            var potrero = NuevoPotrero("Potrero1");
            var ventaAntigua = new Venta(potrero, new DateTime(2026, 1, 1), new Ternero("A", 260, 5), 100);
            var ventaReciente = new Venta(potrero, new DateTime(2026, 6, 1), new Ternero("B", 260, 5), 200);

            var repoVentasMock = new Mock<IRepositorioVentas>();
            repoVentasMock.Setup(r => r.ObtenerTodas()).Returns(new List<Venta> { ventaAntigua, ventaReciente });
            var servicio = new ServicioVentas(new Mock<IRepositorioPotreros>().Object, repoVentasMock.Object);

            // Act
            List<Venta> resultado = servicio.ObtenerTodasLasVentas();

            // Assert
            Assert.Equal(new List<Venta> { ventaReciente, ventaAntigua }, resultado);
        }

        [Fact]
        public void ObtenerVentasPorPotrero_DebeFiltrarPorIdentificacionYOrdenarDescendente()
        {
            // Arrange
            var potreroA = NuevoPotrero("PotreroA");
            var potreroB = NuevoPotrero("PotreroB");
            var ventaA1 = new Venta(potreroA, new DateTime(2026, 1, 1), new Ternero("A1", 260, 5), 100);
            var ventaA2 = new Venta(potreroA, new DateTime(2026, 3, 1), new Ternero("A2", 260, 5), 150);
            var ventaB1 = new Venta(potreroB, new DateTime(2026, 2, 1), new Ternero("B1", 260, 5), 200);

            var repoVentasMock = new Mock<IRepositorioVentas>();
            repoVentasMock.Setup(r => r.ObtenerTodas()).Returns(new List<Venta> { ventaA1, ventaA2, ventaB1 });
            var servicio = new ServicioVentas(new Mock<IRepositorioPotreros>().Object, repoVentasMock.Object);

            // Act
            List<Venta> resultado = servicio.ObtenerVentasPorPotrero("PotreroA");

            // Assert
            Assert.Equal(new List<Venta> { ventaA2, ventaA1 }, resultado);
        }

        [Fact]
        public void ObtenerVentasPorFechas_DebeFiltrarPorRangoInclusivoYOrdenarDescendente()
        {
            // Arrange
            var potrero = NuevoPotrero("Potrero1");
            var fueraDeRangoAntes = new Venta(potrero, new DateTime(2026, 1, 1), new Ternero("A", 260, 5), 100);
            var dentroInicio = new Venta(potrero, new DateTime(2026, 2, 1), new Ternero("B", 260, 5), 200);
            var dentroFin = new Venta(potrero, new DateTime(2026, 3, 1), new Ternero("C", 260, 5), 300);
            var fueraDeRangoDespues = new Venta(potrero, new DateTime(2026, 4, 1), new Ternero("D", 260, 5), 400);

            var repoVentasMock = new Mock<IRepositorioVentas>();
            repoVentasMock.Setup(r => r.ObtenerTodas()).Returns(new List<Venta>
            {
                fueraDeRangoAntes, dentroInicio, dentroFin, fueraDeRangoDespues
            });
            var servicio = new ServicioVentas(new Mock<IRepositorioPotreros>().Object, repoVentasMock.Object);

            // Act
            List<Venta> resultado = servicio.ObtenerVentasPorFechas(new DateTime(2026, 2, 1), new DateTime(2026, 3, 1));

            // Assert
            Assert.Equal(new List<Venta> { dentroFin, dentroInicio }, resultado);
        }

        // ------------------------------------------------------------
        // ObtenerEstadisticas (FASE 4C: sin cobertura en 4A/4B)
        // ------------------------------------------------------------

        [Fact]
        public void ObtenerEstadisticas_ConVentasDeEsteMesYDeOtrosMeses_DebeCalcularTotalesYFiltroDelMes()
        {
            // Arrange
            var potrero = NuevoPotrero("Potrero1");
            var ahora = DateTime.Now;
            var ventaEsteMes1 = new Venta(potrero, ahora, new Ternero("A", 260, 5), 100);
            var ventaEsteMes2 = new Venta(potrero, ahora, new Ternero("B", 260, 5), 300);
            var ventaOtroMes = new Venta(potrero, ahora.AddYears(-1), new Ternero("C", 260, 5), 500);

            var repoVentasMock = new Mock<IRepositorioVentas>();
            repoVentasMock.Setup(r => r.ObtenerTodas()).Returns(new List<Venta> { ventaEsteMes1, ventaEsteMes2, ventaOtroMes });
            var servicio = new ServicioVentas(new Mock<IRepositorioPotreros>().Object, repoVentasMock.Object);

            // Act
            Dictionary<string, object> estadisticas = servicio.ObtenerEstadisticas();

            // Assert
            // NOTA (fix FASE 4C): Venta.Monto es uint, pero Enumerable.Sum()
            // no tiene sobrecarga para uint/ulong; el compilador convierte
            // implícitamente el selector a Func<Venta,long> y usa Sum(long),
            // así que ServicioVentas.ObtenerEstadisticas() devuelve "MontoTotal"
            // y "MontoEsteMes" como long (no uint). El valor numérico es
            // correcto; solo se corrige el tipo esperado en la aserción para
            // que Assert.Equal (que compara tipo Y valor al recibir object)
            // no falle por boxing distinto.
            Assert.Equal(3, estadisticas["TotalVentas"]);
            Assert.Equal(900L, estadisticas["MontoTotal"]);
            Assert.Equal(300.0, estadisticas["PromedioVenta"]);
            Assert.Equal(2, estadisticas["VentasEsteMes"]);
            Assert.Equal(400L, estadisticas["MontoEsteMes"]);
        }

        [Fact]
        public void ObtenerEstadisticas_SinVentas_DebeRetornarPromedioCeroYContadoresEnCero()
        {
            // Arrange: rama "ventas.Any() ? ... : 0" cuando no hay ventas registradas.
            var repoVentasMock = new Mock<IRepositorioVentas>();
            repoVentasMock.Setup(r => r.ObtenerTodas()).Returns(new List<Venta>());
            var servicio = new ServicioVentas(new Mock<IRepositorioPotreros>().Object, repoVentasMock.Object);

            // Act
            Dictionary<string, object> estadisticas = servicio.ObtenerEstadisticas();

            // Assert
            // NOTA (fix FASE 4C): ver comentario equivalente en el test
            // "ConVentasDeEsteMesYDeOtrosMeses" sobre por qué MontoTotal y
            // MontoEsteMes son long y no uint.
            Assert.Equal(0, estadisticas["TotalVentas"]);
            Assert.Equal(0L, estadisticas["MontoTotal"]);
            Assert.Equal(0.0, estadisticas["PromedioVenta"]);
            Assert.Equal(0, estadisticas["VentasEsteMes"]);
            Assert.Equal(0L, estadisticas["MontoEsteMes"]);
        }
    }
}
