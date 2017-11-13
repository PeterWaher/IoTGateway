using System;
using System.Collections.Generic;
using System.Text;

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

		/// <summary>
		/// Base class for XMPP Extensions.
		/// </summary>
		/// <param name="Client">XMPP Client.</param>
		public XmppExtension(XmppClient Client)
		{
			this.client = Client;
			this.client.RegisterExtension(this);
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
		/// Disposes of the extension.
		/// </summary>
		public virtual void Dispose()
		{
			this.client.UnregisterExtension(this);
		}
	}
}
