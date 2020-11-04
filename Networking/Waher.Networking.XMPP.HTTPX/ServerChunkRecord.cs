using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Networking.HTTP;
using Waher.Runtime.Temporary;

namespace Waher.Networking.XMPP.HTTPX
{
	internal class ServerChunkRecord : ChunkRecord
	{
		internal HttpxServer server;
		internal HttpRequest request;
		internal IEndToEndEncryption e2e;
		internal SortedDictionary<int, Chunk> chunks = null;
		internal TemporaryStream file;
		internal string id;
		internal string from;
		internal string to;
		internal string e2eReference;
		internal string postResource;
		internal int nextChunk = 0;
		internal int maxChunkSize;
		internal bool sipub;
		internal bool ibb;
		internal bool s5;
		internal bool jingle;

		internal ServerChunkRecord(HttpxServer Server, string Id, string From, string To, HttpRequest Request,
			IEndToEndEncryption E2e, string EndpointReference, TemporaryStream File, int MaxChunkSize, bool Sipub, bool Ibb,
			bool Socks5, bool Jingle, string PostResource)
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
			this.postResource = PostResource;
		}

		internal override async Task<bool> ChunkReceived(int Nr, bool Last, byte[] Data)
		{
			if (Nr == this.nextChunk)
			{
				this.file?.Write(Data, 0, Data.Length);
				this.nextChunk++;

				if (Last)
					await this.Done();
				else
				{
					while (!(this.chunks is null))
					{
						if (this.chunks.Count == 0)
							this.chunks = null;
						else
						{
							foreach (Chunk Chunk in this.chunks.Values)
							{
								if (Chunk.Nr == this.nextChunk)
								{
									await this.file?.WriteAsync(Chunk.Data, 0, Chunk.Data.Length);
									this.nextChunk++;
									this.chunks.Remove(Chunk.Nr);

									if (Chunk.Last)
									{
										await this.Done();
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

		private Task Done()
		{
			if (!(this.file is null))
			{
				this.file.Position = 0;
				this.file = null;
			}

			return this.server.Process(this.id, this.from, this.to, this.request, this.e2e, this.e2eReference, this.maxChunkSize,
				this.postResource, this.ibb, this.s5);
		}

		internal override Task Fail(string Message)
		{
			this.file?.Dispose();
			this.file = null;

			return Task.CompletedTask;
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
