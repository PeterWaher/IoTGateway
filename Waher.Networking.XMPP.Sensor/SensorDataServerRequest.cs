using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Events;
using Waher.Things;
using Waher.Things.SensorData;

namespace Waher.Networking.XMPP.Sensor
{
	/// <summary>
	/// Manages a sensor data server request.
	/// </summary>
	public class SensorDataServerRequest : SensorDataRequest
	{
		private SensorServer sensorServer;
		private bool started = false;

		/// <summary>
		/// Manages a sensor data server request.
		/// </summary>
		/// <param name="SeqNr">Sequence number assigned to the request.</param>
		/// <param name="SensorServer">Sensor server object.</param>
		/// <param name="RemoteJID">JID of the other side of the conversation in the sensor data readout.</param>
		/// <param name="Nodes">Array of nodes to read. Can be null or empty, if reading a sensor that is not a concentrator.</param>
		/// <param name="Types">Field Types to read.</param>
		/// <param name="Fields">Fields to read.</param>
		/// <param name="From">From what time readout is to be made. Use <see cref="DateTime.MinValue"/> to specify no lower limit.</param>
		/// <param name="To">To what time readout is to be made. Use <see cref="DateTime.MaxValue"/> to specify no upper limit.</param>
		/// <param name="When">When the readout is to be made. Use <see cref="DateTime.MinValue"/> to start the readout immediately.</param>
		/// <param name="ServiceToken">Optional service token, as defined in XEP-0324.</param>
		/// <param name="DeviceToken">Optional device token, as defined in XEP-0324.</param>
		/// <param name="UserToken">Optional user token, as defined in XEP-0324.</param>
		internal SensorDataServerRequest(int SeqNr, SensorServer SensorServer, string RemoteJID, ThingReference[] Nodes, FieldType Types, string[] Fields,
			DateTime From, DateTime To, DateTime When, string ServiceToken, string DeviceToken, string UserToken)
			: base(SeqNr, RemoteJID, Nodes, Types, Fields, From, To, When, ServiceToken, DeviceToken, UserToken)
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
		public void ReportFields(bool Done, IEnumerable<Field> Fields)
		{
			StringBuilder Xml = new StringBuilder();
			ThingReference LastThing = null;
			DateTime LastTimestamp = DateTime.MinValue;
			FieldType FieldTypes;
			FieldQoS FieldQoS;
			EnumField EnumField;
			QuantityField QuantityField;
			string FieldDataTypeName;
			bool TimestampOpen = false;
			bool NodeOpen = false;
			bool First;

			Xml.Append("<fields xmlns='");
			Xml.Append(SensorClient.NamespaceSensorData);
			Xml.Append("' seqnr='");
			Xml.Append(this.SeqNr.ToString());

			if (Done)
			{
				this.sensorServer.Remove(this);
				Xml.Append("' done='true");
			}

			Xml.Append("'>");

			foreach (Field Field in Fields)
			{
				if (LastThing == null || !LastThing.SameThing(Field.Thing))
				{
					if (TimestampOpen)
					{
						Xml.Append("</timestamp>");
						TimestampOpen = false;
					}

					if (NodeOpen)
					{
						Xml.Append("</node>");
						NodeOpen = false;
					}

					LastThing = Field.Thing;
					LastTimestamp = DateTime.MinValue;

					Xml.Append("<node nodeId='");
					Xml.Append(CommonTypes.XmlEncode(LastThing.NodeId));

					if (!string.IsNullOrEmpty(LastThing.SourceId))
					{
						Xml.Append("' sourceId='");
						Xml.Append(CommonTypes.XmlEncode(LastThing.SourceId));
					}

					if (!string.IsNullOrEmpty(LastThing.CacheType))
					{
						Xml.Append("' cacheType='");
						Xml.Append(CommonTypes.XmlEncode(LastThing.CacheType));
					}

					Xml.Append("'>");
					NodeOpen = true;
				}

				if (LastTimestamp != Field.Timestamp)
				{
					if (TimestampOpen)
					{
						Xml.Append("</timestamp>");
						TimestampOpen = false;
					}

					LastTimestamp = Field.Timestamp;

					Xml.Append("<timestamp value='");
					Xml.Append(CommonTypes.XmlEncode(LastTimestamp));
					Xml.Append("'>");

					TimestampOpen = true;
				}

				FieldDataTypeName = Field.FieldDataTypeName;

				Xml.Append('<');
				Xml.Append(FieldDataTypeName);

				Xml.Append(" name='");
				Xml.Append(CommonTypes.XmlEncode(Field.Name));

				if (Field.Writable)
					Xml.Append("' writable='true");

				if (!string.IsNullOrEmpty(Field.Module))
				{
					Xml.Append("' module='");
					Xml.Append(CommonTypes.XmlEncode(Field.Module));
				}

				if (Field.StringIdSteps != null && Field.StringIdSteps.Length > 0)
				{
					Xml.Append("' stringIds='");

					First = true;
					foreach (LocalizationStep Step in Field.StringIdSteps)
					{
						if (First)
							First = false;
						else
							Xml.Append(',');

						Xml.Append(Step.StringId.ToString());

						if (!string.IsNullOrEmpty(Step.Module) || !string.IsNullOrEmpty(Step.Seed))
						{
							Xml.Append('|');
							Xml.Append(CommonTypes.XmlEncode(Step.Module));

							if (!string.IsNullOrEmpty(Step.Seed))
							{
								Xml.Append('|');
								Xml.Append(CommonTypes.XmlEncode(Step.Seed));
							}
						}
					}
				}

				FieldTypes = Field.Type;

				if ((FieldTypes & FieldType.All) == FieldType.All)
					Xml.Append("' all='true");
				else
				{
					if ((FieldTypes & FieldType.Historical) == FieldType.Historical)
					{
						Xml.Append("' historical='true");
						FieldTypes &= ~FieldType.Historical;
					}

					if ((FieldTypes & FieldType.Momentary) != 0)
						Xml.Append("' momentary='true");

					if ((FieldTypes & FieldType.Identity) != 0)
						Xml.Append("' identity='true");

					if ((FieldTypes & FieldType.Status) != 0)
						Xml.Append("' status='true");

					if ((FieldTypes & FieldType.Computed) != 0)
						Xml.Append("' computed='true");

					if ((FieldTypes & FieldType.Peak) != 0)
						Xml.Append("' peak='true");

					if ((FieldTypes & FieldType.HistoricalSecond) != 0)
						Xml.Append("' historicalSecond='true");

					if ((FieldTypes & FieldType.HistoricalMinute) != 0)
						Xml.Append("' historicalMinute='true");

					if ((FieldTypes & FieldType.HistoricalHour) != 0)
						Xml.Append("' historicalHour='true");

					if ((FieldTypes & FieldType.HistoricalDay) != 0)
						Xml.Append("' historicalDay='true");

					if ((FieldTypes & FieldType.HistoricalWeek) != 0)
						Xml.Append("' historicalWeek='true");

					if ((FieldTypes & FieldType.HistoricalMonth) != 0)
						Xml.Append("' historicalMonth='true");

					if ((FieldTypes & FieldType.HistoricalQuarter) != 0)
						Xml.Append("' historicalQuarter='true");

					if ((FieldTypes & FieldType.HistoricalYear) != 0)
						Xml.Append("' historicalYear='true");

					if ((FieldTypes & FieldType.HistoricalOther) != 0)
						Xml.Append("' historicalOther='true");
				}

				FieldQoS = Field.QoS;

				if ((FieldQoS & FieldQoS.Missing) != 0)
					Xml.Append("' missing='true");

				if ((FieldQoS & FieldQoS.InProgress) != 0)
					Xml.Append("' inProgress='true");

				if ((FieldQoS & FieldQoS.AutomaticEstimate) != 0)
					Xml.Append("' automaticEstimate='true");

				if ((FieldQoS & FieldQoS.ManualEstimate) != 0)
					Xml.Append("' manualEstimate='true");

				if ((FieldQoS & FieldQoS.ManualReadout) != 0)
					Xml.Append("' manualReadout='true");

				if ((FieldQoS & FieldQoS.AutomaticReadout) != 0)
					Xml.Append("' automaticReadout='true");

				if ((FieldQoS & FieldQoS.TimeOffset) != 0)
					Xml.Append("' timeOffset='true");

				if ((FieldQoS & FieldQoS.Warning) != 0)
					Xml.Append("' warning='true");

				if ((FieldQoS & FieldQoS.Error) != 0)
					Xml.Append("' error='true");

				if ((FieldQoS & FieldQoS.Signed) != 0)
					Xml.Append("' signed='true");

				if ((FieldQoS & FieldQoS.Invoiced) != 0)
					Xml.Append("' invoiced='true");

				if ((FieldQoS & FieldQoS.EndOfSeries) != 0)
					Xml.Append("' endOfSeries='true");

				if ((FieldQoS & FieldQoS.PowerFailure) != 0)
					Xml.Append("' powerFailure='true");

				if ((FieldQoS & FieldQoS.InvoiceConfirmed) != 0)
					Xml.Append("' invoiceConfirmed='true");

				Xml.Append("' value='");

				if ((QuantityField = Field as QuantityField) != null)
				{
					Xml.Append(CommonTypes.Encode(QuantityField.Value, QuantityField.NrDecimals));
					Xml.Append("' dataType='");
					Xml.Append(CommonTypes.XmlEncode(QuantityField.Unit));
				}
				else if ((EnumField = Field as EnumField) != null)
				{
					Xml.Append(CommonTypes.XmlEncode(Field.ValueString));
					Xml.Append("' dataType='");
					Xml.Append(CommonTypes.XmlEncode(EnumField.EnumerationType));
				}
				else
					Xml.Append(CommonTypes.XmlEncode(Field.ValueString));

				Xml.Append("'/>");
			}

			if (TimestampOpen)
				Xml.Append("</timestamp>");

			if (NodeOpen)
				Xml.Append("</node>");

			Xml.Append("</fields>");

			this.sensorServer.Client.SendMessage(MessageType.Normal, this.RemoteJID, Xml.ToString(), string.Empty, string.Empty,
				string.Empty, string.Empty, string.Empty);
		}

		/// <summary>
		/// Report error states to the client.
		/// </summary>
		/// <param name="Done">If the readout is complete (true) or if more data will be reported (false).</param>
		/// <param name="Fields">Errors that have been detected.</param>
		public void ReportErrors(bool Done, IEnumerable<ThingError> Errors)
		{
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
				Xml.Append(CommonTypes.XmlEncode(Error.NodeId));

				if (!string.IsNullOrEmpty(Error.SourceId))
				{
					Xml.Append("' sourceId='");
					Xml.Append(CommonTypes.XmlEncode(Error.SourceId));
				}

				if (!string.IsNullOrEmpty(Error.CacheType))
				{
					Xml.Append("' cacheType='");
					Xml.Append(CommonTypes.XmlEncode(Error.CacheType));
				}

				Xml.Append("' timestamp='");
				Xml.Append(CommonTypes.XmlEncode(Error.Timestamp));
				Xml.Append("'>");
				Xml.Append(CommonTypes.XmlEncode(Error.ErrorMessage));
				Xml.Append("</error>");
			}

			Xml.Append("</failure>");

			this.sensorServer.Client.SendMessage(MessageType.Normal, this.RemoteJID, Xml.ToString(), string.Empty, string.Empty,
				string.Empty, string.Empty, string.Empty);
		}
	}
}
