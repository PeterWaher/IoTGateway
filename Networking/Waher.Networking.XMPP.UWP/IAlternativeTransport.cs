using System;
using Waher.Runtime.Inventory;

namespace Waher.Networking.XMPP
{
	/// <summary>
	/// Base class for alternative transports.
	/// </summary>
	public interface IAlternativeTransport : ITextTransportLayer, IProcessingSupport<Uri>
	{
		/// <summary>
		/// If the alternative binding mechanism handles heartbeats.
		/// </summary>
		bool HandlesHeartbeats
		{
			get;
		}

		/// <summary>
		/// Instantiates a new alternative connections.
		/// </summary>
		/// <param name="URI">URI defining endpoint.</param>
		/// <param name="Client">XMPP Client</param>
		/// <param name="BindingInterface">Inteface to internal properties of the <see cref="XmppClient"/>.</param>
		/// <returns>Instantiated binding.</returns>
		IAlternativeTransport Instantiate(Uri URI, XmppClient Client, XmppBindingInterface BindingInterface);

		/// <summary>
		/// Creates a session.
		/// </summary>
		void CreateSession();

		/// <summary>
		/// Closes a session.
		/// </summary>
		void CloseSession();
	}
}
