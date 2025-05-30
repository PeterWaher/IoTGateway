﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.XMPP.Events;
using Waher.Networking.XMPP.ServiceDiscovery;
using Waher.Networking.XMPP.StanzaErrors;

namespace Waher.Networking.XMPP.P2P.SOCKS5
{
	/// <summary>
	/// Class managing a SOCKS5 proxy associated with the current XMPP server.
	/// </summary>
	public class Socks5Proxy : XmppExtension, IHostReference
	{
		/// <summary>
		/// http://jabber.org/protocol/bytestreams
		/// </summary>
		public const string Namespace = "http://jabber.org/protocol/bytestreams";

		private readonly Dictionary<string, Socks5Client> streams = new Dictionary<string, Socks5Client>();
		private readonly IEndToEndEncryption e2e;
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

		/// <inheritdoc/>
		public override void Dispose()
		{
			this.client.UnregisterIqSetHandler("query", Namespace, this.QueryHandler, true);
			base.Dispose();
		}

		/// <summary>
		/// Implemented extensions.
		/// </summary>
		public override string[] Extensions => new string[] { "XEP-0065" };

		/// <summary>
		/// If a SOCKS5 proxy has been detected.
		/// </summary>
		public bool HasProxy => this.hasProxy;

		/// <summary>
		/// JID of SOCKS5 proxy.
		/// </summary>
		public string JID => this.jid;

		/// <summary>
		/// Host name or IP address of SOCKS5 proxy.
		/// </summary>
		public string Host => this.host;

		/// <summary>
		/// Port number of SOCKS5 proxy.
		/// </summary>
		public int Port => this.port;

		/// <summary>
		/// Starts the search of SOCKS5 proxies.
		/// </summary>
		/// <param name="Callback">Method to call when search is complete. Properties on this object will have been updated.</param>
		public Task StartSearch(EventHandlerAsync Callback)
		{
			this.hasProxy = false;
			this.jid = null;
			this.host = null;
			this.port = 0;

			return this.client.SendServiceItemsDiscoveryRequest(this.client.Domain, this.SearchResponse, Callback);
		}

		private Task SearchResponse(object Sender, ServiceItemsDiscoveryEventArgs e)
		{
			EventHandlerAsync Callback = (EventHandlerAsync)e.State;
			SearchState State = new SearchState(this, e.Items, Callback);
			return State.DoQuery();
		}

		private class SearchState
		{
			public Socks5Proxy Proxy;
			public EventHandlerAsync Callback;
			public string Component = string.Empty;
			public Item[] Items;
			public int Pos = 0;
			public int NrItems;

			public SearchState(Socks5Proxy Proxy, Item[] Items, EventHandlerAsync Callback)
			{
				this.Proxy = Proxy;
				this.Callback = Callback;
				this.Items = Items;
				this.NrItems = Items.Length;
			}

			public Task Advance()
			{
				this.Pos++;
				return this.DoQuery();
			}

			public Task DoQuery()
			{
				if (this.Pos < this.NrItems)
					return this.Proxy.client.SendServiceDiscoveryRequest(this.Items[this.Pos].JID, this.ItemDiscoveryResponse, null);
				else
					return this.SearchDone();
			}

			private async Task ItemDiscoveryResponse(object Sender, ServiceDiscoveryEventArgs e2)
			{
				if (e2.Features.ContainsKey(Namespace))
				{
					this.Component = this.Items[this.Pos].JID;
					await this.Proxy.client.SendIqGet(this.Component, "<query xmlns=\"" + Namespace + "\"/>", this.SocksQueryResponse, null);
				}
				else
					await this.Advance();
			}

			private Task SocksQueryResponse(object Sender, IqResultEventArgs e3)
			{
				if (e3.Ok)
				{
					XmlElement E = e3.FirstElement;

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
								return this.SearchDone();
							else
								return this.Advance();
						}
						else
							return this.Advance();
					}
					else
						return this.Advance();
				}
				else
					return this.Advance();
			}

			private Task SearchDone()
			{
				return this.Callback.Raise(this.Proxy, EventArgs.Empty);
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
		public Task InitiateSession(string DestinationJid, EventHandlerAsync<StreamEventArgs> Callback, object State)
		{
			return this.InitiateSession(DestinationJid, null, true, Callback, State);
		}

		/// <summary>
		/// Initiates a mediated SOCKS5 session with another.
		/// </summary>
		/// <param name="DestinationJid">JID of destination.</param>
		/// <param name="StreamId">Stream ID to use.</param>
		/// <param name="Callback">Method to call when initiation attempt completes.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task InitiateSession(string DestinationJid, string StreamId, EventHandlerAsync<StreamEventArgs> Callback, object State)
		{
			return this.InitiateSession(DestinationJid, StreamId, true, Callback, State);
		}

		/// <summary>
		/// Initiates a mediated SOCKS5 session with another.
		/// </summary>
		/// <param name="DestinationJid">JID of destination.</param>
		/// <param name="StreamId">Stream ID to use.</param>
		/// <param name="InstantiateSocks5Client">If a SOCKS5 client should be instantiated when activated (true, default), or
		/// if the caller instantiates the SOCKS5 client (false).</param>
		/// <param name="Callback">Method to call when initiation attempt completes.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public async Task InitiateSession(string DestinationJid, string StreamId, bool InstantiateSocks5Client,
			EventHandlerAsync<StreamEventArgs> Callback, object State)
		{
			if (!this.hasProxy)
			{
				await this.Callback(Callback, State, false, null, null);
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

				if (!(StreamId is null))
					this.streams[StreamId] = null;
			}

			if (StreamId is null)
			{
				await this.Callback(Callback, State, false, null, null);
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
				proxy = this,
				instantiateSocks5Client = InstantiateSocks5Client
			};

			if (!(this.e2e is null))
				await this.e2e.SendIqSet(this.client, E2ETransmission.NormalIfNotE2E, DestinationJid, Xml.ToString(), this.InitiationResponse, Rec);
			else
				await this.client.SendIqSet(DestinationJid, Xml.ToString(), this.InitiationResponse, Rec);
		}

		private class InitiationRec
		{
			public string destinationJid;
			public string streamId;
			public object state;
			public bool instantiateSocks5Client;
			public EventHandlerAsync<StreamEventArgs> callback;
			public Socks5Client stream = null;
			public Socks5Proxy proxy;

			internal async Task StateChanged(object Sender, EventArgs e)
			{
				switch (this.stream.State)
				{
					case Socks5State.Authenticated:
						await this.stream.CONNECT(this.streamId, this.proxy.client.FullJID, this.destinationJid);
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

						if (!(this.proxy.e2e is null))
						{
							await this.proxy.e2e.SendIqSet(this.proxy.client, E2ETransmission.NormalIfNotE2E, this.proxy.jid, Xml.ToString(),
								this.proxy.ActivationResponse, this);
						}
						else
							await this.proxy.client.SendIqSet(this.proxy.jid, Xml.ToString(), this.proxy.ActivationResponse, this);
						break;

					case Socks5State.Error:
					case Socks5State.Offline:
						if (!(this.stream is null))
							await this.stream.DisposeAsync();

						await this.proxy.Callback(this.callback, this.state, false, null, this.streamId);
						this.callback = null;
						break;
				}
			}
		}

		private async Task InitiationResponse(object Sender, IqResultEventArgs e)
		{
			InitiationRec Rec = (InitiationRec)e.State;

			if (e.Ok)
			{
				XmlElement E = e.FirstElement;

				if (!(E is null) && E.LocalName == "query" && E.NamespaceURI == Namespace && XML.Attribute(E, "sid") == Rec.streamId)
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
						if (Rec.instantiateSocks5Client)
						{
							Rec.stream = new Socks5Client(this.host, this.port, this.jid);
							Rec.stream.OnStateChange += Rec.StateChanged;

							lock (this.streams)
							{
								this.streams[Rec.streamId] = Rec.stream;
							}
						}
						else
							await this.Callback(Rec.callback, Rec.state, true, null, Rec.streamId);
					}
					else
						await this.Callback(Rec.callback, Rec.state, false, null, Rec.streamId);
				}
				else
					await this.Callback(Rec.callback, Rec.state, false, null, Rec.streamId);
			}
			else
				await this.Callback(Rec.callback, Rec.state, false, null, Rec.streamId);
		}

		private async Task ActivationResponse(object Sender, IqResultEventArgs e)
		{
			InitiationRec Rec = (InitiationRec)e.State;

			if (e.Ok)
				await this.Callback(Rec.callback, Rec.state, true, Rec.stream, Rec.streamId);
			else
			{
				await Rec.stream.DisposeAsync();
				await this.Callback(Rec.callback, Rec.state, false, null, Rec.streamId);
			}

			Rec.callback = null;
		}

		private async Task Callback(EventHandlerAsync<StreamEventArgs> Callback, object State, bool Ok, Socks5Client Stream, string StreamId)
		{
			if (!Ok && !string.IsNullOrEmpty(StreamId))
			{
				lock (this.streams)
				{
					this.streams.Remove(StreamId);
				}
			}

			await Callback.Raise(this, new StreamEventArgs(Ok, Stream, State));
		}

		private async Task QueryHandler(object Sender, IqEventArgs e)
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

			ValidateStreamEventArgs e2 = new ValidateStreamEventArgs(this.client, e, StreamId);
			await this.OnOpen.Raise(this, e2, false);

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

		private async Task ClientStateChanged(object Sender, EventArgs e3)
		{
			Socks5Client Client = (Socks5Client)Sender;
			Socks5QueryState State = (Socks5QueryState)Client.Tag;

			switch (Client.State)
			{
				case Socks5State.Authenticated:
					await Client.CONNECT(State.streamId, State.eventargs.From, this.client.FullJID);
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

					await State.eventargs.IqResult(Xml.ToString());
					break;

				case Socks5State.Error:
				case Socks5State.Offline:
					if (Client.State == Socks5State.Error)
						await State.eventargs.IqError(new BadRequestException("Unable to establish a SOCKS5 connection.", State.eventargs.IQ));

					await Client.DisposeAsync();

					lock (this.streams)
					{
						this.streams.Remove(State.streamId);
					}

					await State.eventargs2.CloseCallback.Raise(this, new StreamEventArgs(false, Client, State.eventargs2.State));
					break;
			}
		}

		/// <summary>
		/// Event raised when a remote entity tries to open a SOCKS5 bytestream for transmission of data to/from the client.
		/// A stream has to be accepted before data can be successfully received.
		/// </summary>
		public event EventHandlerAsync<ValidateStreamEventArgs> OnOpen = null;

	}
}
