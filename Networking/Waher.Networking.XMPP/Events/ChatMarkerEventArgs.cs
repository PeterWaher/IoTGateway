using System;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Chat.Markers;

namespace Waher.Networking.XMPP.Events
{
	/// <summary>
	/// Event arguments for chat markers (XEP-0333).
	/// </summary>
	public class ChatMarkerEventArgs : EventArgs
	{
		/// <summary>
		/// Creates event arguments for a chat marker.
		/// </summary>
		/// <param name="message">Message event.</param>
		/// <param name="markerType">Marker type.</param>
		/// <param name="id">Referenced stanza id.</param>
		public ChatMarkerEventArgs(MessageEventArgs message, ChatMarkerType markerType, string id)
		{
			this.Message = message;
			this.MarkerType = markerType;
			this.Id = id ?? string.Empty;
		}

		/// <summary>
		/// Message event associated with the marker.
		/// </summary>
		public MessageEventArgs Message { get; }

		/// <summary>
		/// Marker type.
		/// </summary>
		public ChatMarkerType MarkerType { get; }

		/// <summary>
		/// Referenced stanza id.
		/// </summary>
		public string Id { get; }

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
