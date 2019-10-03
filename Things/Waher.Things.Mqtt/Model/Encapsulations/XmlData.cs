using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Networking.MQTT;
using Waher.Networking.Sniffers;
using Waher.Runtime.Language;
using Waher.Things;
using Waher.Things.ControlParameters;
using Waher.Things.SensorData;

namespace Waher.Things.Mqtt.Model.Encapsulations
{
	/// <summary>
	/// Represents an MQTT topic with XML data.
	/// </summary>
	public class XmlData : Data
	{
		private string xml;
		private XmlDocument value;

		/// <summary>
		/// Represents an MQTT topic with XML data.
		/// </summary>
		public XmlData(MqttTopic Topic, string Xml, XmlDocument Value)
			: base(Topic)
		{
			this.xml = Xml;
			this.value = Value;
		}

		public override void DataReported(MqttContent Content)
		{
			this.value = new XmlDocument();
			this.xml = Encoding.UTF8.GetString(Content.Data);
			this.value.LoadXml(this.xml);
			this.timestamp = DateTime.Now;
			this.qos = Content.Header.QualityOfService;
			this.retain = Content.Header.Retain;
		}

		public override Task<string> GetTypeName(Language Language)
		{
			return Language.GetStringAsync(typeof(MqttTopicNode), 41, "XML");
		}

		public override void StartReadout(ThingReference ThingReference, ISensorReadout Request, string Prefix, bool Last)
		{
			List<Field> Fields = new List<Field>();

			if (this.value.DocumentElement != null)
				this.AppendFields(ThingReference, Fields, Request, this.value.DocumentElement, Prefix);

			Request.ReportFields(Last, Fields.ToArray());
		}

		private void AppendFields(ThingReference ThingReference, List<Field> Fields, ISensorReadout Request, XmlElement Value, string Prefix)
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

					if (Repetitions == null)
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

		private void Add(ThingReference ThingReference, List<Field> Fields, string Name, string Value, ISensorReadout Request)
		{
			if (int.TryParse(Value, out int i))
				this.Add(Fields, new Int32Field(ThingReference, this.timestamp, Name, i, FieldType.Momentary, FieldQoS.AutomaticReadout), Request);
			else if (long.TryParse(Value, out long l))
				this.Add(Fields, new Int64Field(ThingReference, this.timestamp, Name, l, FieldType.Momentary, FieldQoS.AutomaticReadout), Request);
			else if (CommonTypes.TryParse(Value, out bool b))
				this.Add(Fields, new BooleanField(ThingReference, this.timestamp, Name, b, FieldType.Momentary, FieldQoS.AutomaticReadout), Request);
			else if (CommonTypes.TryParse(Value, out double d, out byte NrDec))
				this.Add(Fields, new QuantityField(ThingReference, this.timestamp, Name, d, NrDec, string.Empty, FieldType.Momentary, FieldQoS.AutomaticReadout), Request);
			else if (CommonTypes.TryParse(Value, out decimal d2, out NrDec))
				this.Add(Fields, new QuantityField(ThingReference, this.timestamp, Name, (double)d2, NrDec, string.Empty, FieldType.Momentary, FieldQoS.AutomaticReadout), Request);
			else if (System.TimeSpan.TryParse(Value, out TimeSpan TimeSpan))
				this.Add(Fields, new TimeField(ThingReference, this.timestamp, Name, TimeSpan, FieldType.Momentary, FieldQoS.AutomaticReadout), Request);
			else if (System.DateTime.TryParse(Value, out DateTime DateTime))
			{
				if (DateTime.TimeOfDay == TimeSpan.Zero)
					this.Add(Fields, new DateField(ThingReference, this.timestamp, Name, DateTime, FieldType.Momentary, FieldQoS.AutomaticReadout), Request);
				else
					this.Add(Fields, new DateTimeField(ThingReference, this.timestamp, Name, DateTime, FieldType.Momentary, FieldQoS.AutomaticReadout), Request);
			}
			else if (CommonTypes.TryParseRfc822(Value, out DateTimeOffset DateTimeOffset))
				this.Add(Fields, new DateTimeField(ThingReference, this.timestamp, Name, DateTimeOffset.DateTime, FieldType.Momentary, FieldQoS.AutomaticReadout), Request);
			else if (Content.Duration.TryParse(Value, out Duration Duration))
				this.Add(Fields, new DurationField(ThingReference, this.timestamp, Name, Duration, FieldType.Momentary, FieldQoS.AutomaticReadout), Request);
			else
				this.Add(Fields, new StringField(ThingReference, this.timestamp, Name, Value, FieldType.Momentary, FieldQoS.AutomaticReadout), Request);
		}

		public override bool IsControllable => true;

		public override ControlParameter[] GetControlParameters()
		{
			return new ControlParameter[]
			{
				new MultiLineTextControlParameter("Value", "Publish", "Value:", "XML value of topic.",
					(n) => this.xml,
					(n, v) =>
					{
						XmlDocument Doc = new XmlDocument();
						Doc.LoadXml(v);
						this.value = Doc;
						this.xml = v;
						this.topic.MqttClient.PUBLISH(this.topic.FullTopic, this.qos, this.retain, Encoding.UTF8.GetBytes(v));
					})
			};
		}

		public override void SnifferOutput(ISniffable Output)
		{
			this.Information(Output, this.xml);
		}

	}
}
