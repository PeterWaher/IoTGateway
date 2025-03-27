using System;

namespace Waher.Runtime.Profiling
{
	/// <summary>
	/// Class that keeps track of benchmarking results and timing.
	/// </summary>
	public class Benchmarking : IDisposable
	{
		private readonly IBenchmarker benchmarker;
		private readonly string name;
		private readonly long m;
		private readonly long n;
		private readonly long startTicks;

		/// <summary>
		/// Class that keeps track of benchmarking results and timing.
		/// </summary>
		/// <param name="Benchmarker">Benchmarker object.</param>
		/// <param name="Name">Name of benchmark.</param>
		/// <param name="N">Complexity of benchmark (N).</param>
		/// <param name="M">Complexity of benchmark (M).</param>
		/// <param name="StartTicks">Start ticks.</param>
		internal Benchmarking(IBenchmarker Benchmarker, string Name, long N, long M, 
			long StartTicks)
		{
			this.benchmarker = Benchmarker;
			this.name = Name;
			this.n = N;
			this.m = M;
			this.startTicks = StartTicks;
		}

		/// <summary>
		/// Stops the benchmarking test and disposes the object.
		/// </summary>
		public void Dispose()
		{
			this.benchmarker.Stop(this.name, this.n, this.m, this.startTicks);
		}
	}
}
