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
		private readonly string label;
		private int ordinal;

		/// <summary>
		/// Event occurred
		/// </summary>
		/// <param name="Ticks">Elapsed ticks.</param>
		/// <param name="Name">Name of event.</param>
		/// <param name="Thread">Profiler thread generating the event.</param>
		public Event(long Ticks, string Name, ProfilerThread Thread)
			: this(Ticks, Name, string.Empty, Thread)
		{
		}

		/// <summary>
		/// Event occurred
		/// </summary>
		/// <param name="Ticks">Elapsed ticks.</param>
		/// <param name="Name">Name of event.</param>
		/// <param name="Label">Optional label.</param>
		/// <param name="Thread">Profiler thread generating the event.</param>
		public Event(long Ticks, string Name, string Label, ProfilerThread Thread)
			: base(Ticks, Thread)
		{
			this.name = Name;
			this.label = Label;
		}

		/// <summary>
		/// Name of event.
		/// </summary>
		public string Name => this.name;

		/// <summary>
		/// Name of event.
		/// </summary>
		public string Label => this.label;

		/// <inheritdoc/>
		public override string EventType => "Event";

		/// <inheritdoc/>
		public override void ExportXmlAttributes(XmlWriter Output, ProfilerEvent Previous, TimeUnit TimeUnit)
		{
			Output.WriteAttributeString("name", this.name);

			if (!string.IsNullOrEmpty(this.label))
				Output.WriteAttributeString("label", this.label);

			base.ExportXmlAttributes(Output, Previous, TimeUnit);
		}

		/// <inheritdoc/>
		public override void ExportPlantUml(PlantUmlStates States)
		{
			StringBuilder Output = States.GetBuilder(this.Ticks);

			Output.Append('E');
			Output.Append(this.ordinal.ToString());
			Output.Append(" -> ");
			Output.Append(this.Thread.Key);

			if (!string.IsNullOrEmpty(this.label))
			{
				Output.Append(" : \"");
				Output.Append(this.label.Replace('"', '\''));
				Output.Append('"');
			}

			Output.AppendLine();
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
