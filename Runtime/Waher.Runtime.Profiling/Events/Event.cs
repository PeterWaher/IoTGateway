using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;
using Waher.Runtime.Profiling.Export;

namespace Waher.Runtime.Profiling.Events
{
	/// <summary>
	/// Event occurred
	/// </summary>
	public class Event : ProfilerEvent
	{
		private readonly string name;
		private int ordinal;
		
		/// <summary>
		/// Event occurred
		/// </summary>
		/// <param name="Ticks">Elapsed ticks.</param>
		/// <param name="Name">Name of event.</param>
		/// <param name="Thread">Profiler thread generating the event.</param>
		public Event(long Ticks, string Name, ProfilerThread Thread)
			: base(Ticks, Thread)
		{
			this.name = Name;
		}

		/// <summary>
		/// Name of event.
		/// </summary>
		public string Name => this.name;

		/// <inheritdoc/>
		public override string EventType => "Event";

		/// <inheritdoc/>
		public override void ExportXmlAttributes(XmlWriter Output, ProfilerEvent Previous, TimeUnit TimeUnit)
		{
			Output.WriteAttributeString("name", this.name);

			base.ExportXmlAttributes(Output, Previous, TimeUnit);
		}

		/// <inheritdoc/>
		public override void ExportPlantUml(PlantUmlStates States)
		{
			StringBuilder Output = States.GetBuilder(this.Ticks);

			Output.Append("E");
			Output.Append(this.ordinal.ToString());
			Output.Append(" -> ");
			Output.AppendLine(this.Thread.Key);
		}

		/// <inheritdoc/>
		public override string PlantUmlState => string.Empty;

		/// <inheritdoc/>
		public override void ExportPlantUmlPreparation()
		{
			this.ordinal = this.Thread.Profiler.GetEventOrdinal(this.name);
		}
	}
}
