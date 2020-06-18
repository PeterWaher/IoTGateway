using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Networking.XMPP.DataForms;

namespace Waher.Networking.XMPP.Search
{
	/// <summary>
	/// Delegate for search result events or callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task SearchResultEventHandler(object Sender, SearchResultEventArgs e);

	/// <summary>
	/// Event arguments for search result responses.
	/// </summary>
	public class SearchResultEventArgs : IqResultEventArgs
	{
		private readonly Dictionary<string, string>[] records;
		private readonly Field[] headers;

		internal SearchResultEventArgs(Dictionary<string, string>[] Records, Field[] Headers, IqResultEventArgs e)
			: base(e)
		{
			this.records = Records;
			this.headers = Headers;
		}

		/// <summary>
		/// Result records.
		/// </summary>
		public Dictionary<string, string>[] Records
		{
			get { return this.records; }
		}

		/// <summary>
		/// Headers
		/// </summary>
		public Field[] Headers
		{
			get { return this.headers; }
		}
	}
}
