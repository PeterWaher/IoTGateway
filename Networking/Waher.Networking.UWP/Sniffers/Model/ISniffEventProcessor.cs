using System.Threading;
using System.Threading.Tasks;

namespace Waher.Networking.Sniffers.Model
{
	/// <summary>
	/// Interface for sniff event processors
	/// </summary>
	public interface ISniffEventProcessor
	{
		/// <summary>
		/// Processes a binary reception event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		/// <param name="Cancel">Cancellation token.</param>
		Task Process(SnifferRxBinary Event, CancellationToken Cancel);

		/// <summary>
		/// Processes a binary transmission event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		/// <param name="Cancel">Cancellation token.</param>
		Task Process(SnifferTxBinary Event, CancellationToken Cancel);

		/// <summary>
		/// Processes a text reception event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		/// <param name="Cancel">Cancellation token.</param>
		Task Process(SnifferRxText Event, CancellationToken Cancel);

		/// <summary>
		/// Processes a text transmission event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		/// <param name="Cancel">Cancellation token.</param>
		Task Process(SnifferTxText Event, CancellationToken Cancel);

		/// <summary>
		/// Processes an information event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		/// <param name="Cancel">Cancellation token.</param>
		Task Process(SnifferInformation Event, CancellationToken Cancel);

		/// <summary>
		/// Processes a warning event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		/// <param name="Cancel">Cancellation token.</param>
		Task Process(SnifferWarning Event, CancellationToken Cancel);

		/// <summary>
		/// Processes an error event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		/// <param name="Cancel">Cancellation token.</param>
		Task Process(SnifferError Event, CancellationToken Cancel);

		/// <summary>
		/// Processes an exception event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		/// <param name="Cancel">Cancellation token.</param>
		Task Process(SnifferException Event, CancellationToken Cancel);
	}
}
