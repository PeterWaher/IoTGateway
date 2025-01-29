using System;
using System.Threading;
using System.Threading.Tasks;

namespace Waher.Networking.HTTP.HTTP2
{
	/// <summary>
	/// Interface for managing HTTP/2 flow control.
	/// </summary>
	public interface IFlowControl : IDisposable
	{
		/// <summary>
		/// Local Connection settings.
		/// </summary>
		ConnectionSettings LocalSettings { get; }

		/// <summary>
		/// Remote Connection settings.
		/// </summary>
		ConnectionSettings RemoteSettings { get; }

		/// <summary>
		/// Called when remote connection settings have been updated.
		/// </summary>
		void RemoteSettingsUpdated();

		/// <summary>
		/// Tries to get a stream, given its associated Stream ID.
		/// </summary>
		/// <param name="StreamId">Stream ID</param>
		/// <param name="Stream">Stream object, if found.</param>
		/// <returns>If a stream object was found with the corresponding ID.</returns>
		bool TryGetStream(int StreamId, out Http2Stream Stream);

		/// <summary>
		/// Tries to add a stream to flow control.
		/// </summary>
		/// <param name="Stream">Stream to add.</param>
		/// <param name="Weight">Weight</param>
		/// <param name="StreamIdDependency">ID of stream dependency, if any. 0 = root.</param>
		/// <param name="Exclusive">If the stream is exclusive child.</param>
		/// <returns>Size of window associated with stream. Negative = error</returns>
		int AddStream(Http2Stream Stream, byte Weight, int StreamIdDependency, bool Exclusive);

		/// <summary>
		/// Tries to remove a string from flow control.
		/// </summary>
		/// <param name="Stream">Stream to remove.</param>
		/// <returns>If the stream could be found, and was removed.</returns>
		bool RemoveStream(Http2Stream Stream);

		/// <summary>
		/// Tries to remove a string from flow control.
		/// </summary>
		/// <param name="StreamId">ID of stream to remove.</param>
		/// <returns>If the stream could be found, and was removed.</returns>
		bool RemoveStream(int StreamId);

		/// <summary>
		/// Requests resources for a stream.
		/// </summary>
		/// <param name="StreamId">ID of stream requesting resources.</param>
		/// <param name="RequestedResources">Amount of resources.</param>
		/// <param name="CancellationToken">Optional Cancellation token.</param>
		/// <returns>Amount of resources granted. If negative, the stream is no
		/// longer controlled (i.e. it has been removed and/or closed).</returns>
		Task<int> RequestResources(int StreamId, int RequestedResources, CancellationToken? CancellationToken);

		/// <summary>
		/// Releases stream resources back to the stream.
		/// </summary>
		/// <param name="StreamId">ID of stream releasing resources.</param>
		/// <param name="Resources">Amount of resources released back</param>
		/// <returns>Size of current window. Negative = error</returns>
		int ReleaseStreamResources(int StreamId, int Resources);

		/// <summary>
		/// Releases connection resources back.
		/// </summary>
		/// <param name="Resources">Amount of resources released back</param>
		/// <returns>Size of current window. Negative = error</returns>
		int ReleaseConnectionResources(int Resources);

		/// <summary>
		/// Adds a pending window size increment
		/// </summary>
		/// <param name="Stream">Stream</param>
		/// <param name="NrBytes">Size of increment, in number of bytes.</param>
		void AddPendingIncrement(Http2Stream Stream, int NrBytes);

		/// <summary>
		/// If the connection has pending window size increments.
		/// </summary>
		bool HasPendingIncrements { get; }

		/// <summary>
		/// Gets available pending window size increments.
		/// </summary>
		/// <returns>Array of increments.</returns>
		PendingWindowIncrement[] GetPendingIncrements();

		/// <summary>
		/// Connection is being terminated. Streams above <paramref name="LastPermittedStreamId"/>
		/// can be closed.
		/// </summary>
		/// <param name="LastPermittedStreamId">Last permitted stream ID.</param>
		void GoingAway(int LastPermittedStreamId);

		/// <summary>
		/// Sets the data label of a profiler thread, if available.
		/// </summary>
		/// <param name="StreamId">Stream ID</param>
		/// <param name="Label">Label to set.</param>
		void SetProfilerDataLabel(int StreamId, string Label);
	}
}
