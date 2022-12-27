using System;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Runtime.Cache;
using Waher.Runtime.Counters.CounterObjects;

namespace Waher.Runtime.Counters
{
	/// <summary>
	/// Static class managing persistent counters.
	/// </summary>
	public static class RuntimeCounters
	{
		private static readonly Cache<CaseInsensitiveString, CounterRec> counters;
		private static readonly object synchObject = new object();

		private class CounterRec
		{
			public TaskCompletionSource<bool> Stored = new TaskCompletionSource<bool>();
			public RuntimeCounter Counter;
			public bool Changed;
		}

		static RuntimeCounters()
		{
			counters = new Cache<CaseInsensitiveString, CounterRec>(int.MaxValue, TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(1));

			counters.Removed += Counters_Removed;
		}

		private static async void Counters_Removed(object Sender, CacheItemEventArgs<CaseInsensitiveString, CounterRec> e)
		{
			try
			{
				lock (synchObject)
				{
					if (!e.Value.Changed)
					{
						e.Value.Stored.TrySetResult(true);
						return;
					}
				}

				if (e.Value.Counter.Counter == 0)
				{
					if (!string.IsNullOrEmpty(e.Value.Counter.ObjectId))
						await Database.Delete(e.Value.Counter);
				}
				else
				{
					if (string.IsNullOrEmpty(e.Value.Counter.ObjectId))
						await Database.Insert(e.Value.Counter);
					else
						await Database.Update(e.Value.Counter);
				}

				e.Value.Stored.TrySetResult(true);
			}
			catch (Exception)
			{
				e.Value.Stored.TrySetResult(false);
			}
		}

		/// <summary>
		/// Flushes all cached counters to the database.
		/// </summary>
		public static async Task FlushAsync()
		{
			CounterRec[] Records = counters.GetValues();
			counters.Clear();

			foreach (CounterRec Rec in Records)
				await Rec.Stored.Task;
		}

		/// <summary>
		/// Increments a counter.
		/// </summary>
		/// <param name="Key">Counter key.</param>
		/// <returns>Current count.</returns>
		public static Task<long> IncrementCounter(CaseInsensitiveString Key)
		{
			return IncrementCounter(Key, 1);
		}

		/// <summary>
		/// Increments a counter.
		/// </summary>
		/// <param name="Key">Counter key.</param>
		/// <param name="Delta">Amount to increment.</param>
		/// <returns>Current count.</returns>
		public static async Task<long> IncrementCounter(CaseInsensitiveString Key, long Delta)
		{
			lock (synchObject)
			{
				if (counters.TryGetValue(Key, out CounterRec Rec))
				{
					Rec.Counter.Counter += Delta;
					Rec.Changed = true;

					return Rec.Counter.Counter;
				}
			}

			RuntimeCounter Counter = await Database.FindFirstDeleteRest<RuntimeCounter>(new FilterFieldEqualTo("Key", Key));

			lock (synchObject)
			{
				if (counters.TryGetValue(Key, out CounterRec Rec))
				{
					Rec.Counter.Counter += Delta;
					Rec.Changed = true;

					return Rec.Counter.Counter;
				}
				else
				{
					if (Counter is null)
					{
						Counter = new RuntimeCounter()
						{
							Counter = Delta,
							Key = Key
						};

						counters[Key] = new CounterRec()
						{
							Counter = Counter,
							Changed = true
						};
					}
					else
					{
						Counter.Counter += Delta;
						counters[Key] = new CounterRec()
						{
							Counter = Counter,
							Changed = true
						};

						return Counter.Counter;
					}
				}
			}

			return Counter.Counter;
		}

		/// <summary>
		/// Decrements a counter.
		/// </summary>
		/// <param name="Key">Counter key.</param>
		/// <returns>Current count.</returns>
		public static Task<long> DecrementCounter(CaseInsensitiveString Key)
		{
			return IncrementCounter(Key, -1);
		}

		/// <summary>
		/// Decrements a counter.
		/// </summary>
		/// <param name="Key">Counter key.</param>
		/// <param name="Delta">Amount to decrement.</param>
		/// <returns>Current count.</returns>
		public static Task<long> DecrementCounter(CaseInsensitiveString Key, long Delta)
		{
			return IncrementCounter(Key, -Delta);
		}

		/// <summary>
		/// Gets the current count of a counter.
		/// </summary>
		/// <param name="Key">Counter key.</param>
		/// <returns>Current count.</returns>
		public static async Task<long> GetCount(CaseInsensitiveString Key)
		{
			lock (synchObject)
			{
				if (counters.TryGetValue(Key, out CounterRec Rec))
					return Rec.Counter.Counter;
			}

			RuntimeCounter Counter = await Database.FindFirstDeleteRest<RuntimeCounter>(new FilterFieldEqualTo("Key", Key));

			lock (synchObject)
			{
				if (counters.TryGetValue(Key, out CounterRec Rec))
					return Rec.Counter.Counter;
				else
				{
					if (Counter is null)
					{
						Counter = new RuntimeCounter()
						{
							Counter = 0,
							Key = Key
						};

						counters[Key] = new CounterRec()
						{
							Counter = Counter,
							Changed = false
						};
					}
					else
					{
						counters[Key] = new CounterRec()
						{
							Counter = Counter,
							Changed = false
						};
					}
				}
			}

			return Counter.Counter;
		}
	}
}
