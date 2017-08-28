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
	/// LWM2M Access Control object instance.
	/// 
	/// The Resource Instance ID MUST be the Short Server ID of a certain LwM2M Server for 
	/// which associated access rights are contained in the Resource Instance value. 
	/// 
	/// The Resource Instance ID 0 is a specific ID, determining the ACL Instance which contains the 
	/// default access rights. 
	/// 
	/// Each bit set in the Resource Instance value, grants an access right to the LwM2M Server to 
	/// the corresponding operation.
	/// </summary>
	public class Lwm2mAccessControlObjectInstance : Lwm2mObjectInstance, ICoapDeleteMethod, ICoapPutMethod
	{
		/// <summary>
		/// The Object ID are applied for.
		/// </summary>
		[DefaultValueNull]
		public ushort? AclObjectId = null;

		/// <summary>
		/// The Object Instance ID are applied for.
		/// </summary>
		[DefaultValueNull]
		public ushort? AclObjectInstanceId = null;

		/// <summary>
		/// Read, Observe, Discover, Write Attributes
		/// </summary>
		[DefaultValueNull]
		public bool? CanRead = null;

		/// <summary>
		/// Write
		/// </summary>
		[DefaultValueNull]
		public bool? CanWrite = null;

		/// <summary>
		/// Execute
		/// </summary>
		[DefaultValueNull]
		public bool? CanExecute = null;

		/// <summary>
		/// Delete
		/// </summary>
		[DefaultValueNull]
		public bool? CanDelete = null;

		/// <summary>
		/// Create
		/// </summary>
		[DefaultValueNull]
		public bool? CanCreate = null;

		/// <summary>
		/// Short Server ID of a certain LwM2M Server; only such an LwM2M Server can manage the 
		/// Resources of this Object Instance.  
		/// 
		/// The specific value MAX_ID=65535 means this Access Control Object Instance is created 
		/// and modified during a Bootstrap phase only. 
		/// </summary>
		[DefaultValueNull]
		public ushort? AccessControlOwner = null;

		/// <summary>
		/// LWM2M Access Control object instance.
		/// </summary>
		public Lwm2mAccessControlObjectInstance()
			: base(0, 0)
		{
		}

		/// <summary>
		/// LWM2M Access Control object instance.
		/// </summary>
		/// <param name="SubId">ID of object instance.</param>
		public Lwm2mAccessControlObjectInstance(int SubId)
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
			if (!string.IsNullOrEmpty(Request.SubPath))
				Response.RST(CoapCode.BadRequest);  // TODO: Handle individual resources.
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

				// E.1 LwM2M Object: LwM2M Security 
				// http://www.openmobilealliance.org/release/LightweightM2M/V1_0-20170208-A/OMA-TS-LightweightM2M-V1_0-20170208-A.pdf

				try
				{
					foreach (TlvRecord Rec in Records)
					{
						switch (Rec.Identifier)
						{
							case 0:
								this.AclObjectId = (ushort)Rec.AsInteger();
								break;

							case 1:
								this.AclObjectInstanceId = (ushort)Rec.AsInteger();
								break;

							case 2:
								long l = Rec.AsInteger();
								this.CanRead = (l & 1) != 0;
								this.CanWrite = (l & 2) != 0;
								this.CanExecute = (l & 4) != 0;
								this.CanDelete = (l & 8) != 0;
								this.CanCreate = (l & 16) != 0;
								break;

							case 3:
								this.AccessControlOwner = (ushort)Rec.AsInteger();
								break;

							default:
								throw new Exception("Unrecognized identifier: " + Rec.Identifier);
						}
					}

					Log.Informational("Access Control information received.", this.Path, Request.From.ToString(),
						new KeyValuePair<string, object>("Object", this.AclObjectId),
						new KeyValuePair<string, object>("Object Instance", this.AclObjectInstanceId),
						new KeyValuePair<string, object>("CanRead", this.CanRead),
						new KeyValuePair<string, object>("CanWrite", this.CanWrite),
						new KeyValuePair<string, object>("CanExecute", this.CanExecute),
						new KeyValuePair<string, object>("CanDelete", this.CanDelete),
						new KeyValuePair<string, object>("CanCreate", this.CanCreate),
						new KeyValuePair<string, object>("Owner", this.AccessControlOwner));
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

			if ((All || ResourceID.Value == 0) && this.AclObjectId.HasValue)
				Writer.Write(IdentifierType.Resource, 0, (short)this.AclObjectId.Value);

			if ((All || ResourceID.Value == 1) && this.AclObjectInstanceId.HasValue)
				Writer.Write(IdentifierType.Resource, 1, (short)this.AclObjectInstanceId.Value);

			if (All || ResourceID.Value == 2)
			{
				sbyte b = 0;

				if (this.CanRead.HasValue && this.CanRead.Value)
					b |= 1;

				if (this.CanWrite.HasValue && this.CanWrite.Value)
					b |= 2;

				if (this.CanExecute.HasValue && this.CanExecute.Value)
					b |= 4;

				if (this.CanDelete.HasValue && this.CanDelete.Value)
					b |= 8;

				if (this.CanCreate.HasValue && this.CanCreate.Value)
					b |= 16;

				Writer.Write(IdentifierType.Resource, 2, b);
			}

			if ((All || ResourceID.Value == 3) && this.AccessControlOwner.HasValue)
				Writer.Write(IdentifierType.Resource, 3, (short)this.AccessControlOwner.Value);
		}

	}
}
