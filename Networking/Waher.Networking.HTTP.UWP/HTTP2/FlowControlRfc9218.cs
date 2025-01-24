using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Waher.Networking.HTTP.HTTP2
{
	/// <summary>
	/// Class that manages HTTP/2 flow control using simplified absolute priorities,
	/// as defined in RFC 9218.
	/// </summary>
	public class FlowControlRfc9218 : FlowControlConnection
	{
		private readonly LinkedList<PriorityNodeRfc9218>[] priorities = new LinkedList<PriorityNodeRfc9218>[8];
		private readonly Dictionary<int, StreamRec> streams = new Dictionary<int, StreamRec>();
		private readonly PriorityNodeRfc9218 root;
		private readonly object synchObj = new object();
		private int lastNodeStreamId = -1;
		private int lastRemoteInitialWindowSize = 0;
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
		public FlowControlRfc9218(ConnectionSettings LocalSettings, ConnectionSettings RemoteSettings)
			: base(LocalSettings, RemoteSettings)
		{
			this.root = new PriorityNodeRfc9218(null, this);
			this.lastRemoteInitialWindowSize = this.RemoteSettings.InitialWindowSize;
		}

		/// <summary>
		/// Root node.
		/// </summary>
		public PriorityNodeRfc9218 Root => this.root;

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
					this.root.SetNewWindowSize(this.LocalSettings.InitialWindowSize, Size, false);

					foreach (LinkedList<PriorityNodeRfc9218> Nodes in this.priorities)
					{
						if (!(Nodes is null))
						{
							foreach (PriorityNodeRfc9218 Node in Nodes)
								Node.SetNewWindowSize(this.LocalSettings.InitialWindowSize, Size, true);
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
				if (this.streams.TryGetValue(StreamId, out StreamRec Rec))
				{
					Node = Rec.Node;
					return true;
				}
				else
				{
					Node = null;
					return false;
				}
			}
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
		/// <param name="Rfc9218Priority">Priority, as defined by RFC 9218.</param>
		/// <param name="Rfc9218Incremental">If stream is incremental</param>
		/// <returns>Size of window associated with stream. Negative = error</returns>
		public int AddStreamForTest(int StreamId, int Rfc9218Priority, bool Rfc9218Incremental)
		{
			return this.AddStreamForTest(StreamId, this.RemoteSettings, Rfc9218Priority, Rfc9218Incremental);
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
			return this.AddStreamForTest(StreamId, Settings, 3, false);
		}

		/// <summary>
		/// Tries to add a stream to flow control for testing purposes.
		/// </summary>
		/// <param name="StreamId">ID of stream to add.</param>
		/// <param name="Settings">Settings to use.</param>
		/// <param name="Rfc9218Priority">Priority, as defined by RFC 9218.</param>
		/// <param name="Rfc9218Incremental">If stream is incremental</param>
		/// <returns>Size of window associated with stream. Negative = error</returns>
		public int AddStreamForTest(int StreamId, ConnectionSettings Settings,
			int Rfc9218Priority, bool Rfc9218Incremental)
		{
			Http2Stream Stream = new Http2Stream(StreamId, Settings)
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
				if (Priority < 0 || Priority > 7)
					Priority = 3;

				StreamRec Rec = new StreamRec()
				{
					Stream = Stream,
					Node = new PriorityNodeRfc9218(Stream, this),
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
					if (Priority < 0 || Priority > 7)
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

				int Left = Available;
				for (int i = 0; i < 8; i++)
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
			lock (this.synchObj)
			{
				LinkedList<int> ToRemove = null;

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
		}

	}
}
