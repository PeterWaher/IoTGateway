using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking.CoAP;
using Waher.Networking.LWM2M.ContentFormats;
using Waher.Networking.LWM2M.Events;
using Waher.Persistence;
using Waher.Persistence.Attributes;

namespace Waher.Networking.LWM2M
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
	public class Lwm2mAccessControlObjectInstance : Lwm2mObjectInstance, ICoapDeleteMethod
	{
		/// <summary>
		/// The Object ID are applied for.
		/// </summary>
		private Lwm2mResourceInteger aclObjectId;

		/// <summary>
		/// The Object Instance ID are applied for.
		/// </summary>
		private Lwm2mResourceInteger aclObjectInstanceId;

		/// <summary>
		/// ACL privilges.
		/// </summary>
		private Lwm2mResourceInteger aclPrivileges;

		/// <summary>
		/// Short Server ID of a certain LwM2M Server; only such an LwM2M Server can manage the 
		/// Resources of this Object Instance.  
		/// 
		/// The specific value MAX_ID=65535 means this Access Control Object Instance is created 
		/// and modified during a Bootstrap phase only. 
		/// </summary>
		private Lwm2mResourceInteger accessControlOwner;

		/// <summary>
		/// LWM2M Access Control object instance.
		/// </summary>
		public Lwm2mAccessControlObjectInstance()
			: this(0)
		{
		}

		/// <summary>
		/// LWM2M Access Control object instance.
		/// </summary>
		/// <param name="InstanceId">ID of object instance.</param>
		public Lwm2mAccessControlObjectInstance(ushort InstanceId)
			: base(2, InstanceId)
		{
			// E.1 LwM2M Object: LwM2M Security 
			// http://www.openmobilealliance.org/release/LightweightM2M/V1_0-20170208-A/OMA-TS-LightweightM2M-V1_0-20170208-A.pdf

			this.aclObjectId = new Lwm2mResourceInteger("Object", 2, InstanceId, 0, false, false, null, false);
			this.aclObjectInstanceId = new Lwm2mResourceInteger("Object Instance", 2, InstanceId, 1, false, false, null, false);
			this.aclPrivileges = new Lwm2mResourceInteger("Privileges", 2, InstanceId, 2, true, false, null, false);
			this.accessControlOwner = new Lwm2mResourceInteger("Owner", 2, InstanceId, 3, true, false, null, false);
		}

		/// <summary>
		/// The Object ID are applied for.
		/// </summary>
		[DefaultValueNull]
		public ushort? AclObjectId
		{
			get { return (ushort?)this.aclObjectId.IntegerValue; }
			set { this.aclObjectId.IntegerValue = value; }
		}

		/// <summary>
		/// The Object Instance ID are applied for.
		/// </summary>
		[DefaultValueNull]
		public ushort? AclObjectInstanceId
		{
			get { return (ushort?)this.aclObjectInstanceId.IntegerValue; }
			set { this.aclObjectInstanceId.IntegerValue = value; }
		}

		/// <summary>
		/// Read, Observe, Discover, Write Attributes
		/// </summary>
		[DefaultValueNull]
		public bool? CanRead
		{
			get { return this.GetPrivilege((byte)AclPrivilege.Read); }
			set { this.SetPrivilege((byte)AclPrivilege.Read, value); }
		}

		/// <summary>
		/// Write
		/// </summary>
		[DefaultValueNull]
		public bool? CanWrite
		{
			get { return this.GetPrivilege((byte)AclPrivilege.Write); }
			set { this.SetPrivilege((byte)AclPrivilege.Write, value); }
		}

		/// <summary>
		/// Execute
		/// </summary>
		[DefaultValueNull]
		public bool? CanExecute
		{
			get { return this.GetPrivilege((byte)AclPrivilege.Execute); }
			set { this.SetPrivilege((byte)AclPrivilege.Execute, value); }
		}

		/// <summary>
		/// Delete
		/// </summary>
		[DefaultValueNull]
		public bool? CanDelete
		{
			get { return this.GetPrivilege((byte)AclPrivilege.Delete); }
			set { this.SetPrivilege((byte)AclPrivilege.Delete, value); }
		}

		/// <summary>
		/// Create
		/// </summary>
		[DefaultValueNull]
		public bool? CanCreate
		{
			get { return this.GetPrivilege((byte)AclPrivilege.Create); }
			set { this.SetPrivilege((byte)AclPrivilege.Create, value); }
		}

		/// <summary>
		/// Short Server ID of a certain LwM2M Server; only such an LwM2M Server can manage the 
		/// Resources of this Object Instance.  
		/// 
		/// The specific value MAX_ID=65535 means this Access Control Object Instance is created 
		/// and modified during a Bootstrap phase only. 
		/// </summary>
		[DefaultValueNull]
		public ushort? AccessControlOwner
		{
			get { return (ushort?)this.accessControlOwner.IntegerValue; }
			set { this.accessControlOwner.IntegerValue = value; }
		}

		private bool? GetPrivilege(int Mask)
		{
			if (this.aclPrivileges.IntegerValue.HasValue)
				return (this.aclPrivileges.IntegerValue.Value & Mask) != 0;
			else
				return null;
		}

		private void SetPrivilege(byte Mask, bool? Value)
		{
			byte b;

			if (this.aclPrivileges.IntegerValue.HasValue)
				b = (byte)this.aclPrivileges.IntegerValue.Value;
			else
				b = 0;

			if (Value.HasValue && Value.Value)
				b |= Mask;
			else
				b &= (byte)~Mask;

			this.aclPrivileges.IntegerValue = b;
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
			else
				await Database.Update(this);
		}

		/// <summary>
		/// Executes the PUT method on the resource.
		/// </summary>
		/// <param name="Request">CoAP Request</param>
		/// <param name="Response">CoAP Response</param>
		/// <exception cref="CoapException">If an error occurred when processing the method.</exception>
		public override void PUT(CoapMessage Request, CoapResponse Response)
		{
			if (this.Object.Client.State == Lwm2mState.Bootstrap &&
				this.Object.Client.IsFromBootstrapServer(Request))
			{
				base.PUT(Request, Response);
			}
			else
				Response.RST(CoapCode.Unauthorized);
		}

	}
}
