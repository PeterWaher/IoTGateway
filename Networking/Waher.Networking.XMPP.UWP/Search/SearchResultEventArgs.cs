using System.Collections.Generic;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Search
{
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
		public Dictionary<string, string>[] Records => this.records;

		/// <summary>
		/// Headers
		/// </summary>
		public Field[] Headers => this.headers;
	}
}
