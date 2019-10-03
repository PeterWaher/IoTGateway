using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
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
	/// Represents an MQTT topic with JSON-encoded data.
	/// </summary>
	public class JsonData : Data
	{
		private string json;
		private object value;

		/// <summary>
		/// Represents an MQTT topic with JSON-encoded data.
		/// </summary>
		public JsonData(MqttTopic Topic, string Json, object Value)
			: base(Topic)
		{
			this.json = Json;
			this.value = Value;
		}

		public override void DataReported(MqttContent Content)
		{
			string s = Encoding.UTF8.GetString(Content.Data);
			this.value = JSON.Parse(s);
			this.json = s;
			this.timestamp = DateTime.Now;
			this.qos = Content.Header.QualityOfService;
			this.retain = Content.Header.Retain;
		}

		public override Task<string> GetTypeName(Language Language)
		{
			return Language.GetStringAsync(typeof(MqttTopicNode), 38, "JSON");
		}

		public override void StartReadout(ThingReference ThingReference, ISensorReadout Request, string Prefix, bool Last)
		{
			List<Field> Fields = new List<Field>();
			this.AppendFields(ThingReference, Fields, Request, this.value, Prefix);
			Request.ReportFields(Last, Fields.ToArray());
		}

		private void AppendFields(ThingReference ThingReference, List<Field> Fields, ISensorReadout Request, object Value, string Prefix)
		{
			if (Value is Dictionary<string, object> Object)
			{
				if (string.IsNullOrEmpty(Prefix))
				{
					foreach (KeyValuePair<string, object> P in Object)
						this.AppendFields(ThingReference, Fields, Request, P.Value, P.Key);
				}
				else
				{
					foreach (KeyValuePair<string, object> P in Object)
						this.AppendFields(ThingReference, Fields, Request, P.Value, Prefix + ", " + P.Key);
				}
			}
			else if (Value is IEnumerable Array && !(Value is string))
			{
				int i = 1;

				if (string.IsNullOrEmpty(Prefix))
				{
					foreach (object Item in Array)
						this.AppendFields(ThingReference, Fields, Request, Item, "#" + (i++).ToString());
				}
				else
				{
					foreach (object Item in Array)
						this.AppendFields(ThingReference, Fields, Request, Item, Prefix + " #" + (i++).ToString());
				}
			}
			else
			{
				if (string.IsNullOrEmpty(Prefix))
					this.Add(ThingReference, Fields, "Value", Value, Request);
				else
					this.Add(ThingReference, Fields, Prefix, Value, Request);
			}
		}

		private void Add(ThingReference ThingReference, List<Field> Fields, string Name, object Value, ISensorReadout Request)
		{
			if (Value == null)
				this.Add(Fields, new StringField(ThingReference, this.timestamp, Name, string.Empty, FieldType.Momentary, FieldQoS.AutomaticReadout), Request);
			else if (Value is string s)
			{
				if (System.TimeSpan.TryParse(s, out TimeSpan TimeSpan))
					this.Add(Fields, new TimeField(ThingReference, this.timestamp, Name, TimeSpan, FieldType.Momentary, FieldQoS.AutomaticReadout), Request);
				else if (System.DateTime.TryParse(s, out DateTime DateTime))
				{
					if (DateTime.TimeOfDay == TimeSpan.Zero)
						this.Add(Fields, new DateField(ThingReference, this.timestamp, Name, DateTime, FieldType.Momentary, FieldQoS.AutomaticReadout), Request);
					else
						this.Add(Fields, new DateTimeField(ThingReference, this.timestamp, Name, DateTime, FieldType.Momentary, FieldQoS.AutomaticReadout), Request);
				}
				else if (CommonTypes.TryParseRfc822(s, out DateTimeOffset DateTimeOffset))
					this.Add(Fields, new DateTimeField(ThingReference, this.timestamp, Name, DateTimeOffset.DateTime, FieldType.Momentary, FieldQoS.AutomaticReadout), Request);
				else if (Content.Duration.TryParse(s, out Duration Duration))
					this.Add(Fields, new DurationField(ThingReference, this.timestamp, Name, Duration, FieldType.Momentary, FieldQoS.AutomaticReadout), Request);
				else
					this.Add(Fields, new StringField(ThingReference, this.timestamp, Name, s, FieldType.Momentary, FieldQoS.AutomaticReadout), Request);
			}
			else if (Value is int i)
				this.Add(Fields, new Int32Field(ThingReference, this.timestamp, Name, i, FieldType.Momentary, FieldQoS.AutomaticReadout), Request);
			else if (Value is long l)
				this.Add(Fields, new Int64Field(ThingReference, this.timestamp, Name, l, FieldType.Momentary, FieldQoS.AutomaticReadout), Request);
			else if (Value is double d)
			{
				string s2 = CommonTypes.Encode(d);

				if (CommonTypes.TryParse(s2, out d, out byte NrDec))
					this.Add(Fields, new QuantityField(ThingReference, this.timestamp, Name, d, NrDec, string.Empty, FieldType.Momentary, FieldQoS.AutomaticReadout), Request);
			}
			else if (Value is decimal d2)
			{
				string s2 = CommonTypes.Encode(d2);

				if (CommonTypes.TryParse(s2, out d2, out byte NrDec))
					this.Add(Fields, new QuantityField(ThingReference, this.timestamp, Name, (double)d2, NrDec, string.Empty, FieldType.Momentary, FieldQoS.AutomaticReadout), Request);
			}
			else if (Value is bool b)
				this.Add(Fields, new BooleanField(ThingReference, this.timestamp, Name, b, FieldType.Momentary, FieldQoS.AutomaticReadout), Request);
			else
				this.Add(Fields, new StringField(ThingReference, this.timestamp, Name, Value.ToString(), FieldType.Momentary, FieldQoS.AutomaticReadout), Request);
		}

		public override bool IsControllable => true;

		public override ControlParameter[] GetControlParameters()
		{
			return new ControlParameter[]
			{
				new MultiLineTextControlParameter("Value", "Publish", "Value:", "JSON value of topic.",
					(n) => this.json,
					(n, v) =>
					{
						this.value = JSON.Parse(v);
						this.json = v;
						this.topic.MqttClient.PUBLISH(this.topic.FullTopic, this.qos, this.retain, Encoding.UTF8.GetBytes(v));
					})
			};
		}

		public override void SnifferOutput(ISniffable Output)
		{
			this.Information(Output, this.json);
		}

	}
}
