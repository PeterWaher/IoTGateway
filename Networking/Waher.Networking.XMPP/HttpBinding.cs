//#define ECHO
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Events;

namespace Waher.Networking.XMPP
{
	/// <summary>
	/// Implements a HTTP Binding mechanism based on BOSH.
	/// 
	/// XEP-0124: Bidirectional-streams Over Synchronous HTTP (BOSH):
	///	https://xmpp.org/extensions/xep-0124.html
	///	
	/// XEP-0206: XMPP Over BOSH:
	/// https://xmpp.org/extensions/xep-0206.html
	/// </summary>
	public class HttpBinding : ITextTransportLayer
	{
		/// <summary>
		/// http://jabber.org/protocol/httpbind
		/// </summary>
		public const string HttpBindNamespace = "http://jabber.org/protocol/httpbind";

		/// <summary>
		/// urn:xmpp:xbosh
		/// </summary>
		public const string BoshNamespace = "urn:xmpp:xbosh";

		private LinkedList<KeyValuePair<string, EventHandler>> outputQueue = new LinkedList<KeyValuePair<string, EventHandler>>();
		private HttpClient[] httpClients;
		private bool[] active;
		private XmppClient xmppClient;
		private string url;
		private string sid = null;
		private string accept = null;
		private string charsets = null;
		private string to;
		private string from;
		private double version = 0;
		private long rid;
		private int waitSeconds = 30;
		private int pollingSeconds = 5;
		private int inactivitySeconds = 15;
		private int maxPauseSeconds = 90;
		private int requests = 1;
		private int hold = 3;
		private bool disposed = false;
		private bool terminated = false;
		private bool restartLogic = false;

		/// <summary>
		/// Implements a HTTP Binding mechanism based on BOSH.
		/// </summary>
		/// <param name="Url">URL to remote HTTP endpoint.</param>
		/// <param name="Client">XMPP Client.</param>
		public HttpBinding(string Url, XmppClient Client)
		{
			this.url = Url;
			this.xmppClient = Client;

			this.httpClients = new HttpClient[]
			{
				new HttpClient(new HttpClientHandler()
				{
#if !NETFW
					ServerCertificateCustomValidationCallback = this.RemoteCertificateValidationCallback,
#endif
					UseCookies = false
				})
				{
					Timeout = TimeSpan.FromMilliseconds(30000)
				}
			};

			this.httpClients[0].DefaultRequestHeaders.ExpectContinue = false;
			this.active = new bool[] { false };

			this.rid = BitConverter.ToUInt32(XmppClient.GetRandomBytes(4), 0) + 1;
		}

		private bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			if (sslPolicyErrors != SslPolicyErrors.None)
				return this.xmppClient.TrustServer;

			return true;
		}

		/// <summary>
		/// Event raised when a packet has been sent.
		/// </summary>
		public event TextEventHandler OnSent;

		/// <summary>
		/// Event received when text data has been received.
		/// </summary>
		public event TextEventHandler OnReceived;

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting
		/// unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			this.disposed = true;
			this.xmppClient = null;

			if (this.httpClients != null)
			{
				foreach (HttpClient Client in this.httpClients)
					Client.Dispose();

				this.httpClients = null;
			}
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
		/// Creates a BOSH session.
		/// </summary>
		public async void CreateSession()
		{
			try
			{
				int i, c;

				lock (this.httpClients)
				{
					this.terminated = false;
					this.outputQueue.Clear();

					c = this.httpClients.Length;

					for (i = 0; i < c; i++)
					{
						if (this.active[i] && this.httpClients[i] != null)
						{
							this.httpClients[i].Dispose();
							this.httpClients[i] = new HttpClient(new HttpClientHandler()
							{
#if !NETFW
								ServerCertificateCustomValidationCallback = this.RemoteCertificateValidationCallback,
#endif
								UseCookies = false
							})
							{
								Timeout = TimeSpan.FromMilliseconds(30000)
							};

							this.httpClients[i].DefaultRequestHeaders.ExpectContinue = false;
							this.active[i] = false;
						}
					}
				}

				StringBuilder Xml = new StringBuilder();

				Xml.Append("<body content='text/xml; charset=utf-8' from='");
				Xml.Append(XML.Encode(this.xmppClient.BareJID));
				Xml.Append("' hold='1' rid='");
				Xml.Append((this.rid++).ToString());
				Xml.Append("' to='");
				Xml.Append(XML.Encode(this.xmppClient.Domain));
#if ECHO
				Xml.Append("' echo='0");
#endif
				Xml.Append("' ver='1.11' wait='30' xml:lang='");
				Xml.Append(XML.Encode(this.xmppClient.Language));
				Xml.Append("' xmpp:version='1.0' xmlns='");
				Xml.Append(HttpBindNamespace);
				Xml.Append("' xmlns:xmpp='");
				Xml.Append(BoshNamespace);
				Xml.Append("'/>");

				string s = Xml.ToString();

				if (this.xmppClient.HasSniffers)
				{
					this.xmppClient.Information("Initiating session.");
					this.xmppClient.TransmitText(s);
				}

				HttpContent Content = new StringContent(s, System.Text.Encoding.UTF8, "text/xml");
				XmlDocument ResponseXml;

				this.xmppClient.NextPing = DateTime.Now.AddMinutes(1);

				HttpResponseMessage Response = await this.httpClients[0].PostAsync(this.url, Content);
				Response.EnsureSuccessStatusCode();

				Stream Stream = await Response.Content.ReadAsStreamAsync(); // Regardless of status code, we check for XML content.
				XmlElement Body;

				byte[] Bin = await Response.Content.ReadAsByteArrayAsync();
				string CharSet = Response.Content.Headers.ContentType.CharSet;
				Encoding Encoding;

				if (string.IsNullOrEmpty(CharSet))
					Encoding = Encoding.UTF8;
				else
					Encoding = System.Text.Encoding.GetEncoding(CharSet);

				string XmlResponse = Encoding.GetString(Bin);

				if (this.xmppClient.HasSniffers)
					this.xmppClient.ReceiveText(XmlResponse);

				ResponseXml = new XmlDocument();
				ResponseXml.LoadXml(XmlResponse);

				if ((Body = ResponseXml.DocumentElement) == null || Body.LocalName != "body" ||
					Body.NamespaceURI != HttpBindNamespace)
				{
					throw new Exception("Unexpected response returned.");
				}

				this.sid = null;

				foreach (XmlAttribute Attr in Body.Attributes)
				{
					switch (Attr.Name)
					{
						case "sid":
							this.sid = Attr.Value;
							break;

						case "wait":
							if (!int.TryParse(Attr.Value, out this.waitSeconds))
								throw new Exception("Invalid wait period.");
							break;

						case "requests":
							if (!int.TryParse(Attr.Value, out this.requests))
								throw new Exception("Invalid number of requests.");
							break;

						case "ver":
							if (!CommonTypes.TryParse(Attr.Value, out this.version))
								throw new Exception("Invalid version number.");
							break;

						case "polling":
							if (!int.TryParse(Attr.Value, out this.pollingSeconds))
								throw new Exception("Invalid polling period.");
							break;

						case "inactivity":
							if (!int.TryParse(Attr.Value, out this.inactivitySeconds))
								throw new Exception("Invalid inactivity period.");
							break;

						case "maxpause":
							if (!int.TryParse(Attr.Value, out this.maxPauseSeconds))
								throw new Exception("Invalid maximum pause period.");
							break;

						case "hold":
							if (!int.TryParse(Attr.Value, out this.hold))
								throw new Exception("Invalid maximum number of requests.");
							break;

						case "to":
							this.to = Attr.Value;
							break;

						case "from":
							this.from = Attr.Value;
							break;

						case "accept":
							this.accept = Attr.Value;
							break;

						case "charsets":
							this.charsets = Attr.Value;
							break;

						case "ack":
							if (!long.TryParse(Attr.Value, out long l) ||
								l != this.rid)
							{
								throw new Exception("Response acknowledgement invalid.");
							}
							break;

						default:
							switch (Attr.LocalName)
							{
								case "restartlogic":
									if (Attr.NamespaceURI == BoshNamespace &&
										CommonTypes.TryParse(Attr.Value, out bool b))
									{
										this.restartLogic = true;
									}
									break;
							}
							break;
					}
				}

				if (string.IsNullOrEmpty(this.sid))
					throw new Exception("Session not granted.");

				if (this.requests > 1)
				{
					Array.Resize<HttpClient>(ref this.httpClients, this.requests);
					Array.Resize<bool>(ref this.active, this.requests);
				}

				for (i = 0; i < this.requests; i++)
				{
					if (this.httpClients[i] != null)
					{
						if (this.active[i])
						{
							this.httpClients[i].Dispose();
							this.httpClients[i] = null;
						}
						else
							continue;
					}

					this.httpClients[i] = new HttpClient(new HttpClientHandler()
					{
#if !NETFW
						ServerCertificateCustomValidationCallback = this.RemoteCertificateValidationCallback,
#endif
						UseCookies = false
					})
					{
						Timeout = TimeSpan.FromMilliseconds(30000)
					};

					this.httpClients[i].DefaultRequestHeaders.ExpectContinue = false;
					this.active[i] = false;
				}

				this.BodyReceived(XmlResponse, true);
			}
			catch (Exception ex)
			{
				this.xmppClient.ConnectionError(ex);
			}
		}

		/// <summary>
		/// Sends a text packet.
		/// </summary>
		/// <param name="Packet">Text packet.</param>
		public void Send(string Packet)
		{
			this.Send(Packet, null);
		}

		/// <summary>
		/// Sends a text packet.
		/// </summary>
		/// <param name="Packet">Text packet.</param>
		/// <param name="DeliveryCallback">Optional method to call when packet has been delivered.</param>
		public async void Send(string Packet, EventHandler DeliveryCallback)
		{
			if (this.terminated)
				return;

			try
			{
				LinkedList<KeyValuePair<string, EventHandler>> Queued = null;
				StringBuilder Xml;
				long Rid;
				int ClientIndex = -1;
				bool AllInactive;
				bool Restart = false;
				int i;

				if (Packet.StartsWith("<?"))
				{
					Packet = null;
					Restart = true;
				}

				do
				{
					lock (this.httpClients)
					{
						if (Queued == null)
						{
							for (ClientIndex = 0; ClientIndex < this.requests; ClientIndex++)
							{
								if (!this.active[ClientIndex])
									break;
							}

							if (ClientIndex >= this.requests)
							{
								if (!string.IsNullOrEmpty(Packet))
								{
									this.xmppClient?.Information("Outbound stanza queued.");
									this.outputQueue.AddLast(new KeyValuePair<string, EventHandler>(Packet, DeliveryCallback));
								}

								return;
							}

							this.active[ClientIndex] = true;

							if (this.outputQueue.First != null)
							{
								Queued = this.outputQueue;
								this.outputQueue = new LinkedList<KeyValuePair<string, EventHandler>>();
							}
						}

						Rid = this.rid++;
					}

					Xml = new StringBuilder();

					Xml.Append("<body rid='");
					Xml.Append(Rid.ToString());
					Xml.Append("' sid='");
					Xml.Append(this.sid);
#if ECHO
					Xml.Append("' echo='");                 
					Xml.Append(ClientIndex.ToString());
#endif
					Xml.Append("' xmlns='");
					Xml.Append(HttpBindNamespace);

					if (Restart)
					{
						Xml.Append("' xmpp:restart='true' xmlns:xmpp='");
						Xml.Append(BoshNamespace);
					}

					Xml.Append("'>");

					if (Queued != null)
					{
						foreach (KeyValuePair<string, EventHandler> P in Queued)
						{
							this.RaiseOnSent(P.Key);
							Xml.Append(P.Key);
						}
					}

					if (Packet != null)
					{
						this.RaiseOnSent(Packet);
						Xml.Append(Packet);

						if (DeliveryCallback != null)
						{
							try
							{
								DeliveryCallback(this, new EventArgs());
							}
							catch (Exception ex)
							{
								Log.Critical(ex);
							}
						}

						Packet = null;
						DeliveryCallback = null;
					}

					Xml.Append("</body>");

					string s = Xml.ToString();

					if (this.xmppClient.HasSniffers)
						this.xmppClient.TransmitText(s);

					HttpContent Content = new StringContent(s, System.Text.Encoding.UTF8, "text/xml");

					this.xmppClient.NextPing = DateTime.Now.AddSeconds(this.waitSeconds + 5);

					HttpResponseMessage Response = await this.httpClients[ClientIndex].PostAsync(this.url, Content);
					Response.EnsureSuccessStatusCode();

					lock (this.httpClients)
					{
						AllInactive = true;

						if (this.outputQueue.First != null)
						{
							Queued = this.outputQueue;
							this.outputQueue = new LinkedList<KeyValuePair<string, EventHandler>>();
						}
						else
						{
							Queued = null;
							this.active[ClientIndex] = false;

							for (i = 0; i < this.requests; i++)
							{
								if (this.active[i])
								{
									AllInactive = false;
									break;
								}
							}
						}
					}

					Stream Stream = await Response.Content.ReadAsStreamAsync(); // Regardless of status code, we check for XML content.

					byte[] Bin = await Response.Content.ReadAsByteArrayAsync();
					string CharSet = Response.Content.Headers.ContentType.CharSet;
					Encoding Encoding;

					if (string.IsNullOrEmpty(CharSet))
						Encoding = Encoding.UTF8;
					else
						Encoding = System.Text.Encoding.GetEncoding(CharSet);

					string XmlResponse = Encoding.GetString(Bin).Trim();

					if (this.xmppClient.HasSniffers)
						this.xmppClient.ReceiveText(XmlResponse);

					this.BodyReceived(XmlResponse, false);
				}
				while (!this.disposed && (Queued != null || (AllInactive && this.xmppClient.State == XmppState.Connected)));
			}
			catch (Exception ex)
			{
				this.xmppClient?.ConnectionError(ex);
			}
		}

		private void BodyReceived(string Xml, bool First)
		{
			string Body;
			int i, j;

			i = Xml.IndexOf("<body");
			if (i >= 0)
			{
				i = Xml.IndexOf('>', i + 5);
				if (i > 0)
				{
					Body = Xml.Substring(0, i + 1);

					if (Xml[i - 1] == '/')
						Xml = null;
					else
					{
						Body += "</body>";

						j = Xml.LastIndexOf("</body");
						if (i < 0)
							Xml = Xml.Substring(i + 1);
						else
							Xml = Xml.Substring(i + 1, j - i - 1).Trim();
					}

					bool Terminate = false;
					string Condition = null;
					string To = null;
					string From = null;
					string Version = null;
					string Language = null;
					string StreamPrefix = "stream";
					XmlDocument BodyDoc = new XmlDocument();
					LinkedList<KeyValuePair<string, string>> Namespaces = null;
					BodyDoc.LoadXml(Body);

					foreach (XmlAttribute Attr in BodyDoc.DocumentElement.Attributes)
					{
						switch (Attr.Name)
						{
							case "type":
								if (Attr.Value == "terminate")
									Terminate = true;
								break;

							case "condition":
								Condition = Attr.Value;
								break;

							case "xml:lang":
								Language = Attr.Value;
								break;

							default:
								if (Attr.Prefix == "xmlns")
								{
									if (Attr.Value == XmppClient.NamespaceStream)
										StreamPrefix = Attr.LocalName;
									else
									{
										if (Namespaces == null)
											Namespaces = new LinkedList<KeyValuePair<string, string>>();

										Namespaces.AddLast(new KeyValuePair<string, string>(Attr.Prefix, Attr.Value));
									}
								}
								else if (Attr.LocalName == "version" && Attr.NamespaceURI == BoshNamespace)
									Version = Attr.Value;
								break;
						}
					}

					if (Terminate)
					{
						this.terminated = true;

						lock (this.httpClients)
						{
							this.outputQueue.Clear();
						}

						throw new Exception("Session terminated. Condition: " + Condition);
					}

					if (First)
					{
						First = false;

						StringBuilder sb = new StringBuilder();

						sb.Append('<');
						sb.Append(StreamPrefix);
						sb.Append(":stream xmlns:");
						sb.Append(StreamPrefix);
						sb.Append("='");
						sb.Append(XmppClient.NamespaceStream);

						if (To != null)
						{
							sb.Append("' to='");
							sb.Append(XML.Encode(To));
						}

						if (From != null)
						{
							sb.Append("' from='");
							sb.Append(XML.Encode(From));
						}

						if (Version != null)
						{
							sb.Append("' version='");
							sb.Append(XML.Encode(Version));
						}

						if (Language != null)
						{
							sb.Append("' xml:lang='");
							sb.Append(XML.Encode(From));
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

						this.xmppClient.StreamHeader = sb.ToString();

						sb.Clear();
						sb.Append("</");
						sb.Append(StreamPrefix);
						sb.Append(":stream>");

						this.xmppClient.StreamFooter = sb.ToString();
					}
				}
			}

			if (Xml != null)
				this.RaiseOnReceived(Xml);
		}
	}
}
