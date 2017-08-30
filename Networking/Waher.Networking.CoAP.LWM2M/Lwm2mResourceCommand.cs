using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.CoAP.LWM2M.ContentFormats;

namespace Waher.Networking.CoAP.LWM2M
{
	/// <summary>
	/// Class managing an LWM2M resource command.
	/// </summary>
	public class Lwm2mResourceCommand : Lwm2mResource
	{
		/// <summary>
		/// Class managing an LWM2M resource command.
		/// </summary>
		/// <param name="Id">ID of object.</param>
		/// <param name="InstanceId">ID of object instance.</param>
		/// <param name="ResourceId">ID of resource.</param>
		public Lwm2mResourceCommand(ushort Id, ushort InstanceId, ushort ResourceId)
			: base(Id, InstanceId, ResourceId)
		{
		}

		/// <summary>
		/// Value of resource.
		/// </summary>
		public override object Value => null;

		/// <summary>
		/// Reads the value from a TLV record.
		/// </summary>
		/// <param name="Record">TLV record.</param>
		public override void Read(TlvRecord Record)
		{
		}

		/// <summary>
		/// Reads the value from a TLV record.
		/// </summary>
		/// <param name="Output">Output.</param>
		public override void Write(ILwm2mWriter Output)
		{
		}

		/// <summary>
		/// Executes the GET method on the resource.
		/// </summary>
		/// <param name="Request">CoAP Request</param>
		/// <param name="Response">CoAP Response</param>
		/// <exception cref="CoapException">If an error occurred when processing the method.</exception>
		public override void GET(CoapMessage Request, CoapResponse Response)
		{
			Response.Respond(CoapCode.MethodNotAllowed);
		}
	}
}
