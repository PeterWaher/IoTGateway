using System;
using System.Collections.Generic;
using System.Reflection;

namespace Waher.Networking.Cluster.Serialization.Properties
{
	/// <summary>
	/// UInt64 property
	/// </summary>
	public class UInt64Property : Property
	{
		/// <summary>
		/// UInt64 property
		/// </summary>
		/// <param name="PI">Property information</param>
		public UInt64Property(PropertyInfo PI)
			: base(PI)
		{
		}

		/// <summary>
		/// Property Type
		/// </summary>
		public override Type PropertyType => typeof(ulong);

		/// <summary>
		/// Serializes the property value of an object.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Object">Object containing property</param>
		public override void Serialize(Serializer Output, object Object)
		{
			Output.WriteUInt64((ulong)this.pi.GetValue(Object));
		}
	}
}
