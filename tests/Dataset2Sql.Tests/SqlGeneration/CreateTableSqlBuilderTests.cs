using System.Data;

namespace Develix.Dataset2Sql.Tests.SqlGeneration;

public class CreateTableSqlBuilderTests
{
    [Test]
    public async Task Build_WhenTableHasSimpleColumns_ThenGeneratesCreateTableSql()
    {
        var table = new DataTable("MARA");
        table.Columns.Add("MATNR", typeof(string));
        table.Columns.Add("BRGEW", typeof(decimal));

        var sql = CreateTableSqlBuilder.Build(table, BracketQuote, SqlTypeMapper.Map);

        await Assert.That(sql).Contains("CREATE TABLE [MARA] (");
        await Assert.That(sql).Contains("    [MATNR] NVARCHAR(MAX),");
        await Assert.That(sql).Contains("    [BRGEW] DECIMAL(18, 6)");
        await Assert.That(sql.TrimEnd()).EndsWith(")");
    }

    [Test]
    public async Task Build_WhenTableHasEscapedIdentifiers_ThenGeneratesCreateTableSql()
    {
        var table = new DataTable("What ' WAIT?");
        table.Columns.Add("How -- did", typeof(long));
        table.Columns.Add("this even happen?", typeof(string));

        var sql = CreateTableSqlBuilder.Build(table, BracketQuote, SqlTypeMapper.Map);

        await Assert.That(sql).Contains("CREATE TABLE [What ' WAIT?] (");
        await Assert.That(sql).Contains("    [How -- did] BIGINT,");
        await Assert.That(sql).Contains("    [this even happen?] NVARCHAR(MAX)");
    }

    [Test]
    public async Task Build_WhenTableIsNull_ThenThrows()
    {
        static string action() => CreateTableSqlBuilder.Build(null!, BracketQuote, SqlTypeMapper.Map);

        await Assert.That(action).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Build_WhenQuoteIdentifierIsNull_ThenThrows()
    {
        var table = new DataTable("MARM");
        string action() => CreateTableSqlBuilder.Build(table, null!, SqlTypeMapper.Map);

        await Assert.That(action).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Build_WhenMapSqlTypeIsNull_ThenThrows()
    {
        var table = new DataTable("MAKT");
        string action() => CreateTableSqlBuilder.Build(table, BracketQuote, null!);

        await Assert.That(action).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Build_WhenTableHasNoColumns_ThenGeneratesCreateTableSql()
    {
        var table = new DataTable("Empty");

        var sql = CreateTableSqlBuilder.Build(table, BracketQuote, SqlTypeMapper.Map);

        await Assert.That(sql).Contains("CREATE TABLE [Empty] (");
        await Assert.That(sql).DoesNotContain("    [");
        await Assert.That(sql.TrimEnd()).EndsWith(")");
    }

    private static string BracketQuote(string name) => $"[{name}]";
}
