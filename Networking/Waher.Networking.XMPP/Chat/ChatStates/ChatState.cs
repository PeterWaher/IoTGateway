using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.Chat.ChatStates
{
    /// <summary>
    /// Specifies the possible states of a chat participant in a messaging conversation. XEP-0085 Chat State
    /// </summary>
    /// <remarks>These states indicate the participant's current activity, such as whether they are actively
    /// engaged, composing a message, temporarily paused, inactive, or have left the conversation. The values are
    /// typically used to provide real-time feedback to other participants in chat applications.</remarks>
    public enum ChatState
    {
        /// <summary>
        /// User is actively participating in the chat session.
        /// </summary>
        /// <remarks>User accepts an initial content message, sends a content message, gives focus to the chat session interface (perhaps after being inactive), or is otherwise paying attention to the conversation.</remarks>
        Active,
        /// <summary>
        /// User is composing a message.
        /// </summary>
        /// <remarks>User is actively interacting with a message input interface specific to this chat session (e.g., by typing in the input area of a chat window).</remarks>
        Composing,
        /// <summary>
        /// User had been composing but now has stopped.
        /// </summary>
        /// <remarks>User was composing but has not interacted with the message input interface for a short period of time (e.g., 30 seconds).</remarks>
        Paused,
        /// <summary>
        /// User has not been actively participating in the chat session.
        /// </summary>
        /// <remarks>User has not interacted with the chat session interface for an intermediate period of time (e.g., 2 minutes).</remarks>
        Inactive,
        /// <summary>
        /// User has effectively ended their participation in the chat session.
        /// </summary>
        /// <remarks>User has not interacted with the chat session interface, system, or device for a relatively long period of time (e.g., 10 minutes).</remarks>
        Gone
    }
}
