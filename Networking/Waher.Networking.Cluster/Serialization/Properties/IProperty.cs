using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Waher.Networking.Cluster.Serialization.Properties
{
	/// <summary>
	/// Abstract base class for properties
	/// </summary>
	public interface IProperty
	{
		/// <summary>
		/// Property Type
		/// </summary>
		Type PropertyType
		{
			get;
		}

		/// <summary>
		/// Serializes the property value of an object.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Value">Value to serialize</param>
		void Serialize(Serializer Output, object Value);

		/// <summary>
		/// Deserializes the property value
		/// </summary>
		/// <param name="Input">Binary representation.</param>
		/// <returns>Deserialized value.</returns>
		object Deserialize(Deserializer Input);
	}
}
