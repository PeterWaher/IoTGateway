using System.Threading.Tasks;
using Waher.Processors.Metering.NodeTypes.Comparisons;
using Waher.Runtime.Language;
using Waher.Things;
using Waher.Things.Attributes;
using Waher.Things.SensorData;

namespace Waher.Processors.Metering.NodeTypes.Calculations
{
	/// <summary>
	/// Calculates statistics of momentary values over a period of time.
	/// </summary>
	public class Statistics : DecisionTreeLeafStatement
	{
		/// <summary>
		/// Calculates statistics of momentary values over a period of time.
		/// </summary>
		public Statistics()
			: base()
		{
		}

		/// <summary>
		/// Aggregation period.
		/// </summary>
		[Header(42, "Period (s):", 30)]
		[Page(21, "Processor", 0)]
		[ToolTip(43, "Time period, in seconds, over which statistics is computed.")]
		[Required]
		public int PeriodSeconds { get; set; }

		/// <summary>
		/// If incompatible fields are to be rejected.
		/// </summary>
		[Header(49, "Persisted state.", 30)]
		[Page(21, "Processor", 0)]
		[ToolTip(50, "If checked, All computations will be persisted, allowing for server to be restarted in mid period.")]
		public bool PersistedState { get; set; }

		/// <summary>
		/// If incompatible fields are to be rejected.
		/// </summary>
		[Header(44, "Reject incompatible fields.", 40)]
		[Page(21, "Processor", 0)]
		[ToolTip(45, "If checked, all non-momentary numerical values will be rejected.")]
		public bool RejectIncompatible { get; set; }

		/// <summary>
		/// If compatible fields are to be replaced with the statistical aggregate values.
		/// </summary>
		[Header(46, "Replace sampled values.", 50)]
		[Page(21, "Processor", 0)]
		[ToolTip(47, "If checked, all field values used in the calculation will be rejected after they have been used to calculate the aggregate values.")]
		public bool ReplaceCompatible { get; set; }

		/// <summary>
		/// Calculate the number of samples in the period.
		/// </summary>
		[Header(51, "Count samples.", 60)]
		[Page(21, "Processor", 0)]
		[ToolTip(52, "If checked, the count of samples will be calcaulted.")]
		public bool CalculateCount { get; set; }

		/// <summary>
		/// Calculates the average value in the period.
		/// </summary>
		[Header(53, "Calcaulte Average.", 70)]
		[Page(21, "Processor", 0)]
		[ToolTip(54, "If checked, the average of the samples will be calcaulted.")]
		public bool CalculateAverage { get; set; }

		/// <summary>
		/// Calculates the median value in the period.
		/// </summary>
		[Header(55, "Calcaulte Median.", 80)]
		[Page(21, "Processor", 0)]
		[ToolTip(56, "If checked, the median of the samples will be calcaulted.")]
		public bool CalculateMedian { get; set; }

		/// <summary>
		/// Calculates the minimum value in the period.
		/// </summary>
		[Header(57, "Calcaulte Minimum.", 90)]
		[Page(21, "Processor", 0)]
		[ToolTip(58, "If checked, the minimum of the samples will be calcaulted.")]
		public bool CalculateMin { get; set; }

		/// <summary>
		/// Calculates the maximum value in the period.
		/// </summary>
		[Header(59, "Calcaulte Maximum.", 100)]
		[Page(21, "Processor", 0)]
		[ToolTip(60, "If checked, the maximum of the samples will be calcaulted.")]
		public bool CalculateMax { get; set; }

		/// <summary>
		/// Calculates the variance of the values in the period.
		/// </summary>
		[Header(61, "Calcaulte Variance.", 110)]
		[Page(21, "Processor", 0)]
		[ToolTip(62, "If checked, the variance of the samples will be calcaulted.")]
		public bool CalculateVariance { get; set; }

		/// <summary>
		/// Calculates the standard deviation of the values in the period.
		/// </summary>
		[Header(63, "Calcaulte Standard Deviation.", 120)]
		[Page(21, "Processor", 0)]
		[ToolTip(64, "If checked, the standard deviation of the samples will be calcaulted.")]
		public bool CalculateStdDev { get; set; }

		/// <summary>
		/// If the individual sample value instances are needed to compute the final 
		/// statistical aggregates.
		/// </summary>
		public bool NeedsSampleInstances =>
			this.CalculateMedian ||
			this.CalculateVariance ||
			this.CalculateStdDev;

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(Conditional), 48, "Statistics");
		}

		/// <summary>
		/// Processes a single sensor data field.
		/// </summary>
		/// <param name="Sensor">Sensor reporting the field.</param>
		/// <param name="Field">Field to process.</param>
		/// <returns>Processed set of fields. Can be null if field does not pass processing.</returns>
		public override Task<Field[]> ProcessField(ISensor Sensor, Field Field)
		{
			if (Field is QuantityField QuantityField)
			{
				return this.ComputeStatisticsAsync(Sensor, Field,
					QuantityField.Value, QuantityField.Unit);
			}
			else if (Field is Int32Field Int32Field)
			{
				return this.ComputeStatisticsAsync(Sensor, Field, 
					Int32Field.Value, null);
			}
			else if (Field is Int64Field Int64Field)
			{
				return this.ComputeStatisticsAsync(Sensor, Field,
					Int64Field.Value, null);
			}
			else
			{
				if (this.RejectIncompatible)
					return Task.FromResult<Field[]>(null);
				else
					return Task.FromResult(new Field[] { Field });
			}
		}

		private async Task<Field[]> ComputeStatisticsAsync(ISensor Sensor, Field Field,
			double Value, string Unit)
		{
			return null;
		}
	}
}
