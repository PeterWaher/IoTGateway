using System;
using System.Text;
using System.Threading.Tasks;
using Waher.Networking.CoAP;
using Waher.Networking.LWM2M.Events;
using Waher.Persistence;
using Waher.Persistence.Attributes;
using Waher.Security.DTLS;

namespace Waher.Networking.LWM2M
{
	/// <summary>
	/// LWM2M Security object instance.
	/// </summary>
	public class Lwm2mSecurityObjectInstance : Lwm2mObjectInstance, ICoapDeleteMethod, ICoapPutMethod, ICoapPostMethod
	{
		/// <summary>
		/// Uniquely identifis the LwM2M Server or LwM2M Bootstrap-Server. The format of the CoAP 
		/// URI is defined in Section 6 of RFC 7252.
		/// </summary>
		private readonly Lwm2mResourceString serverUri;

		/// <summary>
		/// Determines if the current instance concerns a LwM2M Bootstrap-Server(true) or a 
		/// standard LwM2M Server(false).
		/// </summary>
		private readonly Lwm2mResourceBoolean bootstrapServer;

		/// <summary>
		/// Determines which UDP payload security mode is used.
		/// </summary>
		private readonly Lwm2mResourceInteger securityMode;

		/// <summary>
		/// Stores the LwM2M Client‟s Certificate (Certificate mode), public key (RPK mode) or 
		/// PSK Identity (PSK mode). The format is defined in Section E.1.1.
		/// </summary>
		private readonly Lwm2mResourceOpaque publicKeyOrIdentity;

		/// <summary>
		///  Stores the LwM2M Server‟s or LwM2M Bootstrap-Server‟s Certificate(Certificate mode), 
		///  public key(RPK mode). The format is defined in Section E.1.1.
		/// </summary>
		private readonly Lwm2mResourceOpaque serverPublicKey;

		/// <summary>
		/// Stores the secret key or private key of the security mode. The format of the keying
		/// material is defined by the security mode in Section E.1.1. This Resource MUST only be 
		/// changed by a bootstrap-server and MUST NOT be readable by any server.
		/// </summary>
		private readonly Lwm2mResourceOpaque secretKey;

		/// <summary>
		/// This identifier uniquely identifies each LwM2M Server configured for the LwM2M Client.
		/// 
		/// This Resource MUST be set when the Bootstrap-Server Resource has false value.
		/// 
		/// Specific ID:0 and ID:65535 values MUST NOT be used for identifying the LwM2M Server
		/// (Section 6.3).
		/// </summary>
		private readonly Lwm2mResourceInteger shortServerId;

		/// <summary>
		/// Relevant information for a BootstrapServer only.
		/// 
		/// The number of seconds to wait before initiating a Client Initiated Bootstrap once the 
		/// LwM2M Client has determined it should initiate this bootstrap mode.
		/// 
		/// In case client initiated bootstrap is supported by the LwM2M Client, this resource
		/// MUST be supported.
		/// </summary>
		public Lwm2mResourceInteger clientHoldOffTimeSeconds;

		/// <summary>
		/// LWM2M Security object instance.
		/// </summary>
		public Lwm2mSecurityObjectInstance()
			: this(0)
		{
		}

		/// <summary>
		/// LWM2M Security object instance.
		/// </summary>
		/// <param name="InstanceId">ID of object instance.</param>
		public Lwm2mSecurityObjectInstance(ushort InstanceId)
			: base(0, InstanceId)
		{
			// E.1 LwM2M Object: LwM2M Security 
			// http://www.openmobilealliance.org/release/LightweightM2M/V1_0-20170208-A/OMA-TS-LightweightM2M-V1_0-20170208-A.pdf

			this.serverUri = new Lwm2mResourceString("Server", 0, InstanceId, 0, false, false, null);
			this.bootstrapServer = new Lwm2mResourceBoolean("BootstrapServer", 0, InstanceId, 1, false, false, null);
			this.securityMode = new Lwm2mResourceInteger("SecurityMode", 0, InstanceId, 2, false, false, null, false);
			this.publicKeyOrIdentity = new Lwm2mResourceOpaque(null, 0, InstanceId, 3, false, false, null);
			this.serverPublicKey = new Lwm2mResourceOpaque(null, 0, InstanceId, 4, false, false, null);
			this.secretKey = new Lwm2mResourceOpaque(null, 0, InstanceId, 5, false, false, null);
			this.shortServerId = new Lwm2mResourceInteger("ShortServerId", 0, InstanceId, 10, false, false, null, false);
			this.clientHoldOffTimeSeconds = new Lwm2mResourceInteger("ClientHoldOffTimeSeconds", 0, InstanceId, 11, false, false, null, false);

			this.serverUri.OnBeforeGet += this.CheckFromBootstrapServer;
			this.bootstrapServer.OnBeforeGet += this.CheckFromBootstrapServer;
			this.securityMode.OnBeforeGet += this.CheckFromBootstrapServer;
			this.publicKeyOrIdentity.OnBeforeGet += this.CheckFromBootstrapServer;
			this.serverPublicKey.OnBeforeGet += this.CheckFromBootstrapServer;
			this.secretKey.OnBeforeGet += this.CheckFromBootstrapServer;
			this.shortServerId.OnBeforeGet += this.CheckFromBootstrapServer;
			this.clientHoldOffTimeSeconds.OnBeforeGet += this.CheckFromBootstrapServer;

			this.Add(this.serverUri);
			this.Add(this.bootstrapServer);
			this.Add(this.securityMode);
			this.Add(this.publicKeyOrIdentity);
			this.Add(this.serverPublicKey);
			this.Add(this.secretKey);
			this.Add(new Lwm2mResourceNotSupported(0, InstanceId, 6));  // SMS Security Mode 
			this.Add(new Lwm2mResourceNotSupported(0, InstanceId, 7));  // SMS Binding Key Parameters
			this.Add(new Lwm2mResourceNotSupported(0, InstanceId, 8));  // SMS Binding Secret Key(s)
			this.Add(new Lwm2mResourceNotSupported(0, InstanceId, 9));  // LwM2M Server SMS Number 
			this.Add(this.shortServerId);
			this.Add(this.clientHoldOffTimeSeconds);
			this.Add(new Lwm2mResourceNotSupported(0, InstanceId, 12)); //  BootstrapServer Account Timeout
		}

		private Task CheckFromBootstrapServer(object Sender, CoapRequestEventArgs e)
		{
			if (!this.Object.Client.IsFromBootstrapServer(e.Request))
				throw new CoapException(CoapCode.Unauthorized);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Uniquely identifis the LwM2M Server or LwM2M Bootstrap-Server. The format of the CoAP 
		/// URI is defined in Section 6 of RFC 7252.
		/// </summary>
		[DefaultValueNull]
		public string ServerUri
		{
			get { return this.serverUri.StringValue; }
			set { this.serverUri.StringValue = value; }
		}

		/// <summary>
		/// Determines if the current instance concerns a LwM2M Bootstrap-Server(true) or a 
		/// standard LwM2M Server(false).
		/// </summary>
		[DefaultValueNull]
		public bool? BootstrapServer
		{
			get { return this.bootstrapServer.BooleanValue; }
			set { this.bootstrapServer.BooleanValue = value; }
		}

		/// <summary>
		/// Determines which UDP payload security mode is used.
		/// </summary>
		[DefaultValueNull]
		public SecurityMode? SecurityMode
		{
			get { return (SecurityMode?)this.securityMode.IntegerValue; }
			set { this.securityMode.IntegerValue = (int?)value; }
		}

		/// <summary>
		/// Stores the LwM2M Client‟s Certificate (Certificate mode), public key (RPK mode) or 
		/// PSK Identity (PSK mode). The format is defined in Section E.1.1.
		/// </summary>
		[DefaultValueNull]
		public byte[] PublicKeyOrIdentity
		{
			get { return this.publicKeyOrIdentity.OpaqueValue; }
			set { this.publicKeyOrIdentity.OpaqueValue = value; }
		}

		/// <summary>
		///  Stores the LwM2M Server‟s or LwM2M Bootstrap-Server‟s Certificate(Certificate mode), 
		///  public key(RPK mode). The format is defined in Section E.1.1.
		/// </summary>
		[DefaultValueNull]
		public byte[] ServerPublicKey
		{
			get { return this.serverPublicKey.OpaqueValue; }
			set { this.serverPublicKey.OpaqueValue = value; }
		}

		/// <summary>
		/// Stores the secret key or private key of the security mode. The format of the keying
		/// material is defined by the security mode in Section E.1.1. This Resource MUST only be 
		/// changed by a bootstrap-server and MUST NOT be readable by any server.
		/// </summary>
		[DefaultValueNull]
		public byte[] SecretKey
		{
			get { return this.secretKey.OpaqueValue; }
			set { this.secretKey.OpaqueValue = value; }
		}

		/// <summary>
		/// This identifier uniquely identifies each LwM2M Server configured for the LwM2M Client.
		/// 
		/// This Resource MUST be set when the Bootstrap-Server Resource has false value.
		/// 
		/// Specific ID:0 and ID:65535 values MUST NOT be used for identifying the LwM2M Server
		/// (Section 6.3).
		/// </summary>
		[DefaultValueNull]
		public ushort? ShortServerId
		{
			get { return (ushort?)this.shortServerId.IntegerValue; }
			set { this.shortServerId.IntegerValue = value; }
		}

		/// <summary>
		/// Relevant information for a BootstrapServer only.
		/// 
		/// The number of seconds to wait before initiating a Client Initiated Bootstrap once the 
		/// LwM2M Client has determined it should initiate this bootstrap mode.
		/// 
		/// In case client initiated bootstrap is supported by the LwM2M Client, this resource
		/// MUST be supported.
		/// </summary>
		[DefaultValueNull]
		public long? ClientHoldOffTimeSeconds
		{
			get { return this.clientHoldOffTimeSeconds.IntegerValue; }
			set { this.clientHoldOffTimeSeconds.IntegerValue = value; }
		}

		/// <summary>
		/// If the DELETE method is allowed.
		/// </summary>
		public bool AllowsDELETE => true;

		/// <summary>
		/// Executes the DELETE method on the resource.
		/// </summary>
		/// <param name="Request">CoAP Request</param>
		/// <param name="Response">CoAP Response</param>
		/// <exception cref="CoapException">If an error occurred when processing the method.</exception>
		public async Task DELETE(CoapMessage Request, CoapResponse Response)
		{
			if (this.Object.Client.State == Lwm2mState.Bootstrap &&
				this.Object.Client.IsFromBootstrapServer(Request))
			{
				await this.DeleteBootstrapInfo();
				await Response.ACK(CoapCode.Deleted);
			}
			else
				await Response.RST(CoapCode.Unauthorized);
		}

		/// <summary>
		/// Deletes any Bootstrap information.
		/// </summary>
		public override async Task DeleteBootstrapInfo()
		{
			if (!(this.ObjectId is null))
				await Database.Delete(this);
		}

		/// <summary>
		/// Applies any Bootstrap information.
		/// </summary>
		public override async Task ApplyBootstrapInfo()
		{
			if (this.ObjectId is null)
				await Database.Insert(this);
			else
				await Database.Update(this);
		}

		/// <summary>
		/// Executes the PUT method on the resource.
		/// </summary>
		/// <param name="Request">CoAP Request</param>
		/// <param name="Response">CoAP Response</param>
		/// <exception cref="CoapException">If an error occurred when processing the method.</exception>
		public override Task PUT(CoapMessage Request, CoapResponse Response)
		{
			if (this.Object.Client.State == Lwm2mState.Bootstrap &&
				this.Object.Client.IsFromBootstrapServer(Request))
			{
				return base.PUT(Request, Response);
			}
			else
			{
				Response.RST(CoapCode.Unauthorized);
				return Task.CompletedTask;
			}
		}

		/// <summary>
		/// Encodes any link parameters to the object link.
		/// </summary>
		/// <param name="Output">Link output.</param>
		public override void EncodeLinkParameters(StringBuilder Output)
		{
			if (!(this.shortServerId.Value is null))
			{
				Output.Append(";ssid=");
				Output.Append(this.shortServerId.Value.ToString());
			}
		}

		/// <summary>
		/// Executes the GET method on the resource.
		/// </summary>
		/// <param name="Request">CoAP Request</param>
		/// <param name="Response">CoAP Response</param>
		/// <exception cref="CoapException">If an error occurred when processing the method.</exception>
		public override Task GET(CoapMessage Request, CoapResponse Response)
		{
			if (this.Object.Client.IsFromBootstrapServer(Request))
				return base.GET(Request, Response);
			else
			{
				Response.RST(CoapCode.Unauthorized);
				return Task.CompletedTask;
			}
		}

		internal Lwm2mServerReference GetServerReference(bool BootstrapServer)
		{
			if (!this.bootstrapServer.BooleanValue.HasValue ||
				(this.bootstrapServer.BooleanValue.Value ^ BootstrapServer) ||
				string.IsNullOrEmpty(this.serverUri.StringValue))
			{
				return null;
			}

			try
			{
				Uri Uri = new Uri(this.serverUri.StringValue);
				string s = Uri.PathAndQuery;
				int Port;

				if (!string.IsNullOrEmpty(s) && s != "/")
					return null;

				s = Uri.Scheme.ToLower();

				if (Uri.IsDefaultPort)
				{
					switch (s)
					{
						case "coap":
							Port = CoapEndpoint.DefaultCoapPort;
							break;

						case "coaps":
							Port = CoapEndpoint.DefaultCoapsPort;
							break;

						default:
							return null;
					}
				}
				else
					Port = Uri.Port;

				if (!this.securityMode.IntegerValue.HasValue)
					return null;

				SecurityMode SecurityMode = (SecurityMode)this.securityMode.IntegerValue.Value;

				switch (SecurityMode)
				{
					case SecurityMode.NoSec:
						if (s != "coap")
							return null;

						return new Lwm2mServerReference(Uri.Host, Port);

					case SecurityMode.PSK:
						if (s != "coaps")
							return null;

						if (this.publicKeyOrIdentity is null ||
							this.secretKey is null)
						{
							return null;
						}

						PresharedKey Credentials = new PresharedKey(
							this.publicKeyOrIdentity.OpaqueValue,
							this.secretKey.OpaqueValue);

						return new Lwm2mServerReference(Uri.Host, Port, Credentials);

					default:
						return null;
				}
			}
			catch (Exception)
			{
				return null;
			}
		}

		internal bool IsServer(ushort ShortServerId)
		{
			return this.shortServerId.IntegerValue.HasValue &&
				this.shortServerId.IntegerValue.Value == ShortServerId;
		}


	}
}
