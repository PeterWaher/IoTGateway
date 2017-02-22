using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content;
using Waher.Events;
using Waher.Things;
using Waher.Things.SensorData;

namespace Waher.Networking.XMPP.Sensor
{
	/// <summary>
	/// Checks if a field with the given parameters is included in the readout.
	/// </summary>
	/// <param name="FieldName">Unlocalized name of field.</param>
	/// <param name="Timestamp">Timestamp of field.</param>
	/// <param name="Type">Field Types</param>
	/// <returns>If the corresponding field is included.</returns>
	public delegate bool IsIncludedDelegate(string FieldName, DateTime Timestamp, FieldType Type);

	/// <summary>
	/// Manages a sensor data server request.
	/// </summary>
	public class SensorDataServerRequest : SensorDataRequest, ISensorReadout
	{
		private Dictionary<ThingReference, List<Field>> momentaryFields = null;
		private SensorServer sensorServer;
		private bool started = false;

		/// <summary>
		/// Manages a sensor data server request.
		/// </summary>
		/// <param name="SeqNr">Sequence number assigned to the request.</param>
		/// <param name="SensorServer">Sensor server object.</param>
		/// <param name="RemoteJID">JID of the other side of the conversation in the sensor data readout.</param>
		/// <param name="Actor">Actor causing the request to be made.</param>
		/// <param name="Nodes">Array of nodes to read. Can be null or empty, if reading a sensor that is not a concentrator.</param>
		/// <param name="Types">Field Types to read.</param>
		/// <param name="Fields">Fields to read.</param>
		/// <param name="From">From what time readout is to be made. Use <see cref="DateTime.MinValue"/> to specify no lower limit.</param>
		/// <param name="To">To what time readout is to be made. Use <see cref="DateTime.MaxValue"/> to specify no upper limit.</param>
		/// <param name="When">When the readout is to be made. Use <see cref="DateTime.MinValue"/> to start the readout immediately.</param>
		/// <param name="ServiceToken">Optional service token, as defined in XEP-0324.</param>
		/// <param name="DeviceToken">Optional device token, as defined in XEP-0324.</param>
		/// <param name="UserToken">Optional user token, as defined in XEP-0324.</param>
		public SensorDataServerRequest(int SeqNr, SensorServer SensorServer, string RemoteJID, string Actor, ThingReference[] Nodes, FieldType Types,
			string[] Fields, DateTime From, DateTime To, DateTime When, string ServiceToken, string DeviceToken, string UserToken)
			: base(SeqNr, RemoteJID, Actor, Nodes, Types, Fields, From, To, When, ServiceToken, DeviceToken, UserToken)
		{
			this.sensorServer = SensorServer;
		}

		/// <summary>
		/// Sensor Data Server.
		/// </summary>
		public SensorServer SensorServer { get { return this.sensorServer; } }

		internal string Key
		{
			get { return this.RemoteJID + " " + this.SeqNr.ToString(); }
		}

		/// <summary>
		/// If the readout process is started or not.
		/// </summary>
		public bool Started
		{
			get { return this.started; }
			internal set { this.started = value; }
		}

		/// <summary>
		/// Report read fields to the client.
		/// </summary>
		/// <param name="Done">If the readout is complete (true) or if more data will be reported (false).</param>
		/// <param name="Fields">Fields that have been read.</param>
		public void ReportFields(bool Done, params Field[] Fields)
		{
			this.ReportFields(Done, (IEnumerable<Field>)Fields);
		}

		/// <summary>
		/// Report error states to the client.
		/// </summary>
		/// <param name="Done">If the readout is complete (true) or if more data will be reported (false).</param>
		/// <param name="Fields">Errors that have been detected.</param>
		public void ReportErrors(bool Done, params ThingError[] Errors)
		{
			this.ReportErrors(Done, (IEnumerable<ThingError>)Errors);
		}

		/// <summary>
		/// Report read fields to the client.
		/// </summary>
		/// <param name="Done">If the readout is complete (true) or if more data will be reported (false).</param>
		/// <param name="Fields">Fields that have been read.</param>
		public virtual void ReportFields(bool Done, IEnumerable<Field> Fields)
		{
			StringBuilder Output = new StringBuilder();
			ThingReference Ref;
			ThingReference LastNode = null;
			List<Field> LastFields = null;
			bool SendMessage;

			if (Done && !this.DecNodesLeft())
				Done = false;

			using (XmlWriter Xml = XmlWriter.Create(Output, XML.WriterSettings(false, true)))
			{
				SendMessage = OutputFields(Xml, Fields, this.SeqNr, Done, this.IsIncluded);
				Xml.Flush();
			}

			foreach (Field Field in Fields)
			{
				if ((Field.Type & FieldType.Momentary) != 0)
				{
					if (this.momentaryFields == null)
						this.momentaryFields = new Dictionary<ThingReference, List<Field>>();

					Ref = Field.Thing;
					if (Ref == null)
						Ref = ThingReference.Empty;

					if (Ref != LastNode)
					{
						if (!this.momentaryFields.TryGetValue(Ref, out LastFields))
						{
							LastFields = new List<Field>();
							this.momentaryFields[Ref] = LastFields;
						}

						LastNode = Ref;
					}

					LastFields.Add(Field);
				}
			}

			if (Done)
			{
				this.sensorServer.Remove(this);

				if (this.momentaryFields != null)
				{
					foreach (KeyValuePair<ThingReference, List<Field>> Thing in this.momentaryFields)
						this.sensorServer.NewMomentaryValues(Thing.Key, Thing.Value, this.RemoteJID);

					this.momentaryFields = null;
				}
			}

			if (SendMessage)
			{
				this.sensorServer.Client.SendMessage(MessageType.Normal, this.RemoteJID, Output.ToString(), string.Empty, string.Empty,
					string.Empty, string.Empty, string.Empty);
			}
		}

		/// <summary>
		/// Outputs a set of fields to XML using the field format specified in XEP-0323.
		/// </summary>
		/// <param name="Xml">XML output.</param>
		/// <param name="Fields">Fields to output.</param>
		/// <param name="SeqNr">Sequence Number.</param>
		/// <param name="Done">If the readout is done.</param>
		/// <param name="IsIncluded">Optional callback method that can be used to filter output. If null, all fields are output.</param>
		/// <returns>If the response is non-empty, i.e. needs to be sent.</returns>
		public static bool OutputFields(XmlWriter Xml, IEnumerable<Field> Fields, int SeqNr, bool Done, IsIncludedDelegate IsIncluded)
		{
			ThingReference LastThing = null;
			DateTime LastTimestamp = DateTime.MinValue;
			bool TimestampOpen = false;
			bool NodeOpen = false;
			bool Checked;
			bool Empty = true;

			Xml.WriteStartElement("fields", SensorClient.NamespaceSensorData);
			Xml.WriteAttributeString("seqnr", SeqNr.ToString());

			if (Done)
			{
				Empty = false;
				Xml.WriteAttributeString("done", "true");
			}

			foreach (Field Field in Fields)
			{
				Checked = false;

				if (LastThing == null || !LastThing.SameThing(Field.Thing))
				{
					if (IsIncluded != null && !IsIncluded(Field.Name, Field.Timestamp, Field.Type))
						continue;

					Checked = true;

					if (TimestampOpen)
					{
						Xml.WriteEndElement();
						TimestampOpen = false;
					}

					if (NodeOpen)
					{
						Xml.WriteEndElement();
						NodeOpen = false;
					}

					LastThing = Field.Thing;
					LastTimestamp = DateTime.MinValue;

					Xml.WriteStartElement("node");
					Xml.WriteAttributeString("nodeId", LastThing.NodeId);

					if (!string.IsNullOrEmpty(LastThing.SourceId))
						Xml.WriteAttributeString("sourceId", LastThing.SourceId);

					if (!string.IsNullOrEmpty(LastThing.CacheType))
						Xml.WriteAttributeString("cacheType", LastThing.CacheType);

					NodeOpen = true;
				}

				if (LastTimestamp != Field.Timestamp)
				{
					if (IsIncluded != null && !IsIncluded(Field.Name, Field.Timestamp, Field.Type))
						continue;

					Checked = true;

					if (TimestampOpen)
					{
						Xml.WriteEndElement();
						TimestampOpen = false;
					}

					LastTimestamp = Field.Timestamp;

					Xml.WriteStartElement("timestamp");
					Xml.WriteAttributeString("value", XML.Encode(LastTimestamp));

					TimestampOpen = true;
				}

				if (!Checked && IsIncluded != null && !IsIncluded(Field.Name, Field.Timestamp, Field.Type))
					continue;

				OutputField(Xml, Field);
				Empty = false;
			}

			if (TimestampOpen)
				Xml.WriteEndElement();

			if (NodeOpen)
				Xml.WriteEndElement();

			Xml.WriteEndElement();

			return !Empty;
		}

		/// <summary>
		/// Outputs a field to XML using the field format specified in XEP-0323.
		/// </summary>
		/// <param name="Xml">XML output.</param>
		/// <param name="Field">Field to output.</param>
		public static void OutputField(XmlWriter Xml, Field Field)
		{
			FieldType FieldTypes;
			FieldQoS FieldQoS;
			EnumField EnumField;
			QuantityField QuantityField;
			string FieldDataTypeName;
			bool First;

			FieldDataTypeName = Field.FieldDataTypeName;

			Xml.WriteStartElement(FieldDataTypeName);
			Xml.WriteAttributeString("name", Field.Name);

			if (Field.Writable)
				Xml.WriteAttributeString("writable", "true");

			if (!string.IsNullOrEmpty(Field.Module))
				Xml.WriteAttributeString("module", Field.Module);

			if (Field.StringIdSteps != null && Field.StringIdSteps.Length > 0)
			{
				StringBuilder Value = new StringBuilder();

				First = true;
				foreach (LocalizationStep Step in Field.StringIdSteps)
				{
					if (First)
						First = false;
					else
						Value.Append(',');

					Value.Append(Step.StringId.ToString());

					if (!string.IsNullOrEmpty(Step.Module) || !string.IsNullOrEmpty(Step.Seed))
					{
						Value.Append('|');
						Value.Append(XML.Encode(Step.Module));

						if (!string.IsNullOrEmpty(Step.Seed))
						{
							Value.Append('|');
							Value.Append(XML.Encode(Step.Seed));
						}
					}
				}

				Xml.WriteAttributeString("stringIds", Value.ToString());
			}

			FieldTypes = Field.Type;

			if ((FieldTypes & FieldType.All) == FieldType.All)
				Xml.WriteAttributeString("all", "true");
			else
			{
				if ((FieldTypes & FieldType.Historical) == FieldType.Historical)
				{
					Xml.WriteAttributeString("historical", "true");
					FieldTypes &= ~FieldType.Historical;
				}

				if (FieldTypes.HasFlag(FieldType.Momentary))
					Xml.WriteAttributeString("momentary", "true");

				if (FieldTypes.HasFlag(FieldType.Identity))
					Xml.WriteAttributeString("identity", "true");

				if (FieldTypes.HasFlag(FieldType.Status))
					Xml.WriteAttributeString("status", "true");

				if (FieldTypes.HasFlag(FieldType.Computed))
					Xml.WriteAttributeString("computed", "true");

				if (FieldTypes.HasFlag(FieldType.Peak))
					Xml.WriteAttributeString("peak", "true");

				if (FieldTypes.HasFlag(FieldType.HistoricalSecond))
					Xml.WriteAttributeString("historicalSecond", "true");

				if (FieldTypes.HasFlag(FieldType.HistoricalMinute))
					Xml.WriteAttributeString("historicalMinute", "true");

				if (FieldTypes.HasFlag(FieldType.HistoricalHour))
					Xml.WriteAttributeString("historicalHour", "true");

				if (FieldTypes.HasFlag(FieldType.HistoricalDay))
					Xml.WriteAttributeString("historicalDay", "true");

				if (FieldTypes.HasFlag(FieldType.HistoricalWeek))
					Xml.WriteAttributeString("historicalWeek", "true");

				if (FieldTypes.HasFlag(FieldType.HistoricalMonth))
					Xml.WriteAttributeString("historicalMonth", "true");

				if (FieldTypes.HasFlag(FieldType.HistoricalQuarter))
					Xml.WriteAttributeString("historicalQuarter", "true");

				if (FieldTypes.HasFlag(FieldType.HistoricalYear))
					Xml.WriteAttributeString("historicalYear", "true");

				if (FieldTypes.HasFlag(FieldType.HistoricalOther))
					Xml.WriteAttributeString("historicalOther", "true");
			}

			FieldQoS = Field.QoS;

			if (FieldQoS.HasFlag(FieldQoS.Missing))
				Xml.WriteAttributeString("missing", "true");

			if (FieldQoS.HasFlag(FieldQoS.InProgress))
				Xml.WriteAttributeString("inProgress", "true");

			if (FieldQoS.HasFlag(FieldQoS.AutomaticEstimate))
				Xml.WriteAttributeString("automaticEstimate", "true");

			if (FieldQoS.HasFlag(FieldQoS.ManualEstimate))
				Xml.WriteAttributeString("manualEstimate", "true");

			if (FieldQoS.HasFlag(FieldQoS.ManualReadout))
				Xml.WriteAttributeString("manualReadout", "true");

			if (FieldQoS.HasFlag(FieldQoS.AutomaticReadout))
				Xml.WriteAttributeString("automaticReadout", "true");

			if (FieldQoS.HasFlag(FieldQoS.TimeOffset))
				Xml.WriteAttributeString("timeOffset", "true");

			if (FieldQoS.HasFlag(FieldQoS.Warning))
				Xml.WriteAttributeString("warning", "true");

			if (FieldQoS.HasFlag(FieldQoS.Error))
				Xml.WriteAttributeString("error", "true");

			if (FieldQoS.HasFlag(FieldQoS.Signed))
				Xml.WriteAttributeString("signed", "true");

			if (FieldQoS.HasFlag(FieldQoS.Invoiced))
				Xml.WriteAttributeString("invoiced", "true");

			if (FieldQoS.HasFlag(FieldQoS.EndOfSeries))
				Xml.WriteAttributeString("endOfSeries", "true");

			if (FieldQoS.HasFlag(FieldQoS.PowerFailure))
				Xml.WriteAttributeString("powerFailure", "true");

			if (FieldQoS.HasFlag(FieldQoS.InvoiceConfirmed))
				Xml.WriteAttributeString("invoiceConfirmed", "true");

			if ((QuantityField = Field as QuantityField) != null)
			{
				Xml.WriteAttributeString("value", CommonTypes.Encode(QuantityField.Value, QuantityField.NrDecimals));
				Xml.WriteAttributeString("unit", QuantityField.Unit);
			}
			else if ((EnumField = Field as EnumField) != null)
			{
				Xml.WriteAttributeString("value", Field.ValueString);
				Xml.WriteAttributeString("dataType", EnumField.EnumerationType);
			}
			else
				Xml.WriteAttributeString("value", Field.ValueString);

			Xml.WriteEndElement();
		}

		/// <summary>
		/// Report error states to the client.
		/// </summary>
		/// <param name="Done">If the readout is complete (true) or if more data will be reported (false).</param>
		/// <param name="Fields">Errors that have been detected.</param>
		public virtual void ReportErrors(bool Done, IEnumerable<ThingError> Errors)
		{
			if (Done && !this.DecNodesLeft())
				Done = false;

			StringBuilder Xml = new StringBuilder();

			Xml.Append("<failure xmlns='");
			Xml.Append(SensorClient.NamespaceSensorData);
			Xml.Append("' seqnr='");
			Xml.Append(this.SeqNr.ToString());

			if (Done)
			{
				this.sensorServer.Remove(this);
				Xml.Append("' done='true");
			}

			Xml.Append("'>");

			foreach (ThingError Error in Errors)
			{
				Xml.Append("<error nodeId='");
				Xml.Append(XML.Encode(Error.NodeId));

				if (!string.IsNullOrEmpty(Error.SourceId))
				{
					Xml.Append("' sourceId='");
					Xml.Append(XML.Encode(Error.SourceId));
				}

				if (!string.IsNullOrEmpty(Error.CacheType))
				{
					Xml.Append("' cacheType='");
					Xml.Append(XML.Encode(Error.CacheType));
				}

				Xml.Append("' timestamp='");
				Xml.Append(XML.Encode(Error.Timestamp));
				Xml.Append("'>");
				Xml.Append(XML.Encode(Error.ErrorMessage));
				Xml.Append("</error>");
			}

			Xml.Append("</failure>");

			this.sensorServer.Client.SendMessage(MessageType.Normal, this.RemoteJID, Xml.ToString(), string.Empty, string.Empty,
				string.Empty, string.Empty, string.Empty);
		}
	}
}
