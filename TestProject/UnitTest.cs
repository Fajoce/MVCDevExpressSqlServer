using DevExpressSqlserver.Controllers;
using DevExpressSqlserver.Infraestructure.Conexion;
using DevExpressSqlserver.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;

namespace TestProject
{
    public class UnitTest
    {
        private TiposGastosController CreateControllerWithUser(SBERPDbContext context, int userId)
        {
            var controller = new TiposGastosController(context);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            return controller;
        }

        private SBERPDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<SBERPDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + System.Guid.NewGuid())
                .Options;

            return new SBERPDbContext(options);
        }

        [Fact]
        public void Index_WhenCalled_ReturnsViewResult()
        {
            var context = GetInMemoryDbContext();
            var controller = new TiposGastosController(context);

            var result = controller.Index();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void GetData_WhenUserIsAuthenticated_ReturnsUserSpecificData()
        {
            var context = GetInMemoryDbContext();
            int userId = 1;

            context.TiposGasto.Add(new TipoGasto { TipoGastoID = 1, Descripcion = "Gasto A", UsuarioId = userId });
            context.TiposGasto.Add(new TipoGasto { TipoGastoID = 2, Descripcion = "Gasto B", UsuarioId = 999 });
            context.SaveChanges();

            var controller = CreateControllerWithUser(context, userId);
            var loadOptions = new DevExtreme.AspNet.Data.DataSourceLoadOptions();

            var result = controller.GetData(loadOptions);

            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);
        }

        [Fact]
        public void AddNew_WithValidValues_AddsTipoGastoAndReturnsIt()
        {
            var context = GetInMemoryDbContext();
            int userId = 2;

            var controller = CreateControllerWithUser(context, userId);
            var values = JsonConvert.SerializeObject(new { Descripcion = "Nuevo gasto" });

            var result = controller.AddNew(values);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var tipoGasto = Assert.IsType<TipoGasto>(jsonResult.Value);
            Assert.Equal("Nuevo gasto", tipoGasto.Descripcion);
            Assert.Equal(userId, tipoGasto.UsuarioId);
        }

        [Fact]
        public void Update_WithExistingTipoGasto_UpdatesAndReturnsUpdatedObject()
        {
            var context = GetInMemoryDbContext();
            int userId = 3;

            var tipoGasto = new TipoGasto { TipoGastoID = 10, Descripcion = "Viejo", UsuarioId = userId };
            context.TiposGasto.Add(tipoGasto);
            context.SaveChanges();

            var controller = CreateControllerWithUser(context, userId);
            var newValues = JsonConvert.SerializeObject(new { Descripcion = "Actualizado" });

            var result = controller.Update(10, newValues);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var updated = Assert.IsType<TipoGasto>(jsonResult.Value);
            Assert.Equal("Actualizado", updated.Descripcion);
        }

        [Fact]
        public void Delete_WithExistingTipoGasto_RemovesItAndReturnsOk()
        {
            var context = GetInMemoryDbContext();
            int userId = 4;

            var tipoGasto = new TipoGasto { TipoGastoID = 20, Descripcion = "Eliminar", UsuarioId = userId };
            context.TiposGasto.Add(tipoGasto);
            context.SaveChanges();

            var controller = CreateControllerWithUser(context, userId);
            var result = controller.Delete(20);

            Assert.IsType<OkResult>(result);
            Assert.False(context.TiposGasto.Any(tg => tg.TipoGastoID == 20));
        }
    }
}