using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Events;
using Waher.Networking.HTTP;

namespace Waher.Networking.XMPP.HTTPX
{
	internal class ServerChunkRecord : ChunkRecord
	{
		internal HttpxServer server;
		internal HttpRequest request;
		internal SortedDictionary<int, Chunk> chunks = null;
		internal TemporaryFile file;
		internal string id;
		internal string from;
		internal string to;
		internal int nextChunk = 0;
		internal int maxChunkSize;
		internal bool sipub;
		internal bool ibb;
		internal bool jingle;

		internal ServerChunkRecord(HttpxServer Server, string Id, string From, string To, HttpRequest Request, TemporaryFile File, int MaxChunkSize,
			bool Sipub, bool Ibb, bool Jingle)
			: base()
		{
			this.server = Server;
			this.id = Id;
			this.from = From;
			this.to = To;
			this.request = Request;
			this.file = File;
			this.maxChunkSize = MaxChunkSize;
			this.sipub = Sipub;
			this.ibb = Ibb;
			this.jingle = Jingle;
		}

		internal override bool ChunkReceived(int Nr, bool Last, byte[] Data)
		{
			if (Nr == this.nextChunk)
			{
				this.file.Write(Data, 0, Data.Length);
				this.nextChunk++;

				if (Last)
					this.Done();
				else
				{
					while (this.chunks != null)
					{
						if (this.chunks.Count == 0)
							this.chunks = null;
						else
						{
							foreach (Chunk Chunk in this.chunks.Values)
							{
								if (Chunk.Nr == this.nextChunk)
								{
									this.file.Write(Chunk.Data, 0, Chunk.Data.Length);
									this.nextChunk++;
									this.chunks.Remove(Chunk.Nr);

									if (Chunk.Last)
									{
										this.Done();
										this.chunks.Clear();
									}

									break;
								}
								else
									return true;
							}
						}
					}
				}
			}
			else if (Nr > this.nextChunk)
			{
				if (this.chunks == null)
					this.chunks = new SortedDictionary<int, Chunk>();

				this.chunks[Nr] = new Chunk(Nr, Last, Data);
			}

			return true;
		}

		private void Done()
		{
			this.server.Process(this.id, this.from, this.to, this.request, this.maxChunkSize, this.sipub, this.ibb, this.jingle);
		}

		public override void Dispose()
		{
			if (this.request != null)
			{
				this.request.Dispose();
				this.request = null;
			}

			if (this.chunks != null)
			{
				this.chunks.Clear();
				this.chunks = null;
			}
		}
	}
}
