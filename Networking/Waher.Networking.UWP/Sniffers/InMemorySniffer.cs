using System;
using System.Collections;
using System.Collections.Generic;
using Waher.Networking.Sniffers.Model;

namespace Waher.Networking.Sniffers
{
	/// <summary>
	/// Sniffer that stores events in memory.
	/// </summary>
	public class InMemorySniffer : ISniffer, IEnumerable<SnifferEvent>
	{
		private LinkedList<SnifferEvent> events = new LinkedList<SnifferEvent>();

		/// <summary>
		/// Sniffer that stores events in memory.
		/// </summary>
		public InMemorySniffer()
		{
		}

		/// <summary>
		/// Called to inform the viewer of an error state.
		/// </summary>
		/// <param name="Error">Error.</param>
		public void Error(string Error)
		{
			this.events.AddLast(new SnifferError(Error));
		}

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Exception">Exception.</param>
		public void Exception(string Exception)
		{
			this.events.AddLast(new SnifferException(Exception));
		}

		/// <summary>
		/// Called to inform the viewer of something.
		/// </summary>
		/// <param name="Comment">Comment.</param>
		public void Information(string Comment)
		{
			this.events.AddLast(new SnifferInformation(Comment));
		}

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Data">Binary Data.</param>
		public void ReceiveBinary(byte[] Data)
		{
			this.events.AddLast(new SnifferRxBinary(Data));
		}

		/// <summary>
		/// Called when text has been received.
		/// </summary>
		/// <param name="Text">Text</param>
		public void ReceiveText(string Text)
		{
			this.events.AddLast(new SnifferRxText(Text));
		}

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Data">Binary Data.</param>
		public void TransmitBinary(byte[] Data)
		{
			this.events.AddLast(new SnifferTxBinary(Data));
		}

		/// <summary>
		/// Called when text has been transmitted.
		/// </summary>
		/// <param name="Text">Text</param>
		public void TransmitText(string Text)
		{
			this.events.AddLast(new SnifferTxText(Text));
		}

		/// <summary>
		/// Called to inform the viewer of a warning state.
		/// </summary>
		/// <param name="Warning">Warning.</param>
		public void Warning(string Warning)
		{
			this.events.AddLast(new SnifferWarning(Warning));
		}

		/// <summary>
		/// <see cref="IEnumerable.GetEnumerator"/>
		/// </summary>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.events.GetEnumerator();
		}

		/// <summary>
		/// <see cref="IEnumerable{SnifferEvent}.GetEnumerator"/>
		/// </summary>
		public IEnumerator<SnifferEvent> GetEnumerator()
		{
			return this.events.GetEnumerator();
		}

		/// <summary>
		/// Replays sniffer events.
		/// </summary>
		/// <param name="Sniffable">Receiver of sniffer events.</param>
		public void Replay(Sniffable Sniffable)
		{
			if (Sniffable.HasSniffers)
				this.Replay(Sniffable.Sniffers);
		}

		/// <summary>
		/// Replays sniffer events.
		/// </summary>
		/// <param name="Sniffers">Receiver of sniffer events.</param>
		public void Replay(params ISniffer[] Sniffers)
		{
			foreach (SnifferEvent Event in this.events)
			{
				foreach (ISniffer Sniffer in Sniffers)
					Event.Replay(Sniffer);
			}
		}
	}
}
