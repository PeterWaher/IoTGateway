using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.XMPP.ServiceDiscovery;
using Waher.Networking.XMPP.StanzaErrors;

namespace Waher.Networking.XMPP.P2P.SOCKS5
{
	/// <summary>
	/// Class managing a SOCKS5 proxy associated with the current XMPP server.
	/// </summary>
	public class Socks5Proxy : XmppExtension
	{
		/// <summary>
		/// http://jabber.org/protocol/bytestreams
		/// </summary>
		public const string Namespace = "http://jabber.org/protocol/bytestreams";

		private readonly Dictionary<string, Socks5Client> streams = new Dictionary<string, Socks5Client>();
		private IEndToEndEncryption e2e;
		private bool hasProxy = false;
		private string jid = null;
		private string host = null;
		private int port = 0;

		/// <summary>
		/// Class managing a SOCKS5 proxy associated with the current XMPP server.
		/// </summary>
		/// <param name="Client">XMPP Client.</param>
		public Socks5Proxy(XmppClient Client)
			: this(Client, null)
		{
		}

		/// <summary>
		/// Class managing a SOCKS5 proxy associated with the current XMPP server.
		/// </summary>
		/// <param name="Client">XMPP Client.</param>
		/// <param name="E2E">End-to-end encryption interface.</param>
		public Socks5Proxy(XmppClient Client, IEndToEndEncryption E2E)
			: base(Client)
		{
			this.e2e = E2E;

			this.client.RegisterIqSetHandler("query", Namespace, this.QueryHandler, true);
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();

			this.client.UnregisterIqSetHandler("query", Namespace, this.QueryHandler, true);
		}

		/// <summary>
		/// Implemented extensions.
		/// </summary>
		public override string[] Extensions => new string[] { "XEP-0065" };

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

			this.client.SendServiceItemsDiscoveryRequest(this.client.Domain, this.SearchResponse, Callback);
		}

		private void SearchResponse(object Sender, ServiceItemsDiscoveryEventArgs e)
		{
			EventHandler Callback = (EventHandler)e.State;
			SearchState State = new SearchState(this, e.Items, Callback);
			State.DoQuery();
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
					this.Proxy.client.SendServiceDiscoveryRequest(this.Items[this.Pos].JID, this.ItemDiscoveryResponse, null);
				}
				else
					this.SearchDone();
			}

			private void ItemDiscoveryResponse(object Sender, ServiceDiscoveryEventArgs e2)
			{
				if (e2.Features.ContainsKey(Namespace))
				{
					this.Component = this.Items[this.Pos].JID;
					this.Proxy.client.SendIqGet(this.Component, "<query xmlns=\"" + Namespace + "\"/>", this.SocksQueryResponse, null);
				}
				else
					this.Advance();
			}

			private void SocksQueryResponse(object Sender, IqResultEventArgs e3)
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
			this.InitiateSession(DestinationJid, null, Callback, State);
		}

		/// <summary>
		/// Initiates a mediated SOCKS5 session with another.
		/// </summary>
		/// <param name="DestinationJid">JID of destination.</param>
		/// <param name="StreamId">Stream ID to use.</param>
		/// <param name="Callback">Method to call when initiation attempt completes.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void InitiateSession(string DestinationJid, string StreamId, StreamEventHandler Callback, object State)
		{
			if (!this.hasProxy)
			{
				this.Callback(Callback, State, false, null, null);
				return;
			}

			lock (this.streams)
			{
				if (string.IsNullOrEmpty(StreamId))
				{
					do
					{
						StreamId = Guid.NewGuid().ToString().Replace("-", string.Empty);
					}
					while (this.streams.ContainsKey(StreamId));
				}
				else if (this.streams.ContainsKey(StreamId))
					StreamId = null;

				if (StreamId != null)
					this.streams[StreamId] = null;
			}

			if (StreamId is null)
			{
				this.Callback(Callback, State, false, null, null);
				return;
			}

			StringBuilder Xml = new StringBuilder();

			Xml.Append("<query xmlns=\"");
			Xml.Append(Namespace);
			Xml.Append("\" sid=\"");
			Xml.Append(StreamId);
			Xml.Append("\"><streamhost host=\"");
			Xml.Append(this.host);
			Xml.Append("\" jid=\"");
			Xml.Append(this.jid);
			Xml.Append("\" port=\"");
			Xml.Append(this.port.ToString());
			Xml.Append("\"/></query>");

			InitiationRec Rec = new InitiationRec()
			{
				destinationJid = DestinationJid,
				streamId = StreamId,
				callback = Callback,
				state = State,
				proxy = this
			};

			if (this.e2e != null)
				this.e2e.SendIqSet(this.client, E2ETransmission.NormalIfNotE2E, DestinationJid, Xml.ToString(), this.InitiationResponse, Rec);
			else
				this.client.SendIqSet(DestinationJid, Xml.ToString(), this.InitiationResponse, Rec);
		}

		private class InitiationRec
		{
			public string destinationJid;
			public string streamId;
			public object state;
			public StreamEventHandler callback;
			public Socks5Client stream = null;
			public Socks5Proxy proxy;

			internal void StateChanged(object Sender, EventArgs e)
			{
				switch (this.stream.State)
				{
					case Socks5State.Authenticated:
						this.stream.CONNECT(this.streamId, this.proxy.client.FullJID, this.destinationJid);
						break;

					case Socks5State.Connected:
						StringBuilder Xml = new StringBuilder();

						Xml.Append("<query xmlns=\"");
						Xml.Append(Namespace);
						Xml.Append("\" sid=\"");
						Xml.Append(this.streamId);
						Xml.Append("\"><activate>");
						Xml.Append(this.destinationJid);
						Xml.Append("</activate></query>");

						if (this.proxy.e2e != null)
						{
							this.proxy.e2e.SendIqSet(this.proxy.client, E2ETransmission.NormalIfNotE2E, this.proxy.jid, Xml.ToString(),
								this.proxy.ActivationResponse, this);
						}
						else
							this.proxy.client.SendIqSet(this.proxy.jid, Xml.ToString(), this.proxy.ActivationResponse, this);
						break;

					case Socks5State.Error:
					case Socks5State.Offline:
						if (this.stream != null)
							this.stream.Dispose();
						this.proxy.Callback(this.callback, this.state, false, null, this.streamId);
						this.callback = null;
						break;
				}
			}
		}

		private void InitiationResponse(object Sender, IqResultEventArgs e)
		{
			InitiationRec Rec = (InitiationRec)e.State;

			if (e.Ok)
			{
				XmlElement E = e.FirstElement;

				if (E != null && E.LocalName == "query" && E.NamespaceURI == Namespace && XML.Attribute(E, "sid") == Rec.streamId)
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
                        Rec.stream = new Socks5Client(this.host, this.port, this.jid);
                        Rec.stream.OnStateChange += Rec.StateChanged;

						lock (this.streams)
						{
							this.streams[Rec.streamId] = Rec.stream;
						}
					}
					else
						this.Callback(Rec.callback, Rec.state, false, null, Rec.streamId);
				}
				else
					this.Callback(Rec.callback, Rec.state, false, null, Rec.streamId);
			}
			else
				this.Callback(Rec.callback, Rec.state, false, null, Rec.streamId);
		}

		private void ActivationResponse(object Sender, IqResultEventArgs e)
		{
			InitiationRec Rec = (InitiationRec)e.State;

			if (e.Ok)
				this.Callback(Rec.callback, Rec.state, true, Rec.stream, Rec.streamId);
			else
			{
				Rec.stream.Dispose();
				this.Callback(Rec.callback, Rec.state, false, null, Rec.streamId);
			}

			Rec.callback = null;
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
				if (E is null)
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

			if (e2.DataCallback is null || e2.CloseCallback is null)
				throw new NotAcceptableException("Stream not expected.", e.IQ);

			Socks5Client Client;

			lock (this.streams)
			{
				if (this.streams.ContainsKey(StreamId))
					throw new ConflictException("Stream already exists.", e.IQ);

				Client = new Socks5Client(Host, Port, JID)
                {
				    CallbackState = e2.State
                };

				this.streams[StreamId] = Client;
			}

			Client.Tag = new Socks5QueryState()
			{
				streamId = StreamId,
				eventargs = e,
				eventargs2 = e2
			};

			Client.OnDataReceived += e2.DataCallback;
			Client.OnStateChange += this.ClientStateChanged;
		}

		private class Socks5QueryState
		{
			public string streamId;
			public IqEventArgs eventargs;
			public ValidateStreamEventArgs eventargs2;
		}

		private void ClientStateChanged(object Sender, EventArgs e3)
		{
			Socks5Client Client = (Socks5Client)Sender;
			Socks5QueryState State = (Socks5QueryState)Client.Tag;

			switch (Client.State)
			{
				case Socks5State.Authenticated:
					Client.CONNECT(State.streamId, State.eventargs.From, this.client.FullJID);
					break;

				case Socks5State.Connected:
					StringBuilder Xml = new StringBuilder();

					Xml.Append("<query xmlns=\"");
					Xml.Append(Namespace);
					Xml.Append("\" sid=\"");
					Xml.Append(State.streamId);
					Xml.Append("\"><streamhost-used jid=\"");
					Xml.Append(Client.Host);
					Xml.Append("\"/></query>");

					State.eventargs.IqResult(Xml.ToString());
					break;

				case Socks5State.Error:
				case Socks5State.Offline:
					if (Client.State == Socks5State.Error)
						State.eventargs.IqError(new BadRequestException("Unable to establish a SOCKS5 connection.", State.eventargs.IQ));

					Client.Dispose();

					lock (this.streams)
					{
						this.streams.Remove(State.streamId);
					}

					if (State.eventargs2.CloseCallback != null)
					{
						try
						{
							State.eventargs2.CloseCallback(this, new StreamEventArgs(false, Client, State.eventargs2.State));
						}
						catch (Exception ex)
						{
							Log.Critical(ex);
						}
					}
					break;
			}
		}

		/// <summary>
		/// Event raised when a remote entity tries to open a SOCKS5 bytestream for transmission of data to/from the client.
		/// A stream has to be accepted before data can be successfully received.
		/// </summary>
		public event ValidateStreamEventHandler OnOpen = null;

	}
}
