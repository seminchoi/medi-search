namespace HourDataProcessor.Tests;

public class ConfigTests
{
    [Test]
    public void TestConfigInit()
    {
        var config = ConfigHolder.AppConfig;
        Assert.Multiple(() =>
        {
            Assert.That(config.Database?.ConnectionString,
                Is.EqualTo(
                    "Server=localhost;Database=TestLogApp;User Id=sa;Password=rootRoot123;;TrustServerCertificate=True"));
            Assert.That(config.CsvSource?.BasePath,
                Is.EqualTo(
                    "resources"));
            Assert.That(config.CsvSource?.FileNames,
                Has.Count.EqualTo(3));
        });
    }
}