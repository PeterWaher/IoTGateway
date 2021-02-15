using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Runtime.Profiling.Events
{
	/// <summary>
	/// Thread changes state.
	/// </summary>
	public class NewState : ProfilerEvent
	{
		private readonly string state;

		/// <summary>
		/// Thread changes state.
		/// </summary>
		/// <param name="Ticks">Elapsed ticks.</param>
		/// <param name="State">String representation of the new state.</param>
		public NewState(long Ticks, string State)
			: base(Ticks)
		{
			this.state = State;
		}

		/// <summary>
		/// String representation of the new state.
		/// </summary>
		public string State => this.state;
	}
}
