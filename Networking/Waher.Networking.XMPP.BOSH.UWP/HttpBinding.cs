﻿//#define ECHO
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Content.Xml.Text;
using Waher.Events;
using Waher.Runtime.Inventory;
using Waher.Security;

namespace Waher.Networking.XMPP.BOSH
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
	public class HttpBinding : AlternativeTransport
	{
		/// <summary>
		/// http://jabber.org/protocol/httpbind
		/// </summary>
		public const string HttpBindNamespace = "http://jabber.org/protocol/httpbind";

		/// <summary>
		/// urn:xmpp:xbosh
		/// </summary>
		public const string BoshNamespace = "urn:xmpp:xbosh";

		private LinkedList<OutputRec> outputQueue = new LinkedList<OutputRec>();
		private readonly LinkedList<string> keys = new LinkedList<string>();
		private HttpClient[] httpClients;
		private XmppBindingInterface bindingInterface;
		private bool[] active;
		private XmppClient xmppClient;
		private Uri url;
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

		private class OutputRec
		{
			public string Packet;
			public EventHandlerAsync<DeliveryEventArgs> DeliveryCallback;
			public object State;
		}

		/// <summary>
		/// Implements a HTTP Binding mechanism based on BOSH.
		/// </summary>
		public HttpBinding()
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
		public override Grade Supports(Uri URI)
		{
			switch (URI.Scheme.ToLower())
			{
				case "http":
				case "https":
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
			HttpBinding Result = new HttpBinding()
			{
				bindingInterface = BindingInterface,
				url = URI,
				xmppClient = Client,
				active = new bool[1],
				httpClients = new HttpClient[1],
				rid = BitConverter.ToUInt32(XmppClient.GetRandomBytes(4), 0) + 1
			};

			return Result;
		}

		private bool RemoteCertificateValidationCallback(object Sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			if (sslPolicyErrors != SslPolicyErrors.None)
				return this.xmppClient.TrustServer;

			return true;
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
		/// Connection manager supports stream restart logic.
		/// </summary>
		public bool RestartLogic => this.restartLogic;

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting
		/// unmanaged resources.
		/// </summary>
		public override Task DisposeAsync()
		{
			this.disposed = true;
			this.xmppClient = null;
			this.bindingInterface = null;

			if (!(this.httpClients is null))
			{
				foreach (HttpClient Client in this.httpClients)
					Client?.Dispose();

				this.httpClients = Array.Empty<HttpClient>();
			}

			return Task.CompletedTask;
		}

		private async Task<bool> RaiseOnSent(string Payload)
		{
			TextEventHandler h = this.OnSent;
			bool Result = true;

			if (!(h is null))
			{
				try
				{
					Result = await h(this, Payload);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			}

			return Result;
		}

		private async Task<bool> RaiseOnReceived(string Payload)
		{
			TextEventHandler h = this.OnReceived;
			bool Result = true;

			if (!(h is null))
			{
				try
				{
					Result = await h(this, Payload);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			}

			return Result;
		}

		/// <summary>
		/// Creates a BOSH session.
		/// </summary>
		public override async void CreateSession()
		{
			try
			{
				StringBuilder Xml;
				int i, c;

				lock (this.httpClients)
				{
					this.terminated = false;
					this.outputQueue.Clear();

					c = this.httpClients.Length;

					for (i = 0; i < c; i++)
					{
						if ((this.active[i] && !(this.httpClients[i] is null)) || i == 0)
						{
							this.httpClients[i]?.Dispose();
							this.httpClients[i] = new HttpClient(new HttpClientHandler()
							{
#if !NETFW
								ServerCertificateCustomValidationCallback = this.RemoteCertificateValidationCallback,
#endif
								UseCookies = false,
								AutomaticDecompression = (DecompressionMethods)(-1)     // All
							})
							{
								Timeout = TimeSpan.FromMilliseconds(60000)
							};

							this.httpClients[i].DefaultRequestHeaders.ExpectContinue = false;
							this.active[i] = false;
						}
					}

					this.GenerateKeysLocked();

					Xml = new StringBuilder();

					Xml.Append("<body content='text/xml; charset=utf-8' from='");
					Xml.Append(XML.Encode(this.xmppClient.BareJID));
					Xml.Append("' hold='1' rid='");
					Xml.Append((this.rid++).ToString());
					Xml.Append("' to='");
					Xml.Append(XML.Encode(this.xmppClient.Domain));
					Xml.Append("' newkey='");
					Xml.Append(this.keys.First.Value);
					this.keys.RemoveFirst();
				}

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

				HttpContent Content = new StringContent(s, System.Text.Encoding.UTF8, XmlCodec.DefaultContentType);
				XmlDocument ResponseXml;

				this.bindingInterface.NextPing = DateTime.Now.AddMinutes(1);

				HttpResponseMessage Response = await this.httpClients[0].PostAsync(this.url, Content);

				if (!Response.IsSuccessStatusCode)
				{
					ContentResponse Temp = await Waher.Content.Getters.WebGetter.ProcessResponse(Response, this.url);
					await this.bindingInterface.ConnectionError(Temp.Error);
					return;
				}

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

				ResponseXml = new XmlDocument()
				{
					PreserveWhitespace = true
				};
				ResponseXml.LoadXml(XmlResponse);

				if ((Body = ResponseXml.DocumentElement) is null || Body.LocalName != "body" ||
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
					Array.Resize(ref this.httpClients, this.requests);
					Array.Resize(ref this.active, this.requests);
				}

				for (i = 0; i < this.requests; i++)
				{
					if (!(this.httpClients[i] is null))
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
						UseCookies = false,
						AutomaticDecompression = (DecompressionMethods)(-1)     // All
					})
					{
						Timeout = TimeSpan.FromMilliseconds(60000)
					};

					this.httpClients[i].DefaultRequestHeaders.ExpectContinue = false;
					this.active[i] = false;
				}

				await this.BodyReceived(XmlResponse, true);
			}
			catch (Exception ex)
			{
				await this.bindingInterface.ConnectionError(ex);
			}
		}

		/// <summary>
		/// Closes a session.
		/// </summary>
		public override void CloseSession()
		{
			this.terminated = true;
			if (!(this.httpClients is null))
			{
				int i, c = this.httpClients.Length;

				for (i = 0; i < c; i++)
				{
					this.active[i] = false;
					if (!(this.httpClients[i] is null))
					{
						this.httpClients[i].Dispose();
						this.httpClients[i] = null;
					}
				}
			}
		}

		private void GenerateKeysLocked()
		{
			int n = XmppClient.GetRandomValue(8, 24);
			byte[] Bin = XmppClient.GetRandomBytes(20);
			string Key = Hashes.ComputeSHA1HashString(Bin);

			this.keys.Clear();
			this.keys.AddFirst(Key);

			while (--n > 0)
			{
				Bin = Encoding.ASCII.GetBytes(Key);
				Key = Hashes.ComputeSHA1HashString(Bin);
				this.keys.AddFirst(Key);
			}
		}

		/// <summary>
		/// Sends a text packet.
		/// </summary>
		/// <param name="Packet">Text packet.</param>
		/// <returns>If data was sent.</returns>
		public override Task<bool> SendAsync(string Packet)
		{
			return this.SendAsync(Packet, null, null);
		}

		/// <summary>
		/// Sends a text packet.
		/// </summary>
		/// <param name="Packet">Text packet.</param>
		/// <param name="DeliveryCallback">Optional method to call when packet has been delivered.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <returns>If data was sent.</returns>
		public override Task<bool> SendAsync(string Packet, EventHandlerAsync<DeliveryEventArgs> DeliveryCallback, object State)
		{
			if (this.terminated)
				return Task.FromResult(false);

			this.Send(Packet, DeliveryCallback, State);

			return Task.FromResult(true);
		}

		private async void Send(string Packet, EventHandlerAsync<DeliveryEventArgs> DeliveryCallback, object State)
		{
			if (this.terminated)
				return;

			try
			{
				LinkedList<OutputRec> Queued = null;
				StringBuilder Xml;
				long Rid;
				int ClientIndex = -1;
				bool AllInactive = false;
				bool Restart = false;
				bool HasSniffers = this.xmppClient.HasSniffers;
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
						if (ClientIndex < 0)
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
									if (this.xmppClient?.HasSniffers ?? false)
										this.xmppClient.Information("Outbound stanza queued.");

									this.outputQueue.AddLast(new OutputRec()
									{
										Packet = Packet,
										DeliveryCallback = DeliveryCallback,
										State = State
									});
								}

								if (!(Queued is null))
								{
									LinkedListNode<OutputRec> Loop = Queued.Last;

									while (!(Loop is null))
									{
										this.outputQueue.AddFirst(Loop.Value);
										Loop = Loop.Previous;
									}
								}

								return;
							}

							this.active[ClientIndex] = true;

							if (!(this.outputQueue.First is null))
							{
								Queued = this.outputQueue;
								this.outputQueue = new LinkedList<OutputRec>();
							}
						}

						Rid = this.rid++;

						Xml = new StringBuilder();

						Xml.Append("<body rid='");
						Xml.Append(Rid.ToString());
						Xml.Append("' sid='");
						Xml.Append(this.sid);
						Xml.Append("' key='");
						Xml.Append(this.keys.First.Value);

						this.keys.RemoveFirst();
						if (this.keys.First is null)
						{
							this.GenerateKeysLocked();

							Xml.Append("' newkey='");
							Xml.Append(this.keys.First.Value);
							this.keys.RemoveFirst();
						}
					}

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

					if (!(Queued is null))
					{
						foreach (OutputRec Rec in Queued)
						{
							await this.RaiseOnSent(Rec.Packet);
							Xml.Append(Rec.Packet);

							if (!(Rec.DeliveryCallback is null))
								await Rec.DeliveryCallback.Raise(this, new DeliveryEventArgs(Rec.State, true));
						}
					}

					if (!(Packet is null))
					{
						await this.RaiseOnSent(Packet);
						Xml.Append(Packet);

						if (!(DeliveryCallback is null))
							await DeliveryCallback.Raise(this.xmppClient, new DeliveryEventArgs(State, true));

						Packet = null;
						DeliveryCallback = null;
					}

					Xml.Append("</body>");

					string s = Xml.ToString();

					if (this.disposed)
						break;

					if (HasSniffers && !(this.xmppClient is null))
						this.xmppClient.TransmitText(s);

					HttpContent Content = new StringContent(s, System.Text.Encoding.UTF8, XmlCodec.DefaultContentType);

					this.bindingInterface.NextPing = DateTime.Now.AddSeconds(this.waitSeconds + 5);

					HttpResponseMessage Response = await this.httpClients[ClientIndex].PostAsync(this.url, Content);

					if (!Response.IsSuccessStatusCode)
					{
						ContentResponse Temp = await Waher.Content.Getters.WebGetter.ProcessResponse(Response, this.url);
						await this.bindingInterface?.ConnectionError(Temp.Error);
						return;
					}

					lock (this.httpClients)
					{
						if (!(this.outputQueue.First is null))
						{
							Queued = this.outputQueue;
							this.outputQueue = new LinkedList<OutputRec>();
						}
						else
						{
							AllInactive = true;
							Queued = null;

							for (i = 0; i < this.requests; i++)
							{
								if (i != ClientIndex && this.active[i])
								{
									AllInactive = false;
									break;
								}
							}

							if (!AllInactive)
							{
								this.active[ClientIndex] = false;
								ClientIndex = -1;
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

					await this.BodyReceived(XmlResponse, false);
				}
				while (!this.disposed && (!(Queued is null) || (AllInactive && this.xmppClient.State == XmppState.Connected)));

				if (ClientIndex >= 0)
				{
					lock (this.httpClients)
					{
						this.active[ClientIndex] = false;
					}
				}
			}
			catch (TaskCanceledException)
			{
				return;
			}
			catch (Exception ex)
			{
				await this.bindingInterface?.ConnectionError(ex);
			}
		}

		private Task<bool> BodyReceived(string Xml, bool First)
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
					XmlDocument BodyDoc = new XmlDocument()
					{
						PreserveWhitespace = true
					};
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
										if (Namespaces is null)
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
						StringBuilder sb = new StringBuilder();

						sb.Append('<');
						sb.Append(StreamPrefix);
						sb.Append(":stream xmlns:");
						sb.Append(StreamPrefix);
						sb.Append("='");
						sb.Append(XmppClient.NamespaceStream);

						if (!(To is null))
						{
							sb.Append("' to='");
							sb.Append(XML.Encode(To));
						}

						if (!(From is null))
						{
							sb.Append("' from='");
							sb.Append(XML.Encode(From));
						}

						if (!(Version is null))
						{
							sb.Append("' version='");
							sb.Append(XML.Encode(Version));
						}

						if (!(Language is null))
						{
							sb.Append("' xml:lang='");
							sb.Append(XML.Encode(Language));
						}

						sb.Append("' xmlns='");
						sb.Append(XmppClient.NamespaceClient);

						if (!(Namespaces is null))
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
					}
				}
			}

			if (!(Xml is null))
				return this.RaiseOnReceived(Xml);
			else
				return Task.FromResult(true);
		}

		/// <summary>
		/// If reading has been paused.
		/// </summary>
		public override bool Paused => false;

		/// <summary>
		/// Continues a paused connection.
		/// </summary>
		public override void Continue()
		{
			throw new InvalidOperationException("BOSH connections do not support pause & continue.");
		}

	}
}
