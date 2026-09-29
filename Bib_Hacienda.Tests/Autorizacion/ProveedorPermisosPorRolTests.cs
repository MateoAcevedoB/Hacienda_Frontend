using Bib_Hacienda.Aplicacion;
using Xunit;

namespace Bib_Hacienda.Tests.Autorizacion
{
    
    public class ProveedorPermisosPorRolTests
    {
        private readonly ProveedorPermisosPorRol proveedor = new ProveedorPermisosPorRol();

        [Theory]
        [InlineData("Eliminar_Usuario")]
        [InlineData("Consultar_Res")]
        [InlineData("CualquierOperacion")]
        public void TienePermiso_RolAdmin_SiempreDebeRetornarTrue(string operacion)
        {
            Assert.True(proveedor.TienePermiso("admin", operacion));
        }

        [Fact]
        public void TienePermiso_RolEmpleado_OperacionEliminar_DebeRetornarFalse()
        {
            Assert.False(proveedor.TienePermiso("empleado", "Eliminar_Usuario"));
        }

        [Theory]
        [InlineData("Crear_Res")]
        [InlineData("Consultar_Res")]
        [InlineData("Listar_Potreros")]
        public void TienePermiso_RolEmpleado_OperacionNoEliminar_DebeRetornarTrue(string operacion)
        {
            Assert.True(proveedor.TienePermiso("empleado", operacion));
        }

        [Theory]
        [InlineData("Consultar_Res")]
        [InlineData("Listar_Potreros")]
        public void TienePermiso_RolVisitante_ConsultaOListado_DebeRetornarTrue(string operacion)
        {
            Assert.True(proveedor.TienePermiso("visitante", operacion));
        }

        [Theory]
        [InlineData("Crear_Res")]
        [InlineData("Eliminar_Usuario")]
        [InlineData("Actualizar_Potrero")]
        public void TienePermiso_RolVisitante_OperacionNoPermitida_DebeRetornarFalse(string operacion)
        {
            Assert.False(proveedor.TienePermiso("visitante", operacion));
        }

        [Fact]
        public void TienePermiso_RolDesconocido_DebeRetornarFalse()
        {
            Assert.False(proveedor.TienePermiso("rol_inexistente", "Consultar_Res"));
        }
    }
}
