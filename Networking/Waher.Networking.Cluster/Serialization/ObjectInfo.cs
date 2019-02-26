using System;
using System.Collections.Generic;
using System.Reflection;
using Waher.Networking.Cluster.Serialization.Properties;

namespace Waher.Networking.Cluster.Serialization
{
	/// <summary>
	/// Class containing information about objects of a specific type.
	/// </summary>
	internal class ObjectInfo
	{
		private Dictionary<string, PropertyReference> sorted = null;
		public PropertyReference[] Properties;
		public string TypeName;
		public Type Type;

		/// <summary>
		/// Serializes an object.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Object">Object to serialize.</param>
		public void Serialize(Serializer Output, object Object)
		{
			Output.WriteString(this.TypeName);

			if (!(this.Properties is null))
			{
				foreach (PropertyReference Property in this.Properties)
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
		public object Deserialize(Deserializer Input)
		{
			if (this.sorted is null)
			{
				Dictionary<string, PropertyReference> Sorted = new Dictionary<string, PropertyReference>();

				foreach (PropertyReference P in this.Properties)
					Sorted[P.Name] = P;

				this.sorted = Sorted;
			}

			object Result = Activator.CreateInstance(this.Type);
			string PropertyName = Input.ReadString();

			while (!string.IsNullOrEmpty(PropertyName))
			{
				if (!this.sorted.TryGetValue(PropertyName, out PropertyReference P))
					throw new KeyNotFoundException("Property Name not found: " + P.Info.DeclaringType.FullName + "." + P.Name);

				P.Info.SetValue(Result, P.Property.Deserialize(Input));

				PropertyName = Input.ReadString();
			}

			return Result;
		}

	}
}
