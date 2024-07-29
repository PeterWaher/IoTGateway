using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Networking.Sniffers;
using Waher.Networking.MQTT;
using Waher.Runtime.Language;
using Waher.Things.ControlParameters;
using Waher.Things.SensorData;

namespace Waher.Things.Mqtt.Model.Encapsulations
{
	/// <summary>
	/// TODO
	/// </summary>
	public abstract class Data
	{
		/// <summary>
		/// TODO
		/// </summary>
		protected DateTime timestamp = DateTime.Now;

		/// <summary>
		/// TODO
		/// </summary>
		protected MqttQualityOfService qos = MqttQualityOfService.AtLeastOnce;

		/// <summary>
		/// TODO
		/// </summary>
		protected MqttTopic topic;

		/// <summary>
		/// TODO
		/// </summary>
		protected bool retain;

		/// <summary>
		/// TODO
		/// </summary>
		public Data(MqttTopic Topic)
		{
			this.topic = Topic;
		}

		/// <summary>
		/// TODO
		/// </summary>
		public virtual bool IsControllable => false;

		/// <summary>
		/// TODO
		/// </summary>
		public abstract void DataReported(MqttContent Content);

		/// <summary>
		/// TODO
		/// </summary>
		public abstract Task<string> GetTypeName(Language Language);

		/// <summary>
		/// TODO
		/// </summary>
		public abstract void StartReadout(ThingReference ThingReference, ISensorReadout Request, string Prefix, bool Last);

		/// <summary>
		/// TODO
		/// </summary>
		public abstract void SnifferOutput(ISniffable Output);

		/// <summary>
		/// TODO
		/// </summary>
		protected void Information(ISniffable Output, string Info)
		{
			foreach (ISniffer Sniffer in Output.Sniffers)
				Sniffer.Information(this.topic.FullTopic + ": " + Info);
		}

		/// <summary>
		/// TODO
		/// </summary>
		protected string Append(string Prefix, string Name)
		{
			if (string.IsNullOrEmpty(Prefix))
				return Name;

			if (Name == "Value")
				return Prefix;
			else
				return Prefix + ", " + Name;
		}

		/// <summary>
		/// TODO
		/// </summary>
		protected void Add(List<Field> Fields, Field Field, ISensorReadout Request)
		{
			if (Fields.Count > 50)
			{
				Request.ReportFields(false, Fields.ToArray());
				Fields.Clear();
			}

			Fields.Add(Field);
		}

		/// <summary>
		/// TODO
		/// </summary>
		public virtual ControlParameter[] GetControlParameters()
		{
			return new ControlParameter[0];
		}

	}
}
