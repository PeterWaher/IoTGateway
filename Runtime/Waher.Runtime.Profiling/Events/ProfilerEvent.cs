using System;

namespace Waher.Runtime.Profiling.Events
{
	/// <summary>
	/// Abstract base class for profiler events.
	/// </summary>
	public abstract class ProfilerEvent
	{
		private readonly long ticks;

		/// <summary>
		/// Abstract base class for profiler events.
		/// </summary>
		/// <param name="Ticks">Elapsed ticks.</param>
		public ProfilerEvent(long Ticks)
		{
			this.ticks = Ticks;
		}

		/// <summary>
		/// Elapsed ticks.
		/// </summary>
		public long Ticks => this.ticks;
	}
}
