using System;
using System.Collections.Generic;
using System.Reflection;

namespace Waher.Networking.Cluster.Serialization.Properties
{
	/// <summary>
	/// Decimal property
	/// </summary>
	public class DecimalProperty : Property
	{
		/// <summary>
		/// Decimal property
		/// </summary>
		/// <param name="PI">Property information</param>
		public DecimalProperty(PropertyInfo PI)
			: base(PI)
		{
		}

		/// <summary>
		/// Property Type
		/// </summary>
		public override Type PropertyType => typeof(decimal);

		/// <summary>
		/// Serializes the property value of an object.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Object">Object containing property</param>
		public override void Serialize(Serializer Output, object Object)
		{
			Output.WriteDecimal((decimal)this.pi.GetValue(Object));
		}
	}
}
