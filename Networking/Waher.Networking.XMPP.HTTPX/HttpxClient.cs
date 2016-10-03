using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Events;
using Waher.Networking.HTTP;
using Waher.Runtime.Cache;

namespace Waher.Networking.XMPP.HTTPX
{
	/// <summary>
	/// HTTPX client.
	/// </summary>
	public class HttpxClient : IDisposable
	{
		public const string Namespace = "urn:xmpp:http";
		public const string NamespaceHeaders = "http://jabber.org/protocol/shim";

		private XmppClient client;
		private IEndToEndEncryption e2e;
		private int maxChunkSize;

		/// <summary>
		/// HTTPX client.
		/// </summary>
		/// <param name="Client">XMPP Client.</param>
		/// <param name="MaxChunkSize">Max Chunk Size to use.</param>
		public HttpxClient(XmppClient Client, int MaxChunkSize)
		{
			this.client = Client;
			this.e2e = null;
			this.maxChunkSize = MaxChunkSize;

			HttpxChunks.RegisterChunkReceiver(this.client);
		}

		/// <summary>
		/// HTTPX client.
		/// </summary>
		/// <param name="Client">XMPP Client.</param>
		/// <param name="E2e">End-to-end encryption interface.</param>
		/// <param name="MaxChunkSize">Max Chunk Size to use.</param>
		public HttpxClient(XmppClient Client, IEndToEndEncryption E2e, int MaxChunkSize)
		{
			this.client = Client;
			this.e2e = E2e;
			this.maxChunkSize = MaxChunkSize;

			HttpxChunks.RegisterChunkReceiver(this.client);
		}

		/// <summary>
		/// Optional end-to-end encryption interface to use in requests.
		/// </summary>
		public IEndToEndEncryption E2e
		{
			get { return this.e2e; }
			set { this.e2e = value; }
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			HttpxChunks.UnregisterChunkReceiver(this.client);
		}

		/// <summary>
		/// Performs a HTTP GET request.
		/// </summary>
		/// <param name="To">Full JID of entity to query.</param>
		/// <param name="Resource">Local HTTP resource to query.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="Headers">HTTP headers of the request.</param>
		public void GET(string To, string Resource, HttpxResponseEventHandler Callback,
			HttpxResponseDataEventHandler DataCallback, object State, params HttpField[] Headers)
		{
			this.Request(To, "GET", Resource, Callback, DataCallback, State, Headers);
		}

		// TODO: Add more HTTP methods.

		/// <summary>
		/// Performs a HTTP request.
		/// </summary>
		/// <param name="To">Full JID of entity to query.</param>
		/// <param name="Method">HTTP Method.</param>
		/// <param name="LocalResource">Local HTTP resource to query.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="Headers">HTTP headers of the request.</param>
		public void Request(string To, string Method, string LocalResource, HttpxResponseEventHandler Callback,
			HttpxResponseDataEventHandler DataCallback, object State, params HttpField[] Headers)
		{
			this.Request(To, Method, LocalResource, 1.1, Headers, null, Callback, DataCallback, State);
		}

		/// <summary>
		/// Performs a HTTP request.
		/// </summary>
		/// <param name="To">Full JID of entity to query.</param>
		/// <param name="Method">HTTP Method.</param>
		/// <param name="LocalResource">Local resource.</param>
		/// <param name="HttpVersion">HTTP Version.</param>
		/// <param name="Headers">HTTP headers.</param>
		/// <param name="DataStream">Data Stream, if any, or null, if no data is sent.</param>
		/// <param name="DataCallback">Local resource.</param>
		/// <param name="Request">HTTP Request.</param>
		/// <param name="Callback">Callback method to call when response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void Request(string To, string Method, string LocalResource, double HttpVersion, IEnumerable<HttpField> Headers,
			Stream DataStream, HttpxResponseEventHandler Callback, HttpxResponseDataEventHandler DataCallback, object State)
		{
			// TODO: Local IP & port for quick P2P response (TLS, or POST back, web hook).

			StringBuilder Xml = new StringBuilder();

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
			Xml.Append("' sipub='false' ibb='false' jingle='false'>");

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

			if (DataStream != null)
			{
				if (DataStream.Length < this.maxChunkSize)
				{
					int c = (int)DataStream.Length;
					byte[] Data = new byte[c];

					DataStream.Position = 0;
					DataStream.Read(Data, 0, c);

					Xml.Append("<data><base64>");
					Xml.Append(Convert.ToBase64String(Data, Base64FormattingOptions.None));
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

			if (this.e2e != null)
				this.e2e.SendIqSet(this.client, E2ETransmission.NormalIfNotE2E, To, Xml.ToString(), this.ResponseHandler, new object[] { Callback, DataCallback, State }, 60000, 0);
			else
				this.client.SendIqSet(To, Xml.ToString(), this.ResponseHandler, new object[] { Callback, DataCallback, State }, 60000, 0);

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

					DataStream.Read(Data, 0, i);

					Xml.Clear();
					Xml.Append("<chunk xmlns='");
					Xml.Append(Namespace);
					Xml.Append("' streamId='");
					Xml.Append(StreamId);
					Xml.Append("' nr='");
					Xml.Append(Nr.ToString());
					Xml.Append("'>");
					Xml.Append(Convert.ToBase64String(Data, 0, i, Base64FormattingOptions.None));
					Xml.Append("</chunk>");
					Nr++;

					if (this.e2e != null)
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

		private void ResponseHandler(object Sender, IqResultEventArgs e)
		{
			XmlElement E = e.FirstElement;
			HttpResponse Response;
			string StatusMessage;
			double Version;
			int StatusCode;
			object[] P = (object[])e.State;
			HttpxResponseEventHandler Callback = (HttpxResponseEventHandler)P[0];
			HttpxResponseDataEventHandler DataCallback = (HttpxResponseDataEventHandler)P[1];
			object State = P[2];
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
										byte[] Data = Response.Encoding.GetBytes(N2.InnerText);
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

										HttpxChunks.chunkedStreams.Add(e.From + " " + StreamId, new ClientChunkRecord(this,
											new HttpxResponseEventArgs(e, Response, State, Version, StatusCode, StatusMessage, true),
											Response, DataCallback, State, StreamId));

										DisposeResponse = false;
										break;

									case "sipub":
										// TODO: Implement File Transfer support.
										break;

									case "ibb":
										// TODO: Implement In-band byte streams support.
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
				StatusCode = 505;
				StatusMessage = "HTTP Version Not Supported";
				Response = new HttpResponse();
			}

			HttpxResponseEventArgs e2 = new HttpxResponseEventArgs(e, Response, State, Version, StatusCode, StatusMessage, HasData);

			try
			{
				Callback(this, e2);
			}
			catch (Exception)
			{
				// Ignore.
			}
			finally
			{
				if (DisposeResponse)
					Response.Dispose();
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

			if (this.e2e != null)
			{
				this.e2e.SendMessage(this.client, E2ETransmission.NormalIfNotE2E, QoSLevel.Unacknowledged,
					MessageType.Normal, string.Empty, To, Xml.ToString(), string.Empty, string.Empty, string.Empty, 
					string.Empty, string.Empty, null, null);
			}
			else
				this.client.SendMessage(MessageType.Normal, To, Xml.ToString(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
		}
	}
}
