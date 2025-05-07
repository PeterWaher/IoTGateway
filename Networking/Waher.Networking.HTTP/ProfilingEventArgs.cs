using System;
using Waher.Networking.HTTP.HTTP2;
using Waher.Runtime.Profiling;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Event arguments for profiling event handlers.
	/// </summary>
	public class ProfilingEventArgs : EventArgs
	{
		/// <summary>
		/// Event arguments for profiling event handlers.
		/// </summary>
		/// <param name="Profiler">Profiler for session.</param>
		/// <param name="FlowControl">Flow control mechanism used.</param>
		public ProfilingEventArgs(Profiler Profiler, IFlowControl FlowControl)
		{
			this.Profiler = Profiler;
			this.FlowControl = FlowControl;
		}

		/// <summary>
		/// Profiler for session.
		/// </summary>
		public Profiler Profiler { get; }

		/// <summary>
		/// Flow control mechanism used.
		/// </summary>
		public IFlowControl FlowControl { get; }
	}
}
