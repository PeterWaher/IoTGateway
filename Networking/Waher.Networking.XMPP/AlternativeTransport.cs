using System;
using Waher.Runtime.Inventory;

namespace Waher.Networking.XMPP
{
	/// <summary>
	/// Base class for alternative transports.
	/// </summary>
	public abstract class AlternativeTransport : IAlternativeTransport
	{
		/// <summary>
		/// Event raised when a packet has been sent.
		/// </summary>
		public abstract event TextEventHandler OnSent;

		/// <summary>
		/// Event received when text data has been received.
		/// </summary>
		public abstract event TextEventHandler OnReceived;

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting
		/// unmanaged resources.
		/// </summary>
		public abstract void Dispose();

		/// <summary>
		/// Sends a text packet.
		/// </summary>
		/// <param name="Packet">Text packet.</param>
		public abstract void Send(string Packet);

		/// <summary>
		/// Sends a text packet.
		/// </summary>
		/// <param name="Packet">Text packet.</param>
		/// <param name="DeliveryCallback">Optional method to call when packet has been delivered.</param>
		public abstract void Send(string Packet, EventHandler DeliveryCallback);

		/// <summary>
		/// If the reading is paused.
		/// </summary>
		public abstract bool Paused
		{
			get;
		}

		/// <summary>
		/// Call this method to continue operation. Operation can be paused, by returning false from <see cref="OnReceived"/>.
		/// </summary>
		public abstract void Continue();

		/// <summary>
		/// How well the alternative transport handles the XMPP credentials provided.
		/// </summary>
		/// <param name="URI">URI defining endpoint.</param>
		/// <returns>Support grade.</returns>
		public abstract Grade Handles(Uri URI);

		/// <summary>
		/// If the alternative binding mechanism handles heartbeats.
		/// </summary>
		public abstract bool HandlesHeartbeats
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
		public abstract IAlternativeTransport Instantiate(Uri URI, XmppClient Client, XmppBindingInterface BindingInterface);

		/// <summary>
		/// Creates a session.
		/// </summary>
		public abstract void CreateSession();

		/// <summary>
		/// Closes a session.
		/// </summary>
		public abstract void CloseSession();

	}
}
