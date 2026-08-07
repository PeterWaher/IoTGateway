using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking.HTTP.JsonRpc;
using Waher.Networking.HTTP.Mcp.Model.Client;
using Waher.Networking.Sniffers;
using Waher.Security;

namespace Waher.Networking.HTTP.Mcp.Model.Server
{
	/// <summary>
	/// MCP Session information.
	/// </summary>
	public class Session : IDisposableAsync, IJsonRpcSession
	{
		private readonly HashSet<string> subscriptions = new HashSet<string>();
		private readonly ISnifferSet? snifferSet;
		private readonly bool hasSnifferSet;
		private InMemorySniffer? unauthenticatedSniffer;
		private IUser? user = null;
		private string userName = "Not Authenticated";
		private bool isAuthenticated = false;

		/// <summary>
		/// MCP Session information.
		/// </summary>
		/// <param name="SessionId">MCP Session ID.</param>
		/// <param name="ClientProtocolVersion">Protocol version of client.</param>
		/// <param name="ClientCapabilities">Client capabilities, if available.</param>
		/// <param name="Implementation">Information about client, if available.</param>
		/// <param name="RemoteEndpoint">Client remote endpoint.</param>
		/// <param name="SnifferSet">Optional sniffer set used to log agent interaction 
		/// with MCP service.</param>
		public Session(string SessionId, string ClientProtocolVersion,
			ClientCapabilities? ClientCapabilities, Implementation? Implementation,
			string RemoteEndpoint, ISnifferSet? SnifferSet)
		{
			this.SessionId = SessionId;
			this.ClientProtocolVersion = ClientProtocolVersion;
			this.ClientCapabilities = ClientCapabilities;
			this.ClientInformation = Implementation;
			this.RemoteEndpoint = RemoteEndpoint;
			this.snifferSet = SnifferSet;
			this.hasSnifferSet = !(SnifferSet is null);

			this.unauthenticatedSniffer = new InMemorySniffer(200, SessionId);
		}

		/// <summary>
		/// MCP Session ID
		/// </summary>
		public string SessionId { get; }

		/// <summary>
		/// Protocol version of client.
		/// </summary>
		public string ClientProtocolVersion { get; }

		/// <summary>
		/// Client capabilities, if available.
		/// </summary>
		public ClientCapabilities? ClientCapabilities { get; }

		/// <summary>
		/// Information about client, if available.
		/// </summary>
		public Implementation? ClientInformation { get; }

		/// <summary>
		/// Client remote endpoint.
		/// </summary>
		public string RemoteEndpoint { get; }

		/// <summary>
		/// User object reference.
		/// </summary>
		public IUser? User => this.user;

		/// <summary>
		/// User name used for session.
		/// </summary>
		public string UserName => this.userName;

		/// <summary>
		/// If client has been authenticated in the session.
		/// </summary>
		public bool IsAuthenticated => this.isAuthenticated;

		/// <summary>
		/// Disposes the connection
		/// </summary>
		[Obsolete("Use DisposeAsync instead.")]
		public void Dispose()
		{
			this.DisposeAsync().Wait();
		}

		/// <summary>
		/// <see cref="IDisposableAsync.DisposeAsync()"/>
		/// </summary>
		public async Task DisposeAsync()
		{
			if (!(this.unauthenticatedSniffer is null))
			{
				await this.unauthenticatedSniffer.DisposeAsync();
				this.unauthenticatedSniffer = null;
			}

			lock (this.subscriptions)
			{
				this.subscriptions.Clear();
			}
		}

		/// <summary>
		/// Sets the user of the session.
		/// </summary>
		/// <param name="User">User reference</param>
		internal async Task SetUser(IUser User)
		{
			this.user = User;
			this.userName = User.UserName;

			if (!this.isAuthenticated)
			{
				this.isAuthenticated = true;

				if (this.hasSnifferSet)
					this.unauthenticatedSniffer?.Replay(this.userName, this.snifferSet);

				if (!(this.unauthenticatedSniffer is null))
				{
					await this.unauthenticatedSniffer.DisposeAsync();
					this.unauthenticatedSniffer = null;
				}
			}
		}

		/// <summary>
		/// Text has been received from the client.
		/// </summary>
		/// <param name="Text">Received text.</param>
		internal void ReceiveText(string Text)
		{
			if (this.hasSnifferSet)
			{
				if (this.isAuthenticated)
					this.snifferSet!.ReceiveText(this.userName, Text);
				else
					this.unauthenticatedSniffer?.ReceiveText(Text);
			}
		}

		/// <summary>
		/// Text has been transmitted to the client.
		/// </summary>
		/// <param name="Text">Transmitted text.</param>
		public void TransmitText(string Text)
		{
			if (this.hasSnifferSet)
			{
				if (this.isAuthenticated)
					this.snifferSet!.TransmitText(this.userName, Text);
				else
					this.unauthenticatedSniffer?.TransmitText(Text);
			}
		}

		/// <summary>
		/// An information message has been logged.
		/// </summary>
		/// <param name="Text">Information text.</param>
		internal void Information(string Text)
		{
			if (this.hasSnifferSet)
			{
				if (this.isAuthenticated)
					this.snifferSet!.Information(this.userName, Text);
				else
					this.unauthenticatedSniffer?.Information(Text);
			}
		}

		/// <summary>
		/// A warning message has been logged.
		/// </summary>
		/// <param name="Text">Warning text.</param>
		internal void Warning(string Text)
		{
			if (this.hasSnifferSet)
			{
				if (this.isAuthenticated)
					this.snifferSet!.Warning(this.userName, Text);
				else
					this.unauthenticatedSniffer?.Warning(Text);
			}
		}

		/// <summary>
		/// A error message has been logged.
		/// </summary>
		/// <param name="Text">Error text.</param>
		internal void Error(string Text)
		{
			if (this.hasSnifferSet)
			{
				if (this.isAuthenticated)
					this.snifferSet!.Error(this.userName, Text);
				else
					this.unauthenticatedSniffer?.Error(Text);
			}
		}

		/// <summary>
		/// A Exception has occurred.
		/// </summary>
		/// <param name="Exception">Exception object</param>
		internal void Exception(Exception Exception)
		{
			if (this.hasSnifferSet)
			{
				if (this.isAuthenticated)
					this.snifferSet!.Exception(this.userName, Exception);
				else
					this.unauthenticatedSniffer?.Exception(Exception);
			}
		}

		/// <summary>
		/// Checks if a subscription to a resource exists.
		/// </summary>
		/// <param name="Uri">URI of resource.</param>
		/// <returns>If a subscription exists.</returns>
		public bool IsSubscribed(string Uri)
		{
			lock (this.subscriptions)
			{
				return this.subscriptions.Contains(Uri);
			}
		}

		/// <summary>
		/// Subscribes to a resource.
		/// </summary>
		/// <param name="Uri">URI of resource.</param>
		/// <returns>If a subscription was added (true), or if one already existed(false).</returns>
		public bool Subscribe(string Uri)
		{
			lock (this.subscriptions)
			{
				return this.subscriptions.Add(Uri);
			}
		}

		/// <summary>
		/// Unsubscribes from a resource.
		/// </summary>
		/// <param name="Uri">URI of resource.</param>
		/// <returns>If a subscription was removed (true), or if one was not found (false).</returns>
		public bool Unsubscribe(string Uri)
		{
			lock (this.subscriptions)
			{
				return this.subscriptions.Remove(Uri);
			}
		}
	}
}