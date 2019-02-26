using System;
using System.Collections.Generic;
using System.Reflection;

namespace Waher.Networking.Cluster.Serialization.Properties
{
	/// <summary>
	/// Int8 property
	/// </summary>
	public class Int8Property : Property
	{
		/// <summary>
		/// Int8 property
		/// </summary>
		/// <param name="PI">Property information</param>
		public Int8Property(PropertyInfo PI)
			: base(PI)
		{
		}

		/// <summary>
		/// Property Type
		/// </summary>
		public override Type PropertyType => typeof(sbyte);

		/// <summary>
		/// Serializes the property value of an object.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Object">Object containing property</param>
		public override void Serialize(Serializer Output, object Object)
		{
			Output.WriteInt8((sbyte)this.pi.GetValue(Object));
		}
	}
}
