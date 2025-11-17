using iTaxSuite.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace iTaxSuite.Tester
{
    public class UnitTest1
    {
        // [Fact]
        public async Task Test1()
        {
            var controller = new HealthController();

            var actionResult = await controller.Echo();

            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            //var echoResult = Assert.IsType<string>(okResult.Value);
            Assert.Equal(200, okResult.StatusCode);
        }
    }
}