using System;

namespace Waher.Runtime.Profiling
{
	/// <summary>
	/// Class that keeps track of benchmarking results and timing.
	/// </summary>
	public interface IBenchmarker : IDisposable
	{
		/// <summary>
		/// Stops a benchmark.
		/// </summary>
		/// <param name="Name">Name of benchmark.</param>
		/// <param name="N">Complexity of benchmark (N).</param>
		/// <param name="M">Complexity of benchmark (M).</param>
		/// <param name="StartTicks">Elapsed ticks at the start of benchmark.</param>
		void Stop(string Name, long N, long M, long StartTicks);
	}
}
