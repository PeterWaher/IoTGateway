using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Waher.Networking.HTTP.JsonRpc;
using Waher.Networking.HTTP.JsonRpc.Transports;
using Waher.Networking.HTTP.OAuth;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Events;
using Waher.Runtime.Collections;

namespace Waher.Mcp.Xmpp
{
	/// <summary>
	/// Contains information about an MCP XMPP session.
	/// </summary>
	public class McpXmppExtension : IXmppExtension
	{
		private readonly IJsonRpcCall firstCall;
		private readonly HashSet<string> sessionIds = new HashSet<string>();
		private readonly Dictionary<string, PresenceEventArgs> subscriptionRequests =
			new Dictionary<string, PresenceEventArgs>(StringComparer.InvariantCultureIgnoreCase);
		private readonly Dictionary<string,  MessageRec> messagesById =
			new Dictionary<string, MessageRec>();
		private readonly ChunkedList<MessageRec> messages = new ChunkedList<MessageRec>();

		/// <summary>
		/// Contains information about an MCP XMPP session.
		/// </summary>
		/// <param name="Call">Request that generated the connection.</param>
		/// <param name="SessionId">Session creating the object.</param>
		public McpXmppExtension(IJsonRpcCall Call, string SessionId)
		{
			this.firstCall = Call;
			this.sessionIds.Add(SessionId);
		}

		/// <summary>
		/// Implemented extensions.
		/// </summary>
		public string[] Extensions => new string[] { "MCP" };

		/// <summary>
		/// First call that generated the connection.
		/// </summary>
		public IJsonRpcCall FirstCall => this.firstCall;

		/// <summary>
		/// Registered session IDs
		/// </summary>
		public string[] SessionIds
		{
			get
			{
				lock (this.sessionIds)
				{
					string[] Result = new string[this.sessionIds.Count];
					this.sessionIds.CopyTo(Result);
					return Result;
				}
			}
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose()"/>
		/// </summary>
		public void Dispose()
		{
		}

		/// <summary>
		/// Registers an MCP Session ID using the connection.
		/// </summary>
		/// <param name="SessionId">MCP Session ID</param>
		public void Register(string SessionId)
		{
			lock (this.sessionIds)
			{
				this.sessionIds.Add(SessionId);
			}
		}

		/// <summary>
		/// If an MCP Session ID has been registered using the connection.
		/// </summary>
		/// <param name="Session">JSON-RPC Session</param>
		public bool IsRegistered(IJsonRpcSession? Session)
		{
			if (Session is null)
				return false;
			else
				return this.IsRegistered(Session.SessionId);
		}

		/// <summary>
		/// If an MCP Session ID has been registered using the connection.
		/// </summary>
		/// <param name="SessionId">MCP Session ID</param>
		public bool IsRegistered(string SessionId)
		{
			lock (this.sessionIds)
			{
				return this.sessionIds.Contains(SessionId);
			}
		}

		/// <summary>
		/// Adds a presence subscription request.
		/// </summary>
		/// <param name="e">Event arguments.</param>
		public void Add(PresenceEventArgs e)
		{
			lock (this.subscriptionRequests)
			{
				this.subscriptionRequests[e.FromBareJID] = e;
			}
		}

		/// <summary>
		/// Tries to get a pending presence subscription request for a given bare JID. 
		/// If found, the request is removed from the pending requests.
		/// </summary>
		/// <param name="BareJid">The bare JID of the presence subscription request.</param>
		/// <param name="e">The event arguments of the presence subscription request.</param>
		/// <returns>True if a pending presence subscription request was found; otherwise, 
		/// false.</returns>
		public bool TryGetPresenceSubscriptionRequest(string BareJid,
			[NotNullWhen(true)] out PresenceEventArgs? e)
		{
			lock (this.subscriptionRequests)
			{
				if (this.subscriptionRequests.TryGetValue(BareJid, out e))
				{
					this.subscriptionRequests.Remove(BareJid);
					return true;
				}
				else
					return false;
			}
		}

		/// <summary>
		/// Registers a new incoming message.
		/// </summary>
		/// <param name="Message">Message received.</param>
		/// <returns>Message ID</returns>
		public string Register(MessageEventArgs Message)
		{
			lock (this.messages)
			{
				string Id;

				do
				{
					Id = OAuth2Environment.GenerateRandomCode(16);
				}
				while (this.messagesById.ContainsKey(Id));

				MessageRec Rec = new MessageRec(Id, Message);

				this.messagesById[Id] = Rec;
				this.messages.Add(Rec);

				return Id;
			}
		}

		private class MessageRec
		{
			public MessageRec(string MessageId, MessageEventArgs Message)
			{
				this.MessageId = MessageId;
				this.Message = Message;
			}

			public MessageEventArgs Message;
			public string MessageId;

			public override bool Equals(object Obj)
			{
				return Obj is MessageRec Rec && this.MessageId.Equals(Rec.MessageId);
			}

			public override int GetHashCode()
			{
				return this.MessageId.GetHashCode();
			}
		}

		/// <summary>
		/// Tries to retrieve a message associated with a specified message identifier.
		/// </summary>
		/// <param name="MessageId">The identifier of the message to retrieve.</param>
		/// <param name="Pop">If the message should be removed, if found.</param>
		/// <returns>The message event arguments if found; otherwise, null.</returns>
		public MessageEventArgs? TryGetMessage(string MessageId, bool Pop)
		{
			lock (this.messages)
			{
				if (this.messagesById.TryGetValue(MessageId, out MessageRec? Rec))
				{
					if (Pop)
					{
						this.messagesById.Remove(MessageId);
						this.messages.Remove(Rec);
					}

					return Rec.Message;
				}
				else
					return null;
			}
		}

		/// <summary>
		/// Tries to retrieve the first message in the queue.
		/// </summary>
		/// <param name="Pop">If the message should be removed, if found.</param>
		/// <returns>The message event arguments if found; otherwise, null.</returns>
		public MessageEventArgs? TryGetFirstMessage(bool Pop)
		{
			lock (this.messages)
			{
				if (!this.messages.HasFirstItem)
					return null;

				if (!Pop)
					return this.messages.FirstItem.Message;
				else
				{
					MessageRec? Rec = this.messages.RemoveFirst();

					this.messagesById.Remove(Rec.MessageId);
					this.messages.Remove(Rec);
				
					return Rec.Message;
				}
			}
		}

		/// <summary>
		/// Gets the message identifiers of all registered messages, in order of reception.
		/// </summary>
		/// <returns>Message IDs in order of reception.</returns>
		public string[] GetMessageIds()
		{
			lock (this.messages)
			{
				int i, c = this.messages.Count;
				string[] Result = new string[c];

				for (i = 0; i < c; i++)
					Result[i] = this.messages[i].MessageId;

				return Result;
			}
		}
	}
}
