using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Networking.MQTT;
using Waher.Networking.Sniffers;
using Waher.Runtime.Inventory;
using Waher.Runtime.Language;
using Waher.Things.ControlParameters;
using Waher.Things.SensorData;

namespace Waher.Things.Mqtt.Model.Encapsulations
{
	/// <summary>
	/// Represents an MQTT topic with JSON-encoded data.
	/// </summary>
	public class JsonData : MqttData
	{
		private string json;
		private object value;

		/// <summary>
		/// Represents an MQTT topic with JSON-encoded data.
		/// </summary>
		public JsonData()
			: base()
		{
		}

		/// <summary>
		/// Represents an MQTT topic with JSON-encoded data.
		/// </summary>
		public JsonData(MqttTopic Topic, string Json, object Value)
			: base(Topic)
		{
			this.json = Json;
			this.value = Value;
		}

		/// <summary>
		/// Called when new data has been published.
		/// </summary>
		public override bool DataReported(MqttContent Content)
		{
			string s = Content.DataString;

			if ((s.StartsWith("{") && s.EndsWith("}")) || (s.StartsWith("[") && s.EndsWith("]")))
			{
				try
				{
					this.value = JSON.Parse(s);
					this.json = s;
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
			else
				return false;
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override Task<string> GetTypeName(Language Language)
		{
			return Language.GetStringAsync(typeof(MqttTopicNode), 38, "JSON");
		}

		/// <summary>
		/// TODO
		/// </summary>
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
			if (Value is null)
				this.Add(Fields, new StringField(ThingReference, this.Timestamp, Name, string.Empty, FieldType.Momentary, FieldQoS.AutomaticReadout), Request);
			else if (Value is string s)
			{
				if (System.TimeSpan.TryParse(s, out TimeSpan TimeSpan))
					this.Add(Fields, new TimeField(ThingReference, this.Timestamp, Name, TimeSpan, FieldType.Momentary, FieldQoS.AutomaticReadout), Request);
				else if (System.DateTime.TryParse(s, out DateTime DateTime))
				{
					if (DateTime.TimeOfDay == TimeSpan.Zero)
						this.Add(Fields, new DateField(ThingReference, this.Timestamp, Name, DateTime, FieldType.Momentary, FieldQoS.AutomaticReadout), Request);
					else
						this.Add(Fields, new DateTimeField(ThingReference, this.Timestamp, Name, DateTime, FieldType.Momentary, FieldQoS.AutomaticReadout), Request);
				}
				else if (CommonTypes.TryParseRfc822(s, out DateTimeOffset DateTimeOffset))
					this.Add(Fields, new DateTimeField(ThingReference, this.Timestamp, Name, DateTimeOffset.DateTime, FieldType.Momentary, FieldQoS.AutomaticReadout), Request);
				else if (Content.Duration.TryParse(s, out Duration Duration))
					this.Add(Fields, new DurationField(ThingReference, this.Timestamp, Name, Duration, FieldType.Momentary, FieldQoS.AutomaticReadout), Request);
				else
					this.Add(Fields, new StringField(ThingReference, this.Timestamp, Name, s, FieldType.Momentary, FieldQoS.AutomaticReadout), Request);
			}
			else if (Value is int i)
				this.Add(Fields, new Int32Field(ThingReference, this.Timestamp, Name, i, FieldType.Momentary, FieldQoS.AutomaticReadout), Request);
			else if (Value is long l)
				this.Add(Fields, new Int64Field(ThingReference, this.Timestamp, Name, l, FieldType.Momentary, FieldQoS.AutomaticReadout), Request);
			else if (Value is double d)
			{
				string s2 = CommonTypes.Encode(d);

				if (CommonTypes.TryParse(s2, out d, out byte NrDec))
					this.Add(Fields, new QuantityField(ThingReference, this.Timestamp, Name, d, NrDec, string.Empty, FieldType.Momentary, FieldQoS.AutomaticReadout), Request);
			}
			else if (Value is decimal d2)
			{
				string s2 = CommonTypes.Encode(d2);

				if (CommonTypes.TryParse(s2, out d2, out byte NrDec))
					this.Add(Fields, new QuantityField(ThingReference, this.Timestamp, Name, (double)d2, NrDec, string.Empty, FieldType.Momentary, FieldQoS.AutomaticReadout), Request);
			}
			else if (Value is bool b)
				this.Add(Fields, new BooleanField(ThingReference, this.Timestamp, Name, b, FieldType.Momentary, FieldQoS.AutomaticReadout), Request);
			else
				this.Add(Fields, new StringField(ThingReference, this.Timestamp, Name, Value.ToString(), FieldType.Momentary, FieldQoS.AutomaticReadout), Request);
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
				new MultiLineTextControlParameter("Value", "Publish", "Value:", "JSON value of topic.",
					(n) => Task.FromResult<string>(this.json),
					(n, v) =>
					{
						this.value = JSON.Parse(v);
						this.json = v;
						this.Topic.MqttClient.PUBLISH(this.Topic.FullTopic, this.QoS, this.Retain, Encoding.UTF8.GetBytes(v));
						return Task.CompletedTask;
					})
			};
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override void SnifferOutput(ISniffable Output)
		{
			this.Information(Output, this.json);
		}

		/// <summary>
		/// Default support.
		/// </summary>
		public override Grade DefaultSupport => Grade.Excellent;

		/// <summary>
		/// Creates a new instance of the data.
		/// </summary>
		/// <param name="Topic">MQTT Topic node</param>
		/// <param name="Content">MQTT Content</param>
		/// <returns>New object instance.</returns>
		public override IMqttData CreateNew(MqttTopic Topic, MqttContent Content)
		{
			IMqttData Result = new JsonData(Topic, default, default);
			Result.DataReported(Content);
			return Result;
		}
	}
}
