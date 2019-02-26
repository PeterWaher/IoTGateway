using System;
using System.Collections.Generic;
using System.Reflection;

namespace Waher.Networking.Cluster.Serialization.Properties
{
	/// <summary>
	/// UInt16 property
	/// </summary>
	public class UInt16Property : Property
	{
		/// <summary>
		/// Property Type
		/// </summary>
		public override Type PropertyType => typeof(ushort);

		/// <summary>
		/// Serializes the property value of an object.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Value">Value to serialize</param>
		public override void Serialize(Serializer Output, object Value)
		{
			Output.WriteUInt16((ushort)Value);
		}

		/// <summary>
		/// Deserializes the property value
		/// </summary>
		/// <param name="Input">Binary representation.</param>
		/// <returns>Deserialized value.</returns>
		public override object Deserialize(Deserializer Input)
		{
			return Input.ReadUInt16();
		}
	}
}
