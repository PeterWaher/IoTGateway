using System;
using System.Collections.Generic;
using System.Reflection;
using Waher.Runtime.Inventory;

namespace Waher.Networking.Cluster.Serialization.Properties
{
	/// <summary>
	/// Array property
	/// </summary>
	public class ArrayProperty : Property
	{
		private readonly Type arrayType;
		private readonly Type elementType;

		/// <summary>
		/// Array property
		/// </summary>
		internal ArrayProperty(Type ArrayType, Type ElementType)
			: base()
		{
			this.arrayType = ArrayType;
			this.elementType = ElementType;
		}

		/// <summary>
		/// Property Type
		/// </summary>
		public override Type PropertyType => this.arrayType;

		/// <summary>
		/// Serializes the property value of an object.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Value">Value to serialize</param>
		public override void Serialize(Serializer Output, object Value)
		{
			Array A = (Array)Value;
			Type LastType = null;
			ObjectInfo LastInfo = null;

			if (A is null)
				Output.WriteVarUInt64(0);
			else
			{
				Output.WriteVarUInt64((ulong)A.Length + 1);

				foreach (object Element in A)
				{
					if (Element is null)
						Output.WriteString(null);
					else
					{
						Type T = Element.GetType();

						if (LastType is null || T != LastType)
						{
							LastType = T;
							LastInfo = ClusterEndpoint.GetObjectInfo(T);

							Output.WriteString(T.FullName);
						}
						else
							Output.WriteString(string.Empty);

						LastInfo.Serialize(Output, Element);
					}
				}
			}
		}

		/// <summary>
		/// Deserializes the property value
		/// </summary>
		/// <param name="Input">Binary representation.</param>
		/// <returns>Deserialized value.</returns>
		public override object Deserialize(Deserializer Input)
		{
			ulong Len = Input.ReadVarUInt64();
			if (Len == 0)
				return null;

			Len--;

			if (Len > int.MaxValue)
				throw new Exception("Array too long.");

			int i, c = (int)Len;

			Array Result = Array.CreateInstance(this.elementType, c);
			string TypeName;
			Type LastType = null;
			ObjectInfo LastInfo = null;

			for (i = 0; i < c; i++)
			{
				TypeName = Input.ReadString();

				if (TypeName is null)
					Result.SetValue(null, i);
				else
				{
					if (!string.IsNullOrEmpty(TypeName))
					{
						LastType = Types.GetType(TypeName);
						if (LastType is null)
							throw new KeyNotFoundException("Type name not recognized: " + TypeName);

						LastInfo = ClusterEndpoint.GetObjectInfo(LastType);
					}
					else if (LastInfo is null)
						throw new Exception("Invalid array serialization.");

					Result.SetValue(LastInfo.Deserialize(Input), i);
				}
			}

			return Result;
		}

	}
}
