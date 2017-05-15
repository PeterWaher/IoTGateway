using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content;
using Waher.Networking.XMPP.Provisioning;
using Waher.Things;
using Waher.Things.SensorData;
using Waher.Runtime.Timing;

namespace Waher.Networking.XMPP.Sensor
{
	/// <summary>
	/// Delegate for sensor data readout request events, on a sensor server.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="Request">Readout request to process.</param>
	public delegate void SensorDataReadoutEventHandler(object Sender, SensorDataServerRequest Request);

	/// <summary>
	/// Implements an XMPP sensor server interface.
	/// 
	/// The interface is defined in XEP-0323:
	/// http://xmpp.org/extensions/xep-0323.html
	/// 
	/// It also supports the event subscription pattern, documented in the iot-events proto-XEP:
	/// http://www.xmpp.org/extensions/inbox/iot-events.html
	/// </summary>
	public class SensorServer : IDisposable
	{
		private Dictionary<string, SensorDataServerRequest> requests = new Dictionary<string, SensorDataServerRequest>();
		private Scheduler scheduler = new Scheduler();
		private XmppClient client;
		private ProvisioningClient provisioningClient;

		/// <summary>
		/// Implements an XMPP sensor server interface.
		/// 
		/// The interface is defined in XEP-0323:
		/// http://xmpp.org/extensions/xep-0323.html
		/// 
		/// It also supports the event subscription pattern, documented in the iot-events proto-XEP:
		/// http://www.xmpp.org/extensions/inbox/iot-events.html
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="SupportsEvents">If events are supported.</param>
		public SensorServer(XmppClient Client, bool SupportsEvents)
			: this(Client, null, SupportsEvents)
		{
		}

		/// <summary>
		/// Implements an XMPP sensor server interface.
		/// 
		/// The interface is defined in XEP-0323:
		/// http://xmpp.org/extensions/xep-0323.html
		/// 
		/// It also supports the event subscription pattern, documented in the iot-events proto-XEP:
		/// http://www.xmpp.org/extensions/inbox/iot-events.html
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="ProvisioningClient">Provisioning client, if sensor supports provisioning.</param>
		/// <param name="SupportsEvents">If events are supported.</param>
		public SensorServer(XmppClient Client, ProvisioningClient ProvisioningClient, bool SupportsEvents)
		{
			this.client = Client;
			this.provisioningClient = ProvisioningClient;

			this.client.RegisterIqGetHandler("req", SensorClient.NamespaceSensorData, this.ReqHandler, true);
			this.client.RegisterIqSetHandler("req", SensorClient.NamespaceSensorData, this.ReqHandler, false);
			this.client.RegisterIqGetHandler("cancel", SensorClient.NamespaceSensorData, this.CancelHandler, false);
			this.client.RegisterIqSetHandler("cancel", SensorClient.NamespaceSensorData, this.CancelHandler, false);

			if (SupportsEvents)
			{
				this.client.RegisterIqGetHandler("subscribe", SensorClient.NamespaceSensorEvents, this.SubscribeHandler, true);
				this.client.RegisterIqSetHandler("subscribe", SensorClient.NamespaceSensorEvents, this.SubscribeHandler, false);
				this.client.RegisterIqGetHandler("unsubscribe", SensorClient.NamespaceSensorEvents, this.UnsubscribeHandler, false);
				this.client.RegisterIqSetHandler("unsubscribe", SensorClient.NamespaceSensorEvents, this.UnsubscribeHandler, false);

				this.client.OnPresenceUnsubscribe += Client_OnPresenceUnsubscribed;
				this.client.OnPresenceUnsubscribed += Client_OnPresenceUnsubscribed;
				this.client.OnPresence += Client_OnPresence;
			}
		}

		private void Client_OnPresence(object Sender, PresenceEventArgs e)
		{
			Dictionary<int, Subscription> Subscriptions;

			lock (this.subscriptionsByThing)
			{
				if (!this.subscriptionsByJID.TryGetValue(e.From, out Subscriptions))
					return;
			}

			foreach (Subscription Subscription in Subscriptions.Values)
			{
				Subscription.Availability = e.Availability;

				if (e.Availability != Availability.Offline && Subscription.SupressedTrigger)
				{
					Subscription.SupressedTrigger = false;
					Subscription.LastTrigger = DateTime.Now;
					this.TriggerSubscription(Subscription);
				}
			}
		}

		private void Client_OnPresenceUnsubscribed(object Sender, PresenceEventArgs e)
		{
			lock (this.subscriptionsByThing)
			{
				if (!this.subscriptionsByJID.TryGetValue(e.From, out Dictionary<int, Subscription> Subscriptions))
					return;

				this.subscriptionsByJID.Remove(e.From);

				foreach (Subscription Subscription in Subscriptions.Values)
				{
					Subscription.Active = false;

					foreach (ThingReference Ref in Subscription.NodeReferences)
					{
						if (!this.subscriptionsByThing.TryGetValue(Ref, out LinkedList<Subscription> Subscriptions2))
							continue;

						if (!Subscriptions2.Remove(Subscription))
							continue;

						if (Subscriptions2.First == null)
							this.subscriptionsByThing.Remove(Ref);
					}
				}
			}
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			this.client.UnregisterIqGetHandler("req", SensorClient.NamespaceSensorData, this.ReqHandler, true);
			this.client.UnregisterIqSetHandler("req", SensorClient.NamespaceSensorData, this.ReqHandler, false);
			this.client.UnregisterIqGetHandler("cancel", SensorClient.NamespaceSensorData, this.CancelHandler, false);
			this.client.UnregisterIqSetHandler("cancel", SensorClient.NamespaceSensorData, this.CancelHandler, false);

			this.client.UnregisterIqGetHandler("subscribe", SensorClient.NamespaceSensorEvents, this.SubscribeHandler, true);
			this.client.UnregisterIqSetHandler("subscribe", SensorClient.NamespaceSensorEvents, this.SubscribeHandler, false);
			this.client.UnregisterIqGetHandler("unsubscribe", SensorClient.NamespaceSensorEvents, this.UnsubscribeHandler, false);
			this.client.UnregisterIqSetHandler("unsubscribe", SensorClient.NamespaceSensorEvents, this.UnsubscribeHandler, false);
		}

		/// <summary>
		/// XMPP Client
		/// </summary>
		public XmppClient Client
		{
			get { return this.client; }
		}

		private void ReqHandler(object Sender, IqEventArgs e)
		{
			List<ThingReference> Nodes = null;
			List<string> Fields = null;
			XmlElement E = e.Query;
			FieldType FieldTypes = (FieldType)0;
			DateTime From = DateTime.MinValue;
			DateTime To = DateTime.MaxValue;
			DateTime When = DateTime.MinValue;
			string ServiceToken = string.Empty;
			string DeviceToken = string.Empty;
			string UserToken = string.Empty;
			string NodeId;
			string SourceId;
			string CacheType;
			int SeqNr = 0;
			bool b;

			foreach (XmlAttribute Attr in E.Attributes)
			{
				switch (Attr.Name)
				{
					case "seqnr":
						if (!int.TryParse(Attr.Value, out SeqNr))
							SeqNr = 0;
						break;

					case "from":
						if (!XML.TryParse(Attr.Value, out From))
							From = DateTime.MinValue;
						break;

					case "to":
						if (!XML.TryParse(Attr.Value, out To))
							To = DateTime.MaxValue;
						break;

					case "when":
						if (!XML.TryParse(Attr.Value, out When))
							When = DateTime.MinValue;
						break;

					case "serviceToken":
						ServiceToken = Attr.Value;
						break;

					case "deviceToken":
						DeviceToken = Attr.Value;
						break;

					case "userToken":
						UserToken = Attr.Value;
						break;

					case "all":
						if (CommonTypes.TryParse(Attr.Value, out b) && b)
							FieldTypes |= FieldType.All;
						break;

					case "historical":
						if (CommonTypes.TryParse(Attr.Value, out b) && b)
							FieldTypes |= FieldType.Historical;
						break;

					case "momentary":
						if (CommonTypes.TryParse(Attr.Value, out b) && b)
							FieldTypes |= FieldType.Momentary;
						break;

					case "peak":
						if (CommonTypes.TryParse(Attr.Value, out b) && b)
							FieldTypes |= FieldType.Peak;
						break;

					case "status":
						if (CommonTypes.TryParse(Attr.Value, out b) && b)
							FieldTypes |= FieldType.Status;
						break;

					case "computed":
						if (CommonTypes.TryParse(Attr.Value, out b) && b)
							FieldTypes |= FieldType.Computed;
						break;

					case "identity":
						if (CommonTypes.TryParse(Attr.Value, out b) && b)
							FieldTypes |= FieldType.Identity;
						break;

					case "historicalSecond":
						if (CommonTypes.TryParse(Attr.Value, out b) && b)
							FieldTypes |= FieldType.HistoricalSecond;
						break;

					case "historicalMinute":
						if (CommonTypes.TryParse(Attr.Value, out b) && b)
							FieldTypes |= FieldType.HistoricalMinute;
						break;

					case "historicalHour":
						if (CommonTypes.TryParse(Attr.Value, out b) && b)
							FieldTypes |= FieldType.HistoricalMonth;
						break;

					case "historicalDay":
						if (CommonTypes.TryParse(Attr.Value, out b) && b)
							FieldTypes |= FieldType.HistoricalDay;
						break;

					case "historicalWeek":
						if (CommonTypes.TryParse(Attr.Value, out b) && b)
							FieldTypes |= FieldType.HistoricalWeek;
						break;

					case "historicalMonth":
						if (CommonTypes.TryParse(Attr.Value, out b) && b)
							FieldTypes |= FieldType.HistoricalMonth;
						break;

					case "historicalQuarter":
						if (CommonTypes.TryParse(Attr.Value, out b) && b)
							FieldTypes |= FieldType.HistoricalQuarter;
						break;

					case "historicalYear":
						if (CommonTypes.TryParse(Attr.Value, out b) && b)
							FieldTypes |= FieldType.HistoricalYear;
						break;

					case "historicalOther":
						if (CommonTypes.TryParse(Attr.Value, out b) && b)
							FieldTypes |= FieldType.HistoricalOther;
						break;
				}
			}

			foreach (XmlNode N in E.ChildNodes)
			{
				switch (N.LocalName)
				{
					case "node":
						if (Nodes == null)
							Nodes = new List<ThingReference>();

						E = (XmlElement)N;
						NodeId = XML.Attribute(E, "nodeId");
						SourceId = XML.Attribute(E, "sourceId");
						CacheType = XML.Attribute(E, "cacheType");

						Nodes.Add(new ThingReference(NodeId, SourceId, CacheType));
						break;

					case "field":
						if (Fields == null)
							Fields = new List<string>();

						Fields.Add(XML.Attribute((XmlElement)N, "name"));
						break;
				}
			}

			SensorDataServerRequest Request = new SensorDataServerRequest(SeqNr, this, e.From, e.From, Nodes?.ToArray(), FieldTypes,
				Fields?.ToArray(), From, To, When, ServiceToken, DeviceToken, UserToken);

			if (this.provisioningClient != null)
			{
				this.provisioningClient.CanRead(e.FromBareJid, Request.Types, Request.Nodes, Request.FieldNames,
					Request.ServiceToken.Split(space, StringSplitOptions.RemoveEmptyEntries),
					Request.DeviceToken.Split(space, StringSplitOptions.RemoveEmptyEntries),
					Request.UserToken.Split(space, StringSplitOptions.RemoveEmptyEntries),
					(sender2, e2) =>
					{
						if (e2.Ok && e2.CanRead)
						{
							Request.Nodes = e2.Nodes;
							Request.FieldNames = e2.FieldsNames;
							Request.Types = e2.FieldTypes;

							this.AcceptRequest(Request, e, SeqNr);
						}
						else
						{
							e.IqError("<error type='cancel'><forbidden xmlns='urn:ietf:params:xml:ns:xmpp-stanzas' />" +
								"<text xmlns='urn:ietf:params:xml:ns:xmpp-stanzas' xml:lang='en'>Access denied.</text></error>");
						}

					}, null);
			}
			else
				this.AcceptRequest(Request, e, SeqNr);
		}

		private static readonly char[] space = new char[] { ' ' };

		private void AcceptRequest(SensorDataServerRequest Request, IqEventArgs e, int SeqNr)
		{
			string Key = e.From + " " + SeqNr.ToString();
			bool NewRequest;

			lock (this.requests)
			{
				if (NewRequest = !this.requests.ContainsKey(Key))
					this.requests[Key] = Request;
			}

			if (Request.When > DateTime.Now)
			{
				e.IqResult("<accepted xmlns='" + SensorClient.NamespaceSensorData + "' seqnr='" + SeqNr.ToString() + "' queued='true'/>");
				Request.When = this.scheduler.Add(Request.When, this.StartReadout, Request);
			}
			else
			{
				e.IqResult("<accepted xmlns='" + SensorClient.NamespaceSensorData + "' seqnr='" + SeqNr.ToString() + "'/>");
				this.PerformReadout(Request);
			}
		}

		private void StartReadout(object P)
		{
			SensorDataServerRequest Request = (SensorDataServerRequest)P;

			this.client.SendMessage(MessageType.Normal, Request.RemoteJID, "<started xmlns='" + SensorClient.NamespaceSensorData + "' seqnr='" +
				Request.SeqNr.ToString() + "'/>", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

			this.PerformReadout(Request);
		}

		private void PerformReadout(SensorDataServerRequest Request)
		{
			Request.Started = true;

			SensorDataReadoutEventHandler h = this.OnExecuteReadoutRequest;
			if (h == null)
			{
				this.client.SendMessage(MessageType.Normal, Request.RemoteJID, "<done xmlns='" + SensorClient.NamespaceSensorData + "' seqnr='" +
					Request.SeqNr.ToString() + "'/>", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

				lock (this.requests)
				{
					this.requests.Remove(Request.Key);
				}
			}
			else
			{
				try
				{
					h(this, Request);
				}
				catch (Exception ex)
				{
					Request.ReportErrors(true, new ThingError(string.Empty, string.Empty, string.Empty, DateTime.Now, ex.Message));
				}
			}
		}

		/// <summary>
		/// Performs an internal readout.
		/// </summary>
		/// <param name="Actor">Actor causing the request to be made.</param>
		/// <param name="Nodes">Array of nodes to read. Can be null or empty, if reading a sensor that is not a concentrator.</param>
		/// <param name="Types">Field Types to read.</param>
		/// <param name="Fields">Fields to read.</param>
		/// <param name="From">From what time readout is to be made. Use <see cref="DateTime.MinValue"/> to specify no lower limit.</param>
		/// <param name="To">To what time readout is to be made. Use <see cref="DateTime.MaxValue"/> to specify no upper limit.</param>
		/// <param name="OnFieldsReported">Callback method when fields are reported.</param>
		/// <param name="OnErrorsReported">Callback method when errors are reported.</param>
		/// <param name="State">State object passed on to callback methods.</param>
		/// <returns>Request object.</returns>
		public InternalReadoutRequest DoInternalReadout(string Actor, ThingReference[] Nodes, FieldType Types, string[] Fields, DateTime From, DateTime To,
			InternalReadoutFieldsEventHandler OnFieldsReported, InternalReadoutErrorsEventHandler OnErrorsReported, object State)
		{
			InternalReadoutRequest Request = new InternalReadoutRequest(Actor, Nodes, Types, Fields, From, To, OnFieldsReported, OnErrorsReported, State);

			this.OnExecuteReadoutRequest?.Invoke(this, Request);

			return Request;
		}

		internal bool Remove(SensorDataServerRequest Request)
		{
			lock (this.requests)
			{
				return this.requests.Remove(Request.Key);
			}
		}

		/// <summary>
		/// Event raised when a readout request is to be executed.
		/// </summary>
		public event SensorDataReadoutEventHandler OnExecuteReadoutRequest = null;

		private void CancelHandler(object Sender, IqEventArgs e)
		{
			SensorDataServerRequest Request;
			int SeqNr = XML.Attribute(e.Query, "seqnr", 0);
			string Key = e.From + " " + SeqNr.ToString();

			lock (this.requests)
			{
				if (this.requests.TryGetValue(Key, out Request))
					this.requests.Remove(Key);
				else
					Request = null;
			}

			if (Request != null && !Request.Started)
				this.scheduler.Remove(Request.When);

			e.IqResult("<cancelled xmlns='" + SensorClient.NamespaceSensorData + "' seqnr='" + SeqNr.ToString() + "'/>");
		}

		private void SubscribeHandler(object Sender, IqEventArgs e)
		{
			List<ThingReference> Nodes = null;
			Dictionary<string, FieldSubscriptionRule> Fields = null;
			XmlElement E = e.Query;
			FieldType FieldTypes = (FieldType)0;
			Duration MaxAge = null;
			Duration MinInterval = null;
			Duration MaxInterval = null;
			string ServiceToken = string.Empty;
			string DeviceToken = string.Empty;
			string UserToken = string.Empty;
			string NodeId;
			string SourceId;
			string CacheType;
			int SeqNr = 0;
			bool Req = false;
			bool b;

			foreach (XmlAttribute Attr in E.Attributes)
			{
				switch (Attr.Name)
				{
					case "seqnr":
						if (!int.TryParse(Attr.Value, out SeqNr))
							SeqNr = 0;
						break;

					case "maxAge":
						if (!Duration.TryParse(Attr.Value, out MaxAge))
							MaxAge = null;
						break;

					case "minInterval":
						if (!Duration.TryParse(Attr.Value, out MinInterval))
							MinInterval = null;
						break;

					case "maxInterval":
						if (!Duration.TryParse(Attr.Value, out MaxInterval))
							MaxInterval = null;
						break;

					case "serviceToken":
						ServiceToken = Attr.Value;
						break;

					case "deviceToken":
						DeviceToken = Attr.Value;
						break;

					case "userToken":
						UserToken = Attr.Value;
						break;

					case "all":
						if (CommonTypes.TryParse(Attr.Value, out b) && b)
							FieldTypes |= FieldType.All;
						break;

					case "historical":
						if (CommonTypes.TryParse(Attr.Value, out b) && b)
							FieldTypes |= FieldType.Historical;
						break;

					case "momentary":
						if (CommonTypes.TryParse(Attr.Value, out b) && b)
							FieldTypes |= FieldType.Momentary;
						break;

					case "peak":
						if (CommonTypes.TryParse(Attr.Value, out b) && b)
							FieldTypes |= FieldType.Peak;
						break;

					case "status":
						if (CommonTypes.TryParse(Attr.Value, out b) && b)
							FieldTypes |= FieldType.Status;
						break;

					case "computed":
						if (CommonTypes.TryParse(Attr.Value, out b) && b)
							FieldTypes |= FieldType.Computed;
						break;

					case "identity":
						if (CommonTypes.TryParse(Attr.Value, out b) && b)
							FieldTypes |= FieldType.Identity;
						break;

					case "historicalSecond":
						if (CommonTypes.TryParse(Attr.Value, out b) && b)
							FieldTypes |= FieldType.HistoricalSecond;
						break;

					case "historicalMinute":
						if (CommonTypes.TryParse(Attr.Value, out b) && b)
							FieldTypes |= FieldType.HistoricalMinute;
						break;

					case "historicalHour":
						if (CommonTypes.TryParse(Attr.Value, out b) && b)
							FieldTypes |= FieldType.HistoricalMonth;
						break;

					case "historicalDay":
						if (CommonTypes.TryParse(Attr.Value, out b) && b)
							FieldTypes |= FieldType.HistoricalDay;
						break;

					case "historicalWeek":
						if (CommonTypes.TryParse(Attr.Value, out b) && b)
							FieldTypes |= FieldType.HistoricalWeek;
						break;

					case "historicalMonth":
						if (CommonTypes.TryParse(Attr.Value, out b) && b)
							FieldTypes |= FieldType.HistoricalMonth;
						break;

					case "historicalQuarter":
						if (CommonTypes.TryParse(Attr.Value, out b) && b)
							FieldTypes |= FieldType.HistoricalQuarter;
						break;

					case "historicalYear":
						if (CommonTypes.TryParse(Attr.Value, out b) && b)
							FieldTypes |= FieldType.HistoricalYear;
						break;

					case "historicalOther":
						if (CommonTypes.TryParse(Attr.Value, out b) && b)
							FieldTypes |= FieldType.HistoricalOther;
						break;

					case "req":
						if (!CommonTypes.TryParse(Attr.Value, out Req))
							Req = false;
						break;
				}
			}

			foreach (XmlNode N in E.ChildNodes)
			{
				switch (N.LocalName)
				{
					case "node":
						if (Nodes == null)
							Nodes = new List<ThingReference>();

						E = (XmlElement)N;
						NodeId = XML.Attribute(E, "nodeId");
						SourceId = XML.Attribute(E, "sourceId");
						CacheType = XML.Attribute(E, "cacheType");

						ThingReference Ref = new ThingReference(NodeId, SourceId, CacheType);
						if (!Ref.IsEmpty)
						{
							throw new XMPP.StanzaErrors.BadRequestException("Device not a concentrator.", e.IQ);
							// TODO: Concentrator-support, and check which node references are valid and which aren't.
						}

						Nodes.Add(Ref);
						break;

					case "field":
						if (Fields == null)
							Fields = new Dictionary<string, FieldSubscriptionRule>();

						string FieldName = null;
						double? CurrentValue = null;
						double? ChangedBy = null;
						double? ChangedUp = null;
						double? ChangedDown = null;
						double d;

						foreach (XmlAttribute Attr in N.Attributes)
						{
							switch (Attr.Name)
							{
								case "name":
									FieldName = Attr.Value;
									break;

								case "currentValue":
									if (CommonTypes.TryParse(Attr.Value, out d))
										CurrentValue = d;
									break;

								case "changedBy":
									if (CommonTypes.TryParse(Attr.Value, out d))
										ChangedBy = d;
									break;

								case "changedUp":
									if (CommonTypes.TryParse(Attr.Value, out d))
										ChangedUp = d;
									break;

								case "changedDown":
									if (CommonTypes.TryParse(Attr.Value, out d))
										ChangedDown = d;
									break;
							}
						}

						if (!string.IsNullOrEmpty(FieldName))
							Fields[FieldName] = new FieldSubscriptionRule(FieldName, CurrentValue, ChangedBy, ChangedUp, ChangedDown);

						break;
				}
			}

			if (this.provisioningClient != null)
			{
				this.provisioningClient.CanRead(e.FromBareJid, FieldTypes, Nodes, Fields.Keys,
					ServiceToken.Split(space, StringSplitOptions.RemoveEmptyEntries),
					DeviceToken.Split(space, StringSplitOptions.RemoveEmptyEntries),
					UserToken.Split(space, StringSplitOptions.RemoveEmptyEntries),
					(sender2, e2) =>
					{
						if (e2.Ok && e2.CanRead)
						{
							if (e2.FieldsNames != null)
							{
								Dictionary<string, bool> FieldNames = new Dictionary<string, bool>();

								foreach (string FieldName in FieldNames.Keys)
									FieldNames[FieldName] = true;

								LinkedList<string> ToRemove = null;

								foreach (string FieldName in Fields.Keys)
								{
									if (!FieldNames.ContainsKey(FieldName))
									{
										if (ToRemove == null)
											ToRemove = new LinkedList<string>();

										ToRemove.AddLast(FieldName);
									}
								}

								if (ToRemove != null)
								{
									foreach (string FieldName in ToRemove)
										Fields.Remove(FieldName);
								}
							}

							this.PerformSubscription(Req, e, SeqNr, Fields, e2.Nodes, e2.FieldTypes,
								ServiceToken, DeviceToken, UserToken, MaxAge, MinInterval, MaxInterval);
						}
						else
						{
							e.IqError("<error type='cancel'><forbidden xmlns='urn:ietf:params:xml:ns:xmpp-stanzas' />" +
								"<text xmlns='urn:ietf:params:xml:ns:xmpp-stanzas' xml:lang='en'>Access denied.</text></error>");
						}

					}, null);
			}
			else
			{
				this.PerformSubscription(Req, e, SeqNr, Fields, Nodes?.ToArray(), FieldTypes,
					ServiceToken, DeviceToken, UserToken, MaxAge, MinInterval, MaxInterval);
			}
		}

		private void PerformSubscription(bool Req, IqEventArgs e, int SeqNr, Dictionary<string, FieldSubscriptionRule> FieldNames,
			ThingReference[] Nodes, FieldType FieldTypes, string ServiceToken, string DeviceToken, string UserToken,
			Duration MaxAge, Duration MinInterval, Duration MaxInterval)
		{ 
			DateTime Now = DateTime.Now;
			Subscription Subscription;

			if (Req)
			{
				string Key = e.From + " " + SeqNr.ToString();
				string[] Fields2;

				if (FieldNames == null)
					Fields2 = null;
				else
				{
					Fields2 = new string[FieldNames.Count];
					FieldNames.Keys.CopyTo(Fields2, 0);
				}

				SensorDataServerRequest Request = new SensorDataServerRequest(SeqNr, this, e.From, e.From,
					Nodes, FieldTypes, Fields2, DateTime.MinValue, DateTime.MaxValue, DateTime.MinValue,
					ServiceToken, DeviceToken, UserToken);
				bool NewRequest;

				lock (this.requests)
				{
					if (NewRequest = !this.requests.ContainsKey(Key))
						this.requests[Key] = Request;
				}

				if (Request.When > Now)
				{
					e.IqResult("<accepted xmlns='" + SensorClient.NamespaceSensorData + "' seqnr='" + SeqNr.ToString() + "' queued='true'/>");
					Request.When = this.scheduler.Add(Request.When, this.StartReadout, Request);
				}
				else
				{
					e.IqResult("<accepted xmlns='" + SensorClient.NamespaceSensorData + "' seqnr='" + SeqNr.ToString() + "'/>");
					this.PerformReadout(Request);
				}
			}

			if (Nodes == null)
				Nodes = new ThingReference[] { ThingReference.Empty };

			lock (this.subscriptionsByThing)
			{
				Subscription = new Subscription(SeqNr, e.From, Nodes, FieldNames, FieldTypes, MaxAge, MinInterval, MaxInterval,
					ServiceToken, DeviceToken, UserToken);

				foreach (ThingReference Thing in Nodes)
				{
					if (!subscriptionsByThing.TryGetValue(Thing, out LinkedList<Subscription> Subscriptions))
					{
						Subscriptions = new LinkedList<Subscription>();
						subscriptionsByThing[Thing] = Subscriptions;
					}

					LinkedListNode<Subscription> Loop = Subscriptions.First;
					while (Loop != null)
					{
						if (Loop.Value.From == e.From)
						{
							if (Loop.Value.RemoveNode(Thing))
								this.RemoveSubscriptionLocked(e.From, Loop.Value.SeqNr, false);

							Subscriptions.Remove(Loop);
							break;
						}
					}

					Subscriptions.AddLast(Subscription);
				}
			}

			if (!Req)
				e.IqResult("<accepted xmlns='" + SensorClient.NamespaceSensorData + "' seqnr='" + SeqNr.ToString() + "'/>");

			this.UpdateSubscriptionTimers(Now, Subscription);
		}

		internal void UpdateSubscriptionTimers(DateTime ReferenceTimestamp, Subscription Subscription)
		{
			Duration D;

			if ((D = Subscription.MinInterval) != null)
				this.scheduler.Add(ReferenceTimestamp + D, this.CheckMinInterval, Subscription);

			if ((D = Subscription.MaxInterval) != null)
				this.scheduler.Add(ReferenceTimestamp + D, this.CheckMaxInterval, Subscription);
		}

		private void CheckMinInterval(object P)
		{
			Subscription Subscription = (Subscription)P;

			if (Subscription.SupressedTrigger)
			{
				Subscription.SupressedTrigger = false;
				Subscription.LastTrigger = Subscription.LastTrigger + Subscription.MinInterval;
				this.TriggerSubscription(Subscription);
			}
		}

		private void CheckMaxInterval(object P)
		{
			Subscription Subscription = (Subscription)P;
			DateTime TP = Subscription.LastTrigger + Subscription.MaxInterval;

			if (TP <= DateTime.Now)
			{
				Subscription.LastTrigger = TP;
				this.TriggerSubscription(Subscription);
			}
			else
				this.scheduler.Add(Subscription.LastTrigger + Subscription.MaxInterval, this.CheckMaxInterval, Subscription);
		}

		private bool RemoveSubscriptionLocked(string From, int SeqNr, bool RemoveFromThings)
		{
			if (!this.subscriptionsByJID.TryGetValue(From, out Dictionary<int, Subscription> BySeqNr))
				return false;

			if (!BySeqNr.TryGetValue(SeqNr, out Subscription Subscription))
				return false;

			if (!RemoveFromThings)
				return true;

			Subscription.Active = false;

			foreach (ThingReference Ref in Subscription.Nodes)
			{
				if (!this.subscriptionsByThing.TryGetValue(Ref, out LinkedList<Subscription> Subscriptions))
					continue;

				if (!Subscriptions.Remove(Subscription))
					continue;

				if (Subscriptions.First == null)
					this.subscriptionsByThing.Remove(Ref);
			}

			return true;
		}

		private void UnsubscribeHandler(object Sender, IqEventArgs e)
		{
			int SeqNr = XML.Attribute(e.Query, "seqnr", 0);

			lock (this.subscriptionsByThing)
			{
				this.RemoveSubscriptionLocked(e.From, SeqNr, true);
			}

			e.IqResult(string.Empty);
		}

		private Dictionary<ThingReference, LinkedList<Subscription>> subscriptionsByThing = new Dictionary<ThingReference, LinkedList<Subscription>>();
		private Dictionary<string, Dictionary<int, Subscription>> subscriptionsByJID = new Dictionary<string, Dictionary<int, Subscription>>();

		/// <summary>
		/// Reports newly measured values.
		/// </summary>
		/// <param name="Values">New momentary values.</param>
		public void NewMomentaryValues(params Field[] Values)
		{
			this.NewMomentaryValues(null, Values, null);
		}

		/// <summary>
		/// Reports newly measured values.
		/// </summary>
		/// <param name="Reference">Optional node reference</param>
		/// <param name="Values">New momentary values.</param>
		public void NewMomentaryValues(ThingReference Reference, params Field[] Values)
		{
			this.NewMomentaryValues(Reference, Values, null);
		}

		/// <summary>
		/// Reports newly measured values.
		/// </summary>
		/// <param name="Values">New momentary values.</param>
		public void NewMomentaryValues(IEnumerable<Field> Values)
		{
			this.NewMomentaryValues(null, Values, null);
		}

		/// <summary>
		/// Reports newly measured values.
		/// </summary>
		/// <param name="Reference">Optional node reference</param>
		/// <param name="Values">New momentary values.</param>
		public void NewMomentaryValues(ThingReference Reference, IEnumerable<Field> Values)
		{
			this.NewMomentaryValues(Reference, Values, null);
		}


		/// <summary>
		/// Reports newly measured values.
		/// </summary>
		/// <param name="Reference">Optional node reference</param>
		/// <param name="Values">New momentary values.</param>
		/// <param name="ExceptJID">Only check subscriptions not from this JID.</param>
		internal void NewMomentaryValues(ThingReference Reference, IEnumerable<Field> Values, string ExceptJID)
		{
			LinkedList<Subscription> Triggered = null;

			if (Reference == null)
				Reference = ThingReference.Empty;

			lock (this.subscriptionsByThing)
			{
				if (!this.subscriptionsByThing.TryGetValue(Reference, out LinkedList<Subscription> Subscriptions))
					return;

				foreach (Subscription Subscription in Subscriptions)
				{
					if (Subscription.From == ExceptJID)
					{
						Subscription.LastTrigger = DateTime.Now;
						continue;
					}

					if (!Subscription.IsTriggered(Values))
						continue;

					if (Triggered == null)
						Triggered = new LinkedList<Subscription>();

					Triggered.AddLast(Subscription);
				}
			}

			if (Triggered != null)
			{
				foreach (Subscription Subscription in Triggered)
					this.TriggerSubscription(Subscription);
			}
		}

		private void TriggerSubscription(Subscription Subscription)
		{
			string Key = Subscription.From + " " + Subscription.SeqNr.ToString();

			SensorDataServerRequest Request = new SensorDataServerRequest(Subscription.SeqNr, this, Subscription.From,
				Subscription.From, Subscription.NodeReferences, Subscription.FieldTypes, Subscription.FieldNames,
				DateTime.MinValue, DateTime.MaxValue, DateTime.MinValue, Subscription.ServiceToken, Subscription.DeviceToken,
				Subscription.UserToken);
			bool NewRequest;

			lock (this.requests)
			{
				if (NewRequest = !this.requests.ContainsKey(Key))
					this.requests[Key] = Request;
			}

			this.PerformReadout(Request);
			this.UpdateSubscriptionTimers(Subscription.LastTrigger, Subscription);
		}

		/// <summary>
		/// If there are subscriptions registered for a given node.
		/// </summary>
		/// <param name="Reference">Node reference.</param>
		/// <returns>If there are subscriptions for the current node.</returns>
		public bool HasSubscriptions(ThingReference Reference)
		{
			lock (this.subscriptionsByThing)
			{
				return this.subscriptionsByThing.ContainsKey(Reference);
			}
		}

	}
}
