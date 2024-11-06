using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Waher.Networking.Sniffers
{
	/// <summary>
	/// Interface for sniffable classes. Sniffable classes can receive <see cref="ISniffer"/> objects that eavesdrop on communication being performed
	/// at the node.
	/// </summary>
	public interface ISniffable : IEnumerable<ISniffer>
	{
		/// <summary>
		/// Adds a sniffer to the node.
		/// </summary>
		/// <param name="Sniffer">Sniffer to add.</param>
		void Add(ISniffer Sniffer);

		/// <summary>
		/// Adds a range of sniffers to the node.
		/// </summary>
		/// <param name="Sniffers">Sniffers to add.</param>
		void AddRange(IEnumerable<ISniffer> Sniffers);

		/// <summary>
		/// Removes a sniffer, if registered.
		/// </summary>
		/// <param name="Sniffer">Sniffer to remove.</param>
		/// <returns>If the sniffer was found and removed.</returns>
		bool Remove(ISniffer Sniffer);

		/// <summary>
		/// Registered sniffers.
		/// </summary>
		ISniffer[] Sniffers
		{
			get;
		}

		/// <summary>
		/// If there are sniffers registered on the object.
		/// </summary>
		bool HasSniffers
		{
			get;
		}

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Data">Binary Data.</param>
		Task ReceiveBinary(byte[] Data);

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Data">Binary Data.</param>
		Task TransmitBinary(byte[] Data);

		/// <summary>
		/// Called when text has been received.
		/// </summary>
		/// <param name="Text">Text</param>
		Task ReceiveText(string Text);

		/// <summary>
		/// Called when text has been transmitted.
		/// </summary>
		/// <param name="Text">Text</param>
		Task TransmitText(string Text);

		/// <summary>
		/// Called to inform the viewer of something.
		/// </summary>
		/// <param name="Comment">Comment.</param>
		Task Information(string Comment);

		/// <summary>
		/// Called to inform the viewer of a warning state.
		/// </summary>
		/// <param name="Warning">Warning.</param>
		Task Warning(string Warning);

		/// <summary>
		/// Called to inform the viewer of an error state.
		/// </summary>
		/// <param name="Error">Error.</param>
		Task Error(string Error);

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Exception">Exception.</param>
		Task Exception(Exception Exception);
	}
}
