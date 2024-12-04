using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Waher.Networking.HTTP.HTTP2
{
	/// <summary>
	/// Class that manages HTTP/2 flow control using trees of priorty nodes.
	/// </summary>
	public class FlowControl : IDisposable
	{
		private readonly Dictionary<int, PriorityNode> nodes = new Dictionary<int, PriorityNode>();
		private readonly object synchObj = new object();
		private readonly PriorityNode root;
		private ConnectionSettings settings;
		private int lastNodeStreamId = -1;
		private PriorityNode lastNode = null;
		private bool disposed = false;

		/// <summary>
		/// Class that manages HTTP/2 flow control using trees of priorty nodes.
		/// </summary>
		/// <param name="Settings">Connection settings.</param>
		public FlowControl(ConnectionSettings Settings)
		{
			this.settings = Settings;
			this.root = new PriorityNode(null, null, null, 1, this);
		}

		/// <summary>
		/// Connection settings.
		/// </summary>
		public ConnectionSettings Settings => this.settings;

		/// <summary>
		/// Root node.
		/// </summary>
		public PriorityNode Root => this.root;

		/// <summary>
		/// Updates remote settings.
		/// </summary>
		/// <param name="Settings"></param>
		public void UpdateSettings(ConnectionSettings Settings)
		{
			this.settings = Settings;
		}

		/// <summary>
		/// Tries to get a priority node, given its associated Stream ID.
		/// </summary>
		/// <param name="StreamId">Stream ID</param>
		/// <param name="Node">Priority node, if found.</param>
		/// <returns>If a priority node was found with the corresponding ID.</returns>
		public bool TryGetPriorityNode(int StreamId, out PriorityNode Node)
		{
			if (this.disposed)
			{
				Node = null;
				return false;
			}

			lock (this.synchObj)
			{
				return this.nodes.TryGetValue(StreamId, out Node);
			}
		}

		/// <summary>
		/// Tries to get a stream, given its associated Stream ID.
		/// </summary>
		/// <param name="StreamId">Stream ID</param>
		/// <param name="Stream">Stream object, if found.</param>
		/// <returns>If a stream object was found with the corresponding ID.</returns>
		public bool TryGetStream(int StreamId, out Http2Stream Stream)
		{
			if (this.disposed)
			{
				Stream = null;
				return false;
			}

			lock (this.synchObj)
			{
				if (this.nodes.TryGetValue(StreamId, out PriorityNode Node))
				{
					Stream = Node.Stream;
					return true;
				}
				else
				{
					Stream = null;
					return false;
				}
			}
		}

		/// <summary>
		/// Tries to add a stream to flow control for testing purposes, using
		/// default priority settings.
		/// </summary>
		/// <param name="StreamId">ID of stream to add.</param>
		/// <returns>If the stream could be added.</returns>
		public bool AddStreamForTest(int StreamId)
		{
			return this.AddStreamForTest(StreamId, this.settings, 16, 0, false);
		}

		/// <summary>
		/// Tries to add a stream to flow control for testing purposes, using
		/// default priority settings.
		/// </summary>
		/// <param name="StreamId">ID of stream to add.</param>
		/// <param name="Settings">Settings to use.</param>
		/// <returns>If the stream could be added.</returns>
		public bool AddStreamForTest(int StreamId, ConnectionSettings Settings)
		{
			return this.AddStreamForTest(StreamId, Settings, 16, 0, false);
		}

		/// <summary>
		/// Tries to add a stream to flow control for testing purposes.
		/// </summary>
		/// <param name="StreamId">ID of stream to add.</param>
		/// <param name="Weight">Weight</param>
		/// <param name="StreamIdDependency">ID of stream dependency, if any. 0 = root.</param>
		/// <param name="Exclusive">If the stream is exclusive child.</param>
		/// <returns>If the stream could be added.</returns>
		public bool AddStreamForTest(int StreamId, byte Weight, int StreamIdDependency, bool Exclusive)
		{
			return this.AddStreamForTest(StreamId, this.settings, Weight, StreamIdDependency, Exclusive);
		}

		/// <summary>
		/// Tries to add a stream to flow control for testing purposes.
		/// </summary>
		/// <param name="StreamId">ID of stream to add.</param>
		/// <param name="Settings">Settings to use.</param>
		/// <param name="Weight">Weight</param>
		/// <param name="StreamIdDependency">ID of stream dependency, if any. 0 = root.</param>
		/// <param name="Exclusive">If the stream is exclusive child.</param>
		/// <returns>If the stream could be added.</returns>
		public bool AddStreamForTest(int StreamId, ConnectionSettings Settings,
			byte Weight, int StreamIdDependency, bool Exclusive)
		{
			Http2Stream Stream = new Http2Stream(StreamId, Settings);
			return this.AddStream(Stream, Weight, StreamIdDependency, Exclusive);
		}

		/// <summary>
		/// Tries to add a stream to flow control.
		/// </summary>
		/// <param name="Stream">Stream to add.</param>
		/// <param name="Weight">Weight</param>
		/// <param name="StreamIdDependency">ID of stream dependency, if any. 0 = root.</param>
		/// <param name="Exclusive">If the stream is exclusive child.</param>
		/// <returns>If the stream could be added.</returns>
		public bool AddStream(Http2Stream Stream, byte Weight, int StreamIdDependency, bool Exclusive)
		{
			if (this.disposed)
				return false;

			lock (this.synchObj)
			{
				if (this.nodes.Count >= this.settings.MaxConcurrentStreams)
					return false;

				if (StreamIdDependency == 0 ||
					!this.nodes.TryGetValue(StreamIdDependency, out PriorityNode DependentOn))
				{
					DependentOn = null;
				}

				PriorityNode Parent = DependentOn ?? this.root;

				if (this.nodes.TryGetValue(Stream.StreamId, out PriorityNode Node))
				{
					bool Add = false;

					if (!(Node.DependentOn is null) &&
						(Node.DependentOn != DependentOn || Exclusive))
					{
						Node.Parent.RemoveChildDependency(Node);
						Add = true;
					}

					if (Exclusive)
						this.MoveChildren(DependentOn, Node);

					if (Add)
						Parent.AddChildDependency(Node);

					Node.Weight = Weight;
				}
				else
				{
					Node = new PriorityNode(DependentOn, this.root, Stream, Weight, this);

					if (Exclusive)
						this.MoveChildren(DependentOn, Node);

					Parent.AddChildDependency(Node);
					this.nodes[Stream.StreamId] = Node;
				}

				return true;
			}
		}

		private void MoveChildren(PriorityNode From, PriorityNode To)
		{
			LinkedList<PriorityNode> Children = From.MoveChildrenFrom();
			To.MoveChildrenTo(Children);
		}

		/// <summary>
		/// Updates the priority of a stream in the flow control.
		/// </summary>
		/// <param name="Stream">Stream to update.</param>
		/// <param name="Weight">Weight</param>
		/// <param name="StreamIdDependency">ID of stream dependency, if any. 0 = root.</param>
		/// <param name="Exclusive">If the stream is exclusive child.</param>
		/// <returns>If the stream could be updated.</returns>
		public bool UpdatePriority(Http2Stream Stream, byte Weight, int StreamIdDependency, bool Exclusive)
		{
			if (this.disposed)
				return false;

			lock (this.synchObj)
			{
				if (StreamIdDependency == 0 ||
					!this.nodes.TryGetValue(StreamIdDependency, out PriorityNode DependentOn))
				{
					DependentOn = null;
				}

				PriorityNode Parent = DependentOn ?? this.root;

				if (!this.nodes.TryGetValue(Stream.StreamId, out PriorityNode Node))
					return false;

				bool Add = false;

				if (!(Node.DependentOn is null) &&
					(Node.DependentOn != DependentOn || Exclusive))
				{
					Node.Parent.RemoveChildDependency(Node);
					Add = true;
				}

				if (Exclusive)
					this.MoveChildren(DependentOn, Node);

				if (Add)
					Parent.AddChildDependency(Node);

				Node.Weight = Weight;

				return true;
			}
		}

		/// <summary>
		/// Tries to remove a string from flow control.
		/// </summary>
		/// <param name="Stream">Stream to remove.</param>
		/// <returns>If the stream could be found, and was removed.</returns>
		public bool RemoveStream(Http2Stream Stream)
		{
			return this.RemoveStream(Stream.StreamId);
		}

		/// <summary>
		/// Tries to remove a string from flow control.
		/// </summary>
		/// <param name="StreamId">ID of stream to remove.</param>
		/// <returns>If the stream could be found, and was removed.</returns>
		public bool RemoveStream(int StreamId)
		{
			if (this.disposed)
				return false;

			lock (this.synchObj)
			{
				if (!this.nodes.TryGetValue(StreamId, out PriorityNode Node))
					return false;

				this.nodes.Remove(StreamId);
				if (this.lastNodeStreamId == StreamId)
				{
					this.lastNodeStreamId = 0;
					this.lastNode = null;
				}

				PriorityNode Parent = Node.Parent;
				if (!(Parent is null))
				{
					Parent.RemoveChildDependency(Node);

					double Scale = ((double)Node.Weight) / Parent.TotalChildWeights;

					while (Node.HasChildren)
					{
						PriorityNode Child = Node.FirstChild.Value;
						int ScaledWeight = (int)Math.Ceiling(Child.Weight * Scale);
						if (ScaledWeight > 255)
							ScaledWeight = 255;

						Node.RemoveChildDependency(Child);
						Child.Weight = (byte)ScaledWeight;
						Parent.AddChildDependency(Child);
					}
				}

				Node.Dispose();

				return true;
			}
		}

		/// <summary>
		/// Requests resources for a stream.
		/// </summary>
		/// <param name="StreamId">ID of stream requesting resources.</param>
		/// <param name="RequestedResources">Amount of resources.</param>
		/// <returns>Amount of resources granted. If negative, the stream is no
		/// longer controlled (i.e. it has been removed and/or closed).</returns>
		public Task<int> RequestResources(int StreamId, int RequestedResources)
		{
			return this.RequestResources(StreamId, RequestedResources, null);
		}

		/// <summary>
		/// Requests resources for a stream.
		/// </summary>
		/// <param name="StreamId">ID of stream requesting resources.</param>
		/// <param name="RequestedResources">Amount of resources.</param>
		/// <param name="CancelToken">Optional cancel token</param>
		/// <returns>Amount of resources granted. If negative, the stream is no
		/// longer controlled (i.e. it has been removed and/or closed).</returns>
		public Task<int> RequestResources(int StreamId, int RequestedResources,
			CancellationToken? CancelToken)
		{
			if (this.disposed)
				return Task.FromResult(-1);

			lock (this.synchObj)
			{
				if (StreamId != this.lastNodeStreamId)
				{
					if (!this.nodes.TryGetValue(StreamId, out this.lastNode))
						return Task.FromResult(-1);

					this.lastNodeStreamId = StreamId;
				}

				return this.lastNode.RequestAvailableResources(RequestedResources, CancelToken);
			}
		}

		/// <summary>
		/// Releases stream resources back to the stream.
		/// </summary>
		/// <param name="StreamId">ID of stream releasing resources.</param>
		/// <param name="Resources">Amount of resources released back</param>
		/// <returns>If stream was found with the corresponding ID.</returns>
		public bool ReleaseStreamResources(int StreamId, int Resources)
		{
			if (this.disposed)
				return false;

			lock (this.synchObj)
			{
				if (StreamId != this.lastNodeStreamId)
				{
					if (!this.nodes.TryGetValue(StreamId, out this.lastNode))
						return false;

					this.lastNodeStreamId = StreamId;
				}

				return this.lastNode.ReleaseStreamResources(Resources);
			}
		}

		/// <summary>
		/// Releases connection resources back.
		/// </summary>
		/// <param name="Resources">Amount of resources released back</param>
		/// <returns>If successful.</returns>
		public bool ReleaseConnectionResources(int Resources)
		{
			if (this.disposed)
				return false;

			lock (this.synchObj)
			{
				return this.root.ReleaseConnectionResources(Resources);
			}
		}

		/// <summary>
		/// Disposes the object and terminates all tasks.
		/// </summary>
		public void Dispose()
		{
			if (!this.disposed)
			{
				this.disposed = true;
				this.root?.Dispose();
			}
		}
	}
}
