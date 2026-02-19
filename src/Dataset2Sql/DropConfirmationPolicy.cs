namespace Develix.Dataset2Sql;

public static class DropConfirmationPolicy
{
    public static bool ShouldDrop(string dbName, bool autoConfirmDrop, Func<string> readConfirmation)
    {
        if (autoConfirmDrop)
            return true;

        var response = readConfirmation();
        return string.Equals(response, dbName, StringComparison.Ordinal);
    }
}
