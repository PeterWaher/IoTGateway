using System;
using System.Collections.Generic;
using System.Reflection;

namespace Waher.Networking.Cluster.Serialization.Properties
{
	/// <summary>
	/// UInt8 property
	/// </summary>
	public class UInt8Property : Property
	{
		/// <summary>
		/// UInt8 property
		/// </summary>
		/// <param name="PI">Property information</param>
		public UInt8Property(PropertyInfo PI)
			: base(PI)
		{
		}

		/// <summary>
		/// Property Type
		/// </summary>
		public override Type PropertyType => typeof(byte);

		/// <summary>
		/// Serializes the property value of an object.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Object">Object containing property</param>
		public override void Serialize(Serializer Output, object Object)
		{
			Output.WriteUInt8((byte)this.pi.GetValue(Object));
		}
	}
}
