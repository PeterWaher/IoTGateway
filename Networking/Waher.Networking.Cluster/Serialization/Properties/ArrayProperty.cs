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
		private readonly IProperty elementProperty;

		/// <summary>
		/// Array property
		/// </summary>
		internal ArrayProperty(Type ArrayType, Type ElementType, IProperty ElementProperty)
			: base()
		{
			this.arrayType = ArrayType;
			this.elementType = ElementType;
			this.elementProperty = ElementProperty;
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

			if (A is null)
				Output.WriteVarUInt64(0);
			else
			{
				Output.WriteVarUInt64((ulong)A.Length + 1);

				foreach (object Element in A)
					this.elementProperty.Serialize(Output, Element);
			}
		}

		/// <summary>
		/// Deserializes the property value
		/// </summary>
		/// <param name="Input">Binary representation.</param>
		/// <param name="ExpectedType">Expected Type</param>
		/// <returns>Deserialized value.</returns>
		public override object Deserialize(Deserializer Input, Type ExpectedType)
		{
			ulong Len = Input.ReadVarUInt64();
			if (Len == 0)
				return null;

			Len--;

			if (Len > int.MaxValue)
				throw new Exception("Array too long.");

			int i, c = (int)Len;

			Array Result = Array.CreateInstance(this.elementType, c);

			for (i = 0; i < c; i++)
				Result.SetValue(this.elementProperty.Deserialize(Input,this.elementType), i);

			return Result;
		}

	}
}
