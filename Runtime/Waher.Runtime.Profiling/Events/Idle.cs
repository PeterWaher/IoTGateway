using System;
using System.Collections.Generic;
using System.Text;

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
		public Idle(long Ticks)
			: base(Ticks)
		{
		}
	}
}
