using System;
using System.Collections.Generic;
using System.Reflection;

namespace Waher.Networking.Cluster.Serialization.Properties
{
	/// <summary>
	/// Single precision floating-point property
	/// </summary>
	public class SingleProperty : Property
	{
		/// <summary>
		/// Single precision floating-point property
		/// </summary>
		/// <param name="PI">Property information</param>
		public SingleProperty(PropertyInfo PI)
			: base(PI)
		{
		}

		/// <summary>
		/// Property Type
		/// </summary>
		public override Type PropertyType => typeof(float);

		/// <summary>
		/// Serializes the property value of an object.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Object">Object containing property</param>
		public override void Serialize(Serializer Output, object Object)
		{
			Output.WriteSingle((float)this.pi.GetValue(Object));
		}
	}
}
