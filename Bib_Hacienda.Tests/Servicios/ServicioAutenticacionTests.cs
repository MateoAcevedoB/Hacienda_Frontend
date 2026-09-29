using Bib_Hacienda.Aplicacion;
using Bib_Hacienda.Dominio;
using Moq;
using Xunit;

namespace Bib_Hacienda.Tests.Servicios
{
       public class ServicioAutenticacionTests
    {
        private static (ServicioAutenticacion servicio, Mock<IRepositorioUsuarios> repoMock,
            Mock<IServicioHash> hashMock, Mock<IProveedorPermisos> permisosMock) NuevoServicio(
                List<Usuario>? usuarios = null)
        {
            var repoMock = new Mock<IRepositorioUsuarios>();
            repoMock.Setup(r => r.ObtenerTodos()).Returns(usuarios ?? new List<Usuario>());
            var hashMock = new Mock<IServicioHash>();
            var permisosMock = new Mock<IProveedorPermisos>();
            var servicio = new ServicioAutenticacion(repoMock.Object, hashMock.Object, permisosMock.Object);
            return (servicio, repoMock, hashMock, permisosMock);
        }


        [Fact]
        public void crear_usuario_DatosValidos_DebeHashearContrasenaYAgregarUsuario()
        {
            var (servicio, repoMock, hashMock, _) = NuevoServicio();
            hashMock.Setup(h => h.Hash("clave123")).Returns("HASH(clave123)");

            ResultadoOperacion resultado = servicio.crear_usuario("admin", "clave123");

            Assert.True(resultado.Exito);
            Assert.Contains("admin", resultado.Mensaje);
            hashMock.Verify(h => h.Hash("clave123"), Times.Once);
            repoMock.Verify(r => r.Agregar(It.Is<Usuario>(
                u => u.Nombre == "admin" && u.ContrasenaHash == "HASH(clave123)" && u.ContrasenaHash != "clave123")), Times.Once);
        }

        [Fact]
        public void crear_usuario_NombreDuplicado_DebeRetornarFalloYNoAgregar()
        {
            var existente = new Usuario("admin", "HASH_EXISTENTE");
            var (servicio, repoMock, hashMock, _) = NuevoServicio(new List<Usuario> { existente });

            ResultadoOperacion resultado = servicio.crear_usuario("admin", "clave123");

            Assert.False(resultado.Exito);
            Assert.Contains("Ya existe un usuario", resultado.Mensaje);
            hashMock.Verify(h => h.Hash(It.IsAny<string>()), Times.Never);
            repoMock.Verify(r => r.Agregar(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact]
        public void crear_usuario_NombreVacio_DebeRetornarFalloYNoAgregar()
        {
            var (servicio, repoMock, _, _) = NuevoServicio();

            ResultadoOperacion resultado = servicio.crear_usuario("   ", "clave123");

            Assert.False(resultado.Exito);
            Assert.Contains("nombre del usuario no puede estar vacío", resultado.Mensaje);
            repoMock.Verify(r => r.Agregar(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact]
        public void crear_usuario_ContrasenaVacia_DebeRetornarFalloYNoAgregar()
        {
            var (servicio, repoMock, _, _) = NuevoServicio();

            ResultadoOperacion resultado = servicio.crear_usuario("admin", "");

            Assert.False(resultado.Exito);
            Assert.Contains("contraseña no puede estar vacía", resultado.Mensaje);
            repoMock.Verify(r => r.Agregar(It.IsAny<Usuario>()), Times.Never);
        }


        [Fact]
        public void ValidarCredenciales_UsuarioInexistente_DebeRetornarFalse()
        {
            var (servicio, _, _, _) = NuevoServicio();

            bool resultado = servicio.ValidarCredenciales("fantasma", "clave123");

            Assert.False(resultado);
        }

        [Fact]
        public void ValidarCredenciales_UsuarioExistenteConHashCorrecto_DebeRetornarTrue()
        {
            var usuario = new Usuario("admin", "HASH(clave123)");
            var (servicio, _, hashMock, _) = NuevoServicio(new List<Usuario> { usuario });
            hashMock.Setup(h => h.Verificar("clave123", "HASH(clave123)")).Returns(true);

            bool resultado = servicio.ValidarCredenciales("admin", "clave123");

            Assert.True(resultado);
        }

        [Fact]
        public void ValidarCredenciales_ContrasenaLegacyEnTextoPlano_DebeRetornarTrue()
        {
            var usuario = new Usuario("admin", "clave123");
            var (servicio, _, hashMock, _) = NuevoServicio(new List<Usuario> { usuario });
            hashMock.Setup(h => h.Verificar("clave123", "clave123")).Returns(false);

            bool resultado = servicio.ValidarCredenciales("admin", "clave123");

            Assert.True(resultado);
        }

        [Fact]
        public void ValidarCredenciales_ContrasenaIncorrecta_DebeRetornarFalse()
        {
            var usuario = new Usuario("admin", "HASH(clave123)");
            var (servicio, _, hashMock, _) = NuevoServicio(new List<Usuario> { usuario });
            hashMock.Setup(h => h.Verificar("incorrecta", "HASH(clave123)")).Returns(false);

            bool resultado = servicio.ValidarCredenciales("admin", "incorrecta");

            Assert.False(resultado);
        }


        [Fact]
        public void buscar_usuario_Existente_DebeRetornarUsuario()
        {
            var usuario = new Usuario("admin", "HASH(clave123)");
            var repoMock = new Mock<IRepositorioUsuarios>();
            repoMock.Setup(r => r.BuscarPorNombre("admin")).Returns(usuario);
            var servicio = new ServicioAutenticacion(repoMock.Object, new Mock<IServicioHash>().Object, new Mock<IProveedorPermisos>().Object);

            Usuario resultado = servicio.buscar_usuario("admin");

            Assert.Same(usuario, resultado);
        }

        [Fact]
        public void buscar_usuario_Inexistente_DebeLanzarExcepcion()
        {
            var repoMock = new Mock<IRepositorioUsuarios>();
            repoMock.Setup(r => r.BuscarPorNombre("fantasma")).Returns((Usuario)null!);
            var servicio = new ServicioAutenticacion(repoMock.Object, new Mock<IServicioHash>().Object, new Mock<IProveedorPermisos>().Object);

            var ex = Assert.Throws<Exception>(() => servicio.buscar_usuario("fantasma"));
            Assert.Contains("No se encontró el usuario", ex.Message);
        }

        [Fact]
        public void buscar_usuario_NombreVacio_DebeLanzarExcepcion()
        {
            var repoMock = new Mock<IRepositorioUsuarios>();
            var servicio = new ServicioAutenticacion(repoMock.Object, new Mock<IServicioHash>().Object, new Mock<IProveedorPermisos>().Object);

            var ex = Assert.Throws<Exception>(() => servicio.buscar_usuario(""));
            Assert.Contains("no puede estar vacío", ex.Message);
            repoMock.Verify(r => r.BuscarPorNombre(It.IsAny<string>()), Times.Never);
        }


        [Fact]
        public void listar_usuarios_ConUsuarios_DebeRetornarLista()
        {
            var usuario1 = new Usuario("admin", "H1");
            var usuario2 = new Usuario("empleado", "H2");
            var (servicio, _, _, _) = NuevoServicio(new List<Usuario> { usuario1, usuario2 });

            List<Usuario> resultado = servicio.listar_usuarios();

            Assert.Equal(2, resultado.Count);
            Assert.Contains(usuario1, resultado);
            Assert.Contains(usuario2, resultado);
        }

        [Fact]
        public void listar_usuarios_SinUsuarios_DebeRetornarListaVacia()
        {
            var (servicio, _, _, _) = NuevoServicio();

            List<Usuario> resultado = servicio.listar_usuarios();

            Assert.Empty(resultado);
        }


        [Fact]
        public void AutorizarOperacion_UsuarioNulo_DebeRetornarFallo()
        {
            var (servicio, _, _, permisosMock) = NuevoServicio();

            ResultadoOperacion resultado = servicio.AutorizarOperacion(null!, "ConsultarPotreros");

            Assert.False(resultado.Exito);
            Assert.Contains("no autenticado", resultado.Mensaje);
            permisosMock.Verify(p => p.TienePermiso(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void AutorizarOperacion_UsuarioNoRegistrado_DebeRetornarFallo()
        {
            var repoMock = new Mock<IRepositorioUsuarios>();
            repoMock.Setup(r => r.BuscarPorNombre("fantasma")).Returns((Usuario)null!);
            var permisosMock = new Mock<IProveedorPermisos>();
            var servicio = new ServicioAutenticacion(repoMock.Object, new Mock<IServicioHash>().Object, permisosMock.Object);
            var usuario = new Usuario("fantasma", "H1");

            ResultadoOperacion resultado = servicio.AutorizarOperacion(usuario, "ConsultarPotreros");

            Assert.False(resultado.Exito);
            Assert.Contains("no está registrado", resultado.Mensaje);
            permisosMock.Verify(p => p.TienePermiso(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void AutorizarOperacion_ConPermiso_DebeRetornarExito()
        {
            var usuario = new Usuario("admin", "H1");
            var repoMock = new Mock<IRepositorioUsuarios>();
            repoMock.Setup(r => r.BuscarPorNombre("admin")).Returns(usuario);
            var permisosMock = new Mock<IProveedorPermisos>();
            permisosMock.Setup(p => p.TienePermiso("admin", "EliminarUsuario")).Returns(true);
            var servicio = new ServicioAutenticacion(repoMock.Object, new Mock<IServicioHash>().Object, permisosMock.Object);

            ResultadoOperacion resultado = servicio.AutorizarOperacion(usuario, "EliminarUsuario");

            Assert.True(resultado.Exito);
            Assert.Contains("autorizado", resultado.Mensaje);
        }

        [Fact]
        public void AutorizarOperacion_SinPermiso_DebeRetornarFallo()
        {
            var usuario = new Usuario("empleado", "H1");
            var repoMock = new Mock<IRepositorioUsuarios>();
            repoMock.Setup(r => r.BuscarPorNombre("empleado")).Returns(usuario);
            var permisosMock = new Mock<IProveedorPermisos>();
            permisosMock.Setup(p => p.TienePermiso("empleado", "EliminarUsuario")).Returns(false);
            var servicio = new ServicioAutenticacion(repoMock.Object, new Mock<IServicioHash>().Object, permisosMock.Object);

            ResultadoOperacion resultado = servicio.AutorizarOperacion(usuario, "EliminarUsuario");

            Assert.False(resultado.Exito);
            Assert.Contains("DENEGADO", resultado.Mensaje);
        }

        [Fact]
        public void AutorizarOperacion_RolDesconocido_DebeRetornarFalloSegunProveedorPermisos()
        {
            var usuario = new Usuario("desconocido", "H1");
            var repoMock = new Mock<IRepositorioUsuarios>();
            repoMock.Setup(r => r.BuscarPorNombre("desconocido")).Returns(usuario);
            var permisosMock = new Mock<IProveedorPermisos>();
            permisosMock.Setup(p => p.TienePermiso("desconocido", "ConsultarPotreros")).Returns(false);
            var servicio = new ServicioAutenticacion(repoMock.Object, new Mock<IServicioHash>().Object, permisosMock.Object);

            ResultadoOperacion resultado = servicio.AutorizarOperacion(usuario, "ConsultarPotreros");

            Assert.False(resultado.Exito);
            Assert.Contains("DENEGADO", resultado.Mensaje);
        }


        [Fact]
        public void ObtenerEstadisticas_ConUsuarios_DebeRetornarTotalUsuarios()
        {
            var (servicio, _, _, _) = NuevoServicio(new List<Usuario> { new Usuario("admin", "H1"), new Usuario("empleado", "H2") });

            Dictionary<string, object> estadisticas = servicio.ObtenerEstadisticas();

            Assert.Equal(2, estadisticas["TotalUsuarios"]);
        }

        [Fact]
        public void ObtenerEstadisticas_SinUsuarios_DebeRetornarTotalUsuariosCero()
        {
            var (servicio, _, _, _) = NuevoServicio();

            Dictionary<string, object> estadisticas = servicio.ObtenerEstadisticas();

            Assert.Equal(0, estadisticas["TotalUsuarios"]);
        }
    }
}
