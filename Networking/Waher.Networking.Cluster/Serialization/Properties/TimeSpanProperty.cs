using System;
using System.Collections.Generic;
using System.Reflection;

namespace Waher.Networking.Cluster.Serialization.Properties
{
	/// <summary>
	/// TimeSpan property
	/// </summary>
	public class TimeSpanProperty : Property
	{
		/// <summary>
		/// TimeSpan property
		/// </summary>
		/// <param name="PI">Property information</param>
		public TimeSpanProperty(PropertyInfo PI)
			: base(PI)
		{
		}

		/// <summary>
		/// Property Type
		/// </summary>
		public override Type PropertyType => typeof(TimeSpan);

		/// <summary>
		/// Serializes the property value of an object.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Object">Object containing property</param>
		public override void Serialize(Serializer Output, object Object)
		{
			Output.WriteTimeSpan((TimeSpan)this.pi.GetValue(Object));
		}
	}
}
