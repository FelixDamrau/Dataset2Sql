using System.Data;

namespace Develix.Dataset2Sql.Tests.SqlGeneration;

public class CreateTableSqlBuilderTests
{
    [Test]
    public async Task Build_GeneratesCreateTableSql_ForSimpleTable()
    {
        var table = new DataTable("People");
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("Name", typeof(string));

        var sql = CreateTableSqlBuilder.Build(table, BracketQuote, SqlTypeMapper.Map);

        await Assert.That(sql).Contains("CREATE TABLE [People] (");
        await Assert.That(sql).Contains("    [Id] INT,");
        await Assert.That(sql).Contains("    [Name] NVARCHAR(MAX)");
        await Assert.That(sql.TrimEnd()).EndsWith(")");
    }

    [Test]
    public async Task Build_GeneratesCreateTableSql_ForEscapedIdentifiers()
    {
        var table = new DataTable("Order Details");
        table.Columns.Add("Order", typeof(long));
        table.Columns.Add("Customer Name", typeof(string));

        var sql = CreateTableSqlBuilder.Build(table, BracketQuote, SqlTypeMapper.Map);

        await Assert.That(sql).Contains("CREATE TABLE [Order Details] (");
        await Assert.That(sql).Contains("    [Order] BIGINT,");
        await Assert.That(sql).Contains("    [Customer Name] NVARCHAR(MAX)");
    }

    [Test]
    public async Task Build_Throws_WhenTableIsNull()
    {
        var action = () => CreateTableSqlBuilder.Build(null!, BracketQuote, SqlTypeMapper.Map);

        await Assert.That(action).Throws<ArgumentNullException>();
    }

    private static string BracketQuote(string name) => $"[{name}]";
}
