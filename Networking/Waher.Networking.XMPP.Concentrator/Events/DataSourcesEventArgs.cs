using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Delegate for data sources callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void DataSourcesEventHandler(object Sender, DataSourcesEventArgs e);

	/// <summary>
	/// Event arguments for data sources responses.
	/// </summary>
	public class DataSourcesEventArgs : IqResultEventArgs
	{
		private DataSourceReference[] dataSources;

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
