using Waher.Runtime.Inventory;

namespace Waher.Script.Persistence.SQL
{
    /// <summary>
    /// Interface for named data sources that can be used in SQL statements.
    /// </summary>
    public interface INamedDataSource : IDataSource, IProcessingSupport<string>
	{
	}
}
