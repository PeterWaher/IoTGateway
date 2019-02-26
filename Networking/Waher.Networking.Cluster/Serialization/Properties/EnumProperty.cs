using System;
using System.Collections.Generic;
using System.Reflection;

namespace Waher.Networking.Cluster.Serialization.Properties
{
	/// <summary>
	/// Enumeration property
	/// </summary>
	public class EnumProperty : Property
	{
		private readonly Type enumType;
		private readonly TypeInfo enumTypeInfo;
		private readonly bool asInt;

		/// <summary>
		/// Enumeration property
		/// </summary>
		internal EnumProperty(Type EnumType)
			: base()
		{
			this.enumType = EnumType;
			this.enumTypeInfo = EnumType.GetTypeInfo();
			this.asInt = this.enumTypeInfo.IsDefined(typeof(FlagsAttribute), false);
		}

		/// <summary>
		/// Property Type
		/// </summary>
		public override Type PropertyType => this.enumType;

		/// <summary>
		/// Serializes the property value of an object.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Value">Value to serialize</param>
		public override void Serialize(Serializer Output, object Value)
		{
			if (this.asInt)
				Output.WriteInt32((int)Value);
			else
				Output.WriteString(Value.ToString());
		}

		/// <summary>
		/// Deserializes the property value
		/// </summary>
		/// <param name="Input">Binary representation.</param>
		/// <param name="ExpectedType">Expected Type</param>
		/// <returns>Deserialized value.</returns>
		public override object Deserialize(Deserializer Input, Type ExpectedType)
		{
			if (this.asInt)
				return Enum.ToObject(this.enumType, Input.ReadInt32());
			else
				return Enum.Parse(this.enumType, Input.ReadString());
		}
	}
}
