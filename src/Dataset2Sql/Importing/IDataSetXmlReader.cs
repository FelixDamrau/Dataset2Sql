using System.Data;

namespace Develix.Dataset2Sql.Importing;

public interface IDataSetXmlReader
{
    DataSet Read(string xmlPath);
}
