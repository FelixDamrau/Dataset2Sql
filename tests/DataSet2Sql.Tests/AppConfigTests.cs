using System.Text.Json;

namespace Develix.DataSet2Sql.Tests;

public class AppConfigTests
{
    [Test]
    public async Task CreateDefaultConfigJson_WhenCalled_ThenContainsDatabaseSettingsSection()
    {
        var json = AppConfig.CreateDefaultConfigJson();
        using var document = JsonDocument.Parse(json);

        var hasSection = document.RootElement.TryGetProperty("DatabaseSettings", out _);

        await Assert.That(hasSection).IsTrue();
    }
}
