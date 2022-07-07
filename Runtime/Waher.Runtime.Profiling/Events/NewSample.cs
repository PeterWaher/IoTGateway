using System.Globalization;
using System.Xml;

namespace Waher.Runtime.Profiling.Events
{
	/// <summary>
	/// Contains a new sample value.
	/// </summary>
	public class NewSample : ProfilerEvent
	{
		private readonly double sample;

		/// <summary>
		/// Contains a new sample value.
		/// </summary>
		/// <param name="Ticks">Elapsed ticks.</param>
		/// <param name="Sample">Sample value.</param>
		/// <param name="Thread">Profiler thread generating the event.</param>
		public NewSample(long Ticks, double Sample, ProfilerThread Thread)
			: base(Ticks, Thread)
		{
			this.sample = Sample;
		}

		/// <summary>
		/// Sample value.
		/// </summary>
		public double Sample => this.sample;

		/// <inheritdoc/>
		public override string EventType => "NewSample";

		/// <inheritdoc/>
		public override void ExportXmlAttributes(XmlWriter Output, ProfilerEvent Previous, TimeUnit TimeUnit)
		{
			Output.WriteAttributeString("sample", this.PlantUmlState);

			base.ExportXmlAttributes(Output, Previous, TimeUnit);
		}

		/// <inheritdoc/>
		public override string PlantUmlState
		{
			get
			{
				return this.sample.ToString().Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, ".");
			}
		}
	}
}
