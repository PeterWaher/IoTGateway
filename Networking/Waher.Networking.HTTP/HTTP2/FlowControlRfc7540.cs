﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Waher.Runtime.Profiling;

namespace Waher.Networking.HTTP.HTTP2
{
	/// <summary>
	/// Class that manages HTTP/2 flow control using trees of priorty nodes, as defined
	/// in RFC 7540.
	/// </summary>
	public class FlowControlRfc7540 : FlowControlConnection
	{
		private readonly Dictionary<int, PriorityNodeRfc7540> nodes = new Dictionary<int, PriorityNodeRfc7540>();
		private readonly object synchObj = new object();
		private readonly PriorityNodeRfc7540 root;
		private readonly Profiler profiler;
		private readonly bool hasProfiler;
		private int lastNodeStreamId = -1;
		private int lastRemoteInitialWindowSize = 0;
		private PriorityNodeRfc7540 lastNode = null;
		private bool disposed = false;

		/// <summary>
		/// Class that manages HTTP/2 flow control using trees of priorty nodes.
		/// </summary>
		/// <param name="LocalSettings">Local Connection settings.</param>
		/// <param name="RemoteSettings">Remote Connection settings.</param>
		/// <param name="Profiler">Connection profiler.</param>
		public FlowControlRfc7540(ConnectionSettings LocalSettings, ConnectionSettings RemoteSettings,
			Profiler Profiler)
			: base(LocalSettings, RemoteSettings)
		{
			this.profiler = Profiler;
			this.hasProfiler = !(Profiler is null);

			if (Profiler is null)
				this.root = new PriorityNodeRfc7540(null, null, null, 1, this, null, null);
			else
			{
				this.root = new PriorityNodeRfc7540(null, null, null, 1, this,
					HttpClientConnection.CreateProfilerWindowThread(Profiler, 0),
					HttpClientConnection.CreateProfilerDataThread(Profiler, 0));
			}

			this.lastRemoteInitialWindowSize = this.RemoteSettings.InitialWindowSize;
		}

		/// <summary>
		/// Root node.
		/// </summary>
		public PriorityNodeRfc7540 Root => this.root;

		/// <summary>
		/// Called when connection settings have been updated.
		/// </summary>
		public override void RemoteSettingsUpdated()
		{
			int Size = this.RemoteSettings.InitialWindowSize;
			int WindowSizeDiff = Size - this.lastRemoteInitialWindowSize;
			this.lastRemoteInitialWindowSize = Size;

			if (WindowSizeDiff != 0)
			{
				lock (this.synchObj)
				{
					this.root.SetNewWindowSize(this.LocalSettings.InitialWindowSize, Size, true);
				}
			}
		}

		/// <summary>
		/// Tries to get a priority node, given its associated Stream ID.
		/// </summary>
		/// <param name="StreamId">Stream ID</param>
		/// <param name="Node">Priority node, if found.</param>
		/// <returns>If a priority node was found with the corresponding ID.</returns>
		public bool TryGetPriorityNode(int StreamId, out PriorityNodeRfc7540 Node)
		{
			if (this.disposed)
			{
				Node = null;
				return false;
			}

			lock (this.synchObj)
			{
				if (StreamId != this.lastNodeStreamId)
				{
					if (!this.nodes.TryGetValue(StreamId, out this.lastNode))
					{
						this.lastNodeStreamId = 0;
						Node = null;
						return false;
					}

					this.lastNodeStreamId = StreamId;
				}

				Node = this.lastNode;
			}

			return true;
		}

		/// <summary>
		/// Tries to get a stream, given its associated Stream ID.
		/// </summary>
		/// <param name="StreamId">Stream ID</param>
		/// <param name="Stream">Stream object, if found.</param>
		/// <returns>If a stream object was found with the corresponding ID.</returns>
		public override bool TryGetStream(int StreamId, out Http2Stream Stream)
		{
			if (this.TryGetPriorityNode(StreamId, out PriorityNodeRfc7540 Node))
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

		/// <summary>
		/// Tries to add a stream to flow control for testing purposes, using
		/// default priority settings.
		/// </summary>
		/// <param name="StreamId">ID of stream to add.</param>
		/// <returns>If the stream could be added.</returns>
		public int AddStreamForTest(int StreamId)
		{
			return this.AddStreamForTest(StreamId, this.RemoteSettings, 16, 0, false);
		}

		/// <summary>
		/// Tries to add a stream to flow control for testing purposes, using
		/// default priority settings.
		/// </summary>
		/// <param name="StreamId">ID of stream to add.</param>
		/// <param name="Settings">Settings to use.</param>
		/// <returns>Size of window associated with stream. Negative = error</returns>
		public int AddStreamForTest(int StreamId, ConnectionSettings Settings)
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
		/// <returns>Size of window associated with stream. Negative = error</returns>
		public int AddStreamForTest(int StreamId, byte Weight, int StreamIdDependency, bool Exclusive)
		{
			return this.AddStreamForTest(StreamId, this.RemoteSettings, Weight, StreamIdDependency, Exclusive);
		}

		/// <summary>
		/// Tries to add a stream to flow control for testing purposes.
		/// </summary>
		/// <param name="StreamId">ID of stream to add.</param>
		/// <param name="Settings">Settings to use.</param>
		/// <param name="Weight">Weight</param>
		/// <param name="StreamIdDependency">ID of stream dependency, if any. 0 = root.</param>
		/// <param name="Exclusive">If the stream is exclusive child.</param>
		/// <returns>Size of window associated with stream. Negative = error</returns>
		public int AddStreamForTest(int StreamId, ConnectionSettings Settings,
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
		/// <returns>Size of window associated with stream. Negative = error</returns>
		public override int AddStream(Http2Stream Stream, byte Weight, int StreamIdDependency, bool Exclusive)
		{
			if (this.disposed)
				return -1;

			lock (this.synchObj)
			{
				if (this.nodes.Count >= this.RemoteSettings.MaxConcurrentStreams)
					return -1;

				if (!this.nodes.TryGetValue(StreamIdDependency, out PriorityNodeRfc7540 DependentOn))
				{
					if (StreamIdDependency == 0)
						DependentOn = this.root;
					else
						DependentOn = null;
				}

				PriorityNodeRfc7540 Parent = DependentOn ?? this.root;

				if (this.nodes.TryGetValue(Stream.StreamId, out PriorityNodeRfc7540 Node))
				{
					bool Add = false;

					if (!(Node.DependentOn is null) &&
						(Node.DependentOn != DependentOn || Exclusive))
					{
						Node.Parent.RemoveChildDependency(Node);
						Add = true;
					}

					if (Exclusive)
						this.MoveChildrenLocked(DependentOn, Node);

					if (Add)
						Parent.AddChildDependency(Node);

					Node.Weight = Weight;
				}
				else
				{
					if (this.hasProfiler)
					{
						Node = new PriorityNodeRfc7540(null, this.root, Stream, Weight, this,
							HttpClientConnection.CreateProfilerWindowThread(this.profiler, Stream.StreamId),
							HttpClientConnection.CreateProfilerDataThread(this.profiler, Stream.StreamId));
					}
					else
						Node = new PriorityNodeRfc7540(null, this.root, Stream, Weight, this, null, null);

					if (Exclusive && !(DependentOn is null))
						this.MoveChildrenLocked(DependentOn, Node);

					Parent.AddChildDependency(Node);
					this.nodes[Stream.StreamId] = Node;
				}

				return Node.AvailableResources;
			}
		}

		private void MoveChildrenLocked(PriorityNodeRfc7540 From, PriorityNodeRfc7540 To)
		{
			LinkedList<PriorityNodeRfc7540> Children = From?.MoveChildrenFrom();
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
					!this.nodes.TryGetValue(StreamIdDependency, out PriorityNodeRfc7540 DependentOn))
				{
					DependentOn = null;
				}

				PriorityNodeRfc7540 Parent = DependentOn ?? this.root;

				if (!this.nodes.TryGetValue(Stream.StreamId, out PriorityNodeRfc7540 Node))
					return false;

				bool Add = false;

				if (!(Node.DependentOn is null) &&
					(Node.DependentOn != DependentOn || Exclusive))
				{
					Node.Parent.RemoveChildDependency(Node);
					Add = true;
				}

				if (Exclusive)
					this.MoveChildrenLocked(DependentOn, Node);

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
		public override bool RemoveStream(Http2Stream Stream)
		{
			Stream.State = StreamState.Closed;
			return this.RemoveStream(Stream.StreamId);
		}

		/// <summary>
		/// Tries to remove a string from flow control.
		/// </summary>
		/// <param name="StreamId">ID of stream to remove.</param>
		/// <returns>If the stream could be found, and was removed.</returns>
		public override bool RemoveStream(int StreamId)
		{
			if (this.disposed)
				return false;

			lock (this.synchObj)
			{
				return this.RemoveStreamLocked(StreamId);
			}
		}

		private bool RemoveStreamLocked(int StreamId)
		{
			if (!this.nodes.TryGetValue(StreamId, out PriorityNodeRfc7540 Node))
				return false;

			this.nodes.Remove(StreamId);

			if (this.lastNodeStreamId == StreamId)
			{
				this.lastNodeStreamId = 0;
				this.lastNode = null;
			}

			PriorityNodeRfc7540 Parent = Node.Parent;
			if (!(Parent is null))
			{
				Parent.RemoveChildDependency(Node);

				double Scale = Parent.TotalChildWeights == 0 ? 1 : ((double)Node.Weight) / Parent.TotalChildWeights;

				while (Node.HasChildren)
				{
					PriorityNodeRfc7540 Child = Node.FirstChild.Value;
					int ScaledWeight = (int)Math.Ceiling(Child.Weight * Scale);
					if (ScaledWeight > 255)
						ScaledWeight = 255;

					if (Node.RemoveChildDependency(Child))
					{
						Child.Weight = (byte)ScaledWeight;
						Parent.AddChildDependency(Child);
					}
					else
						break;
				}
			}

			Node.Dispose();

			return true;
		}

		/// <summary>
		/// Requests resources for a stream.
		/// </summary>
		/// <param name="StreamId">ID of stream requesting resources.</param>
		/// <param name="RequestedResources">Amount of resources.</param>
		/// <param name="CancellationToken">Optional Cancellation token</param>
		/// <returns>Amount of resources granted. If negative, the stream is no
		/// longer controlled (i.e. it has been removed and/or closed).</returns>
		public override Task<int> RequestResources(int StreamId, int RequestedResources,
			CancellationToken? CancellationToken)
		{
			if (this.disposed)
				return Task.FromResult(-1);

			lock (this.synchObj)
			{
				if (StreamId != this.lastNodeStreamId)
				{
					if (!this.nodes.TryGetValue(StreamId, out this.lastNode))
					{
						this.lastNodeStreamId = 0;
						return Task.FromResult(-1);
					}

					this.lastNodeStreamId = StreamId;
				}

				return this.lastNode.RequestAvailableResources(RequestedResources, CancellationToken);
			}
		}

		/// <summary>
		/// Releases stream resources back to the stream.
		/// </summary>
		/// <param name="StreamId">ID of stream releasing resources.</param>
		/// <param name="Resources">Amount of resources released back</param>
		/// <returns>Size of current window. Negative = error</returns>
		public override int ReleaseStreamResources(int StreamId, int Resources)
		{
			if (this.disposed)
				return -1;

			lock (this.synchObj)
			{
				if (StreamId != this.lastNodeStreamId)
				{
					if (!this.nodes.TryGetValue(StreamId, out this.lastNode))
					{
						this.lastNodeStreamId = 0;
						return -1;
					}

					this.lastNodeStreamId = StreamId;
				}

				return this.lastNode.ReleaseStreamResources(Resources);
			}
		}

		/// <summary>
		/// Releases connection resources back.
		/// </summary>
		/// <param name="Resources">Amount of resources released back</param>
		/// <returns>Size of current window. Negative = error</returns>
		public override int ReleaseConnectionResources(int Resources)
		{
			if (this.disposed)
				return -1;

			lock (this.synchObj)
			{
				int Available = this.root.ReleaseConnectionResources(Resources);

				if (Available > this.ConnectionWindowSize)
					this.ConnectionWindowSize = Available;

				return Available;
			}
		}

		/// <summary>
		/// Disposes the object and terminates all tasks.
		/// </summary>
		public override void Dispose()
		{
			if (!this.disposed)
			{
				this.disposed = true;
				this.root?.Dispose();
			}
		}

		/// <summary>
		/// Connection is being terminated. Streams above <paramref name="LastPermittedStreamId"/>
		/// can be closed.
		/// </summary>
		/// <param name="LastPermittedStreamId">Last permitted stream ID.</param>
		public override void GoingAway(int LastPermittedStreamId)
		{
			lock (this.synchObj)
			{
				LinkedList<int> ToRemove = null;

				foreach (int StreamId in this.nodes.Keys)
				{
					if (StreamId > LastPermittedStreamId)
					{
						if (ToRemove is null)
							ToRemove = new LinkedList<int>();

						ToRemove.AddLast(StreamId);
					}
				}

				if (!(ToRemove is null))
				{
					foreach (int StreamId in ToRemove)
						this.RemoveStreamLocked(StreamId);
				}
			}
		}

		/// <summary>
		/// Sets the data label of a profiler thread, if available.
		/// </summary>
		/// <param name="StreamId">Stream ID</param>
		/// <param name="Label">Label to set.</param>
		public override void SetProfilerDataLabel(int StreamId, string Label)
		{
			if (this.TryGetPriorityNode(StreamId, out PriorityNodeRfc7540 Node))
			{
				ProfilerThread Thread = Node.DataThread;
				if (!(Thread is null))
					Thread.Label = Thread.Label + " (" + Label + ")";
			}
		}

	}
}