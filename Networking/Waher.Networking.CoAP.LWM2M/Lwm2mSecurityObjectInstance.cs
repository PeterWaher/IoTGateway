using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking.CoAP.LWM2M.ContentFormats;
using Waher.Persistence;
using Waher.Persistence.Attributes;
using Waher.Security.DTLS;

namespace Waher.Networking.CoAP.LWM2M
{
	/// <summary>
	/// LWM2M Security object instance.
	/// </summary>
	public class Lwm2mSecurityObjectInstance : Lwm2mObjectInstance, ICoapDeleteMethod, ICoapPutMethod
	{
		/// <summary>
		/// Uniquely identifis the LwM2M Server or LwM2M Bootstrap-Server. The format of the CoAP 
		/// URI is defined in Section 6 of RFC 7252.
		/// </summary>
		private Lwm2mResourceString serverUri = new Lwm2mResourceString(0, 0, 0, null);

		/// <summary>
		/// Determines if the current instance concerns a LwM2M Bootstrap-Server(true) or a 
		/// standard LwM2M Server(false).
		/// </summary>
		private Lwm2mResourceBoolean bootstrapServer = new Lwm2mResourceBoolean(0, 0, 1, null);

		/// <summary>
		/// Determines which UDP payload security mode is used.
		/// </summary>
		private Lwm2mResourceInteger securityMode = new Lwm2mResourceInteger(0, 0, 2, null, false);

		/// <summary>
		/// Stores the LwM2M Client‟s Certificate (Certificate mode), public key (RPK mode) or 
		/// PSK Identity (PSK mode). The format is defined in Section E.1.1.
		/// </summary>
		private Lwm2mResourceOpaque publicKeyOrIdentity = new Lwm2mResourceOpaque(0, 0, 3, null);

		/// <summary>
		///  Stores the LwM2M Server‟s or LwM2M Bootstrap-Server‟s Certificate(Certificate mode), 
		///  public key(RPK mode). The format is defined in Section E.1.1.
		/// </summary>
		private Lwm2mResourceOpaque serverPublicKey = new Lwm2mResourceOpaque(0, 0, 4, null);

		/// <summary>
		/// Stores the secret key or private key of the security mode. The format of the keying
		/// material is defined by the security mode in Section E.1.1. This Resource MUST only be 
		/// changed by a bootstrap-server and MUST NOT be readable by any server.
		/// </summary>
		private Lwm2mResourceOpaque secretKey = new Lwm2mResourceOpaque(0, 0, 5, null);

		/// <summary>
		/// This identifier uniquely identifies each LwM2M Server configured for the LwM2M Client.
		/// 
		/// This Resource MUST be set when the Bootstrap-Server Resource has false value.
		/// 
		/// Specific ID:0 and ID:65535 values MUST NOT be used for identifying the LwM2M Server
		/// (Section 6.3).
		/// </summary>
		private Lwm2mResourceInteger shortServerId = new Lwm2mResourceInteger(0, 0, 10, null, false);

		/// <summary>
		/// Relevant information for a BootstrapServer only.
		/// 
		/// The number of seconds to wait before initiating a Client Initiated Bootstrap once the 
		/// LwM2M Client has determined it should initiate this bootstrap mode.
		/// 
		/// In case client initiated bootstrap is supported by the LwM2M Client, this resource
		/// MUST be supported.
		/// </summary>
		public Lwm2mResourceInteger clientHoldOffTimeSeconds = new Lwm2mResourceInteger(0, 0, 11, null, false);

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
			this.serverUri = new Lwm2mResourceString(0, InstanceId, 0, null);
			this.bootstrapServer = new Lwm2mResourceBoolean(0, InstanceId, 1, null);
			this.securityMode = new Lwm2mResourceInteger(0, InstanceId, 2, null, false);
			this.publicKeyOrIdentity = new Lwm2mResourceOpaque(0, InstanceId, 3, null);
			this.serverPublicKey = new Lwm2mResourceOpaque(0, InstanceId, 4, null);
			this.secretKey = new Lwm2mResourceOpaque(0, InstanceId, 5, null);
			this.shortServerId = new Lwm2mResourceInteger(0, InstanceId, 10, null, false);
			this.clientHoldOffTimeSeconds = new Lwm2mResourceInteger(0, InstanceId, 11, null, false);

			this.Add(this.serverUri);
			this.Add(this.bootstrapServer);
			this.Add(this.securityMode);
			this.Add(this.publicKeyOrIdentity);
			this.Add(this.serverPublicKey);
			this.Add(this.secretKey);
			this.Add(this.shortServerId);
			this.Add(this.clientHoldOffTimeSeconds);
		}

		/// <summary>
		/// Uniquely identifis the LwM2M Server or LwM2M Bootstrap-Server. The format of the CoAP 
		/// URI is defined in Section 6 of RFC 7252.
		/// </summary>
		[DefaultValueNull]
		private string ServerUri
		{
			get { return this.serverUri.StringValue; }
			set { this.serverUri.StringValue = value; }
		}

		/// <summary>
		/// Determines if the current instance concerns a LwM2M Bootstrap-Server(true) or a 
		/// standard LwM2M Server(false).
		/// </summary>
		[DefaultValueNull]
		private bool? BootstrapServer
		{
			get { return this.bootstrapServer.BooleanValue; }
			set { this.bootstrapServer.BooleanValue = value; }
		}

		/// <summary>
		/// Determines which UDP payload security mode is used.
		/// </summary>
		[DefaultValueNull]
		private SecurityMode? SecurityMode
		{
			get { return (SecurityMode?)this.securityMode.IntegerValue; }
			set { this.securityMode.IntegerValue = (int?)SecurityMode; }
		}

		/// <summary>
		/// Stores the LwM2M Client‟s Certificate (Certificate mode), public key (RPK mode) or 
		/// PSK Identity (PSK mode). The format is defined in Section E.1.1.
		/// </summary>
		[DefaultValueNull]
		private byte[] PublicKeyOrIdentity
		{
			get { return this.publicKeyOrIdentity.OpaqueValue; }
			set { this.publicKeyOrIdentity.OpaqueValue = value; }
		}

		/// <summary>
		///  Stores the LwM2M Server‟s or LwM2M Bootstrap-Server‟s Certificate(Certificate mode), 
		///  public key(RPK mode). The format is defined in Section E.1.1.
		/// </summary>
		[DefaultValueNull]
		private byte[] ServerPublicKey
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
		private byte[] SecretKey
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
		private ushort? ShortServerId
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
		/// If the PUT method is allowed.
		/// </summary>
		public bool AllowsPUT => true;

		/// <summary>
		/// Executes the GET method on the resource.
		/// </summary>
		/// <param name="Request">CoAP Request</param>
		/// <param name="Response">CoAP Response</param>
		/// <exception cref="CoapException">If an error occurred when processing the method.</exception>
		public void DELETE(CoapMessage Request, CoapResponse Response)
		{
			if (this.Object.Client.State == Lwm2mState.Bootstrap &&
				this.Object.Client.IsFromBootstrapServer(Request))
			{
				Task T = this.DeleteBootstrapInfo();
				Response.ACK(CoapCode.Deleted);
			}
			else
				Response.RST(CoapCode.Unauthorized);
		}

		/// <summary>
		/// Deletes any Bootstrap information.
		/// </summary>
		public override async Task DeleteBootstrapInfo()
		{
			if (this.ObjectId != null)
				await Database.Delete(this);
		}

		/// <summary>
		/// Applies any Bootstrap information.
		/// </summary>
		public override async Task ApplyBootstrapInfo()
		{
			if (this.ObjectId == null)
				await Database.Insert(this);
		}

		/// <summary>
		/// Executes the GET method on the resource.
		/// </summary>
		/// <param name="Request">CoAP Request</param>
		/// <param name="Response">CoAP Response</param>
		/// <exception cref="CoapException">If an error occurred when processing the method.</exception>
		public void PUT(CoapMessage Request, CoapResponse Response)
		{
			if (this.Object.Client.State == Lwm2mState.Bootstrap &&
				this.Object.Client.IsFromBootstrapServer(Request))
			{
				TlvRecord[] Records = Request.Decode() as TlvRecord[];
				if (Records == null)
				{
					Response.RST(CoapCode.BadRequest);
					return;
				}

				// E.1 LwM2M Object: LwM2M Security 
				// http://www.openmobilealliance.org/release/LightweightM2M/V1_0-20170208-A/OMA-TS-LightweightM2M-V1_0-20170208-A.pdf

				try
				{
					foreach (TlvRecord Rec in Records)
					{
						switch (Rec.Identifier)
						{
							case 0:
								this.serverUri.Read(Rec);
								break;

							case 1:
								this.bootstrapServer.Read(Rec);
								break;

							case 2:
								this.securityMode.Read(Rec);
								break;

							case 3:
								this.publicKeyOrIdentity.Read(Rec);
								break;

							case 4:
								this.serverPublicKey.Read(Rec);
								break;

							case 5:
								this.secretKey.Read(Rec);
								break;

							case 10:
								this.shortServerId.Read(Rec);
								break;

							case 11:
								this.clientHoldOffTimeSeconds.Read(Rec);
								break;
						}
					}

					Log.Informational("Bootstrap information received.", this.Path, Request.From.ToString(),
						new KeyValuePair<string, object>("Server", this.serverUri.Value),
						new KeyValuePair<string, object>("BootstrapServer", this.bootstrapServer.Value),
						new KeyValuePair<string, object>("SecurityMode", this.securityMode.Value),
						new KeyValuePair<string, object>("ShortServerId", this.shortServerId.Value),
						new KeyValuePair<string, object>("ClientHoldOffTimeSeconds", this.clientHoldOffTimeSeconds.Value));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
					Response.RST(CoapCode.BadRequest);
					return;
				}

				Response.ACK(CoapCode.Changed);
			}
			else
				Response.RST(CoapCode.Unauthorized);
		}

		/// <summary>
		/// Encodes any link parameters to the object link.
		/// </summary>
		/// <param name="Output">Link output.</param>
		public override void EncodeLinkParameters(StringBuilder Output)
		{
			if (this.shortServerId.Value != null)
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
		public override void GET(CoapMessage Request, CoapResponse Response)
		{
			if (this.Object.Client.IsFromBootstrapServer(Request))
				base.GET(Request, Response);
			else
				Response.RST(CoapCode.Forbidden);
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

						if (this.publicKeyOrIdentity == null ||
							this.secretKey == null)
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
