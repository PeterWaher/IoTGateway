using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Networking.HTTP;

namespace Waher.Networking.XMPP.HTTPX
{
	internal class HttpxResponse : TransferEncoding
	{
		private readonly static Dictionary<string, HttpxResponse> activeStreams = new Dictionary<string, HttpxResponse>();

		private StringBuilder response = new StringBuilder();
		private readonly XmppClient client;
		private readonly IEndToEndEncryption e2e;
		private readonly InBandBytestreams.IbbClient ibbClient;
		private readonly P2P.SOCKS5.Socks5Proxy socks5Proxy;
		private readonly string id;
		private readonly string to;
		private readonly string from;
		private readonly int maxChunkSize;
		private bool? chunked = null;
		private int nr = 0;
		private string streamId = null;
		private byte[] chunk = null;
		private InBandBytestreams.OutgoingStream ibbOutput = null;
		private P2P.SOCKS5.OutgoingStream socks5Output = null;
		private int pos;
		private bool cancelled = false;
		private byte[] tail = null;

		public HttpxResponse(XmppClient Client, IEndToEndEncryption E2e, string Id, string To, string From, int MaxChunkSize,
			InBandBytestreams.IbbClient IbbClient, P2P.SOCKS5.Socks5Proxy Socks5Proxy) : base()
		{
			this.client = Client;
			this.e2e = E2e;
			this.ibbClient = IbbClient;
			this.socks5Proxy = Socks5Proxy;
			this.id = Id;
			this.to = To;
			this.from = From;
			this.maxChunkSize = MaxChunkSize;
		}

		private void AssertNotCancelled()
		{
			if (this.cancelled)
				throw new IOException("Stream cancelled.");
		}

		public override async Task BeforeContentAsync(HttpResponse Response, bool ExpectContent)
		{
			this.response.Append("<resp xmlns='");
			this.response.Append(HttpxClient.Namespace);
			this.response.Append("' version='1.1' statusCode='");
			this.response.Append(Response.StatusCode.ToString());
			this.response.Append("' statusMessage='");
			this.response.Append(XML.Encode(Response.StatusMessage));
			this.response.Append("'><headers xmlns='");
			this.response.Append(HttpxClient.NamespaceHeaders);
			this.response.Append("'>");

			if (Response.ContentLength.HasValue)
				this.chunked = (Response.ContentLength.Value > this.maxChunkSize);
			else if (ExpectContent)
				this.chunked = true;
			else
			{
				if ((Response.StatusCode < 100 || Response.StatusCode > 199) && Response.StatusCode != 204 && Response.StatusCode != 304)
					this.response.Append("<header name='Content-Length'>0</header>");
			}

			foreach (KeyValuePair<string, string> P in Response.GetHeaders())
			{
				this.response.Append("<header name='");
				this.response.Append(XML.Encode(P.Key));
				this.response.Append("'>");
				this.response.Append(XML.Encode(P.Value));
				this.response.Append("</header>");
			}

			this.response.Append("</headers>");

			if (this.chunked.HasValue)
			{
				if (this.chunked.Value)
				{
					this.streamId = Guid.NewGuid().ToString().Replace("-", string.Empty);

					if (this.socks5Proxy != null)
					{
						this.response.Append("<data><s5 sid='");
						this.response.Append(this.streamId);
						this.response.Append("' e2e='");
						this.response.Append(CommonTypes.Encode(this.e2e != null));
						this.response.Append("'/></data>");
						this.ReturnResponse();

						this.socks5Output = new P2P.SOCKS5.OutgoingStream(this.streamId, this.from, this.to, 49152, this.e2e);
						this.socks5Output.OnAbort += this.OnAbort;

						await this.socks5Proxy.InitiateSession(this.to, this.streamId, this.InitiationCallback, null);
					}
					else if (this.ibbClient != null)
					{
						this.response.Append("<data><ibb sid='");
						this.response.Append(this.streamId);
						this.response.Append("'/></data>");
						this.ReturnResponse();

						this.ibbOutput = this.ibbClient.OpenStream(this.to, this.maxChunkSize, this.streamId, this.e2e);
						this.ibbOutput.OnAbort += this.OnAbort;
					}
					else
					{
						this.chunk = new byte[this.maxChunkSize];

						this.response.Append("<data><chunkedBase64 streamId='");
						this.response.Append(this.streamId);
						this.response.Append("'/></data>");
						this.ReturnResponse();
					}

					lock (activeStreams)
					{
						activeStreams[this.from + " " + this.streamId] = this;
					}
				}
				else
					this.response.Append("<data><base64>");
			}
			else
				this.ReturnResponse();
		}

		private async Task InitiationCallback(object Sender, P2P.SOCKS5.StreamEventArgs e)
		{
			if (e.Ok)
				await this.socks5Output.Opened(e.Stream);
			else
				this.OnAbort(null, new EventArgs());
		}

		private void OnAbort(object sender, EventArgs e)
		{
			this.Cancel();
		}

		public override async Task<bool> ContentSentAsync()
		{
			this.ReturnResponse();

			if (this.chunked.HasValue && this.chunked.Value)
			{
				lock (activeStreams)
				{
					activeStreams.Remove(this.from + " " + this.streamId);
				}

				if (this.socks5Output != null)
					await this.socks5Output.Close();
				else if (this.ibbOutput != null)
					await this.ibbOutput.Close();
				else
					await this.SendChunk(true);
			}

			return true;
		}

		private void ReturnResponse()
		{
			this.AssertNotCancelled();

			if (this.response != null)
			{
				StringBuilder Resp = this.response;
				this.response = null;

				if (this.chunked.HasValue && !this.chunked.Value)
				{
					if (!(this.tail is null))
						Resp.Append(Convert.ToBase64String(this.tail));

					Resp.Append("</base64></data>");
				}

				Resp.Append("</resp>");

				if (this.e2e != null)
					this.e2e.SendIqResult(this.client, E2ETransmission.IgnoreIfNotE2E, this.id, this.to, Resp.ToString());
				else
					this.client.SendIqResult(this.id, this.to, Resp.ToString());
			}
		}

		public override Task<ulong> DecodeAsync(byte[] Buffer, int Offset, int NrRead)
		{
			throw new InternalServerErrorException();   // Will not be called.
		}

		public override async Task<bool> EncodeAsync(byte[] Buffer, int Offset, int NrBytes)
		{
			this.AssertNotCancelled();

			if (this.chunked.Value)
			{
				if (this.socks5Output != null)
					await this.socks5Output.Write(Buffer, Offset, NrBytes);
				else if (this.ibbOutput != null)
					await this.ibbOutput.Write(Buffer, Offset, NrBytes);
				else
				{
					int NrLeft = this.maxChunkSize - this.pos;

					while (NrBytes > 0)
					{
						if (NrBytes <= NrLeft)
						{
							Array.Copy(Buffer, Offset, this.chunk, this.pos, NrBytes);
							this.pos += NrBytes;
							NrBytes = 0;

							if (this.pos >= this.maxChunkSize)
								await this.SendChunk(false);
						}
						else
						{
							Array.Copy(Buffer, Offset, this.chunk, this.pos, NrLeft);
							this.pos += NrLeft;
							Offset += NrLeft;
							NrBytes -= NrLeft;
							await this.SendChunk(false);
							NrLeft = this.maxChunkSize;
						}
					}
				}
			}
			else
			{
				int i = this.tail?.Length ?? 0;
				int c = i + NrBytes;
				int d = c % 3;

				if (this.tail is null && d == 0)
					this.response.Append(Convert.ToBase64String(Buffer, Offset, NrBytes));
				else
				{
					c -= d;

					byte[] Bin = new byte[c];
					int j;

					if (i > 0)
						Array.Copy(this.tail, 0, Bin, 0, i);

					Array.Copy(Buffer, Offset, Bin, i, j = c - i);

					if (d == 0)
						this.tail = null;
					else
					{
						if (this.tail is null || this.tail.Length != d)
							this.tail = new byte[d];

						Array.Copy(Buffer, Offset + j, this.tail, 0, d);
					}
			
					this.response.Append(Convert.ToBase64String(Bin));
				}
			}

			return true;
		}

		private async Task SendChunk(bool Last)
		{
			if (this.client.State != XmppState.Connected)
				this.Cancel();

			this.AssertNotCancelled();

			StringBuilder Xml = new StringBuilder();
			string Key = this.from + " " + this.streamId;

			Xml.Append("<chunk xmlns='");
			Xml.Append(HttpxClient.Namespace);
			Xml.Append("' streamId='");
			Xml.Append(this.streamId);
			Xml.Append("' nr='");
			Xml.Append(this.nr.ToString());

			if (Last)
				Xml.Append("' last='true");
			else
			{
				lock (activeStreams)
				{
					if (!activeStreams.ContainsKey(Key))	// Has already been removed, if Last=true
						throw new IOException("Chunked stream not open.");
				}
			}

			Xml.Append("'>");

			if (this.pos > 0)
				Xml.Append(Convert.ToBase64String(this.chunk, 0, this.pos));

			Xml.Append("</chunk>");

			TaskCompletionSource<bool> ChunkSent = new TaskCompletionSource<bool>();

			this.client.SendMessage(QoSLevel.Unacknowledged, MessageType.Normal, this.to, Xml.ToString(), string.Empty, string.Empty, string.Empty,
				string.Empty, string.Empty, (sender, e) =>
				{
					ChunkSent.TrySetResult(true);
					return Task.CompletedTask;
				}, null);

			await Task.WhenAny(ChunkSent.Task, Task.Delay(1000));    // Limit read speed to rate at which messages can be sent to the network.

			this.nr++;
			this.pos = 0;
		}

		public override async Task<bool> FlushAsync()
		{
			this.ReturnResponse();

			if (this.pos > 0)
				await this.SendChunk(false);

			return true;
		}

		public void Cancel()
		{
			this.cancelled = true;

			if (this.chunked.HasValue && this.chunked.Value)
			{
				lock (activeStreams)
				{
					activeStreams.Remove(this.from + " " + this.streamId);
				}
			}
		}

		public static void CancelChunkedTransfer(string To, string From, string StreamId)
		{
			string Key = From + " " + StreamId;

			lock (activeStreams)
			{
				if (activeStreams.TryGetValue(Key, out HttpxResponse Response))
				{
					if (Response.to == To)
					{
						activeStreams.Remove(Key);
						Response.cancelled = true;
					}
				}
			}
		}
	}
}
