using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Waher.Runtime.Profiling;

namespace Waher.Networking.HTTP.HTTP2
{
	/// <summary>
	/// Class that manages HTTP/2 flow control using simplified absolute priorities,
	/// as defined in RFC 9218.
	/// </summary>
	public class FlowControlRfc9218 : FlowControlConnection
	{
		private const int priorityLevels = 8;

		private readonly LinkedList<PriorityNodeRfc9218>[] priorities = new LinkedList<PriorityNodeRfc9218>[priorityLevels];
		private readonly Dictionary<int, StreamRec> streams = new Dictionary<int, StreamRec>();
		private readonly PriorityNodeRfc9218 root;
		private readonly Profiler profiler;
		private readonly object synchObj = new object();
		private int lastNodeStreamId = -1;
		private int lastRemoteStreamWindowSize = 0;
		private StreamRec lastRec = null;
		private bool disposed = false;

		private class StreamRec
		{
			public Http2Stream Stream;
			public PriorityNodeRfc9218 Node;
			public int Priority;
		}

		/// <summary>
		/// Class that manages HTTP/2 flow control using trees of priorty nodes.
		/// </summary>
		/// <param name="LocalSettings">Local Connection settings.</param>
		/// <param name="RemoteSettings">Remote Connection settings.</param>
		/// <param name="Profiler">Connection profiler.</param>
		public FlowControlRfc9218(ConnectionSettings LocalSettings, ConnectionSettings RemoteSettings,
			Profiler Profiler)
			: this(LocalSettings, RemoteSettings, null, Profiler)
		{
		}

		/// <summary>
		/// Class that manages HTTP/2 flow control using trees of priorty nodes.
		/// </summary>
		/// <param name="LocalSettings">Local Connection settings.</param>
		/// <param name="RemoteSettings">Remote Connection settings.</param>
		/// <param name="Connection">HTTP/2 connection object.</param>
		/// <param name="Profiler">Connection profiler.</param>
		internal FlowControlRfc9218(ConnectionSettings LocalSettings, ConnectionSettings RemoteSettings,
			HttpClientConnection Connection, Profiler Profiler)
			: base(LocalSettings, RemoteSettings, Connection)
		{
			this.profiler = Profiler;
			this.root = new PriorityNodeRfc9218(null, this, this.profiler);
			this.root.CheckProfilerThreads();

			this.lastRemoteStreamWindowSize = this.RemoteSettings.InitialStreamWindowSize;
		}

		/// <summary>
		/// Root node.
		/// </summary>
		public PriorityNodeRfc9218 Root => this.root;

		/// <summary>
		/// Called when the remote connection settings have been updated.
		/// </summary>
		public override void RemoteSettingsUpdated()
		{
			int Size = this.RemoteSettings.InitialStreamWindowSize;

			if (this.lastRemoteStreamWindowSize != Size)
			{
				this.lastRemoteStreamWindowSize = Size;

				lock (this.synchObj)
				{
					this.root.SetNewWindowSize(this.root.OutputWindowSize, Size, false);

					foreach (LinkedList<PriorityNodeRfc9218> Nodes in this.priorities)
					{
						if (!(Nodes is null))
						{
							foreach (PriorityNodeRfc9218 Node in Nodes)
								Node.SetNewWindowSize(this.root.OutputWindowSize, Size, true);
						}
					}
				}
			}
		}

		/// <summary>
		/// Tries to get a priority node, given its associated Stream ID.
		/// </summary>
		/// <param name="StreamId">Stream ID</param>
		/// <param name="Node">Priority node, if found.</param>
		/// <returns>If a priority node was found with the corresponding ID.</returns>
		public bool TryGetPriorityNode(int StreamId, out PriorityNodeRfc9218 Node)
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
					if (!this.streams.TryGetValue(StreamId, out this.lastRec))
					{
						this.lastNodeStreamId = 0;
						Node = null;
						return false;
					}

					this.lastNodeStreamId = StreamId;
				}

				Node = this.lastRec.Node;
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
			if (this.disposed)
			{
				Stream = null;
				return false;
			}

			lock (this.synchObj)
			{
				if (StreamId != this.lastNodeStreamId)
				{
					if (!this.streams.TryGetValue(StreamId, out this.lastRec))
					{
						this.lastNodeStreamId = 0;
						Stream = null;
						return false;
					}

					this.lastNodeStreamId = StreamId;
				}

				Stream = this.lastRec.Stream;
			}

			return true;
		}

		/// <summary>
		/// Tries to add a stream to flow control for testing purposes, using
		/// default priority settings.
		/// </summary>
		/// <param name="StreamId">ID of stream to add.</param>
		/// <param name="Rfc9218Priority">Priority, as defined by RFC 9218.</param>
		/// <param name="Rfc9218Incremental">If stream is incremental</param>
		/// <returns>Size of window associated with stream. Negative = error</returns>
		public int AddStreamForTest(int StreamId, int Rfc9218Priority, bool Rfc9218Incremental)
		{
			return this.AddStreamForTest(StreamId, this.LocalSettings, Rfc9218Priority, 
				Rfc9218Incremental);
		}

		/// <summary>
		/// Tries to add a stream to flow control for testing purposes, using
		/// default priority settings.
		/// </summary>
		/// <param name="StreamId">ID of stream to add.</param>
		/// <param name="LocalSettings">Local Settings to use.</param>
		/// <returns>Size of window associated with stream. Negative = error</returns>
		public int AddStreamForTest(int StreamId, ConnectionSettings LocalSettings)
		{
			return this.AddStreamForTest(StreamId, LocalSettings, 3, false);
		}

		/// <summary>
		/// Tries to add a stream to flow control for testing purposes.
		/// </summary>
		/// <param name="StreamId">ID of stream to add.</param>
		/// <param name="LocalSettings">Local Settings to use.</param>
		/// <param name="Rfc9218Priority">Priority, as defined by RFC 9218.</param>
		/// <param name="Rfc9218Incremental">If stream is incremental</param>
		/// <returns>Size of window associated with stream. Negative = error</returns>
		public int AddStreamForTest(int StreamId, ConnectionSettings LocalSettings,
			int Rfc9218Priority, bool Rfc9218Incremental)
		{
			Http2Stream Stream = new Http2Stream(StreamId, LocalSettings)
			{
				Rfc9218Priority = Rfc9218Priority,
				Rfc9218Incremental = Rfc9218Incremental
			};

			return this.AddStream(Stream, 0, 0, false);
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
				if (this.streams.Count >= this.RemoteSettings.MaxConcurrentStreams)
					return -1;

				int Priority = Stream.Rfc9218Priority;
				if (Priority < 0 || Priority >= priorityLevels)
					Priority = 3;

				PriorityNodeRfc9218 Node = new PriorityNodeRfc9218(Stream, this, this.profiler);

				StreamRec Rec = new StreamRec()
				{
					Stream = Stream,
					Node = Node,
					Priority = Priority
				};

				this.streams[Stream.StreamId] = Rec;

				LinkedList<PriorityNodeRfc9218> Nodes = this.priorities[Priority];
				if (Nodes is null)
				{
					Nodes = new LinkedList<PriorityNodeRfc9218>();
					this.priorities[Priority] = Nodes;
				}

				Nodes.AddLast(Rec.Node);
				Node.CheckProfilerThreads();

				return Rec.Node.AvailableResources;
			}
		}

		/// <summary>
		/// Updates the priority of a stream in the flow control.
		/// </summary>
		/// <param name="Stream">Stream to update.</param>
		/// <param name="Rfc9218Priority">Priority, as defined by RFC 9218.</param>
		/// <param name="Rfc9218Incremental">If stream is incremental</param>
		/// <returns>If the stream could be updated.</returns>
		public bool UpdatePriority(Http2Stream Stream, int? Rfc9218Priority, bool? Rfc9218Incremental)
		{
			if (this.disposed)
				return false;

			lock (this.synchObj)
			{
				if (!this.streams.TryGetValue(Stream.StreamId, out StreamRec Rec))
					return false;

				if (Rfc9218Incremental.HasValue)
					Rec.Stream.Rfc9218Incremental = Rfc9218Incremental.Value;

				if (Rfc9218Priority.HasValue)
				{
					int Priority = Rfc9218Priority.Value;
					if (Priority < 0 || Priority >= priorityLevels)
						Priority = 3;

					if (Priority != Rec.Priority)
					{
						LinkedList<PriorityNodeRfc9218> Nodes = this.priorities[Rec.Priority];
						Nodes.Remove(Rec.Node);

						Rec.Priority = Priority;
						Nodes = this.priorities[Priority];
						if (Nodes is null)
						{
							Nodes = new LinkedList<PriorityNodeRfc9218>();
							this.priorities[Priority] = Nodes;
						}

						Nodes.AddLast(Rec.Node);
					}
				}

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
				if (!this.RemoveStreamLocked(StreamId))
					return false;
			}

			this.StreamRemoved(StreamId);

			return true;
		}

		private bool RemoveStreamLocked(int StreamId)
		{
			if (this.streams.TryGetValue(StreamId, out StreamRec Rec))
			{
				Rec.Stream.State = StreamState.Closed;
				this.streams.Remove(StreamId);

				LinkedList<PriorityNodeRfc9218> Nodes = this.priorities[Rec.Priority];
				Nodes?.Remove(Rec.Node);

				if (this.lastNodeStreamId == StreamId)
				{
					this.lastNodeStreamId = 0;
					this.lastRec = null;
				}

				Rec.Node.Dispose();

				return true;
			}
			else
				return false;
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
					if (!this.streams.TryGetValue(StreamId, out this.lastRec))
					{
						this.lastNodeStreamId = 0;
						return Task.FromResult(-1);
					}

					this.lastNodeStreamId = StreamId;
				}

				return this.lastRec.Node.RequestAvailableResources(RequestedResources, CancellationToken);
			}
		}

		/// <summary>
		/// Releases stream resources back to the stream, as a result of a client sending a
		/// WINDOW_UPDATE frame with Stream ID > 0.
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
					if (!this.streams.TryGetValue(StreamId, out this.lastRec))
					{
						this.lastNodeStreamId = 0;
						return -1;
					}

					this.lastNodeStreamId = StreamId;
				}

				return this.lastRec.Node.ReleaseStreamResources(Resources);
			}
		}

		/// <summary>
		/// Releases connection resources back, as a result of a client sending a
		/// WINDOW_UPDATE frame with Stream ID = 0.
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

				int Left = Available;
				for (int i = 0; i < priorityLevels; i++)
				{
					LinkedList<PriorityNodeRfc9218> Queue = this.priorities[i];
					if (!(Queue is null))
					{
						foreach (PriorityNodeRfc9218 Node in Queue)
						{
							Node.TriggerPending(ref Left);
							if (Left <= 0)
								break;
						}

						if (Left <= 0)
							break;
					}
				}

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

				foreach (LinkedList<PriorityNodeRfc9218> Nodes in this.priorities)
				{
					if (Nodes is null)
						continue;

					foreach (PriorityNodeRfc9218 Node in Nodes)
						Node.Dispose();
				}
			}
		}

		/// <summary>
		/// Connection is being terminated. Streams above <paramref name="LastPermittedStreamId"/>
		/// can be closed.
		/// </summary>
		/// <param name="LastPermittedStreamId">Last permitted stream ID.</param>
		public override void GoingAway(int LastPermittedStreamId)
		{
			LinkedList<int> ToRemove = null;

			lock (this.synchObj)
			{
				foreach (int StreamId in this.streams.Keys)
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

			if (!(ToRemove is null))
			{
				foreach (int StreamId in ToRemove)
					this.StreamRemoved(StreamId);
			}
		}

		/// <summary>
		/// Sets the stream label of a profiler thread, if available.
		/// </summary>
		/// <param name="StreamId">Stream ID</param>
		/// <param name="Label">Label to set.</param>
		public override void SetProfilerStreamLabel(int StreamId, string Label)
		{
			if (this.TryGetPriorityNode(StreamId, out PriorityNodeRfc9218 Node))
			{
				Node.CheckProfilerThreads();

				ProfilerThread Thread = Node.Stream.StreamThread;
				if (!(Thread is null))
					Thread.Label = Thread.Label + " (" + Label + ")";
			}
		}

		/// <summary>
		/// Gets an enumerator of available priority nodes.
		/// </summary>
		/// <returns>Enumerator</returns>
		public override IEnumerator<IPriorityNode> GetEnumerator()
		{
			return new PriorityNodeEnumerator(this);
		}

		private class PriorityNodeEnumerator : IEnumerator<IPriorityNode> 
		{
			private readonly FlowControlRfc9218 flowControl;
			private LinkedListNode<PriorityNodeRfc9218> currentNode = null;
			private int index = -1;

			internal PriorityNodeEnumerator(FlowControlRfc9218 FlowControl)
			{
				this.flowControl = FlowControl;
			}

			public IPriorityNode Current
			{
				get
				{
					if (this.currentNode is null)
						throw new System.InvalidOperationException();
					else
						return this.currentNode.Value;
				}
			}

			object IEnumerator.Current => this.Current;

			public void Dispose()
			{
				this.Reset();
			}

			public bool MoveNext()
			{
				if (!(this.currentNode is null))
				{
					this.currentNode = this.currentNode.Next;
					if (!(this.currentNode is null))
						return true;
				}

				if (this.index >= priorityLevels)
					return false;

				do
				{
					this.index++;
					if (this.index >= priorityLevels)
						return false;

					this.currentNode = this.flowControl.priorities[this.index]?.First;
				}
				while (this.currentNode is null);

				return true;
			}

			public void Reset()
			{
				this.index = -1;
				this.currentNode = null;
			}
		}

		/// <summary>
		/// Exports a PlantUML header.
		/// </summary>
		/// <param name="Output">UML diagram will be exported here.</param>
		protected override void ExportPlantUmlHeader(StringBuilder Output)
		{
			base.ExportPlantUmlHeader(Output);

			this.root?.ExportPlantUml(Output);
		}

	}
}