using System;
using System.Collections.Generic;
using System.Reflection;

namespace Waher.Networking.Cluster.Serialization.Properties
{
	/// <summary>
	/// Int64 property
	/// </summary>
	public class Int64Property : Property
	{
		/// <summary>
		/// Int64 property
		/// </summary>
		/// <param name="PI">Property information</param>
		public Int64Property(PropertyInfo PI)
			: base(PI)
		{
		}

		/// <summary>
		/// Property Type
		/// </summary>
		public override Type PropertyType => typeof(long);

		/// <summary>
		/// Serializes the property value of an object.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Object">Object containing property</param>
		public override void Serialize(Serializer Output, object Object)
		{
			Output.WriteInt64((long)this.pi.GetValue(Object));
		}
	}
}
