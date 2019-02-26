using System;
using System.Collections.Generic;
using Waher.Runtime.Inventory;

namespace Waher.Networking.Cluster.Serialization
{
	/// <summary>
	/// Class containing information about objects of a specific type.
	/// </summary>
	internal class ObjectInfo
	{
		private Dictionary<string, PropertyReference> sorted = null;
		public PropertyReference[] Properties;
		public Type Type;

		/// <summary>
		/// Serializes an object.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Object">Object to serialize.</param>
		public void Serialize(Serializer Output, object Object)
		{
			if (Object is null)
				Output.WriteByte(0);
			else
			{
				Type T = Object.GetType();
				ObjectInfo Info = this;

				if (T == this.Type)
					Output.WriteString(string.Empty);
				else
				{
					Output.WriteString(T.FullName);
					Info = ClusterEndpoint.GetObjectInfo(T);
				}

				foreach (PropertyReference Property in Info.Properties)
				{
					Output.WriteString(Property.Name);
					object Value = Property.Info.GetValue(Object);
					Property.Property.Serialize(Output, Value);
				}

				Output.WriteByte(0);
			}
		}

		/// <summary>
		/// Deserializes an object
		/// </summary>
		/// <param name="Input">Binary input</param>
		/// <returns>Deserialized object.</returns>
		/// <exception cref="KeyNotFoundException">If the corresponding type, or any of the embedded properties, could not be found.</exception>
		public object Deserialize(Deserializer Input, Type ExpectedType)
		{
			string TypeName = Input.ReadString();
			ObjectInfo Info;

			if (TypeName is null)
				return null;
			else if (string.IsNullOrEmpty(TypeName))
				Info = this;
			else
			{
				Type T = Types.GetType(TypeName);
				if (T is null)
					throw new KeyNotFoundException("Type name not recognized: " + TypeName);

				Info = ClusterEndpoint.GetObjectInfo(T);
			}

			if (Info.sorted is null)
			{
				Dictionary<string, PropertyReference> Sorted = new Dictionary<string, PropertyReference>();

				foreach (PropertyReference P in Info.Properties)
					Sorted[P.Name] = P;

				Info.sorted = Sorted;
			}

			object Result = Activator.CreateInstance(Info.Type);
			string PropertyName = Input.ReadString();

			while (!string.IsNullOrEmpty(PropertyName))
			{
				if (!Info.sorted.TryGetValue(PropertyName, out PropertyReference P))
					throw new KeyNotFoundException("Property Name not found: " + Info.Type.FullName + "." + PropertyName);

				P.Info.SetValue(Result, P.Property.Deserialize(Input, P.Property.PropertyType));

				PropertyName = Input.ReadString();
			}

			return Result;
		}

	}
}
