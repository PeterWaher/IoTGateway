using System;
using System.Collections.Generic;
using System.Text;

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
		public Stop(long Ticks)
			: base(Ticks)
		{
		}
	}
}
