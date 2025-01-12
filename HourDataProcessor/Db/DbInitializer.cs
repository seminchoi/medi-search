using Microsoft.Data.SqlClient;

namespace HourDataProcessor.Db;

public static class DbInitializer
{
    public static void Initialize()
    {
        var connectionString = ConfigHolder.AppConfig.Database.ConnectionString;
        var rootPath = AppContext.BaseDirectory;
        var sqlPath = Path.Combine(rootPath, "resources", "init.sql");
        
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