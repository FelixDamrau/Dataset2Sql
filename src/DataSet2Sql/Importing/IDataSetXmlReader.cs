using System.Data;

namespace Develix.DataSet2Sql.Importing;

public interface IDataSetXmlReader
{
    DataSet Read(string xmlPath);
}
