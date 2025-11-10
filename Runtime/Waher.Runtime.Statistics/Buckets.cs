using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Persistence;
using Waher.Script.Objects;

namespace Waher.Runtime.Statistics
{
	/// <summary>
	/// A collection of buckets
	/// </summary>
	public class Buckets
	{
		private readonly Dictionary<string, Bucket> buckets = new Dictionary<string, Bucket>();
		private readonly Duration bucketTime;
		private readonly bool persistBuckets;
		private readonly bool persistSamples;
		private readonly bool retainSamplesInPeriod;
		private DateTime start;

		/// <summary>
		/// A collection of buckets
		/// </summary>
		/// <param name="StartTime">Starting time</param>
		/// <param name="BucketTime">Duration of one bucket, where statistics is collected.</param>
		/// <param name="PersistBuckets">If buckets are to be persisted.</param>
		/// <param name="PersistSamples">If generated samples are to be persisted.</param>
		public Buckets(DateTime StartTime, Duration BucketTime, bool PersistBuckets,
			bool PersistSamples)
			: this(StartTime, BucketTime, PersistBuckets, PersistSamples, false)
		{
		}

		/// <summary>
		/// A collection of buckets
		/// </summary>
		/// <param name="StartTime">Starting time</param>
		/// <param name="BucketTime">Duration of one bucket, where statistics is collected.</param>
		/// <param name="PersistBuckets">If buckets are to be persisted.</param>
		/// <param name="PersistSamples">If generated samples are to be persisted.</param>
		/// <param name="RetainSamplesInPeriod">If standard deviation, variance or median 
		/// values are to be calculated, samples need to be retained in the period.</param>
		public Buckets(DateTime StartTime, Duration BucketTime, bool PersistBuckets,
			bool PersistSamples, bool RetainSamplesInPeriod)
		{
			this.start = StartTime;
			this.bucketTime = BucketTime;
			this.persistBuckets = PersistBuckets;
			this.persistSamples = PersistSamples;
			this.retainSamplesInPeriod = RetainSamplesInPeriod;
		}

		private async Task<Bucket> GetBucket(string Id)
		{
			Bucket Bucket;

			lock (this.buckets)
			{
				if (this.buckets.TryGetValue(Id, out Bucket))
					return Bucket;

				Bucket = new Bucket(Id, this.retainSamplesInPeriod, this.start,
					this.bucketTime, this.persistSamples);

				this.buckets[Id] = Bucket;
			}

			if (this.persistBuckets)
				await Database.Insert(Bucket);

			return Bucket;
		}

		private async Task<Bucket> GetBucket(byte[] Id)
		{
			string TextId = Convert.ToBase64String(Id);
			Bucket Bucket;

			lock (this.buckets)
			{
				if (this.buckets.TryGetValue(TextId, out Bucket))
					return Bucket;

				Bucket = new Bucket(Id, false, this.retainSamplesInPeriod, this.start,
					this.bucketTime, this.persistSamples);

				this.buckets[TextId] = Bucket;
			}

			if (this.persistBuckets)
				await Database.Insert(Bucket);

			return Bucket;
		}

		/// <summary>
		/// Counts an event.
		/// </summary>
		/// <param name="Counter">Counter ID</param>
		/// <returns>Sample statistic of last period, if period changed.</returns>
		public async Task<SampleStatistic> CountEvent(string Counter)
		{
			Bucket Bucket = await this.GetBucket(Counter);

			SampleStatistic Statistic = await Bucket.CountOccurrence(DateTime.UtcNow);
			if (this.persistSamples && !(Statistic is null))
				this.start = Statistic.Stop;

			return Statistic;
		}

		/// <summary>
		/// Counts an event.
		/// </summary>
		/// <param name="Counter">Counter ID</param>
		/// <returns>Sample statistic of last period, if period changed.</returns>
		public async Task<SampleStatistic> CountEvent(byte[] Counter)
		{
			Bucket Bucket = await this.GetBucket(Counter);

			SampleStatistic Statistic = await Bucket.CountOccurrence(DateTime.UtcNow);
			if (this.persistSamples && !(Statistic is null))
				this.start = Statistic.Stop;

			return Statistic;
		}

		/// <summary>
		/// Increments a counter.
		/// </summary>
		/// <param name="Counter">Counter ID</param>
		/// <returns>Sample statistic of last period, if period changed.</returns>
		public async Task<SampleStatistic> IncrementCounter(string Counter)
		{
			Bucket Bucket = await this.GetBucket(Counter);

			SampleStatistic Statistic = await Bucket.Inc();
			if (this.persistSamples && !(Statistic is null))
				this.start = Statistic.Stop;

			return Statistic;
		}

		/// <summary>
		/// Increments a counter.
		/// </summary>
		/// <param name="Counter">Counter ID</param>
		/// <returns>Sample statistic of last period, if period changed.</returns>
		public async Task<SampleStatistic> IncrementCounter(byte[] Counter)
		{
			Bucket Bucket = await this.GetBucket(Counter);

			SampleStatistic Statistic = await Bucket.Inc();
			if (this.persistSamples && !(Statistic is null))
				this.start = Statistic.Stop;

			return Statistic;
		}

		/// <summary>
		/// Decrements a counter.
		/// </summary>
		/// <param name="Counter">Counter ID</param>
		/// <returns>Sample statistic of last period, if period changed.</returns>
		public async Task<SampleStatistic> DecrementCounter(string Counter)
		{
			Bucket Bucket = await this.GetBucket(Counter);

			SampleStatistic Statistic = await Bucket.Dec();
			if (this.persistSamples && !(Statistic is null))
				this.start = Statistic.Stop;

			return Statistic;
		}

		/// <summary>
		/// Decrements a counter.
		/// </summary>
		/// <param name="Counter">Counter ID</param>
		/// <returns>Sample statistic of last period, if period changed.</returns>
		public async Task<SampleStatistic> DecrementCounter(byte[] Counter)
		{
			Bucket Bucket = await this.GetBucket(Counter);

			SampleStatistic Statistic = await Bucket.Dec();
			if (this.persistSamples && !(Statistic is null))
				this.start = Statistic.Stop;

			return Statistic;
		}

		/// <summary>
		/// Samples a value
		/// </summary>
		/// <param name="Counter">Counter ID</param>
		/// <param name="Value">Value</param>
		/// <returns>Sample statistic of last period, if period changed.</returns>
		public async Task<SampleStatistic> Sample(string Counter, double Value)
		{
			Bucket Bucket = await this.GetBucket(Counter);

			SampleStatistic Statistic = await Bucket.Sample(DateTime.UtcNow, Value);
			if (this.persistSamples && !(Statistic is null))
				this.start = Statistic.Stop;

			return Statistic;
		}

		/// <summary>
		/// Samples a value
		/// </summary>
		/// <param name="Counter">Counter ID</param>
		/// <param name="Value">Value</param>
		/// <returns>Sample statistic of last period, if period changed.</returns>
		public async Task<SampleStatistic> Sample(byte[] Counter, double Value)
		{
			Bucket Bucket = await this.GetBucket(Counter);

			SampleStatistic Statistic = await Bucket.Sample(DateTime.UtcNow, Value);
			if (this.persistSamples && !(Statistic is null))
				this.start = Statistic.Stop;

			return Statistic;
		}

		/// <summary>
		/// Samples a value
		/// </summary>
		/// <param name="Counter">Counter ID</param>
		/// <param name="Value">Value</param>
		public async Task<SampleStatistic> Sample(string Counter, PhysicalQuantity Value)
		{
			Bucket Bucket = await this.GetBucket(Counter);

			SampleStatistic Statistic = await Bucket.Sample(DateTime.UtcNow, Value);
			if (this.persistSamples && !(Statistic is null))
				this.start = Statistic.Stop;

			return Statistic;
		}

		/// <summary>
		/// Samples a value
		/// </summary>
		/// <param name="Counter">Counter ID</param>
		/// <param name="Value">Value</param>
		public async Task<SampleStatistic> Sample(byte[] Counter, PhysicalQuantity Value)
		{
			Bucket Bucket = await this.GetBucket(Counter);

			SampleStatistic Statistic = await Bucket.Sample(DateTime.UtcNow, Value);
			if (this.persistSamples && !(Statistic is null))
				this.start = Statistic.Stop;

			return Statistic;
		}

		/// <summary>
		/// Number of counters
		/// </summary>
		public int Count
		{
			get
			{
				lock (this.buckets)
				{
					return this.buckets.Count;
				}
			}
		}

		/// <summary>
		/// Tries to get a bucket, given its ID.
		/// </summary>
		/// <param name="Id">Bucket ID</param>
		/// <param name="Bucket">Bucket, if found.</param>
		/// <returns>If a bucket was found.</returns>
		public bool TryGetBucket(string Id, out Bucket Bucket)
		{
			lock (this.buckets)
			{
				return this.buckets.TryGetValue(Id, out Bucket);
			}
		}

		/// <summary>
		/// Tries to get a bucket, given its ID.
		/// </summary>
		/// <param name="Id">Bucket ID</param>
		/// <param name="Bucket">Bucket, if found.</param>
		/// <returns>If a bucket was found.</returns>
		public bool TryGetBucket(byte[] Id, out Bucket Bucket)
		{
			lock (this.buckets)
			{
				return this.buckets.TryGetValue(Convert.ToBase64String(Id), out Bucket);
			}
		}

		/// <summary>
		/// Sample IDs
		/// </summary>
		public string[] IDs
		{
			get
			{
				lock (this.buckets)
				{
					string[] Result = new string[this.buckets.Count];
					this.buckets.Keys.CopyTo(Result, 0);
					return Result;
				}
			}
		}

	}
}
