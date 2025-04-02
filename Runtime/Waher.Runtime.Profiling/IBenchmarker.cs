using System;

namespace Waher.Runtime.Profiling
{
	/// <summary>
	/// Class that keeps track of benchmarking results and timing.
	/// </summary>
	public interface IBenchmarker : IDisposable
	{
		/// <summary>
		/// Registered tests.
		/// </summary>
		string[] Tests { get; }

		/// <summary>
		/// Stops a benchmark.
		/// </summary>
		/// <param name="Name">Name of benchmark.</param>
		/// <param name="N">Complexity of benchmark (N).</param>
		/// <param name="M">Complexity of benchmark (M).</param>
		/// <param name="StartTicks">Elapsed ticks at the start of benchmark.</param>
		void Stop(string Name, long N, long M, long StartTicks);

		/// <summary>
		/// Gets benchmarking result in ticks, as script.
		/// </summary>
		/// <returns>Script</returns>
		string GetResultScriptTicks();

		/// <summary>
		/// Gets benchmarking result in seconds, as script.
		/// </summary>
		/// <returns>Script</returns>
		string GetResultScriptSeconds();

		/// <summary>
		/// Gets benchmarking result in milliseconds, as script.
		/// </summary>
		/// <returns>Script</returns>
		string GetResultScriptMilliseconds();

		/// <summary>
		/// Gets benchmarking result in microseconds, as script.
		/// </summary>
		/// <returns>Script</returns>
		string GetResultScriptMicroseconds();

		/// <summary>
		/// Removes a benchmark.
		/// </summary>
		/// <param name="Name">Name of benchmark.</param>
		/// <returns>If the benchmark was found, and removed.</returns>
		bool Remove(string Name);
	}
}
