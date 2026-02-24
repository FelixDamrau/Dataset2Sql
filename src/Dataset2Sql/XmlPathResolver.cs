namespace Develix.Dataset2Sql;

public static class XmlPathResolver
{
    public static string Resolve(string? xmlFilePath, bool isInputRedirected, Func<string> promptForXmlPath)
    {
        if (!string.IsNullOrWhiteSpace(xmlFilePath))
            return xmlFilePath.Trim('"', '\'');

        if (isInputRedirected)
            throw new InvalidOperationException("XML file path is required in non-interactive mode. Use --xml.");

        return promptForXmlPath().Trim('"', '\'');
    }
}
