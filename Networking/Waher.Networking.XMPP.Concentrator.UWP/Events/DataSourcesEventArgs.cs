using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Concentrator
{
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
		public DataSourceReference[] DataSources => this.dataSources;
	}
}
