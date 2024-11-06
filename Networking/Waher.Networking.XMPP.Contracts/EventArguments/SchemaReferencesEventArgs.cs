using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Contracts.EventArguments
{
	/// <summary>
	/// Event arguments for Schema References responses
	/// </summary>
	public class SchemaReferencesEventArgs : IqResultEventArgs
	{
		private readonly SchemaReference[] references;

		/// <summary>
		/// Event arguments for Schema References responses
		/// </summary>
		/// <param name="e">IQ response event arguments.</param>
		/// <param name="References">Schema References.</param>
		public SchemaReferencesEventArgs(IqResultEventArgs e, SchemaReference[] References)
			: base(e)
		{
			this.references = References;
		}

		/// <summary>
		/// Schema References
		/// </summary>
		public SchemaReference[] References => this.references;
	}
}
