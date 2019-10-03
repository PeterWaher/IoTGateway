using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Networking.Sniffers;
using Waher.Networking.MQTT;
using Waher.Runtime.Language;
using Waher.Things;
using Waher.Things.ControlParameters;
using Waher.Things.SensorData;

namespace Waher.Things.Mqtt.Model.Encapsulations
{
	public abstract class Data
	{
		protected DateTime timestamp = DateTime.Now;
		protected MqttQualityOfService qos = MqttQualityOfService.AtLeastOnce;
		protected MqttTopic topic;
		protected bool retain;

		public Data(MqttTopic Topic)
		{
			this.topic = Topic;
		}

		public virtual bool IsControllable => false;

		public abstract void DataReported(MqttContent Content);
		public abstract Task<string> GetTypeName(Language Language);
		public abstract void StartReadout(ThingReference ThingReference, ISensorReadout Request, string Prefix, bool Last);
		public abstract void SnifferOutput(ISniffable Output);

		protected void Information(ISniffable Output, string Info)
		{
			foreach (ISniffer Sniffer in Output.Sniffers)
				Sniffer.Information(this.topic.FullTopic + ": " + Info);
		}

		protected string Append(string Prefix, string Name)
		{
			if (string.IsNullOrEmpty(Prefix))
				return Name;

			if (Name == "Value")
				return Prefix;
			else
				return Prefix + ", " + Name;
		}

		protected void Add(List<Field> Fields, Field Field, ISensorReadout Request)
		{
			if (Fields.Count > 50)
			{
				Request.ReportFields(false, Fields.ToArray());
				Fields.Clear();
			}

			Fields.Add(Field);
		}

		public virtual ControlParameter[] GetControlParameters()
		{
			return new ControlParameter[0];
		}

	}
}
