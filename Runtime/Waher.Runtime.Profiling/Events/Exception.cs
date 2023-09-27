using System.Text;
using System.Xml;
using Waher.Runtime.Profiling.Export;

namespace Waher.Runtime.Profiling.Events
{
	/// <summary>
	/// Exception occurred
	/// </summary>
	public class Exception : ProfilerEvent
	{
		private readonly System.Exception exception;
		private readonly string label;
		private int ordinal;

		/// <summary>
		/// Exception occurred
		/// </summary>
		/// <param name="Ticks">Elapsed ticks.</param>
		/// <param name="Exception">Exception object.</param>
		/// <param name="Thread">Profiler thread generating the event.</param>
		public Exception(long Ticks, System.Exception Exception, ProfilerThread Thread)
			: this(Ticks, Exception, string.Empty, Thread)
		{
		}

		/// <summary>
		/// Exception occurred
		/// </summary>
		/// <param name="Ticks">Elapsed ticks.</param>
		/// <param name="Exception">Exception object.</param>
		/// <param name="Label">Optional label.</param>
		/// <param name="Thread">Profiler thread generating the event.</param>
		public Exception(long Ticks, System.Exception Exception, string Label, ProfilerThread Thread)
			: base(Ticks, Thread)
		{
			this.exception = Exception;
			this.label = Label;
		}

		/// <summary>
		/// Exception object.
		/// </summary>
		public System.Exception ExceptionObject => this.exception;

		/// <summary>
		/// Name of event.
		/// </summary>
		public string Label => this.label;

		/// <inheritdoc/>
		public override string EventType => "Exception";

		/// <inheritdoc/>
		public override void ExportXmlAttributes(XmlWriter Output, ProfilerEvent Previous, TimeUnit TimeUnit)
		{
			Output.WriteAttributeString("type", this.exception.GetType().FullName);
			Output.WriteAttributeString("messsage", this.exception.Message);

			if (!string.IsNullOrEmpty(this.label))
				Output.WriteAttributeString("label", this.label);

			base.ExportXmlAttributes(Output, Previous, TimeUnit);

			Output.WriteElementString("StackTrace", this.exception.StackTrace);
		}

		/// <inheritdoc/>
		public override void ExportPlantUml(PlantUmlStates States)
		{
			StringBuilder Output = States.GetBuilder(this.Ticks);

			Output.Append('X');
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
			this.ordinal = this.Thread.Profiler.GetExceptionOrdinal(this.exception);
		}
	}
}
