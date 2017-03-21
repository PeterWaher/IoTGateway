using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content;
using Waher.Events;
using Waher.Networking.XMPP.ServiceDiscovery;
using Waher.Networking.XMPP.StanzaErrors;

namespace Waher.Networking.XMPP.P2P.SOCKS5
{
	/// <summary>
	/// Class managing a SOCKS5 proxy associated with the current XMPP server.
	/// </summary>
	public class Socks5Proxy : IDisposable
	{
		/// <summary>
		/// http://jabber.org/protocol/bytestreams
		/// </summary>
		public const string Namespace = "http://jabber.org/protocol/bytestreams";

		private Dictionary<string, Socks5Client> streams = new Dictionary<string, Socks5Client>();
		private XmppClient client;
		private bool hasProxy = false;
		private string jid = null;
		private string host = null;
		private int port = 0;

		/// <summary>
		/// Class managing a SOCKS5 proxy associated with the current XMPP server.
		/// </summary>
		/// <param name="Client">XMPP Client.</param>
		public Socks5Proxy(XmppClient Client)
		{
			this.client = Client;

			this.client.RegisterIqSetHandler("query", Namespace, this.QueryHandler, true);
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			this.client.UnregisterIqSetHandler("query", Namespace, this.QueryHandler, true);
		}

		/// <summary>
		/// If a SOCKS5 proxy has been detected.
		/// </summary>
		public bool HasProxy
		{
			get { return this.hasProxy; }
		}

		/// <summary>
		/// JID of SOCKS5 proxy.
		/// </summary>
		public string JID
		{
			get { return this.jid; }
		}

		/// <summary>
		/// Host name or IP address of SOCKS5 proxy.
		/// </summary>
		public string Host
		{
			get { return this.host; }
		}

		/// <summary>
		/// Port number of SOCKS5 proxy.
		/// </summary>
		public int Port
		{
			get { return this.port; }
		}

		/// <summary>
		/// Starts the search of SOCKS5 proxies.
		/// </summary>
		/// <param name="Callback">Method to call when search is complete. Properties on this object will have been updated.</param>
		public void StartSearch(EventHandler Callback)
		{
			this.hasProxy = false;
			this.jid = null;
			this.host = null;
			this.port = 0;

			this.client.SendServiceItemsDiscoveryRequest(this.client.Domain, (sender, e) =>
			{
				SearchState State = new SearchState(this, e.Items, Callback);
				State.DoQuery();
			}, null);
		}

		private class SearchState
		{
			public Socks5Proxy Proxy;
			public EventHandler Callback;
			public string Component = string.Empty;
			public Item[] Items;
			public int Pos = 0;
			public int NrItems;

			public SearchState(Socks5Proxy Proxy, Item[] Items, EventHandler Callback)
			{
				this.Proxy = Proxy;
				this.Callback = Callback;
				this.Items = Items;
				this.NrItems = Items.Length;
			}

			public void Advance()
			{
				this.Pos++;
				this.DoQuery();
			}

			public void DoQuery()
			{
				if (this.Pos < this.NrItems)
				{
					this.Proxy.client.SendServiceDiscoveryRequest(this.Items[this.Pos].JID, (sender2, e2) =>
					{
						if (e2.Features.ContainsKey(Namespace))
						{
							this.Component = this.Items[this.Pos].JID;

							this.Proxy.client.SendIqGet(this.Component, "<query xmlns='" + Namespace + "'/>", (sender3, e3) =>
							{
								if (e3.Ok)
								{
									XmlElement E = (XmlElement)e3.FirstElement;

									if (E.LocalName == "query" && E.NamespaceURI == Namespace)
									{
										E = (XmlElement)E.FirstChild;

										if (E.LocalName == "streamhost" && E.NamespaceURI == Namespace)
										{
											this.Proxy.jid = XML.Attribute(E, "jid");
											this.Proxy.port = XML.Attribute(E, "port", 0);
											this.Proxy.host = XML.Attribute(E, "host");
											this.Proxy.hasProxy = !string.IsNullOrEmpty(this.Proxy.jid) &&
												!string.IsNullOrEmpty(this.Proxy.host) &&
												this.Proxy.port > 0;

											if (this.Proxy.hasProxy)
												this.SearchDone();
											else
												this.Advance();
										}
										else
											this.Advance();
									}
									else
										this.Advance();
								}
								else
									this.Advance();

							}, null);
						}
						else
							this.Advance();
					}, null);
				}
				else
					this.SearchDone();
			}

			private void SearchDone()
			{
				if (this.Callback != null)
				{
					try
					{
						this.Callback(this.Proxy, new EventArgs());
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}
			}
		}

		/// <summary>
		/// Sets the SOCKS5 proxy to use. This method can be called, if searching for a SOCKS5 proxy is not desired, or does not
		/// find a proxy.
		/// </summary>
		/// <param name="Host">SOCKS5 host name.</param>
		/// <param name="Port">Port number.</param>
		/// <param name="JID">JID of stream host.</param>
		public void Use(string Host, int Port, string JID)
		{
			this.host = Host;
			this.port = Port;
			this.jid = JID;
			this.hasProxy = !string.IsNullOrEmpty(this.jid) && !string.IsNullOrEmpty(this.host) && this.port > 0;
		}

		/// <summary>
		/// Initiates a mediated SOCKS5 session with another.
		/// </summary>
		/// <param name="DestinationJid">JID of destination.</param>
		/// <param name="Callback">Method to call when initiation attempt completes.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void InitiateSession(string DestinationJid, StreamEventHandler Callback, object State)
		{
			if (!this.hasProxy)
			{
				this.Callback(Callback, State, false, null, null);
				return;
			}

			string StreamId;

			lock (this.streams)
			{
				do
				{
					StreamId = Guid.NewGuid().ToString().Replace("-", string.Empty);
				}
				while (this.streams.ContainsKey(StreamId));

				this.streams[StreamId] = null;
			}

			StringBuilder Xml = new StringBuilder();

			Xml.Append("<query xmlns='");
			Xml.Append(Namespace);
			Xml.Append("' sid='");
			Xml.Append(StreamId);
			Xml.Append("'><streamhost host='");
			Xml.Append(this.host);
			Xml.Append("' jid='");
			Xml.Append(this.jid);
			Xml.Append("' port='");
			Xml.Append(this.port.ToString());
			Xml.Append("'/></query>");

			this.client.SendIqSet(DestinationJid, Xml.ToString(), (sender, e) =>
			{
				if (e.Ok)
				{
					XmlElement E = e.FirstElement;

					if (E != null && E.LocalName == "query" && E.NamespaceURI == Namespace && XML.Attribute(E, "sid") == StreamId)
					{
						XmlElement E2;
						string StreamHostUsed = null;

						foreach (XmlNode N in E.ChildNodes)
						{
							E2 = N as XmlElement;
							if (E2.LocalName == "streamhost-used" && E2.NamespaceURI == Namespace)
							{
								StreamHostUsed = XML.Attribute(E2, "jid");
								break;
							}
						}

						if (!string.IsNullOrEmpty(StreamHostUsed) && StreamHostUsed == this.host)
						{
							Socks5Client Stream = new Socks5Client(this.host, this.port, this.jid,
								new Sniffers.ConsoleOutSniffer(Sniffers.BinaryPresentationMethod.Hexadecimal));	// TODO: Remove

							Stream.OnStateChange += (sender2, e2) =>
							{
								switch (Stream.State)
								{
									case Socks5State.Authenticated:
										Stream.CONNECT(StreamId, this.client.FullJID, DestinationJid);
										break;

									case Socks5State.Connected:
										Xml.Clear();

										Xml.Append("<query xmlns='");
										Xml.Append(Namespace);
										Xml.Append("' sid='");
										Xml.Append(StreamId);
										Xml.Append("'><activate>");
										Xml.Append(DestinationJid);
										Xml.Append("</activate></query>");

										this.client.SendIqSet(this.jid, Xml.ToString(), (sender3, e3) =>
										{
											if (e.Ok)
												this.Callback(Callback, State, true, Stream, StreamId);
											else
											{
												Stream.Dispose();
												this.Callback(Callback, State, false, null, StreamId);
											}

											Callback = null;
										}, null);
										break;

									case Socks5State.Error:
									case Socks5State.Offline:
										if (Stream != null)
											Stream.Dispose();
										this.Callback(Callback, State, false, null, StreamId);
										Callback = null;
										break;
								}
							};

							lock (this.streams)
							{
								this.streams[StreamId] = Stream;
							}
						}
						else
							this.Callback(Callback, State, false, null, StreamId);
					}
					else
						this.Callback(Callback, State, false, null, StreamId);
				}
				else
					this.Callback(Callback, State, false, null, StreamId);
			}, null);
		}

		private void Callback(StreamEventHandler Callback, object State, bool Ok, Socks5Client Stream, string StreamId)
		{
			if (!Ok && !string.IsNullOrEmpty(StreamId))
			{
				lock (this.streams)
				{
					this.streams.Remove(StreamId);
				}
			}

			if (Callback != null)
			{
				try
				{
					Callback(this, new StreamEventArgs(Ok, Stream, State));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		private void QueryHandler(object Sender, IqEventArgs e)
		{
			string StreamId = XML.Attribute(e.Query, "sid");
			XmlElement E;

			if (string.IsNullOrEmpty(StreamId) || StreamId != Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(StreamId)))
				throw new NotAcceptableException("Invalid Stream ID.", e.IQ);

			string Host = null;
			string JID = null;
			int Port = 0;

			foreach (XmlNode N in e.Query.ChildNodes)
			{
				E = N as XmlElement;
				if (E == null)
					continue;

				if (E.LocalName == "streamhost" && E.NamespaceURI == Namespace)
				{
					Host = XML.Attribute(E, "host");
					JID = XML.Attribute(E, "jid");
					Port = XML.Attribute(E, "port", 0);

					break;
				}
			}

			if (string.IsNullOrEmpty(JID) || string.IsNullOrEmpty(Host) || Port <= 0 || Port >= 0x10000)
				throw new BadRequestException("Invalid parameters.", e.IQ);

			ValidateStreamEventHandler h = this.OnOpen;
			ValidateStreamEventArgs e2 = new ValidateStreamEventArgs(this.client, e, StreamId);
			if (h != null)
			{
				try
				{
					h(this, e2);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}

			if (e2.DataCallback == null || e2.CloseCallback == null)
				throw new NotAcceptableException("Stream not expected.", e.IQ);

			Socks5Client Client;

			lock (this.streams)
			{
				if (this.streams.ContainsKey(StreamId))
					throw new ConflictException("Stream already exists.", e.IQ);

				Client = new Socks5Client(Host, Port, JID,
					new Sniffers.ConsoleErrorSniffer(Sniffers.BinaryPresentationMethod.Hexadecimal)); // TODO: Remove

				this.streams[StreamId] = Client;
			}

			Client.OnDataReceived += e2.DataCallback;

			Client.OnStateChange += (sender2, e3) =>
			{
				switch (Client.State)
				{
					case Socks5State.Authenticated:
						Client.CONNECT(StreamId, e.From, this.client.FullJID);
						break;

					case Socks5State.Connected:
						StringBuilder Xml = new StringBuilder();

						Xml.Append("<query xmlns='");
						Xml.Append(Namespace);
						Xml.Append("' sid ='");
						Xml.Append(StreamId);
						Xml.Append("'><streamhost-used jid='");
						Xml.Append(Host);
						Xml.Append("'/></query>");

						e.IqResult(Xml.ToString());
						break;

					case Socks5State.Error:
					case Socks5State.Offline:
						if (Client.State == Socks5State.Error)
							e.IqError(new BadRequestException("Unable to establish a SOCKS5 connection.", e.IQ));

						Client.Dispose();

						lock (this.streams)
						{
							this.streams.Remove(StreamId);
						}

						if (e2.CloseCallback != null)
						{
							try
							{
								e2.CloseCallback(this, new StreamEventArgs(false, Client, e2.State));
							}
							catch (Exception ex)
							{
								Log.Critical(ex);
							}
						}
						break;
				}
			};
		}

		/// <summary>
		/// Event raised when a remote entity tries to open a SOCKS5 bytestream for transmission of data to/from the client.
		/// A stream has to be accepted before data can be successfully received.
		/// </summary>
		public event ValidateStreamEventHandler OnOpen = null;

	}
}
