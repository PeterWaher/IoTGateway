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
using Waher.Script.Constants;
using Waher.Security;
using Waher.Things.Attributes;
using Waher.Things.DisplayableParameters;
using Waher.Things.SensorData;

namespace Waher.Things.Ip
{
	/// <summary>
	/// Node representing a port on an IP Host machine.
	/// </summary>
	public class IpHostPort : IpHost
	{
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
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(IpHostPort), 23, "Port on IP Host");
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
			await Client.ConnectAsync(this.Host, this.port, this.tls);

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
			await Client.ConnectAsync(this.Host, this.port, this.tls);

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

			return Result;
		}

		/// <summary>
		/// Starts the readout of the sensor.
		/// </summary>
		/// <param name="Request">Request object. All fields and errors should be reported to this interface.</param>
		public async override Task StartReadout(ISensorReadout Request)
		{
			try
			{
				DateTime Now = DateTime.UtcNow;
				string Module = typeof(IpHost).Namespace;
				
				using BinaryTcpClient Client = await this.ConnectTcp(false);

				List<Field> Fields = new List<Field>()
					{
						new QuantityField(this, Now, "Connect", (DateTime.UtcNow-Now).TotalMilliseconds, 
							0, "ms", FieldType.Momentary, FieldQoS.AutomaticReadout, Module, 13)
					};

				if (Request.IsIncluded(FieldType.Identity) && this.Tls)
				{
					AddCertificateFields(Client.RemoteCertificate, 
						Client.RemoteCertificateValid, this, Now, Fields);
				}

				await Request.ReportFields(false, Fields);
			}
			catch (Exception ex)
			{
				await Request.ReportErrors(false, new ThingError(this, ex.Message));
			}

			await base.StartReadout(Request);
		}

		/// <summary>
		/// Adds information from a certificate as fields to a collection of fields.
		/// </summary>
		/// <param name="Certificate">Certificate</param>
		/// <param name="IsValid">If certificate is valid.</param>
		/// <param name="Node">Node reporting certificate.</param>
		/// <param name="Timestamp">Timestamp of readout.</param>
		/// <param name="Fields">Collection of fields.</param>
		public static void AddCertificateFields(X509Certificate Certificate, bool IsValid,
			ThingReference Node, DateTime Timestamp, List<Field> Fields)
		{
			if (!(Certificate is null))
			{
				string Module = typeof(IpHost).Namespace;
				string s;

				Fields.Add(new BooleanField(Node, Timestamp, "Certificate Valid", IsValid, FieldType.Identity | FieldType.Status, FieldQoS.AutomaticReadout, Module, 14));
				Fields.Add(new StringField(Node, Timestamp, "Subject", Certificate.Subject, FieldType.Identity, FieldQoS.AutomaticReadout, Module, 15));
				Fields.Add(new StringField(Node, Timestamp, "Issuer", Certificate.Issuer, FieldType.Identity, FieldQoS.AutomaticReadout, Module, 16));
				Fields.Add(new StringField(Node, Timestamp, "S/N", Certificate.GetSerialNumberString(), FieldType.Identity, FieldQoS.AutomaticReadout, Module, 17));
				Fields.Add(new StringField(Node, Timestamp, "Digest", Certificate.GetCertHashString(), FieldType.Identity, FieldQoS.AutomaticReadout, Module, 20));
				Fields.Add(new StringField(Node, Timestamp, "Algorithm", Certificate.GetKeyAlgorithm(), FieldType.Identity, FieldQoS.AutomaticReadout, Module, 21));
				Fields.Add(new StringField(Node, Timestamp, "Public Key", Certificate.GetPublicKeyString(), FieldType.Identity, FieldQoS.AutomaticReadout, Module, 22));

				if (CommonTypes.TryParseRfc822(s = Certificate.GetEffectiveDateString(), out DateTimeOffset TP))
					Fields.Add(new DateTimeField(Node, Timestamp, "Effective", TP.UtcDateTime, FieldType.Identity, FieldQoS.AutomaticReadout, Module, 18));
				else
					Fields.Add(new StringField(Node, Timestamp, "Effective", s, FieldType.Identity, FieldQoS.AutomaticReadout, Module, 18));

				if (CommonTypes.TryParseRfc822(s = Certificate.GetExpirationDateString(), out TP))
					Fields.Add(new DateTimeField(Node, Timestamp, "Expires", TP.UtcDateTime, FieldType.Identity, FieldQoS.AutomaticReadout, Module, 19));
				else
					Fields.Add(new StringField(Node, Timestamp, "Expires", s, FieldType.Identity, FieldQoS.AutomaticReadout, Module, 19));
			}
		}

	}
}
