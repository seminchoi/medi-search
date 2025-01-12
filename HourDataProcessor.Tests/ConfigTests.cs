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
                    "Server=localhost;Database=TestLogApp;User Id=sa;Password=rootRoot123;Pooling=true;Min Pool Size=1; Max Pool Size = 4;TrustServerCertificate=True"));
        });
    }
}