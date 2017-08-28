using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking.CoAP.LWM2M.ContentFormats;
using Waher.Persistence;
using Waher.Persistence.Attributes;

namespace Waher.Networking.CoAP.LWM2M
{
	/// <summary>
	/// LWM2M Server object instance.
	/// </summary>
	public class Lwm2mServerObjectInstance : Lwm2mObjectInstance, ICoapDeleteMethod, ICoapPutMethod
	{
		/// <summary>
		/// Used as link to associate server Object Instance.
		/// </summary>
		[DefaultValueNull]
		public ushort? ShortServerId = null;

		/// <summary>
		/// Specify the lifetime of the registration in seconds (see Section 5.3 Registration).
		/// </summary>
		[DefaultValueNull]
		public long? LifetimeSeconds = null;

		/// <summary>
		/// The default value the LwM2M Client should use for the Minimum Period of an Observation 
		/// in the absence of this parameter being included in an Observation. 
		/// 
		/// If this Resource doesn‟t exist, the default value is 0.
		/// </summary>
		[DefaultValue(0)]
		public long DefaultMinimumPeriodSeconds = 0;

		/// <summary>
		/// The default value the LwM2M Client should use for the Maximum Period of an Observation 
		/// in the absence of this parameter being included in an Observation. 
		/// </summary>
		[DefaultValueNull]
		public long? DefaultMaximumPeriodSeconds = null;

		/// <summary>
		/// If this Resource is executed, this LwM2M Server Object is disabled for a certain 
		/// period defined in the Disabled Timeout Resource. After receiving “Execute” operation, 
		/// LwM2M Client MUST send response of the operation and perform de-registration process, 
		/// and underlying network connection between the Client and Server MUST be disconnected 
		/// to disable the LwM2M Server account.
		/// 
		/// After the above process, the LwM2M Client MUST NOT send any message to the Server and 
		/// ignore all the messages from the LwM2M Server for the period.
		/// </summary>
		[DefaultValueNull]
		public bool? Disabled = null;

		/// <summary>
		/// A period to disable the Server. After this period, the LwM2M Client MUST perform 
		/// registration process to the Server. If this Resource is not set, a default timeout 
		/// value is 86400 (1 day).
		/// </summary>
		[DefaultValue(86400)]
		public long DisableTimeoutSeconds = 86400;

		/// <summary>
		/// If true, the LwM2M Client stores “Notify” operations to the LwM2M Server while the 
		/// LwM2M Server account is disabled or the LwM2M Client is offline. After the LwM2M Server 
		/// account is enabled or the LwM2M Client is online, the LwM2M Client reports the stored 
		/// “Notify” operations to the Server.
		/// 
		/// If false, the LwM2M Client discards all the “Notify” operations or temporarily disables 
		/// the Observe function while the LwM2M Server is disabled or the LwM2M Client is offline.
		/// 
		/// The default value is true. The maximum number of storing Notifications per Server is up 
		/// to the implementation.
		/// </summary>
		[DefaultValueNull]
		public bool? NotificationStoring = null;

		/// <summary>
		/// This Resource defines the transport binding configured for the LwM2M Client. 
		/// 
		/// If the LwM2M Client supports the binding specified in this Resource, the LwM2M Client 
		/// MUST use that transport for the Current Binding Mode.
		/// </summary>
		[DefaultValueNull]
		public string Binding = null;

		/// <summary>
		/// If this Resource is executed the LwM2M Client MUST perform an “Update” operation with 
		/// this LwM2M Server using that transport for the Current Binding Mode.
		/// </summary>
		[DefaultValueNull]
		public bool? RegistrationUpdate = null;

		/// <summary>
		/// LWM2M Server object instance.
		/// </summary>
		public Lwm2mServerObjectInstance()
			: base(1, 0)
		{
		}

		/// <summary>
		/// LWM2M Server object instance.
		/// </summary>
		/// <param name="SubId">ID of object instance.</param>
		public Lwm2mServerObjectInstance(int SubId)
			: base(1, SubId)
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
			if (!string.IsNullOrEmpty(Request.SubPath))
				Response.RST(CoapCode.BadRequest);	// TODO: Handle individual resources.
			else if (this.Object.Client.State == Lwm2mState.Bootstrap &&
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
			if (!string.IsNullOrEmpty(Request.SubPath))
				Response.RST(CoapCode.BadRequest);  // TODO: Handle individual resources.
			else if (this.Object.Client.State == Lwm2mState.Bootstrap &&
				this.Object.Client.IsFromBootstrapServer(Request))
			{
				TlvRecord[] Records = Request.Decode() as TlvRecord[];
				if (Records == null)
				{
					Response.RST(CoapCode.BadRequest);
					return;
				}

				// E.2 LwM2M Object: LwM2M Server 
				// http://www.openmobilealliance.org/release/LightweightM2M/V1_0-20170208-A/OMA-TS-LightweightM2M-V1_0-20170208-A.pdf

				try
				{
					foreach (TlvRecord Rec in Records)
					{
						switch (Rec.Identifier)
						{
							case 0:
								this.ShortServerId = (ushort)Rec.AsInteger();
								break;

							case 1:
								this.LifetimeSeconds = Rec.AsInteger();
								break;

							case 2:
								this.DefaultMinimumPeriodSeconds = Rec.AsInteger();
								break;

							case 3:
								this.DefaultMaximumPeriodSeconds = Rec.AsInteger();
								break;

							case 4:
								this.Disabled = true;
								break;

							case 5:
								this.DisableTimeoutSeconds = Rec.AsInteger();
								break;

							case 6:
								this.NotificationStoring = Rec.AsBoolean();
								break;

							case 7:
								this.Binding = Rec.AsString();
								break;

							case 8:
								this.RegistrationUpdate = true;
								break;

							default:
								throw new Exception("Unrecognized identifier: " + Rec.Identifier);
						}
					}

					Log.Informational("Server information received.", this.Path, Request.From.ToString(),
						new KeyValuePair<string, object>("ShortServerId", this.ShortServerId),
						new KeyValuePair<string, object>("LifetimeSeconds", this.LifetimeSeconds),
						new KeyValuePair<string, object>("DefaultMinimumPeriodSeconds", this.DefaultMinimumPeriodSeconds),
						new KeyValuePair<string, object>("DefaultMaximumPeriodSeconds", this.DefaultMaximumPeriodSeconds),
						new KeyValuePair<string, object>("Disabled", this.Disabled),
						new KeyValuePair<string, object>("DisableTimeoutSeconds", this.DisableTimeoutSeconds),
						new KeyValuePair<string, object>("NotificationStoring", this.NotificationStoring),
						new KeyValuePair<string, object>("Binding", this.Binding),
						new KeyValuePair<string, object>("RegistrationUpdate", this.RegistrationUpdate));
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
		/// Exports resources.
		/// </summary>
		/// <param name="ResourceID">Resource ID, if a single resource is to be exported, otherwise null.</param>
		/// <param name="Writer">Output</param>
		public override void Export(int? ResourceID, ILwm2mWriter Writer)
		{
			bool All = !ResourceID.HasValue;

			if ((All || ResourceID.Value == 0) && this.ShortServerId.HasValue)
				Writer.Write(IdentifierType.Resource, 0, (short)this.ShortServerId.Value);

			if ((All || ResourceID.Value == 1) && this.LifetimeSeconds.HasValue)
				Writer.Write(IdentifierType.Resource, 1, this.LifetimeSeconds.Value);

			if (All || ResourceID.Value == 2)
				Writer.Write(IdentifierType.Resource, 2, this.DefaultMinimumPeriodSeconds);

			if ((All || ResourceID.Value == 3) && this.DefaultMaximumPeriodSeconds.HasValue)
				Writer.Write(IdentifierType.Resource, 3, this.DefaultMaximumPeriodSeconds.Value);

			if ((All || ResourceID.Value == 4) && this.Disabled.HasValue)
				Writer.Write(IdentifierType.Resource, 4, this.Disabled.Value);

			if (All || ResourceID.Value == 5)
				Writer.Write(IdentifierType.Resource, 5, this.DisableTimeoutSeconds);

			if ((All || ResourceID.Value == 6) && this.NotificationStoring.HasValue)
				Writer.Write(IdentifierType.Resource, 6, this.NotificationStoring.Value);

			if ((All || ResourceID.Value == 7) && this.Binding != null)
				Writer.Write(IdentifierType.Resource, 7, this.Binding);

			if ((All || ResourceID.Value == 8) && this.RegistrationUpdate.HasValue)
				Writer.Write(IdentifierType.Resource, 8, this.RegistrationUpdate.Value);
		}

	}
}
