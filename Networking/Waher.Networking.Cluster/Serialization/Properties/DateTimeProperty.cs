using System;
using System.Collections.Generic;
using System.Reflection;

namespace Waher.Networking.Cluster.Serialization.Properties
{
	/// <summary>
	/// DateTime property
	/// </summary>
	public class DateTimeProperty : Property
	{
		/// <summary>
		/// DateTime property
		/// </summary>
		/// <param name="PI">Property information</param>
		public DateTimeProperty(PropertyInfo PI)
			: base(PI)
		{
		}

		/// <summary>
		/// Property Type
		/// </summary>
		public override Type PropertyType => typeof(DateTime);

		/// <summary>
		/// Serializes the property value of an object.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Object">Object containing property</param>
		public override void Serialize(Serializer Output, object Object)
		{
			Output.WriteDateTime((DateTime)this.pi.GetValue(Object));
		}
	}
}
