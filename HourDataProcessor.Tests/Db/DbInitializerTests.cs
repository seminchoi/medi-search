using HourDataProcessor.Db;
using Microsoft.Data.SqlClient;

namespace HourDataProcessor.Tests.Db;

public class DbInitializerTests
{
    [SetUp]
    public void SetUp()
    {
        ConfigHolder.AppConfig.Database = new DatabaseConfig
        {
            DevMode = false,
            ConnectionString =
                "Server=localhost;Database=InitTestLogAppSem;User Id=sa;Password=rootRoot123;TrustServerCertificate=True",
            MasterConnectionString =
                "Server=localhost;Database=master;User Id=sa;Password=rootRoot123;TrustServerCertificate=True"
        };
    }

    [Test(Description = "DB 초기화 작업을 성공한다.")]
    public void TestDbInitializer()
    {
        DbInitializer.Initialize();

        using var connection = new SqlConnection(ConfigHolder.AppConfig.Database.ConnectionString);
        connection.Open();
        connection.Close();
    }

    [TearDown]
    public void TearDown()
    {
        DbInitializer.ExecuteQueryUsingFile(ConfigHolder.AppConfig.Database.MasterConnectionString,
            "destroy-database.sql");
    }
}