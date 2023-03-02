using System.Collections.Generic;
using System.Text;
using Waher.Security;

namespace Waher.Things.Xmpp.Model
{
	/// <summary>
	/// Static class managing connections to XMPP brokers.
	/// </summary>
	public static class XmppBrokers
	{
		private static readonly Dictionary<string, XmppBroker> brokers = new Dictionary<string, XmppBroker>();

		/// <summary>
		/// Gets sort key for XMPP broker
		/// </summary>
		/// <param name="Host">Host address</param>
		/// <param name="Port">Port number</param>
		/// <param name="Tls">If TLS is used</param>
		/// <param name="UserName">User name</param>
		/// <param name="Password">Password</param>
		/// <returns></returns>
		public static string GetKey(string Host, int Port, bool Tls, string UserName, string Password, string PasswordMechanism)
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine(Host);
			sb.AppendLine(Port.ToString());
			sb.AppendLine(Tls.ToString());
			sb.AppendLine(UserName);
			sb.AppendLine(Password);
			sb.AppendLine(PasswordMechanism);

			return Hashes.ComputeSHA1HashString(Encoding.UTF8.GetBytes(sb.ToString()));
		}

		public static XmppBroker GetBroker(XmppBrokerNode Node, string Key, string Host, int Port, bool Tls, string UserName, 
			string Password, string PasswordMechanism, bool TrustServer, bool AllowInsecureMechanisms)
		{
			XmppBroker Broker;

			lock (brokers)
			{
				if (brokers.TryGetValue(Key, out Broker))
					return Broker;
			}

			Broker = new XmppBroker(Node, Host, Port, Tls, UserName, Password, PasswordMechanism, TrustServer, AllowInsecureMechanisms);

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

		public static void DestroyBroker(string Key)
		{
			XmppBroker Broker;

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
