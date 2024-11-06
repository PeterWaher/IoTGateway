using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Contracts.EventArguments
{
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
