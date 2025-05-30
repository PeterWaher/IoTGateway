using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Contracts.EventArguments
{
	/// <summary>
	/// Event arguments for key responses
	/// </summary>
	public class KeyEventArgs : IqResultEventArgs
	{
		private readonly IE2eEndpoint key;

		/// <summary>
		/// Event arguments for key responses
		/// </summary>
		/// <param name="e">IQ response event arguments.</param>
		public KeyEventArgs(KeyEventArgs e)
			: this(e, e.key)
		{
		}

		/// <summary>
		/// Event arguments for key responses
		/// </summary>
		/// <param name="e">IQ response event arguments.</param>
		/// <param name="Key">Key.</param>
		public KeyEventArgs(IqResultEventArgs e, IE2eEndpoint Key)
			: base(e)
		{
			this.key = Key;
		}

		/// <summary>
		/// Event arguments for key responses
		/// </summary>
		/// <param name="Key">Key.</param>
		/// <param name="State">State object.</param>
		public KeyEventArgs(IE2eEndpoint Key, object State)
			: base(new IqResultEventArgs(null, string.Empty, string.Empty, string.Empty, true, State))
		{
			this.key = Key;
		}

		/// <summary>
		/// Public key of server endpoint.
		/// </summary>
		public IE2eEndpoint Key => this.key;
	}
}
