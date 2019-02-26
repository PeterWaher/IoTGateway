using System;
using System.Collections.Generic;
using System.Reflection;

namespace Waher.Networking.Cluster.Serialization.Properties
{
	/// <summary>
	/// Int32 property
	/// </summary>
	public class Int32Property : Property
	{
		/// <summary>
		/// Int32 property
		/// </summary>
		/// <param name="PI">Property information</param>
		public Int32Property(PropertyInfo PI)
			: base(PI)
		{
		}

		/// <summary>
		/// Property Type
		/// </summary>
		public override Type PropertyType => typeof(int);

		/// <summary>
		/// Serializes the property value of an object.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Object">Object containing property</param>
		public override void Serialize(Serializer Output, object Object)
		{
			Output.WriteInt32((int)this.pi.GetValue(Object));
		}
	}
}
