using System;
using Waher.Persistence.Attributes;

namespace Waher.Runtime.Statistics
{
	/// <summary>
	/// Represents collected statistical information from a small portion of time.
	/// </summary>
	[TypeName(TypeNameSerialization.None)]
	[CollectionName("SampleStatistics")]
	[Index("Id", "Start")]
	[Index("Start", "Id")]
	public class SampleStatistic
	{
		private string objectId = null;
		private string id = null;
		private DateTime start = DateTime.MinValue;
		private DateTime stop = DateTime.MinValue;
		private string unit = null;
		private long count = 0;
		private double? mean = null;
		private double? median = null;
		private double? variance = null;
		private double? stdDev = null;
		private double? min = null;
		private double? max = null;

		/// <summary>
		/// Represents collected statistical information from a small portion of time.
		/// </summary>
		public SampleStatistic()
		{
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
		/// Series ID
		/// </summary>
		public string Id
		{
			get => this.id;
			set => this.id = value;
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
		[DefaultValueDateTimeMinValue]
		public DateTime Stop
		{
			get => this.stop;
			set => this.stop = value;
		}

		/// <summary>
		/// Number of events
		/// </summary>
		[DefaultValue(0L)]
		public long Count
		{
			get => this.count;
			set => this.count = value;
		}

		/// <summary>
		/// Mean value
		/// </summary>
		[DefaultValueNull]
		public double? Mean
		{
			get => this.mean;
			set => this.mean = value;
		}

		/// <summary>
		/// Variance of values
		/// </summary>
		[DefaultValueNull]
		public double? Variance
		{
			get => this.variance;
			set => this.variance = value;
		}

		/// <summary>
		/// Standard deviation of values
		/// </summary>
		[DefaultValueNull]
		public double? StdDev
		{
			get => this.stdDev;
			set => this.stdDev = value;
		}

		/// <summary>
		/// Standard deviation of values
		/// </summary>
		[DefaultValueNull]
		public double? Median
		{
			get => this.median;
			set => this.median = value;
		}

		/// <summary>
		/// Smallest value
		/// </summary>
		[DefaultValueNull]
		public double? Min
		{
			get => this.min;
			set => this.min = value;
		}

		/// <summary>
		/// Largest value
		/// </summary>
		[DefaultValueNull]
		public double? Max
		{
			get => this.max;
			set => this.max = value;
		}

		/// <summary>
		/// Unit of values
		/// </summary>
		[DefaultValueStringEmpty]
		public string Unit
		{
			get => this.unit;
			set => this.unit = value;
		}
	}
}
