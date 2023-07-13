using System.Threading.Tasks;
using Waher.Things;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Delegate for data source callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task DataSourceEventHandler(object Sender, DataSourceEventArgs e);

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
