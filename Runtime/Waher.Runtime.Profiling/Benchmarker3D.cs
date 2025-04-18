﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Mail;
using System.Text;

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
		/// Registered complexities (N).
		/// </summary>
		public long[] ComplexitiesN
		{
			get
			{
				lock (this.syncObject)
				{
					return this.GetComplexitiesNLocked();
				}
			}
		}

		private long[] GetComplexitiesNLocked()
		{
			long[] Result = new long[this.complexitiesN.Count];
			this.complexitiesN.Keys.CopyTo(Result, 0);
			return Result;
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
					return this.GetComplexitiesMLocked();
				}
			}
		}

		private long[] GetComplexitiesMLocked()
		{
			long[] Result = new long[this.complexitiesM.Count];
			this.complexitiesM.Keys.CopyTo(Result, 0);
			return Result;
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
				return this.GetScaledTicksLocked(Scale);
			}
		}

		private double?[,,] GetScaledTicksLocked(double Scale)
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
			long[] ComplexitiesM;
			double?[,,] Values;
			double? d;
			int i, j, k, c, N, M;

			lock (this.syncObject)
			{
				Names = this.GetTestsLocked();
				ComplexitiesN = this.GetComplexitiesNLocked();
				ComplexitiesM = this.GetComplexitiesMLocked();
				Values = this.GetScaledTicksLocked(Scale);
			}

			c = Names.Length;
			N = ComplexitiesN.Length;
			M = ComplexitiesM.Length;

			sb.Append('{');
			for (i = 0; i < c; i++)
			{
				if (i > 0)
					sb.Append(',');

				sb.Append("\r\n\t\"");
				sb.Append(Names[i].Replace("\"", "\\\""));
				sb.Append("\":\r\n\t\t[[\"N\\\\M\"");

				foreach (long Complexity in ComplexitiesM)
				{
					sb.Append(',');
					sb.Append(Complexity.ToString());
				}

				sb.Append(']');

				for (j = 0; j < N; j++)
				{
					sb.Append(",\r\n\t\t [");
					sb.Append(ComplexitiesN[j].ToString());

					for (k = 0; k < M; k++)
					{
						sb.Append(',');

						d = Values[i, j, k];
						if (d.HasValue)
							sb.Append(d.Value.ToString().Replace(System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, "."));
						else
							sb.Append("null");
					}

					sb.Append(']');
				}

				sb.Append(']');
			}

			if (c > 0)
				sb.AppendLine();

			sb.Append('}');

			return sb.ToString();
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
			GC.GetTotalMemory(true);

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
		public bool Remove(long? N)
		{
			return this.Remove(N, null);
		}

		/// <summary>
		/// Removes a complexity.
		/// </summary>
		/// <param name="N">Complexity N</param>
		/// <param name="M">Complexity M</param>
		/// <returns>If the benchmark was found, and removed.</returns>
		public bool Remove(long? N, long? M)
		{
			bool Removed = false;

			lock (this.syncObject)
			{
				foreach (Dictionary<long, Dictionary<long, long>> ComplexitiesN in this.ticks.Values)
				{
					if (N.HasValue)
						ComplexitiesN.Remove(N.Value);

					if (M.HasValue)
					{
						foreach (Dictionary<long, long> ComplexitiesM in ComplexitiesN.Values)
							ComplexitiesM.Remove(M.Value);
					}
				}

				if (N.HasValue && this.complexitiesN.Remove(N.Value))
					Removed = true;

				if (M.HasValue && this.complexitiesM.Remove(M.Value))
					Removed = true;
			}

			return Removed;
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
