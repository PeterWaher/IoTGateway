using System;
using System.Xml;

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
		/// <param name="Thread">Profiler thread generating the event.</param>
		public NewState(long Ticks, string State, ProfilerThread Thread)
			: base(Ticks, Thread)
		{
			this.state = State;
		}

		/// <summary>
		/// String representation of the new state.
		/// </summary>
		public string State => this.state;

		/// <inheritdoc/>
		public override string EventType => "NewState";

		/// <inheritdoc/>
		public override void ExportXmlAttributes(XmlWriter Output, ProfilerEvent Previous, TimeUnit TimeUnit)
		{
			Output.WriteAttributeString("state", this.state);

			base.ExportXmlAttributes(Output, Previous, TimeUnit);
		}

		/// <inheritdoc/>
		public override string PlantUmlState => this.state;
	}
}
