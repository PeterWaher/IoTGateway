using System;
using Waher.Networking.XMPP.Chat.ChatStates;

namespace Waher.Networking.XMPP.Events
{
    /// <summary>
    /// Provides data for events that report a change in chat state, such as when a user is typing or has paused typing.
    /// </summary>
    /// <remarks>Use this class with event handlers that need information about the current chat state and the
    /// associated message event. The chat state indicates the user's activity in the chat, such as composing, paused,
    /// or inactive.</remarks>
	public class ChatStateEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the ChatStateEventArgs class with the specified message and chat state.
		/// </summary>
		/// <param name="message">The message event data associated with the chat state change. Cannot be null.</param>
		/// <param name="state">The new chat state represented by this event.</param>
		/// <param name="previousState">The previous chat state observed for the same contact.</param>
		public ChatStateEventArgs(MessageEventArgs message, ChatState state, ChatState previousState)
		{
			this.Message = message;
			this.State = state;
			this.PreviousState = previousState;
		}

		/// <summary>
		/// Message event associated with the chat state change.
		/// </summary>
		public MessageEventArgs Message { get; }

		/// <summary>
		/// New chat state.
		/// </summary>
		public ChatState State { get; }

		/// <summary>
		/// Previous chat state tracked for the same bare JID.
		/// </summary>
		public ChatState PreviousState { get; }
	}

}
