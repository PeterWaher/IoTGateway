using System;
using System.Collections.Generic;
using System.Globalization;
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
		private int ordinal;

		/// <summary>
		/// Exception occurred
		/// </summary>
		/// <param name="Ticks">Elapsed ticks.</param>
		/// <param name="Exception">Exception object.</param>
		/// <param name="Thread">Profiler thread generating the event.</param>
		public Exception(long Ticks, System.Exception Exception, ProfilerThread Thread)
			: base(Ticks, Thread)
		{
			this.exception = Exception;
		}

		/// <summary>
		/// Exception object.
		/// </summary>
		public System.Exception ExceptionObject => this.exception;

		/// <inheritdoc/>
		public override string EventType => "Exception";

		/// <inheritdoc/>
		public override void ExportXmlAttributes(XmlWriter Output, ProfilerEvent Previous, TimeUnit TimeUnit)
		{
			Output.WriteAttributeString("type", this.exception.GetType().FullName);
			Output.WriteAttributeString("messsage", this.exception.Message);

			base.ExportXmlAttributes(Output, Previous, TimeUnit);

			Output.WriteElementString("StackTrace", this.exception.StackTrace);
		}

		/// <inheritdoc/>
		public override void ExportPlantUml(PlantUmlStates States)
		{
			StringBuilder Output = States.GetBuilder(this.Ticks);

			Output.Append("X");
			Output.Append(this.ordinal.ToString());
			Output.Append(" -> ");
			Output.AppendLine(this.Thread.Key);
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
