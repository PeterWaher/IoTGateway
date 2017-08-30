using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.CoAP.LWM2M.ContentFormats;

namespace Waher.Networking.CoAP.LWM2M
{
	/// <summary>
	/// Class managing an LWM2M resource integer.
	/// </summary>
	public class Lwm2mResourceInteger : Lwm2mResource
	{
		private long? value;
		bool signed;

		/// <summary>
		/// Class managing an LWM2M resource integer.
		/// </summary>
		/// <param name="Id">ID of object.</param>
		/// <param name="InstanceId">ID of object instance.</param>
		/// <param name="ResourceId">ID of resource.</param>
		/// <param name="Value">Value of resource.</param>
		/// <param name="Signed">If integers on this resource are signed (true), or unsigned (false).</param>
		public Lwm2mResourceInteger(ushort Id, ushort InstanceId, ushort ResourceId, long? Value, bool Signed)
			: base(Id, InstanceId, ResourceId)
		{
			this.value = Value;
			this.signed = Signed;
		}

		/// <summary>
		/// Value of resource.
		/// </summary>
		public override object Value => this.value;

		/// <summary>
		/// Resource value.
		/// </summary>
		public long? IntegerValue
		{
			get { return this.value; }
			set { this.value = value; }
		}

		/// <summary>
		/// Reads the value from a TLV record.
		/// </summary>
		/// <param name="Record">TLV record.</param>
		public override void Read(TlvRecord Record)
		{
			this.value = Record.AsSignedInteger();
		}

		/// <summary>
		/// Reads the value from a TLV record.
		/// </summary>
		/// <param name="Output">Output.</param>
		public override void Write(ILwm2mWriter Output)
		{
			if (this.value.HasValue)
			{
				long l = this.value.Value;

				if (this.signed)
				{
					if (l >= sbyte.MinValue && l <= sbyte.MaxValue)
						Output.Write(IdentifierType.Resource, this.ResourceId, (sbyte)l);
					else if (l >= short.MinValue && l <= short.MaxValue)
						Output.Write(IdentifierType.Resource, this.ResourceId, (short)l);
					else if (l >= int.MinValue && l <= int.MaxValue)
						Output.Write(IdentifierType.Resource, this.ResourceId, (int)l);
					else
						Output.Write(IdentifierType.Resource, this.ResourceId, l);
				}
				else
				{
					if (l >= byte.MinValue && l <= byte.MaxValue)
						Output.Write(IdentifierType.Resource, this.ResourceId, (sbyte)l);
					else if (l >= ushort.MinValue && l <= ushort.MaxValue)
						Output.Write(IdentifierType.Resource, this.ResourceId, (short)l);
					else if (l >= uint.MinValue && l <= uint.MaxValue)
						Output.Write(IdentifierType.Resource, this.ResourceId, (int)l);
					else
						Output.Write(IdentifierType.Resource, this.ResourceId, l);
				}
			}
			else
				Output.Write(IdentifierType.Resource, this.ResourceId);
		}
	}
}
