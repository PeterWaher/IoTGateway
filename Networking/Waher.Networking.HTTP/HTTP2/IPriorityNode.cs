using System;
using System.Threading;
using System.Threading.Tasks;
using Waher.Runtime.Profiling;

namespace Waher.Networking.HTTP.HTTP2
{
	/// <summary>
	/// Represents a node in a HTTP/2 priority tree
	/// </summary>
	public interface IPriorityNode : IDisposable
	{
		/// <summary>
		/// Corresponding HTTP/2 stream.
		/// </summary>
		Http2Stream Stream { get; }

		/// <summary>
		/// Currently available resources
		/// </summary>
		int AvailableResources { get; }

		/// <summary>
		/// Requests resources from the available pool of resources in the tree.
		/// </summary>
		/// <param name="RequestedResources">Requested amount of resources.</param>
		/// <param name="CancellationToken">Optional Cancellation token</param>
		/// <returns>Number of resources granted.</returns>
		Task<int> RequestAvailableResources(int RequestedResources,
			CancellationToken? CancellationToken);

		/// <summary>
		/// Releases stream resources back to the stream.
		/// </summary>
		/// <param name="Resources">Amount of resources released back</param>
		/// <returns>Size of current window. Negative = error</returns>
		int ReleaseStreamResources(int Resources);

		/// <summary>
		/// Releases connection resources back.
		/// </summary>
		/// <param name="Resources">Amount of resources released back</param>
		/// <returns>Size of current window. Negative = error</returns>
		int ReleaseConnectionResources(int Resources);

		/// <summary>
		/// Window Profiler thread, if any.
		/// </summary>
		ProfilerThread WindowThread { get; }

		/// <summary>
		/// Window Data thread, if any.
		/// </summary>
		ProfilerThread DataThread { get; }
	}
}
