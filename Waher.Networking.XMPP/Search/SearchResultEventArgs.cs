using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Networking.XMPP.DataForms;

namespace Waher.Networking.XMPP.Search
{
	/// <summary>
	/// Delegate for search result events or callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void SearchResultEventHandler(object Sender, SearchResultEventArgs e);

	/// <summary>
	/// Event arguments for search result responses.
	/// </summary>
	public class SearchResultEventArgs : IqResultEventArgs
	{
		private Dictionary<string, string>[] records;

		internal SearchResultEventArgs(Dictionary<string, string>[] Records, IqResultEventArgs e)
			: base(e)
		{
			this.records = Records;
		}

		/// <summary>
		/// Result records.
		/// </summary>
		public Dictionary<string, string>[] Records
		{
			get { return this.records; }
		}

	}
}
