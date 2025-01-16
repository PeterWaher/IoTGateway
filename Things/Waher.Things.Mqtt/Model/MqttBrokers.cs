using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
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
		/// <param name="ConnectionSubscription">Connection subscription</param>
		/// <returns>Key</returns>
		public static string GetKey(string Host, int Port, bool Tls, bool TrustServer, string UserName, string Password, string ConnectionSubscription)
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine(Host);
			sb.AppendLine(Port.ToString());
			sb.AppendLine(Tls.ToString());
			sb.AppendLine(TrustServer.ToString());
			sb.AppendLine(UserName);
			sb.AppendLine(Password);
			sb.AppendLine(ConnectionSubscription);

			return Hashes.ComputeSHA1HashString(Encoding.UTF8.GetBytes(sb.ToString()));
		}

		/// <summary>
		/// Gets an MQTT Broker object, if available in the cache.
		/// </summary>
		/// <returns>MQTT Broker connection object, or null if none in the cache.</returns>
		public static MqttBroker GetCachedBroker(string Key)
		{
			lock (brokers)
			{
				if (brokers.TryGetValue(Key, out MqttBroker Broker))
					return Broker;
				else
					return null;
			}
		}

		/// <summary>
		/// Gets an MQTT Broker object, according to connection parameters. If one is not
		/// in memory, one is created and cached.
		/// </summary>
		/// <returns>MQTT Broker connection object.</returns>
		public static async Task<MqttBroker> GetBroker(MqttBrokerNode Node, string Key, string Host, int Port, bool Tls, bool TrustServer,
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
				await Broker.SetWill(WillTopic, WillData, WillRetain, WillQoS);
				return Broker;
			}
			else
			{
				Broker = new MqttBroker(Node, Host, Port, Tls, TrustServer, UserName, Password, ConnectionSubscription,
					WillTopic, WillData, WillRetain, WillQoS);

				MqttBroker Result;
				MqttBroker Obsolete;

				lock (brokers)
				{
					if (brokers.TryGetValue(Key, out Result))
						Obsolete = Broker;
					else
					{
						brokers[Key] = Result = Broker;
						Obsolete = null;
					}
				}

				if (!(Obsolete is null))
					await Obsolete.DisposeAsync();

				return Result;
			}
		}

		/// <summary>
		/// TODO
		/// </summary>
		public static Task DestroyBroker(string Key)
		{
			MqttBroker Broker;

			lock (brokers)
			{
				if (!brokers.TryGetValue(Key, out Broker))
					return Task.CompletedTask;

				brokers.Remove(Key);
			}

			return Broker.DisposeAsync();
		}

	}
}
