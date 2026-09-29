using Bib_Hacienda.Aplicacion;
using Bib_Hacienda.Dominio;
using Moq;
using Xunit;
using static Bib_Hacienda.Dominio.Viva;

namespace Bib_Hacienda.Tests.Servicios
{
    
    public class ServicioInventarioVacunasTests
    {
        private static (ServicioInventarioVacunas servicio, Mock<IRepositorioVacunas> repoMock,
            Mock<IFabricaVacunaBacteriana> fabBacMock, Mock<IFabricaVacunaViva> fabVivMock) NuevoServicio(
                List<Vacuna>? disponibles = null)
        {
            var repoMock = new Mock<IRepositorioVacunas>();
            repoMock.Setup(r => r.ObtenerDisponibles()).Returns(disponibles ?? new List<Vacuna>());
            var fabBacMock = new Mock<IFabricaVacunaBacteriana>();
            var fabVivMock = new Mock<IFabricaVacunaViva>();
            var servicio = new ServicioInventarioVacunas(repoMock.Object, fabBacMock.Object, fabVivMock.Object);
            return (servicio, repoMock, fabBacMock, fabVivMock);
        }

        // ------------------------------------------------------------
        // crear_vacuna (bacteriana individual)
        // ------------------------------------------------------------

        [Fact]
        public void crear_vacuna_BacterianaIndividual_DatosValidos_DebeCrearYAgregarAlInventario()
        {
            // Arrange
            var (servicio, repoMock, fabBacMock, _) = NuevoServicio();
            var fechaAplicacion = DateTime.Now;
            var fechaVencimiento = DateTime.Now.AddMonths(6);
            var vacunaCreada = new Bacteriana("Aftosa", "L1", fechaVencimiento, fechaAplicacion, 3);
            fabBacMock.Setup(f => f.Crear("Aftosa", "L1", fechaVencimiento, fechaAplicacion, 3u)).Returns(vacunaCreada);

            // Act
            ResultadoOperacion resultado = servicio.crear_vacuna("Aftosa", "L1", fechaVencimiento, fechaAplicacion, 3u);

            // Assert
            Assert.True(resultado.Exito);
            Assert.Contains("Aftosa", resultado.Mensaje);
            repoMock.Verify(r => r.Agregar(vacunaCreada), Times.Once);
        }

        [Fact]
        public void crear_vacuna_BacterianaIndividual_NombreVacio_DebeRetornarFalloYNoAgregar()
        {
            // Arrange
            var (servicio, repoMock, fabBacMock, _) = NuevoServicio();

            // Act
            ResultadoOperacion resultado = servicio.crear_vacuna("", "L1", DateTime.Now.AddMonths(6), DateTime.Now, 3u);

            // Assert
            Assert.False(resultado.Exito);
            Assert.Contains("nombre de la vacuna no puede estar vacío", resultado.Mensaje);
            fabBacMock.Verify(f => f.Crear(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<uint>()), Times.Never);
            repoMock.Verify(r => r.Agregar(It.IsAny<Vacuna>()), Times.Never);
        }

        [Fact]
        public void crear_vacuna_BacterianaIndividual_LoteVacio_DebeRetornarFallo()
        {
            var (servicio, _, _, _) = NuevoServicio();

            ResultadoOperacion resultado = servicio.crear_vacuna("Aftosa", "", DateTime.Now.AddMonths(6), DateTime.Now, 3u);

            Assert.False(resultado.Exito);
            Assert.Contains("lote de la vacuna no puede estar vacío", resultado.Mensaje);
        }

        [Fact]
        public void crear_vacuna_BacterianaIndividual_FechaVencimientoNoPosteriorAAplicacion_DebeRetornarFallo()
        {
            var (servicio, _, _, _) = NuevoServicio();
            var fecha = DateTime.Now;

            ResultadoOperacion resultado = servicio.crear_vacuna("Aftosa", "L1", fecha, fecha, 3u);

            Assert.False(resultado.Exito);
            Assert.Contains("fecha de vencimiento debe ser posterior", resultado.Mensaje);
        }

        [Fact]
        public void crear_vacuna_BacterianaIndividual_LoteDuplicado_DebeRetornarFalloYNoAgregar()
        {
            // Arrange
            var existente = new Bacteriana("Brucelosis", "L1", DateTime.Now.AddMonths(6), DateTime.Now, 3);
            var (servicio, repoMock, fabBacMock, _) = NuevoServicio(new List<Vacuna> { existente });

            // Act
            ResultadoOperacion resultado = servicio.crear_vacuna("Aftosa", "L1", DateTime.Now.AddMonths(6), DateTime.Now, 3u);

            // Assert
            Assert.False(resultado.Exito);
            Assert.Contains("Ya existe una vacuna con el lote", resultado.Mensaje);
            fabBacMock.Verify(f => f.Crear(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<uint>()), Times.Never);
            repoMock.Verify(r => r.Agregar(It.IsAny<Vacuna>()), Times.Never);
        }

        // ------------------------------------------------------------
        // crear_vacuna (viva individual)
        // ------------------------------------------------------------

        [Fact]
        public void crear_vacuna_VivaIndividual_DatosValidos_DebeCrearYAgregarAlInventario()
        {
            // Arrange
            var (servicio, repoMock, _, fabVivMock) = NuevoServicio();
            var fechaAplicacion = DateTime.Now;
            var fechaVencimiento = DateTime.Now.AddMonths(6);
            var vacunaCreada = new Viva("Rabia", "L1", fechaVencimiento, fechaAplicacion, enum_l_atenuaciones.Atenuacion10);
            fabVivMock.Setup(f => f.Crear("Rabia", "L1", fechaVencimiento, fechaAplicacion, enum_l_atenuaciones.Atenuacion10)).Returns(vacunaCreada);

            // Act
            ResultadoOperacion resultado = servicio.crear_vacuna("Rabia", "L1", fechaVencimiento, fechaAplicacion, enum_l_atenuaciones.Atenuacion10);

            // Assert
            Assert.True(resultado.Exito);
            Assert.Contains("Rabia", resultado.Mensaje);
            repoMock.Verify(r => r.Agregar(vacunaCreada), Times.Once);
        }

        [Fact]
        public void crear_vacuna_VivaIndividual_NombreVacio_DebeRetornarFalloYNoAgregar()
        {
            // Arrange: mismo guard que la sobrecarga bacteriana, sin
            // cobertura previa en la sobrecarga viva.
            var (servicio, repoMock, _, fabVivMock) = NuevoServicio();

            // Act
            ResultadoOperacion resultado = servicio.crear_vacuna("", "L1", DateTime.Now.AddMonths(6), DateTime.Now, enum_l_atenuaciones.Atenuacion10);

            // Assert
            Assert.False(resultado.Exito);
            Assert.Contains("nombre de la vacuna no puede estar vacío", resultado.Mensaje);
            fabVivMock.Verify(f => f.Crear(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<enum_l_atenuaciones>()), Times.Never);
            repoMock.Verify(r => r.Agregar(It.IsAny<Vacuna>()), Times.Never);
        }

        [Fact]
        public void crear_vacuna_VivaIndividual_LoteVacio_DebeRetornarFallo()
        {
            var (servicio, _, _, _) = NuevoServicio();

            ResultadoOperacion resultado = servicio.crear_vacuna("Rabia", "", DateTime.Now.AddMonths(6), DateTime.Now, enum_l_atenuaciones.Atenuacion10);

            Assert.False(resultado.Exito);
            Assert.Contains("lote de la vacuna no puede estar vacío", resultado.Mensaje);
        }

        [Fact]
        public void crear_vacuna_VivaIndividual_FechaVencimientoNoPosteriorAAplicacion_DebeRetornarFallo()
        {
            var (servicio, _, _, _) = NuevoServicio();
            var fecha = DateTime.Now;

            ResultadoOperacion resultado = servicio.crear_vacuna("Rabia", "L1", fecha, fecha, enum_l_atenuaciones.Atenuacion10);

            Assert.False(resultado.Exito);
            Assert.Contains("fecha de vencimiento debe ser posterior", resultado.Mensaje);
        }

        [Fact]
        public void crear_vacuna_VivaIndividual_LoteDuplicado_DebeRetornarFallo()
        {
            // Arrange
            var existente = new Viva("Carbunclo", "L1", DateTime.Now.AddMonths(6), DateTime.Now, enum_l_atenuaciones.Atenuacion20);
            var (servicio, _, _, fabVivMock) = NuevoServicio(new List<Vacuna> { existente });

            // Act
            ResultadoOperacion resultado = servicio.crear_vacuna("Rabia", "L1", DateTime.Now.AddMonths(6), DateTime.Now, enum_l_atenuaciones.Atenuacion10);

            // Assert
            Assert.False(resultado.Exito);
            Assert.Contains("Ya existe una vacuna con el lote", resultado.Mensaje);
            fabVivMock.Verify(f => f.Crear(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<enum_l_atenuaciones>()), Times.Never);
        }

        // ------------------------------------------------------------
        // crear_vacuna (lote bacteriano)
        // ------------------------------------------------------------

        [Fact]
        public void crear_vacuna_LoteBacteriano_CantidadCero_DebeRetornarFallo()
        {
            var (servicio, _, _, _) = NuevoServicio();

            ResultadoOperacion resultado = servicio.crear_vacuna("Aftosa", "LB", DateTime.Now.AddMonths(6), DateTime.Now, 3u, 0u);

            Assert.False(resultado.Exito);
            Assert.Contains("cantidad debe ser mayor a 0", resultado.Mensaje);
        }

        [Fact]
        public void crear_vacuna_LoteBacteriano_CantidadMayorA100_DebeRetornarFallo()
        {
            var (servicio, _, _, _) = NuevoServicio();

            ResultadoOperacion resultado = servicio.crear_vacuna("Aftosa", "LB", DateTime.Now.AddMonths(6), DateTime.Now, 3u, 101u);

            Assert.False(resultado.Exito);
            Assert.Contains("No se pueden crear más de 100 vacunas", resultado.Mensaje);
        }

        [Fact]
        public void crear_vacuna_LoteBacteriano_TodosLosLotesValidos_DebeCrearLaCantidadSolicitada()
        {
            // Arrange
            var (servicio, repoMock, fabBacMock, _) = NuevoServicio();
            fabBacMock.Setup(f => f.Crear(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<uint>()))
                .Returns((string n, string l, DateTime fv, DateTime fa, uint p) => new Bacteriana(n, l, fv, fa, p));

            // Act
            ResultadoOperacion resultado = servicio.crear_vacuna("Aftosa", "LB", DateTime.Now.AddMonths(6), DateTime.Now, 3u, 3u);

            // Assert
            Assert.True(resultado.Exito);
            Assert.Contains("Cantidad creada: 3 de 3", resultado.Mensaje);
            repoMock.Verify(r => r.Agregar(It.IsAny<Vacuna>()), Times.Exactly(3));
            fabBacMock.Verify(f => f.Crear("Aftosa", "LB-001", It.IsAny<DateTime>(), It.IsAny<DateTime>(), 3u), Times.Once);
            fabBacMock.Verify(f => f.Crear("Aftosa", "LB-002", It.IsAny<DateTime>(), It.IsAny<DateTime>(), 3u), Times.Once);
            fabBacMock.Verify(f => f.Crear("Aftosa", "LB-003", It.IsAny<DateTime>(), It.IsAny<DateTime>(), 3u), Times.Once);
        }

        [Fact]
        public void crear_vacuna_LoteBacteriano_AlgunosLotesYaExisten_DebeCrearSoloLosNuevos()
        {
            // Arrange: LB-002 ya existe en el inventario.
            var existente = new Bacteriana("Aftosa", "LB-002", DateTime.Now.AddMonths(6), DateTime.Now, 3);
            var (servicio, repoMock, fabBacMock, _) = NuevoServicio(new List<Vacuna> { existente });
            fabBacMock.Setup(f => f.Crear(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<uint>()))
                .Returns((string n, string l, DateTime fv, DateTime fa, uint p) => new Bacteriana(n, l, fv, fa, p));

            // Act
            ResultadoOperacion resultado = servicio.crear_vacuna("Aftosa", "LB", DateTime.Now.AddMonths(6), DateTime.Now, 3u, 3u);

            // Assert
            Assert.True(resultado.Exito);
            Assert.Contains("Cantidad creada: 2 de 3", resultado.Mensaje);
            repoMock.Verify(r => r.Agregar(It.IsAny<Vacuna>()), Times.Exactly(2));
            fabBacMock.Verify(f => f.Crear(It.IsAny<string>(), "LB-002", It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<uint>()), Times.Never);
        }

        [Fact]
        public void crear_vacuna_LoteBacteriano_TodosLosLotesYaExisten_DebeRetornarFallo()
        {
            // Arrange
            var existentes = new List<Vacuna>
            {
                new Bacteriana("Aftosa", "LB-001", DateTime.Now.AddMonths(6), DateTime.Now, 3)
            };
            var (servicio, repoMock, fabBacMock, _) = NuevoServicio(existentes);

            // Act
            ResultadoOperacion resultado = servicio.crear_vacuna("Aftosa", "LB", DateTime.Now.AddMonths(6), DateTime.Now, 3u, 1u);

            // Assert
            Assert.False(resultado.Exito);
            Assert.Contains("No se pudo crear ninguna vacuna", resultado.Mensaje);
            repoMock.Verify(r => r.Agregar(It.IsAny<Vacuna>()), Times.Never);
        }

        // ------------------------------------------------------------
        // crear_vacuna (lote vivo)
        // ------------------------------------------------------------

        [Fact]
        public void crear_vacuna_LoteVivo_TodosLosLotesValidos_DebeCrearLaCantidadSolicitada()
        {
            // Arrange
            var (servicio, repoMock, _, fabVivMock) = NuevoServicio();
            fabVivMock.Setup(f => f.Crear(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<enum_l_atenuaciones>()))
                .Returns((string n, string l, DateTime fv, DateTime fa, enum_l_atenuaciones g) => new Viva(n, l, fv, fa, g));

            // Act
            ResultadoOperacion resultado = servicio.crear_vacuna("Rabia", "LV", DateTime.Now.AddMonths(6), DateTime.Now, enum_l_atenuaciones.Atenuacion10, 2u);

            // Assert
            Assert.True(resultado.Exito);
            Assert.Contains("Cantidad creada: 2 de 2", resultado.Mensaje);
            repoMock.Verify(r => r.Agregar(It.IsAny<Vacuna>()), Times.Exactly(2));
        }

        [Fact]
        public void crear_vacuna_LoteVivo_CantidadMayorA100_DebeRetornarFallo()
        {
            var (servicio, _, _, _) = NuevoServicio();

            ResultadoOperacion resultado = servicio.crear_vacuna("Rabia", "LV", DateTime.Now.AddMonths(6), DateTime.Now, enum_l_atenuaciones.Atenuacion10, 150u);

            Assert.False(resultado.Exito);
            Assert.Contains("No se pueden crear más de 100 vacunas", resultado.Mensaje);
        }

        [Fact]
        public void crear_vacuna_LoteVivo_CantidadCero_DebeRetornarFallo()
        {
            // Arrange: mismo guard "cantidad <= 0" que la sobrecarga bacteriana,
            // sin cobertura previa en la sobrecarga viva.
            var (servicio, _, _, _) = NuevoServicio();

            ResultadoOperacion resultado = servicio.crear_vacuna("Rabia", "LV", DateTime.Now.AddMonths(6), DateTime.Now, enum_l_atenuaciones.Atenuacion10, 0u);

            Assert.False(resultado.Exito);
            Assert.Contains("cantidad debe ser mayor a 0", resultado.Mensaje);
        }

        [Fact]
        public void crear_vacuna_LoteVivo_AlgunosLotesYaExisten_DebeCrearSoloLosNuevos()
        {
            // Arrange: LV-002 ya existe en el inventario.
            var existente = new Viva("Rabia", "LV-002", DateTime.Now.AddMonths(6), DateTime.Now, enum_l_atenuaciones.Atenuacion20);
            var (servicio, repoMock, _, fabVivMock) = NuevoServicio(new List<Vacuna> { existente });
            fabVivMock.Setup(f => f.Crear(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<enum_l_atenuaciones>()))
                .Returns((string n, string l, DateTime fv, DateTime fa, enum_l_atenuaciones g) => new Viva(n, l, fv, fa, g));

            // Act
            ResultadoOperacion resultado = servicio.crear_vacuna("Rabia", "LV", DateTime.Now.AddMonths(6), DateTime.Now, enum_l_atenuaciones.Atenuacion10, 3u);

            // Assert
            Assert.True(resultado.Exito);
            Assert.Contains("Cantidad creada: 2 de 3", resultado.Mensaje);
            repoMock.Verify(r => r.Agregar(It.IsAny<Vacuna>()), Times.Exactly(2));
            fabVivMock.Verify(f => f.Crear(It.IsAny<string>(), "LV-002", It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<enum_l_atenuaciones>()), Times.Never);
        }

        [Fact]
        public void crear_vacuna_LoteVivo_TodosLosLotesYaExisten_DebeRetornarFallo()
        {
            // Arrange
            var existentes = new List<Vacuna>
            {
                new Viva("Rabia", "LV-001", DateTime.Now.AddMonths(6), DateTime.Now, enum_l_atenuaciones.Atenuacion20)
            };
            var (servicio, repoMock, _, _) = NuevoServicio(existentes);

            // Act
            ResultadoOperacion resultado = servicio.crear_vacuna("Rabia", "LV", DateTime.Now.AddMonths(6), DateTime.Now, enum_l_atenuaciones.Atenuacion10, 1u);

            // Assert
            Assert.False(resultado.Exito);
            Assert.Contains("No se pudo crear ninguna vacuna", resultado.Mensaje);
            repoMock.Verify(r => r.Agregar(It.IsAny<Vacuna>()), Times.Never);
        }

        // ------------------------------------------------------------
        // CrearVacuna (fachada)
        // ------------------------------------------------------------

        [Fact]
        public void CrearVacuna_SoloPeriodoAplicacion_DebeDelegarEnSobrecargaBacteriana()
        {
            // Arrange
            var (servicio, repoMock, fabBacMock, _) = NuevoServicio();
            var fecha = DateTime.Now.AddMonths(6);
            var fechaAplic = DateTime.Now;
            var creada = new Bacteriana("Aftosa", "L1", fecha, fechaAplic, 3);
            fabBacMock.Setup(f => f.Crear("Aftosa", "L1", fecha, fechaAplic, 3u)).Returns(creada);

            // Act
            ResultadoOperacion resultado = servicio.CrearVacuna("Aftosa", "L1", fecha, fechaAplic, 3u, null);

            // Assert
            Assert.True(resultado.Exito);
            repoMock.Verify(r => r.Agregar(creada), Times.Once);
        }

        [Fact]
        public void CrearVacuna_SoloAtenuacion_DebeDelegarEnSobrecargaViva()
        {
            // Arrange
            var (servicio, repoMock, _, fabVivMock) = NuevoServicio();
            var fecha = DateTime.Now.AddMonths(6);
            var fechaAplic = DateTime.Now;
            var creada = new Viva("Rabia", "L1", fecha, fechaAplic, enum_l_atenuaciones.Atenuacion10);
            fabVivMock.Setup(f => f.Crear("Rabia", "L1", fecha, fechaAplic, enum_l_atenuaciones.Atenuacion10)).Returns(creada);

            // Act
            ResultadoOperacion resultado = servicio.CrearVacuna("Rabia", "L1", fecha, fechaAplic, null, enum_l_atenuaciones.Atenuacion10);

            // Assert
            Assert.True(resultado.Exito);
            repoMock.Verify(r => r.Agregar(creada), Times.Once);
        }

        [Fact]
        public void CrearVacuna_AmbosParametrosPresentes_DebeRetornarFallo()
        {
            var (servicio, _, _, _) = NuevoServicio();

            ResultadoOperacion resultado = servicio.CrearVacuna("Aftosa", "L1", DateTime.Now.AddMonths(6), DateTime.Now, 3u, enum_l_atenuaciones.Atenuacion10);

            Assert.False(resultado.Exito);
            Assert.Contains("parámetros inválidos", resultado.Mensaje);
        }

        [Fact]
        public void CrearVacuna_NingunParametroPresente_DebeRetornarFallo()
        {
            var (servicio, _, _, _) = NuevoServicio();

            ResultadoOperacion resultado = servicio.CrearVacuna("Aftosa", "L1", DateTime.Now.AddMonths(6), DateTime.Now, null, null);

            Assert.False(resultado.Exito);
            Assert.Contains("parámetros inválidos", resultado.Mensaje);
        }

        // ------------------------------------------------------------
        // ObtenerVacunasDisponibles
        // ------------------------------------------------------------

        [Fact]
        public void ObtenerVacunasDisponibles_ConVacunas_DebeRetornarOrdenadasPorNombre()
        {
            // Arrange
            var vacunaZ = new Bacteriana("Zeta", "L1", DateTime.Now.AddMonths(6), DateTime.Now, 3);
            var vacunaA = new Viva("Alfa", "L2", DateTime.Now.AddMonths(6), DateTime.Now, enum_l_atenuaciones.Atenuacion10);
            var (servicio, _, _, _) = NuevoServicio(new List<Vacuna> { vacunaZ, vacunaA });

            // Act
            List<Vacuna> resultado = servicio.ObtenerVacunasDisponibles();

            // Assert
            Assert.Equal(new List<Vacuna> { vacunaA, vacunaZ }, resultado);
        }

        [Fact]
        public void ObtenerVacunasDisponibles_SinVacunas_DebeRetornarListaVacia()
        {
            var (servicio, _, _, _) = NuevoServicio();

            List<Vacuna> resultado = servicio.ObtenerVacunasDisponibles();

            Assert.Empty(resultado);
        }

        // ------------------------------------------------------------
        // ObtenerEstadisticas
        // ------------------------------------------------------------

        [Fact]
        public void ObtenerEstadisticas_DebeContarPorTipoYVigencia()
        {
            // Arrange
            var bacterianaVigente = new Bacteriana("Aftosa", "L1", DateTime.Now.AddMonths(6), DateTime.Now, 3);
            var vivaVigente = new Viva("Rabia", "L2", DateTime.Now.AddMonths(6), DateTime.Now, enum_l_atenuaciones.Atenuacion10);
            var bacterianaVencida = new Bacteriana("Brucelosis", "L3", DateTime.Now.AddDays(-1), DateTime.Now.AddMonths(-2), 3);
            var (servicio, _, _, _) = NuevoServicio(new List<Vacuna> { bacterianaVigente, vivaVigente, bacterianaVencida });

            // Act
            Dictionary<string, object> estadisticas = servicio.ObtenerEstadisticas();

            // Assert
            Assert.Equal(3, estadisticas["TotalVacunas"]);
            Assert.Equal(2, estadisticas["Bacterianas"]);
            Assert.Equal(1, estadisticas["Vivas"]);
            Assert.Equal(1, estadisticas["Vencidas"]);
            Assert.Equal(2, estadisticas["Vigentes"]);
        }

        [Fact]
        public void ObtenerEstadisticas_SinVacunas_DebeRetornarTodosLosContadoresEnCero()
        {
            // Arrange
            var (servicio, _, _, _) = NuevoServicio();

            // Act
            Dictionary<string, object> estadisticas = servicio.ObtenerEstadisticas();

            // Assert
            Assert.Equal(0, estadisticas["TotalVacunas"]);
            Assert.Equal(0, estadisticas["Bacterianas"]);
            Assert.Equal(0, estadisticas["Vivas"]);
            Assert.Equal(0, estadisticas["Vencidas"]);
            Assert.Equal(0, estadisticas["Vigentes"]);
        }
    }
}
