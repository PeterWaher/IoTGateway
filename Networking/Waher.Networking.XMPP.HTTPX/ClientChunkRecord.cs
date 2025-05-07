using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking.HTTP;

namespace Waher.Networking.XMPP.HTTPX
{
	internal class ClientChunkRecord : ChunkRecord
	{
		private readonly HttpxClient client;
		private readonly HttpxResponseEventArgs e;
		private readonly object state;
		private readonly string streamId;
		private readonly string from;
		private readonly string to;
		private readonly string endpointReference;
		private readonly bool e2e;
		private readonly EventHandlerAsync<HttpxResponseDataEventArgs> dataCallback;
		private readonly IE2eSymmetricCipher symmetricCipher;
		private readonly SemaphoreSlim synchObj = new SemaphoreSlim(1);
		private SortedDictionary<int, Chunk> chunks = null;
		private HttpResponse response;
		private int nextChunk = 0;
		private bool disposed = false;

		internal ClientChunkRecord(HttpxClient Client, HttpxResponseEventArgs e, HttpResponse Response,
			EventHandlerAsync<HttpxResponseDataEventArgs> DataCallback, object State, string StreamId, string From, string To, bool E2e,
			string EndpointReference, IE2eSymmetricCipher SymmetricCipher)
			: base()
		{
			this.client = Client;
			this.e = e;
			this.response = Response;
			this.dataCallback = DataCallback;
			this.state = State;
			this.streamId = StreamId;
			this.from = From;
			this.to = To;
			this.e2e = E2e;
			this.endpointReference = EndpointReference;
			this.symmetricCipher = SymmetricCipher;
		}

		public string From => this.from;
		public string To => this.to;
		public bool E2e => this.e2e;
		public string EndpointReference => this.endpointReference;
		public IE2eSymmetricCipher SymmetricCipher => this.symmetricCipher;

		internal override async Task<bool> ChunkReceived(int Nr, bool Last, bool ConstantBuffer, byte[] Data)
		{
			if (this.disposed)
				throw new ObjectDisposedException(nameof(ClientChunkRecord));

			await this.synchObj.WaitAsync();
			try
			{
				if (Nr == this.nextChunk)
				{
					if (Data.Length > 0 || Last)
					{
						HttpxResponseDataEventArgs e = new HttpxResponseDataEventArgs(null, ConstantBuffer, Data, this.streamId, Last, this.state);
						if (!await this.dataCallback.Raise(this.client, e, false))
						{
							await this.client.CancelTransfer(this.e.From, this.streamId);
							return false;
						}
					}

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
										HttpxResponseDataEventArgs e = new HttpxResponseDataEventArgs(null, Chunk.ConstantBuffer, Chunk.Data, this.streamId, Chunk.Last, this.state);
										if (!await this.dataCallback.Raise(this.client, e, false))
											return false;

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
			if (!(this.response is null))
			{
				try
				{
					await this.response.DisposeAsync();
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			}

			this.chunks?.Clear();
		}

		internal override async Task Fail(string Message)
		{
			if (this.disposed)
				throw new ObjectDisposedException(nameof(ClientChunkRecord));

			await this.synchObj.WaitAsync();
			try
			{
				if (this.response is null)
					return;

				HttpxResponseDataEventArgs e = new HttpxResponseDataEventArgs(null, true, Array.Empty<byte>(), this.streamId, true, this.state);
				await this.dataCallback.Raise(this.client, e, false);

				if (!this.response.HeaderSent)
					await this.response.SendResponse(new InternalServerErrorException(Message));

				await this.client.CancelTransfer(this.e.From, this.streamId);

				await this.DoneLocked();
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
					if (!(this.response is null))
					{
						await this.response.DisposeAsync();
						this.response = null;
					}

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
