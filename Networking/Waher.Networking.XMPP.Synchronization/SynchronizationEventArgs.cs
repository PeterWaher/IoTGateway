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
		private readonly double latencySeconds;
		private readonly double clockDifferenceSeconds;

		internal SynchronizationEventArgs(double LatencySeconds, double ClockDifferenceSeconds, IqResultEventArgs e)
			: base(e)
		{
			this.latencySeconds = LatencySeconds;
			this.clockDifferenceSeconds = ClockDifferenceSeconds;
		}

		/// <summary>
		/// Measured network latency in one direction, in seconds.
		/// </summary>
		public double LatencySeconds => this.latencySeconds;

		/// <summary>
		/// Measured clock difference between source clock and client clock, in seconds.
		/// Source clock = client clock + clock difference
		/// </summary>
		public double ClockDifferenceSeconds => this.clockDifferenceSeconds;

	}
}
