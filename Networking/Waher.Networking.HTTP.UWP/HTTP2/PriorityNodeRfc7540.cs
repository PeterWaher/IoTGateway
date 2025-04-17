using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Waher.Runtime.Profiling;

namespace Waher.Networking.HTTP.HTTP2
{
	/// <summary>
	/// Represents a node in a HTTP/2 priority tree
	/// </summary>
	public class PriorityNodeRfc7540 : IPriorityNode
	{
		private LinkedList<PendingRequest> pendingRequests = null;
		private LinkedList<PriorityNodeRfc7540> childNodes = null;
		private PriorityNodeRfc7540 dependentOn;
		private ProfilerThread dataThread;
		private ProfilerThread windowThread;
		private readonly Profiler profiler;
		private readonly PriorityNodeRfc7540 root;
		private readonly HttpClientConnection connection;
		private readonly int maxFrameSize;
		private readonly bool hasProfiler;
		private double resourceFraction = 1;
		private int totalChildWeights = 0;
		private int windowSize0;
		private int windowSize;
		private int windowSizeFraction;
		private byte weight;
		private bool disposed = false;

		/// <summary>
		/// Represents a node in a HTTP/2 priority tree
		/// </summary>
		/// <param name="DependentNode">Node dependency.</param>
		/// <param name="Root">Root node.</param>
		/// <param name="Stream">Corresponding HTTP/2 stream.</param>
		/// <param name="Weight">Weight assigned to the node.</param>
		/// <param name="FlowControl">Flow control object.</param>
		/// <param name="Profiler">Profiler, if available.</param>
		public PriorityNodeRfc7540(PriorityNodeRfc7540 DependentNode, PriorityNodeRfc7540 Root, Http2Stream Stream, byte Weight,
			FlowControlRfc7540 FlowControl, Profiler Profiler)
		{
			ConnectionSettings Settings = Stream is null ? FlowControl.LocalSettings : FlowControl.RemoteSettings;

			this.dependentOn = DependentNode;
			this.root = Root;
			this.Stream = Stream;
			this.weight = Weight;
			this.windowSize = this.windowSize0 = this.windowSizeFraction = Settings.InitialWindowSize;
			this.maxFrameSize = Settings.MaxFrameSize;
			this.profiler = Profiler;
			this.hasProfiler = !(this.profiler is null);
			this.connection = FlowControl.Connection;
		}

		/// <summary>
		/// Parent node.
		/// </summary>
		public PriorityNodeRfc7540 DependentOn
		{
			get => this.dependentOn;
			internal set => this.dependentOn = value;
		}

		/// <summary>
		/// Parent node in the priority tree.
		/// </summary>
		public PriorityNodeRfc7540 Parent => this.DependentOn ?? this.root;

		/// <summary>
		/// Corresponding HTTP/2 stream.
		/// </summary>
		public Http2Stream Stream { get; }

		/// <summary>
		/// Window Profiler thread, if any.
		/// </summary>
		public ProfilerThread WindowThread => this.windowThread;

		/// <summary>
		/// Window Data thread, if any.
		/// </summary>
		public ProfilerThread DataThread => this.dataThread;

		/// <summary>
		/// HTTP/2 client connection object.
		/// </summary>
		internal HttpClientConnection Connection => this.connection;

		/// <summary>
		/// Fraction of the resources the node can use.
		/// </summary>
		public double ResourceFraction
		{
			get => this.resourceFraction;
			internal set
			{
				this.resourceFraction = value;
				this.windowSizeFraction = (int)Math.Ceiling(this.windowSize0 * this.resourceFraction);
			}
		}

		internal LinkedList<PriorityNodeRfc7540> MoveChildrenFrom()
		{
			LinkedList<PriorityNodeRfc7540> Result = this.childNodes;
			this.childNodes = null;
			this.totalChildWeights = 0;
			return Result;
		}

		internal void MoveChildrenTo(LinkedList<PriorityNodeRfc7540> Children)
		{
			if (!(Children is null))
			{
				if (this.childNodes is null)
					this.childNodes = new LinkedList<PriorityNodeRfc7540>();

				foreach (PriorityNodeRfc7540 Child in Children)
				{
					this.childNodes.AddLast(Child);
					this.totalChildWeights += Child.Weight;
				}

				this.RecalculateChildFractions();
			}
		}

		/// <summary>
		/// Currently available resources
		/// </summary>
		public int AvailableResources => this.windowSize + this.windowSizeFraction - this.windowSize0;

		/// <summary>
		/// Weight assigned to the node.
		/// </summary>
		public byte Weight
		{
			get => this.weight;
			internal set
			{
				if (this.weight != value)
				{
					this.weight = value;

					if (!(this.dependentOn is null))
					{
						this.dependentOn.totalChildWeights += value - this.weight;
						this.ResourceFraction = this.dependentOn.ResourceFraction * this.weight / this.dependentOn.TotalChildWeights;
					}
				}
			}
		}

		/// <summary>
		/// Sum of weights of children.
		/// </summary>
		public int TotalChildWeights => this.totalChildWeights;

		/// <summary>
		/// If the node has child nodes.
		/// </summary>
		public bool HasChildren => !(this.childNodes is null);

		/// <summary>
		/// First child node, if any, or null if none.
		/// </summary>
		public LinkedListNode<PriorityNodeRfc7540> FirstChild => this.childNodes?.First;

		/// <summary>
		/// Adds a child node to the node.
		/// </summary>
		/// <param name="Child">Child node.</param>
		public void AddChildDependency(PriorityNodeRfc7540 Child)
		{
			PriorityNodeRfc7540 OrgChildParent = Child.Parent;
			OrgChildParent?.RemoveChildDependency(Child);

			PriorityNodeRfc7540 Loop = this.Parent;
			while (!(Loop is null) && Loop != Child)
				Loop = Loop.Parent;

			if (!(Loop is null))
			{
				this.Parent.RemoveChildDependency(this);
				OrgChildParent.AddChildDependency(this);
			}

			if (this.childNodes is null)
				this.childNodes = new LinkedList<PriorityNodeRfc7540>();

			this.childNodes.AddLast(Child);
			Child.dependentOn = this;

			this.totalChildWeights += Child.Weight;

			this.RecalculateChildFractions();
		}

		/// <summary>
		/// Removes a child node from the node.
		/// </summary>
		/// <param name="Child">Child node.</param>
		/// <returns>If the child node was not found.</returns>
		public bool RemoveChildDependency(PriorityNodeRfc7540 Child)
		{
			Child.DependentOn = null;

			if (this.childNodes is null)
				return false;

			if (!this.childNodes.Remove(Child))
				return false;

			this.totalChildWeights -= Child.Weight;

			if (this.childNodes?.First is null)
				this.childNodes = null;
			else
				this.RecalculateChildFractions();

			return true;
		}

		private void RecalculateChildFractions()
		{
			if (!(this.childNodes is null) && this.totalChildWeights > 0)
			{
				LinkedListNode<PriorityNodeRfc7540> Loop = this.childNodes.First;
				PriorityNodeRfc7540 Child;

				while (!(Loop is null))
				{
					Child = Loop.Value;
					Child.ResourceFraction = this.ResourceFraction * Child.weight / this.totalChildWeights;
					Loop = Loop.Next;
				}
			}
		}

		private void RecalculateChildWindows(int ConnectionWindowSize, int StreamWindowSize)
		{
			if (!(this.childNodes is null) && this.totalChildWeights > 0)
			{
				LinkedListNode<PriorityNodeRfc7540> Loop = this.childNodes.First;
				PriorityNodeRfc7540 Child;

				while (!(Loop is null))
				{
					Child = Loop.Value;
					Child.SetNewWindowSize(ConnectionWindowSize, (int)(StreamWindowSize * this.ResourceFraction * Child.weight / this.totalChildWeights), false);
					Loop = Loop.Next;
				}
			}
		}

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

				if (this.hasProfiler)
				{
					if (this.windowThread is null)
						this.CheckProfilerThreads();

					this.windowThread.NewSample(this.windowSize);
					this.root.windowThread.NewSample(this.root.windowSize);

					this.windowSize -= Available;
					this.root.windowSize -= Available;

					this.windowThread.NewSample(this.windowSize);
					this.root.windowThread.NewSample(this.root.windowSize);

					this.dataThread.NewSample(0);
					this.dataThread.NewSample(Available);
				}
				else
				{
					this.windowSize -= Available;
					this.root.windowSize -= Available;
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

			if (this.hasProfiler)
			{
				if (this.windowThread is null)
					this.CheckProfilerThreads();

				this.windowThread.NewSample(this.windowSize);

				this.windowSize = NewSize;

				this.windowThread.Event("StreamWindowUpdate");
				this.windowThread.NewSample(this.windowSize);
			}
			else
				this.windowSize = NewSize;

			if (NewSize > this.windowSize0)
			{
				this.windowSize0 = NewSize;
				this.windowSizeFraction = (int)Math.Ceiling(this.windowSize0 * this.resourceFraction);
			}

			Resources = Math.Min(this.root.AvailableResources, NewSize);
			this.TriggerPending(ref Resources);

			return this.windowSize;
		}

		private void TriggerPending(ref int Resources)
		{
			LinkedList<KeyValuePair<int, PendingRequest>> ToRelease = null;
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

				if (this.hasProfiler)
				{
					if (this.windowThread is null)
						this.CheckProfilerThreads();

					this.windowThread.NewSample(this.windowSize);
					this.root.windowThread.NewSample(this.root.windowSize);

					this.windowSize -= i;
					this.root.windowSize -= i;

					this.windowThread.NewSample(this.windowSize);
					this.root.windowThread.NewSample(this.root.windowSize);
				}
				else
				{
					this.windowSize -= i;
					this.root.windowSize -= i;
				}

				if (ToRelease is null)
					ToRelease = new LinkedList<KeyValuePair<int, PendingRequest>>();

				ToRelease.AddLast(new KeyValuePair<int, PendingRequest>(i, Loop.Value));

				Resources -= i;
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

			if (this.hasProfiler)
			{
				if (this.windowThread is null)
					this.CheckProfilerThreads();

				this.windowThread.NewSample(this.windowSize);

				this.windowSize = NewSize;

				this.windowThread.Event("ConnectionWindowUpdate");
				this.windowThread.NewSample(this.windowSize);
			}
			else
				this.windowSize = NewSize;

			if (NewSize > this.windowSize0)
			{
				this.windowSize0 = NewSize;
				this.windowSizeFraction = (int)Math.Ceiling(this.windowSize0 * this.resourceFraction);
			}


			Resources = NewSize;
			this.TriggerPendingIfAvailbleDown(ref Resources);

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
			this.windowSizeFraction = (int)Math.Ceiling(this.windowSize0 * this.resourceFraction);

			if (this.hasProfiler)
			{
				if (this.windowThread is null)
					this.CheckProfilerThreads();

				this.windowThread.NewSample(this.windowSize);

				this.windowSize += Diff;

				this.windowThread.Event("Settings");
				this.windowThread.NewSample(this.windowSize);
			}
			else
				this.windowSize += Diff;

			this.RecalculateChildWindows(ConnectionWindowSize, StreamWindowSize);

			if (Trigger)
			{
				if (!(this.root is null))
					WindowSize = Math.Min(this.root.AvailableResources, WindowSize);

				if (WindowSize > 0)
					this.TriggerPendingIfAvailbleDown(ref WindowSize);
			}
		}

		private void TriggerPendingIfAvailbleDown(ref int Resources)
		{
			this.TriggerPending(ref Resources);
			if (Resources <= 0 || this.totalChildWeights <= 0)
				return;

			LinkedListNode<PriorityNodeRfc7540> ChildLoop = this.childNodes?.First;
			PriorityNodeRfc7540 Child;
			int Resources0 = Resources;
			int Part;
			int Delta;

			while (Resources > 0 && !(ChildLoop is null))
			{
				Child = ChildLoop.Value;
				Part = (Resources0 * Child.weight + this.totalChildWeights - 1) / this.totalChildWeights;

				if (Part > 0)
				{
					Part = Delta = Math.Min(Child.AvailableResources, Math.Min(Part, Resources));
					if (Part > 0)
					{
						Child.TriggerPendingIfAvailbleDown(ref Part);
						Delta -= Part;
						Resources -= Delta;
					}
				}

				ChildLoop = ChildLoop.Next;
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

				LinkedListNode<PriorityNodeRfc7540> ChildLoop = this.childNodes?.First;

				while (!(ChildLoop is null))
				{
					ChildLoop.Value.Dispose();
					ChildLoop = ChildLoop.Next;
				}

				this.childNodes = null;

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
					this.windowThread.NewSample(this.windowSize);
				}
			}
		}
	}
}
