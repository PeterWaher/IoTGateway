using System;
using System.Collections.Generic;
using System.Reflection;

namespace Waher.Networking.Cluster.Serialization.Properties
{
	/// <summary>
	/// DateTimeOffset property
	/// </summary>
	public class DateTimeOffsetProperty : Property
	{
		/// <summary>
		/// DateTimeOffset property
		/// </summary>
		/// <param name="PI">Property information</param>
		public DateTimeOffsetProperty(PropertyInfo PI)
			: base(PI)
		{
		}

		/// <summary>
		/// Property Type
		/// </summary>
		public override Type PropertyType => typeof(DateTimeOffset);

		/// <summary>
		/// Serializes the property value of an object.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Object">Object containing property</param>
		public override void Serialize(Serializer Output, object Object)
		{
			Output.WriteDateTimeOffset((DateTimeOffset)this.pi.GetValue(Object));
		}
	}
}
