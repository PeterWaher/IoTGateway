using System;
using System.Collections.Generic;
using System.Reflection;
using Waher.Runtime.Inventory;

namespace Waher.Networking.Cluster.Serialization.Properties
{
	/// <summary>
	/// Object property
	/// </summary>
	public class ObjectProperty : Property
	{
		private readonly Type objectType;
		private readonly ObjectInfo info;

		/// <summary>
		/// Object property
		/// </summary>
		internal ObjectProperty(Type ObjectType, ObjectInfo Info)
			: base()
		{
			this.objectType = ObjectType;
			this.info = Info;
		}

		/// <summary>
		/// Property Type
		/// </summary>
		public override Type PropertyType => this.objectType;

		/// <summary>
		/// Serializes the property value of an object.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Value">Value to serialize</param>
		public override void Serialize(Serializer Output, object Value)
		{
			this.info.Serialize(Output, Value);
		}

		/// <summary>
		/// Deserializes the property value
		/// </summary>
		/// <param name="Input">Binary representation.</param>
		/// <param name="ExpectedType">Expected Type</param>
		/// <returns>Deserialized value.</returns>
		public override object Deserialize(Deserializer Input, Type ExpectedType)
		{
			return this.info.Deserialize(Input, this.objectType);
		}

	}
}
