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
	internal class ClientChunkRecord : ChunkRecord
	{
		internal HttpxClient client;
		internal HttpxResponseEventArgs e;
		internal SortedDictionary<int, Chunk> chunks = null;
		internal HttpResponse response;
		internal HttpxResponseDataEventHandler dataCallback;
		internal object state;
		internal int nextChunk = 0;

		internal ClientChunkRecord(HttpxClient Client, HttpxResponseEventArgs e, HttpResponse Response, 
			HttpxResponseDataEventHandler DataCallback, object State)
			: base()
		{
			this.client = Client;
			this.e = e;
			this.response = Response;
			this.dataCallback = DataCallback;
			this.state = State;
		}

		internal override bool ChunkReceived(int Nr, bool Last, byte[] Data)
		{
			if (Nr == this.nextChunk)
			{
				try
				{
					this.dataCallback(this.client, new HttpxResponseDataEventArgs(null, Data, Last));
				}
				catch (Exception)
				{
					return false;
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
										this.dataCallback(this.client, new HttpxResponseDataEventArgs(null, Chunk.Data, Chunk.Last));
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
				if (this.chunks == null)
					this.chunks = new SortedDictionary<int, Chunk>();

				this.chunks[Nr] = new Chunk(Nr, Last, Data);
			}

			return true;
		}

		private void Done()
		{
			try
			{
				this.response.Dispose();
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		public override void Dispose()
		{
			if (this.response != null)
			{
				this.response.Dispose();
				this.response = null;
			}

			if (this.chunks != null)
			{
				this.chunks.Clear();
				this.chunks = null;
			}
		}
	}
}
