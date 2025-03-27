using System.Collections.Generic;
using System.Diagnostics;

namespace Waher.Runtime.Profiling
{
	/// <summary>
	/// Class that keeps track of benchmarking results by name, two complexities N and M,
	/// and timing.
	/// </summary>
	public class Benchmarker3D : IBenchmarker
	{
		private readonly SortedDictionary<string, bool> tests = new SortedDictionary<string, bool>();
		private readonly SortedDictionary<long, bool> complexitiesN = new SortedDictionary<long, bool>();
		private readonly SortedDictionary<long, bool> complexitiesM = new SortedDictionary<long, bool>();
		private readonly Dictionary<string, Dictionary<long, Dictionary<long, long>>> ticks = new Dictionary<string, Dictionary<long, Dictionary<long, long>>>();
		private readonly object syncObject = new object();
		private readonly Stopwatch watch;

		/// <summary>
		/// Class that keeps track of benchmarking results by name, two complexities N and M,
		/// and timing.
		/// </summary>
		public Benchmarker3D()
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
		/// Registered complexities (N).
		/// </summary>
		public long[] ComplexitiesN
		{
			get
			{
				lock (this.syncObject)
				{
					long[] Result = new long[this.complexitiesN.Count];
					this.complexitiesN.Keys.CopyTo(Result, 0);
					return Result;
				}
			}
		}

		/// <summary>
		/// Registered complexities (M).
		/// </summary>
		public long[] ComplexitiesM
		{
			get
			{
				lock (this.syncObject)
				{
					long[] Result = new long[this.complexitiesM.Count];
					this.complexitiesM.Keys.CopyTo(Result, 0);
					return Result;
				}
			}
		}

		/// <summary>
		/// Benchmarking results, in ticks. Test names of the first index, complexities N in 
		/// the second and complexities M in the third.
		/// </summary>
		public double?[,,] Ticks => this.GetScaledTicks(1);

		/// <summary>
		/// Benchmarking results, in minutes, Test names of the first index, 
		/// complexities N in the second and complexities M in the third.
		/// </summary>
		public double?[,,] Minutes => this.GetScaledTicks(1.0 / (Stopwatch.Frequency * 60.0));

		/// <summary>
		/// Benchmarking results, in seconds, Test names of the first index, 
		/// complexities N in the second and complexities M in the third.
		/// </summary>
		public double?[,,] Seconds => this.GetScaledTicks(1.0 / Stopwatch.Frequency);

		/// <summary>
		/// Benchmarking results, in milliseconds, Test names of the first index, 
		/// complexities N in the second and complexities M in the third.
		/// </summary>
		public double?[,,] Milliseconds => this.GetScaledTicks(1000.0 / Stopwatch.Frequency);

		/// <summary>
		/// Benchmarking results, in microseconds, Test names of the first index, 
		/// complexities N in the second and complexities M in the third.
		/// </summary>
		public double?[,,] Microseconds => this.GetScaledTicks(1000000.0 / Stopwatch.Frequency);

		private double?[,,] GetScaledTicks(double Scale)
		{
			lock (this.syncObject)
			{
				double?[,,] Result = new double?[this.tests.Count, this.complexitiesN.Count,
					this.complexitiesM.Count];
				int i, j, k;

				i = 0;
				foreach (string Name in this.tests.Keys)
				{
					j = 0;
					foreach (long ComplexityN in this.complexitiesN.Keys)
					{
						k = 0;
						foreach (long ComplexityM in this.complexitiesM.Keys)
						{
							if (this.ticks.TryGetValue(Name, out Dictionary<long, Dictionary<long, long>> ComplexitiesN) &&
								ComplexitiesN.TryGetValue(ComplexityN, out Dictionary<long, long> ComplexitiesM) &&
								ComplexitiesM.TryGetValue(ComplexityM, out long Ticks))
							{
								Result[i, j, k] = Ticks * Scale;
							}
							else
								Result[i, j, k] = null;

							k++;
						}

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
		/// <param name="N">Complexity of benchmark (N).</param>
		/// <param name="M">Complexity of benchmark (M).</param>
		/// <returns>Benchmarking object. Dispose this object when benchmarking is
		/// completed.</returns>
		public Benchmarking Start(string Name, long N, long M)
		{
			lock (this.syncObject)
			{
				if (!this.tests.ContainsKey(Name))
					this.tests[Name] = true;

				if (!this.complexitiesN.ContainsKey(N))
					this.complexitiesN[N] = true;

				if (!this.complexitiesM.ContainsKey(M))
					this.complexitiesM[M] = true;
			}

			return new Benchmarking(this, Name, N, M, this.watch.ElapsedTicks);
		}

		/// <summary>
		/// Stops a benchmark.
		/// </summary>
		/// <param name="Name">Name of benchmark.</param>
		/// <param name="N">Complexity of benchmark (N).</param>
		/// <param name="M">Complexity of benchmark (M).</param>
		/// <param name="StartTicks">Elapsed ticks at the start of benchmark.</param>
		public void Stop(string Name, long N, long M, long StartTicks)
		{
			long ElapsedTicks = this.watch.ElapsedTicks - StartTicks;

			lock (this.syncObject)
			{
				if (!this.ticks.TryGetValue(Name, out Dictionary<long, Dictionary<long, long>> ComplexitiesN))
				{
					ComplexitiesN = new Dictionary<long, Dictionary<long, long>>();
					this.ticks[Name] = ComplexitiesN;
				}

				if (!ComplexitiesN.TryGetValue(N, out Dictionary<long, long> ComplexitiesM))
				{
					ComplexitiesM = new Dictionary<long, long>();
					ComplexitiesN[N] = ComplexitiesM;
				}

				ComplexitiesM[M] = ElapsedTicks;
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
