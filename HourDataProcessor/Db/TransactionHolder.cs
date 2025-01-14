using Microsoft.Data.SqlClient;

namespace HourDataProcessor.Db;

public static class TransactionHolder
{
    public static readonly ThreadLocal<SqlConnection?> Connection = new();
    public static readonly ThreadLocal<SqlTransaction?> Transaction = new();
}