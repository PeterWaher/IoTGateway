using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.CoAP;
using Waher.Networking.LWM2M.ContentFormats;

namespace Waher.Networking.LWM2M
{
	/// <summary>
	/// Class managing a LWM2M resource that is not supported.
	/// </summary>
	public class Lwm2mResourceNotSupported : Lwm2mResource
	{
		/// <summary>
		/// Class managing a LWM2M resource that is not supported.
		/// </summary>
		/// <param name="Id">ID of object.</param>
		/// <param name="InstanceId">ID of object instance.</param>
		/// <param name="ResourceId">ID of resource.</param>
		public Lwm2mResourceNotSupported(ushort Id, ushort InstanceId, ushort ResourceId)
			: base(null, Id, InstanceId, ResourceId, true)
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
		/// Resets the parameter to its default value.
		/// </summary>
		public override void Reset()
		{
		}

		/// <summary>
		/// If resource should be published through /.well-known/core
		/// </summary>
		public override bool WellKnownCoRE => false;
	}
}
