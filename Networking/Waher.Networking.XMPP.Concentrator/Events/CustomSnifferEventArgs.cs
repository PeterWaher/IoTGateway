using Waher.Networking.Sniffers;
using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Concentrator
{
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
