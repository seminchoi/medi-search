namespace HourDataProcessor.Tests;

public class ConfigTests
{
    [Test]
    public void TestConfigInit()
    {
        var config = ConfigHolder.AppConfig;
        Assert.Multiple(() =>
        {
            Assert.That(config.Database?.ConnectionString, Is.Not.Null);
            Assert.That(config.Database?.ConnectionString,
                Is.EqualTo(
                    "Server=localhost;Database=TestLogApp;User Id=sa;Password=rootRoot123;;TrustServerCertificate=True"));
        });
    }
}