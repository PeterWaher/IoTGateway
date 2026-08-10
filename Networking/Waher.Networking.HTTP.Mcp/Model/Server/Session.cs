using System;
using System.Collections;
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
	public class Session : IJsonRpcSession, ICommunicationLayer, IDisposableAsync
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
		/// If events raised from the communication layer are decoupled, i.e. executed
		/// in parallel with the source that raised them.
		/// </summary>
		public bool DecoupledEvents => throw new NotSupportedException();

		/// <summary>
		/// Not supported.
		/// </summary>
		public IEnumerator<ISniffer> GetEnumerator()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Not supported.
		/// </summary>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		/// <summary>
		/// Adds a sniffer to the node.
		/// </summary>
		/// <param name="Sniffer">Sniffer to add.</param>
		public void Add(ISniffer Sniffer) => throw new NotSupportedException();

		/// <summary>
		/// Adds a range of sniffers to the node.
		/// </summary>
		/// <param name="Sniffers">Sniffers to add.</param>
		public void AddRange(IEnumerable<ISniffer> Sniffers) => throw new NotSupportedException();

		/// <summary>
		/// Removes a sniffer, if registered.
		/// </summary>
		/// <param name="Sniffer">Sniffer to remove.</param>
		/// <returns>If the sniffer was found and removed.</returns>
		public bool Remove(ISniffer Sniffer) => throw new NotSupportedException();

		/// <summary>
		/// Registered sniffers.
		/// </summary>
		public ISniffer[] Sniffers => throw new NotSupportedException();

		/// <summary>
		/// If there are sniffers registered on the object.
		/// </summary>
		public bool HasSniffers => this.hasSnifferSet;

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Count">Number of bytes received.</param>
		public void ReceiveBinary(int Count)
		{
			if (this.hasSnifferSet)
			{
				if (this.isAuthenticated)
					this.snifferSet!.ReceiveBinary(this.userName, Count);
				else
					this.unauthenticatedSniffer?.ReceiveBinary(Count);
			}
		}

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		public void ReceiveBinary(bool ConstantBuffer, byte[] Data)
		{
			if (this.hasSnifferSet)
			{
				if (this.isAuthenticated)
					this.snifferSet!.ReceiveBinary(this.userName, ConstantBuffer, Data);
				else
					this.unauthenticatedSniffer?.ReceiveBinary(ConstantBuffer, Data);
			}
		}

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where received data begins.</param>
		/// <param name="Count">Number of bytes received.</param>
		public void ReceiveBinary(bool ConstantBuffer, byte[] Data, int Offset, int Count)
		{
			if (this.hasSnifferSet)
			{
				if (this.isAuthenticated)
					this.snifferSet!.ReceiveBinary(this.userName, ConstantBuffer, Data, Offset, Count);
				else
					this.unauthenticatedSniffer?.ReceiveBinary(ConstantBuffer, Data, Offset, Count);
			}
		}

		/// <summary>
		/// Text has been received from the client.
		/// </summary>
		/// <param name="Text">Received text.</param>
		public void ReceiveText(string Text)
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
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Count">Number of bytes transmitted.</param>
		public void TransmitBinary(int Count)
		{
			if (this.hasSnifferSet)
			{
				if (this.isAuthenticated)
					this.snifferSet!.TransmitBinary(this.userName, Count);
				else
					this.unauthenticatedSniffer?.TransmitBinary(Count);
			}
		}

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		public void TransmitBinary(bool ConstantBuffer, byte[] Data)
		{
			if (this.hasSnifferSet)
			{
				if (this.isAuthenticated)
					this.snifferSet!.TransmitBinary(this.userName, ConstantBuffer, Data);
				else
					this.unauthenticatedSniffer?.TransmitBinary(ConstantBuffer, Data);
			}
		}

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where transmitted data begins.</param>
		/// <param name="Count">Number of bytes transmitted.</param>
		public void TransmitBinary(bool ConstantBuffer, byte[] Data, int Offset, int Count)
		{
			if (this.hasSnifferSet)
			{
				if (this.isAuthenticated)
					this.snifferSet!.TransmitBinary(this.userName, ConstantBuffer, Data, Offset, Count);
				else
					this.unauthenticatedSniffer?.TransmitBinary(ConstantBuffer, Data, Offset, Count);
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
		public void Information(string Text)
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
		public void Warning(string Text)
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
		public void Error(string Text)
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
		public void Exception(string Exception)
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
		/// A Exception has occurred.
		/// </summary>
		/// <param name="Exception">Exception object</param>
		public void Exception(Exception Exception)
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
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Count">Number of bytes received.</param>
		public void ReceiveBinary(DateTime Timestamp, int Count)
		{
			if (this.hasSnifferSet)
			{
				if (this.isAuthenticated)
					this.snifferSet!.ReceiveBinary(this.userName, Timestamp, Count);
				else
					this.unauthenticatedSniffer?.ReceiveBinary(Timestamp, Count);
			}
		}

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (Timestamp, true),
		/// or if the contents in the buffer may change after the call (Timestamp, false).</param>
		/// <param name="Data">Binary Data.</param>
		public void ReceiveBinary(DateTime Timestamp, bool ConstantBuffer, byte[] Data)
		{
			if (this.hasSnifferSet)
			{
				if (this.isAuthenticated)
					this.snifferSet!.ReceiveBinary(this.userName, Timestamp, ConstantBuffer, Data);
				else
					this.unauthenticatedSniffer?.ReceiveBinary(Timestamp, ConstantBuffer, Data);
			}
		}

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (Timestamp, true),
		/// or if the contents in the buffer may change after the call (Timestamp, false).</param>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where received data begins.</param>
		/// <param name="Count">Number of bytes received.</param>
		public void ReceiveBinary(DateTime Timestamp, bool ConstantBuffer, byte[] Data, int Offset, int Count)
		{
			if (this.hasSnifferSet)
			{
				if (this.isAuthenticated)
					this.snifferSet!.ReceiveBinary(this.userName, Timestamp, ConstantBuffer, Data, Offset, Count);
				else
					this.unauthenticatedSniffer?.ReceiveBinary(Timestamp, ConstantBuffer, Data, Offset, Count);
			}
		}

		/// <summary>
		/// Text has been received from the client.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Received text.</param>
		public void ReceiveText(DateTime Timestamp, string Text)
		{
			if (this.hasSnifferSet)
			{
				if (this.isAuthenticated)
					this.snifferSet!.ReceiveText(this.userName, Timestamp, Text);
				else
					this.unauthenticatedSniffer?.ReceiveText(Timestamp, Text);
			}
		}

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Count">Number of bytes transmitted.</param>
		public void TransmitBinary(DateTime Timestamp, int Count)
		{
			if (this.hasSnifferSet)
			{
				if (this.isAuthenticated)
					this.snifferSet!.TransmitBinary(this.userName, Timestamp, Count);
				else
					this.unauthenticatedSniffer?.TransmitBinary(Timestamp, Count);
			}
		}

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (Timestamp, true),
		/// or if the contents in the buffer may change after the call (Timestamp, false).</param>
		/// <param name="Data">Binary Data.</param>
		public void TransmitBinary(DateTime Timestamp, bool ConstantBuffer, byte[] Data)
		{
			if (this.hasSnifferSet)
			{
				if (this.isAuthenticated)
					this.snifferSet!.TransmitBinary(this.userName, Timestamp, ConstantBuffer, Data);
				else
					this.unauthenticatedSniffer?.TransmitBinary(Timestamp, ConstantBuffer, Data);
			}
		}

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (Timestamp, true),
		/// or if the contents in the buffer may change after the call (Timestamp, false).</param>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where transmitted data begins.</param>
		/// <param name="Count">Number of bytes transmitted.</param>
		public void TransmitBinary(DateTime Timestamp, bool ConstantBuffer, byte[] Data, int Offset, int Count)
		{
			if (this.hasSnifferSet)
			{
				if (this.isAuthenticated)
					this.snifferSet!.TransmitBinary(this.userName, Timestamp, ConstantBuffer, Data, Offset, Count);
				else
					this.unauthenticatedSniffer?.TransmitBinary(Timestamp, ConstantBuffer, Data, Offset, Count);
			}
		}

		/// <summary>
		/// Text has been transmitted to the client.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Transmitted text.</param>
		public void TransmitText(DateTime Timestamp, string Text)
		{
			if (this.hasSnifferSet)
			{
				if (this.isAuthenticated)
					this.snifferSet!.TransmitText(this.userName, Timestamp, Text);
				else
					this.unauthenticatedSniffer?.TransmitText(Timestamp, Text);
			}
		}

		/// <summary>
		/// An information message has been logged.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Information text.</param>
		public void Information(DateTime Timestamp, string Text)
		{
			if (this.hasSnifferSet)
			{
				if (this.isAuthenticated)
					this.snifferSet!.Information(this.userName, Timestamp, Text);
				else
					this.unauthenticatedSniffer?.Information(Timestamp, Text);
			}
		}

		/// <summary>
		/// A warning message has been logged.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Warning text.</param>
		public void Warning(DateTime Timestamp, string Text)
		{
			if (this.hasSnifferSet)
			{
				if (this.isAuthenticated)
					this.snifferSet!.Warning(this.userName, Timestamp, Text);
				else
					this.unauthenticatedSniffer?.Warning(Timestamp, Text);
			}
		}

		/// <summary>
		/// A error message has been logged.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Error text.</param>
		public void Error(DateTime Timestamp, string Text)
		{
			if (this.hasSnifferSet)
			{
				if (this.isAuthenticated)
					this.snifferSet!.Error(this.userName, Timestamp, Text);
				else
					this.unauthenticatedSniffer?.Error(Timestamp, Text);
			}
		}

		/// <summary>
		/// A Exception has occurred.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Exception">Exception object</param>
		public void Exception(DateTime Timestamp, string Exception)
		{
			if (this.hasSnifferSet)
			{
				if (this.isAuthenticated)
					this.snifferSet!.Exception(this.userName, Timestamp, Exception);
				else
					this.unauthenticatedSniffer?.Exception(Timestamp, Exception);
			}
		}

		/// <summary>
		/// A Exception has occurred.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Exception">Exception object</param>
		public void Exception(DateTime Timestamp, Exception Exception)
		{
			if (this.hasSnifferSet)
			{
				if (this.isAuthenticated)
					this.snifferSet!.Exception(this.userName, Timestamp, Exception);
				else
					this.unauthenticatedSniffer?.Exception(Timestamp, Exception);
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