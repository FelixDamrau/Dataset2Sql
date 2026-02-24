namespace Develix.Dataset2Sql.Tests.SqlGeneration;

public class SqlTypeMapperTests
{
    [Test]
    [Arguments(typeof(string), "NVARCHAR(MAX)")]
    [Arguments(typeof(int), "INT")]
    [Arguments(typeof(long), "BIGINT")]
    [Arguments(typeof(decimal), "DECIMAL(18, 6)")]
    [Arguments(typeof(double), "FLOAT")]
    [Arguments(typeof(float), "REAL")]
    [Arguments(typeof(DateTime), "DATETIME")]
    [Arguments(typeof(bool), "BIT")]
    [Arguments(typeof(byte), "TINYINT")]
    [Arguments(typeof(Guid), "UNIQUEIDENTIFIER")]
    [Arguments(typeof(TimeSpan), "NVARCHAR(MAX)")]
    [Arguments(typeof(Type), "NVARCHAR(MAX)")]
    public async Task Map_WhenGivenDotNetType_ThenReturnsExpectedSqlType(Type dotNetType, string expectedSqlType)
    {
        var actual = SqlTypeMapper.Map(dotNetType);

        await Assert.That(actual).IsEqualTo(expectedSqlType);
    }
}
