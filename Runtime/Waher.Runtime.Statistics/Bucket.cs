using System;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Persistence;
using Waher.Persistence.Attributes;
using Waher.Runtime.Collections;
using Waher.Script.Objects;
using Waher.Script.Units;

namespace Waher.Runtime.Statistics
{
	/// <summary>
	/// Statistical bucket
	/// </summary>
	[TypeName(TypeNameSerialization.None)]
	[CollectionName("SampleBuckets")]
	[Index("Id")]
	[Index("Stop", "Id")]
	public class Bucket
	{
		private string objectId = null;
		private ChunkedList<double> samples;
		private string id;
		private bool retainSamplesInPeriod;
		private Duration bucketTime;
		private DateTime start;
		private DateTime stop;
		private string unit = null;
		private Unit parsedUnit = null;
		private long count = 0;
		private long totCount = 0;
		private long relativeCounter = 0;
		private double sum = 0;
		private double min = double.MaxValue;
		private double max = double.MinValue;
		private bool hasSamples = false;
		private bool persistSamples = false;

		/// <summary>
		/// Statistical bucket
		/// </summary>
		public Bucket()
			: this(null, false, DateTime.MinValue, Duration.Zero, false)
		{
		}

		/// <summary>
		/// Statistical bucket
		/// </summary>
		/// <param name="Id">ID of bucket.</param>
		/// <param name="RetainSamplesInPeriod">If standard deviation, variance or median 
		/// values are to be calculated, samples need to be retained in the period.</param>
		/// <param name="StartTime">Starting time</param>
		/// <param name="BucketTime">Duration of one bucket, where statistics is collected.</param>
		/// <param name="PersistSamples">If samples generated should be persisted.</param>
		public Bucket(string Id, bool RetainSamplesInPeriod, DateTime StartTime, 
			Duration BucketTime, bool PersistSamples)
		{
			this.id = Id;
			this.samples = RetainSamplesInPeriod ? new ChunkedList<double>() : null;
			this.retainSamplesInPeriod = RetainSamplesInPeriod;
			this.bucketTime = BucketTime;
			this.start = StartTime;
			this.stop = this.start + BucketTime;
			this.persistSamples = PersistSamples;
		}

		/// <summary>
		/// Object ID
		/// </summary>
		[ObjectId]
		public string ObjectId
		{
			get => this.objectId;
			set => this.objectId = value;
		}

		/// <summary>
		/// Bucket ID
		/// </summary>
		public string Id
		{
			get => this.id;
			set => this.id = value;
		}

		/// <summary>
		/// Time to accumulate values.
		/// </summary>
		public Duration BucketTime
		{
			get => this.bucketTime;
			set => this.bucketTime = value;
		}

		/// <summary>
		/// Samples in period, if retained.
		/// </summary>
		public double[] Samples
		{
			get => this.samples?.ToArray();
			set
			{
				if (value is null)
					this.samples = null;
				else
					this.samples = new ChunkedList<double>(value);
			}
		}

		/// <summary>
		/// If samples are to be retained in the period.
		/// </summary>
		public bool RetainSamplesInPeriod
		{
			get => this.retainSamplesInPeriod;
			set => this.retainSamplesInPeriod = value;
		}

		/// <summary>
		/// If samples generated should be persisted.
		/// </summary>
		public bool PersistSamples
		{
			get => this.persistSamples;
			set => this.persistSamples = value;
		}

		/// <summary>
		/// Start of period.
		/// </summary>
		public DateTime Start
		{
			get => this.start;
			set => this.start = value;
		}

		/// <summary>
		/// End of period.
		/// </summary>
		public DateTime Stop
		{
			get => this.stop;
			set => this.stop = value;
		}

		/// <summary>
		/// Counter
		/// </summary>
		public long Count
		{
			get
			{
				lock (this)
				{
					return this.count;
				}
			}

			set
			{
				lock (this)
				{
					this.count = value;
				}
			}
		}

		/// <summary>
		/// Total Counter
		/// </summary>
		public long TotalCount
		{
			get
			{
				lock (this)
				{
					return this.totCount;
				}
			}

			set
			{
				lock (this)
				{
					this.totCount = value;
				}
			}
		}

		/// <summary>
		/// Relative Counter
		/// </summary>
		public long RelativeCounter
		{
			get
			{
				lock (this)
				{
					return this.relativeCounter;
				}
			}

			set
			{
				lock (this)
				{
					this.relativeCounter = value;
				}
			}
		}

		/// <summary>
		/// If bucket has samples.
		/// </summary>
		public bool HasSamples
		{
			get
			{
				lock (this)
				{
					return this.hasSamples;
				}
			}

			set
			{
				lock (this)
				{
					this.hasSamples = value;
				}
			}
		}

		/// <summary>
		/// Sum of samples.
		/// </summary>
		public double Sum
		{
			get
			{
				lock (this)
				{
					return this.sum;
				}
			}

			set
			{
				lock (this)
				{
					this.sum = value;
				}
			}
		}

		/// <summary>
		/// Smallest sample
		/// </summary>
		public double Min
		{
			get
			{
				lock (this)
				{
					return this.min;
				}
			}

			set
			{
				lock (this)
				{
					this.min = value;
				}
			}
		}

		/// <summary>
		/// Largest sample
		/// </summary>
		public double Max
		{
			get
			{
				lock (this)
				{
					return this.max;
				}
			}

			set
			{
				lock (this)
				{
					this.max = value;
				}
			}
		}

		/// <summary>
		/// Mean (average) value of samples.
		/// </summary>
		public double Mean
		{
			get
			{
				lock (this)
				{
					return this.sum / this.count;
				}
			}
		}

		/// <summary>
		/// Unit used.
		/// </summary>
		public string Unit
		{
			get
			{
				lock (this)
				{
					return this.unit;
				}
			}

			set
			{
				lock (this)
				{
					if (string.IsNullOrEmpty(value))
					{
						this.parsedUnit = null;
						this.unit = null;
					}
					else if (Script.Units.Unit.TryParse(value, out this.parsedUnit))
						this.unit = value;
					else
						throw new ArgumentException("Invalid unit.", nameof(value));
				}
			}
		}

		/// <summary>
		/// Increments counter.
		/// </summary>
		/// <returns>Sample statistic of last period, if period changed.</returns>
		public Task<SampleStatistic> Inc()
		{
			DateTime Timestamp = DateTime.UtcNow;
			long Count;

			lock (this)
			{
				Count = ++this.relativeCounter;
			}

			return this.Sample(Timestamp, Count);
		}

		/// <summary>
		/// Decrements counter.
		/// </summary>
		/// <returns>Sample statistic of last period, if period changed.</returns>
		public Task<SampleStatistic> Dec()
		{
			DateTime Timestamp = DateTime.UtcNow;
			long Count;

			lock (this)
			{
				Count = --this.relativeCounter;
			}

			return this.Sample(Timestamp, Count);
		}

		private SampleStatistic NextStatisticLocked()
		{
			SampleStatistic Result;

			if (this.hasSamples)
			{
				double Mean = this.sum / this.count;

				if (this.retainSamplesInPeriod)
				{
					double Variance = this.CalcVarianceLocked();
					double StdDev = Math.Sqrt(Variance);

					Result = new SampleStatistic()
					{
						Id = this.id,
						Start = this.start,
						Stop = this.stop,
						Count = this.count,
						Mean = Mean,
						Variance = Variance,
						StdDev = StdDev,
						Median = this.CalcMedianLocked(),
						Min = this.min,
						Max = this.max,
						Unit = this.unit
					};

					this.samples.Clear();
				}
				else
				{
					Result = new SampleStatistic()
					{
						Id = this.id,
						Start = this.start,
						Stop = this.stop,
						Count = this.count,
						Mean = Mean,
						Min = this.min,
						Max = this.max,
						Unit = this.unit
					};
				}

				this.hasSamples = false;
				this.sum = 0;
				this.min = double.MaxValue;
				this.max = double.MinValue;
			}
			else
			{
				Result = new SampleStatistic()
				{
					Id = this.id,
					Start = this.start,
					Stop = this.stop,
					Count = this.count,
					Unit = this.unit
				};
			}

			this.count = 0;
			this.start = this.stop;
			this.stop = this.start + this.bucketTime;

			return Result;
		}

		/// <summary>
		/// Adds a sample
		/// </summary>
		/// <param name="Timestamp">Timestamp of value.</param>
		/// <param name="Value">Sample value reported</param>
		/// <returns>Sample statistic of last period, if period changed.</returns>
		public Task<SampleStatistic> Sample(DateTime Timestamp, PhysicalQuantity Value)
		{
			double v;

			if (this.unit is null)
			{
				this.parsedUnit = Value.Unit;
				this.unit = this.parsedUnit.ToString();
				v = Value.Magnitude;
			}
			else if (!Script.Units.Unit.TryConvert(Value.Magnitude, Value.Unit, this.parsedUnit, out v))
				throw new Exception("Incompatible units: " + Value.Unit.ToString() + " and " + this.unit.ToString());

			return this.Sample(Timestamp, v);
		}

		/// <summary>
		/// Adds a sample
		/// </summary>
		/// <param name="Timestamp">Timestamp of value.</param>
		/// <param name="Value">Sample value reported</param>
		/// <returns>Sample statistic of last period, if period changed.</returns>
		public async Task<SampleStatistic> Sample(DateTime Timestamp, double Value)
		{
			ChunkedList<SampleStatistic> Statistics = null;
			SampleStatistic Statistic = null;
			DateTime Result;

			lock (this)
			{
				while (Timestamp >= this.stop)
				{
					if (!(Statistic is null))
					{
						Statistics ??= new ChunkedList<SampleStatistic>();
						Statistics.Add(Statistic);
					}

					Statistic = this.NextStatisticLocked();
				}

				this.sum += Value;
				this.count++;
				this.totCount++;

				if (this.hasSamples)
				{
					if (Value < this.min)
						this.min = Value;
					else if (Value > this.max)
						this.max = Value;
				}
				else
				{
					if (Value < this.min)
						this.min = Value;

					if (Value > this.max)
						this.max = Value;

					this.hasSamples = true;
				}

				this.samples?.Add(Value);

				Result = this.start;
			}

			if (this.persistSamples && !(Statistic is null))
			{
				if (Statistics is null)
					await Database.Insert(Statistic);
				else
				{
					Statistics.Add(Statistic);
					await Database.Insert(Statistics);
				}
			}

			if (!string.IsNullOrEmpty(this.objectId))
				await Database.Update(this);

			return Statistic;
		}

		/// <summary>
		/// Counts one occurrence
		/// </summary>
		/// <param name="Timestamp">Timestamp of occurrence.</param>
		/// <returns>Start time of bucket to which the value was reported.</returns>
		public async Task<DateTime> CountOccurrence(DateTime Timestamp)
		{
			ChunkedList<SampleStatistic> Statistics = null;
			SampleStatistic Statistic = null;
			DateTime Result;

			lock (this)
			{
				while (Timestamp >= this.stop)
				{
					if (!(Statistic is null))
					{
						Statistics ??= new ChunkedList<SampleStatistic>();
						Statistics.Add(Statistic);
					}

					Statistic = this.NextStatisticLocked();
				}

				this.count++;
				this.totCount++;

				Result = this.start;
			}

			if (this.persistSamples && !(Statistic is null))
			{
				if (Statistics is null)
					await Database.Insert(Statistic);
				else
				{
					Statistics.Add(Statistic);
					await Database.Insert(Statistics);
				}
			}

			if (!string.IsNullOrEmpty(this.objectId))
				await Database.Update(this);

			return Result;
		}

		/// <summary>
		/// Variance of samples
		/// </summary>
		public double Variance
		{
			get
			{
				if (this.samples is null)
					throw new InvalidOperationException("Bucket not prepared for calculating variances or standard deviations.");

				lock (this)
				{
					return this.CalcVarianceLocked();
				}
			}
		}

		/// <summary>
		/// (Biased) standard deviation of samples
		/// </summary>
		public double StdDev => Math.Sqrt(this.Variance);

		/// <summary>
		/// Median of samples
		/// </summary>
		public double? Median
		{
			get
			{
				if (this.samples is null)
					throw new InvalidOperationException("Bucket not prepared for calculating medians.");

				lock (this)
				{
					return this.CalcMedianLocked();
				}
			}
		}

		private double CalcVarianceLocked()
		{
			double μ = this.sum / this.count;
			double S = 0;
			double x;

			foreach (double d in this.samples)
			{
				x = d - μ;
				S += x * x;
			}

			return S / this.count;
		}

		private double? CalcMedianLocked()
		{
			int c = this.samples.Count;

			if ((c & 1) == 1)
				return this.samples[c >> 1];
			else if (c == 0)
				return null;
			else
			{
				int c2 = c >> 1;
				double Left = this.samples[c2 - 1];
				double Right = this.samples[c2];
				return (Left + Right) / 2;
			}
		}

		/// <summary>
		/// Terminates the ongoing collection of data.
		/// </summary>
		/// <returns>Sample statistic of last period, if period changed.</returns>
		public async Task<SampleStatistic> FlushAsync()
		{
			SampleStatistic Statistic;

			lock (this)
			{
				if (this.count == 0)
					return null;

				Statistic = this.NextStatisticLocked();
			}

			if (this.persistSamples)
				await Database.Insert(Statistic);

			if (!string.IsNullOrEmpty(this.objectId))
				await Database.Update(this);

			return Statistic;
		}
	}
}
