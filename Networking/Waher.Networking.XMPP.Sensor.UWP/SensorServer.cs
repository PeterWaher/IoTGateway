using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content;
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
	/// </summary>
	public class SensorServer : IDisposable
	{
		private Dictionary<string, SensorDataServerRequest> requests = new Dictionary<string, SensorDataServerRequest>();
#if WINDOWS_UWP
		private Scheduler scheduler = new Scheduler();
#else
		private Scheduler scheduler = new Scheduler(System.Threading.ThreadPriority.BelowNormal, "XMPP Sensor Data Scheduled Readout Thread");
#endif
		private XmppClient client;

		/// <summary>
		/// Implements an XMPP sensor server interface.
		/// 
		/// The interface is defined in XEP-0323:
		/// http://xmpp.org/extensions/xep-0323.html
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		public SensorServer(XmppClient Client)
		{
			this.client = Client;

			this.client.RegisterIqGetHandler("req", SensorClient.NamespaceSensorData, this.ReqHandler, true);
			this.client.RegisterIqSetHandler("req", SensorClient.NamespaceSensorData, this.ReqHandler, false);
			this.client.RegisterIqGetHandler("cancel", SensorClient.NamespaceSensorData, this.CancelHandler, false);
			this.client.RegisterIqSetHandler("cancel", SensorClient.NamespaceSensorData, this.CancelHandler, false);
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

			// TODO: Check with provisioning if permitted, and reduce request if necessary.

			string Key = e.From + " " + SeqNr.ToString();
			SensorDataServerRequest Request = new SensorDataServerRequest(SeqNr, this, e.From, e.From, Nodes == null ? null : Nodes.ToArray(), FieldTypes,
				Fields == null ? null : Fields.ToArray(), From, To, When, ServiceToken, DeviceToken, UserToken);
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
			SensorDataReadoutEventHandler h = this.OnExecuteReadoutRequest;
			if (h != null)
				h(this, Request);

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

	}
}
