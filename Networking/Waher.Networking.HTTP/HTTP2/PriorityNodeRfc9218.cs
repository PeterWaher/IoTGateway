using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Waher.Runtime.Profiling;

namespace Waher.Networking.HTTP.HTTP2
{
	/// <summary>
	/// Represents a node in a HTTP/2 priority tree
	/// </summary>
	public class PriorityNodeRfc9218 : IPriorityNode
	{
		private readonly PriorityNodeRfc9218 root;
		private readonly Profiler profiler;
		private readonly HttpClientConnection connection;
		private LinkedList<PendingRequest> pendingRequests = null;
		private ProfilerThread windowThread;
		private ProfilerThread dataThread;
		private readonly int maxFrameSize;
		private readonly bool hasProfiler;
		private int windowSize0;
		private int windowSize;
		private bool disposed = false;

		/// <summary>
		/// Represents a node in a HTTP/2 priority tree
		/// </summary>
		/// <param name="Stream">Corresponding HTTP/2 stream.</param>
		/// <param name="FlowControl">Flow control object.</param>
		/// <param name="Profiler">Profiler, if any</param>
		internal PriorityNodeRfc9218(Http2Stream Stream, FlowControlRfc9218 FlowControl, 
			Profiler Profiler)
		{
			ConnectionSettings Settings = Stream is null ? FlowControl.LocalSettings : FlowControl.RemoteSettings;

			this.Stream = Stream;
			this.root = FlowControl.Root;
			this.windowSize = this.windowSize0 = Settings.InitialWindowSize;
			this.maxFrameSize = Settings.MaxFrameSize;
			this.profiler = Profiler;
			this.hasProfiler = !(this.profiler is null);
			this.connection = FlowControl.Connection;
		}

		/// <summary>
		/// Corresponding HTTP/2 stream.
		/// </summary>
		public Http2Stream Stream { get; }

		/// <summary>
		/// Window Size
		/// </summary>
		public int WindowSize => this.windowSize;

		/// <summary>
		/// Window Profiler thread, if any.
		/// </summary>
		public ProfilerThread WindowThread => this.windowThread;

		/// <summary>
		/// Window Data thread, if any.
		/// </summary>
		public ProfilerThread DataThread => this.dataThread;

		/// <summary>
		/// Currently available resources
		/// </summary>
		public int AvailableResources => this.windowSize;

		/// <summary>
		/// HTTP/2 client connection object.
		/// </summary>
		internal HttpClientConnection Connection => this.connection;

		/// <summary>
		/// Requests resources from the available pool of resources in the tree.
		/// </summary>
		/// <param name="RequestedResources">Requested amount of resources.</param>
		/// <param name="CancellationToken">Optional Cancellation token</param>
		/// <returns>Number of resources granted.</returns>
		public Task<int> RequestAvailableResources(int RequestedResources,
			CancellationToken? CancellationToken)
		{
			int Available = Math.Min(RequestedResources, this.AvailableResources);
			Available = Math.Min(Available, this.root.AvailableResources);

			if (Available <= 0)
			{
				PendingRequest Request = new PendingRequest(RequestedResources);

				if (this.pendingRequests is null)
					this.pendingRequests = new LinkedList<PendingRequest>();
				
				this.pendingRequests.AddLast(Request);

				if (CancellationToken.HasValue && CancellationToken.Value.CanBeCanceled)
					CancellationToken.Value.Register(() => Request.Request.TrySetException(new OperationCanceledException()));

				return Request.Request.Task;
			}
			else
			{
				if (Available > this.maxFrameSize)
					Available = this.maxFrameSize;

				this.windowSize -= Available;
				this.root.windowSize -= Available;

				if (this.hasProfiler)
				{
					if (this.windowThread is null)
						this.CheckProfilerThreads();

					this.windowThread.NewSample(this.AvailableResources);
					this.root.windowThread.NewSample(this.root.windowSize);
					this.dataThread.NewSample(Available);
				}

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

			if (this.hasProfiler)
			{
				if (this.windowThread is null)
					this.CheckProfilerThreads();

				this.windowThread.Event("StreamWindowUpdate");
				this.windowThread.NewSample(this.AvailableResources);
			}

			Resources = Math.Min(this.root.AvailableResources, NewSize);
			this.TriggerPending(ref Resources);

			return this.windowSize;
		}

		internal void TriggerPending(ref int Resources)
		{
			LinkedList<KeyValuePair<int, PendingRequest>> ToRelease = null;
			LinkedListNode<PendingRequest> Loop;
			int MaxResources = Math.Min(this.AvailableResources, Resources);
			int i;

			while (MaxResources > 0 && !((Loop = this.pendingRequests?.First) is null))
			{
				this.pendingRequests.RemoveFirst();

				i = Loop.Value.Requested;

				if (MaxResources < i)
					i = MaxResources;

				if (this.maxFrameSize < i)
					i = this.maxFrameSize;

				this.windowSize -= i;
				this.root.windowSize -= i;

				if (this.hasProfiler)
				{
					if (this.windowThread is null)
						this.CheckProfilerThreads();

					this.windowThread.NewSample(this.AvailableResources);
					this.root.windowThread.NewSample(this.root.windowSize);
				}

				if (ToRelease is null)
					ToRelease = new LinkedList<KeyValuePair<int, PendingRequest>>();

				ToRelease.AddLast(new KeyValuePair<int, PendingRequest>(i, Loop.Value));

				Resources -= i;
				MaxResources -= i;
			}

			if (!(ToRelease is null))
			{
				foreach (KeyValuePair<int, PendingRequest> P in ToRelease)
					P.Value.Request.TrySetResult(P.Key);
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

			if (this.hasProfiler)
			{
				if (this.windowThread is null)
					this.CheckProfilerThreads();

				this.windowThread.Event("ConnectionWindowUpdate");
				this.windowThread.NewSample(this.AvailableResources);
			}

			return NewSize;
		}

		/// <summary>
		/// Sets a new window size.
		/// </summary>
		/// <param name="ConnectionWindowSize">Connection Window size</param>
		/// <param name="StreamWindowSize">Stream Window size</param>
		/// <param name="Trigger">If pending streams should be triggered.</param>
		public void SetNewWindowSize(int ConnectionWindowSize, int StreamWindowSize, bool Trigger)
		{
			int WindowSize = this.Stream is null ? ConnectionWindowSize : StreamWindowSize;
			int Diff = WindowSize - this.windowSize0;
			this.windowSize0 = WindowSize;
			this.windowSize += Diff;

			if (this.hasProfiler)
			{
				this.windowThread?.Event("Settings");
				this.windowThread?.NewSample(this.AvailableResources);
			}

			if (Trigger)
			{
				if (!(this.root is null))
					WindowSize = Math.Min(this.root.AvailableResources, WindowSize);

				if (this.windowSize > 0)
				{
					WindowSize = this.windowSize;
					this.TriggerPending(ref WindowSize);
				}
			}
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

				if (this.hasProfiler)
				{
					this.windowThread?.NewSample(this.windowSize);
					this.windowThread?.Idle();
					this.windowThread?.Stop();

					this.dataThread?.NewSample(0);
					this.dataThread?.Idle();
					this.dataThread?.Stop();
				}
			}
		}

		/// <summary>
		/// Checks the node has the corresponding profiler threads created.
		/// </summary>
		public void CheckProfilerThreads()
		{
			if (this.hasProfiler)
			{
				if (this.dataThread is null && !(this.Stream is null))
				{
					this.dataThread = HttpClientConnection.CreateProfilerDataThread(this.profiler, this.Stream.StreamId);
					this.dataThread.NewSample(0);
				}

				if (this.windowThread is null)
				{
					this.windowThread = HttpClientConnection.CreateProfilerWindowThread(this.profiler, this.Stream?.StreamId ?? 0);
					this.windowThread.NewSample(this.AvailableResources);
				}
			}
		}

		/// <summary>
		/// Exports the priorty node to PlantUML format.
		/// </summary>
		/// <param name="Output">UML diagram will be exported here.</param>
		public void ExportPlantUml(StringBuilder Output)
		{
			if (this.Stream is null)
				Output.AppendLine("object \"Root\" as S {");
			else
			{
				Output.Append("object \"Stream ");
				Output.Append(this.Stream.StreamId.ToString());
				Output.Append("\" as S");
				Output.Append(this.Stream.StreamId.ToString());
				Output.AppendLine(" {");
				Output.Append("resource = \"");
				Output.Append(PriorityNodeRfc7540.GetResourceFromLabel(this.Stream.StreamThread?.Label));
				Output.AppendLine("\"");
				Output.Append("pendingRequests = ");
				Output.AppendLine(this.pendingRequests?.Count.ToString() ?? "0");
			}

			Output.Append("maxFrameSize = ");
			Output.AppendLine(this.maxFrameSize.ToString());
			Output.Append("windowSize0 = ");
			Output.AppendLine(this.windowSize0.ToString());
			Output.Append("windowSize = ");
			Output.AppendLine(this.windowSize.ToString());
			Output.Append("AvailableResources = ");
			Output.AppendLine(this.AvailableResources.ToString());
			Output.AppendLine("}");
			Output.AppendLine();

			if (!(this.Stream is null))
			{
				Output.Append("S *-- S");
				Output.AppendLine(this.Stream.StreamId.ToString());
				Output.AppendLine();
			}
		}

	}
}
