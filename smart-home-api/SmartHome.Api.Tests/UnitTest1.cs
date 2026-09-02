using Microsoft.Extensions.DependencyInjection;

namespace SmartHome.Api.Tests
{
    public class SwaggerStartupTests
    {
        [Test]
        public void Swagger_services_can_be_registered()
        {
            var services = new ServiceCollection();

            Assert.DoesNotThrow(() => services.AddSwaggerGen());
        }
    }
}
