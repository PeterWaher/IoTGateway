using System;
using Waher.Networking.XMPP;

namespace Waher.Networking.XMPP.Events
{
	/// <summary>
	/// Event arguments for message correction events (XEP-0308).
	/// </summary>
	public class MessageCorrectionEventArgs : EventArgs
	{
		/// <summary>
		/// Creates event arguments for a message correction.
		/// </summary>
		/// <param name="message">Message event.</param>
		/// <param name="replaceId">Id of the stanza being replaced.</param>
		/// <param name="newBody">Corrected body.</param>
		/// <param name="newSubject">Corrected subject.</param>
		public MessageCorrectionEventArgs(MessageEventArgs message, string replaceId, string newBody, string newSubject)
		{
			this.Message = message;
			this.ReplaceId = replaceId ?? string.Empty;
			this.NewBody = newBody ?? string.Empty;
			this.NewSubject = newSubject ?? string.Empty;
		}

		/// <summary>
		/// Message event associated with the correction.
		/// </summary>
		public MessageEventArgs Message { get; }

		/// <summary>
		/// Id of the stanza being replaced.
		/// </summary>
		public string ReplaceId { get; }

		/// <summary>
		/// Corrected body.
		/// </summary>
		public string NewBody { get; }

		/// <summary>
		/// Corrected subject.
		/// </summary>
		public string NewSubject { get; }

		/// <summary>
		/// Message type.
		/// </summary>
		public MessageType MessageType => this.Message?.Type ?? MessageType.Normal;

		/// <summary>
		/// From JID.
		/// </summary>
		public string From => this.Message?.From;

		/// <summary>
		/// To JID.
		/// </summary>
		public string To => this.Message?.To;
	}
}
