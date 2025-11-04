namespace Waher.Things
{
	/// <summary>
	/// Interface for collections of data sources.
	/// </summary>
	public interface IDataSources
	{
		/// <summary>
		/// Tries to get a data source.
		/// </summary>
		/// <param name="SourceId">Data Source ID</param>
		/// <param name="DataSource">Data Source, if found.</param>
		/// <returns>If a data source was found with the same ID.</returns>
		bool TryGetDataSource(string SourceId, out IDataSource DataSource);
	}
}
