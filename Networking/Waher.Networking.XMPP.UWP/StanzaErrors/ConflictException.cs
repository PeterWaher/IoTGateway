using System.Xml;

namespace Waher.Networking.XMPP.StanzaErrors
{
	/// <summary>
	/// Access cannot be granted because an existing resource exists with the same name or address; the associated error type SHOULD be "cancel".
	/// </summary>
	public class ConflictException : StanzaCancelExceptionException
	{
		private readonly string[] alternatives;

		/// <summary>
		/// Access cannot be granted because an existing resource exists with the same name or address; the associated error type SHOULD be "cancel".
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="Stanza">Stanza causing exception.</param>
		public ConflictException(string Message, XmlElement Stanza)
			: base(string.IsNullOrEmpty(Message) ? "Conflict." : Message, Stanza)
		{
			this.alternatives = StreamErrors.ConflictException.GetAlternatives(Stanza);
		}

		/// <inheritdoc/>
		public override string ErrorStanzaName
		{
			get { return "conflict"; }
		}

		/// <summary>
		/// If response contains alternatives.
		/// </summary>
		public bool HasAlternatives => !(this.alternatives is null) && this.alternatives.Length > 0;

		/// <summary>
		/// Alternatives
		/// </summary>
		public string[] Alternatives => this.alternatives;
	}
}
