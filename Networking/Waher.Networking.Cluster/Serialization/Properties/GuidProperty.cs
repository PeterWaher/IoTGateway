using System;
using System.Collections.Generic;
using System.Reflection;

namespace Waher.Networking.Cluster.Serialization.Properties
{
	/// <summary>
	/// Guid property
	/// </summary>
	public class GuidProperty : Property
	{
		/// <summary>
		/// Guid property
		/// </summary>
		/// <param name="PI">Property information</param>
		public GuidProperty(PropertyInfo PI)
			: base(PI)
		{
		}

		/// <summary>
		/// Property Type
		/// </summary>
		public override Type PropertyType => typeof(Guid);

		/// <summary>
		/// Serializes the property value of an object.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Object">Object containing property</param>
		public override void Serialize(Serializer Output, object Object)
		{
			Output.WriteGuid((Guid)this.pi.GetValue(Object));
		}
	}
}
