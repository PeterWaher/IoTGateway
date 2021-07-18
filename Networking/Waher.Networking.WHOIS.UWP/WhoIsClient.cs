using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Waher.Networking.Sniffers;

namespace Waher.Networking.WHOIS
{
	/// <summary>
	/// WHOIS client, as defined in RFC 3912:
	/// https://tools.ietf.org/html/rfc3912
	/// </summary>
	public partial class WhoIsClient : TextTcpClient
	{
		private readonly StringBuilder receivedText = new StringBuilder();
		private readonly AutoResetEvent received = new AutoResetEvent(false);
		private readonly ManualResetEvent closed = new ManualResetEvent(false);

		/// <summary>
		/// Default WHOIS Port = 43
		/// </summary>
		public const int DefaultWhoIsPort = 43;

		/// <summary>
		/// WHOIS client, as defined in RFC 3912:
		/// https://tools.ietf.org/html/rfc3912
		/// </summary>
		/// <param name="Sniffers">Sniffers.</param>
		public WhoIsClient(params ISniffer[] Sniffers)
			: this(Encoding.ASCII, Sniffers)
		{
		}

		/// <summary>
		/// WHOIS client, as defined in RFC 3912:
		/// https://tools.ietf.org/html/rfc3912
		/// </summary>
		/// <param name="Encoding">Text encoding to use. (Default=<see cref="Encoding.ASCII"/>)</param>
		/// <param name="Sniffers">Sniffers.</param>
		public WhoIsClient(Encoding Encoding, params ISniffer[] Sniffers)
			: base(Encoding, Sniffers)
		{
		}

		/// <summary>
		/// Queries an Internet Registry about an entity.
		/// </summary>
		/// <param name="InternetRegistry">Domain name of Internet Registry to query.</param>
		/// <param name="Entity">Entity to be queried.</param>
		/// <returns>WHOIS Information</returns>
		public async Task<string> DoQuery(string InternetRegistry, string Entity)
		{
			this.received.Reset();

			await this.ConnectAsync(InternetRegistry, DefaultWhoIsPort);
			this.received.WaitOne(500);

			lock (this.receivedText)
			{
				this.receivedText.Clear();
			}

			this.closed.Reset();
			await this.SendAsync(Entity + "\r\n");

			this.closed.WaitOne(5000);

			string s;

			lock (this.receivedText)
			{
				s = this.receivedText.ToString();
				this.receivedText.Clear();
			}

			return s.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n");
		}

		/// <summary>
		/// Method called when text data has been received.
		/// </summary>
		/// <param name="Data">Text data received.</param>
		protected override Task<bool> TextDataReceived(string Data)
		{
			lock (this.receivedText)
			{
				this.receivedText.Append(Data);
			}

			this.received.Set();

			return Task.FromResult<bool>(true);
		}

		/// <summary>
		/// Method called when the connection has been disconnected.
		/// </summary>
		protected override void Disconnected()
		{
			this.closed.Set();
			base.Disconnected();
		}

		/// <summary>
		/// Disposes of the object. The underlying <see cref="TcpClient"/> is either disposed directly, or when asynchronous
		/// operations have ceased.
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();

			this.received.Dispose();
			this.closed.Dispose();
			this.receivedText.Clear();
		}

		/// <summary>
		/// Queries an Internet Registry about an entity.
		/// </summary>
		/// <param name="InternetRegistry">Domain name of Internet Registry to query.</param>
		/// <param name="Entity">Entity to be queried.</param>
		/// <returns>WHOIS Information</returns>
		public static async Task<string> Query(string InternetRegistry, string Entity)
		{
			using (WhoIsClient Client = new WhoIsClient())
			{
				return await Client.DoQuery(InternetRegistry, Entity);
			}
		}

		/// <summary>
		/// Queries the corresponding Internet Registry about an IP Address.
		/// </summary>
		/// <param name="Address">IP Address to query</param>
		/// <returns>WHOIS Information, if available, null if not.</returns>
		public static async Task<string> Query(IPAddress Address)
		{
			switch (Address.AddressFamily)
			{
				case AddressFamily.InterNetwork:
					byte[] Bytes = Address.GetAddressBytes();
					byte b = Bytes[0];

					if (b >= ipv4ToWhoIsService.Length)
						return null;

					WhoIsIpv4ServiceEnum Service = ipv4ToWhoIsService[b];
					string InternetRegistry = ipv4WhoIsServices[(int)Service];

					using (WhoIsClient Client = new WhoIsClient())
					{
						return await Client.DoQuery(InternetRegistry, Address.ToString());
					}

				default:
					return null;
			}
		}

		/// <summary>
		/// Gets the RDAP URI for an IP Address. It points to a JSON object containing WHOIS information about the IP Address.
		/// </summary>
		/// <param name="Address">IP Address</param>
		/// <returns>RDAP URI, if available, null if not.</returns>
		public static Uri RdapUri(IPAddress Address)
		{
			switch (Address.AddressFamily)
			{
				case AddressFamily.InterNetwork:
					byte[] Bytes = Address.GetAddressBytes();
					byte b = Bytes[0];

					if (b >= ipv4ToWhoIsService.Length)
						return null;

					WhoIsIpv4ServiceEnum Service = ipv4ToWhoIsService[b];
					string RdapService = rdapServices[(int)Service];

					if (string.IsNullOrEmpty(RdapService))
						return null;

					return new Uri(RdapService + "ip/" + Address.ToString());

				default:
					return null;
			}
		}

	}
}
