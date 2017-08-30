using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Networking.CoAP;
using Waher.Persistence;
using Waher.Persistence.Filters;

namespace Waher.Networking.LWM2M
{
	/// <summary>
	/// LWM2M Server object.
	/// </summary>
    public class Lwm2mServerObject : Lwm2mObject, ICoapPutMethod, ICoapPostMethod
	{
		/// <summary>
		/// LWM2M Server object.
		/// </summary>
		public Lwm2mServerObject()
			: base(1)
		{
		}

		/// <summary>
		/// Loads any Bootstrap information.
		/// </summary>
		public override async Task LoadBootstrapInfo()
		{
			this.ClearInstances();

			foreach (Lwm2mServerObjectInstance Instance in await Database.Find<Lwm2mServerObjectInstance>(
				new FilterFieldEqualTo("Id", this.Id), "InstanceId"))
			{
				try
				{
					this.Add(Instance);
				}
				catch (Exception)
				{
					await Database.Delete(Instance);
				}
			}

			await base.LoadBootstrapInfo();
		}

		/// <summary>
		/// Deletes any Bootstrap information.
		/// </summary>
		public override async Task DeleteBootstrapInfo()
		{
			await base.DeleteBootstrapInfo();
			this.ClearInstances();
		}

		/// <summary>
		/// If the resource handles subpaths.
		/// </summary>
		public override bool HandlesSubPaths => true;

		/// <summary>
		/// If the PUT method is allowed.
		/// </summary>
		public bool AllowsPUT => true;

		/// <summary>
		/// Executes the PUT method on the resource.
		/// </summary>
		/// <param name="Request">CoAP Request</param>
		/// <param name="Response">CoAP Response</param>
		/// <exception cref="CoapException">If an error occurred when processing the method.</exception>
		public void PUT(CoapMessage Request, CoapResponse Response)
		{
			if (this.Client.State == Lwm2mState.Bootstrap &&
				this.Client.IsFromBootstrapServer(Request))
			{
				if (!string.IsNullOrEmpty(Request.SubPath) &&
					ushort.TryParse(Request.SubPath.Substring(1), out ushort InstanceId))
				{
					Lwm2mServerObjectInstance Instance = new Lwm2mServerObjectInstance(InstanceId);
					this.Add(Instance);
					this.Client.Endpoint.Register(Instance);
					Instance.AfterRegister(this.Client);

					Request.Path += Request.SubPath;
					Request.SubPath = string.Empty;

					Instance.PUT(Request, Response);
				}
				else
					Response.RST(CoapCode.BadRequest);
			}
			else
				Response.RST(CoapCode.Unauthorized);
		}

		/// <summary>
		/// If the POST method is allowed.
		/// </summary>
		public bool AllowsPOST => this.AllowsPUT;

		/// <summary>
		/// Executes the POST method on the resource.
		/// </summary>
		/// <param name="Request">CoAP Request</param>
		/// <param name="Response">CoAP Response</param>
		/// <exception cref="CoapException">If an error occurred when processing the method.</exception>
		public void POST(CoapMessage Request, CoapResponse Response)
		{
			this.PUT(Request, Response);
		}

	}
}
