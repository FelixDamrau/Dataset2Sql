namespace Develix.Dataset2Sql;

public static class SqlTypeMapper
{
    public static string Map(Type dotNetType)
    {
        return dotNetType switch
        {
            _ when dotNetType == typeof(string) => "NVARCHAR(MAX)",
            _ when dotNetType == typeof(int) => "INT",
            _ when dotNetType == typeof(long) => "BIGINT",
            _ when dotNetType == typeof(decimal) => "DECIMAL(18, 6)",
            _ when dotNetType == typeof(double) => "FLOAT",
            _ when dotNetType == typeof(float) => "REAL",
            _ when dotNetType == typeof(DateTime) => "DATETIME",
            _ when dotNetType == typeof(bool) => "BIT",
            _ when dotNetType == typeof(byte) => "TINYINT",
            _ when dotNetType == typeof(Guid) => "UNIQUEIDENTIFIER",
            _ => "NVARCHAR(MAX)"
        };
    }
}
