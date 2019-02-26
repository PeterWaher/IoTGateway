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
		/// Property Type
		/// </summary>
		public override Type PropertyType => typeof(DateTimeOffset);

		/// <summary>
		/// Serializes the property value of an object.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Value">Value to serialize</param>
		public override void Serialize(Serializer Output, object Value)
		{
			Output.WriteDateTimeOffset((DateTimeOffset)Value);
		}

		/// <summary>
		/// Deserializes the property value
		/// </summary>
		/// <param name="Input">Binary representation.</param>
		/// <returns>Deserialized value.</returns>
		public override object Deserialize(Deserializer Input)
		{
			return Input.ReadDateTimeOffset();
		}
	}
}
