using System.Data;
using System.Text;

namespace Develix.Dataset2Sql;

public static class CreateTableSqlBuilder
{
    public static string Build(DataTable table, Func<string, string> quoteIdentifier, Func<Type, string> mapSqlType)
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(quoteIdentifier);
        ArgumentNullException.ThrowIfNull(mapSqlType);

        var safeTableName = quoteIdentifier(table.TableName);
        var createTableQuery = new StringBuilder();
        createTableQuery.AppendLine($"CREATE TABLE {safeTableName} (");

        for (var i = 0; i < table.Columns.Count; i++)
        {
            var column = table.Columns[i];
            var sqlType = mapSqlType(column.DataType);
            var safeColumnName = quoteIdentifier(column.ColumnName);

            createTableQuery.Append($"    {safeColumnName} {sqlType}");

            if (i < table.Columns.Count - 1)
                createTableQuery.AppendLine(",");
            else
                createTableQuery.AppendLine();
        }

        createTableQuery.AppendLine(")");

        return createTableQuery.ToString();
    }
}
