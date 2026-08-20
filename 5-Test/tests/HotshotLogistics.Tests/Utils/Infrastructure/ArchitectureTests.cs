using HotshotLogistics.Application.Services;
using NetArchTest.Rules;
namespace HotshotLogistics.Tests.Utils.Infrastructure
{
    /// <summary>
    /// Architecture rules and layering tests.
    /// </summary>
    public class ArchitectureTests
    {
        private const string s_presentation = "HotshotLogistics.Api";
        private const string s_infrastructure = "HotshotLogistics.Infrastructure";
        private const string s_application = "HotshotLogistics.Application";
        private const string s_data = "HotshotLogistics.Data";

        [Fact]
        public void Domain_should_not_depend_on_other_layers()
        {
            var result = Types.InAssembly(typeof(HotshotLogistics.Domain.Entities.Driver).Assembly)
                .ShouldNot()
                .HaveDependencyOnAny(s_application, s_data, s_infrastructure, s_presentation)
                .GetResult();

            Assert.True(result.IsSuccessful, string.Join(',', result.FailingTypeNames ?? Array.Empty<string>()));
        }

        [Fact]
        public void Application_should_not_depend_on_presentation()
        {
            var result = Types.InAssembly(typeof(DriverService).Assembly)
                .ShouldNot()
                .HaveDependencyOn(s_presentation)
                .GetResult();

            Assert.True(result.IsSuccessful, string.Join(',', result.FailingTypeNames ?? Array.Empty<string>()));
        }

        [Fact]
        public void Repositories_should_be_internal()
        {
            var result = Types.InAssembly(typeof(DriverRepository).Assembly)
                .That().HaveNameEndingWith("Repository")
                .Should().NotBePublic()
                .GetResult();

            Assert.True(result.IsSuccessful, string.Join(',', result.FailingTypeNames ?? Array.Empty<string>()));
        }
    }
}
