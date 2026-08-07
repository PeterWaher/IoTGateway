using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Waher.Networking.HTTP;
using Waher.Networking.HTTP.JsonRpc;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Events;

namespace Waher.Mcp.Xmpp
{
	/// <summary>
	/// Contains information about an MCP XMPP session.
	/// </summary>
	internal class McpXmppExtension : IXmppExtension
	{
		private readonly HttpRequest firstRequest;
		private readonly HashSet<string> sessionIds = new HashSet<string>();
		private readonly Dictionary<string, PresenceEventArgs> subscriptionRequests =
			new Dictionary<string, PresenceEventArgs>(StringComparer.InvariantCultureIgnoreCase);

		/// <summary>
		/// Contains information about an MCP XMPP session.
		/// </summary>
		/// <param name="Request">Request that generated the connection.</param>
		/// <param name="SessionId">Session creating the object.</param>
		public McpXmppExtension(HttpRequest Request, string SessionId)
		{
			this.firstRequest = Request;
			this.sessionIds.Add(SessionId);
		}

		/// <summary>
		/// Implemented extensions.
		/// </summary>
		public string[] Extensions => new string[] { "MCP" };

		/// <summary>
		/// First request that generated the connection.
		/// </summary>
		public HttpRequest FirstRequest => this.firstRequest;

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
	}
}
