using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.MQTT;
using Waher.Security;

namespace Waher.Things.Mqtt.Model
{
	/// <summary>
	/// Static class managing connections to MQTT brokers.
	/// </summary>
	public static class MqttBrokers
	{
		private static readonly Dictionary<string, MqttBroker> brokers = new Dictionary<string, MqttBroker>();

		/// <summary>
		/// Gets sort key for MQTT broker
		/// </summary>
		/// <param name="Host">Host address</param>
		/// <param name="Port">Port number</param>
		/// <param name="Tls">If TLS is used</param>
		/// <param name="UserName">User name</param>
		/// <param name="Password">Password</param>
		/// <returns></returns>
		public static string GetKey(string Host, int Port, bool Tls, string UserName, string Password)
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine(Host);
			sb.AppendLine(Port.ToString());
			sb.AppendLine(Tls.ToString());
			sb.AppendLine(UserName);
			sb.AppendLine(Password);

			return Hashes.ComputeSHA1HashString(Encoding.UTF8.GetBytes(sb.ToString()));
		}

		public static MqttBroker GetBroker(MqttBrokerNode Node, string Key, string Host, int Port, bool Tls, string UserName, string Password,
			string WillTopic, string WillData, bool WillRetain, MqttQualityOfService WillQoS)
		{
			MqttBroker Broker;

			lock (brokers)
			{
				if (!brokers.TryGetValue(Key, out Broker))
					Broker = null;
			}

			if (Broker != null)
			{
				Broker.SetWill(WillTopic, WillData, WillRetain, WillQoS);
				return Broker;
			}
			else
			{
				Broker = new MqttBroker(Node, Host, Port, Tls, UserName, Password, WillTopic, WillData, WillRetain, WillQoS);

				lock (brokers)
				{
					if (brokers.ContainsKey(Key))
					{
						Broker.Dispose();
						return brokers[Key];
					}
					else
					{
						brokers[Key] = Broker;
						return Broker;
					}
				}
			}
		}

		public static void DestroyBroker(string Key)
		{
			MqttBroker Broker;

			lock (brokers)
			{
				if (!brokers.TryGetValue(Key, out Broker))
					return;

				brokers.Remove(Key);
			}

			Broker.Dispose();
		}

	}
}
