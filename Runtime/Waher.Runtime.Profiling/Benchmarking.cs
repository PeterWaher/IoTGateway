using System;

namespace Waher.Runtime.Profiling
{
	/// <summary>
	/// Class that keeps track of benchmarking results and timing.
	/// </summary>
	public class Benchmarking : IDisposable
	{
		private readonly Benchmarker benchmarker;
		private readonly string name;
		private readonly long complexity;
		private readonly long startTicks;

		/// <summary>
		/// Class that keeps track of benchmarking results and timing.
		/// </summary>
		/// <param name="Benchmarker">Benchmarker object.</param>
		/// <param name="Name">Name of benchmark.</param>
		/// <param name="Complexity">Complexity of benchmark.</param>
		/// <param name="StartTicks">Start ticks.</param>
		public Benchmarking(Benchmarker Benchmarker, string Name, long Complexity, 
			long StartTicks)
		{
			this.benchmarker = Benchmarker;
			this.name = Name;
			this.complexity = Complexity;
			this.startTicks = StartTicks;
		}

		/// <summary>
		/// Stops the benchmarking test and disposes the object.
		/// </summary>
		public void Dispose()
		{
			this.benchmarker.Stop(this.name, this.complexity, this.startTicks);
		}
	}
}
