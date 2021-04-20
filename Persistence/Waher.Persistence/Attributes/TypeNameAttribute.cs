using System;

namespace Waher.Persistence.Attributes
{
	/// <summary>
	/// How the type name should be serialized.
	/// </summary>
	public enum TypeNameSerialization
	{
		/// <summary>
		/// No type name field will be serialized.
		/// </summary>
		None,

		/// <summary>
		/// Only the local name of the type will be serialized.
		/// </summary>
		LocalName,

		/// <summary>
		/// The full type name will be serialized.
		/// </summary>
		FullName
	}

	/// <summary>
	/// This attribute defines the name of the collection that will house objects of this type.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
	public class TypeNameAttribute : Attribute
	{
		private readonly TypeNameSerialization typeNameSerialization;
		private readonly string fieldName;

		/// <summary>
		/// This attribute defines the name of the collection that will house objects of this type.
		/// </summary>
		public TypeNameAttribute()
			: this(TypeNameSerialization.FullName, "_type")
		{
		}

		/// <summary>
		/// This attribute defines the name of the collection that will house objects of this type.
		/// </summary>
		/// <param name="TypeNameSerialization">How the type name should be serialized.</param>
		public TypeNameAttribute(TypeNameSerialization TypeNameSerialization)
			: this(TypeNameSerialization, "_type")
		{
		}

		/// <summary>
		/// This attribute defines the name of the collection that will house objects of this type.
		/// </summary>
		/// <param name="TypeNameSerialization">How the type name should be serialized.</param>
		/// <param name="FieldName">Type field name.</param>
		public TypeNameAttribute(TypeNameSerialization TypeNameSerialization, string FieldName)
			: base()
		{
			this.typeNameSerialization = TypeNameSerialization;
			this.fieldName = FieldName;
		}

		/// <summary>
		/// How the type name should be serialized.
		/// </summary>
		public TypeNameSerialization TypeNameSerialization
		{
			get { return this.typeNameSerialization; }
		}

		/// <summary>
		/// Type field name.
		/// </summary>
		public string FieldName
		{
			get { return this.fieldName; }
		}
	}
}
