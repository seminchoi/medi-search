namespace HourDataProcessor.Tests;

public class AppConfigTests
{
    [Fact(DisplayName = "Config를 성공적으로 초기화한다.")]
    public void TestAppConfigInit()
    {
        var config = ConfigHolder.AppConfig;
        Assert.NotNull(config.Database.ConnectionString);
        Assert.Equal("Server=localhost;Database=TestLogApp;User Id=sa;Password=rootRoot123;Pooling=true;Min Pool Size=1; Max Pool Size = 4;TrustServerCertificate=True", 
            config.Database.ConnectionString);
    }
}