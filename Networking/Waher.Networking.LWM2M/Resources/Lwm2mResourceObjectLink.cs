using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.LWM2M.ContentFormats;

namespace Waher.Networking.LWM2M
{
	/// <summary>
	/// Class managing an LWM2M resource object link.
	/// </summary>
	public class Lwm2mResourceObjectLink : Lwm2mResource
	{
		private ushort? defaultRefId;
		private ushort? defaultRefInstanceId;
		private ushort? refId;
		private ushort? refInstanceId;

		/// <summary>
		/// Class managing an LWM2M resource object link.
		/// </summary>
		/// <param name="Name">Name of parameter. If null, parameter values will not be logged</param>
		/// <param name="Id">ID of object.</param>
		/// <param name="InstanceId">ID of object instance.</param>
		/// <param name="ResourceId">ID of resource.</param>
		/// <param name="CanWrite">If the resource allows servers to update the value using write commands.</param>
		/// <param name="ReferenceId">Referenced object id.</param>
		/// <param name="ReferenceInstanceId">Referenced object instance id.</param>
		public Lwm2mResourceObjectLink(string Name, ushort Id, ushort InstanceId, ushort ResourceId,
			bool CanWrite, ushort? ReferenceId, ushort? ReferenceInstanceId)
			: base(Name, Id, InstanceId, ResourceId, CanWrite)
		{
			this.defaultRefId = this.refId = ReferenceId;
			this.defaultRefInstanceId = this.refInstanceId = ReferenceInstanceId;
		}

		/// <summary>
		/// Value of resource.
		/// </summary>
		public override object Value
		{
			get
			{
				if (this.refId.HasValue && this.refInstanceId.HasValue)
					return new KeyValuePair<ushort, ushort>(this.refId.Value, this.refInstanceId.Value);
				else
					return null;
			}
		}

		/// <summary>
		/// Referenced object id.
		/// </summary>
		public ushort? ReferenceId
		{
			get { return this.refId; }
			set { this.refId = value; }
		}

		/// <summary>
		/// Referenced object instance id.
		/// </summary>
		public ushort? ReferenceInstanceId
		{
			get { return this.refInstanceId; }
			set { this.refInstanceId = value; }
		}

		/// <summary>
		/// Reads the value from a TLV record.
		/// </summary>
		/// <param name="Record">TLV record.</param>
		public override void Read(TlvRecord Record)
		{
			KeyValuePair<ushort, ushort> P = Record.AsObjectLink();
			this.refId = P.Key;
			this.refInstanceId = P.Value;
		}

		/// <summary>
		/// Reads the value from a TLV record.
		/// </summary>
		/// <param name="Output">Output.</param>
		public override void Write(ILwm2mWriter Output)
		{
			if (this.refId.HasValue && this.refInstanceId.HasValue)
				Output.Write(IdentifierType.Resource, this.ResourceId, this.refId.Value, this.refInstanceId.Value);
			else
				Output.Write(IdentifierType.Resource, this.ResourceId);
		}

		/// <summary>
		/// Resets the parameter to its default value.
		/// </summary>
		public override void Reset()
		{
			this.refId = this.defaultRefId;
			this.refInstanceId = this.defaultRefInstanceId;
		}
	}
}
