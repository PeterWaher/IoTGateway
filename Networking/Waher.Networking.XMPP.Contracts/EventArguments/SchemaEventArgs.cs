using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.XMPP.P2P.E2E;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Delegate for schema callback methods.
	/// </summary>
	/// <param name="Sender">Sender</param>
	/// <param name="e">Event arguments</param>
	public delegate void SchemaEventHandler(object Sender, SchemaEventArgs e);

	/// <summary>
	/// Event arguments for schema responses
	/// </summary>
	public class SchemaEventArgs : IqResultEventArgs
	{
		private readonly byte[] schema;

		/// <summary>
		/// Event arguments for schema responses
		/// </summary>
		/// <param name="e">IQ response event arguments.</param>
		/// <param name="Schema">XML Schema.</param>
		public SchemaEventArgs(IqResultEventArgs e, byte[] Schema)
			: base(e)
		{
			this.schema = Schema;
		}

		/// <summary>
		/// Binary XML Schema.
		/// </summary>
		public byte[] Schema => this.schema;
	}
}
