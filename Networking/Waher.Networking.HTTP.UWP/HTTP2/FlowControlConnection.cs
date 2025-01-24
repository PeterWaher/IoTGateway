using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Waher.Networking.HTTP.HTTP2
{
	/// <summary>
	/// Manages connection-level flow control.
	/// </summary>
	public abstract class FlowControlConnection : IFlowControl
	{
		private readonly List<PendingWindowIncrement> pendingIncrements = new List<PendingWindowIncrement>();
		private readonly ConnectionSettings localSettings;
		private readonly ConnectionSettings remoteSettings;
		private long connectionWindowSize;
		private long connectionBytesCommunicated = 0;
		private bool hasPendingIncrements = false;

		/// <summary>
		/// Manages connection-level flow control.
		/// </summary>
		/// <param name="LocalSettings">Local Connection settings.</param>
		/// <param name="RemoteSettings">Remote Connection settings.</param>
		public FlowControlConnection(ConnectionSettings LocalSettings, ConnectionSettings RemoteSettings)
		{
			this.localSettings = LocalSettings;
			this.remoteSettings = RemoteSettings;
			this.connectionWindowSize = LocalSettings.InitialWindowSize;
		}

		/// <summary>
		/// Local Connection settings.
		/// </summary>
		public ConnectionSettings LocalSettings => this.localSettings;

		/// <summary>
		/// Remote Connection settings.
		/// </summary>
		public ConnectionSettings RemoteSettings => this.remoteSettings;

		/// <summary>
		/// Called when connection settings have been updated.
		/// </summary>
		public abstract void RemoteSettingsUpdated();

		/// <summary>
		/// Tries to get a stream, given its associated Stream ID.
		/// </summary>
		/// <param name="StreamId">Stream ID</param>
		/// <param name="Stream">Stream object, if found.</param>
		/// <returns>If a stream object was found with the corresponding ID.</returns>
		public abstract bool TryGetStream(int StreamId, out Http2Stream Stream);

		/// <summary>
		/// Tries to add a stream to flow control.
		/// </summary>
		/// <param name="Stream">Stream to add.</param>
		/// <param name="Weight">Weight</param>
		/// <param name="StreamIdDependency">ID of stream dependency, if any. 0 = root.</param>
		/// <param name="Exclusive">If the stream is exclusive child.</param>
		/// <returns>Size of window associated with stream. Negative = error</returns>
		public abstract int AddStream(Http2Stream Stream, byte Weight, int StreamIdDependency, bool Exclusive);

		/// <summary>
		/// Tries to remove a string from flow control.
		/// </summary>
		/// <param name="Stream">Stream to remove.</param>
		/// <returns>If the stream could be found, and was removed.</returns>
		public abstract bool RemoveStream(Http2Stream Stream);

		/// <summary>
		/// Tries to remove a string from flow control.
		/// </summary>
		/// <param name="StreamId">ID of stream to remove.</param>
		/// <returns>If the stream could be found, and was removed.</returns>
		public abstract bool RemoveStream(int StreamId);

		/// <summary>
		/// Requests resources for a stream.
		/// </summary>
		/// <param name="StreamId">ID of stream requesting resources.</param>
		/// <param name="RequestedResources">Amount of resources.</param>
		/// <param name="CancellationToken">Optional Cancellation token.</param>
		/// <returns>Amount of resources granted. If negative, the stream is no
		/// longer controlled (i.e. it has been removed and/or closed).</returns>
		public abstract Task<int> RequestResources(int StreamId, int RequestedResources, CancellationToken? CancellationToken);

		/// <summary>
		/// Releases stream resources back to the stream.
		/// </summary>
		/// <param name="StreamId">ID of stream releasing resources.</param>
		/// <param name="Resources">Amount of resources released back</param>
		/// <returns>Size of current window. Negative = error</returns>
		public abstract int ReleaseStreamResources(int StreamId, int Resources);

		/// <summary>
		/// Releases connection resources back.
		/// </summary>
		/// <param name="Resources">Amount of resources released back</param>
		/// <returns>Size of current window. Negative = error</returns>
		public abstract int ReleaseConnectionResources(int Resources);

		/// <summary>
		/// Connection is being terminated. Streams above <paramref name="LastPermittedStreamId"/>
		/// can be closed.
		/// </summary>
		/// <param name="LastPermittedStreamId">Last permitted stream ID.</param>
		public abstract void GoingAway(int LastPermittedStreamId);

		/// <summary>
		/// Disposes the object and terminates all tasks.
		/// </summary>
		public abstract void Dispose();

		/// <summary>
		/// If the connection has pending window size increments.
		/// </summary>
		public bool HasPendingIncrements => this.hasPendingIncrements;

		/// <summary>
		/// Gets available pending window size increments.
		/// </summary>
		/// <returns>Array of increments.</returns>
		public PendingWindowIncrement[] GetPendingIncrements()
		{
			PendingWindowIncrement[] Result = this.pendingIncrements.ToArray();
			this.pendingIncrements.Clear();
			this.hasPendingIncrements = false;
			return Result;
		}

		/// <summary>
		/// Adds a pending window size increment
		/// </summary>
		/// <param name="Stream">Stream</param>
		/// <param name="NrBytes">Size of increment, in number of bytes.</param>
		public void AddPendingIncrement(Http2Stream Stream, int NrBytes)
		{
			int StreamId = Stream.StreamId;

			foreach (PendingWindowIncrement Increment in this.pendingIncrements)
			{
				if (Increment.Stream.StreamId == StreamId)
				{
					Increment.NrBytes += NrBytes;
					return;
				}
			}

			this.pendingIncrements.Add(new PendingWindowIncrement()
			{
				Stream = Stream,
				NrBytes = NrBytes
			});

			this.hasPendingIncrements = true;
		}

		/// <summary>
		/// Sets the window size increment for stream, modified using the WINDOW_UPDATE
		/// frame.
		/// </summary>
		/// <returns>If increment was valid.</returns>
		public bool SetWindowSizeIncrementLocked(int Increment)
		{
			if (Increment <= 0 || Increment > int.MaxValue - 1)
				return false;

			long Size = this.connectionWindowSize + Increment;

			if (Size < 0 || Size > int.MaxValue - 1)
				return false;

			this.connectionWindowSize = Size;

			return true;
		}

		/// <summary>
		/// Number of data bytes sent over the connection
		/// </summary>
		/// <param name="Increment">Increment</param>
		public void DataBytesSentLocked(int Increment)
		{
			this.connectionBytesCommunicated += Increment;
		}

		/// <summary>
		/// Connection window size
		/// </summary>
		public int ConnectionWindowSize
		{
			get => (int)(this.connectionWindowSize - this.connectionBytesCommunicated);
			internal set => this.connectionWindowSize = this.connectionBytesCommunicated + value;
		}
	}
}
