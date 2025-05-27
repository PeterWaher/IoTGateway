#define LOG_SOCKS5_EVENTS

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.HTTP;
using Waher.Networking.XMPP.Events;
using Waher.Runtime.IO;
using Waher.Runtime.Temporary;
using Waher.Runtime.Threading;
using Waher.Security;

namespace Waher.Networking.XMPP.HTTPX
{
	/// <summary>
	/// HTTPX client.
	/// </summary>
	public class HttpxClient : XmppExtension
	{
		/// <summary>
		/// String identifying the extension on the client.
		/// </summary>
		public const string ExtensionId = "XEP-0332-c";

		/// <summary>
		/// urn:xmpp:http
		/// </summary>
		public const string Namespace = "urn:xmpp:http";

		/// <summary>
		/// urn:xmpp:http
		/// </summary>
		public const string NamespaceJwt = "urn:xmpp:jwt:0";

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
		public override string[] Extensions => new string[] { ExtensionId };

		/// <summary>
		/// Optional end-to-end encryption interface to use in requests.
		/// </summary>
		public IEndToEndEncryption E2e
		{
			get => this.e2e;
			set => this.e2e = value;
		}

		/// <summary>
		/// In-band bytestream client, if supported.
		/// </summary>
		public InBandBytestreams.IbbClient IbbClient
		{
			get => this.ibbClient;
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
			get => this.socks5Proxy;
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

		/// <inheritdoc/>
		public override void Dispose()
		{
			HttpxChunks.UnregisterChunkReceiver(this.client);
			base.Dispose();
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
		public Task GET(string To, string Resource, EventHandlerAsync<HttpxResponseEventArgs> Callback,
			EventHandlerAsync<HttpxResponseDataEventArgs> DataCallback, object State, params HttpField[] Headers)
		{
			return this.Request(To, "GET", Resource, Callback, DataCallback, State, Headers);
		}

		/// <summary>
		/// Performs a HTTP POST request.
		/// </summary>
		/// <param name="To">Full JID of entity to query.</param>
		/// <param name="Resource">Local HTTP resource to query.</param>
		/// <param name="Data">Data to post to the resource. Data will be encoded prior to performing the request.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="DataCallback">Callback method to call when data is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="Headers">HTTP headers of the request.</param>
		public async Task POST(string To, string Resource, object Data,
			EventHandlerAsync<HttpxResponseEventArgs> Callback, EventHandlerAsync<HttpxResponseDataEventArgs> DataCallback,
			object State, params HttpField[] Headers)
		{
			ContentResponse P = await InternetContent.EncodeAsync(Data, Encoding.UTF8);
			P.AssertOk();

			await this.POST(To, Resource, P.Encoded, P.ContentType, Callback, DataCallback, State, Headers);
		}

		/// <summary>
		/// Performs a HTTP POST request.
		/// </summary>
		/// <param name="To">Full JID of entity to query.</param>
		/// <param name="Resource">Local HTTP resource to query.</param>
		/// <param name="Data">Binary data to post to the resource.</param>
		/// <param name="ContentType">Content-Type of data to post.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="DataCallback">Callback method to call when data is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="Headers">HTTP headers of the request.</param>
		public async Task POST(string To, string Resource, byte[] Data, string ContentType,
			EventHandlerAsync<HttpxResponseEventArgs> Callback, EventHandlerAsync<HttpxResponseDataEventArgs> DataCallback,
			object State, params HttpField[] Headers)
		{
			MemoryStream DataStream = new MemoryStream(Data);

			try
			{
				Task ResponseReceived(object Sender, HttpxResponseEventArgs e)
				{
					DataStream?.Dispose();
					DataStream = null;

					return Callback.Raise(Sender, e);
				}
				;

				await this.POST(To, Resource, DataStream, ContentType, ResponseReceived, DataCallback, State, Headers);
			}
			catch (Exception ex)
			{
				DataStream?.Dispose();
				ExceptionDispatchInfo.Capture(ex).Throw();
			}
		}

		/// <summary>
		/// Performs a HTTP POST request.
		/// </summary>
		/// <param name="To">Full JID of entity to query.</param>
		/// <param name="Resource">Local HTTP resource to query.</param>
		/// <param name="DataStream">Binary data stream to post to the resource.</param>
		/// <param name="ContentType">Content-Type of data to post.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="DataCallback">Callback method to call when data is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="Headers">HTTP headers of the request.</param>
		public Task POST(string To, string Resource, Stream DataStream, string ContentType,
			EventHandlerAsync<HttpxResponseEventArgs> Callback, EventHandlerAsync<HttpxResponseDataEventArgs> DataCallback,
			object State, params HttpField[] Headers)
		{
			List<HttpField> Headers2 = new List<HttpField>()
			{
				new HttpField("Content-Type", ContentType)
			};

			if (!(Headers is null))
			{
				foreach (HttpField Field in Headers)
				{
					if (Field.Key != "Content-Type")
						Headers2.Add(Field);
				}
			}

			return this.Request(To, "POST", Resource, 1.1, Headers2, DataStream, Callback, DataCallback, State);
		}

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
		public Task Request(string To, string Method, string LocalResource, EventHandlerAsync<HttpxResponseEventArgs> Callback,
			EventHandlerAsync<HttpxResponseDataEventArgs> DataCallback, object State, params HttpField[] Headers)
		{
			return this.Request(To, Method, LocalResource, 1.1, Headers, null, Callback, DataCallback, State);
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
		public async Task Request(string To, string Method, string LocalResource, double HttpVersion, IEnumerable<HttpField> Headers,
			Stream DataStream, EventHandlerAsync<HttpxResponseEventArgs> Callback, EventHandlerAsync<HttpxResponseDataEventArgs> DataCallback, object State)
		{
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
				string Resource = await this.postResource.GetUrl(this.ResponsePostbackHandler, ResponseState);

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
			Xml.Append(NamespaceHeaders);
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
					DataStream.Position = 0;
					byte[] Data = await DataStream.ReadAllAsync();

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

			await this.SendIqSet(To, Xml.ToString(), ResponseState);

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

					await DataStream.ReadAllAsync(Data, 0, i);

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

					await this.SendChunk(To, Xml.ToString(), ResponseState);
				}
			}
		}

		private async Task SendIqSet(string To, string Xml, object ResponseState)
		{
			TaskCompletionSource<bool> StanzaSent = new TaskCompletionSource<bool>();
			Task FlagStanzaAsSent(object Sender, EventArgs e)
			{
				StanzaSent.TrySetResult(true);
				return Task.CompletedTask;
			}
			;

			if (!(this.e2e is null))
			{
				await this.e2e.SendIqSet(this.client, E2ETransmission.NormalIfNotE2E, To, Xml,
					this.ResponseHandler, ResponseState, 60000, 0, FlagStanzaAsSent);
			}
			else
			{
				await this.client.SendIqSet(To, Xml, this.ResponseHandler, ResponseState,
					60000, 0, FlagStanzaAsSent);
			}

			Task _ = Task.Delay(10000).ContinueWith((_2) =>
				StanzaSent.TrySetException(new GenericException(new TimeoutException("Unable to send HTTPX request."), null, To)));

			await StanzaSent.Task;  // By waiting for request to have been sent, E2E synchronization has already been performed, if necessary.
		}

		private async Task SendChunk(string To, string Xml, object ResponseState)
		{
			TaskCompletionSource<bool> StanzaSent = new TaskCompletionSource<bool>();
			Task FlagStanzaAsSent(object Sender, EventArgs e)
			{
				StanzaSent.TrySetResult(true);
				return Task.CompletedTask;
			};

			if (!(this.e2e is null))
			{
				await this.e2e.SendMessage(this.client, E2ETransmission.NormalIfNotE2E, QoSLevel.Unacknowledged,
					MessageType.Normal, string.Empty, To, Xml.ToString(), string.Empty, string.Empty,
					string.Empty, string.Empty, string.Empty, FlagStanzaAsSent, ResponseState);
			}
			else
			{
				await this.client.SendMessage(QoSLevel.Unacknowledged, MessageType.Normal, string.Empty, To, Xml.ToString(),
					string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, FlagStanzaAsSent, ResponseState);
			}

			Task _ = Task.Delay(10000).ContinueWith((_2) =>
				StanzaSent.TrySetException(new GenericException(new TimeoutException("Unable to send HTTPX data chunk."), null, To)));

			await StanzaSent.Task;  // By waiting for chunk to have been sent, transmission is throttled to the network bandwidth.
		}

		internal class ResponseState : IDisposable
		{
			public EventHandlerAsync<HttpxResponseEventArgs> Callback;
			public EventHandlerAsync<HttpxResponseDataEventArgs> DataCallback;
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
				this.synchObj = new MultiReadSingleWriteObject(this);
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
					if (!(this.synchObj is null))
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
					if (!(this.synchObj is null))
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
							CipherLocalName = this.symmetricCipherReference[(i + 1)..];
							CipherNamespace = this.symmetricCipherReference[..i];
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
								Array.Resize(ref Buf, (int)Count);
								BufSize = (int)Count;
							}

							await Data.ReadAllAsync(Buf, 0, BufSize);

							Count -= BufSize;

							HttpxResponseDataEventArgs e = new HttpxResponseDataEventArgs(this.HttpxResponse,
								true, Buf, string.Empty, Count <= 0, this.State);

							await this.DataCallback.Raise(Sender, e, false);
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
					this.synchObj = null;

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
			ClientChunkRecord Record = null;
			PendingChunkRecord PendingRecord = null;

			if (e.Ok && !(E is null) && E.LocalName == "resp" && E.NamespaceURI == Namespace)
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

										ResponseState.HttpxResponse = new HttpxResponseEventArgs(e, Response, ResponseState.State, Version, StatusCode, StatusMessage, true, true, null);

										Record = new ClientChunkRecord(this, ResponseState.HttpxResponse,
											Response, ResponseState.DataCallback, ResponseState.State, StreamId, e.From,
											e.To, false, null, null);

										PendingRecord = await HttpxChunks.Add(e.From + " " + StreamId, Record);

										DisposeResponse = false;
										HasData = true;
										break;

									case "ibb":
										StreamId = XML.Attribute((XmlElement)N2, "sid");

										ResponseState.HttpxResponse = new HttpxResponseEventArgs(e, Response, ResponseState.State, Version, StatusCode, StatusMessage, true, true, null);

										Record = new ClientChunkRecord(this, ResponseState.HttpxResponse,
											Response, ResponseState.DataCallback, ResponseState.State, StreamId, e.From,
											e.To, false, null, null);

										PendingRecord = await HttpxChunks.Add(e.From + " " + StreamId, Record);

										DisposeResponse = false;
										HasData = true;
										break;

									case "s5":
										StreamId = XML.Attribute((XmlElement)N2, "sid");
										bool E2e = XML.Attribute((XmlElement)N2, "e2e", false);

										ResponseState.HttpxResponse = new HttpxResponseEventArgs(e, Response, ResponseState.State, Version, StatusCode, StatusMessage, true, true, null);

										Record = new ClientChunkRecord(this, ResponseState.HttpxResponse,
											Response, ResponseState.DataCallback, ResponseState.State, StreamId, e.From,
											e.To, E2e, e.E2eReference, e.E2eSymmetricCipher);

										PendingRecord = await HttpxChunks.Add(e.From + " " + StreamId, Record);

										DisposeResponse = false;
										HasData = true;
										break;

									case "sha256":
										E2e = XML.Attribute((XmlElement)N2, "e2e", false);
										string DigestBase64 = N2.InnerText;

										ResponseState.HttpxResponse = new HttpxResponseEventArgs(e, Response, ResponseState.State, Version, StatusCode, StatusMessage, true, true, null);

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
				new HttpxResponseEventArgs(e, Response, ResponseState.State, Version, StatusCode, StatusMessage, HasData, true, Data);

			try
			{
				await ResponseState.Callback.Raise(this, e2, false);

				if (!(PendingRecord is null) && !(Record is null))
					await PendingRecord.Replay(Record);
			}
			finally
			{
				if (DisposeResponse)
				{
					await Response.DisposeAsync();
					ResponseState.Dispose();
				}
			}
		}

		/// <summary>
		/// Requests the transfer of a stream to be cancelled.
		/// </summary>
		/// <param name="To">The sender of the stream.</param>
		/// <param name="StreamId">Stream ID.</param>
		public async Task CancelTransfer(string To, string StreamId)
		{
			await HttpxChunks.Cancel(To + " " + StreamId);

			StringBuilder Xml = new StringBuilder();

			Xml.Append("<cancel xmlns='");
			Xml.Append(Namespace);
			Xml.Append("' streamId='");
			Xml.Append(StreamId);
			Xml.Append("'/>");

			if (!(this.e2e is null))
			{
				await this.e2e.SendMessage(this.client, E2ETransmission.NormalIfNotE2E, QoSLevel.Unacknowledged,
					MessageType.Normal, string.Empty, To, Xml.ToString(), string.Empty, string.Empty, string.Empty,
					string.Empty, string.Empty, null, null);
			}
			else
				await this.client.SendMessage(MessageType.Normal, To, Xml.ToString(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
		}

		private async Task IbbClient_OnOpen(object Sender, InBandBytestreams.ValidateStreamEventArgs e)
		{
			string Key = e.From + " " + e.StreamId;

			if (await HttpxChunks.Contains(Key))
				e.AcceptStream(this.IbbDataReceived, this.IbbStreamClosed, new object[] { Key, -1, true, null });
		}

		private async Task IbbDataReceived(object Sender, InBandBytestreams.DataReceivedEventArgs e)
		{
			object[] P = (object[])e.State;
			string Key = (string)P[0];
			int Nr = (int)P[1];
			bool ConstantBuffer = (bool)P[2];
			byte[] PrevData = (byte[])P[3];

			if (await HttpxChunks.Received(Key, Nr, false, ConstantBuffer, PrevData))
			{
				Nr++;
				P[1] = Nr;
				P[2] = e.ConstantBuffer;
				P[3] = e.Data;
			}
		}

		private async Task IbbStreamClosed(object Sender, InBandBytestreams.StreamClosedEventArgs e)
		{
			object[] P = (object[])e.State;
			string Key = (string)P[0];
			int Nr = (int)P[1];
			bool ConstantBuffer = (bool)P[2];
			byte[] PrevData = (byte[])P[3];

			if (e.Reason == InBandBytestreams.CloseReason.Done)
			{
				await HttpxChunks.Received(Key, Nr, true, ConstantBuffer, PrevData);
				P[2] = null;
			}
			else
				await HttpxChunks.Cancel(Key);
		}

		private async Task Socks5Proxy_OnOpen(object Sender, P2P.SOCKS5.ValidateStreamEventArgs e)
		{
			string Key = e.From + " " + e.StreamId;

			if (await HttpxChunks.TryGetRecord(Key, false) is ClientChunkRecord ClientRec)
			{
#if LOG_SOCKS5_EVENTS
				this.client.Information("Accepting SOCKS5 stream from " + e.From);
#endif
				e.AcceptStream(this.Socks5DataReceived, this.Socks5StreamClosed, new Socks5Receiver(Key, e.StreamId,
					ClientRec.From, ClientRec.To, ClientRec.E2e, ClientRec.EndpointReference, ClientRec.SymmetricCipher));
			}
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
			ChunkRecord Rec = await HttpxChunks.TryGetRecord(Rx.Key, false);

			if (!(Rec is null))
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
								await HttpxChunks.Cancel(Rx.Key);
								await Rec.ChunkReceived(Rx.Nr++, true, true, Array.Empty<byte>());
								await e.Stream.DisposeAsync();
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
									Rx.Block = await this.e2e.Decrypt(Rx.EndpointReference, Id, Rx.StreamId, Rx.From, Rx.To, Rx.Block, Rx.SymmetricCipher);
									if (Rx.Block is null)
									{
										string Message = "Decryption of chunk " + Rx.Nr.ToString() + " failed.";
#if LOG_SOCKS5_EVENTS
										this.client.Error(Message);
#endif
										await Rec.Fail(Message);
										await e.Stream.DisposeAsync();
										return;
									}
								}

#if LOG_SOCKS5_EVENTS
								this.client.Information("Chunk " + Rx.Nr.ToString() + " received and forwarded.");
#endif
								await Rec.ChunkReceived(Rx.Nr++, false, false, Rx.Block);
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
				await e.Stream.DisposeAsync();
			}
		}

		private async Task Socks5StreamClosed(object Sender, P2P.SOCKS5.StreamEventArgs e)
		{
#if LOG_SOCKS5_EVENTS
			this.client.Information("SOCKS5 stream closed.");
#endif
			Socks5Receiver Rx = (Socks5Receiver)e.State;
			ChunkRecord Rec = await HttpxChunks.TryGetRecord(Rx.Key, true);

			if (!(Rec is null))
				await Rec.ChunkReceived(Rx.Nr++, true, true, Array.Empty<byte>());
		}

		/// <summary>
		/// Gets a JWT token from the server to which the client is connceted. The JWT token encodes the
		/// current XMPP connection, and can be used in distributed transactions over other protocols
		/// (such as HTTP) to refer back to the current connection.
		/// </summary>
		/// <param name="Seconds">Requested number of seconds for which the token will be valid.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task GetJwtToken(int Seconds, EventHandlerAsync<TokenResponseEventArgs> Callback, object State)
		{
			return this.GetJwtToken(this.client.Domain, Seconds, Callback, State);
		}

		/// <summary>
		/// Gets a JWT token from a token factory addressed by <paramref name="Address"/>.
		/// </summary>
		/// <param name="Address">Address to token factory.</param>
		/// <param name="Seconds">Requested number of seconds for which the token will be valid.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task GetJwtToken(string Address, int Seconds, EventHandlerAsync<TokenResponseEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<jwt xmlns='");
			Xml.Append(NamespaceJwt);
			Xml.Append("' seconds='");
			Xml.Append(Seconds.ToString());
			Xml.Append("'/>");

			return this.client.SendIqGet(Address, Xml.ToString(), async (Sender, e) =>
			{
				string Token = null;

				if (e.Ok && !(e.FirstElement is null) && e.FirstElement.LocalName == "token" && e.FirstElement.NamespaceURI == NamespaceJwt)
					Token = e.FirstElement.InnerText;
				else
					e.Ok = false;

				await Callback.Raise(this, new TokenResponseEventArgs(e, Token));
			}, State);
		}

		/// <summary>
		/// Gets a JWT token from the server to which the client is connceted. The JWT token encodes the
		/// current XMPP connection, and can be used in distributed transactions over other protocols
		/// (such as HTTP) to refer back to the current connection.
		/// </summary>
		/// <param name="Seconds">Requested number of seconds for which the token will be valid.</param>
		/// <returns>Generated token</returns>
		/// <exception cref="Exception">If unable to create token.</exception>
		public Task<string> GetJwtTokenAsync(int Seconds)
		{
			return this.GetJwtTokenAsync(this.client.Domain, Seconds);
		}

		/// <summary>
		/// Gets a JWT token from a token factory addressed by <paramref name="Address"/>.
		/// </summary>
		/// <param name="Address">Address to token factory.</param>
		/// <param name="Seconds">Requested number of seconds for which the token will be valid.</param>
		/// <returns>Generated token</returns>
		/// <exception cref="Exception">If unable to create token.</exception>
		public async Task<string> GetJwtTokenAsync(string Address, int Seconds)
		{
			TaskCompletionSource<string> Result = new TaskCompletionSource<string>();

			await this.GetJwtToken(Address, Seconds, (Sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(e.Token);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to get token."));

				return Task.CompletedTask;
			}, null);

			return await Result.Task;
		}
	}
}
