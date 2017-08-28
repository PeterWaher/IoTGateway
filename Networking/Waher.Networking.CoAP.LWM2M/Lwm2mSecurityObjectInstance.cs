using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking.CoAP.LWM2M.ContentFormats;
using Waher.Persistence;
using Waher.Persistence.Attributes;

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
		[DefaultValueNull]
		public string ServerUri = null;

		/// <summary>
		/// MSISDN used by the LwM2M Client to send messages to the LwM2M Server via the SMS 
		/// binding. The LwM2M Client SHALL silently ignore any SMS originated from unknown MSISDN.
		/// </summary>
		[DefaultValueNull]
		public string ServerSmsNumber = null;

		/// <summary>
		/// Determines if the current instance concerns a LwM2M Bootstrap-Server(true) or a 
		/// standard LwM2M Server(false).
		/// </summary>
		[DefaultValueNull]
		public bool? BootstrapServer = null;

		/// <summary>
		/// Determines which UDP payload security mode is used.
		/// </summary>
		[DefaultValueNull]
		public SecurityMode? SecurityMode = null;

		/// <summary>
		/// Determines which SMS security mode is used(see section 7.2)
		/// </summary>
		[DefaultValueNull]
		public SmsSecurityMode? SmsSecurityMode = null;

		/// <summary>
		/// Stores the LwM2M Client‟s Certificate (Certificate mode), public key (RPK mode) or 
		/// PSK Identity (PSK mode). The format is defined in Section E.1.1.
		/// </summary>
		[DefaultValueNull]
		public byte[] PublicKeyOrIdentity = null;

		/// <summary>
		///  Stores the LwM2M Server‟s or LwM2M Bootstrap-Server‟s Certificate(Certificate mode), 
		///  public key(RPK mode). The format is defined in Section E.1.1.
		/// </summary>
		[DefaultValueNull]
		public byte[] ServerPublicKey = null;

		/// <summary>
		/// Stores the secret key or private key of the security mode. The format of the keying
		/// material is defined by the security mode in Section E.1.1. This Resource MUST only be 
		/// changed by a bootstrap-server and MUST NOT be readable by any server.
		/// </summary>
		[DefaultValueNull]
		public byte[] SecretKey = null;

		/// <summary>
		/// Stores the KIc, KID, SPI and TAR. The format is defined in Section E.1.2. 
		/// </summary>
		[DefaultValueNull]
		public byte[] SmsBindingKey = null;

		/// <summary>
		/// Stores the values of the key(s) for the SMS binding.
		/// 
		/// This resource MUST only be changed by a bootstrap-server and MUST NOT be readable 
		/// by any server.
		/// </summary>
		[DefaultValueNull]
		public byte[] SmsBindingSecretKey = null;

		/// <summary>
		/// This identifier uniquely identifies each LwM2M Server configured for the LwM2M Client.
		/// 
		/// This Resource MUST be set when the Bootstrap-Server Resource has false value.
		/// 
		/// Specific ID:0 and ID:65535 values MUST NOT be used for identifying the LwM2M Server
		/// (Section 6.3).
		/// </summary>
		[DefaultValueNull]
		public ushort? ShortServerId = null;

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
		public long? ClientHoldOffTimeSeconds = null;

		/// <summary>
		/// The LwM2M Client MUST purge the LwM2M Bootstrap-Server Account after the timeout value 
		/// given by this resource.The lowest timeout value is 1. 
		/// 
		/// If the value is set to 0, or if this resource is not instantiated, the Bootstrap-Server
		/// Account lifetime is infinite.
		/// </summary>
		[DefaultValueNull]
		public long? BootstrapServerAccountTimeoutSeconds = null;

		/// <summary>
		/// LWM2M Security object instance.
		/// </summary>
		public Lwm2mSecurityObjectInstance()
			: base(0, 0)
		{
		}

		/// <summary>
		/// LWM2M Security object instance.
		/// </summary>
		/// <param name="SubId">ID of object instance.</param>
		public Lwm2mSecurityObjectInstance(int SubId)
			: base(0, SubId)
		{
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
			if (this.Object.Client.State == Lwm2mState.Bootstrap)
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
			if (this.Object.Client.State == Lwm2mState.Bootstrap)
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
								ServerUri = Rec.AsString();
								break;

							case 1:
								BootstrapServer = Rec.AsBoolean();
								break;

							case 2:
								sbyte Mode = Rec.AsInt8();
								if (Mode < 0 || Mode > 4)
									throw new Exception("Invalid security mode: " + Mode.ToString());
								SecurityMode = (SecurityMode)Mode;
								break;

							case 3:
								PublicKeyOrIdentity = Rec.RawValue;
								break;

							case 4:
								ServerPublicKey = Rec.RawValue;
								break;

							case 5:
								SecretKey = Rec.RawValue;
								break;

							case 6:
								SmsSecurityMode = (SmsSecurityMode)((byte)Rec.AsInt8());
								break;

							case 7:
								SmsBindingKey = Rec.RawValue;
								break;

							case 8:
								SmsBindingSecretKey = Rec.RawValue;
								break;

							case 9:
								ServerSmsNumber = Rec.AsString();
								break;

							case 10:
								ShortServerId = (ushort)Rec.AsInteger();
								break;

							case 11:
								ClientHoldOffTimeSeconds = Rec.AsInteger();
								break;

							case 12:
								BootstrapServerAccountTimeoutSeconds = Rec.AsInteger();
								break;

							default:
								throw new Exception("Unrecognized identifier: " + Rec.Identifier);
						}
					}

					Log.Informational("Bootstrap information received.", this.Path, Request.From.ToString(),
						new KeyValuePair<string, object>("Server", ServerUri),
						new KeyValuePair<string, object>("BootstrapServer", BootstrapServer),
						new KeyValuePair<string, object>("SecurityMode", SecurityMode),
						new KeyValuePair<string, object>("ShortServerId", ShortServerId),
						new KeyValuePair<string, object>("ClientHoldOffTimeSeconds", ClientHoldOffTimeSeconds),
						new KeyValuePair<string, object>("BootstrapServerAccountTimeoutSeconds", BootstrapServerAccountTimeoutSeconds));
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
			if (this.ShortServerId.HasValue)
			{
				Output.Append(";ssid=");
				Output.Append(this.ShortServerId.Value.ToString());
			}
		}
	}
}
