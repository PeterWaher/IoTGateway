using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Runtime.Profiling.Events
{
	/// <summary>
	/// Event occurred
	/// </summary>
	public class Event : ProfilerEvent
	{
		private readonly string name;

		/// <summary>
		/// Event occurred
		/// </summary>
		/// <param name="Ticks">Elapsed ticks.</param>
		/// <param name="Name">Name of event.</param>
		public Event(long Ticks, string Name)
			: base(Ticks)
		{
			this.name = Name;
		}

		/// <summary>
		/// Name of event.
		/// </summary>
		public string Name => this.name;
	}
}
