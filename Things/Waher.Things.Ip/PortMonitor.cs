using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Networking;
using Waher.Networking.Sniffers;
using Waher.Persistence.Attributes;
using Waher.Runtime.Inventory;
using Waher.Runtime.Language;
using Waher.Script.Functions.Runtime;
using Waher.Security;
using Waher.Things.Attributes;
using Waher.Things.DisplayableParameters;
using Waher.Things.Metering;
using Waher.Things.SensorData;

namespace Waher.Things.Ip
{
	/// <summary>
	/// Node representing a port on an IP Host machine.
	/// </summary>
	public class PortMonitor : ProvisionedMeteringNode, ISensor
	{
		private string protocol = string.Empty;
		private int port = 0;
		private bool tls = false;

		/// <summary>
		/// If the node accepts a presumptive child, i.e. can receive as a child (if that child accepts the node as a parent).
		/// </summary>
		/// <param name="Child">Presumptive child node.</param>
		/// <returns>If the child is acceptable.</returns>
		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult(false);
		}

		/// <summary>
		/// If the node accepts a presumptive parent, i.e. can be added to that parent (if that parent accepts the node as a child).
		/// </summary>
		/// <param name="Parent">Presumptive parent node.</param>
		/// <returns>If the parent is acceptable.</returns>
		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult(Parent is IpHost);
		}

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(PortMonitor), 40, "Port Monitor");
		}

		/// <summary>
		/// Port number.
		/// </summary>
		[Page(1, "IP")]
		[Header(4, "Port Number:", 60)]
		[ToolTip(5, "Port number to use when communicating with device.")]
		[DefaultValue(0)]
		[Range(0, 65535)]
		[Required]
		public int Port
		{
			get => this.port;
			set => this.port = value;
		}

		/// <summary>
		/// If connection is encrypted using TLS or not.
		/// </summary>
		[Page(1, "IP")]
		[Header(11, "Encrypted (TLS)", 70)]
		[ToolTip(12, "Check if Transport Layer Encryption (TLS) should be used.")]
		public bool Tls
		{
			get => this.tls;
			set => this.tls = value;
		}

		/// <summary>
		/// Port number.
		/// </summary>
		[Page(1, "IP")]
		[Header(41, "Protocol:", 60)]
		[ToolTip(42, "Name of protocol or service on port number.")]
		[DefaultValueStringEmpty]
		[Required]
		public string Protocol
		{
			get => this.protocol;
			set => this.protocol = value;
		}

		/// <summary>
		/// Old property, replaced by <see cref="Protocol"/>.
		/// </summary>
		[Obsolete]
		public string PortName
		{
			get => this.protocol;
			set => this.protocol = value;
		}

		/// <summary>
		/// Gets the host name or IP address of the parent IP Host node.
		/// </summary>
		/// <returns>Host name.</returns>
		/// <exception cref="Exception">If parent node is not an IP Host node.</exception>
		public async Task<string> GetHostName()
		{
			if (await this.GetParent() is IpHost Parent)
				return Parent.Host;
			else
				throw new Exception("Parent node not an IP Host node.");
		}

		/// <summary>
		/// Connect to the remote host and port using a binary protocol over TCP.
		/// </summary>
		/// <param name="DecoupledEvents">If events raised from the communication 
		/// layer are decoupled, i.e. executed in parallel with the source that raised 
		/// them.</param>
		/// <param name="Sniffers">Sniffers</param>
		/// <returns>Binary TCP transport.</returns>
		public async Task<BinaryTcpClient> ConnectTcp(bool DecoupledEvents, params ISniffer[] Sniffers)
		{
			X509Certificate Certificate = Types.TryGetModuleParameter<X509Certificate>("X509");
			BinaryTcpClient Client = new BinaryTcpClient(DecoupledEvents, Sniffers);
			string HostName = await this.GetHostName();
			await Client.ConnectAsync(HostName, this.port, this.tls);

			if (this.tls)
			{
				await Client.UpgradeToTlsAsClient(Certificate, Crypto.SecureTls);
				Client.Continue();
			}

			return Client;
		}

		/// <summary>
		/// Connect to the remote host and port using a text-based protocol over TCP.
		/// </summary>
		/// <param name="Encoding">Encoding to use.</param>
		/// <param name="DecoupledEvents">If events raised from the communication 
		/// layer are decoupled, i.e. executed in parallel with the source that raised 
		/// them.</param>
		/// <param name="Sniffers">Sniffers</param>
		/// <returns>Text-based TCP transport.</returns>
		public async Task<TextTcpClient> ConnectTcp(Encoding Encoding, bool DecoupledEvents, params ISniffer[] Sniffers)
		{
			X509Certificate Certificate = Types.TryGetModuleParameter<X509Certificate>("X509");
			TextTcpClient Client = new TextTcpClient(Encoding, DecoupledEvents, Sniffers);
			string HostName = await this.GetHostName();
			await Client.ConnectAsync(HostName, this.port, this.tls);

			if (this.tls)
			{
				await Client.UpgradeToTlsAsClient(Certificate, Crypto.SecureTls);
				Client.Continue();
			}

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

			if (!string.IsNullOrEmpty(this.Protocol))
				Result.AddLast(new StringParameter("Protocol", await Language.GetStringAsync(typeof(IpHost), 43, "Protocol"), this.protocol));

			return Result;
		}

		/// <summary>
		/// Starts the readout of the sensor.
		/// </summary>
		/// <param name="Request">Request object. All fields and errors should be reported to this interface.</param>
		public async virtual Task StartReadout(ISensorReadout Request)
		{
			try
			{
				DateTime Now = DateTime.UtcNow;
				string Module = typeof(IpHost).Namespace;
				
				using BinaryTcpClient Client = await this.ConnectTcp(false);

				List<Field> Fields = new List<Field>()
					{
						new QuantityField(this, Now, "Connect", (DateTime.UtcNow-Now).TotalMilliseconds, 0, "ms", FieldType.Momentary, FieldQoS.AutomaticReadout, Module, 13)
					};

				if (Request.IsIncluded(FieldType.Identity) && this.Tls)
				{
					IpHostPort.AddCertificateFields(Client.RemoteCertificate, 
						Client.RemoteCertificateValid, this, Now, Fields);
				}

				await Request.ReportFields(true, Fields);
			}
			catch (Exception ex)
			{
				await Request.ReportErrors(true, new ThingError(this, ex.Message));
			}
		}

	}
}
