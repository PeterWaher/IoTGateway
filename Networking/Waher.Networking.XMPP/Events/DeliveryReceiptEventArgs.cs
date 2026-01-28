using System;
using Waher.Networking.XMPP;

namespace Waher.Networking.XMPP.Events
{
	/// <summary>
	/// Event arguments for delivery receipt events (XEP-0184).
	/// </summary>
	public class DeliveryReceiptEventArgs : EventArgs
	{
		/// <summary>
		/// Creates event arguments for a receipt request or response.
		/// </summary>
		/// <param name="message">Message event.</param>
		/// <param name="id">Referenced stanza id.</param>
		/// <param name="isRequest">If this is a receipt request.</param>
		public DeliveryReceiptEventArgs(MessageEventArgs message, string id, bool isRequest)
		{
			this.Message = message;
			this.Id = id ?? string.Empty;
			this.IsRequest = isRequest;
		}

		/// <summary>
		/// Message event associated with the receipt.
		/// </summary>
		public MessageEventArgs Message { get; }

		/// <summary>
		/// Referenced stanza id.
		/// </summary>
		public string Id { get; }

		/// <summary>
		/// If this event represents a receipt request.
		/// </summary>
		public bool IsRequest { get; }

		/// <summary>
		/// If this event represents a receipt response.
		/// </summary>
		public bool IsReceived => !this.IsRequest;

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
