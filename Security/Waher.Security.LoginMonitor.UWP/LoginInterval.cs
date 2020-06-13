using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Security.LoginMonitor
{
	/// <summary>
	/// Number of failing login attempts possible during given time period.
	/// </summary>
	public class LoginInterval
	{
		private readonly TimeSpan interval;
		private readonly int nrAttempts;

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

			this.interval = Interval;
			this.nrAttempts = NrAttempts;
		}

		/// <summary>
		/// Number of allowed login attempts.
		/// </summary>
		public TimeSpan Interval => this.interval;

		/// <summary>
		/// Time period during which failing attempts can be made.
		/// </summary>
		public int NrAttempts => this.nrAttempts;
	}
}
