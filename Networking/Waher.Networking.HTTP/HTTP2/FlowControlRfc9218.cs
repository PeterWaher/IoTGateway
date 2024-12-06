using System.Collections.Generic;
using System.Threading.Tasks;

namespace Waher.Networking.HTTP.HTTP2
{
	/// <summary>
	/// Class that manages HTTP/2 flow control using simplified absolute priorities,
	/// as defined in RFC 9218.
	/// </summary>
	public class FlowControlRfc9218 : IFlowControl
	{
		private readonly LinkedList<PriorityNode>[] priorities = new LinkedList<PriorityNode>[8];
		private readonly Dictionary<int, StreamRec> streams = new Dictionary<int, StreamRec>();
		private readonly PriorityNode root;
		private readonly object synchObj = new object();
		private ConnectionSettings settings;
		private int lastNodeStreamId = -1;
		private StreamRec lastRec = null;
		private bool disposed = false;

		private class StreamRec
		{
			public Http2Stream Stream;
			public PriorityNode Node;
			public int Priority;
		}

		/// <summary>
		/// Class that manages HTTP/2 flow control using trees of priorty nodes.
		/// </summary>
		/// <param name="Settings">Connection settings.</param>
		public FlowControlRfc9218(ConnectionSettings Settings)
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
		/// <param name="Settings">Connection settings.</param>
		public void UpdateSettings(ConnectionSettings Settings)
		{
			this.settings = Settings;
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
				if (this.streams.TryGetValue(StreamId, out StreamRec Rec))
				{
					Stream = Rec.Stream;
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
				if (this.streams.Count >= this.settings.MaxConcurrentStreams)
					return false;

				int Priority = Stream.Rfc9218Priority;
				if (Priority < 0 || Priority > 7)
					Priority = 3;

				StreamRec Rec = new StreamRec()
				{
					Stream = Stream,
					Node = new PriorityNode(null, this.root, Stream, Weight, this),
					Priority = Priority
				};

				this.streams[Stream.StreamId] = Rec;

				LinkedList<PriorityNode> Nodes = this.priorities[Priority];
				if (Nodes is null)
				{
					Nodes = new LinkedList<PriorityNode>();
					this.priorities[Priority] = Nodes;
				}

				Nodes.AddLast(Rec.Node);

				return true;
			}
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
				if (!this.streams.TryGetValue(Stream.StreamId, out StreamRec Rec))
					return false;

				int Priority = Stream.Rfc9218Priority;
				if (Priority < 0 || Priority > 7)
					Priority = 3;

				if (Priority == Rec.Priority)
					return true;

				LinkedList<PriorityNode> Nodes = this.priorities[Rec.Priority];
				Nodes.Remove(Rec.Node);

				Rec.Priority = Priority;
				Nodes = this.priorities[Priority];
				if (Nodes is null)
				{
					Nodes = new LinkedList<PriorityNode>();
					this.priorities[Priority] = Nodes;
				}

				Nodes.AddLast(Rec.Node);

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
				if (this.streams.TryGetValue(StreamId, out StreamRec Rec))
				{
					Rec.Stream.State = StreamState.Closed;
					this.streams.Remove(StreamId);

					LinkedList<PriorityNode> Nodes = this.priorities[Rec.Priority];
					Nodes.Remove(Rec.Node);

					if (this.lastNodeStreamId == StreamId)
					{
						this.lastNodeStreamId = 0;
						this.lastRec = null;
					}

					return true;
				}
				else
					return false;
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
			if (this.disposed)
				return Task.FromResult(-1);

			lock (this.synchObj)
			{
				if (StreamId != this.lastNodeStreamId)
				{
					if (!this.streams.TryGetValue(StreamId, out this.lastRec))
						return Task.FromResult(-1);
				}

				return this.lastRec.Node.RequestAvailableResources(RequestedResources);
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
					if (!this.streams.TryGetValue(StreamId, out this.lastRec))
						return false;
				}

				return this.lastRec.Node.ReleaseStreamResources(Resources);
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
