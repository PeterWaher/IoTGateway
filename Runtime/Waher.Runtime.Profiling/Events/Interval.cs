using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;
using Waher.Runtime.Profiling.Export;

namespace Waher.Runtime.Profiling.Events
{
	/// <summary>
	/// Displays an interval
	/// </summary>
	public class Interval : ProfilerEvent
	{
		private readonly string label;
		private readonly long toTicks;

		/// <summary>
		/// Thread changes state.
		/// </summary>
		/// <param name="FromTicks">Elapsed ticks to start of interval.</param>
		/// <param name="ToTicks">Elapsed ticks to end of interval.</param>
		/// <param name="Label">Label to display with the interval.</param>
		/// <param name="Thread">Profiler thread generating the event.</param>
		public Interval(long FromTicks, long ToTicks, string Label, ProfilerThread Thread)
			: base(FromTicks, Thread)
		{
			this.label = Label;
			this.toTicks = ToTicks;
		}

		/// <summary>
		/// String representation of the new state.
		/// </summary>
		public string State => this.label;

		/// <summary>
		/// Elapsed ticks to end of interval.
		/// </summary>
		public long ToTicks => this.toTicks;

		/// <inheritdoc/>
		public override string EventType => "Interval";

		/// <inheritdoc/>
		public override void ExportXmlAttributes(XmlWriter Output, ProfilerEvent Previous, TimeUnit TimeUnit)
		{
			Output.WriteAttributeString("label", this.label);
			Output.WriteAttributeString("toTicks", this.label);

			base.ExportXmlAttributes(Output, Previous, TimeUnit);
		}

		/// <inheritdoc/>
		public override string PlantUmlState
		{
			get
			{
				if (string.IsNullOrEmpty(this.label))
					return "\"\"";
				else
					return this.label;
			}
		}

		/// <inheritdoc/>
		public override void ExportPlantUml(PlantUmlStates States)
		{
			StringBuilder Output = States.GetBuilder(this.Ticks);
			Output.Append(this.Thread.Key);
			Output.Append('@');

			KeyValuePair<double, string> Time = this.Thread.Profiler.ToTime(this.Ticks, this.Thread, States.TimeUnit);
			Output.Append(Time.Key.ToString("F7").Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, "."));

			Output.Append(" <-> @");

			Time = this.Thread.Profiler.ToTime(this.toTicks, this.Thread, States.TimeUnit);
			Output.Append(Time.Key.ToString("F7").Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, "."));

			Output.Append(" : ");
			Output.AppendLine(this.label);
		}

	}
}
