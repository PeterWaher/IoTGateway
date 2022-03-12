using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Networking;
using Waher.Networking.Sniffers;
using Waher.Persistence.Attributes;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.DisplayableParameters;

namespace Waher.Things.Ip
{
	public class IpHostPort : IpHost
	{
		private int port = 0;
		private bool tls = false;

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
		/// Port number.
		/// </summary>
		[Page(1, "IP")]
		[Header(11, "Encrypted (TLS)", 70)]
		[ToolTip(12, "Check if Transpotrt Layer Encryption (TLS) should be used.")]
		public bool Tls
		{
			get { return this.tls; }
			set { this.tls = value; }
		}

		/// <summary>
		/// Connect to the remote host and port using a binary protocol over TCP.
		/// </summary>
		/// <param name="Sniffers">Sniffers</param>
		/// <returns>Binary TCP transport.</returns>
		public async Task<BinaryTcpClient> ConnectTcp(params ISniffer[] Sniffers)
		{
			BinaryTcpClient Client = new BinaryTcpClient(Sniffers);
			await Client.ConnectAsync(this.Host, this.port);

			if (this.tls)
				await Client.UpgradeToTlsAsClient(System.Security.Authentication.SslProtocols.Tls12);

			return Client;
		}

		/// <summary>
		/// Connect to the remote host and port using a text-based protocol over TCP.
		/// </summary>
		/// <param name="Encoding">Encoding to use.</param>
		/// <param name="Sniffers">Sniffers</param>
		/// <returns>Text-based TCP transport.</returns>
		public async Task<TextTcpClient> ConnectTcp(Encoding Encoding, params ISniffer[] Sniffers)
		{
			TextTcpClient Client = new TextTcpClient(Encoding, Sniffers);
			await Client.ConnectAsync(this.Host, this.port);

			if (this.tls)
				await Client.UpgradeToTlsAsClient(System.Security.Authentication.SslProtocols.Tls12);
			
			return Client;
		}

		/// <summary>
		/// Gets displayable parameters.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>Set of displayable parameters.</returns>
		public async override Task<IEnumerable<Parameter>> GetDisplayableParametersAsync(Language Language, RequestOrigin Caller)
		{
			LinkedList<Parameter> Result = await base.GetDisplayableParametersAsync(Language, Caller) as LinkedList<Parameter>;

			Result.AddLast(new Int32Parameter("Port", await Language.GetStringAsync(typeof(IpHost), 10, "Port"), this.port));

			return Result;
		}

	}
}
