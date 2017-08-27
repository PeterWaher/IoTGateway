using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.CoAP.LWM2M
{
	/// <summary>
	/// LWM2M Security object.
	/// </summary>
	public class Lwm2mSecurityObject : Lwm2mObject, ICoapPutMethod
	{
		/// <summary>
		/// LWM2M Security object.
		/// </summary>
		public Lwm2mSecurityObject()
			: base(0)
		{
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
		/// Executes the GET method on the resource.
		/// </summary>
		/// <param name="Request">CoAP Request</param>
		/// <param name="Response">CoAP Response</param>
		/// <exception cref="CoapException">If an error occurred when processing the method.</exception>
		public void PUT(CoapMessage Request, CoapResponse Response)
		{
			if (this.Client.State == Lwm2mState.Bootstrap)
			{
				if (!string.IsNullOrEmpty(Request.SubPath) &&
					int.TryParse(Request.SubPath.Substring(1), out int SubId))
				{
					Lwm2mSecurityObjectInstance Instance = new Lwm2mSecurityObjectInstance(SubId);
					this.Add(Instance);
					this.Client.Endpoint.Register(Instance);

					Instance.PUT(Request, Response);
				}
				else
					Response.RST(CoapCode.BadRequest);
			}
			else
				Response.RST(CoapCode.Unauthorized);
		}
	}
}
