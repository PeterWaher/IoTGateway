using System.Threading.Tasks;
using Waher.Networking.Sniffers;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Delegate for custom sniffer callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task CustomSnifferEventHandler(object Sender, CustomSnifferEventArgs e);

	/// <summary>
	/// Event arguments for custom sniffer events.
	/// </summary>
	public class CustomSnifferEventArgs : MessageEventArgs
	{
		private readonly string snifferId;
		private ISniffer sniffer;

		internal CustomSnifferEventArgs(string SnifferId, MessageEventArgs Message)
			: base(Message)
		{
			this.snifferId = SnifferId;
			this.sniffer = null;
		}

		/// <summary>
		/// ID of sniffer session.
		/// </summary>
		public string SnifferId => this.snifferId;

		/// <summary>
		/// Custom sniffer to receive message. Leave as null if message should be ignored.
		/// </summary>
		public ISniffer Sniffer
		{
			get => this.sniffer;
			set => this.sniffer = value;
		}
	}
}
