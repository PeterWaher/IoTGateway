using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Waher.Networking.HTTP.HTTP2
{
	/// <summary>
	/// Represents a node in a HTTP/2 priority tree
	/// </summary>
	public class PriorityNode : IDisposable
	{
		private LinkedList<PendingRequest> pendingRequests = null;
		private LinkedList<PriorityNode> childNodes = null;
		private readonly int windowSize0;
		private readonly int maxFrameSize;
		private double resourceFraction = 1;
		private int totalChildWeights = 0;
		private int windowSize;
		private int windowSizeFraction;
		private byte weight;
		private bool disposed = false;

		private class PendingRequest
		{
			public TaskCompletionSource<int> Request;
			public int Requested;
		}

		/// <summary>
		/// Represents a node in a HTTP/2 priority tree
		/// </summary>
		/// <param name="DependentNode">Node dependency.</param>
		/// <param name="Root">Root node.</param>
		/// <param name="Stream">Corresponding HTTP/2 stream.</param>
		/// <param name="Weight">Weight assigned to the node.</param>
		/// <param name="FlowControl">Flow control object.</param>
		public PriorityNode(PriorityNode DependentNode, PriorityNode Root, Http2Stream Stream, byte Weight,
			IFlowControl FlowControl)
		{
			ConnectionSettings Settings = FlowControl.Settings;

			this.DependentOn = DependentNode;
			this.Root = Root;
			this.Stream = Stream;
			this.weight = Weight;
			this.windowSize = this.windowSize0 = this.windowSizeFraction = Settings.InitialWindowSize;
			this.maxFrameSize = Settings.MaxFrameSize;
		}

		/// <summary>
		/// Parent node.
		/// </summary>
		public PriorityNode DependentOn
		{
			get;
			internal set;
		}

		/// <summary>
		/// Parent node in the priority tree.
		/// </summary>
		public PriorityNode Parent => this.DependentOn ?? this.Root;

		/// <summary>
		/// Root node.
		/// </summary>
		public PriorityNode Root { get; }

		/// <summary>
		/// Corresponding HTTP/2 stream.
		/// </summary>
		public Http2Stream Stream { get; }

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

		internal LinkedList<PriorityNode> MoveChildrenFrom()
		{
			LinkedList<PriorityNode> Result = this.childNodes;
			this.childNodes = null;
			this.totalChildWeights = 0;
			return Result;
		}

		internal void MoveChildrenTo(LinkedList<PriorityNode> Children)
		{
			if (!(Children is null))
			{
				this.childNodes ??= new LinkedList<PriorityNode>();

				foreach (PriorityNode Child in Children)
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
					if (!(this.DependentOn is null))
						this.DependentOn.totalChildWeights += value - this.weight;

					this.weight = value;
					this.ResourceFraction = this.DependentOn.ResourceFraction * this.weight / this.DependentOn.totalChildWeights;
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
		public LinkedListNode<PriorityNode> FirstChild => this.childNodes?.First;

		/// <summary>
		/// Adds a child node to the node.
		/// </summary>
		/// <param name="Child">Child node.</param>
		public void AddChildDependency(PriorityNode Child)
		{
			PriorityNode OrgChildParent = Child.Parent;
			OrgChildParent?.RemoveChildDependency(Child);

			PriorityNode Loop = this.Parent;
			while (!(Loop is null) && Loop != Child)
				Loop = Loop.Parent;

			if (!(Loop is null))
			{
				this.Parent.RemoveChildDependency(this);
				OrgChildParent.AddChildDependency(this);
			}

			this.childNodes ??= new LinkedList<PriorityNode>();
			this.childNodes.AddLast(Child);

			this.totalChildWeights += Child.Weight;

			this.RecalculateChildFractions();
		}

		/// <summary>
		/// Removes a child node from the node.
		/// </summary>
		/// <param name="Child">Child node.</param>
		/// <returns>If the child node was not found.</returns>
		public bool RemoveChildDependency(PriorityNode Child)
		{
			if (Child.Parent != this)
				return false;

			Child.DependentOn = null;

			if (this.childNodes is null)
				return false;

			if (!this.childNodes.Remove(Child))
				return false;

			this.totalChildWeights -= Child.Weight;

			if (this.childNodes.First is null)
				this.childNodes = null;
			else
				this.RecalculateChildFractions();

			return true;
		}

		private void RecalculateChildFractions()
		{
			if (!(this.childNodes is null))
			{
				LinkedListNode<PriorityNode> Loop = this.childNodes.First;
				PriorityNode Child;

				while (!(Loop is null))
				{
					Child = Loop.Value;
					Child.ResourceFraction = this.ResourceFraction * Child.weight / this.totalChildWeights;
					Loop = Loop.Next;
				}
			}
		}

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
			Available = Math.Min(Available, this.Root.AvailableResources);

			if (Available == 0)
			{
				PendingRequest Request = new PendingRequest()
				{
					Request = new TaskCompletionSource<int>(),
					Requested = RequestedResources
				};
				this.pendingRequests ??= new LinkedList<PendingRequest>();
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
				this.Root.windowSize -= Available;

				return Task.FromResult(Available);
			}
		}

		/// <summary>
		/// Releases stream resources back to the stream.
		/// </summary>
		/// <param name="Resources">Amount of resources released back</param>
		public bool ReleaseStreamResources(int Resources)
		{
			int NewSize = this.windowSize + Resources;
			if (NewSize < 0 || NewSize > int.MaxValue - 1)
				return false;

			this.windowSize = NewSize;

			this.TriggerPending(ref Resources);

			return true;
		}

		private void TriggerPending(ref int Resources)
		{
			LinkedListNode<PendingRequest> Loop;
			int i;

			while (Resources > 0 && !((Loop = this.pendingRequests?.First) is null))
			{
				this.pendingRequests.RemoveFirst();

				i = Math.Min(Loop.Value.Requested, Resources);
				i = Math.Min(i, this.maxFrameSize);
				this.windowSize -= i;
				Loop.Value.Request.TrySetResult(i);
				Resources -= i;
			}
		}

		/// <summary>
		/// Releases connection resources back.
		/// </summary>
		/// <param name="Resources">Amount of resources released back</param>
		/// <returns>If successful.</returns>
		public bool ReleaseConnectionResources(int Resources)
		{
			int NewSize = this.windowSize + Resources;
			if (NewSize < 0 || NewSize > int.MaxValue - 1)
				return false;

			this.windowSize = NewSize;

			this.TriggerPendingIfAvailbleDown(ref Resources);

			return true;
		}

		private void TriggerPendingIfAvailbleDown(ref int Resources)
		{
			this.TriggerPending(ref Resources);
			if (Resources <= 0)
				return;

			LinkedListNode<PriorityNode> ChildLoop = this.childNodes?.First;
			PriorityNode Child;
			int Resources0 = Resources;
			int Part;
			int Delta;

			while (Resources > 0 && !(ChildLoop is null))
			{
				Child = ChildLoop.Value;
				Part = (Resources0 * Child.weight + this.totalChildWeights - 1) / this.totalChildWeights;

				if (Part > 0)
				{
					Part = Delta = Math.Min(Part, Resources);
					Child.TriggerPendingIfAvailbleDown(ref Part);
					Delta -= Part;
					Resources -= Delta;
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

				LinkedListNode<PriorityNode> ChildLoop = this.childNodes?.First;

				while (!(ChildLoop is null))
				{
					ChildLoop.Value.Dispose();
					ChildLoop = ChildLoop.Next;
				}

				this.childNodes = null;
			}
		}
	}
}
