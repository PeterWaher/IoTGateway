using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Runtime.Inventory;
using Waher.Security;

namespace Waher.Networking.XMPP.WebSocket
{
	/// <summary>
	/// Implements a Web-socket XMPP protocol, as defined in RFC 7395.
	/// https://tools.ietf.org/html/rfc7395
	/// </summary>
	public class WebSocketBinding : AlternativeTransport
	{
		/// <summary>
		/// urn:ietf:params:xml:ns:xmpp-framing
		/// </summary>
		public const string FramingNamespace = "urn:ietf:params:xml:ns:xmpp-framing";

		private readonly LinkedList<KeyValuePair<string, EventHandler>> queue = new LinkedList<KeyValuePair<string, EventHandler>>();
		private XmppClient xmppClient;
		private XmppBindingInterface bindingInterface;
		private ClientWebSocket webSocketClient;
		private ArraySegment<byte> inputBuffer;
		private Uri url;
		private string to;
		private string from;
		private string language;
		private double version = 0;
		private bool terminated = false;
		private bool writing = false;
		private bool closeSent = false;
		private bool disposed = false;

		/// <summary>
		/// Implements a Web-socket XMPP protocol, as defined in RFC 7395.
		/// https://tools.ietf.org/html/rfc7395
		/// </summary>
		public WebSocketBinding()
		{
		}

		/// <summary>
		/// If the alternative binding mechanism handles heartbeats.
		/// </summary>
		public override bool HandlesHeartbeats => true;

		/// <summary>
		/// How well the alternative transport handles the XMPP credentials provided.
		/// </summary>
		/// <param name="URI">URI defining endpoint.</param>
		/// <returns>Support grade.</returns>
		public override Grade Handles(Uri URI)
		{
			switch (URI.Scheme.ToLower())
			{
				case "ws":
				case "wss":
					return Grade.Ok;

				default:
					return Grade.NotAtAll;
			}
		}

		/// <summary>
		/// Instantiates a new alternative connections.
		/// </summary>
		/// <param name="URI">URI defining endpoint.</param>
		/// <param name="Client">XMPP Client</param>
		/// <param name="BindingInterface">Inteface to internal properties of the <see cref="XmppClient"/>.</param>
		/// <returns>Instantiated binding.</returns>
		public override IAlternativeTransport Instantiate(Uri URI, XmppClient Client, XmppBindingInterface BindingInterface)
		{
			return new WebSocketBinding()
			{
				bindingInterface = BindingInterface,
				url = URI,
				xmppClient = Client,
				inputBuffer = new ArraySegment<byte>(new byte[65536])
			};
		}

		/// <summary>
		/// Event raised when a packet has been sent.
		/// </summary>
		public override event TextEventHandler OnSent;

		/// <summary>
		/// Event received when text data has been received.
		/// </summary>
		public override event TextEventHandler OnReceived;

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting
		/// unmanaged resources.
		/// </summary>
		public override void Dispose()
		{
			this.disposed = true;
			this.terminated = true;
			this.xmppClient = null;

			this.webSocketClient?.Dispose();
			this.webSocketClient = null;
		}

		private void RaiseOnSent(string Payload)
		{
			TextEventHandler h = this.OnSent;

			if (h != null)
			{
				try
				{
					h(this, Payload);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		private void RaiseOnReceived(string Payload)
		{
			TextEventHandler h = this.OnReceived;

			if (h != null)
			{
				try
				{
					h(this, Payload);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		/// <summary>
		/// Creates a Web-socket session.
		/// </summary>
		public override async void CreateSession()
		{
			try
			{
				lock (this.queue)
				{
					this.terminated = false;
					this.writing = false;
					this.closeSent = false;
					this.queue.Clear();

					this.webSocketClient?.Dispose();
					this.webSocketClient = null;

					this.webSocketClient = new ClientWebSocket();
					this.webSocketClient.Options.KeepAliveInterval = TimeSpan.FromSeconds(30);
				}

				await this.webSocketClient.ConnectAsync(this.url, CancellationToken.None);

				// TODO: this.xmppClient.TrustServer

				if (this.xmppClient.HasSniffers)
					this.xmppClient.Information("Initiating session.");

				this.Send("<?");

				string XmlResponse = await this.ReadText();

				XmlDocument ResponseXml = new XmlDocument();
				ResponseXml.LoadXml(XmlResponse);

				XmlElement Open;

				if ((Open = ResponseXml.DocumentElement) is null || Open.LocalName != "open" ||
					Open.NamespaceURI != FramingNamespace)
				{
					throw new Exception("Unexpected response returned.");
				}

				string StreamPrefix = "stream";
				LinkedList<KeyValuePair<string, string>> Namespaces = null;

				this.to = null;
				this.from = null;
				this.version = 0;
				this.language = null;

				foreach (XmlAttribute Attr in Open.Attributes)
				{
					switch (Attr.Name)
					{
						case "version":
							if (!CommonTypes.TryParse(Attr.Value, out this.version))
								throw new Exception("Invalid version number.");
							break;

						case "to":
							this.to = Attr.Value;
							break;

						case "from":
							this.from = Attr.Value;
							break;

						case "xml:lang":
							this.language = Attr.Value;
							break;

						default:
							if (Attr.Prefix == "xmlns")
							{
								if (Attr.Value == XmppClient.NamespaceStream)
									StreamPrefix = Attr.LocalName;
								else
								{
									if (Namespaces is null)
										Namespaces = new LinkedList<KeyValuePair<string, string>>();

									Namespaces.AddLast(new KeyValuePair<string, string>(Attr.Prefix, Attr.Value));
								}
							}
							break;
					}
				}

				StringBuilder sb = new StringBuilder();

				sb.Append('<');
				sb.Append(StreamPrefix);
				sb.Append(":stream xmlns:");
				sb.Append(StreamPrefix);
				sb.Append("='");
				sb.Append(XmppClient.NamespaceStream);

				if (this.to != null)
				{
					sb.Append("' to='");
					sb.Append(XML.Encode(this.to));
				}

				if (this.from != null)
				{
					sb.Append("' from='");
					sb.Append(XML.Encode(this.from));
				}

				if (this.version > 0)
				{
					sb.Append("' version='");
					sb.Append(CommonTypes.Encode(this.version));
				}

				if (this.language != null)
				{
					sb.Append("' xml:lang='");
					sb.Append(XML.Encode(this.language));
				}

				sb.Append("' xmlns='");
				sb.Append(XmppClient.NamespaceClient);

				if (Namespaces != null)
				{
					foreach (KeyValuePair<string, string> P in Namespaces)
					{
						sb.Append("' xmlns:");
						sb.Append(P.Key);
						sb.Append("='");
						sb.Append(XML.Encode(P.Value));
					}
				}

				sb.Append("'>");

				this.bindingInterface.StreamHeader = sb.ToString();

				sb.Clear();
				sb.Append("</");
				sb.Append(StreamPrefix);
				sb.Append(":stream>");

				this.bindingInterface.StreamFooter = sb.ToString();

				this.StartReading();
			}
			catch (Exception ex)
			{
				this.bindingInterface.ConnectionError(ex);
			}
		}

		private async void StartReading()
		{
			try
			{
				while (!this.terminated)
				{
					string Xml = await this.ReadText();

					if (!this.FragmentReceived(Xml))
						break;
				}
			}
			catch (Exception ex)
			{
				this.bindingInterface.ConnectionError(ex);
			}
		}

		/// <summary>
		/// Closes a session.
		/// </summary>
		public override void CloseSession()
		{
			if (this.webSocketClient is null)
				this.terminated = true;
			else
			{
				try
				{
					if (!this.closeSent && this.webSocketClient.State == WebSocketState.Open)
					{
						this.closeSent = true;

						this.Send("<close xmlns=\"urn:ietf:params:xml:ns:xmpp-framing\"/>", async (sender, e) =>
						{
							this.terminated = true;

							try
							{
								if (this.webSocketClient.State == WebSocketState.Open)
									await this.webSocketClient.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);

								this.webSocketClient.Dispose();
								this.webSocketClient = null;
							}
							catch (Exception)
							{
								this.webSocketClient = null;
							}
						});
					}
					else
					{
						this.terminated = true;

						this.webSocketClient.Dispose();
						this.webSocketClient = null;
					}
				}
				catch (Exception)
				{
					// Ignore.
				}
			}
		}

		private async Task<string> ReadText()
		{
			WebSocketReceiveResult Response = await this.webSocketClient?.ReceiveAsync(this.inputBuffer, CancellationToken.None);
			if (Response is null)
				return string.Empty;

			this.AssureText(Response);

			int Count = Response.Count;
			if (Count == 0)
				return string.Empty;

			string s = Encoding.UTF8.GetString(this.inputBuffer.Array, 0, Count);

			if (this.xmppClient.HasSniffers)
				this.xmppClient.ReceiveText(s);

			if (Response.EndOfMessage)
				return s;

			StringBuilder sb = new StringBuilder(s);

			do
			{
				Response = await this.webSocketClient.ReceiveAsync(this.inputBuffer, CancellationToken.None);
				this.AssureText(Response);

				Count = Response.Count;
				s = Encoding.UTF8.GetString(this.inputBuffer.Array, 0, Count);
				sb.Append(s);

				if (this.xmppClient.HasSniffers)
					this.xmppClient.ReceiveText(s);
			}
			while (!Response.EndOfMessage && !this.disposed);

			return sb.ToString();
		}

		private void AssureText(WebSocketReceiveResult Response)
		{
			if (Response.CloseStatus.HasValue)
				throw new Exception("Web-socket connection closed. Code: " + Response.CloseStatus.Value.ToString() + ", Description: " + Response.CloseStatusDescription);

			if (Response.MessageType != WebSocketMessageType.Text)
				throw new Exception("Expected text.");
		}

		/// <summary>
		/// Sends a text packet.
		/// </summary>
		/// <param name="Packet">Text packet.</param>
		public override void Send(string Packet)
		{
			this.Send(Packet, null);
		}

		/// <summary>
		/// Sends a text packet.
		/// </summary>
		/// <param name="Packet">Text packet.</param>
		/// <param name="DeliveryCallback">Optional method to call when packet has been delivered.</param>
		public override async void Send(string Packet, EventHandler DeliveryCallback)
		{
			if (this.terminated)
				return;

			if (Packet is null)
				throw new ArgumentException("Null payloads not allowed.", nameof(Packet));

			if (Packet.StartsWith("<?"))
			{
				StringBuilder Xml = new StringBuilder();

				Xml.Append("<open xmlns=\"");
				Xml.Append(FramingNamespace);
				Xml.Append("\" to=\"");
				Xml.Append(this.xmppClient.Domain);
				Xml.Append("\" xml:lang=\"en\" version=\"1.0\"/>");

				Packet = Xml.ToString();
			}

			lock (this.queue)
			{
				if (this.writing)
				{
					this.xmppClient?.Information("Outbound stanza queued.");
					this.queue.AddLast(new KeyValuePair<string, EventHandler>(Packet, DeliveryCallback));
					return;
				}
				else
					this.writing = true;
			}

			try
			{
				bool HasSniffers = this.xmppClient.HasSniffers;

				while (Packet != null && !this.disposed)
				{
					if (HasSniffers)
						this.xmppClient?.TransmitText(Packet);

					ArraySegment<byte> Buffer = new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(Packet));
					await this.webSocketClient?.SendAsync(Buffer, WebSocketMessageType.Text, true, CancellationToken.None);

					this.bindingInterface.NextPing = DateTime.Now.AddMinutes(1);

					if (DeliveryCallback != null)
					{
						try
						{
							DeliveryCallback(this.xmppClient, new EventArgs());
						}
						catch (Exception ex)
						{
							Log.Critical(ex);
						}
					}

					lock (this.queue)
					{
						if (this.queue.First != null)
						{
							LinkedListNode<KeyValuePair<string, EventHandler>> Node = this.queue.First;
							Packet = Node.Value.Key;
							DeliveryCallback = Node.Value.Value;
							this.queue.RemoveFirst();
						}
						else
						{
							Packet = null;
							DeliveryCallback = null;
							this.writing = false;
						}
					}
				}
			}
			catch (Exception ex)
			{
				lock (this.queue)
				{
					this.writing = false;
					this.queue.Clear();
				}

				this.bindingInterface.ConnectionError(ex);
			}
		}

		private bool FragmentReceived(string Xml)
		{
			if (this.terminated)
				return false;

			if (Xml.StartsWith("<close"))
			{
				XmlDocument Doc = new XmlDocument();
				Doc.LoadXml(Xml);

				if (Doc.DocumentElement != null && Doc.DocumentElement.LocalName == "close" && Doc.DocumentElement.NamespaceURI == FramingNamespace)
				{
					if (!this.closeSent)
						this.CloseSession();

					return false;
				}
			}

			this.RaiseOnReceived(Xml);

			return true;
		}
	}
}
