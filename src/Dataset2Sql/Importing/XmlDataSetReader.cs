using System.Data;

namespace Develix.Dataset2Sql.Importing;

public sealed class XmlDataSetReader : IDataSetXmlReader
{
    public DataSet Read(string xmlPath)
    {
        var dataSet = new DataSet();
        dataSet.ReadXml(xmlPath);
        return dataSet;
    }
}
