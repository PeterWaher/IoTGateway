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
		/// <param name="TrustServer">If server certificate should be automatically trusted.</param>
		/// <param name="UserName">User name</param>
		/// <param name="Password">Password</param>
		/// <returns></returns>
		public static string GetKey(string Host, int Port, bool Tls, bool TrustServer, string UserName, string Password)
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine(Host);
			sb.AppendLine(Port.ToString());
			sb.AppendLine(Tls.ToString());
			sb.AppendLine(TrustServer.ToString());
			sb.AppendLine(UserName);
			sb.AppendLine(Password);

			return Hashes.ComputeSHA1HashString(Encoding.UTF8.GetBytes(sb.ToString()));
		}

		/// <summary>
		/// TODO
		/// </summary>
		public static MqttBroker GetBroker(MqttBrokerNode Node, string Key, string Host, int Port, bool Tls, bool TrustServer,
			string UserName, string Password, string ConnectionSubscription, string WillTopic, string WillData, bool WillRetain, 
			MqttQualityOfService WillQoS)
		{
			MqttBroker Broker;

			lock (brokers)
			{
				if (!brokers.TryGetValue(Key, out Broker))
					Broker = null;
			}

			if (!(Broker is null))
			{
				Broker.SetWill(WillTopic, WillData, WillRetain, WillQoS);
				return Broker;
			}
			else
			{
				Broker = new MqttBroker(Node, Host, Port, Tls, TrustServer, UserName, Password, ConnectionSubscription,
					WillTopic, WillData, WillRetain, WillQoS);

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

		/// <summary>
		/// TODO
		/// </summary>
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
