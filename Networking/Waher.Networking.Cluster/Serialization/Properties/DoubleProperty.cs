using System;
using System.Collections.Generic;
using System.Reflection;

namespace Waher.Networking.Cluster.Serialization.Properties
{
	/// <summary>
	/// Double precision floating-point property
	/// </summary>
	public class DoubleProperty : Property
	{
		/// <summary>
		/// Property Type
		/// </summary>
		public override Type PropertyType => typeof(double);

		/// <summary>
		/// Serializes the property value of an object.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Object">Object containing property</param>
		public override void Serialize(Serializer Output, object Value)
		{
			Output.WriteDouble((double)Value);
		}

		/// <summary>
		/// Deserializes the property value
		/// </summary>
		/// <param name="Input">Binary representation.</param>
		/// <returns>Deserialized value.</returns>
		public override object Deserialize(Deserializer Input)
		{
			return Input.ReadDouble();
		}
	}
}
