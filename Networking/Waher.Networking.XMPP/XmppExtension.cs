using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Networking.Sniffers;

namespace Waher.Networking.XMPP
{
	/// <summary>
	/// Base class for XMPP Extensions.
	/// </summary>
	public abstract class XmppExtension : IXmppExtension, ISniffable
	{
		/// <summary>
		/// XMPP Client used by the extension.
		/// </summary>
		protected XmppClient client;

		private bool clientDisposed = false;

		/// <summary>
		/// Base class for XMPP Extensions.
		/// </summary>
		/// <param name="Client">XMPP Client.</param>
		public XmppExtension(XmppClient Client)
		{
			this.client = Client;
			this.client.RegisterExtension(this);
			this.client.OnDisposed += this.Client_OnDisposed;
		}

		private Task Client_OnDisposed(object Sender, EventArgs e)
		{
			this.clientDisposed = true;
			return Task.CompletedTask;
		}

		/// <summary>
		/// Implemented extensions.
		/// </summary>
		public abstract string[] Extensions
		{
			get;
		}

		/// <summary>
		/// XMPP Client.
		/// </summary>
		public XmppClient Client => this.client;

		/// <summary>
		/// If the client has been disposed.
		/// </summary>
		public bool ClientDisposed => this.clientDisposed;

		/// <summary>
		/// Disposes of the extension.
		/// </summary>
		public virtual void Dispose()
		{
			this.client.UnregisterExtension(this);
		}

		#region ISniffable

		/// <summary>
		/// Adds a sniffer to the node.
		/// </summary>
		/// <param name="Sniffer">Sniffer to add.</param>
		public void Add(ISniffer Sniffer) => this.Client.Add(Sniffer);

		/// <summary>
		/// Adds a range of sniffers to the node.
		/// </summary>
		/// <param name="Sniffers">Sniffers to add.</param>
		public void AddRange(IEnumerable<ISniffer> Sniffers) => this.Client.AddRange(Sniffers);

		/// <summary>
		/// Removes a sniffer, if registered.
		/// </summary>
		/// <param name="Sniffer">Sniffer to remove.</param>
		/// <returns>If the sniffer was found and removed.</returns>
		public bool Remove(ISniffer Sniffer) => this.Client.Remove(Sniffer);

		/// <summary>
		/// Gets a typed enumerator.
		/// </summary>
		public IEnumerator<ISniffer> GetEnumerator() => this.Client.GetEnumerator();

		/// <summary>
		/// Gets an untyped enumerator.
		/// </summary>
		IEnumerator IEnumerable.GetEnumerator() => this.Client.GetEnumerator();

		/// <summary>
		/// Registered sniffers.
		/// </summary>
		public ISniffer[] Sniffers => this.Client.Sniffers;

		/// <summary>
		/// If there are sniffers registered on the object.
		/// </summary>
		public bool HasSniffers => this.Client.HasSniffers;

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Data">Binary Data.</param>
		public Task ReceiveBinary(byte[] Data) => this.Client.ReceiveBinary(Data);

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Data">Binary Data.</param>
		public Task TransmitBinary(byte[] Data) => this.Client.TransmitBinary(Data);

		/// <summary>
		/// Called when text has been received.
		/// </summary>
		/// <param name="Text">Text</param>
		public Task ReceiveText(string Text) => this.Client.ReceiveText(Text);

		/// <summary>
		/// Called when text has been transmitted.
		/// </summary>
		/// <param name="Text">Text</param>
		public Task TransmitText(string Text) => this.Client.TransmitText(Text);

		/// <summary>
		/// Called to inform the viewer of something.
		/// </summary>
		/// <param name="Comment">Comment.</param>
		public Task Information(string Comment) => this.Client.Information(Comment);

		/// <summary>
		/// Called to inform the viewer of a warning state.
		/// </summary>
		/// <param name="Warning">Warning.</param>
		public Task Warning(string Warning) => this.Client.Warning(Warning);

		/// <summary>
		/// Called to inform the viewer of an error state.
		/// </summary>
		/// <param name="Error">Error.</param>
		public Task Error(string Error) => this.Client.Error(Error);

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Exception">Exception.</param>
		public Task Exception(Exception Exception) => this.Client.Exception(Exception);

		#endregion
	}
}
