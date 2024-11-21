using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

public class TestConfig
{
    protected readonly IConfiguration _config;
    public TestConfig()
    {
        _config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("testsettings.json", optional: false, reloadOnChange: true)
            .Build();
    }
}
