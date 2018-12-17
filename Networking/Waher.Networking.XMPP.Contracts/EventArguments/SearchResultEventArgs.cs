using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.XMPP.P2P.E2E;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Delegate for Search Result callback methods.
	/// </summary>
	/// <param name="Sender">Sender</param>
	/// <param name="e">Event arguments</param>
	public delegate void SearchResultEventHandler(object Sender, SearchResultEventArgs e);

	/// <summary>
	/// Event arguments for Search Result responses
	/// </summary>
	public class SearchResultEventArgs : IqResultEventArgs
	{
		private readonly string[] references;
		private readonly int offset;
		private readonly int maxCount;
		private readonly bool more;

		/// <summary>
		/// Event arguments for Search Result responses
		/// </summary>
		/// <param name="e">IQ response event arguments.</param>
		/// <param name="Offset">Result starts with the response at this offset into result set.</param>
		/// <param name="MaxCount">Result is limited to this number of items.</param>
		/// <param name="More">If more results are available on the server.</param>
		/// <param name="References">Search Result.</param>
		public SearchResultEventArgs(IqResultEventArgs e, int Offset, int MaxCount, bool More, string[] References)
			: base(e)
		{
			this.offset = Offset;
			this.maxCount = MaxCount;
			this.more = More;
			this.references = References;
		}

		/// <summary>
		/// Result starts with the response at this offset into result set.
		/// </summary>
		public int Offset => this.offset;

		/// <summary>
		/// Result is limited to this number of items.
		/// </summary>
		public int MaxCount => this.maxCount;

		/// <summary>
		/// If more results are available on the server.
		/// </summary>
		public bool More => this.more;

		/// <summary>
		/// Contract references
		/// </summary>
		public string[] References => this.references;
	}
}
