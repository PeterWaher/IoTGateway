using System;
using System.Collections.Generic;
using System.Reflection;

namespace Waher.Networking.Cluster.Serialization.Properties
{
	/// <summary>
	/// UInt32 property
	/// </summary>
	public class UInt32Property : Property
	{
		/// <summary>
		/// UInt32 property
		/// </summary>
		/// <param name="PI">Property information</param>
		public UInt32Property(PropertyInfo PI)
			: base(PI)
		{
		}

		/// <summary>
		/// Property Type
		/// </summary>
		public override Type PropertyType => typeof(uint);

		/// <summary>
		/// Serializes the property value of an object.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Object">Object containing property</param>
		public override void Serialize(Serializer Output, object Object)
		{
			Output.WriteUInt32((uint)this.pi.GetValue(Object));
		}
	}
}
