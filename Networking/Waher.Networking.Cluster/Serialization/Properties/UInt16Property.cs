using System;
using System.Collections.Generic;
using System.Reflection;

namespace Waher.Networking.Cluster.Serialization.Properties
{
	/// <summary>
	/// UInt16 property
	/// </summary>
	public class UInt16Property : Property
	{
		/// <summary>
		/// UInt16 property
		/// </summary>
		/// <param name="PI">Property information</param>
		public UInt16Property(PropertyInfo PI)
			: base(PI)
		{
		}

		/// <summary>
		/// Property Type
		/// </summary>
		public override Type PropertyType => typeof(ushort);

		/// <summary>
		/// Serializes the property value of an object.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Object">Object containing property</param>
		public override void Serialize(Serializer Output, object Object)
		{
			Output.WriteUInt16((ushort)this.pi.GetValue(Object));
		}
	}
}
