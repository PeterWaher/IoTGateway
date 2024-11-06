using Waher.Things;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Event arguments for data source events.
	/// </summary>
	public class DataSourceEventArgs
	{
		private readonly IDataSource dataSource;

		internal DataSourceEventArgs(IDataSource DataSource)
		{
			this.dataSource = DataSource;
		}

		/// <summary>
		/// DataSource of the concentrator server.
		/// </summary>
		public IDataSource DataSource => this.dataSource;
	}
}
