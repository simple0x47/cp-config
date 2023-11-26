using Core.Secrets;
using Microsoft.Extensions.Configuration;

namespace Cuplan.Config.UnitTests;

public class TestBase
{
    public TestBase()
    {
        ProjectRootPath = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
        SolutionRootPath = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.Parent.FullName;
        TestDataPath = $"{ProjectRootPath}/TestData";
        SecretsManager = new BitwardenSecretsManager(null);
        Config = new ConfigurationBuilder().AddJsonFile($"{SolutionRootPath}/Config/appsettings.Development.json")
            .Build();
    }

    protected string ProjectRootPath { get; }

    protected string SolutionRootPath { get; }
    protected string TestDataPath { get; }
    protected ISecretsManager SecretsManager { get; }
    protected IConfiguration Config { get; }
}