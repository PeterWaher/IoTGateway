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
			if (Value is null)
				Output.WriteVarUInt64(0);
			else
			{
				ObjectInfo Info;
				Type T = Value.GetType();

				if (T == this.objectType)
				{
					Info = this.info;
					Output.WriteString(string.Empty);
				}
				else
				{
					Info = ClusterEndpoint.GetObjectInfo(T);
					Output.WriteString(T.FullName);
				}

				Info.Serialize(Output, Value);
			}
		}

		/// <summary>
		/// Deserializes the property value
		/// </summary>
		/// <param name="Input">Binary representation.</param>
		/// <returns>Deserialized value.</returns>
		public override object Deserialize(Deserializer Input)
		{
			string TypeName = Input.ReadString();
			ObjectInfo Info;

			if (TypeName is null)
				return null;

			if (string.IsNullOrEmpty(TypeName))
				Info = this.info;
			else
			{
				Type T = Types.GetType(TypeName);
				if (T is null)
					throw new KeyNotFoundException("Type name not recognized: " + TypeName);

				Info = ClusterEndpoint.GetObjectInfo(T);
			}

			return Info.Deserialize(Input);
		}

	}
}
