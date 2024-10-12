using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Networking.MQTT;
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP.Sensor;
using Waher.Runtime.Language;
using Waher.Things.ControlParameters;
using Waher.Things.SensorData;
using Waher.Runtime.Inventory;

namespace Waher.Things.Mqtt.Model.Encapsulations
{
	/// <summary>
	/// Represents an MQTT topic with XML data.
	/// </summary>
	public class XmlData : MqttData
	{
		private string xml;
		private XmlDocument value;

		/// <summary>
		/// Represents an MQTT topic with XML data.
		/// </summary>
		public XmlData()
			: base()
		{
		}

		/// <summary>
		/// Represents an MQTT topic with XML data.
		/// </summary>
		/// <param name="Topic">MQTT Topic</param>
		/// <param name="Xml">String-representation of XML</param>
		/// <param name="Value">Data value</param>
		public XmlData(MqttTopic Topic, string Xml, XmlDocument Value)
			: base(Topic)
		{
			this.xml = Xml;
			this.value = Value;
		}

		/// <summary>
		/// Called when new data has been published.
		/// </summary>
		/// <param name="Topic">MQTT Topic Node</param>
		/// <param name="Content">Published MQTT Content</param>
		public override bool DataReported(MqttTopic Topic, MqttContent Content)
		{
			try
			{
				string s = Content.DataString;

				this.value = new XmlDocument()
				{
					PreserveWhitespace = false
				};
				this.value.LoadXml(s);
				this.xml = s;
				this.Timestamp = DateTime.UtcNow;
				this.QoS = Content.Header.QualityOfService;
				this.Retain = Content.Header.Retain;

				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		/// <summary>
		/// Type name representing data.
		/// </summary>
		public override Task<string> GetTypeName(Language Language)
		{
			return Language.GetStringAsync(typeof(MqttTopicNode), 41, "XML");
		}

		/// <summary>
		/// Starts a readout of the data.
		/// </summary>
		/// <param name="ThingReference">Thing reference.</param>
		/// <param name="Request">Sensor-data request</param>
		/// <param name="Prefix">Field-name prefix.</param>
		/// <param name="Last">If the last readout call for request.</param>
		public override void StartReadout(ThingReference ThingReference, ISensorReadout Request, string Prefix, bool Last)
		{
			List<Field> Fields = new List<Field>();

			if (!(this.value.DocumentElement is null))
				this.AppendFields(ThingReference, Fields, Request, this.value.DocumentElement, Prefix);

			Request.ReportFields(Last, Fields.ToArray());
		}

		private void AppendFields(ThingReference ThingReference, List<Field> Fields, ISensorReadout Request, XmlElement Value, string Prefix)
		{
			if (Array.IndexOf(SensorClient.NamespacesSensorData, Value.NamespaceURI) >= 0)
			{
				Networking.XMPP.Sensor.SensorData SensorData = SensorClient.ParseFields(Value);
				if (!(SensorData.Fields is null))
					Fields.AddRange(SensorData.Fields);
			}
			else
			{
				foreach (XmlAttribute Attribute in Value.Attributes)
				{
					if (string.IsNullOrEmpty(Prefix))
						this.Add(ThingReference, Fields, Attribute.Name, Attribute.Value, Request);
					else
						this.Add(ThingReference, Fields, this.Append(Prefix, Attribute.Name), Attribute.Value, Request);
				}

				Dictionary<string, int> Repetitions = null;
				string s;

				foreach (XmlNode N in Value)
				{
					if (N is XmlElement ChildElement)
					{
						s = ChildElement.LocalName;

						if (Repetitions is null)
							Repetitions = new Dictionary<string, int>();

						if (Repetitions.TryGetValue(s, out int i))
							Repetitions[s] = i - 1;
						else
							Repetitions[s] = 0;
					}
				}

				foreach (XmlNode N in Value)
				{
					if (N is XmlElement ChildElement)
					{
						s = ChildElement.LocalName;

						if (Repetitions.TryGetValue(s, out int i))
						{
							if (i < 0)
								Repetitions[s] = i = 1;
							else if (i > 0)
								Repetitions[s] = ++i;

							if (i != 0)
								s += ", #" + i.ToString();
						}

						if (string.IsNullOrEmpty(Prefix))
							this.AppendFields(ThingReference, Fields, Request, ChildElement, s);
						else
							this.AppendFields(ThingReference, Fields, Request, ChildElement, Prefix + ", " + s);
					}
					else if (N is XmlText XmlText)
					{
						if (string.IsNullOrEmpty(Prefix))
							this.Add(ThingReference, Fields, "Value", XmlText.InnerText, Request);
						else
							this.Add(ThingReference, Fields, Prefix, XmlText.InnerText, Request);
					}
				}
			}
		}

		private void Add(ThingReference ThingReference, List<Field> Fields, string Name, string Value, ISensorReadout Request)
		{
			if (int.TryParse(Value, out int i))
				this.Add(Fields, new Int32Field(ThingReference, this.Timestamp, Name, i, FieldType.Momentary, FieldQoS.AutomaticReadout), Request);
			else if (long.TryParse(Value, out long l))
				this.Add(Fields, new Int64Field(ThingReference, this.Timestamp, Name, l, FieldType.Momentary, FieldQoS.AutomaticReadout), Request);
			else if (CommonTypes.TryParse(Value, out bool b))
				this.Add(Fields, new BooleanField(ThingReference, this.Timestamp, Name, b, FieldType.Momentary, FieldQoS.AutomaticReadout), Request);
			else if (CommonTypes.TryParse(Value, out double d, out byte NrDec))
				this.Add(Fields, new QuantityField(ThingReference, this.Timestamp, Name, d, NrDec, string.Empty, FieldType.Momentary, FieldQoS.AutomaticReadout), Request);
			else if (CommonTypes.TryParse(Value, out decimal d2, out NrDec))
				this.Add(Fields, new QuantityField(ThingReference, this.Timestamp, Name, (double)d2, NrDec, string.Empty, FieldType.Momentary, FieldQoS.AutomaticReadout), Request);
			else if (System.TimeSpan.TryParse(Value, out TimeSpan TimeSpan))
				this.Add(Fields, new TimeField(ThingReference, this.Timestamp, Name, TimeSpan, FieldType.Momentary, FieldQoS.AutomaticReadout), Request);
			else if (System.DateTime.TryParse(Value, out DateTime DateTime))
			{
				if (DateTime.TimeOfDay == TimeSpan.Zero)
					this.Add(Fields, new DateField(ThingReference, this.Timestamp, Name, DateTime, FieldType.Momentary, FieldQoS.AutomaticReadout), Request);
				else
					this.Add(Fields, new DateTimeField(ThingReference, this.Timestamp, Name, DateTime, FieldType.Momentary, FieldQoS.AutomaticReadout), Request);
			}
			else if (CommonTypes.TryParseRfc822(Value, out DateTimeOffset DateTimeOffset))
				this.Add(Fields, new DateTimeField(ThingReference, this.Timestamp, Name, DateTimeOffset.DateTime, FieldType.Momentary, FieldQoS.AutomaticReadout), Request);
			else if (Content.Duration.TryParse(Value, out Duration Duration))
				this.Add(Fields, new DurationField(ThingReference, this.Timestamp, Name, Duration, FieldType.Momentary, FieldQoS.AutomaticReadout), Request);
			else
				this.Add(Fields, new StringField(ThingReference, this.Timestamp, Name, Value, FieldType.Momentary, FieldQoS.AutomaticReadout), Request);
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override bool IsControllable => true;

		/// <summary>
		/// TODO
		/// </summary>
		public override ControlParameter[] GetControlParameters()
		{
			return new ControlParameter[]
			{
				new MultiLineTextControlParameter("Value", "Publish", "Value:", "XML value of topic.",
					(n) => Task.FromResult<string>(this.xml),
					(n, v) =>
					{
						XmlDocument Doc = new XmlDocument()
						{
							PreserveWhitespace = true
						};
						Doc.LoadXml(v);
						this.value = Doc;
						this.xml = v;
						this.Topic.MqttClient.PUBLISH(this.Topic.FullTopic, this.QoS, this.Retain, Encoding.UTF8.GetBytes(v));
						return Task.CompletedTask;
					})
			};
		}

		/// <summary>
		/// Outputs the parsed data to the sniffer.
		/// </summary>
		public override void SnifferOutput(ISniffable Output)
		{
			this.Information(Output, this.xml);
		}

		/// <summary>
		/// Default support.
		/// </summary>
		public override Grade DefaultSupport => Grade.Excellent;

		/// <summary>
		/// Creates a new instance of the data.
		/// </summary>
		/// <param name="Topic">MQTT Topic</param>
		/// <param name="Content">MQTT Content</param>
		/// <returns>New object instance.</returns>
		public override IMqttData CreateNew(MqttTopic Topic, MqttContent Content)
		{
			IMqttData Result = new XmlData(Topic, default, default);
			Result.DataReported(Topic, Content);
			return Result;
		}
	}
}
