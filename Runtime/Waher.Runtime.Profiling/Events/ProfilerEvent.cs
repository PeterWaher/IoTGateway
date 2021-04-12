using System;
using System.Text;
using System.Xml;
using Waher.Runtime.Profiling.Export;

namespace Waher.Runtime.Profiling.Events
{
	/// <summary>
	/// Abstract base class for profiler events.
	/// </summary>
	public abstract class ProfilerEvent
	{
		private readonly long ticks;
		private readonly ProfilerThread thread;

		/// <summary>
		/// Abstract base class for profiler events.
		/// </summary>
		/// <param name="Ticks">Elapsed ticks.</param>
		/// <param name="Thread">Profiler thread generating the event.</param>
		public ProfilerEvent(long Ticks, ProfilerThread Thread)
		{
			this.ticks = Ticks;
			this.thread = Thread;
		}

		/// <summary>
		/// Elapsed ticks.
		/// </summary>
		public long Ticks => this.ticks;

		/// <summary>
		/// Profiler thread generating the event.
		/// </summary>
		public ProfilerThread Thread => this.thread;

		/// <summary>
		/// Type of event.
		/// </summary>
		public abstract string EventType
		{
			get;
		}

		/// <summary>
		/// Exports the event to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		/// <param name="Previous">Previous event. null=First event in thread.</param>
		/// <param name="TimeUnit">Time unit to use.</param>
		public virtual void ExportXml(XmlWriter Output, ProfilerEvent Previous, TimeUnit TimeUnit)
		{
			Output.WriteStartElement(this.EventType);
			this.ExportXmlAttributes(Output, Previous, TimeUnit);
			Output.WriteEndElement();
		}

		/// <summary>
		/// Exports event attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		/// <param name="Previous">Previous event. null=First event in thread.</param>
		/// <param name="TimeUnit">Time unit to use.</param>
		public virtual void ExportXmlAttributes(XmlWriter Output, ProfilerEvent Previous, TimeUnit TimeUnit)
		{
			Output.WriteAttributeString("ticks", this.ticks.ToString());
			Output.WriteAttributeString("time", this.thread.ToTimeStr(this.ticks, TimeUnit));

			if (!(Previous is null))
			{
				Output.WriteAttributeString("elapsedTicks", (this.ticks - Previous.ticks).ToString());
				Output.WriteAttributeString("elapsedTime", this.thread.ToTimeStr(this.ticks - Previous.ticks, TimeUnit));
			}
		}

		/// <summary>
		/// Exports events to PlantUML.
		/// </summary>
		/// <param name="States">PlantUML States</param>
		public virtual void ExportPlantUml(PlantUmlStates States)
		{
			StringBuilder Output = States.GetBuilder(this.ticks);
			Output.Append(this.thread.Key);
			Output.Append(" is ");
			Output.AppendLine(this.PlantUmlState);
		}

		/// <summary>
		/// PlantUML state representing event.
		/// </summary>
		public abstract string PlantUmlState
		{
			get;
		}

		/// <summary>
		/// Prepares the event for export to PlantUML
		/// </summary>
		public virtual void ExportPlantUmlPreparation()
		{
			// Do nothing by defualt.
		}

		/// <summary>
		/// Accumulates the event.
		/// </summary>
		/// <param name="Accumulator">Accumulator.</param>
		public virtual void Accumulate(Accumulator Accumulator)
		{
			Accumulator.AddAsIs(this);
		}
	}
}
