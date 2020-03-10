using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking
{
	/// <summary>
	/// Event handler for text packet events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="Text">Text packet.</param>
	/// <returns>If the process should be continued.</returns>
	public delegate Task<bool> TextEventHandler(object Sender, string Text);

	/// <summary>
	/// Interface for text transport layers.
	/// </summary>
	public interface ITextTransportLayer : IDisposable
	{
		/// <summary>
		/// Sends a text packet.
		/// </summary>
		/// <param name="Text">Text packet.</param>
		/// <returns>If data was sent.</returns>
		Task<bool> Send(string Text);

		/// <summary>
		/// Sends a text packet.
		/// </summary>
		/// <param name="Text">Text packet.</param>
		/// <param name="DeliveryCallback">Optional method to call when packet has been delivered.</param>
		/// <returns>If data was sent.</returns>
		Task<bool> Send(string Text, EventHandler DeliveryCallback);

		/// <summary>
		/// Event raised when a packet has been sent.
		/// </summary>
		event TextEventHandler OnSent;

		/// <summary>
		/// Event received when text data has been received.
		/// </summary>
		event TextEventHandler OnReceived;

		/// <summary>
		/// If the reading is paused.
		/// </summary>
		bool Paused
		{
			get;
		}

		/// <summary>
		/// Call this method to continue operation. Operation can be paused, by returning false from <see cref="OnReceived"/>.
		/// </summary>
		void Continue();
	}
}
