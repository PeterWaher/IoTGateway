using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Waher.Networking.HTTP.HTTP2
{
	/// <summary>
	/// Represents a node in a HTTP/2 priority tree
	/// </summary>
	public class PriorityNodeRfc9218 : IPriorityNode
	{
		private readonly PriorityNodeRfc9218 root;
		private LinkedList<PendingRequest> pendingRequests = null;
		private readonly int maxFrameSize;
		private int windowSize0;
		private int windowSize;
		private bool disposed = false;

		/// <summary>
		/// Represents a node in a HTTP/2 priority tree
		/// </summary>
		/// <param name="Stream">Corresponding HTTP/2 stream.</param>
		/// <param name="FlowControl">Flow control object.</param>
		public PriorityNodeRfc9218(Http2Stream Stream, FlowControlRfc9218 FlowControl)
		{
			ConnectionSettings Settings = Stream is null ? FlowControl.LocalSettings : FlowControl.RemoteSettings;

			this.Stream = Stream;
			this.root = FlowControl.Root;
			this.windowSize = this.windowSize0 = Settings.InitialWindowSize;
			this.maxFrameSize = Settings.MaxFrameSize;
		}

		/// <summary>
		/// Corresponding HTTP/2 stream.
		/// </summary>
		public Http2Stream Stream { get; }

		/// <summary>
		/// Currently available resources
		/// </summary>
		public int AvailableResources => this.windowSize;

		/// <summary>
		/// Requests resources from the available pool of resources in the tree.
		/// </summary>
		/// <param name="RequestedResources">Requested amount of resources.</param>
		/// <returns>Number of resources granted.</returns>
		public Task<int> RequestAvailableResources(int RequestedResources)
		{
			return this.RequestAvailableResources(RequestedResources, null);
		}

		/// <summary>
		/// Requests resources from the available pool of resources in the tree.
		/// </summary>
		/// <param name="RequestedResources">Requested amount of resources.</param>
		/// <param name="CancelToken">Optional cancel token</param>
		/// <returns>Number of resources granted.</returns>
		public Task<int> RequestAvailableResources(int RequestedResources,
			CancellationToken? CancelToken)
		{
			int Available = Math.Min(RequestedResources, this.AvailableResources);
			Available = Math.Min(Available, this.root.AvailableResources);

			if (Available <= 0)
			{
				PendingRequest Request = new PendingRequest(RequestedResources);

				if (this.pendingRequests is null)
					this.pendingRequests = new LinkedList<PendingRequest>();

				this.pendingRequests.AddLast(Request);

				if (CancelToken.HasValue)
					CancelToken.Value.Register(() => Request.Request.TrySetException(new OperationCanceledException()));

				return Request.Request.Task;
			}
			else
			{
				if (Available > this.maxFrameSize)
					Available = this.maxFrameSize;

				this.windowSize -= Available;
				this.root.windowSize -= Available;

				return Task.FromResult(Available);
			}
		}

		/// <summary>
		/// Releases stream resources back to the stream.
		/// </summary>
		/// <param name="Resources">Amount of resources released back</param>
		/// <returns>Size of current window. Negative = error</returns>
		public int ReleaseStreamResources(int Resources)
		{
			int NewSize = this.windowSize + Resources;
			if (NewSize < 0 || NewSize > int.MaxValue - 1)
				return -2;

			this.windowSize = NewSize;

			if (NewSize > this.windowSize0)
				this.windowSize0 = NewSize;

			Resources = Math.Min(this.root.AvailableResources, NewSize);
			this.TriggerPending(ref Resources);

			return NewSize;
		}

		internal void TriggerPending(ref int Resources)
		{
			LinkedListNode<PendingRequest> Loop;
			int i;

			while (Resources > 0 && !((Loop = this.pendingRequests?.First) is null))
			{
				this.pendingRequests.RemoveFirst();

				i = Loop.Value.Requested;

				if (Resources < i)
					i = Resources;

				if (this.maxFrameSize < i)
					i = this.maxFrameSize;

				this.windowSize -= i;
				this.root.windowSize -= i;
				Loop.Value.Request.TrySetResult(i);
				Resources -= i;
			}
		}

		/// <summary>
		/// Releases connection resources back.
		/// </summary>
		/// <param name="Resources">Amount of resources released back</param>
		/// <returns>Size of current window. Negative = error</returns>
		public int ReleaseConnectionResources(int Resources)
		{
			int NewSize = this.windowSize + Resources;
			if (NewSize < 0 || NewSize > int.MaxValue - 1)
				return -1;

			this.windowSize = NewSize;

			if (NewSize > this.windowSize0)
				this.windowSize0 = NewSize;

			return NewSize;
		}

		/// <summary>
		/// Sets a new window size.
		/// </summary>
		/// <param name="ConnectionWindowSize">Connection Window size</param>
		/// <param name="StreamWindowSize">Stream Window size</param>
		public void SetNewWindowSize(int ConnectionWindowSize, int StreamWindowSize)
		{
			int WindowSize = this.Stream is null ? ConnectionWindowSize : StreamWindowSize;
			int Diff = WindowSize - this.windowSize0;
			this.windowSize0 = WindowSize;
			this.windowSize += Diff;

			if (!(this.root is null))
				WindowSize = Math.Min(this.root.AvailableResources, WindowSize);

			if (WindowSize > 0)
				this.TriggerPending(ref WindowSize);
		}

		/// <summary>
		/// Disposes the node and its children, and releases any pending tasks.
		/// </summary>
		public void Dispose()
		{
			if (!this.disposed)
			{
				this.disposed = true;

				LinkedListNode<PendingRequest> RequestLoop = this.pendingRequests?.First;

				while (!(RequestLoop is null))
				{
					RequestLoop.Value.Request.TrySetResult(-1);
					RequestLoop = RequestLoop.Next;
				}

				this.pendingRequests = null;

				if (!(this.Stream is null))
					this.Stream.State = StreamState.Closed;
			}
		}
	}
}
