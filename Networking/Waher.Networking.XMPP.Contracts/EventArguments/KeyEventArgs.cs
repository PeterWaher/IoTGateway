using System;
using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Contracts.EventArguments
{
	/// <summary>
	/// Event arguments for key responses
	/// </summary>
	public class KeyEventArgs : IqResultEventArgs
	{
		/// <summary>
		/// Event arguments for key responses
		/// </summary>
		/// <param name="e">IQ response event arguments.</param>
		public KeyEventArgs(KeyEventArgs e)
			: this(e, e.Key, e.ValidFrom, e.ValidTo)
		{
		}

		/// <summary>
		/// Event arguments for key responses
		/// </summary>
		/// <param name="e">IQ response event arguments.</param>
		/// <param name="Key">Key.</param>
		/// <param name="From">Timestamp indicating when the public key was created, in UTC.</param>
		/// <param name="To">Timestamp indicating when the public key was replaced or 
		/// changed, and no longer used for creating signatures, in UTC.</param>
		public KeyEventArgs(IqResultEventArgs e, IE2eEndpoint Key, 
			DateTime From, DateTime? To)
			: base(e)
		{
			this.Key = Key;
			this.ValidFrom = From;
			this.ValidTo = To;
		}

		/// <summary>
		/// Event arguments for key responses
		/// </summary>
		/// <param name="Key">Key.</param>
		/// <param name="From">Timestamp indicating when the public key was created, in UTC.</param>
		/// <param name="To">Timestamp indicating when the public key was replaced or 
		/// changed, and no longer used for creating signatures, in UTC.</param>
		/// <param name="State">State object.</param>
		public KeyEventArgs(IE2eEndpoint Key, object State, DateTime From, DateTime? To)
			: base(new IqResultEventArgs(null, string.Empty, string.Empty, string.Empty, true, State))
		{
			this.Key = Key;
			this.ValidFrom = From;
			this.ValidTo = To;
		}

		/// <summary>
		/// Public key of server endpoint.
		/// </summary>
		public IE2eEndpoint Key { get; }

		/// <summary>
		/// Timestamp indicating when the public key was created, in UTC.
		/// </summary>
		public DateTime ValidFrom { get; }

		/// <summary>
		/// Timestamp indicating when the public key was replaced or changed, and no 
		/// longer used for creating signatures, in UTC.
		/// </summary>
		public DateTime? ValidTo { get; }
	}
}
