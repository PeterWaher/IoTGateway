using System;

namespace Waher.Runtime.Profiling.Events
{
	/// <summary>
	/// Processing stops.
	/// </summary>
	public class Stop : ProfilerEvent
	{
		/// <summary>
		/// Processing stops.
		/// </summary>
		/// <param name="Ticks">Elapsed ticks.</param>
		/// <param name="Thread">Profiler thread generating the event.</param>
		public Stop(long Ticks, ProfilerThread Thread)
			: base(Ticks, Thread)
		{
		}

		/// <inheritdoc/>
		public override string EventType => "Stop";

		/// <inheritdoc/>
		public override string PlantUmlState => "{hidden}";
	}
}
