using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Security;

namespace Waher.Things.Ip.Model
{
	/// <summary>
	/// Static class managing Port Proxies.
	/// </summary>
	public static class ProxyPorts
	{
		private static readonly Dictionary<string, ProxyPort> proxies = new Dictionary<string, ProxyPort>();

		/// <summary>
		/// Gets sort key for Port Proxy
		/// </summary>
		/// <param name="Host">Host address</param>
		/// <param name="Port">Port number</param>
		/// <param name="Tls">If TLS is used</param>
		/// <param name="UserName">User name</param>
		/// <param name="Password">Password</param>
		/// <returns></returns>
		public static string GetKey(string Host, int Port, bool Tls, bool TrustServer, int ListeningPort)
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine(Host);
			sb.AppendLine(Port.ToString());
			sb.AppendLine(Tls.ToString());
			sb.AppendLine(TrustServer.ToString());
			sb.AppendLine(ListeningPort.ToString());

			return Hashes.ComputeSHA1HashString(Encoding.UTF8.GetBytes(sb.ToString()));
		}

		public static async Task<ProxyPort> GetProxy(IpHostPortProxy Node, string Key, string Host, int Port, bool Tls, bool TrustServer, int ListeningPort)
		{
			ProxyPort Proxy;

			lock (proxies)
			{
				if (!proxies.TryGetValue(Key, out Proxy))
					Proxy = null;
			}

			if (!(Proxy is null))
				return Proxy;
			else
			{
				Proxy = await ProxyPort.Create(Node, Host, Port, Tls, TrustServer, ListeningPort);

				lock (proxies)
				{
					if (proxies.ContainsKey(Key))
					{
						Proxy.Dispose();
						return proxies[Key];
					}
					else
					{
						proxies[Key] = Proxy;
						return Proxy;
					}
				}
			}
		}

		public static void DestroyProxy(string Key)
		{
			ProxyPort Proxy;

			lock (proxies)
			{
				if (!proxies.TryGetValue(Key, out Proxy))
					return;

				proxies.Remove(Key);
			}

			Proxy.Dispose();
		}

	}
}
