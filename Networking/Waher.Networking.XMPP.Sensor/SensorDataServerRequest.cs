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
		private readonly SensorServer sensorServer;
		private bool started = false;

		/// <summary>
		/// Manages a sensor data server request.
		/// </summary>
		/// <param name="Id">Request identity.</param>
		/// <param name="SensorServer">Sensor server object.</param>
		/// <param name="RemoteJID">JID of the other side of the conversation in the sensor data readout.</param>
		/// <param name="Actor">Actor causing the request to be made.</param>
		/// <param name="Nodes">Array of nodes to read. Can be null or empty, if reading a sensor that is not a concentrator.</param>
		/// <param name="Types">Field Types to read.</param>
		/// <param name="Fields">Fields to read.</param>
		/// <param name="From">From what time readout is to be made. Use <see cref="DateTime.MinValue"/> to specify no lower limit.</param>
		/// <param name="To">To what time readout is to be made. Use <see cref="DateTime.MaxValue"/> to specify no upper limit.</param>
		/// <param name="When">When the readout is to be made. Use <see cref="DateTime.MinValue"/> to start the readout immediately.</param>
		/// <param name="ServiceToken">Optional service token.</param>
		/// <param name="DeviceToken">Optional device token.</param>
		/// <param name="UserToken">Optional user token.</param>
		public SensorDataServerRequest(string Id, SensorServer SensorServer, string RemoteJID, string Actor, IThingReference[] Nodes, FieldType Types,
			string[] Fields, DateTime From, DateTime To, DateTime When, string ServiceToken, string DeviceToken, string UserToken)
			: base(Id, RemoteJID, Actor, Nodes, Types, Fields, From, To, When, ServiceToken, DeviceToken, UserToken)
		{
			this.sensorServer = SensorServer;
		}

		/// <summary>
		/// Sensor Data Server.
		/// </summary>
		public SensorServer SensorServer { get { return this.sensorServer; } }

		internal string Key
		{
			get { return this.RemoteJID + " " + this.Id; }
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
		/// <param name="Errors">Errors that have been detected.</param>
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
				SendMessage = OutputFields(Xml, Fields, this.Id, Done, this.IsIncluded);
				Xml.Flush();
			}

			foreach (Field Field in Fields)
			{
				if ((Field.Type & FieldType.Momentary) != 0)
				{
					if (this.momentaryFields is null)
						this.momentaryFields = new Dictionary<ThingReference, List<Field>>();

					Ref = Field.Thing;
					if (Ref is null)
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
		/// Outputs a set of fields to XML using the field format specified in the IEEE XMPP IoT extensions.
		/// </summary>
		/// <param name="Xml">XML output.</param>
		/// <param name="Fields">Fields to output.</param>
		/// <param name="Id">Request identity.</param>
		/// <param name="Done">If the readout is done.</param>
		/// <returns>If the response is non-empty, i.e. needs to be sent.</returns>
		public static bool OutputFields(XmlWriter Xml, IEnumerable<Field> Fields, string Id, bool Done)
		{
			return OutputFields(Xml, Fields, null, Id, Done, null);
		}

		/// <summary>
		/// Outputs a set of fields to XML using the field format specified in the IEEE XMPP IoT extensions.
		/// </summary>
		/// <param name="Xml">XML output.</param>
		/// <param name="Fields">Fields to output.</param>
		/// <param name="Id">Request identity.</param>
		/// <param name="Done">If the readout is done.</param>
		/// <param name="IsIncluded">Optional callback method that can be used to filter output. If null, all fields are output.</param>
		/// <returns>If the response is non-empty, i.e. needs to be sent.</returns>
		public static bool OutputFields(XmlWriter Xml, IEnumerable<Field> Fields, string Id, bool Done, IsIncludedDelegate IsIncluded)
		{
			return OutputFields(Xml, Fields, null, Id, Done, IsIncluded);
		}

		/// <summary>
		/// Outputs a set of fields to XML using the field format specified in the IEEE XMPP IoT extensions.
		/// </summary>
		/// <param name="Xml">XML output.</param>
		/// <param name="Fields">Fields to output.</param>
		/// <param name="Errors">Errors to report.</param>
		/// <param name="Id">Request identity.</param>
		/// <param name="Done">If the readout is done.</param>
		/// <param name="IsIncluded">Optional callback method that can be used to filter output. If null, all fields are output.</param>
		/// <returns>If the response is non-empty, i.e. needs to be sent.</returns>
		public static bool OutputFields(XmlWriter Xml, IEnumerable<Field> Fields, IEnumerable<ThingError> Errors, string Id, bool Done, IsIncludedDelegate IsIncluded)
		{
			Xml.WriteStartElement("resp", SensorClient.NamespaceSensorData);
			Xml.WriteAttributeString("id", Id);

			if (!Done)
				Xml.WriteAttributeString("more", "true");

			bool Result = OutputFields(Xml, Fields, Errors, IsIncluded);

			Xml.WriteEndElement();

			if (Done)
				return true;
			else
				return Result;
		}

		/// <summary>
		/// Outputs a set of fields to XML using the field format specified in the IEEE XMPP IoT extensions.
		/// </summary>
		/// <param name="Xml">XML output.</param>
		/// <param name="Fields">Fields to output.</param>
		/// <returns>If the response is non-empty, i.e. needs to be sent.</returns>
		public static bool OutputFields(XmlWriter Xml, IEnumerable<Field> Fields)
		{
			return OutputFields(Xml, Fields, null, null);
		}

		/// <summary>
		/// Outputs a set of fields to XML using the field format specified in the IEEE XMPP IoT extensions.
		/// </summary>
		/// <param name="Xml">XML output.</param>
		/// <param name="Fields">Fields to output. Can be null.</param>
		/// <param name="Errors">Any errors to output. Can be null.</param>
		/// <param name="IsIncluded">Optional callback method that can be used to filter output. If null, all fields are output.</param>
		/// <returns>If the response is non-empty, i.e. needs to be sent.</returns>
		public static bool OutputFields(XmlWriter Xml, IEnumerable<Field> Fields, IEnumerable<ThingError> Errors, IsIncludedDelegate IsIncluded)
		{
			ThingReference LastThing = null;
			DateTime LastTimestamp = DateTime.MinValue;
			bool TimestampOpen = false;
			bool NodeOpen = false;
			bool Checked;
			bool Empty = true;

			if (!(Fields is null))
			{
				foreach (Field Field in Fields)
				{
					Checked = false;

					if (LastThing is null || !LastThing.SameThing(Field.Thing))
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

						if (!string.IsNullOrEmpty(LastThing.NodeId))
						{
							Xml.WriteStartElement("nd");
							Xml.WriteAttributeString("id", LastThing.NodeId);

							if (!string.IsNullOrEmpty(LastThing.SourceId))
								Xml.WriteAttributeString("src", LastThing.SourceId);

							if (!string.IsNullOrEmpty(LastThing.Partition))
								Xml.WriteAttributeString("pt", LastThing.Partition);

							NodeOpen = true;
						}
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

						Xml.WriteStartElement("ts");
						Xml.WriteAttributeString("v", XML.Encode(LastTimestamp));

						TimestampOpen = true;
					}

					if (!Checked && IsIncluded != null && !IsIncluded(Field.Name, Field.Timestamp, Field.Type))
						continue;

					OutputField(Xml, Field);
					Empty = false;
				}
			}

			if (!(Errors is null))
			{
				foreach (ThingError Error in Errors)
				{
					if (LastThing is null || !LastThing.SameThing(Error))
					{
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

						LastThing = Error;
						LastTimestamp = DateTime.MinValue;

						if (!string.IsNullOrEmpty(LastThing.NodeId))
						{
							Xml.WriteStartElement("nd");
							Xml.WriteAttributeString("id", LastThing.NodeId);

							if (!string.IsNullOrEmpty(LastThing.SourceId))
								Xml.WriteAttributeString("src", LastThing.SourceId);

							if (!string.IsNullOrEmpty(LastThing.Partition))
								Xml.WriteAttributeString("pt", LastThing.Partition);

							NodeOpen = true;
						}
					}

					if (LastTimestamp != Error.Timestamp)
					{
						if (TimestampOpen)
						{
							Xml.WriteEndElement();
							TimestampOpen = false;
						}

						LastTimestamp = Error.Timestamp;

						Xml.WriteStartElement("ts");
						Xml.WriteAttributeString("v", XML.Encode(LastTimestamp));

						TimestampOpen = true;
					}

					OutputError(Xml, Error);
					Empty = false;
				}
			}

			if (TimestampOpen)
				Xml.WriteEndElement();

			if (NodeOpen)
				Xml.WriteEndElement();

			return !Empty;
		}

		/// <summary>
		/// Outputs a field to XML using the field format specified in the IEEE XMPP IoT extensions.
		/// </summary>
		/// <param name="Xml">XML output.</param>
		/// <param name="Error">Error to output.</param>
		public static void OutputError(XmlWriter Xml, ThingError Error)
		{
			Xml.WriteElementString("err", Error.ErrorMessage);
		}

		/// <summary>
		/// Outputs a field to XML using the field format specified in the IEEE XMPP IoT extensions.
		/// </summary>
		/// <param name="Xml">XML output.</param>
		/// <param name="Field">Field to output.</param>
		public static void OutputField(XmlWriter Xml, Field Field)
		{
			FieldType FieldTypes;
			FieldQoS FieldQoS;
			EnumField EnumField;
			string FieldDataTypeName;
			bool First;

			FieldDataTypeName = Field.FieldDataTypeName;

			Xml.WriteStartElement(FieldDataTypeName);
			Xml.WriteAttributeString("n", Field.Name);

			if (Field.Writable)
				Xml.WriteAttributeString("ctr", "true");

			if (!string.IsNullOrEmpty(Field.Module))
				Xml.WriteAttributeString("lns", Field.Module);

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

				Xml.WriteAttributeString("loc", Value.ToString());
			}

			FieldTypes = Field.Type;

			if ((FieldTypes & FieldType.All) == FieldType.All)
				Xml.WriteAttributeString("all", "true");
			else
			{
				if (FieldTypes.HasFlag(FieldType.Momentary))
					Xml.WriteAttributeString("m", "true");

				if (FieldTypes.HasFlag(FieldType.Identity))
					Xml.WriteAttributeString("i", "true");

				if (FieldTypes.HasFlag(FieldType.Status))
					Xml.WriteAttributeString("s", "true");

				if (FieldTypes.HasFlag(FieldType.Computed))
					Xml.WriteAttributeString("c", "true");

				if (FieldTypes.HasFlag(FieldType.Peak))
					Xml.WriteAttributeString("p", "true");

				if (FieldTypes.HasFlag(FieldType.Historical))
					Xml.WriteAttributeString("h", "true");
			}

			FieldQoS = Field.QoS;

			if (FieldQoS.HasFlag(FieldQoS.Missing))
				Xml.WriteAttributeString("ms", "true");

			if (FieldQoS.HasFlag(FieldQoS.InProgress))
				Xml.WriteAttributeString("pr", "true");

			if (FieldQoS.HasFlag(FieldQoS.AutomaticEstimate))
				Xml.WriteAttributeString("ae", "true");

			if (FieldQoS.HasFlag(FieldQoS.ManualEstimate))
				Xml.WriteAttributeString("me", "true");

			if (FieldQoS.HasFlag(FieldQoS.ManualReadout))
				Xml.WriteAttributeString("mr", "true");

			if (FieldQoS.HasFlag(FieldQoS.AutomaticReadout))
				Xml.WriteAttributeString("ar", "true");

			if (FieldQoS.HasFlag(FieldQoS.TimeOffset))
				Xml.WriteAttributeString("of", "true");

			if (FieldQoS.HasFlag(FieldQoS.Warning))
				Xml.WriteAttributeString("w", "true");

			if (FieldQoS.HasFlag(FieldQoS.Error))
				Xml.WriteAttributeString("er", "true");

			if (FieldQoS.HasFlag(FieldQoS.Signed))
				Xml.WriteAttributeString("so", "true");

			if (FieldQoS.HasFlag(FieldQoS.Invoiced))
				Xml.WriteAttributeString("iv", "true");

			if (FieldQoS.HasFlag(FieldQoS.EndOfSeries))
				Xml.WriteAttributeString("eos", "true");

			if (FieldQoS.HasFlag(FieldQoS.PowerFailure))
				Xml.WriteAttributeString("pf", "true");

			if (FieldQoS.HasFlag(FieldQoS.InvoiceConfirmed))
				Xml.WriteAttributeString("ic", "true");

			if (Field is QuantityField QuantityField)
			{
				Xml.WriteAttributeString("v", CommonTypes.Encode(QuantityField.Value, QuantityField.NrDecimals));
				Xml.WriteAttributeString("u", QuantityField.Unit);
			}
			else if ((EnumField = Field as EnumField) != null)
			{
				Xml.WriteAttributeString("v", Field.ValueString);
				Xml.WriteAttributeString("t", EnumField.EnumerationType);
			}
			else
				Xml.WriteAttributeString("v", Field.ValueString);

			Xml.WriteEndElement();
		}

		/// <summary>
		/// Report error states to the client.
		/// </summary>
		/// <param name="Done">If the readout is complete (true) or if more data will be reported (false).</param>
		/// <param name="Errors">Errors that have been detected.</param>
		public virtual void ReportErrors(bool Done, IEnumerable<ThingError> Errors)
		{
			if (Done && !this.DecNodesLeft())
				Done = false;

			StringBuilder Xml = new StringBuilder();

			Xml.Append("<resp xmlns='");
			Xml.Append(SensorClient.NamespaceSensorData);
			Xml.Append("' id='");
			Xml.Append(this.Id);

			if (!Done)
			{
				this.sensorServer.Remove(this);
				Xml.Append("' more='true");
			}

			Xml.Append("'>");

			ThingReference LastThing = null;
			DateTime LastTimestamp = DateTime.MinValue;
			bool NodeOpen = false;
			bool TimestampOpen = false;

			foreach (ThingError Error in Errors)
			{
				if (!string.IsNullOrEmpty(Error.NodeId) && (LastThing is null || !Error.SameThing(Error)))
				{
					if (TimestampOpen)
					{
						Xml.Append("</ts>");
						TimestampOpen = false;
					}

					if (NodeOpen)
					{
						Xml.Append("</nd>");
						NodeOpen = false;
					}

					Xml.Append("<nd id='");
					Xml.Append(XML.Encode(Error.NodeId));

					if (!string.IsNullOrEmpty(Error.SourceId))
					{
						Xml.Append("' src='");
						Xml.Append(XML.Encode(Error.SourceId));
					}

					if (!string.IsNullOrEmpty(Error.Partition))
					{
						Xml.Append("' pt='");
						Xml.Append(XML.Encode(Error.Partition));
					}

					Xml.Append("'>");
					NodeOpen = true;
				}

				if (Error.Timestamp != LastTimestamp)
				{
					if (TimestampOpen)
						Xml.Append("</ts>");
					else
						TimestampOpen = true;

					Xml.Append("<ts v='");
					Xml.Append(XML.Encode(Error.Timestamp));
					Xml.Append("'>");
				}

				Xml.Append("<err>");
				Xml.Append(XML.Encode(Error.ErrorMessage));
				Xml.Append("</err>");
			}

			if (TimestampOpen)
				Xml.Append("</ts>");

			if (NodeOpen)
				Xml.Append("</nd>");

			Xml.Append("</resp>");

			this.sensorServer.Client.SendMessage(MessageType.Normal, this.RemoteJID, Xml.ToString(), string.Empty, string.Empty,
				string.Empty, string.Empty, string.Empty);
		}

		/// <summary>
		/// Report that readout has started. Can optionally be used to report feedback to end-user when readout is slow.
		/// </summary>
		public virtual void Start()
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<started xmlns='");
			Xml.Append(SensorClient.NamespaceSensorData);
			Xml.Append("' id='");
			Xml.Append(this.Id);
			Xml.Append("'/>");

			this.sensorServer.Client.SendMessage(MessageType.Normal, this.RemoteJID, Xml.ToString(), string.Empty, string.Empty,
				string.Empty, string.Empty, string.Empty);
		}
	}
}
