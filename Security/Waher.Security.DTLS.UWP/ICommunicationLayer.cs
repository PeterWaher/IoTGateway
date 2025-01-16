using System.Threading.Tasks;

namespace Waher.Security.DTLS
{
	/// <summary>
	/// Delegate for data reception events.
	/// </summary>
	/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
	/// or if the contents in the buffer may change after the call (false).</param>
	/// <param name="Data">Data packet being received.</param>
	/// <param name="RemoteEndpoint">Remote endpoint, represented as a string.</param>
	public delegate Task DataReceivedEventHandler(bool ConstantBuffer, byte[] Data, object RemoteEndpoint);

	/// <summary>
	/// Represents the communication layer which the DTLS layer will use.
	/// </summary>
	public interface ICommunicationLayer
    {
		/// <summary>
		/// Sends a packet.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Packet">Packet to send.</param>
		/// <param name="RemoteEndpoint">Remote endpoint.</param>
		Task SendPacket(bool ConstantBuffer, byte[] Packet, object RemoteEndpoint);

		/// <summary>
		/// Event raised when a packet has been received.
		/// </summary>
		event DataReceivedEventHandler PacketReceived;
    }
}
