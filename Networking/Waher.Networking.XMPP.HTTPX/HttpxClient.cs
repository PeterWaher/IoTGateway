#define LOG_SOCKS5_EVENTS

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Networking.HTTP;
using Waher.Runtime.Temporary;
using Waher.Runtime.Threading;
using Waher.Security;
using Waher.Events;
using Waher.Networking.XMPP.P2P;

namespace Waher.Networking.XMPP.HTTPX
{
	/// <summary>
	/// HTTPX client.
	/// </summary>
	public class HttpxClient : XmppExtension
	{
		/// <summary>
		/// urn:xmpp:http
		/// </summary>
		public const string Namespace = "urn:xmpp:http";

		/// <summary>
		/// http://jabber.org/protocol/shim
		/// </summary>
		public const string NamespaceHeaders = "http://jabber.org/protocol/shim";

		private InBandBytestreams.IbbClient ibbClient = null;
		private P2P.SOCKS5.Socks5Proxy socks5Proxy = null;
		private IEndToEndEncryption e2e;
		private IPostResource postResource;
		private readonly int maxChunkSize;

		/// <summary>
		/// HTTPX client.
		/// </summary>
		/// <param name="Client">XMPP Client.</param>
		/// <param name="MaxChunkSize">Max Chunk Size to use.</param>
		public HttpxClient(XmppClient Client, int MaxChunkSize)
			: this(Client, null, MaxChunkSize)
		{
		}

		/// <summary>
		/// HTTPX client.
		/// </summary>
		/// <param name="Client">XMPP Client.</param>
		/// <param name="E2e">End-to-end encryption interface.</param>
		/// <param name="MaxChunkSize">Max Chunk Size to use.</param>
		public HttpxClient(XmppClient Client, IEndToEndEncryption E2e, int MaxChunkSize)
			: base(Client)
		{
			this.e2e = E2e;
			this.maxChunkSize = MaxChunkSize;

			HttpxChunks.RegisterChunkReceiver(this.client);
		}

		/// <summary>
		/// Implemented extensions.
		/// </summary>
		public override string[] Extensions => new string[] { "XEP-0332" };

		/// <summary>
		/// Optional end-to-end encryption interface to use in requests.
		/// </summary>
		public IEndToEndEncryption E2e
		{
			get { return this.e2e; }
			set { this.e2e = value; }
		}

		/// <summary>
		/// In-band bytestream client, if supported.
		/// </summary>
		public InBandBytestreams.IbbClient IbbClient
		{
			get { return this.ibbClient; }
			set
			{
				if (!(this.ibbClient is null))
					this.ibbClient.OnOpen -= this.IbbClient_OnOpen;

				this.ibbClient = value;
				this.ibbClient.OnOpen += this.IbbClient_OnOpen;
			}
		}

		/// <summary>
		/// SOCKS5 proxy, if supported.
		/// </summary>
		public P2P.SOCKS5.Socks5Proxy Socks5Proxy
		{
			get { return this.socks5Proxy; }
			set
			{
				if (!(this.socks5Proxy is null))
					this.socks5Proxy.OnOpen -= this.Socks5Proxy_OnOpen;

				this.socks5Proxy = value;
				this.socks5Proxy.OnOpen += this.Socks5Proxy_OnOpen;
			}
		}

		/// <summary>
		/// If responses can be posted to a specific resource.
		/// </summary>
		public IPostResource PostResource
		{
			get => this.postResource;
			set => this.postResource = value;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();

			HttpxChunks.UnregisterChunkReceiver(this.client);
		}

		/// <summary>
		/// Performs an HTTP GET request.
		/// </summary>
		/// <param name="To">Full JID of entity to query.</param>
		/// <param name="Resource">Local HTTP resource to query.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="DataCallback">Callback method to call when data is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="Headers">HTTP headers of the request.</param>
		public void GET(string To, string Resource, HttpxResponseEventHandler Callback,
			HttpxResponseDataEventHandler DataCallback, object State, params HttpField[] Headers)
		{
			this.Request(To, "GET", Resource, Callback, DataCallback, State, Headers);
		}

		// TODO: Add more HTTP methods.

		/// <summary>
		/// Performs an HTTP request.
		/// </summary>
		/// <param name="To">Full JID of entity to query.</param>
		/// <param name="Method">HTTP Method.</param>
		/// <param name="LocalResource">Local HTTP resource to query.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="DataCallback">Callback method to call when data is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="Headers">HTTP headers of the request.</param>
		public void Request(string To, string Method, string LocalResource, HttpxResponseEventHandler Callback,
			HttpxResponseDataEventHandler DataCallback, object State, params HttpField[] Headers)
		{
			this.Request(To, Method, LocalResource, 1.1, Headers, null, Callback, DataCallback, State);
		}

		/// <summary>
		/// Performs an HTTP request.
		/// </summary>
		/// <param name="To">Full JID of entity to query.</param>
		/// <param name="Method">HTTP Method.</param>
		/// <param name="LocalResource">Local resource.</param>
		/// <param name="HttpVersion">HTTP Version.</param>
		/// <param name="Headers">HTTP headers.</param>
		/// <param name="DataStream">Data Stream, if any, or null, if no data is sent.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="DataCallback">Local resource.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void Request(string To, string Method, string LocalResource, double HttpVersion, IEnumerable<HttpField> Headers,
			Stream DataStream, HttpxResponseEventHandler Callback, HttpxResponseDataEventHandler DataCallback, object State)
		{
			// TODO: Local IP & port for quick P2P response (TLS).

			StringBuilder Xml = new StringBuilder();
			ResponseState ResponseState = new ResponseState()
			{
				Callback = Callback,
				DataCallback = DataCallback,
				State = State
			};

			Xml.Append("<req xmlns='");
			Xml.Append(Namespace);
			Xml.Append("' method='");
			Xml.Append(Method);
			Xml.Append("' resource='");
			Xml.Append(XML.Encode(LocalResource));
			Xml.Append("' version='");
			Xml.Append(HttpVersion.ToString("F1").Replace(System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, "."));
			Xml.Append("' maxChunkSize='");
			Xml.Append(this.maxChunkSize.ToString());

			if (!(this.postResource is null))
			{
				string Resource = this.postResource.GetUrl(this.ResponsePostbackHandler, ResponseState);

				Xml.Append("' post='");
				Xml.Append(XML.Encode(Resource));

				ResponseState.PreparePostBackCall(this.e2e, Resource, this.client);
			}

			Xml.Append("' sipub='false' ibb='");
			Xml.Append(CommonTypes.Encode(!(this.ibbClient is null)));
			Xml.Append("' s5='");
			Xml.Append(CommonTypes.Encode(!(this.socks5Proxy is null)));
			Xml.Append("' jingle='false'>");

			Xml.Append("<headers xmlns='");
			Xml.Append(HttpxClient.NamespaceHeaders);
			Xml.Append("'>");

			foreach (HttpField HeaderField in Headers)
			{
				Xml.Append("<header name='");
				Xml.Append(XML.Encode(HeaderField.Key));
				Xml.Append("'>");
				Xml.Append(XML.Encode(HeaderField.Value));
				Xml.Append("</header>");
			}
			Xml.Append("</headers>");

			string StreamId = null;

			if (!(DataStream is null))
			{
				if (DataStream.Length < this.maxChunkSize)
				{
					int c = (int)DataStream.Length;
					byte[] Data = new byte[c];

					DataStream.Position = 0;
					DataStream.Read(Data, 0, c);

					Xml.Append("<data><base64>");
					Xml.Append(Convert.ToBase64String(Data));
					Xml.Append("</base64></data>");
				}
				else
				{
					StreamId = Guid.NewGuid().ToString().Replace("-", string.Empty);

					Xml.Append("<data><chunkedBase64 streamId='");
					Xml.Append(StreamId);
					Xml.Append("'/></data>");
				}
			}

			Xml.Append("</req>");

			if (!(this.e2e is null))
				this.e2e.SendIqSet(this.client, E2ETransmission.NormalIfNotE2E, To, Xml.ToString(), this.ResponseHandler, ResponseState, 60000, 0);
			else
				this.client.SendIqSet(To, Xml.ToString(), this.ResponseHandler, ResponseState, 60000, 0);

			if (!string.IsNullOrEmpty(StreamId))
			{
				byte[] Data = new byte[this.maxChunkSize];
				long Pos = 0;
				long Len = DataStream.Length;
				int Nr = 0;
				int i;

				DataStream.Position = 0;

				while (Pos < Len)
				{
					if (Pos + this.maxChunkSize <= Len)
						i = this.maxChunkSize;
					else
						i = (int)(Len - Pos);

					if (i != DataStream.Read(Data, 0, i))
						throw new IOException("Unexpected end of stream.");

					Pos += i;

					Xml.Clear();
					Xml.Append("<chunk xmlns='");
					Xml.Append(Namespace);
					Xml.Append("' streamId='");
					Xml.Append(StreamId);
					Xml.Append("' nr='");
					Xml.Append(Nr.ToString());

					if (Pos >= Len)
						Xml.Append("' last='true");

					Xml.Append("'>");
					Xml.Append(Convert.ToBase64String(Data, 0, i));
					Xml.Append("</chunk>");
					Nr++;

					if (!(this.e2e is null))
					{
						this.e2e.SendMessage(this.client, E2ETransmission.NormalIfNotE2E, QoSLevel.Unacknowledged,
							MessageType.Normal, string.Empty, To, Xml.ToString(), string.Empty, string.Empty,
							string.Empty, string.Empty, string.Empty, null, null);
					}
					else
					{
						this.client.SendMessage(MessageType.Normal, To, Xml.ToString(), string.Empty, string.Empty, string.Empty,
							string.Empty, string.Empty);
					}
				}
			}
		}

		private class ResponseState : IDisposable
		{
			public HttpxResponseEventHandler Callback;
			public HttpxResponseDataEventHandler DataCallback;
			public HttpxResponseEventArgs HttpxResponse = null;
			public object State;

			private string sha256 = null;
			private string id = null;
			private string from = null;
			private string to = null;
			private string endpointReference = null;
			private string symmetricCipherReference = null;
			private Stream data = null;
			private XmppClient client;
			private bool e2e = false;
			private bool disposeData = false;
			private bool disposed = false;
			private IEndToEndEncryption endpointSecurity;
			private MultiReadSingleWriteObject synchObj = null;

			public void PreparePostBackCall(IEndToEndEncryption EndpointSecurity, string Id, XmppClient Client)
			{
				this.synchObj = new MultiReadSingleWriteObject();
				this.endpointSecurity = EndpointSecurity;
				this.id = Id;
				this.client = Client;
			}

			public async Task PostDataReceived(object Sender, Stream Data, string From, string To, string EndpointReference, string SymmetricCipherReference)
			{
				if (this.disposed)
					return;

				if (!await this.synchObj.TryBeginWrite(60000))
				{
					this.client.Error("Unable to get access to HTTPX client. Dropping posted response.");
					return;
				}

				try
				{
					this.from = From;
					this.to = To;
					this.endpointReference = EndpointReference;
					this.symmetricCipherReference = SymmetricCipherReference;

					if (this.sha256 is null)
					{
						this.data = new TemporaryStream();
						await Data.CopyToAsync(this.data);
						this.disposeData = true;

						this.client.Information("HTTP(S) POST received. Waiting for HTTPX response.");
					}
					else
					{
						this.client.Information("HTTP(S) POST received.");
						string Msg = await this.CheckPostedData(Sender, Data);
						if (!string.IsNullOrEmpty(Msg))
							throw new BadRequestException(Msg);
					}
				}
				finally
				{
					await this.synchObj.EndWrite();
				}
			}

			public async Task Sha256Received(object Sender, string Sha256, bool E2e)
			{
				if (this.disposed)
					return;

				if (!await this.synchObj.TryBeginWrite(60000))
				{
					this.client.Error("Unable to get access to HTTPX client. Dropping posted response.");
					return;
				}

				try
				{
					this.sha256 = Sha256;
					this.e2e = E2e;

					if (!(this.data is null))
						await this.CheckPostedData(Sender, this.data);
				}
				finally
				{
					await this.synchObj.EndWrite();
				}
			}

			private async Task<string> CheckPostedData(object Sender, Stream Data)
			{
				try
				{
					string CipherLocalName;
					string CipherNamespace;
					string Msg;

					Data.Position = 0;

					if (this.e2e)
					{
						int i = this.symmetricCipherReference.IndexOf('#');

						if (i < 0)
						{
							CipherLocalName = this.symmetricCipherReference;
							CipherNamespace = string.Empty;
						}
						else
						{
							CipherLocalName = this.symmetricCipherReference.Substring(i + 1);
							CipherNamespace = this.symmetricCipherReference.Substring(0, i);
						}

						if (!this.endpointSecurity.TryGetSymmetricCipher(CipherLocalName, CipherNamespace, out IE2eSymmetricCipher SymmetricCipher))
						{
							this.client.Error(Msg = "Symmetric cipher not understood: " + this.symmetricCipherReference);
							return Msg;
						}

						Stream Decrypted = await this.endpointSecurity.Decrypt(this.endpointReference, this.id, "POST", this.from, this.to, Data, SymmetricCipher);
						if (Decrypted is null)
						{
							StringBuilder sb = new StringBuilder();

							sb.Append("Unable to decrypt POSTed payload. Endpoint: ");
							sb.Append(this.endpointReference);
							sb.Append(", Id: ");
							sb.Append(this.id);
							sb.Append(", Type: POST, From: ");
							sb.Append(this.from);
							sb.Append(", To: ");
							sb.Append(this.to);
							sb.Append(", Cipher: ");
							sb.Append(this.symmetricCipherReference);
							sb.Append(", Bytes: ");
							sb.Append(Data.Length.ToString());

							this.client.Error(Msg = sb.ToString());
							return Msg;
						}

						if (this.disposeData)
							this.data?.Dispose();

						this.data = Data = Decrypted;
						this.disposeData = true;
					}

					Data.Position = 0;
					byte[] Digest = Hashes.ComputeSHA256Hash(Data);
					string DigestBase64 = Convert.ToBase64String(Digest);

					if (DigestBase64 == this.sha256)
					{
						this.client.Information("POSTed response validated and accepted.");

						long Count = Data.Length;
						int BufSize = (int)Math.Min(65536, Count);
						byte[] Buf = new byte[BufSize];

						Data.Position = 0;

						while (Count > 0)
						{
							if (Count < BufSize)
							{
								Array.Resize<byte>(ref Buf, (int)Count);
								BufSize = (int)Count;
							}

							if (BufSize != await Data.ReadAsync(Buf, 0, BufSize))
								throw new IOException("Unexpected end of file.");

							Count -= BufSize;

							if (!(this.DataCallback is null))
							{
								try
								{
									await this.DataCallback(Sender, new HttpxResponseDataEventArgs(this.HttpxResponse, Buf, string.Empty, Count <= 0, this.State));
								}
								catch (Exception ex)
								{
									Log.Critical(ex);
								}
							}
						}
					}
					else
					{
						this.client.Error(Msg = "Dropping POSTed response, as SHA-256 digest did not match reported digest in response.");
						return Msg;
					}
				}
				finally
				{
					this.Dispose();
				}

				return null;
			}

			public void Dispose()
			{
				if (!this.disposed)
				{
					this.disposed = true;
					this.synchObj?.Dispose();

					if (this.disposeData)
					{
						this.data?.Dispose();
						this.data = null;
					}
				}
			}
		}

		private Task ResponsePostbackHandler(object Sender, PostBackEventArgs e)
		{
			ResponseState ResponseState = (ResponseState)e.State;
			return ResponseState.PostDataReceived(this, e.Data, e.From, e.To, e.EndpointReference, e.SymmetricCipherReference);
		}

		private async Task ResponseHandler(object Sender, IqResultEventArgs e)
		{
			XmlElement E = e.FirstElement;
			HttpResponse Response;
			string StatusMessage;
			double Version;
			int StatusCode;
			ResponseState ResponseState = (ResponseState)e.State;
			byte[] Data = null;
			bool HasData = false;
			bool DisposeResponse = true;

			if (e.Ok && E != null && E.LocalName == "resp" && E.NamespaceURI == Namespace)
			{
				Version = XML.Attribute(E, "version", 0.0);
				StatusCode = XML.Attribute(E, "statusCode", 0);
				StatusMessage = XML.Attribute(E, "statusMessage");
				Response = new HttpResponse();

				foreach (XmlNode N in E.ChildNodes)
				{
					switch (N.LocalName)
					{
						case "headers":
							foreach (XmlNode N2 in N.ChildNodes)
							{
								switch (N2.LocalName)
								{
									case "header":
										string Key = XML.Attribute((XmlElement)N2, "name");
										string Value = N2.InnerText;

										Response.SetHeader(Key, Value);
										break;
								}
							}
							break;

						case "data":
							foreach (XmlNode N2 in N.ChildNodes)
							{
								switch (N2.LocalName)
								{
									case "text":
										MemoryStream ms = new MemoryStream();
										Response.SetResponseStream(ms);
										Data = Response.Encoding.GetBytes(N2.InnerText);
										ms.Write(Data, 0, Data.Length);
										ms.Position = 0;
										HasData = true;
										break;

									case "xml":
										ms = new MemoryStream();
										Response.SetResponseStream(ms);
										Data = Response.Encoding.GetBytes(N2.InnerText);
										ms.Write(Data, 0, Data.Length);
										ms.Position = 0;
										HasData = true;
										break;

									case "base64":
										ms = new MemoryStream();
										Response.SetResponseStream(ms);
										Data = Convert.FromBase64String(N2.InnerText);
										ms.Write(Data, 0, Data.Length);
										ms.Position = 0;
										HasData = true;
										break;

									case "chunkedBase64":
										string StreamId = XML.Attribute((XmlElement)N2, "streamId");

										ResponseState.HttpxResponse = new HttpxResponseEventArgs(e, Response, ResponseState.State, Version, StatusCode, StatusMessage, true, null);

										HttpxChunks.chunkedStreams.Add(e.From + " " + StreamId, new ClientChunkRecord(this,
											ResponseState.HttpxResponse, Response, ResponseState.DataCallback, ResponseState.State,
											StreamId, e.From, e.To, false, null, null));

										DisposeResponse = false;
										HasData = true;
										break;

									case "ibb":
										StreamId = XML.Attribute((XmlElement)N2, "sid");

										ResponseState.HttpxResponse = new HttpxResponseEventArgs(e, Response, ResponseState.State, Version, StatusCode, StatusMessage, true, null);

										HttpxChunks.chunkedStreams.Add(e.From + " " + StreamId, new ClientChunkRecord(this,
											ResponseState.HttpxResponse, Response, ResponseState.DataCallback, ResponseState.State,
											StreamId, e.From, e.To, false, null, null));

										DisposeResponse = false;
										HasData = true;
										break;

									case "s5":
										StreamId = XML.Attribute((XmlElement)N2, "sid");
										bool E2e = XML.Attribute((XmlElement)N2, "e2e", false);

										ResponseState.HttpxResponse = new HttpxResponseEventArgs(e, Response, ResponseState.State, Version, StatusCode, StatusMessage, true, null);

										HttpxChunks.chunkedStreams.Add(e.From + " " + StreamId, new ClientChunkRecord(this,
											ResponseState.HttpxResponse, Response, ResponseState.DataCallback, ResponseState.State,
											StreamId, e.From, e.To, E2e, e.E2eReference, e.E2eSymmetricCipher));

										DisposeResponse = false;
										HasData = true;
										break;

									case "sha256":
										E2e = XML.Attribute((XmlElement)N2, "e2e", false);
										string DigestBase64 = N2.InnerText;

										ResponseState.HttpxResponse = new HttpxResponseEventArgs(e, Response, ResponseState.State, Version, StatusCode, StatusMessage, true, null);

										Task _ = Task.Run(() => ResponseState.Sha256Received(this, DigestBase64, E2e));

										DisposeResponse = false;
										HasData = true;
										break;

									case "sipub":
										// TODO: Implement File Transfer support.
										break;

									case "jingle":
										// TODO: Implement Jingle support.
										break;
								}
							}
							break;
					}
				}
			}
			else
			{
				Version = 0.0;
				StatusCode = 503;
				StatusMessage = "Service Unavailable";
				Response = new HttpResponse();
			}

			HttpxResponseEventArgs e2 = ResponseState.HttpxResponse ??
				new HttpxResponseEventArgs(e, Response, ResponseState.State, Version, StatusCode, StatusMessage, HasData, Data);

			try
			{
				await ResponseState.Callback(this, e2);
			}
			catch (Exception)
			{
				// Ignore.
			}
			finally
			{
				if (DisposeResponse)
				{
					Response.Dispose();
					ResponseState.Dispose();
				}
			}
		}

		/// <summary>
		/// Requests the transfer of a stream to be cancelled.
		/// </summary>
		/// <param name="To">The sender of the stream.</param>
		/// <param name="StreamId">Stream ID.</param>
		public void CancelTransfer(string To, string StreamId)
		{
			HttpxChunks.chunkedStreams.Remove(To + " " + StreamId);

			StringBuilder Xml = new StringBuilder();

			Xml.Append("<cancel xmlns='");
			Xml.Append(Namespace);
			Xml.Append("' streamId='");
			Xml.Append(StreamId);
			Xml.Append("'/>");

			if (!(this.e2e is null))
			{
				this.e2e.SendMessage(this.client, E2ETransmission.NormalIfNotE2E, QoSLevel.Unacknowledged,
					MessageType.Normal, string.Empty, To, Xml.ToString(), string.Empty, string.Empty, string.Empty,
					string.Empty, string.Empty, null, null);
			}
			else
				this.client.SendMessage(MessageType.Normal, To, Xml.ToString(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
		}

		private Task IbbClient_OnOpen(object Sender, InBandBytestreams.ValidateStreamEventArgs e)
		{
			string Key = e.From + " " + e.StreamId;

			if (HttpxChunks.chunkedStreams.ContainsKey(Key))
				e.AcceptStream(this.IbbDataReceived, this.IbbStreamClosed, new object[] { Key, -1, null });

			return Task.CompletedTask;
		}

		private async Task IbbDataReceived(object Sender, InBandBytestreams.DataReceivedEventArgs e)
		{
			object[] P = (object[])e.State;
			string Key = (string)P[0];
			int Nr = (int)P[1];
			byte[] PrevData = (byte[])P[2];

			if (HttpxChunks.chunkedStreams.TryGetValue(Key, out ChunkRecord Rec))
			{
				if (!(PrevData is null))
					await Rec.ChunkReceived(Nr, false, PrevData);

				Nr++;
				P[1] = Nr;
				P[2] = e.Data;
			}
		}

		private async Task IbbStreamClosed(object Sender, InBandBytestreams.StreamClosedEventArgs e)
		{
			object[] P = (object[])e.State;
			string Key = (string)P[0];
			int Nr = (int)P[1];
			byte[] PrevData = (byte[])P[2];

			if (HttpxChunks.chunkedStreams.TryGetValue(Key, out ChunkRecord Rec))
			{
				if (e.Reason == InBandBytestreams.CloseReason.Done)
				{
					if (!(PrevData is null))
						await Rec.ChunkReceived(Nr, true, PrevData);
					else
						await Rec.ChunkReceived(Nr, true, new byte[0]);

					P[2] = null;
				}
				else
					HttpxChunks.chunkedStreams.Remove(Key);
			}
		}

		private Task Socks5Proxy_OnOpen(object Sender, P2P.SOCKS5.ValidateStreamEventArgs e)
		{
			string Key = e.From + " " + e.StreamId;
			ClientChunkRecord ClientRec;

			if (HttpxChunks.chunkedStreams.TryGetValue(Key, out ChunkRecord Rec))
			{
				ClientRec = Rec as ClientChunkRecord;

				if (!(ClientRec is null))
				{
#if LOG_SOCKS5_EVENTS
					this.client.Information("Accepting SOCKS5 stream from " + e.From);
#endif
					e.AcceptStream(this.Socks5DataReceived, this.Socks5StreamClosed, new Socks5Receiver(Key, e.StreamId,
						ClientRec.from, ClientRec.to, ClientRec.e2e, ClientRec.endpointReference, ClientRec.symmetricCipher));
				}
			}

			return Task.CompletedTask;
		}

		private class Socks5Receiver
		{
			public string Key;
			public string StreamId;
			public string From;
			public string To;
			public string EndpointReference;
			public IE2eSymmetricCipher SymmetricCipher;
			public int State = 0;
			public int BlockSize;
			public int BlockPos;
			public int Nr = 0;
			public byte[] Block;
			public bool E2e;

			public Socks5Receiver(string Key, string StreamId, string From, string To, bool E2e, string EndpointReference,
				IE2eSymmetricCipher SymmetricCipher)
			{
				this.Key = Key;
				this.StreamId = StreamId;
				this.From = From;
				this.To = To;
				this.E2e = E2e;
				this.EndpointReference = EndpointReference;
				this.SymmetricCipher = SymmetricCipher;
			}
		}

		private async Task Socks5DataReceived(object Sender, P2P.SOCKS5.DataReceivedEventArgs e)
		{
			Socks5Receiver Rx = (Socks5Receiver)e.State;

			if (HttpxChunks.chunkedStreams.TryGetValue(Rx.Key, out ChunkRecord Rec))
			{
#if LOG_SOCKS5_EVENTS
				this.client.Information(e.Count.ToString() + " bytes received over SOCKS5 stream " + Rx.Key + ".");
#endif
				byte[] Buffer = e.Buffer;
				int Offset = e.Offset;
				int Count = e.Count;
				int d;

				while (Count > 0)
				{
					switch (Rx.State)
					{
						case 0:
							Rx.BlockSize = Buffer[Offset++];
							Count--;
							Rx.State++;
							break;

						case 1:
							Rx.BlockSize <<= 8;
							Rx.BlockSize |= Buffer[Offset++];
							Count--;

							if (Rx.BlockSize == 0)
							{
								HttpxChunks.chunkedStreams.Remove(Rx.Key);
								await Rec.ChunkReceived(Rx.Nr++, true, new byte[0]);
								e.Stream.Dispose();
								return;
							}

							Rx.BlockPos = 0;

							if (Rx.Block is null || Rx.Block.Length != Rx.BlockSize)
								Rx.Block = new byte[Rx.BlockSize];

							Rx.State++;
							break;

						case 2:
							d = Math.Min(Count, Rx.BlockSize - Rx.BlockPos);

							Array.Copy(Buffer, Offset, Rx.Block, Rx.BlockPos, d);
							Offset += d;
							Rx.BlockPos += d;
							Count -= d;

							if (Rx.BlockPos >= Rx.BlockSize)
							{
								if (Rx.E2e)
								{
									string Id = Rec.NextId().ToString();
									Rx.Block = this.e2e.Decrypt(Rx.EndpointReference, Id, Rx.StreamId, Rx.From, Rx.To, Rx.Block, Rx.SymmetricCipher);
									if (Rx.Block is null)
									{
										string Message = "Decryption of chunk " + Rx.Nr.ToString() + " failed.";
#if LOG_SOCKS5_EVENTS
										this.client.Error(Message);
#endif
										await Rec.Fail(Message);
										e.Stream.Dispose();
										return;
									}
								}

#if LOG_SOCKS5_EVENTS
								this.client.Information("Chunk " + Rx.Nr.ToString() + " received and forwarded.");
#endif
								await Rec.ChunkReceived(Rx.Nr++, false, Rx.Block);
								Rx.State = 0;
							}
							break;
					}
				}
			}
			else
			{
#if LOG_SOCKS5_EVENTS
				this.client.Warning(e.Count.ToString() + " bytes received over SOCKS5 stream " + Rx.Key + " and discarded.");
#endif
				e.Stream.Dispose();
			}
		}

		private async Task Socks5StreamClosed(object Sender, P2P.SOCKS5.StreamEventArgs e)
		{
#if LOG_SOCKS5_EVENTS
			this.client.Information("SOCKS5 stream closed.");
#endif
			Socks5Receiver Rx = (Socks5Receiver)e.State;

			if (HttpxChunks.chunkedStreams.TryGetValue(Rx.Key, out ChunkRecord Rec))
			{
				HttpxChunks.chunkedStreams.Remove(Rx.Key);
				await Rec.ChunkReceived(Rx.Nr++, true, new byte[0]);
			}
		}

	}
}
