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
		internal TemporaryFile file;
		internal HttpResponse response;
		internal HttpxResponseEventHandler callback;
		internal object state;
		internal int nextChunk = 0;

		internal ClientChunkRecord(HttpxClient Client, HttpxResponseEventArgs e, HttpResponse Response, TemporaryFile File,
			HttpxResponseEventHandler Callback, object State)
			: base()
		{
			this.client = Client;
			this.e = e;
			this.response = Response;
			this.file = File;
			this.callback = Callback;
			this.state = State;
		}

		internal override void ChunkReceived(int Nr, bool Last, byte[] Data)
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
									return;
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
		}

		private void Done()
		{
			try
			{
				if (this.callback != null)
					this.callback(this.client, this.e);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
			finally
			{
				this.response.Dispose();
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
