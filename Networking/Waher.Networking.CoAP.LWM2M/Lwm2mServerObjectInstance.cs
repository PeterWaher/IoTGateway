using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking.CoAP.LWM2M.ContentFormats;
using Waher.Persistence;

namespace Waher.Networking.CoAP.LWM2M
{
	/// <summary>
	/// LWM2M Server object instance.
	/// </summary>
	public class Lwm2mServerObjectInstance : Lwm2mObjectInstance, ICoapDeleteMethod, ICoapPutMethod
	{
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

				// E.2 LwM2M Object: LwM2M Server 
				// http://www.openmobilealliance.org/release/LightweightM2M/V1_0-20170208-A/OMA-TS-LightweightM2M-V1_0-20170208-A.pdf

				ushort? ShortServerId = null;
				long? LifetimeSeconds = null;
				long? DefaultMinimumPeriodSeconds = null;
				long? DefaultMaximumPeriodSeconds = null;
				bool? Disabled = null;
				bool? RegistrationUpdate = null;
				long DisableTimeoutSeconds = 86400;
				bool? NotificationStoring = null;
				string Binding = null;

				try
				{
					foreach (TlvRecord Rec in Records)
					{
						switch (Rec.Identifier)
						{
							case 0:
								ShortServerId = (ushort)Rec.AsInteger();
								break;

							case 1:
								LifetimeSeconds = Rec.AsInteger();
								break;

							case 2:
								DefaultMinimumPeriodSeconds = Rec.AsInteger();
								break;

							case 3:
								DefaultMaximumPeriodSeconds = Rec.AsInteger();
								break;

							case 4:
								Disabled = true;
								break;

							case 5:
								DisableTimeoutSeconds = Rec.AsInteger();
								break;

							case 6:
								NotificationStoring = Rec.AsBoolean();
								break;

							case 7:
								Binding = Rec.AsString();
								break;

							case 8:
								RegistrationUpdate = true;
								break;

							default:
								throw new Exception("Unrecognized identifier: " + Rec.Identifier);
						}
					}

					Log.Informational("Server information received.", this.Path, Request.From.ToString());
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
	}
}
