using System;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Delegate for data sources callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task DataSourcesEventHandler(object Sender, DataSourcesEventArgs e);

	/// <summary>
	/// Event arguments for data sources responses.
	/// </summary>
	public class DataSourcesEventArgs : IqResultEventArgs
	{
		private readonly DataSourceReference[] dataSources;

		internal DataSourcesEventArgs(DataSourceReference[] DataSources, IqResultEventArgs Response)
			: base(Response)
		{
			this.dataSources = DataSources;
		}

		/// <summary>
		/// DataSources of the concentrator server.
		/// </summary>
		public DataSourceReference[] DataSources
		{
			get { return this.dataSources; }
		}
	}
}
