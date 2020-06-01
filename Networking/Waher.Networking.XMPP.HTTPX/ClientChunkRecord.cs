using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Events;
using Waher.Networking.HTTP;
using Waher.Networking.HTTP.ScriptExtensions.Functions.ServerErrors;

namespace Waher.Networking.XMPP.HTTPX
{
	internal class ClientChunkRecord : ChunkRecord
	{
		internal HttpxClient client;
		internal HttpxResponseEventArgs e;
		internal SortedDictionary<int, Chunk> chunks = null;
		internal HttpResponse response;
		internal HttpxResponseDataEventHandler dataCallback;
		internal IE2eSymmetricCipher symmetricCipher;
		internal object state;
		internal string streamId;
		internal string from;
		internal string to;
		internal string endpointReference;
		internal int nextChunk = 0;
		internal bool e2e;

		internal ClientChunkRecord(HttpxClient Client, HttpxResponseEventArgs e, HttpResponse Response,
			HttpxResponseDataEventHandler DataCallback, object State, string StreamId, string From, string To, bool E2e,
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

		internal override bool ChunkReceived(int Nr, bool Last, byte[] Data)
		{
			if (Nr == this.nextChunk)
			{
				if (Data.Length > 0 || Last)
				{
					try
					{
						this.dataCallback(this.client, new HttpxResponseDataEventArgs(null, Data, this.streamId, Last, this.state));
					}
					catch (Exception)
					{
						this.client.CancelTransfer(this.e.From, this.streamId);
						return false;
					}
				}

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
									try
									{
										this.dataCallback(this.client, new HttpxResponseDataEventArgs(null, Chunk.Data, this.streamId, Chunk.Last, this.state));
									}
									catch (Exception)
									{
										return false;
									}

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
			try
			{
				this.response?.Dispose();
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		internal override void Fail(string Message)
		{
			if (this.response is null)
				return;

			try
			{
				this.dataCallback(this.client, new HttpxResponseDataEventArgs(null, new byte[0], this.streamId, true, this.state));
			}
			catch (Exception)
			{
				// Ignore
			}

			if (!this.response.HeaderSent)
				this.response.SendResponse(new InternalServerErrorException(Message));

			this.client.CancelTransfer(this.e.From, this.streamId);

			this.Done();
		}

		public override void Dispose()
		{
			this.response?.Dispose();
			this.response = null;

			this.chunks?.Clear();
			this.chunks = null;
		}
	}
}
