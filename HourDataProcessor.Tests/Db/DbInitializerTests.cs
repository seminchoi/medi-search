using HourDataProcessor.Db;
using Microsoft.Data.SqlClient;

namespace HourDataProcessor.Tests.Db;

public class DbInitializerTests
{
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
        DbInitializer.ExecuteQueryUsingFile(ConfigHolder.AppConfig.Database.MasterConnectionString, "destroy-database.sql");
    }
}