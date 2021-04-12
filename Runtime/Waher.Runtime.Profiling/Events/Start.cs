using System;
using Waher.Runtime.Profiling.Export;

namespace Waher.Runtime.Profiling.Events
{
	/// <summary>
	/// Processing starts.
	/// </summary>
	public class Start : ProfilerEvent
	{
		/// <summary>
		/// Processing starts.
		/// </summary>
		/// <param name="Ticks">Elapsed ticks.</param>
		/// <param name="Thread">Profiler thread generating the event.</param>
		public Start(long Ticks, ProfilerThread Thread)
			: base(Ticks, Thread)
		{
		}

		/// <inheritdoc/>
		public override string EventType => "Start";

		/// <inheritdoc/>
		public override string PlantUmlState
		{
			get
			{
				if (this.Thread.Type == ProfilerThreadType.StateMachine)
					return "idle";
				else
					return "{-}";
			}
		}

		/// <inheritdoc/>
		public override void Accumulate(Accumulator Accumulator)
		{
			Accumulator.Start(this);
		}
	}
}
