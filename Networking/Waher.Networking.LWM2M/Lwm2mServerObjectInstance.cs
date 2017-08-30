using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking.CoAP;
using Waher.Networking.LWM2M.ContentFormats;
using Waher.Persistence;
using Waher.Persistence.Attributes;

namespace Waher.Networking.LWM2M
{
	/// <summary>
	/// LWM2M Server object instance.
	/// </summary>
	public class Lwm2mServerObjectInstance : Lwm2mObjectInstance, ICoapDeleteMethod, ICoapPutMethod, ICoapPostMethod
	{
		/// <summary>
		/// Used as link to associate server Object Instance.
		/// </summary>
		private Lwm2mResourceInteger shortServerId;

		/// <summary>
		/// Specify the lifetime of the registration in seconds (see Section 5.3 Registration).
		/// </summary>
		private Lwm2mResourceInteger lifetimeSeconds;

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
		private Lwm2mResourceBoolean notificationStoring = null;

		/// <summary>
		/// This Resource defines the transport binding configured for the LwM2M Client. 
		/// 
		/// If the LwM2M Client supports the binding specified in this Resource, the LwM2M Client 
		/// MUST use that transport for the Current Binding Mode.
		/// </summary>
		private Lwm2mResourceString binding = null;

		/// <summary>
		/// If this Resource is executed the LwM2M Client MUST perform an “Update” operation 
		/// with this LwM2M Server using that transport for the Current Binding Mode. 
		/// </summary>
		private Lwm2mResourceCommand registrationUpdateTrigger = null;

		/// <summary>
		/// LWM2M Server object instance.
		/// </summary>
		public Lwm2mServerObjectInstance()
			: this(0)
		{
		}

		/// <summary>
		/// LWM2M Server object instance.
		/// </summary>
		/// <param name="InstanceId">ID of object instance.</param>
		public Lwm2mServerObjectInstance(ushort InstanceId)
			: base(1, InstanceId)
		{
			// E.2 LwM2M Object: LwM2M Server 
			// http://www.openmobilealliance.org/release/LightweightM2M/V1_0-20170208-A/OMA-TS-LightweightM2M-V1_0-20170208-A.pdf

			this.shortServerId = new Lwm2mResourceInteger("ShortServerId", 1, InstanceId, 0, false, null, false);
			this.lifetimeSeconds = new Lwm2mResourceInteger("LifetimeSeconds", 1, InstanceId, 1, true, null, false);
			this.notificationStoring = new Lwm2mResourceBoolean("NotificationStoring", 1, InstanceId, 6, true, null);
			this.binding = new Lwm2mResourceString("Binding", 1, InstanceId, 7, true, null);
			this.registrationUpdateTrigger = new Lwm2mResourceCommand("RegistrationUpdateTrigger", 1, InstanceId, 8);

			this.registrationUpdateTrigger.OnExecute += RegistrationUpdateTrigger_OnExecute;

			this.Add(this.shortServerId);
			this.Add(this.lifetimeSeconds);
			this.Add(new Lwm2mResourceNotSupported(1, InstanceId, 2));  // Default Minimum Period 
			this.Add(new Lwm2mResourceNotSupported(1, InstanceId, 3));  // Default Maximum Period 
			this.Add(new Lwm2mResourceNotSupported(1, InstanceId, 4));  // Disable 
			this.Add(new Lwm2mResourceNotSupported(1, InstanceId, 5));  // Disable Timeout 
			this.Add(this.notificationStoring);
			this.Add(this.binding);
			this.Add(this.registrationUpdateTrigger);
		}

		private void RegistrationUpdateTrigger_OnExecute(object sender, EventArgs e)
		{
			this.Object.Client.RegisterUpdate();
		}

		/// <summary>
		/// Used as link to associate server Object Instance.
		/// </summary>
		[DefaultValueNull]
		public ushort? ShortServerId
		{
			get { return (ushort?)this.shortServerId.IntegerValue; }
			set { this.shortServerId.IntegerValue = value; }
		}

		/// <summary>
		/// Specify the lifetime of the registration in seconds (see Section 5.3 Registration).
		/// </summary>
		[DefaultValueNull]
		public long? LifetimeSeconds
		{
			get { return this.lifetimeSeconds.IntegerValue; }
			set { this.lifetimeSeconds.IntegerValue = value; }
		}

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
		private bool? NotificationStoring
		{
			get { return this.notificationStoring.BooleanValue; }
			set { this.notificationStoring.BooleanValue = value; }
		}

		/// <summary>
		/// This Resource defines the transport binding configured for the LwM2M Client. 
		/// 
		/// If the LwM2M Client supports the binding specified in this Resource, the LwM2M Client 
		/// MUST use that transport for the Current Binding Mode.
		/// </summary>
		[DefaultValueNull]
		private string Binding
		{
			get { return this.binding.StringValue; }
			set { this.binding.StringValue = value; }
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

		internal bool Register(Lwm2mClient Client)
		{
			if (!this.shortServerId.IntegerValue.HasValue)
				return false;

			Lwm2mSecurityObjectInstance SecurityInfo = Client.GetSecurityInfo(
				(ushort)this.shortServerId.IntegerValue.Value);

			if (SecurityInfo == null)
				return false;

			Lwm2mServerReference Ref = SecurityInfo.GetServerReference(false);
			if (Ref == null)
				return false;

			Client.Register(this.lifetimeSeconds.IntegerValue.HasValue ? 
				(int)this.lifetimeSeconds.IntegerValue.Value : 86400, Ref);

			return true;
		}

	}
}
