using System;
using System.Collections;
using System.Collections.Generic;
using Waher.Networking.Sniffers.Model;

namespace Waher.Networking.Sniffers
{
    /// <summary>
    /// Sniffer that stores events in memory.
    /// </summary>
    public class InMemorySniffer : SnifferBase, IEnumerable<SnifferEvent>
    {
        private readonly LinkedList<SnifferEvent> events = new LinkedList<SnifferEvent>();

        /// <summary>
        /// Sniffer that stores events in memory.
        /// </summary>
        public InMemorySniffer()
        {
        }

        /// <summary>
        /// Called to inform the viewer of an error state.
        /// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
        /// <param name="Error">Error.</param>
        public override void Error(DateTime Timestamp, string Error)
        {
            lock (this.events)
            {
                this.events.AddLast(new SnifferError(Timestamp, Error));
            }
        }

        /// <summary>
        /// Called to inform the viewer of an exception state.
        /// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
        /// <param name="Exception">Exception.</param>
        public override void Exception(DateTime Timestamp, string Exception)
        {
            lock (this.events)
            {
                this.events.AddLast(new SnifferException(Timestamp, Exception));
            }
        }

        /// <summary>
        /// Called to inform the viewer of something.
        /// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
        /// <param name="Comment">Comment.</param>
        public override void Information(DateTime Timestamp, string Comment)
        {
            lock (this.events)
            {
                this.events.AddLast(new SnifferInformation(Timestamp, Comment));
            }
        }

        /// <summary>
        /// Called when binary data has been received.
        /// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
        /// <param name="Data">Binary Data.</param>
        public override void ReceiveBinary(DateTime Timestamp, byte[] Data)
        {
            lock (this.events)
            {
                this.events.AddLast(new SnifferRxBinary(Timestamp, Data));
            }
        }

        /// <summary>
        /// Called when text has been received.
        /// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
        /// <param name="Text">Text</param>
        public override void ReceiveText(DateTime Timestamp, string Text)
        {
            lock (this.events)
            {
                this.events.AddLast(new SnifferRxText(Timestamp, Text));
            }
        }

        /// <summary>
        /// Called when binary data has been transmitted.
        /// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
        /// <param name="Data">Binary Data.</param>
        public override void TransmitBinary(DateTime Timestamp, byte[] Data)
        {
            lock (this.events)
            {
                this.events.AddLast(new SnifferTxBinary(Timestamp, Data));
            }
        }

        /// <summary>
        /// Called when text has been transmitted.
        /// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
        /// <param name="Text">Text</param>
        public override void TransmitText(DateTime Timestamp, string Text)
        {
            lock (this.events)
            {
                this.events.AddLast(new SnifferTxText(Timestamp, Text));
            }
        }

        /// <summary>
        /// Called to inform the viewer of a warning state.
        /// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
        /// <param name="Warning">Warning.</param>
        public override void Warning(DateTime Timestamp, string Warning)
        {
            lock (this.events)
            {
                this.events.AddLast(new SnifferWarning(Timestamp, Warning));
            }
        }

        /// <summary>
        /// <see cref="IEnumerable.GetEnumerator"/>
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.ToList().GetEnumerator();
        }

        /// <summary>
        /// <see cref="IEnumerable{SnifferEvent}.GetEnumerator"/>
        /// </summary>
        public IEnumerator<SnifferEvent> GetEnumerator()
        {
            return this.ToList().GetEnumerator();
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
            foreach (SnifferEvent Event in this.ToList())
            {
                foreach (ISniffer Sniffer in Sniffers)
                    Event.Replay(Sniffer);
            }
        }

        /// <summary>
        /// Returns recorded events as a typed list.
        /// </summary>
        /// <returns>Recorded events.</returns>
        public List<SnifferEvent> ToList()
        {
            List<SnifferEvent> Result = new List<SnifferEvent>();

            lock (this.events)
            {
                Result.AddRange(this.events);
            }

            return Result;
        }

        /// <summary>
        /// Returns recorded events as an array.
        /// </summary>
        /// <returns>Recorded events.</returns>
        public SnifferEvent[] ToArray()
        {
            return this.ToList().ToArray();
        }
    }
}
