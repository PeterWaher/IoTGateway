using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Waher.Networking;
using Waher.Persistence.Attributes;
using Waher.Runtime.Language;
using Waher.Things.Attributes;

namespace Waher.Things.Ip
{
	public abstract class IpHostPort : IpHost
	{
		private int port = 0;

		/// <summary>
		/// Port number.
		/// </summary>
		[Page(1, "IP")]
		[Header(4, "Port Number:", 60)]
		[ToolTip(5, "Port number to use when communicating with device.")]
		[DefaultValue(0)]
		[Range(0, 65535)]
		public int Port
		{
			get { return this.port; }
			set { this.port = value; }
		}

		/// <summary>
		/// Connect to the remote host and port using TCP.
		/// </summary>
		/// <returns>TCP transport.</returns>
		public async Task<TcpTransport> ConnectTcp()
		{
			TcpClient Client = new TcpClient();

			await Client.ConnectAsync(this.Host, this.port);

			return new TcpTransport(Client);
		}

	}
}
