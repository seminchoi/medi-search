using Microsoft.Data.SqlClient;

namespace HourDataProcessor.Db;

public static class DbInitializer
{
    public static void Initialize()
    {
        ExecuteQueryUsingFile(ConfigHolder.AppConfig.Database.InitConnectionString, "init-database.sql");
        ExecuteQueryUsingFile(ConfigHolder.AppConfig.Database.ConnectionString, "init-table.sql");
    }

    public static void ExecuteQueryUsingFile(string connectionString, string fileName)
    {
        var rootPath = AppContext.BaseDirectory;
        var sqlPath = Path.Combine(rootPath, "resources", fileName);
        
        if (!File.Exists(sqlPath))
        {
            throw new FileNotFoundException("DDL file not found", sqlPath);
        }
       
        var sqlContent = File.ReadAllText(sqlPath);
        
        try
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();

            using var command = new SqlCommand(sqlContent, connection);
            command.ExecuteNonQuery();
            Console.WriteLine("DDL executed successfully.");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}