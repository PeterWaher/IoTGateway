using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Processors.Metering.NodeTypes.Comparisons;
using Waher.Runtime.Collections;
using Waher.Runtime.Language;
using Waher.Runtime.Statistics;
using Waher.Script.Objects;
using Waher.Security;
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
		private static readonly Dictionary<string, Buckets> buckets = new Dictionary<string, Buckets>();

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
		[Header(53, "Calculate Average.", 70)]
		[Page(21, "Processor", 0)]
		[ToolTip(54, "If checked, the average of the samples will be calcaulted.")]
		public bool CalculateAverage { get; set; }

		/// <summary>
		/// Calculates the median value in the period.
		/// </summary>
		[Header(55, "Calculate Median.", 80)]
		[Page(21, "Processor", 0)]
		[ToolTip(56, "If checked, the median of the samples will be calcaulted.")]
		public bool CalculateMedian { get; set; }

		/// <summary>
		/// Calculates the minimum value in the period.
		/// </summary>
		[Header(57, "Calculate Minimum.", 90)]
		[Page(21, "Processor", 0)]
		[ToolTip(58, "If checked, the minimum of the samples will be calcaulted.")]
		public bool CalculateMin { get; set; }

		/// <summary>
		/// Calculates the maximum value in the period.
		/// </summary>
		[Header(59, "Calculate Maximum.", 100)]
		[Page(21, "Processor", 0)]
		[ToolTip(60, "If checked, the maximum of the samples will be calcaulted.")]
		public bool CalculateMax { get; set; }

		/// <summary>
		/// Calculates the variance of the values in the period.
		/// </summary>
		[Header(61, "Calculate Variance.", 110)]
		[Page(21, "Processor", 0)]
		[ToolTip(62, "If checked, the variance of the samples will be calcaulted.")]
		public bool CalculateVariance { get; set; }

		/// <summary>
		/// Calculates the standard deviation of the values in the period.
		/// </summary>
		[Header(63, "Calculate Standard Deviation.", 120)]
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
					QuantityField.Value, QuantityField.NrDecimals, QuantityField.Unit);
			}
			else if (Field is Int32Field Int32Field)
			{
				return this.ComputeStatisticsAsync(Sensor, Field,
					Int32Field.Value, 0, null);
			}
			else if (Field is Int64Field Int64Field)
			{
				return this.ComputeStatisticsAsync(Sensor, Field,
					Int64Field.Value, 0, null);
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
			double Value, byte NrDec, string Unit)
		{
			Buckets NodeBuckets;
			Duration Period;

			lock (buckets)
			{
				if (buckets.TryGetValue(this.NodeId, out NodeBuckets))
				{
					Period = NodeBuckets.Period;

					if (Period.Seconds != this.PeriodSeconds ||
						Period.Minutes != 0 ||
						Period.Hours != 0 ||
						Period.Days != 0 ||
						Period.Months != 0 ||
						Period.Years != 0 ||
						Period.Negation)
					{
						NodeBuckets = null;
					}
				}
				else
					NodeBuckets = null;

				if (NodeBuckets is null)
				{
					DateTime Now = DateTime.UtcNow;
					TimeSpan Span = Now.Subtract(JSON.UnixEpoch);
					double Seconds = Span.TotalSeconds;
					Seconds -= Math.IEEERemainder(Seconds, this.PeriodSeconds);
					DateTime Start = JSON.UnixEpoch.AddSeconds(Seconds);

					Period = new Duration(false, 0, 0, 0, 0, 0, this.PeriodSeconds);
					NodeBuckets = new Buckets(Start, Period, true, false, this.NeedsSampleInstances);

					buckets[this.NodeId] = NodeBuckets;
				}
			}

			StringBuilder sb = new StringBuilder();
			sb.Append(Sensor.NodeId);
			sb.Append('|');
			sb.Append(Sensor.SourceId);
			sb.Append('|');
			sb.Append(Sensor.Partition);
			sb.Append('|');
			sb.Append(Field.Name);
			sb.Append('|');
			sb.Append(((int)Field.Type).ToString());

			byte[] BucketId = Hashes.ComputeSHA256Hash(Encoding.UTF8.GetBytes(sb.ToString()));
			SampleStatistic Statistic;

			if (string.IsNullOrEmpty(Unit))
				Statistic = await NodeBuckets.Sample(BucketId, Value);
			else if (Script.Units.Unit.TryParse(Unit, out Script.Units.Unit ParsedUnit))
				Statistic = await NodeBuckets.Sample(BucketId, new PhysicalQuantity(Value, ParsedUnit));
			else
				Statistic = null;

			if (Statistic is null)
			{
				if (this.ReplaceCompatible)
					return null;
				else
					return new Field[] { Field };
			}

			ChunkedList<Field> Fields = new ChunkedList<Field>();
			byte AvgNrDec;

			if (Statistic.Count > 1)
				AvgNrDec = (byte)(NrDec + Math.Floor(Math.Log10(Statistic.Count)));
			else
				AvgNrDec = NrDec;

			if (!this.ReplaceCompatible)
				Fields.Add(Field);

			Fields.Add(new DateTimeField(Field.Thing, Statistic.Stop,
				"Period, Start", Statistic.Start, FieldType.Computed,
				FieldQoS.AutomaticReadout, false));

			Fields.Add(new DateTimeField(Field.Thing, Statistic.Stop,
				"Period, Stop", Statistic.Stop, FieldType.Computed,
				FieldQoS.AutomaticReadout, false));

			// TODO: Localization steps

			if (this.CalculateCount)
			{
				Fields.Add(new Int64Field(Field.Thing, Statistic.Stop,
					Field.Name + ", Count", Statistic.Count, FieldType.Computed,
					FieldQoS.AutomaticReadout, false));

				// TODO: Localization steps
			}

			if (this.CalculateAverage && Statistic.Mean.HasValue)
			{
				Fields.Add(new QuantityField(Field.Thing, Statistic.Stop,
					Field.Name + ", Mean", Statistic.Mean.Value, AvgNrDec, Statistic.Unit,
					FieldType.Computed, FieldQoS.AutomaticReadout, false));

				// TODO: Localization steps
			}

			if (this.CalculateMedian && Statistic.Median.HasValue)
			{
				Fields.Add(new QuantityField(Field.Thing, Statistic.Stop,
					Field.Name + ", Median", Statistic.Median.Value, NrDec, Statistic.Unit,
					FieldType.Computed, FieldQoS.AutomaticReadout, false));

				// TODO: Localization steps
			}

			if (this.CalculateMin && Statistic.Min.HasValue)
			{
				Fields.Add(new QuantityField(Field.Thing, Statistic.Stop,
					Field.Name + ", Min", Statistic.Min.Value, NrDec, Statistic.Unit,
					FieldType.Peak, FieldQoS.AutomaticReadout, false));

				// TODO: Localization steps
			}

			if (this.CalculateMax && Statistic.Max.HasValue)
			{
				Fields.Add(new QuantityField(Field.Thing, Statistic.Stop,
					Field.Name + ", Max", Statistic.Max.Value, NrDec, Statistic.Unit,
					FieldType.Peak, FieldQoS.AutomaticReadout, false));

				// TODO: Localization steps
			}

			if (this.CalculateVariance && Statistic.Variance.HasValue)
			{
				Fields.Add(new QuantityField(Field.Thing, Statistic.Stop,
					Field.Name + ", Variance", Statistic.Variance.Value, AvgNrDec, Statistic.Unit,
					FieldType.Computed, FieldQoS.AutomaticReadout, false));

				// TODO: Localization steps
			}

			if (this.CalculateStdDev && Statistic.StdDev.HasValue)
			{
				Fields.Add(new QuantityField(Field.Thing, Statistic.Stop,
					Field.Name + ", StdDev", Statistic.StdDev.Value, AvgNrDec, Statistic.Unit,
					FieldType.Computed, FieldQoS.AutomaticReadout, false));

				// TODO: Localization steps
			}

			return Fields.ToArray();
		}

		/// <summary>
		/// Destroys the node. If it is a child to a parent node, it is removed from the parent first.
		/// </summary>
		public override Task DestroyAsync()
		{
			lock (buckets)
			{
				buckets.Remove(this.NodeId);
			}

			return base.DestroyAsync();
		}

		/// <summary>
		/// Persists changes to the node, and generates a node updated event.
		/// </summary>
		protected override Task NodeUpdated()
		{
			if (this.OldId != this.NodeId)
			{
				lock (buckets)
				{
					if (buckets.TryGetValue(this.OldId, out Buckets Buckets))
						buckets[this.NodeId] = Buckets;

					buckets.Remove(this.OldId);
				}
			}

			return base.NodeUpdated();
		}
	}
}
