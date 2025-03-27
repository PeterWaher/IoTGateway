using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Waher.Runtime.Profiling
{
	/// <summary>
	/// Class that keeps track of benchmarking results by name, complexity and timing.
	/// </summary>
	public class Benchmarker2D : IBenchmarker
	{
		private readonly SortedDictionary<string, bool> tests = new SortedDictionary<string, bool>();
		private readonly SortedDictionary<long, bool> complexities = new SortedDictionary<long, bool>();
		private readonly Dictionary<string, Dictionary<long, long>> ticks = new Dictionary<string, Dictionary<long, long>>();
		private readonly object syncObject = new object();
		private readonly Stopwatch watch;

		/// <summary>
		/// Class that keeps track of benchmarking results by name, complexity and timing.
		/// </summary>
		public Benchmarker2D()
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
					return this.GetTestsLocked();
				}
			}
		}

		private string[] GetTestsLocked()
		{
			string[] Result = new string[this.tests.Count];
			this.tests.Keys.CopyTo(Result, 0);
			return Result;
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
					return this.GetComplexitiesLocked();
				}
			}
		}

		private long[] GetComplexitiesLocked()
		{
			long[] Result = new long[this.complexities.Count];
			this.complexities.Keys.CopyTo(Result, 0);
			return Result;
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
				return this.GetScaledTicksLocked(Scale);
			}
		}

		private double?[,] GetScaledTicksLocked(double Scale)
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

		/// <summary>
		/// Gets benchmarking result in ticks, as script.
		/// </summary>
		/// <returns>Script</returns>
		public string GetResultScriptTicks() => this.GetResultScript(1);

		/// <summary>
		/// Gets benchmarking result in seconds, as script.
		/// </summary>
		/// <returns>Script</returns>
		public string GetResultScriptSeconds() => this.GetResultScript(1.0 / Stopwatch.Frequency);

		/// <summary>
		/// Gets benchmarking result in milliseconds, as script.
		/// </summary>
		/// <returns>Script</returns>
		public string GetResultScriptMilliseconds() => this.GetResultScript(1000.0 / Stopwatch.Frequency);

		/// <summary>
		/// Gets benchmarking result in microseconds, as script.
		/// </summary>
		/// <returns>Script</returns>
		public string GetResultScriptMicroseconds() => this.GetResultScript(1000000.0 / Stopwatch.Frequency);

		private string GetResultScript(double Scale)
		{
			StringBuilder sb = new StringBuilder();
			string[] Names;
			long[] ComplexitiesN;
			double?[,] Values;
			double? d;
			int i, j, c, N;

			lock (this.syncObject)
			{
				Names = this.GetTestsLocked();
				ComplexitiesN = this.GetComplexitiesLocked();
				Values = this.GetScaledTicksLocked(Scale);
			}

			c = Names.Length;
			N = ComplexitiesN.Length;

			sb.Append("[[\"N\\\\Test\"");
			foreach (string Name in Names)
			{
				sb.Append(",\"");
				sb.Append(Name.Replace("\"", "\\\""));
				sb.Append('"');
			}
			sb.Append(']');

			for (j = 0; j < N; j++)
			{
				sb.Append(",\r\n [");
				sb.Append(ComplexitiesN[j].ToString());

				for (i = 0; i < c; i++)
				{
					sb.Append(',');

					d = Values[i, j];
					if (d.HasValue)
						sb.Append(d.Value.ToString().Replace(System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, "."));
					else
						sb.Append("null");
				}

				sb.Append(']');
			}

			sb.Append(']');

			return sb.ToString();
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
			GC.GetTotalMemory(true);

			lock (this.syncObject)
			{
				if (!this.tests.ContainsKey(Name))
					this.tests[Name] = true;

				if (!this.complexities.ContainsKey(Complexity))
					this.complexities[Complexity] = true;
			}

			return new Benchmarking(this, Name, Complexity, 0, this.watch.ElapsedTicks);
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
				if (!this.ticks.TryGetValue(Name, out Dictionary<long, long> Complexities))
				{
					Complexities = new Dictionary<long, long>();
					this.ticks[Name] = Complexities;
				}

				Complexities[N] = ElapsedTicks;
			}
		}

		/// <summary>
		/// Removes a benchmark.
		/// </summary>
		/// <param name="Name">Name of benchmark.</param>
		/// <returns>If the benchmark was found, and removed.</returns>
		public bool Remove(string Name)
		{
			lock (this.syncObject)
			{
				this.ticks.Remove(Name);
				return this.tests.Remove(Name);
			}
		}

		/// <summary>
		/// Removes a complexity.
		/// </summary>
		/// <param name="N">Complexity N</param>
		/// <returns>If the benchmark was found, and removed.</returns>
		public bool Remove(long N)
		{
			lock (this.syncObject)
			{
				foreach (Dictionary<long, long> Complexities in this.ticks.Values)
					Complexities.Remove(N);

				return this.complexities.Remove(N);
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
