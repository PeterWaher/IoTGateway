﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Waher.Runtime.Profiling
{
	/// <summary>
	/// Class that keeps track of benchmarking results and timing.
	/// </summary>
	public class Benchmarker : IDisposable
	{
		private readonly SortedDictionary<string, bool> tests = new SortedDictionary<string, bool>();
		private readonly SortedDictionary<long, bool> complexities = new SortedDictionary<long, bool>();
		private readonly Dictionary<string, Dictionary<long, long>> ticks = new Dictionary<string, Dictionary<long, long>>();
		private readonly object syncObject = new object();
		private readonly Stopwatch watch;

		/// <summary>
		/// Class that keeps track of benchmarking results and timing.
		/// </summary>
		public Benchmarker()
		{
			this.watch = new Stopwatch();
			this.watch.Start();
		}

		/// <summary>
		/// Registered tests.
		/// </summary>
		public string[] Tests
		{
			get
			{
				lock (this.syncObject)
				{
					string[] Result = new string[this.tests.Count];
					this.tests.Keys.CopyTo(Result, 0);
					return Result;
				}
			}
		}

		/// <summary>
		/// Registered complexities.
		/// </summary>
		public long[] Complexities
		{
			get
			{
				lock (this.syncObject)
				{
					long[] Result = new long[this.complexities.Count];
					this.complexities.Keys.CopyTo(Result, 0);
					return Result;
				}
			}
		}

		/// <summary>
		/// Benchmarking results, in ticks. Test names of the first index, complexities in 
		/// the second.
		/// </summary>
		public double?[,] Ticks => this.GetScaledTicks(1);

		/// <summary>
		/// Benchmarking results, in minutes, Test names of the first index, 
		/// complexities in the second.
		/// </summary>
		public double?[,] Minutes => this.GetScaledTicks(1.0 / (Stopwatch.Frequency * 60.0));

		/// <summary>
		/// Benchmarking results, in seconds, Test names of the first index, 
		/// complexities in the second.
		/// </summary>
		public double?[,] Seconds => this.GetScaledTicks(1.0 / Stopwatch.Frequency);

		/// <summary>
		/// Benchmarking results, in milliseconds, Test names of the first index, 
		/// complexities in the second.
		/// </summary>
		public double?[,] Milliseconds => this.GetScaledTicks(1000.0 / Stopwatch.Frequency);

		/// <summary>
		/// Benchmarking results, in microseconds, Test names of the first index, 
		/// complexities in the second.
		/// </summary>
		public double?[,] Microseconds => this.GetScaledTicks(1000000.0 / Stopwatch.Frequency);

		private double?[,] GetScaledTicks(double Scale)
		{
			lock (this.syncObject)
			{
				double?[,] Result = new double?[this.tests.Count, this.complexities.Count];
				int i, j;

				i = 0;
				foreach (string Name in this.tests.Keys)
				{
					j = 0;
					foreach (long Complexity in this.complexities.Keys)
					{
						if (this.ticks.TryGetValue(Name, out Dictionary<long, long> Complexities) &&
							Complexities.TryGetValue(Complexity, out long Ticks))
						{
							Result[i, j] = Ticks * Scale;
						}
						else
							Result[i, j] = null;

						j++;
					}

					i++;
				}

				return Result;
			}
		}

		/// <summary>
		/// Starts a benchmark.
		/// </summary>
		/// <param name="Name">Name of benchmark.</param>
		/// <param name="Complexity">Complexity of benchmark (N).</param>
		/// <returns>Benchmarking object. Dispose this object when benchmarking is
		/// completed.</returns>
		public Benchmarking Start(string Name, long Complexity)
		{
			lock (this.syncObject)
			{
				if (!this.tests.ContainsKey(Name))
					this.tests[Name] = true;

				if (!this.complexities.ContainsKey(Complexity))
					this.complexities[Complexity] = true;
			}

			return new Benchmarking(this, Name, Complexity, this.watch.ElapsedTicks);
		}

		/// <summary>
		/// Stops a benchmark.
		/// </summary>
		/// <param name="Name">Name of benchmark.</param>
		/// <param name="Complexity">Complexity of benchmark (N).</param>
		/// <param name="StartTicks">Elapsed ticks at the start of benchmark.</param>
		internal void Stop(string Name, long Complexity, long StartTicks)
		{
			long ElapsedTicks = this.watch.ElapsedTicks - StartTicks;

			lock (this.syncObject)
			{
				if (!this.ticks.TryGetValue(Name, out Dictionary<long, long> Complexities))
				{
					Complexities = new Dictionary<long, long>();
					this.ticks[Name] = Complexities;
				}

				Complexities[Complexity] = ElapsedTicks;
			}
		}

		/// <summary>
		/// Disposes of the object.
		/// </summary>
		public void Dispose()
		{
			this.watch.Stop();
		}
	}
}
