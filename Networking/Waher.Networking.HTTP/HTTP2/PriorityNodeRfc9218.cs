//#define INFO_IN_SNIFFERS

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Waher.Events;
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
		private readonly IObservableLayer observable;
		private readonly HttpClientConnection connection;
		private LinkedList<PendingRequest> pendingRequests = null;
		private ProfilerThread windowThread;
		private ProfilerThread dataThread;
		private readonly int outputMaxFrameSize;
		private readonly bool hasProfiler;
		private int outputWindowSize0;
		private int outputWindowSize;
		private bool disposed = false;

		/// <summary>
		/// Represents a node in a HTTP/2 priority tree
		/// </summary>
		/// <param name="Stream">Corresponding HTTP/2 stream.</param>
		/// <param name="FlowControl">Flow control object.</param>
		/// <param name="Profiler">Profiler, if any</param>
		/// <param name="Observable">Observable object.</param>
		internal PriorityNodeRfc9218(Http2Stream Stream, FlowControlRfc9218 FlowControl, 
			Profiler Profiler, IObservableLayer Observable)
		{
			if (Stream is null)
				this.outputWindowSize0 = ConnectionSettings.DefaultHttp2InitialConnectionWindowSize;
			else
				this.outputWindowSize0 = FlowControl.RemoteSettings.InitialStreamWindowSize;

			this.Stream = Stream;
			this.root = FlowControl.Root;
			this.outputWindowSize = this.outputWindowSize0;
			this.outputMaxFrameSize = FlowControl.RemoteSettings.MaxFrameSize;
			this.profiler = Profiler;
			this.hasProfiler = !(this.profiler is null);
			this.connection = FlowControl.Connection;
			this.observable = Observable;
		}

		/// <summary>
		/// Corresponding HTTP/2 stream.
		/// </summary>
		public Http2Stream Stream { get; }

		/// <summary>
		/// Window Size
		/// </summary>
		public int OutputWindowSize => this.outputWindowSize;

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
		public int AvailableResources => this.outputWindowSize;

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
			int AvailableResources = this.AvailableResources;
			int RootAvailableResources = this.root.AvailableResources;
			int Available = Math.Min(RequestedResources,
				Math.Min(RootAvailableResources, AvailableResources));

#if INFO_IN_SNIFFERS
			StringBuilder sb = new StringBuilder();

			sb.Append("Stream ");
			sb.Append((this.Stream?.StreamId ?? 0).ToString());
			sb.Append(" Requests resources: Requested = ");
			sb.Append(RequestedResources.ToString());
			sb.Append(", Available = ");
			sb.Append(AvailableResources.ToString());
			sb.Append(" (at root = ");
			sb.Append(RootAvailableResources.ToString());
			sb.Append("), Granted = ");
			sb.Append(Available.ToString());

			this.observable?.Information(sb.ToString());
#endif

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
				if (Available > this.outputMaxFrameSize)
					Available = this.outputMaxFrameSize;

				if (this.hasProfiler)
				{
					if (this.windowThread is null)
						this.CheckProfilerThreads();

					this.windowThread.NewSample(this.AvailableResources);
					this.root.windowThread.NewSample(this.root.outputWindowSize);
					this.dataThread.NewSample(Available);
				}

				this.outputWindowSize -= Available;
				this.root.outputWindowSize -= Available;

				if (this.hasProfiler)
				{
					this.windowThread.NewSample(this.AvailableResources);
					this.root.windowThread.NewSample(this.root.outputWindowSize);
					this.dataThread.NewSample(Available);
				}

				return Task.FromResult(Available);
			}
		}

		/// <summary>
		/// Releases stream resources back to the stream, as a result of a client sending a
		/// WINDOW_UPDATE frame with Stream ID > 0.
		/// </summary>
		/// <param name="Resources">Amount of resources released back</param>
		/// <returns>Size of current window. Negative = error</returns>
		public int ReleaseStreamResources(int Resources)
		{
			int NewSize = this.outputWindowSize + Resources;
			if (NewSize < 0 || NewSize > int.MaxValue - 1)
				return -2;

			if (this.hasProfiler)
			{
				if (this.windowThread is null)
					this.CheckProfilerThreads();

				this.windowThread.NewSample(this.AvailableResources);
				this.windowThread.Event("StreamWindowUpdate");
			}

			this.outputWindowSize = NewSize;

#if INFO_IN_SNIFFERS
			StringBuilder sb = new StringBuilder();

			sb.Append("Stream ");
			sb.Append((this.Stream?.StreamId ?? 0).ToString());
			sb.Append(" State: New Size = ");
			sb.Append(NewSize.ToString());
#endif

			if (NewSize > this.outputWindowSize0)
			{
				this.outputWindowSize0 = NewSize;

#if INFO_IN_SNIFFERS
				sb.Append(", New Max Size = ");
				sb.Append(this.outputWindowSize0.ToString());
#endif
			}

			if (this.hasProfiler)
				this.windowThread.NewSample(this.AvailableResources);

			int RootAvailableResources = this.root.AvailableResources;
			Resources = Math.Min(RootAvailableResources, this.AvailableResources);

#if INFO_IN_SNIFFERS
			sb.Append(", Available Resources = ");
			sb.Append(Resources.ToString());
			sb.Append(" (at root = ");
			sb.Append(RootAvailableResources.ToString());
			sb.Append(')');

			this.observable?.Information(sb.ToString());
#endif
			this.TriggerPending(ref Resources);

			return this.outputWindowSize;
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

				if (this.outputMaxFrameSize < i)
					i = this.outputMaxFrameSize;

				if (this.hasProfiler)
				{
					if (this.windowThread is null)
						this.CheckProfilerThreads();

					this.windowThread.NewSample(this.AvailableResources);
					this.root.windowThread.NewSample(this.root.outputWindowSize);
				}

				this.outputWindowSize -= i;
				this.root.outputWindowSize -= i;

				if (this.hasProfiler)
				{
					this.windowThread.NewSample(this.AvailableResources);
					this.root.windowThread.NewSample(this.root.outputWindowSize);
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
		/// Releases connection resources back, as a result of a client sending a
		/// WINDOW_UPDATE frame with Stream ID = 0.
		/// </summary>
		/// <param name="Resources">Amount of resources released back</param>
		/// <returns>Size of current window. Negative = error</returns>
		public int ReleaseConnectionResources(int Resources)
		{
			int NewSize = this.outputWindowSize + Resources;
			if (NewSize < 0 || NewSize > int.MaxValue - 1)
				return -1;

			if (this.hasProfiler)
			{
				if (this.windowThread is null)
					this.CheckProfilerThreads();

				this.windowThread.NewSample(this.AvailableResources);
				this.windowThread.Event("ConnectionWindowUpdate");
			}

			this.outputWindowSize = NewSize;

			if (NewSize > this.outputWindowSize0)
				this.outputWindowSize0 = NewSize;

			if (this.hasProfiler)
				this.windowThread.NewSample(this.AvailableResources);

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
			int Diff = WindowSize - this.outputWindowSize0;
			this.outputWindowSize0 = WindowSize;
			this.outputWindowSize += Diff;

			if (this.hasProfiler)
			{
				this.windowThread?.Event("Settings");
				this.windowThread?.NewSample(this.AvailableResources);
			}

			if (Trigger)
			{
				if (!(this.root is null))
					WindowSize = Math.Min(this.root.AvailableResources, WindowSize);

				if (this.outputWindowSize > 0)
				{
					WindowSize = this.outputWindowSize;
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
					this.windowThread?.NewSample(this.outputWindowSize);
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
			Output.AppendLine(this.outputMaxFrameSize.ToString());
			Output.Append("windowSize0 = ");
			Output.AppendLine(this.outputWindowSize0.ToString());
			Output.Append("windowSize = ");
			Output.AppendLine(this.outputWindowSize.ToString());
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

		internal void ExportStates(StringBuilder sb, int Indent)
		{
			int AvailableResources = this.AvailableResources;

			sb.Append("| ");

			if (Indent > 0)
				sb.Append("+-");

			sb.Append((this.Stream?.StreamId ?? 0).ToString());
			sb.Append(": Available = ");
			sb.Append(AvailableResources.ToString());

			if (!(this.root is null))
			{
				int RootAvailableResources = this.root.AvailableResources;

				sb.Append(" (at root = ");
				sb.Append(RootAvailableResources.ToString());
				sb.Append(')');
			}

			sb.AppendLine();
		}

	}
}
