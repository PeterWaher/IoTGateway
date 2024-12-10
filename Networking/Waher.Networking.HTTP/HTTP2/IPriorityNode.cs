using System;
using System.Threading;
using System.Threading.Tasks;

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
		/// <returns>Number of resources granted.</returns>
		Task<int> RequestAvailableResources(int RequestedResources);

		/// <summary>
		/// Requests resources from the available pool of resources in the tree.
		/// </summary>
		/// <param name="RequestedResources">Requested amount of resources.</param>
		/// <param name="CancelToken">Optional cancel token</param>
		/// <returns>Number of resources granted.</returns>
		Task<int> RequestAvailableResources(int RequestedResources,
			CancellationToken? CancelToken);

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
	}
}
