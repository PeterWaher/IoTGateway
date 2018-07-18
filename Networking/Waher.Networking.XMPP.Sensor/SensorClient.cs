using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Things;
using Waher.Things.SensorData;

namespace Waher.Networking.XMPP.Sensor
{
	/// <summary>
	/// Implements an XMPP sensor client interface.
	/// 
	/// The interface is defined in the IEEE XMPP IoT extensions:
	/// https://gitlab.com/IEEE-SA/XMPPI/IoT
	/// </summary>
	public class SensorClient : XmppExtension
	{
		/// <summary>
		/// urn:ieee:iot:sd:1.0
		/// </summary>
		public const string NamespaceSensorData = "urn:ieee:iot:sd:1.0";

		/// <summary>
		/// urn:ieee:iot:events:1.0
		/// </summary>
		public const string NamespaceSensorEvents = "urn:ieee:iot:events:1.0";

		private readonly Dictionary<string, SensorDataClientRequest> requests = new Dictionary<string, SensorDataClientRequest>();
		private readonly object synchObj = new object();

		/// <summary>
		/// Implements an XMPP sensor client interface.
		/// 
		/// The interface is defined in the IEEE XMPP IoT extensions:
		/// https://gitlab.com/IEEE-SA/XMPPI/IoT
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		public SensorClient(XmppClient Client)
			: base(Client)
		{
			this.client.RegisterMessageHandler("started", NamespaceSensorData, this.StartedHandler, false);
			this.client.RegisterMessageHandler("done", NamespaceSensorData, this.DoneHandler, false);
			this.client.RegisterMessageHandler("resp", NamespaceSensorData, this.FieldsHandler, false);
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();

			this.client.UnregisterMessageHandler("started", NamespaceSensorData, this.StartedHandler, false);
			this.client.UnregisterMessageHandler("done", NamespaceSensorData, this.DoneHandler, false);
			this.client.UnregisterMessageHandler("resp", NamespaceSensorData, this.FieldsHandler, false);
		}

		/// <summary>
		/// Implemented extensions.
		/// </summary>
		public override string[] Extensions => new string[] { "XEP-0323" };

		/// <summary>
		/// Requests a sensor data readout.
		/// </summary>
		/// <param name="Destination">JID of sensor to read.</param>
		/// <param name="Types">Field Types to read.</param>
		/// <returns>Request object maintaining the current status of the request.</returns>
		public SensorDataClientRequest RequestReadout(string Destination, FieldType Types)
		{
			return this.RequestReadout(Destination, null, Types, null, DateTime.MinValue, DateTime.MaxValue, DateTime.MinValue, string.Empty, string.Empty, string.Empty);
		}

		/// <summary>
		/// Requests a sensor data readout.
		/// </summary>
		/// <param name="Destination">JID of sensor to read.</param>
		/// <param name="Types">Field Types to read.</param>
		/// <param name="Fields">Fields to read.</param>
		/// <returns>Request object maintaining the current status of the request.</returns>
		public SensorDataClientRequest RequestReadout(string Destination, string[] Fields, FieldType Types)
		{
			return this.RequestReadout(Destination, null, Types, Fields, DateTime.MinValue, DateTime.MaxValue, DateTime.MinValue, string.Empty, string.Empty, string.Empty);
		}

		/// <summary>
		/// Requests a sensor data readout.
		/// </summary>
		/// <param name="Destination">JID of sensor to read.</param>
		/// <param name="Types">Field Types to read.</param>
		/// <param name="Fields">Fields to read.</param>
		/// <param name="From">From what time readout is to be made. Use <see cref="DateTime.MinValue"/> to specify no lower limit.</param>
		/// <returns>Request object maintaining the current status of the request.</returns>
		public SensorDataClientRequest RequestReadout(string Destination, FieldType Types, string[] Fields, DateTime From)
		{
			return this.RequestReadout(Destination, null, Types, Fields, From, DateTime.MaxValue, DateTime.MinValue, string.Empty, string.Empty, string.Empty);
		}

		/// <summary>
		/// Requests a sensor data readout.
		/// </summary>
		/// <param name="Destination">JID of sensor to read.</param>
		/// <param name="Types">Field Types to read.</param>
		/// <param name="Fields">Fields to read.</param>
		/// <param name="From">From what time readout is to be made. Use <see cref="DateTime.MinValue"/> to specify no lower limit.</param>
		/// <param name="To">To what time readout is to be made. Use <see cref="DateTime.MaxValue"/> to specify no upper limit.</param>
		/// <returns>Request object maintaining the current status of the request.</returns>
		public SensorDataClientRequest RequestReadout(string Destination, FieldType Types, string[] Fields, DateTime From, DateTime To)
		{
			return this.RequestReadout(Destination, null, Types, Fields, From, To, DateTime.MinValue, string.Empty, string.Empty, string.Empty);
		}

		/// <summary>
		/// Requests a sensor data readout.
		/// </summary>
		/// <param name="Destination">JID of sensor to read.</param>
		/// <param name="Types">Field Types to read.</param>
		/// <param name="Fields">Fields to read.</param>
		/// <param name="From">From what time readout is to be made. Use <see cref="DateTime.MinValue"/> to specify no lower limit.</param>
		/// <param name="To">To what time readout is to be made. Use <see cref="DateTime.MaxValue"/> to specify no upper limit.</param>
		/// <param name="When">When the readout is to be made. Use <see cref="DateTime.MinValue"/> to start the readout immediately.</param>
		/// <returns>Request object maintaining the current status of the request.</returns>
		public SensorDataClientRequest RequestReadout(string Destination, FieldType Types, string[] Fields, DateTime From, DateTime To, DateTime When)
		{
			return this.RequestReadout(Destination, null, Types, Fields, From, To, When, string.Empty, string.Empty, string.Empty);
		}

		/// <summary>
		/// Requests a sensor data readout.
		/// </summary>
		/// <param name="Destination">JID of sensor to read.</param>
		/// <param name="Types">Field Types to read.</param>
		/// <param name="Fields">Fields to read.</param>
		/// <param name="From">From what time readout is to be made. Use <see cref="DateTime.MinValue"/> to specify no lower limit.</param>
		/// <param name="To">To what time readout is to be made. Use <see cref="DateTime.MaxValue"/> to specify no upper limit.</param>
		/// <param name="When">When the readout is to be made. Use <see cref="DateTime.MinValue"/> to start the readout immediately.</param>
		/// <param name="ServiceToken">Optional service token.</param>
		/// <param name="DeviceToken">Optional device token.</param>
		/// <param name="UserToken">Optional user token.</param>
		/// <returns>Request object maintaining the current status of the request.</returns>
		public SensorDataClientRequest RequestReadout(string Destination, FieldType Types, string[] Fields, DateTime From, DateTime To, DateTime When,
			string ServiceToken, string DeviceToken, string UserToken)
		{
			return this.RequestReadout(Destination, null, Types, Fields, From, To, When, ServiceToken, DeviceToken, UserToken);
		}

		/// <summary>
		/// Requests a sensor data readout.
		/// </summary>
		/// <param name="Destination">JID of sensor or concentrator containing the thing(s) to read.</param>
		/// <param name="Nodes">Array of nodes to read. Can be null or empty, if reading a sensor that is not a concentrator.</param>
		/// <param name="Types">Field Types to read.</param>
		/// <returns>Request object maintaining the current status of the request.</returns>
		public SensorDataClientRequest RequestReadout(string Destination, ThingReference[] Nodes, FieldType Types)
		{
			return this.RequestReadout(Destination, Nodes, Types, null, DateTime.MinValue, DateTime.MaxValue, DateTime.MinValue, string.Empty, string.Empty, string.Empty);
		}

		/// <summary>
		/// Requests a sensor data readout.
		/// </summary>
		/// <param name="Destination">JID of sensor or concentrator containing the thing(s) to read.</param>
		/// <param name="Nodes">Array of nodes to read. Can be null or empty, if reading a sensor that is not a concentrator.</param>
		/// <param name="Types">Field Types to read.</param>
		/// <param name="Fields">Fields to read.</param>
		/// <returns>Request object maintaining the current status of the request.</returns>
		public SensorDataClientRequest RequestReadout(string Destination, ThingReference[] Nodes, string[] Fields, FieldType Types)
		{
			return this.RequestReadout(Destination, Nodes, Types, Fields, DateTime.MinValue, DateTime.MaxValue, DateTime.MinValue, string.Empty, string.Empty, string.Empty);
		}

		/// <summary>
		/// Requests a sensor data readout.
		/// </summary>
		/// <param name="Destination">JID of sensor or concentrator containing the thing(s) to read.</param>
		/// <param name="Nodes">Array of nodes to read. Can be null or empty, if reading a sensor that is not a concentrator.</param>
		/// <param name="Types">Field Types to read.</param>
		/// <param name="Fields">Fields to read.</param>
		/// <param name="From">From what time readout is to be made. Use <see cref="DateTime.MinValue"/> to specify no lower limit.</param>
		/// <returns>Request object maintaining the current status of the request.</returns>
		public SensorDataClientRequest RequestReadout(string Destination, ThingReference[] Nodes, FieldType Types, string[] Fields, DateTime From)
		{
			return this.RequestReadout(Destination, Nodes, Types, Fields, From, DateTime.MaxValue, DateTime.MinValue, string.Empty, string.Empty, string.Empty);
		}

		/// <summary>
		/// Requests a sensor data readout.
		/// </summary>
		/// <param name="Destination">JID of sensor or concentrator containing the thing(s) to read.</param>
		/// <param name="Nodes">Array of nodes to read. Can be null or empty, if reading a sensor that is not a concentrator.</param>
		/// <param name="Types">Field Types to read.</param>
		/// <param name="Fields">Fields to read.</param>
		/// <param name="From">From what time readout is to be made. Use <see cref="DateTime.MinValue"/> to specify no lower limit.</param>
		/// <param name="To">To what time readout is to be made. Use <see cref="DateTime.MaxValue"/> to specify no upper limit.</param>
		/// <returns>Request object maintaining the current status of the request.</returns>
		public SensorDataClientRequest RequestReadout(string Destination, ThingReference[] Nodes, FieldType Types, string[] Fields, DateTime From, DateTime To)
		{
			return this.RequestReadout(Destination, Nodes, Types, Fields, From, To, DateTime.MinValue, string.Empty, string.Empty, string.Empty);
		}

		/// <summary>
		/// Requests a sensor data readout.
		/// </summary>
		/// <param name="Destination">JID of sensor or concentrator containing the thing(s) to read.</param>
		/// <param name="Nodes">Array of nodes to read. Can be null or empty, if reading a sensor that is not a concentrator.</param>
		/// <param name="Types">Field Types to read.</param>
		/// <param name="Fields">Fields to read.</param>
		/// <param name="From">From what time readout is to be made. Use <see cref="DateTime.MinValue"/> to specify no lower limit.</param>
		/// <param name="To">To what time readout is to be made. Use <see cref="DateTime.MaxValue"/> to specify no upper limit.</param>
		/// <param name="When">When the readout is to be made. Use <see cref="DateTime.MinValue"/> to start the readout immediately.</param>
		/// <returns>Request object maintaining the current status of the request.</returns>
		public SensorDataClientRequest RequestReadout(string Destination, ThingReference[] Nodes, FieldType Types, string[] Fields, DateTime From, DateTime To, DateTime When)
		{
			return this.RequestReadout(Destination, Nodes, Types, Fields, From, To, When, string.Empty, string.Empty, string.Empty);
		}

		/// <summary>
		/// Requests a sensor data readout.
		/// </summary>
		/// <param name="Destination">JID of sensor or concentrator containing the thing(s) to read.</param>
		/// <param name="Nodes">Array of nodes to read. Can be null or empty, if reading a sensor that is not a concentrator.</param>
		/// <param name="Types">Field Types to read.</param>
		/// <param name="Fields">Fields to read.</param>
		/// <param name="From">From what time readout is to be made. Use <see cref="DateTime.MinValue"/> to specify no lower limit.</param>
		/// <param name="To">To what time readout is to be made. Use <see cref="DateTime.MaxValue"/> to specify no upper limit.</param>
		/// <param name="When">When the readout is to be made. Use <see cref="DateTime.MinValue"/> to start the readout immediately.</param>
		/// <param name="ServiceToken">Optional service token.</param>
		/// <param name="DeviceToken">Optional device token.</param>
		/// <param name="UserToken">Optional user token.</param>
		/// <returns>Request object maintaining the current status of the request.</returns>
		public SensorDataClientRequest RequestReadout(string Destination, IThingReference[] Nodes, FieldType Types, string[] Fields, DateTime From, DateTime To, DateTime When,
			string ServiceToken, string DeviceToken, string UserToken)
		{
			StringBuilder Xml = new StringBuilder();
			string Id = Guid.NewGuid().ToString().Replace("-", string.Empty);

			lock (this.synchObj)
			{
				this.requests[Id] = null;
			}

			Xml.Append("<req xmlns='");
			Xml.Append(NamespaceSensorData);
			Xml.Append("' id='");
			Xml.Append(XML.Encode(Id));

			if ((Types & FieldType.All) == FieldType.All)
				Xml.Append("' all='true");
			else
			{
				if (Types.HasFlag(FieldType.Momentary))
					Xml.Append("' m='true");

				if (Types.HasFlag(FieldType.Peak))
					Xml.Append("' p='true");

				if (Types.HasFlag(FieldType.Status))
					Xml.Append("' s='true");

				if (Types.HasFlag(FieldType.Computed))
					Xml.Append("' c='true");

				if (Types.HasFlag(FieldType.Identity))
					Xml.Append("' i='true");

				if (Types.HasFlag(FieldType.Historical))
					Xml.Append("' h='true");
			}

			if (From != DateTime.MinValue)
			{
				Xml.Append("' from='");
				Xml.Append(XML.Encode(From));
			}

			if (To != DateTime.MaxValue)
			{
				Xml.Append("' to='");
				Xml.Append(XML.Encode(To));
			}

			if (When != DateTime.MinValue)
			{
				Xml.Append("' when='");
				Xml.Append(XML.Encode(When));
			}

			if (!string.IsNullOrEmpty(ServiceToken))
			{
				Xml.Append("' st='");
				Xml.Append(ServiceToken);
			}

			if (!string.IsNullOrEmpty(DeviceToken))
			{
				Xml.Append("' dt='");
				Xml.Append(DeviceToken);
			}

			if (!string.IsNullOrEmpty(UserToken))
			{
				Xml.Append("' ut='");
				Xml.Append(UserToken);
			}

			Xml.Append("'>");

			if (Nodes != null)
			{
				foreach (IThingReference Node in Nodes)
				{
					Xml.Append("<nd id='");
					Xml.Append(XML.Encode(Node.NodeId));

					if (!string.IsNullOrEmpty(Node.SourceId))
					{
						Xml.Append("' src='");
						Xml.Append(XML.Encode(Node.SourceId));
					}

					if (!string.IsNullOrEmpty(Node.Partition))
					{
						Xml.Append("' pt='");
						Xml.Append(XML.Encode(Node.Partition));
					}

					Xml.Append("'/>");
				}
			}

			if (Fields != null)
			{
				foreach (string Field in Fields)
				{
					Xml.Append("<f n='");
					Xml.Append(XML.Encode(Field));
					Xml.Append("'/>");
				}
			}

			Xml.Append("</req>");

			SensorDataClientRequest Request = new SensorDataClientRequest(Id, this, Destination, Destination, Nodes, Types, Fields, From, To, When,
				ServiceToken, DeviceToken, UserToken);

			lock (this.requests)
			{
				this.requests[Id] = Request;
			}

			this.client.SendIqGet(Destination, Xml.ToString(), this.RequestResponse, Request);

			return Request;
		}

		private void RequestResponse(object Sender, IqResultEventArgs e)
		{
			SensorDataClientRequest Request = (SensorDataClientRequest)e.State;

			if (e.Ok)
			{
				foreach (XmlNode N in e.Response.ChildNodes)
				{
					switch (N.LocalName)
					{
						case "accepted":
							XmlElement E = (XmlElement)N;
							string Id = XML.Attribute(E, "id");
							bool Queued = XML.Attribute(E, "queued", false);

							if (Id == Request.Id)
								Request.Accept(Queued);
							else
								Request.Fail("Request identity mismatch.");

							return;

						case "started":
							E = (XmlElement)N;
							Id = XML.Attribute(E, "id");

							if (Id == Request.Id)
							{
								Request.Accept(false);
								Request.Started();
							}
							else
								Request.Fail("Request identity mismatch.");

							return;

						case "resp":
							E = (XmlElement)N;
							Id = XML.Attribute(E, "id");

							if (Id == Request.Id)
								this.ProcessFields(E, Request);
							else
								Request.Fail("Request identity mismatch.");

							return;
					}
				}

				Request.Fail("Invalid response to request.");
			}
			else
				Request.Fail(e.ErrorText);
		}

		private void StartedHandler(object Sender, MessageEventArgs e)
		{
			SensorDataClientRequest Request;
			string Id = XML.Attribute(e.Content, "id");

			lock (this.requests)
			{
				if (!this.requests.TryGetValue(Id, out Request))
					return;
			}

			Request.Started();
		}

		private void DoneHandler(object Sender, MessageEventArgs e)
		{
			SensorDataClientRequest Request;
			string Id = XML.Attribute(e.Content, "id");

			lock (this.requests)
			{
				if (!this.requests.TryGetValue(Id, out Request))
					return;
			}

			Request.Done();
		}

		private void AssertReceiving(SensorDataClientRequest Request)
		{
			switch (Request.State)
			{
				case SensorDataReadoutState.Requested:
					Request.State = SensorDataReadoutState.Accepted;
					Request.State = SensorDataReadoutState.Started;
					Request.State = SensorDataReadoutState.Receiving;
					break;

				case SensorDataReadoutState.Accepted:
					Request.State = SensorDataReadoutState.Started;
					Request.State = SensorDataReadoutState.Receiving;
					break;

				case SensorDataReadoutState.Started:
					Request.State = SensorDataReadoutState.Receiving;
					break;

				case SensorDataReadoutState.Failure:
				case SensorDataReadoutState.Done:
					Request.Clear();
					Request.State = SensorDataReadoutState.Receiving;
					break;
			}
		}

		private void FieldsHandler(object Sender, MessageEventArgs e)
		{
			SensorDataClientRequest Request;
			string Id = XML.Attribute(e.Content, "id");

			lock (this.requests)
			{
				if (!this.requests.TryGetValue(Id, out Request))
					return;
			}

			this.ProcessFields(e.Content, Request);
		}

		private void ProcessFields(XmlElement Content, SensorDataClientRequest Request)
		{
			this.AssertReceiving(Request);

			Tuple<List<Field>, List<ThingError>> Response = ParseFields(Content, out bool Done);

			if (Response.Item1 != null)
				Request.LogFields(Response.Item1);

			if (Response.Item2 != null)
				Request.LogErrors(Response.Item2);

			if (Done)
				Request.Done();
		}

		/// <summary>
		/// Parses sensor data field definitions.
		/// </summary>
		/// <param name="Content">Fields element containing sensor data as defined in the IEEE XMPP IoT extensions.</param>
		/// <returns>Parsed fields.</returns>
		public static SensorData ParseFields(XmlElement Content)
		{
			Tuple<List<Field>, List<ThingError>> Response = ParseFields(Content, out bool Done, out string Id);

			return new SensorData()
			{
				Done = Done,
				Errors = Response.Item2,
				Fields = Response.Item1,
				Id = Id
			};
		}

		/// <summary>
		/// Parses sensor data field definitions.
		/// </summary>
		/// <param name="Content">Fields element containing sensor data as defined in the IEEE XMPP IoT extensions.</param>
		/// <param name="Done">If sensor data readout is done.</param>
		/// <returns>Parsed fields.</returns>
		public static Tuple<List<Field>, List<ThingError>> ParseFields(XmlElement Content, out bool Done)
		{
			return ParseFields(Content, out Done, out string Id);
		}

		/// <summary>
		/// Parses sensor data field definitions.
		/// </summary>
		/// <param name="Content">Fields element containing sensor data as defined in the IEEE XMPP IoT extensions.</param>
		/// <param name="Done">If sensor data readout is done.</param>
		/// <param name="Id">Readout identity.</param>
		/// <returns>Parsed fields.</returns>
		public static Tuple<List<Field>, List<ThingError>> ParseFields(XmlElement Content, out bool Done, out string Id)
		{
			List<ThingError> Errors = null;
			List<Field> Fields = null;

			Done = !XML.Attribute(Content, "more", false);
			Id = XML.Attribute(Content, "id");

			foreach (XmlNode N in Content.ChildNodes)
			{
				if (!(N is XmlElement E))
					continue;

				switch (E.LocalName)
				{
					case "nd":
						ParseNode(E, ref Fields, ref Errors);
						break;

					case "ts":
						ParseTimespan(E, ThingReference.Empty, ref Fields, ref Errors);
						break;
				}
			}

			return new Tuple<List<Field>, List<ThingError>>(Fields, Errors);
		}

		private static void ParseNode(XmlElement E,
			ref List<Field> Fields, ref List<ThingError> Errors)
		{
			string NodeId = XML.Attribute(E, "id");
			string SourceId = XML.Attribute(E, "src");
			string Partition = XML.Attribute(E, "pt");
			ThingReference Thing = new ThingReference(NodeId, SourceId, Partition);

			foreach (XmlNode N2 in E.ChildNodes)
			{
				if (!(N2 is XmlElement E2))
					continue;

				if (E2.LocalName == "ts")
					ParseTimespan(E2, Thing, ref Fields, ref Errors);
			}
		}

		private static void ParseTimespan(XmlElement E2, ThingReference Thing,
			ref List<Field> Fields, ref List<ThingError> Errors)
		{
			DateTime Timestamp = XML.Attribute(E2, "v", DateTime.MinValue);

			foreach (XmlNode N3 in E2.ChildNodes)
			{
				if (!(N3 is XmlElement E))
					continue;

				if (E.LocalName == "err")
				{
					if (Errors == null)
						Errors = new List<ThingError>();

					Errors.Add(new ThingError(Thing, Timestamp, E.InnerText));
				}
				else
				{
					FieldType FieldTypes = (FieldType)0;
					FieldQoS FieldQoS = (FieldQoS)0;
					string FieldName = string.Empty;
					string Module = string.Empty;
					string StringIds = string.Empty;
					string ValueString = string.Empty;
					string ValueType = string.Empty;
					string Unit = string.Empty;
					bool Writable = false;

					if (Fields == null)
						Fields = new List<Field>();

					foreach (XmlAttribute Attr in E.Attributes)
					{
						switch (Attr.Name)
						{
							case "n":
								FieldName = Attr.Value;
								break;

							case "lns":
								Module = Attr.Value;
								break;

							case "loc":
								StringIds = Attr.Value;
								break;

							case "ctr":
								if (!CommonTypes.TryParse(Attr.Value, out Writable))
									Writable = false;
								break;

							case "m":
								if (CommonTypes.TryParse(Attr.Value, out bool b) && b)
									FieldTypes |= FieldType.Momentary;
								break;

							case "p":
								if (CommonTypes.TryParse(Attr.Value, out b) && b)
									FieldTypes |= FieldType.Peak;
								break;

							case "s":
								if (CommonTypes.TryParse(Attr.Value, out b) && b)
									FieldTypes |= FieldType.Status;
								break;

							case "c":
								if (CommonTypes.TryParse(Attr.Value, out b) && b)
									FieldTypes |= FieldType.Computed;
								break;

							case "i":
								if (CommonTypes.TryParse(Attr.Value, out b) && b)
									FieldTypes |= FieldType.Identity;
								break;

							case "h":
								if (CommonTypes.TryParse(Attr.Value, out b) && b)
									FieldTypes |= FieldType.Historical;
								break;

							case "ms":
								if (CommonTypes.TryParse(Attr.Value, out b) && b)
									FieldQoS |= FieldQoS.Missing;
								break;

							case "pr":
								if (CommonTypes.TryParse(Attr.Value, out b) && b)
									FieldQoS |= FieldQoS.InProgress;
								break;

							case "ae":
								if (CommonTypes.TryParse(Attr.Value, out b) && b)
									FieldQoS |= FieldQoS.AutomaticEstimate;
								break;

							case "me":
								if (CommonTypes.TryParse(Attr.Value, out b) && b)
									FieldQoS |= FieldQoS.ManualEstimate;
								break;

							case "mr":
								if (CommonTypes.TryParse(Attr.Value, out b) && b)
									FieldQoS |= FieldQoS.ManualReadout;
								break;

							case "ar":
								if (CommonTypes.TryParse(Attr.Value, out b) && b)
									FieldQoS |= FieldQoS.AutomaticReadout;
								break;

							case "of":
								if (CommonTypes.TryParse(Attr.Value, out b) && b)
									FieldQoS |= FieldQoS.TimeOffset;
								break;

							case "w":
								if (CommonTypes.TryParse(Attr.Value, out b) && b)
									FieldQoS |= FieldQoS.Warning;
								break;

							case "er":
								if (CommonTypes.TryParse(Attr.Value, out b) && b)
									FieldQoS |= FieldQoS.Error;
								break;

							case "so":
								if (CommonTypes.TryParse(Attr.Value, out b) && b)
									FieldQoS |= FieldQoS.Signed;
								break;

							case "iv":
								if (CommonTypes.TryParse(Attr.Value, out b) && b)
									FieldQoS |= FieldQoS.Invoiced;
								break;

							case "eos":
								if (CommonTypes.TryParse(Attr.Value, out b) && b)
									FieldQoS |= FieldQoS.EndOfSeries;
								break;

							case "pf":
								if (CommonTypes.TryParse(Attr.Value, out b) && b)
									FieldQoS |= FieldQoS.PowerFailure;
								break;

							case "ic":
								if (CommonTypes.TryParse(Attr.Value, out b) && b)
									FieldQoS |= FieldQoS.InvoiceConfirmed;
								break;

							case "v":
								ValueString = Attr.Value;
								break;

							case "u":
								Unit = Attr.Value;
								break;

							case "t":
								ValueType = Attr.Value;
								break;
						}
					}

					LocalizationStep[] LocalizationSteps;

					if (string.IsNullOrEmpty(StringIds))
						LocalizationSteps = null;
					else
						LocalizationSteps = ParseStringIds(StringIds);

					switch (E.LocalName)
					{
						case "b":
							if (CommonTypes.TryParse(ValueString, out bool b))
								Fields.Add(new BooleanField(Thing, Timestamp, FieldName, b, FieldTypes, FieldQoS, Writable, Module, LocalizationSteps));
							break;

						case "d":
							if (XML.TryParse(ValueString, out DateTime DT))
								Fields.Add(new DateField(Thing, Timestamp, FieldName, DT, FieldTypes, FieldQoS, Writable, Module, LocalizationSteps));
							break;

						case "dt":
							if (XML.TryParse(ValueString, out DT))
								Fields.Add(new DateTimeField(Thing, Timestamp, FieldName, DT, FieldTypes, FieldQoS, Writable, Module, LocalizationSteps));
							break;

						case "dr":
							if (Duration.TryParse(ValueString, out Duration D))
								Fields.Add(new DurationField(Thing, Timestamp, FieldName, D, FieldTypes, FieldQoS, Writable, Module, LocalizationSteps));
							break;

						case "e":
							Fields.Add(new EnumField(Thing, Timestamp, FieldName, ValueString, ValueType, FieldTypes, FieldQoS, Writable, Module, LocalizationSteps));
							break;

						case "i":
							if (int.TryParse(ValueString, out int i))
								Fields.Add(new Int32Field(Thing, Timestamp, FieldName, i, FieldTypes, FieldQoS, Writable, Module, LocalizationSteps));
							break;

						case "l":
							if (long.TryParse(ValueString, out long l))
								Fields.Add(new Int64Field(Thing, Timestamp, FieldName, l, FieldTypes, FieldQoS, Writable, Module, LocalizationSteps));
							break;

						case "q":
							if (CommonTypes.TryParse(ValueString, out double d, out byte NrDec))
								Fields.Add(new QuantityField(Thing, Timestamp, FieldName, d, NrDec, Unit, FieldTypes, FieldQoS, Writable, Module, LocalizationSteps));
							break;

						case "s":
							Fields.Add(new StringField(Thing, Timestamp, FieldName, ValueString, FieldTypes, FieldQoS, Writable, Module, LocalizationSteps));
							break;

						case "t":
							if (TimeSpan.TryParse(ValueString, out TimeSpan TS))
								Fields.Add(new TimeField(Thing, Timestamp, FieldName, TS, FieldTypes, FieldQoS, Writable, Module, LocalizationSteps));
							break;
					}
				}
			}
		}

		private static LocalizationStep[] ParseStringIds(string StringIds)
		{
			if (string.IsNullOrEmpty(StringIds))
				return null;

			if (int.TryParse(StringIds, out int StringId))
				return new LocalizationStep[1] { new LocalizationStep(StringId) };

			string[] Steps = StringIds.Split(',');
			string[] Parts;
			string Module;
			string Seed;
			int i, d, c = Steps.Length;
			LocalizationStep[] Result = new LocalizationStep[c];

			for (i = 0; i < c; i++)
			{
				Parts = Steps[i].Split('|');
				d = Parts.Length;

				if (!int.TryParse(Parts[0], out StringId))
					continue;

				if (d > 1)
				{
					Module = Parts[1];

					if (d > 2)
						Seed = Parts[2];
					else
						Seed = string.Empty;
				}
				else
				{
					Module = string.Empty;
					Seed = string.Empty;
				}

				Result[i] = new LocalizationStep(StringId, Module, Seed);
			}

			return Result;
		}

		/// <summary>
		/// Subscribes to sensor data readout.
		/// </summary>
		/// <param name="Destination">JID of sensor or concentrator containing the thing(s) to subscribe to.</param>
		/// <param name="Types">Field Types to subscribe to.</param>
		/// <param name="Fields">Fields to subscribe to, and any applicable change rules to apply to the subscription.</param>
		/// <param name="ImmediateReadout">If an immediate readout should be performed.</param>
		/// <returns>Request object maintaining the current status of the subscription.</returns>
		public SensorDataSubscriptionRequest Subscribe(string Destination, FieldType Types, FieldSubscriptionRule[] Fields,
			bool ImmediateReadout)
		{
			return this.Subscribe(Destination, null, Types, Fields, null, null, null, string.Empty, string.Empty, string.Empty, ImmediateReadout);
		}

		/// <summary>
		/// Subscribes to sensor data readout.
		/// </summary>
		/// <param name="Destination">JID of sensor or concentrator containing the thing(s) to subscribe to.</param>
		/// <param name="Types">Field Types to subscribe to.</param>
		/// <param name="MinInterval">Optional smallest acceptable event interval.</param>
		/// <param name="MaxInterval">Optional largest desired event interval.</param>
		/// <param name="ImmediateReadout">If an immediate readout should be performed.</param>
		/// <returns>Request object maintaining the current status of the subscription.</returns>
		public SensorDataSubscriptionRequest Subscribe(string Destination, FieldType Types,
			Duration MinInterval, Duration MaxInterval, bool ImmediateReadout)
		{
			return this.Subscribe(Destination, null, Types, null, MinInterval, MaxInterval, null, string.Empty, string.Empty, string.Empty, ImmediateReadout);
		}

		/// <summary>
		/// Subscribes to sensor data readout.
		/// </summary>
		/// <param name="Destination">JID of sensor or concentrator containing the thing(s) to subscribe to.</param>
		/// <param name="Types">Field Types to subscribe to.</param>
		/// <param name="Fields">Fields to subscribe to, and any applicable change rules to apply to the subscription.</param>
		/// <param name="MinInterval">Optional smallest acceptable event interval.</param>
		/// <param name="MaxInterval">Optional largest desired event interval.</param>
		/// <param name="ImmediateReadout">If an immediate readout should be performed.</param>
		/// <returns>Request object maintaining the current status of the subscription.</returns>
		public SensorDataSubscriptionRequest Subscribe(string Destination, FieldType Types, FieldSubscriptionRule[] Fields,
			Duration MinInterval, Duration MaxInterval, bool ImmediateReadout)
		{
			return this.Subscribe(Destination, null, Types, Fields, MinInterval, MaxInterval, null, string.Empty, string.Empty, string.Empty, ImmediateReadout);
		}

		/// <summary>
		/// Subscribes to sensor data readout.
		/// </summary>
		/// <param name="Destination">JID of sensor or concentrator containing the thing(s) to subscribe to.</param>
		/// <param name="Types">Field Types to subscribe to.</param>
		/// <param name="MinInterval">Optional smallest acceptable event interval.</param>
		/// <param name="MaxInterval">Optional largest desired event interval.</param>
		/// <param name="MaxAge">Optional maximum age of historical data.</param>
		/// <param name="ImmediateReadout">If an immediate readout should be performed.</param>
		/// <returns>Request object maintaining the current status of the subscription.</returns>
		public SensorDataSubscriptionRequest Subscribe(string Destination, FieldType Types,
			Duration MinInterval, Duration MaxInterval, Duration MaxAge, bool ImmediateReadout)
		{
			return this.Subscribe(Destination, null, Types, null, MinInterval, MaxInterval, MaxAge, string.Empty, string.Empty, string.Empty, ImmediateReadout);
		}

		/// <summary>
		/// Subscribes to sensor data readout.
		/// </summary>
		/// <param name="Destination">JID of sensor or concentrator containing the thing(s) to subscribe to.</param>
		/// <param name="Types">Field Types to subscribe to.</param>
		/// <param name="Fields">Fields to subscribe to, and any applicable change rules to apply to the subscription.</param>
		/// <param name="MinInterval">Optional smallest acceptable event interval.</param>
		/// <param name="MaxInterval">Optional largest desired event interval.</param>
		/// <param name="MaxAge">Optional maximum age of historical data.</param>
		/// <param name="ImmediateReadout">If an immediate readout should be performed.</param>
		/// <returns>Request object maintaining the current status of the subscription.</returns>
		public SensorDataSubscriptionRequest Subscribe(string Destination, FieldType Types, FieldSubscriptionRule[] Fields,
			Duration MinInterval, Duration MaxInterval, Duration MaxAge, bool ImmediateReadout)
		{
			return this.Subscribe(Destination, null, Types, Fields, MinInterval, MaxInterval, MaxAge, string.Empty, string.Empty, string.Empty, ImmediateReadout);
		}

		/// <summary>
		/// Subscribes to sensor data readout.
		/// </summary>
		/// <param name="Destination">JID of sensor or concentrator containing the thing(s) to subscribe to.</param>
		/// <param name="Types">Field Types to subscribe to.</param>
		/// <param name="MinInterval">Optional smallest acceptable event interval.</param>
		/// <param name="MaxInterval">Optional largest desired event interval.</param>
		/// <param name="MaxAge">Optional maximum age of historical data.</param>
		/// <param name="ServiceToken">Optional service token.</param>
		/// <param name="DeviceToken">Optional device token.</param>
		/// <param name="UserToken">Optional user token.</param>
		/// <param name="ImmediateReadout">If an immediate readout should be performed.</param>
		/// <returns>Request object maintaining the current status of the subscription.</returns>
		public SensorDataSubscriptionRequest Subscribe(string Destination, FieldType Types,
			Duration MinInterval, Duration MaxInterval, Duration MaxAge, string ServiceToken, string DeviceToken, string UserToken,
			bool ImmediateReadout)
		{
			return this.Subscribe(Destination, null, Types, null, MinInterval, MaxInterval, MaxAge, ServiceToken, DeviceToken, UserToken, ImmediateReadout);
		}

		/// <summary>
		/// Subscribes to sensor data readout.
		/// </summary>
		/// <param name="Destination">JID of sensor or concentrator containing the thing(s) to subscribe to.</param>
		/// <param name="Types">Field Types to subscribe to.</param>
		/// <param name="Fields">Fields to subscribe to, and any applicable change rules to apply to the subscription.</param>
		/// <param name="MinInterval">Optional smallest acceptable event interval.</param>
		/// <param name="MaxInterval">Optional largest desired event interval.</param>
		/// <param name="MaxAge">Optional maximum age of historical data.</param>
		/// <param name="ServiceToken">Optional service token.</param>
		/// <param name="DeviceToken">Optional device token.</param>
		/// <param name="UserToken">Optional user token.</param>
		/// <param name="ImmediateReadout">If an immediate readout should be performed.</param>
		/// <returns>Request object maintaining the current status of the subscription.</returns>
		public SensorDataSubscriptionRequest Subscribe(string Destination, FieldType Types, FieldSubscriptionRule[] Fields,
			Duration MinInterval, Duration MaxInterval, Duration MaxAge, string ServiceToken, string DeviceToken, string UserToken,
			bool ImmediateReadout)
		{
			return this.Subscribe(Destination, null, Types, Fields, MinInterval, MaxInterval, MaxAge, ServiceToken, DeviceToken, UserToken, ImmediateReadout);
		}

		/// <summary>
		/// Subscribes to sensor data readout.
		/// </summary>
		/// <param name="Destination">JID of sensor or concentrator containing the thing(s) to subscribe to.</param>
		/// <param name="Nodes">Array of nodes to subscribe to. Can be null or empty, if subscribe to a sensor that is not a concentrator.</param>
		/// <param name="Types">Field Types to subscribe to.</param>
		/// <param name="Fields">Fields to subscribe to, and any applicable change rules to apply to the subscription.</param>
		/// <param name="ImmediateReadout">If an immediate readout should be performed.</param>
		/// <returns>Request object maintaining the current status of the subscription.</returns>
		public SensorDataSubscriptionRequest Subscribe(string Destination, ThingReference[] Nodes, FieldType Types, FieldSubscriptionRule[] Fields,
			bool ImmediateReadout)
		{
			return this.Subscribe(Destination, Nodes, Types, Fields, null, null, null, string.Empty, string.Empty, string.Empty, ImmediateReadout);
		}

		/// <summary>
		/// Subscribes to sensor data readout.
		/// </summary>
		/// <param name="Destination">JID of sensor or concentrator containing the thing(s) to subscribe to.</param>
		/// <param name="Nodes">Array of nodes to subscribe to. Can be null or empty, if subscribe to a sensor that is not a concentrator.</param>
		/// <param name="Types">Field Types to subscribe to.</param>
		/// <param name="MinInterval">Optional smallest acceptable event interval.</param>
		/// <param name="MaxInterval">Optional largest desired event interval.</param>
		/// <param name="ImmediateReadout">If an immediate readout should be performed.</param>
		/// <returns>Request object maintaining the current status of the subscription.</returns>
		public SensorDataSubscriptionRequest Subscribe(string Destination, ThingReference[] Nodes, FieldType Types,
			Duration MinInterval, Duration MaxInterval, bool ImmediateReadout)
		{
			return this.Subscribe(Destination, Nodes, Types, null, MinInterval, MaxInterval, null, string.Empty, string.Empty, string.Empty, ImmediateReadout);
		}

		/// <summary>
		/// Subscribes to sensor data readout.
		/// </summary>
		/// <param name="Destination">JID of sensor or concentrator containing the thing(s) to subscribe to.</param>
		/// <param name="Nodes">Array of nodes to subscribe to. Can be null or empty, if subscribe to a sensor that is not a concentrator.</param>
		/// <param name="Types">Field Types to subscribe to.</param>
		/// <param name="Fields">Fields to subscribe to, and any applicable change rules to apply to the subscription.</param>
		/// <param name="MinInterval">Optional smallest acceptable event interval.</param>
		/// <param name="MaxInterval">Optional largest desired event interval.</param>
		/// <param name="ImmediateReadout">If an immediate readout should be performed.</param>
		/// <returns>Request object maintaining the current status of the subscription.</returns>
		public SensorDataSubscriptionRequest Subscribe(string Destination, ThingReference[] Nodes, FieldType Types, FieldSubscriptionRule[] Fields,
			Duration MinInterval, Duration MaxInterval, bool ImmediateReadout)
		{
			return this.Subscribe(Destination, Nodes, Types, Fields, MinInterval, MaxInterval, null, string.Empty, string.Empty, string.Empty, ImmediateReadout);
		}

		/// <summary>
		/// Subscribes to sensor data readout.
		/// </summary>
		/// <param name="Destination">JID of sensor or concentrator containing the thing(s) to subscribe to.</param>
		/// <param name="Nodes">Array of nodes to subscribe to. Can be null or empty, if subscribe to a sensor that is not a concentrator.</param>
		/// <param name="Types">Field Types to subscribe to.</param>
		/// <param name="MinInterval">Optional smallest acceptable event interval.</param>
		/// <param name="MaxInterval">Optional largest desired event interval.</param>
		/// <param name="MaxAge">Optional maximum age of historical data.</param>
		/// <param name="ImmediateReadout">If an immediate readout should be performed.</param>
		/// <returns>Request object maintaining the current status of the subscription.</returns>
		public SensorDataSubscriptionRequest Subscribe(string Destination, ThingReference[] Nodes, FieldType Types,
			Duration MinInterval, Duration MaxInterval, Duration MaxAge, bool ImmediateReadout)
		{
			return this.Subscribe(Destination, Nodes, Types, null, MinInterval, MaxInterval, MaxAge, string.Empty, string.Empty, string.Empty, ImmediateReadout);
		}

		/// <summary>
		/// Subscribes to sensor data readout.
		/// </summary>
		/// <param name="Destination">JID of sensor or concentrator containing the thing(s) to subscribe to.</param>
		/// <param name="Nodes">Array of nodes to subscribe to. Can be null or empty, if subscribe to a sensor that is not a concentrator.</param>
		/// <param name="Types">Field Types to subscribe to.</param>
		/// <param name="Fields">Fields to subscribe to, and any applicable change rules to apply to the subscription.</param>
		/// <param name="MinInterval">Optional smallest acceptable event interval.</param>
		/// <param name="MaxInterval">Optional largest desired event interval.</param>
		/// <param name="MaxAge">Optional maximum age of historical data.</param>
		/// <param name="ImmediateReadout">If an immediate readout should be performed.</param>
		/// <returns>Request object maintaining the current status of the subscription.</returns>
		public SensorDataSubscriptionRequest Subscribe(string Destination, ThingReference[] Nodes, FieldType Types, FieldSubscriptionRule[] Fields,
			Duration MinInterval, Duration MaxInterval, Duration MaxAge, bool ImmediateReadout)
		{
			return this.Subscribe(Destination, Nodes, Types, Fields, MinInterval, MaxInterval, MaxAge, string.Empty, string.Empty, string.Empty, ImmediateReadout);
		}

		/// <summary>
		/// Subscribes to sensor data readout.
		/// </summary>
		/// <param name="Destination">JID of sensor or concentrator containing the thing(s) to subscribe to.</param>
		/// <param name="Nodes">Array of nodes to subscribe to. Can be null or empty, if subscribe to a sensor that is not a concentrator.</param>
		/// <param name="Types">Field Types to subscribe to.</param>
		/// <param name="MinInterval">Optional smallest acceptable event interval.</param>
		/// <param name="MaxInterval">Optional largest desired event interval.</param>
		/// <param name="MaxAge">Optional maximum age of historical data.</param>
		/// <param name="ServiceToken">Optional service token.</param>
		/// <param name="DeviceToken">Optional device token.</param>
		/// <param name="UserToken">Optional user token.</param>
		/// <param name="ImmediateReadout">If an immediate readout should be performed.</param>
		/// <returns>Request object maintaining the current status of the subscription.</returns>
		public SensorDataSubscriptionRequest Subscribe(string Destination, ThingReference[] Nodes, FieldType Types,
			Duration MinInterval, Duration MaxInterval, Duration MaxAge, string ServiceToken, string DeviceToken, string UserToken,
			bool ImmediateReadout)
		{
			return this.Subscribe(Destination, Nodes, Types, null, MinInterval, MaxInterval, MaxAge, ServiceToken, DeviceToken, UserToken, ImmediateReadout);
		}

		/// <summary>
		/// Subscribes to sensor data readout.
		/// </summary>
		/// <param name="Destination">JID of sensor or concentrator containing the thing(s) to subscribe to.</param>
		/// <param name="Nodes">Array of nodes to subscribe to. Can be null or empty, if subscribe to a sensor that is not a concentrator.</param>
		/// <param name="Types">Field Types to subscribe to.</param>
		/// <param name="Fields">Fields to subscribe to, and any applicable change rules to apply to the subscription.</param>
		/// <param name="MinInterval">Optional smallest acceptable event interval.</param>
		/// <param name="MaxInterval">Optional largest desired event interval.</param>
		/// <param name="MaxAge">Optional maximum age of historical data.</param>
		/// <param name="ServiceToken">Optional service token.</param>
		/// <param name="DeviceToken">Optional device token.</param>
		/// <param name="UserToken">Optional user token.</param>
		/// <param name="ImmediateReadout">If an immediate readout should be performed.</param>
		/// <returns>Request object maintaining the current status of the subscription.</returns>
		public SensorDataSubscriptionRequest Subscribe(string Destination, IThingReference[] Nodes, FieldType Types, FieldSubscriptionRule[] Fields,
			Duration MinInterval, Duration MaxInterval, Duration MaxAge, string ServiceToken, string DeviceToken, string UserToken,
			bool ImmediateReadout)
		{
			StringBuilder Xml = new StringBuilder();
			string Id = Guid.NewGuid().ToString().Replace("-", string.Empty);

			lock (this.synchObj)
			{
				this.requests[Id] = null;
			}

			Xml.Append("<subscribe xmlns='");
			Xml.Append(NamespaceSensorEvents);
			Xml.Append("' id='");
			Xml.Append(XML.Encode(Id));

			if ((Types & FieldType.All) == FieldType.All)
				Xml.Append("' all='true");
			else
			{
				if (Types.HasFlag(FieldType.Momentary))
					Xml.Append("' m='true");

				if (Types.HasFlag(FieldType.Peak))
					Xml.Append("' p='true");

				if (Types.HasFlag(FieldType.Status))
					Xml.Append("' s='true");

				if (Types.HasFlag(FieldType.Computed))
					Xml.Append("' c='true");

				if (Types.HasFlag(FieldType.Identity))
					Xml.Append("' i='true");

				if (Types.HasFlag(FieldType.Historical))
					Xml.Append("' h='true");
			}

			if (MinInterval != null)
			{
				Xml.Append("' minInt='");
				Xml.Append(MinInterval.ToString());
			}

			if (MaxInterval != null)
			{
				Xml.Append("' maxInt='");
				Xml.Append(MaxInterval.ToString());
			}

			if (MaxAge != null)
			{
				Xml.Append("' maxAge='");
				Xml.Append(MaxAge.ToString());
			}

			if (!string.IsNullOrEmpty(ServiceToken))
			{
				Xml.Append("' st='");
				Xml.Append(ServiceToken);
			}

			if (!string.IsNullOrEmpty(DeviceToken))
			{
				Xml.Append("' dt='");
				Xml.Append(DeviceToken);
			}

			if (!string.IsNullOrEmpty(UserToken))
			{
				Xml.Append("' ut='");
				Xml.Append(UserToken);
			}

			if (ImmediateReadout)
				Xml.Append("' req='true");

			Xml.Append("'>");

			if (Nodes != null)
			{
				foreach (IThingReference Node in Nodes)
				{
					Xml.Append("<nd id='");
					Xml.Append(XML.Encode(Node.NodeId));

					if (!string.IsNullOrEmpty(Node.SourceId))
					{
						Xml.Append("' src='");
						Xml.Append(XML.Encode(Node.SourceId));
					}

					if (!string.IsNullOrEmpty(Node.Partition))
					{
						Xml.Append("' pt='");
						Xml.Append(XML.Encode(Node.Partition));
					}

					Xml.Append("'/>");
				}
			}

			if (Fields != null)
			{
				foreach (FieldSubscriptionRule Field in Fields)
				{
					Xml.Append("<f n='");
					Xml.Append(XML.Encode(Field.FieldName));

					if (Field.CurrentValue != null)
					{
						Xml.Append("' v='");

						if (Field.CurrentValue is double)
							Xml.Append(CommonTypes.Encode((double)Field.CurrentValue));
					}

					if (Field.ChangedBy.HasValue)
					{
						Xml.Append("' by='");
						Xml.Append(CommonTypes.Encode(Field.ChangedBy.Value));
					}

					if (Field.ChangedUp.HasValue)
					{
						Xml.Append("' up='");
						Xml.Append(CommonTypes.Encode(Field.ChangedUp.Value));
					}

					if (Field.ChangedDown.HasValue)
					{
						Xml.Append("' dn='");
						Xml.Append(CommonTypes.Encode(Field.ChangedDown.Value));
					}

					Xml.Append("'/>");
				}
			}

			Xml.Append("</subscribe>");

			SensorDataSubscriptionRequest Request = new SensorDataSubscriptionRequest(Id, this, Destination, Destination, Nodes, Types,
				Fields, MinInterval, MaxInterval, MaxAge, ServiceToken, DeviceToken, UserToken);

			lock (this.requests)
			{
				this.requests[Id] = Request;
			}

			this.client.SendIqGet(Destination, Xml.ToString(), this.RequestResponse, Request);

			return Request;
		}

	}
}
