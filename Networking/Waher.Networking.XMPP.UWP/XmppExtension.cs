using System;

namespace Waher.Networking.XMPP
{
	/// <summary>
	/// Base class for XMPP Extensions.
	/// </summary>
	public abstract class XmppExtension : IXmppExtension
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
			this.client.OnDisposed += Client_OnDisposed;
		}

		private void Client_OnDisposed(object sender, EventArgs e)
		{
			this.clientDisposed = true;
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
		public XmppClient Client
		{
			get { return this.client; }
		}

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
	}
}
