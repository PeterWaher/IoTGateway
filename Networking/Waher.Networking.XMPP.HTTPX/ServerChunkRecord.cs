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
		internal IEndToEndEncryption e2e;
		internal SortedDictionary<int, Chunk> chunks = null;
		internal TemporaryFile file;
		internal string id;
		internal string from;
		internal string to;
		internal string e2eReference;
		internal int nextChunk = 0;
		internal int maxChunkSize;
		internal bool sipub;
		internal bool ibb;
		internal bool s5;
		internal bool jingle;

		internal ServerChunkRecord(HttpxServer Server, string Id, string From, string To, HttpRequest Request,
			IEndToEndEncryption E2e, string EndpointReference, TemporaryFile File, int MaxChunkSize, bool Sipub, bool Ibb,
			bool Socks5, bool Jingle)
			: base()
		{
			this.server = Server;
			this.id = Id;
			this.from = From;
			this.to = To;
			this.request = Request;
			this.e2e = E2e;
			this.e2eReference = EndpointReference;
			this.file = File;
			this.maxChunkSize = MaxChunkSize;
			this.sipub = Sipub;
			this.ibb = Ibb;
			this.s5 = Socks5;
			this.jingle = Jingle;
		}

		internal override bool ChunkReceived(int Nr, bool Last, byte[] Data)
		{
			if (Nr == this.nextChunk)
			{
				this.file?.Write(Data, 0, Data.Length);
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
									this.file?.Write(Chunk.Data, 0, Chunk.Data.Length);
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
				if (this.chunks is null)
					this.chunks = new SortedDictionary<int, Chunk>();

				this.chunks[Nr] = new Chunk(Nr, Last, Data);
			}

			return true;
		}

		private void Done()
		{
			this.server.Process(this.id, this.from, this.to, this.request, this.e2e, this.e2eReference, this.maxChunkSize,
				this.sipub, this.ibb, this.s5, this.jingle);
		}

		internal override void Fail(string Message)
		{
			this.file?.Dispose();
			this.file = null;
			this.Done();
		}

		public override void Dispose()
		{
			this.request?.Dispose();
			this.request = null;

			this.chunks?.Clear();
			this.chunks = null;
		}
	}
}
