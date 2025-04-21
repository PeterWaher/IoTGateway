using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Waher.Networking.HTTP;
using Waher.Runtime.Temporary;

namespace Waher.Networking.XMPP.HTTPX
{
	internal class ServerChunkRecord : ChunkRecord
	{
		private readonly HttpxServer server;
		private readonly IEndToEndEncryption e2e;
		private readonly string id;
		private readonly string from;
		private readonly string to;
		private readonly string e2eReference;
		private readonly string postResource;
		private readonly int maxChunkSize;
		private readonly bool sipub;
		private readonly bool ibb;
		private readonly bool s5;
		private readonly bool jingle;
		private readonly SemaphoreSlim synchObj = new SemaphoreSlim(1);
		private HttpRequest request;
		private SortedDictionary<int, Chunk> chunks = null;
		private TemporaryStream file;
		private int nextChunk = 0;
		private bool disposed = false;

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

		internal override async Task<bool> ChunkReceived(int Nr, bool Last, bool ConstantBuffer, byte[] Data)
		{
			if (this.disposed)
				throw new ObjectDisposedException(nameof(ClientChunkRecord));

			await this.synchObj.WaitAsync();
			try
			{
				if (Nr == this.nextChunk)
				{
					this.file?.Write(Data, 0, Data.Length);
					this.nextChunk++;

					if (Last)
						await this.DoneLocked();
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
											await this.DoneLocked();

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
					this.chunks ??= new SortedDictionary<int, Chunk>();
					this.chunks[Nr] = new Chunk(Nr, Last, ConstantBuffer, Data);
				}

				return true;
			}
			finally
			{
				this.synchObj.Release();
			}
		}

		private async Task DoneLocked()
		{
			if (!(this.file is null))
			{
				this.file.Position = 0;
				this.file = null;
			}

			await this.server.Process(this.id, this.from, this.to, this.request, this.e2e, this.e2eReference, this.maxChunkSize,
				this.postResource, this.ibb, this.s5);
		
			this.chunks?.Clear();
		}

		internal override async Task Fail(string Message)
		{
			if (this.disposed)
				throw new ObjectDisposedException(nameof(ClientChunkRecord));

			await this.synchObj.WaitAsync();
			try
			{
				this.file?.Dispose();
				this.file = null;
			}
			finally
			{
				this.synchObj.Release();
			}
		}

		public override async Task DisposeAsync()
		{
			if (!this.disposed)
			{
				this.disposed = true;

				await this.synchObj.WaitAsync();
				try
				{
					this.request?.Dispose();
					this.request = null;

					this.chunks?.Clear();
					this.chunks = null;
				}
				finally
				{
					this.synchObj.Dispose();
				}
			}
		}
	}
}
