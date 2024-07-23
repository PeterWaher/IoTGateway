using System;
using Waher.Content;

namespace Waher.Security.LoginMonitor
{
	/// <summary>
	/// Number of failing login attempts possible during given time period.
	/// </summary>
	public class LoginInterval
	{
		private readonly TimeSpan intervalTimeSpan;
		private readonly Duration intervalDuration;
		private readonly int nrAttempts;
		private readonly bool isDuration;

		/// <summary>
		/// Number of failing login attempts possible during given time period.
		/// </summary>
		/// <param name="NrAttempts">Number of allowed login attempts.</param>
		/// <param name="Interval">Time period during which failing attempts can be made.</param>
		public LoginInterval(int NrAttempts, TimeSpan Interval)
		{
			if (NrAttempts <= 0)
				throw new ArgumentException("Must be positive.", nameof(NrAttempts));

			if (Interval <= TimeSpan.Zero)
				throw new ArgumentException("Must be positive.", nameof(Interval));

			this.intervalTimeSpan = Interval;
			this.intervalDuration = Duration.Zero;
			this.nrAttempts = NrAttempts;
			this.isDuration = false;
		}

		/// <summary>
		/// Number of failing login attempts possible during given time period.
		/// </summary>
		/// <param name="NrAttempts">Number of allowed login attempts.</param>
		/// <param name="Interval">Time period during which failing attempts can be made.</param>
		public LoginInterval(int NrAttempts, Duration Interval)
		{
			if (NrAttempts <= 0)
				throw new ArgumentException("Must be positive.", nameof(NrAttempts));

			if (Interval <= Duration.Zero)
				throw new ArgumentException("Must be positive.", nameof(Interval));

			this.intervalTimeSpan = TimeSpan.Zero;
			this.intervalDuration = Interval;
			this.nrAttempts = NrAttempts;
			this.isDuration = true;
		}

		/// <summary>
		/// Adds an interval to a time stamp.
		/// </summary>
		/// <param name="TP">Timestamp to add the interval to.</param>
		/// <returns>Timestamp + interval.<</returns>
		public DateTime AddIntervalTo(DateTime TP)
		{
			if (this.isDuration)
				return TP + this.intervalDuration;
			else
				return TP + this.intervalTimeSpan;
		}

		/// <summary>
		/// Time period during which failing attempts can be made.
		/// </summary>
		public int NrAttempts => this.nrAttempts;
	}
}
