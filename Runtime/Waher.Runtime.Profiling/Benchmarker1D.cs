using System.Collections.Generic;
using System.Diagnostics;

namespace Waher.Runtime.Profiling
{
	/// <summary>
	/// Class that keeps track of benchmarking results by name and timing.
	/// </summary>
	public class Benchmarker1D : IBenchmarker
	{
		private readonly SortedDictionary<string, bool> tests = new SortedDictionary<string, bool>();
		private readonly Dictionary<string, long> ticks = new Dictionary<string, long>();
		private readonly object syncObject = new object();
		private readonly Stopwatch watch;

		/// <summary>
		/// Class that keeps track of benchmarking results and timing.
		/// </summary>
		public Benchmarker1D()
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
		/// Benchmarking results, in ticks. Test names of the first index, complexities in 
		/// the second.
		/// </summary>
		public double?[] Ticks => this.GetScaledTicks(1);

		/// <summary>
		/// Benchmarking results, in minutes, Test names of the first index, 
		/// complexities in the second.
		/// </summary>
		public double?[] Minutes => this.GetScaledTicks(1.0 / (Stopwatch.Frequency * 60.0));

		/// <summary>
		/// Benchmarking results, in seconds, Test names of the first index, 
		/// complexities in the second.
		/// </summary>
		public double?[] Seconds => this.GetScaledTicks(1.0 / Stopwatch.Frequency);

		/// <summary>
		/// Benchmarking results, in milliseconds, Test names of the first index, 
		/// complexities in the second.
		/// </summary>
		public double?[] Milliseconds => this.GetScaledTicks(1000.0 / Stopwatch.Frequency);

		/// <summary>
		/// Benchmarking results, in microseconds, Test names of the first index, 
		/// complexities in the second.
		/// </summary>
		public double?[] Microseconds => this.GetScaledTicks(1000000.0 / Stopwatch.Frequency);

		private double?[] GetScaledTicks(double Scale)
		{
			lock (this.syncObject)
			{
				double?[] Result = new double?[this.tests.Count];
				int i;

				i = 0;
				foreach (string Name in this.tests.Keys)
				{
					if (this.ticks.TryGetValue(Name, out long Ticks))
						Result[i] = Ticks * Scale;
					else
						Result[i] = null;

					i++;
				}

				return Result;
			}
		}

		/// <summary>
		/// Starts a benchmark.
		/// </summary>
		/// <param name="Name">Name of benchmark.</param>
		/// <returns>Benchmarking object. Dispose this object when benchmarking is
		/// completed.</returns>
		public Benchmarking Start(string Name)
		{
			lock (this.syncObject)
			{
				if (!this.tests.ContainsKey(Name))
					this.tests[Name] = true;
			}

			return new Benchmarking(this, Name, 0, 0, this.watch.ElapsedTicks);
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
				this.ticks[Name] = ElapsedTicks;
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
