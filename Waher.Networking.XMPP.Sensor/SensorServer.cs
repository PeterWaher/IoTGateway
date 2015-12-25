using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Things;
using Waher.Things.SensorData;
using Waher.Networking.Timing;

namespace Waher.Networking.XMPP.Sensor
{
	/// <summary>
	/// Delegate for sensor data readout request events, on a sensor server.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="Request">Readout request to process.</param>
	public delegate void SensorDataReadoutEventHandler(SensorServer Sender, SensorDataServerRequest Request);

	/// <summary>
	/// Implements an XMPP sensor server interface.
	/// 
	/// The interface is defined in XEP-0323:
	/// http://xmpp.org/extensions/xep-0323.html
	/// </summary>
	public class SensorServer
	{
		private Dictionary<string, SensorDataServerRequest> requests = new Dictionary<string, SensorDataServerRequest>();
		private Scheduler scheduler = new Scheduler(System.Threading.ThreadPriority.BelowNormal, "XMPP Sensor Data Scheduled Readout Thread");
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
			this.client.RegisterIqSetHandler("req", SensorClient.NamespaceSensorData, this.ReqHandler, true);
			this.client.RegisterIqGetHandler("cancel", SensorClient.NamespaceSensorData, this.CancelHandler, false);
			this.client.RegisterIqSetHandler("cancel", SensorClient.NamespaceSensorData, this.CancelHandler, false);
		}

		/// <summary>
		/// XMPP Client
		/// </summary>
		public XmppClient Client
		{
			get { return this.client; }
		}

		private void ReqHandler(XmppClient Client, IqEventArgs e)
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
						if (!CommonTypes.TryParse(Attr.Value, out From))
							From = DateTime.MinValue;
						break;

					case "to":
						if (!CommonTypes.TryParse(Attr.Value, out To))
							To = DateTime.MaxValue;
						break;

					case "when":
						if (!CommonTypes.TryParse(Attr.Value, out When))
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
						NodeId = CommonTypes.XmlAttribute(E, "nodeId");
						SourceId = CommonTypes.XmlAttribute(E, "sourceId");
						CacheType = CommonTypes.XmlAttribute(E, "cacheType");

						Nodes.Add(new ThingReference(NodeId, SourceId, CacheType));
						break;

					case "field":
						if (Fields == null)
							Fields = new List<string>();

						Fields.Add(N.InnerText);
						break;
				}
			}

			// TODO: Check with provisioning if permitted, and reduce request if necessary.

			string Key = e.From + " " + SeqNr.ToString();
			SensorDataServerRequest Request = new SensorDataServerRequest(SeqNr, this, e.From, Nodes == null ? null : Nodes.ToArray(), FieldTypes,
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
		/// Event raised when a readout request is to be executed.
		/// </summary>
		public event SensorDataReadoutEventHandler OnExecuteReadoutRequest = null;

		private void CancelHandler(XmppClient Client, IqEventArgs e)
		{
			SensorDataServerRequest Request;
			int SeqNr = CommonTypes.XmlAttribute(e.Query, "seqnr", 0);
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
