using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Waher.Runtime.Profiling
{
	/// <summary>
	/// Class that keeps track of benchmarking results by name and timing.
	/// </summary>
	public class Benchmarker1D : IBenchmarker
	{
		private readonly SortedDictionary<string, long> tests = new SortedDictionary<string, long>();
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
				return this.GetScaledTicksLocked(Scale);
			}
		}

		private double?[] GetScaledTicksLocked(double Scale)
		{
			double?[] Result = new double?[this.tests.Count];
			int i;

			i = 0;
			foreach (string Name in this.tests.Keys)
			{
				if (this.tests.TryGetValue(Name, out long Ticks))
					Result[i] = Ticks * Scale;
				else
					Result[i] = null;

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
			double?[] Values;
			double? d;
			int i, c;

			lock (this.syncObject)
			{
				Names = this.GetTestsLocked();
				Values = this.GetScaledTicksLocked(Scale);
			}

			c = Names.Length;

			sb.Append('[');

			for (i = 0; i < c; i++)
			{
				if (i > 0)
					sb.Append(",\r\n ");
				sb.Append("[\"");
				sb.Append(Names[i].Replace("\"", "\\\""));
				sb.Append("\",");

				d = Values[i];
				if (d.HasValue)
					sb.Append(d.Value.ToString().Replace(System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, "."));
				else
					sb.Append("null");

				sb.Append(']');
			}

			sb.Append(']');

			return sb.ToString();
		}

		/// <summary>
		/// Starts a benchmark.
		/// </summary>
		/// <param name="Name">Name of benchmark.</param>
		/// <returns>Benchmarking object. Dispose this object when benchmarking is
		/// completed.</returns>
		public Benchmarking Start(string Name)
		{
			GC.GetTotalMemory(true);

			lock (this.syncObject)
			{
				if (!this.tests.ContainsKey(Name))
					this.tests[Name] = 0;
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
				this.tests[Name] = ElapsedTicks;
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
				return this.tests.Remove(Name);
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
