using Microsoft.Data.SqlClient;

namespace HourDataProcessor.Db;

public static class DbInitializer
{
    public static void Initialize()
    {
        var dbConfig = ConfigHolder.AppConfig.Database;
        if (dbConfig.DevMode)
        {
            ExecuteQueryUsingFile(dbConfig.MasterConnectionString, "destroy-database.sql");
        }
        ExecuteQueryUsingFile(dbConfig.MasterConnectionString, "init-database.sql");
        ExecuteQueryUsingFile(dbConfig.ConnectionString, "init-table.sql");
    }

    public static void ExecuteQueryUsingFile(string connectionString, string filePath)
    {
        var rootPath = AppContext.BaseDirectory;
        var sqlPath = Path.Combine(rootPath, "resources", filePath);
        
        if (!File.Exists(sqlPath))
        {
            throw new FileNotFoundException("DDL file not found", sqlPath);
        }
       
        var sqlContent = File.ReadAllText(sqlPath);

        using var connection = new SqlConnection(connectionString);
        connection.Open();

        using var command = new SqlCommand(sqlContent, connection);
        command.ExecuteNonQuery();
    }
}