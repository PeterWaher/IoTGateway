using System;
using System.Collections.Generic;
using System.Reflection;

namespace Waher.Networking.Cluster.Serialization.Properties
{
	/// <summary>
	/// Int16 property
	/// </summary>
	public class Int16Property : Property
	{
		/// <summary>
		/// Int16 property
		/// </summary>
		/// <param name="PI">Property information</param>
		public Int16Property(PropertyInfo PI)
			: base(PI)
		{
		}

		/// <summary>
		/// Property Type
		/// </summary>
		public override Type PropertyType => typeof(short);

		/// <summary>
		/// Serializes the property value of an object.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Object">Object containing property</param>
		public override void Serialize(Serializer Output, object Object)
		{
			Output.WriteInt16((short)this.pi.GetValue(Object));
		}
	}
}
