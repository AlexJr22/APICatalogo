using APICatalogo.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace APICatalogo_xUnitTest.UnitTests
{
    public class GetProdutosUnitTests : IClassFixture<ProdutosUnitTestController>
    {
        private readonly ProdutosController _controller;

        public GetProdutosUnitTests(ProdutosUnitTestController controller)
        {
            _controller = new ProdutosController(
                controller.repository, controller.mapper);
        }

        [Fact]
        public async Task GetProdutosById_OkResult()
        {
            // Arrange
            var produtoId = 2;

            // Act
            var data = await _controller.Get(produtoId);

            // Assert (padrão)
            //var OkResult = Assert.IsType<OkObjectResult>(data.Result);
            //Assert.Equal(200, OkResult.StatusCode);

            // Assert (fluentassertions)
            data.Result.Should()
                .BeOfType<OkObjectResult>()
                .Which.StatusCode
                .Should().Be(200);
        }

        [Fact]
        public async Task GetProdutosById_Return_NotFound()
        {
            int id = -1;

            var data = await _controller.Get(id);

            data.Result.Should()
                .BeOfType<NotFoundObjectResult>()
                .Which.StatusCode
                .Should().Be(404);
        }
    }
}
