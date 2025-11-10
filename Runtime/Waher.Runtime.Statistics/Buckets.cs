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

		/// <summary>
		/// Counts an event.
		/// </summary>
		/// <param name="Counter">Counter ID</param>
		public async Task CountEvent(string Counter)
		{
			Bucket Bucket;

			lock (this.buckets)
			{
				if (!this.buckets.TryGetValue(Counter, out Bucket))
				{
					Bucket = new Bucket(Counter, this.retainSamplesInPeriod, this.start, 
						this.bucketTime, this.persistSamples);

					this.buckets[Counter] = Bucket;
				}
			}

			this.start = await Bucket.CountOccurrence(DateTime.UtcNow);

			if (this.persistBuckets && string.IsNullOrEmpty(Bucket.ObjectId))
				await Database.Insert(Bucket);
		}

		/// <summary>
		/// Increments a counter.
		/// </summary>
		/// <param name="Counter">Counter ID</param>
		/// <returns>Sample statistic of last period, if period changed.</returns>
		public async Task<SampleStatistic> IncrementCounter(string Counter)
		{
			Bucket Bucket;

			lock (this.buckets)
			{
				if (!this.buckets.TryGetValue(Counter, out Bucket))
				{
					Bucket = new Bucket(Counter, false, this.start, this.bucketTime,
						this.persistSamples);

					this.buckets[Counter] = Bucket;
				}
			}

			SampleStatistic Statistic = await Bucket.Inc();
			if (this.persistSamples && !(Statistic is null))
				this.start = Statistic.Stop;

			if (this.persistBuckets && string.IsNullOrEmpty(Bucket.ObjectId))
				await Database.Insert(Bucket);

			return Statistic;
		}

		/// <summary>
		/// Decrements a counter.
		/// </summary>
		/// <param name="Counter">Counter ID</param>
		/// <returns>Sample statistic of last period, if period changed.</returns>
		public async Task<SampleStatistic> DecrementCounter(string Counter)
		{
			Bucket Bucket;

			lock (this.buckets)
			{
				if (!this.buckets.TryGetValue(Counter, out Bucket))
				{
					Bucket = new Bucket(Counter, false, this.start, this.bucketTime, 
						this.persistSamples);

					this.buckets[Counter] = Bucket;
				}
			}

			SampleStatistic Statistic = await Bucket.Dec();
			if (this.persistSamples && !(Statistic is null))
				this.start = Statistic.Stop;

			if (this.persistBuckets && string.IsNullOrEmpty(Bucket.ObjectId))
				await Database.Insert(Bucket);
		
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
			Bucket Bucket;

			lock (this.buckets)
			{
				if (!this.buckets.TryGetValue(Counter, out Bucket))
				{
					Bucket = new Bucket(Counter, true, this.start, this.bucketTime, 
						this.persistSamples);

					this.buckets[Counter] = Bucket;
				}
			}

			SampleStatistic Statistic = await Bucket.Sample(DateTime.UtcNow, Value);
			if (this.persistSamples && !(Statistic is null))
				this.start = Statistic.Stop;

			if (this.persistBuckets && string.IsNullOrEmpty(Bucket.ObjectId))
				await Database.Insert(Bucket);
		
			return Statistic;
		}

		/// <summary>
		/// Samples a value
		/// </summary>
		/// <param name="Counter">Counter ID</param>
		/// <param name="Value">Value</param>
		public async Task<SampleStatistic> Sample(string Counter, PhysicalQuantity Value)
		{
			Bucket Bucket;

			lock (this.buckets)
			{
				if (!this.buckets.TryGetValue(Counter, out Bucket))
				{
					Bucket = new Bucket(Counter, true, this.start, this.bucketTime, 
						this.persistSamples);

					this.buckets[Counter] = Bucket;
				}
			}

			SampleStatistic Statistic = await Bucket.Sample(DateTime.UtcNow, Value);
			if (this.persistSamples && !(Statistic is null))
				this.start = Statistic.Stop;

			if (this.persistBuckets && string.IsNullOrEmpty(Bucket.ObjectId))
				await Database.Insert(Bucket);
		
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
		/// Gets a sample bucket.
		/// </summary>
		/// <param name="Id">Bucket ID</param>
		/// <returns>Bucket.</returns>
		public async Task<Bucket> GetSampleBucket(string Id)
		{
			Bucket Bucket;

			lock (this.buckets)
			{
				if (!this.buckets.TryGetValue(Id, out Bucket))
				{
					Bucket = new Bucket(Id, true, this.start, this.bucketTime, 
						this.persistSamples);

					this.buckets[Id] = Bucket;
				}
			}

			if (this.persistBuckets && string.IsNullOrEmpty(Bucket.ObjectId))
				await Database.Insert(Bucket);

			return Bucket;
		}

		/// <summary>
		/// Gets a count bucket.
		/// </summary>
		/// <param name="Id">Bucket ID</param>
		/// <returns>Bucket.</returns>
		public async Task<Bucket> GetCountBucket(string Id)
		{
			Bucket Bucket;

			lock (this.buckets)
			{
				if (!this.buckets.TryGetValue(Id, out Bucket))
				{
					Bucket = new Bucket(Id, false, this.start, this.bucketTime, 
						this.persistSamples);

					this.buckets[Id] = Bucket;
				}
			}

			if (this.persistBuckets && string.IsNullOrEmpty(Bucket.ObjectId))
				await Database.Insert(Bucket);

			return Bucket;
		}

		/// <summary>
		/// Registers a custom bucket.
		/// </summary>
		/// <param name="Bucket">Bucket</param>
		public void Register(Bucket Bucket)
		{
			lock (this.buckets)
			{
				if (this.buckets.ContainsKey(Bucket.Id))
					throw new Exception("A bucket with ID " + Bucket.Id + " already registered.");

				this.buckets[Bucket.Id] = Bucket;
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
