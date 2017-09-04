using System;
using System.Threading.Tasks;
using Waher.Networking.LWM2M.ContentFormats;

namespace Waher.Networking.LWM2M
{
	/// <summary>
	/// Class managing an LWM2M resource none value.
	/// </summary>
	public class Lwm2mResourceNone : Lwm2mResource
	{
		/// <summary>
		/// Class managing an LWM2M resource none value.
		/// </summary>
		/// <param name="Name">Name of parameter. If null, parameter values will not be logged</param>
		/// <param name="Id">ID of object.</param>
		/// <param name="InstanceId">ID of object instance.</param>
		/// <param name="ResourceId">ID of resource.</param>
		public Lwm2mResourceNone(string Name, ushort Id, ushort InstanceId, ushort ResourceId)
			: base(Name, Id, InstanceId, ResourceId, false, false)
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
		public override Task Read(TlvRecord Record)
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Reads the value from a TLV record.
		/// </summary>
		/// <param name="Output">Output.</param>
		public override void Write(ILwm2mWriter Output)
		{
			Output.Write(IdentifierType.Resource, this.ResourceId);
		}

		/// <summary>
		/// Resets the parameter to its default value.
		/// </summary>
		public override void Reset()
		{
		}
	}
}
