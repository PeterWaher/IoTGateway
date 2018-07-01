using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.Synchronization
{
	/// <summary>
	/// Delegate for clock synchronization callback methods.
	/// </summary>
	/// <param name="Sender">Sender</param>
	/// <param name="e">Event argument</param>
	public delegate void SynchronizationEventHandler(object Sender, SynchronizationEventArgs e);

	/// <summary>
	/// Event arguments containing the response of a clock synchronization request.
	/// </summary>
	public class SynchronizationEventArgs : IqResultEventArgs
    {
		private readonly long latency100Ns;
		private readonly long clockDifference100Ns;

		internal SynchronizationEventArgs(long Latency100Ns, long ClockDifference100Ns, IqResultEventArgs e)
			: base(e)
		{
			this.latency100Ns = Latency100Ns;
			this.clockDifference100Ns = ClockDifference100Ns;
		}

		/// <summary>
		/// Measured network latency in one direction, in units of 100 ns.
		/// </summary>
		public long Latency100Ns => this.latency100Ns;

		/// <summary>
		/// Measured clock difference between source clock and client clock, in units of 100 ns.
		/// Source clock = client clock + clock difference
		/// </summary>
		public long ClockDifference100Ns => this.clockDifference100Ns;

	}
}
