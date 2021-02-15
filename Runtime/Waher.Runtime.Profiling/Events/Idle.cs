using System;

namespace Waher.Runtime.Profiling.Events
{
	/// <summary>
	/// Thread goes idle.
	/// </summary>
	public class Idle : ProfilerEvent
	{
		/// <summary>
		/// Thread goes idle.
		/// </summary>
		/// <param name="Ticks">Elapsed ticks.</param>
		/// <param name="Thread">Profiler thread generating the event.</param>
		public Idle(long Ticks, ProfilerThread Thread)
			: base(Ticks, Thread)
		{
		}

		/// <inheritdoc/>
		public override string EventType => "Idle";

		/// <inheritdoc/>
		public override string PlantUmlState => "{hidden}";
	}
}
