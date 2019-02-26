using System;
using System.Collections.Generic;
using System.Reflection;

namespace Waher.Networking.Cluster.Serialization.Properties
{
	/// <summary>
	/// Nullable property
	/// </summary>
	public class NullableProperty : Property
	{
		private readonly Type nullableType;
		private readonly Type elementType;
		private readonly ConstructorInfo constructor;
		private readonly PropertyInfo valueProperty;
		private readonly IProperty element;

		/// <summary>
		/// Nullable property
		/// </summary>
		public NullableProperty(Type NullableType, Type ElementType, IProperty Element)
			: base()
		{
			this.nullableType = NullableType;
			this.elementType = ElementType;
			this.element = Element;
			this.constructor = null;

			foreach (ConstructorInfo CI in this.nullableType.GetTypeInfo().DeclaredConstructors)
			{
				ParameterInfo[] P = CI.GetParameters();
				if (P.Length == 1 && P[0].ParameterType == this.elementType)
				{
					this.constructor = CI;
					break;
				}
			}

			this.valueProperty = this.nullableType.GetRuntimeProperty("Value");
		}

		/// <summary>
		/// Property Type
		/// </summary>
		public override Type PropertyType => this.nullableType;

		/// <summary>
		/// Serializes the property value of an object.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Value">Value to serialize</param>
		public override void Serialize(Serializer Output, object Value)
		{
			if (Value is null)
				Output.WriteBoolean(false);
			else
			{
				Output.WriteBoolean(true);
				this.element.Serialize(Output, this.valueProperty.GetMethod.Invoke(Value, null));
			}
		}

		/// <summary>
		/// Deserializes the property value
		/// </summary>
		/// <param name="Input">Binary representation.</param>
		/// <returns>Deserialized value.</returns>
		public override object Deserialize(Deserializer Input)
		{
			if (Input.ReadBoolean())
				return this.element.Deserialize(Input);
			else
				return null;
		}
	}
}
