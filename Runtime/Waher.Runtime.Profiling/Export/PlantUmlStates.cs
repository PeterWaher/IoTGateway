using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Runtime.Profiling.Export
{
	/// <summary>
	/// Contains internal states used during generation of PlantUML diagram.s
	/// </summary>
	public class PlantUmlStates
	{
		/// <summary>
		/// Time unit.
		/// </summary>
		public readonly TimeUnit TimeUnit;

		/// <summary>
		/// Event parts of diagrams.
		/// </summary>
		public readonly SortedDictionary<long, StringBuilder> ByTime = new SortedDictionary<long, StringBuilder>();

		/// <summary>
		/// Summary part of diagrams.
		/// </summary>
		public readonly StringBuilder Summary = new StringBuilder();

		/// <summary>
		/// Contains internal states used during generation of PlantUML diagram.s
		/// </summary>
		/// <param name="TimeUnit">Time unit.</param>
		public PlantUmlStates(TimeUnit TimeUnit)
		{
			this.TimeUnit = TimeUnit;
		}

		/// <summary>
		/// Gets the PlantUML output <see cref="StringBuilder"/> associated with a time-point.
		/// </summary>
		/// <param name="Ticks">Tick count of builder.</param>
		/// <returns>PlantUML export <see cref="StringBuilder"/>.</returns>
		public StringBuilder GetBuilder(long Ticks)
		{
			if (!this.ByTime.TryGetValue(Ticks, out StringBuilder Output))
			{
				Output = new StringBuilder();
				this.ByTime[Ticks] = Output;
			}

			return Output;
		}
	}
}
