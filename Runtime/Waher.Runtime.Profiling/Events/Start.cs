using System;
using System.Collections.Generic;
using System.Text;

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
		public Start(long Ticks)
			: base(Ticks)
		{
		}
	}
}
