using System.Net.Http.Headers;
using Core.Secrets;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;
using Xunit.Abstractions;

namespace Cuplan.Config.IntegrationTests.Controllers;

public class TestBase : IClassFixture<WebApplicationFactory<Program>>
{
    public TestBase(WebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        ProjectRootPath = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
        SolutionRootPath = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.Parent.FullName;
        TestDataPath = $"{ProjectRootPath}/TestData";
        SecretsManager = new BitwardenSecretsManager(null);
        Config = new ConfigurationBuilder().AddJsonFile($"{SolutionRootPath}/Config/appsettings.Development.json")
            .Build();

        string apiAccessToken = SecretsManager.Get(Config["ApiTesting:AccessTokenSecret"])!;
        Client = factory.CreateClient();
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiAccessToken);

        Output = output;
    }

    protected string ProjectRootPath { get; }

    protected string SolutionRootPath { get; }
    protected string TestDataPath { get; }
    protected ISecretsManager SecretsManager { get; }
    protected IConfiguration Config { get; }
    protected ITestOutputHelper Output { get; }
    protected HttpClient Client { get; }
}