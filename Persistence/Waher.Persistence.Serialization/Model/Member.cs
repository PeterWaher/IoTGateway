﻿using System;
using System.Reflection;
using System.Text;

namespace Waher.Persistence.Serialization.Model
{
	/// <summary>
	/// Class member.
	/// </summary>
	public abstract class Member
	{
		private readonly string name;
		private object defaultValue;
		private readonly Type memberType;
		private readonly TypeCode memberTypeCode;
		private readonly TypeInfo memberTypeInfo;
		private IObjectSerializer nestedSerializer = null;
		private readonly ulong fieldCode;
		private readonly uint memberFieldDataTypeCode;
		private readonly int decryptedMinLength;
		private readonly bool isNestedObject;
		private bool byReference = false;
		private readonly bool nullable = false;
		private bool isDefaultValueDefined = false;
		private readonly bool isEnum = false;
		private readonly bool hasFlags = false;
		private readonly bool encrypted = false;

		/// <summary>
		/// Class member.
		/// </summary>
		/// <param name="Name">Member name.</param>
		/// <param name="FieldCode">Field Code.</param>
		/// <param name="MemberType">Member type.</param>
		/// <param name="Encrypted">If the member is/should be encrypted.</param>
		/// <param name="DecryptedMinLength">Minimum length of the property, in bytes, before 
		/// encryption. If the clear text property is shorter than this, random bytes will be 
		/// appended to pad the property to this length, before encryption.</param>
		public Member(string Name, ulong FieldCode, Type MemberType, bool Encrypted, int DecryptedMinLength)
		{
			this.name = Name;
			this.fieldCode = FieldCode;
			this.memberType = MemberType;
			this.memberTypeInfo = MemberType.GetTypeInfo();
			this.encrypted = Encrypted;
			this.decryptedMinLength = DecryptedMinLength;

			if (this.memberTypeInfo.IsGenericType)
			{
				Type GT = this.memberType.GetGenericTypeDefinition();
				if (GT == typeof(Nullable<>))
				{
					this.nullable = true;
					this.memberType = this.memberType.GenericTypeArguments[0];
					this.memberTypeInfo = this.memberType.GetTypeInfo();
				}
			}

			this.memberFieldDataTypeCode = ObjectSerializer.GetFieldDataTypeCode(this.memberType);
			this.isNestedObject = this.memberFieldDataTypeCode == ObjectSerializer.TYPE_OBJECT;

			switch (this.memberFieldDataTypeCode)
			{
				case ObjectSerializer.TYPE_BOOLEAN:
					this.memberTypeCode = TypeCode.Boolean;
					break;

				case ObjectSerializer.TYPE_BYTE:
					this.memberTypeCode = TypeCode.Byte;
					break;

				case ObjectSerializer.TYPE_INT16:
				case ObjectSerializer.TYPE_VARINT16:
					this.memberTypeCode = TypeCode.Int16;
					break;

				case ObjectSerializer.TYPE_INT32:
				case ObjectSerializer.TYPE_VARINT32:
					this.memberTypeCode = TypeCode.Int32;
					if (this.memberTypeInfo.IsEnum)
					{
						this.isEnum = true;
						this.hasFlags = true;
					}
					break;

				case ObjectSerializer.TYPE_INT64:
				case ObjectSerializer.TYPE_VARINT64:
					this.memberTypeCode = TypeCode.Int64;
					break;

				case ObjectSerializer.TYPE_SBYTE:
					this.memberTypeCode = TypeCode.SByte;
					break;

				case ObjectSerializer.TYPE_UINT16:
				case ObjectSerializer.TYPE_VARUINT16:
					this.memberTypeCode = TypeCode.UInt16;
					break;

				case ObjectSerializer.TYPE_UINT32:
				case ObjectSerializer.TYPE_VARUINT32:
					this.memberTypeCode = TypeCode.UInt32;
					break;

				case ObjectSerializer.TYPE_UINT64:
				case ObjectSerializer.TYPE_VARUINT64:
					this.memberTypeCode = TypeCode.UInt64;
					break;

				case ObjectSerializer.TYPE_DECIMAL:
					this.memberTypeCode = TypeCode.Decimal;
					break;

				case ObjectSerializer.TYPE_DOUBLE:
					this.memberTypeCode = TypeCode.Double;
					break;

				case ObjectSerializer.TYPE_SINGLE:
					this.memberTypeCode = TypeCode.Single;
					break;

				case ObjectSerializer.TYPE_DATETIME:
					this.memberTypeCode = TypeCode.DateTime;
					break;

				case ObjectSerializer.TYPE_CHAR:
					this.memberTypeCode = TypeCode.Char;
					break;

				case ObjectSerializer.TYPE_STRING:
					this.memberTypeCode = TypeCode.String;
					break;

				case ObjectSerializer.TYPE_MIN:
				case ObjectSerializer.TYPE_MAX:
				case ObjectSerializer.TYPE_NULL:
					this.memberTypeCode = TypeCode.Empty;
					break;

				case ObjectSerializer.TYPE_ENUM:
					this.memberTypeCode = TypeCode.Object;
					this.isEnum = true;
					this.hasFlags = this.memberTypeInfo.IsDefined(typeof(FlagsAttribute), false);
					break;

				case ObjectSerializer.TYPE_OBJECT:
				case ObjectSerializer.TYPE_TIMESPAN:
				case ObjectSerializer.TYPE_BYTEARRAY:
				case ObjectSerializer.TYPE_GUID:
				case ObjectSerializer.TYPE_ARRAY:
				case ObjectSerializer.TYPE_DATETIMEOFFSET:
				case ObjectSerializer.TYPE_CI_STRING:
				default:
					this.memberTypeCode = TypeCode.Object;
					break;
			}
		}

		/// <summary>
		/// Member name.
		/// </summary>
		public string Name => this.name;

		/// <summary>
		/// Member type.
		/// </summary>
		public Type MemberType => this.memberType;

		/// <summary>
		/// Member type information.
		/// </summary>
		public TypeInfo MemberTypeInfo => this.memberTypeInfo;

		/// <summary>
		/// Member type code.
		/// </summary>
		public TypeCode MemberTypeCode => this.memberTypeCode;

		/// <summary>
		/// Member field data type code.
		/// </summary>
		public uint MemberFieldDataTypeCode => this.memberFieldDataTypeCode;

		/// <summary>
		/// Field Code.
		/// </summary>
		public ulong FieldCode => this.fieldCode;

		/// <summary>
		/// If the member type represents a nested object.
		/// </summary>
		public bool IsNestedObject => this.isNestedObject;

		/// <summary>
		/// If the member is/should be encrypted.
		/// </summary>
		public bool Encrypted => this.encrypted;

		/// <summary>Minimum length of the property, in bytes, before encryption. If the 
		/// clear text property is shorter than this, random bytes will be appended to pad 
		/// the property to this length, before encryption.
		/// </summary>
		public int DecryptedMinLength => this.decryptedMinLength;

		/// <summary>
		/// Default value.
		/// </summary>
		public object DefaultValue
		{
			get => this.defaultValue;
			internal set
			{
				this.defaultValue = value;
				this.isDefaultValueDefined = true;
			}
		}

		/// <summary>
		/// If a default value is defined for the member.
		/// </summary>
		public bool IsDefaultValueDefined => this.isDefaultValueDefined;

		/// <summary>
		/// If value is stored by reference.
		/// </summary>
		public bool ByReference
		{
			get => this.byReference;
			internal set => this.byReference = value;
		}

		/// <summary>
		/// If value is stored by reference.
		/// </summary>
		public bool Nullable => this.nullable;

		/// <summary>
		/// Nested serializer.
		/// </summary>
		public IObjectSerializer NestedSerializer
		{
			get => this.nestedSerializer;
			internal set => this.nestedSerializer = value;
		}

		/// <summary>
		/// If the member is an enumeration.
		/// </summary>
		public bool IsEnum => this.isEnum;

		/// <summary>
		/// If the member is an enumeration using flag values.
		/// </summary>
		public bool HasFlags => this.hasFlags;

		/// <summary>
		/// Gets the member value.
		/// </summary>
		/// <param name="Object">Object.</param>
		/// <returns>Member value.</returns>
		public abstract object Get(object Object);

		/// <summary>
		/// Sets the member value.
		/// </summary>
		/// <param name="Object">Object.</param>
		/// <param name="Value">Member value.</param>
		public abstract void Set(object Object, object Value);

		/// <summary>
		/// If the object member has the default value.
		/// </summary>
		/// <param name="Object">Object.</param>
		/// <returns>If its member has the default value.</returns>
		public bool HasDefaultValue(object Object)
		{
			if (!this.isDefaultValueDefined)
				return false;

			object Value = this.Get(Object);

			if ((this.defaultValue is null) ^ (Value is null))
				return false;

			if (this.defaultValue is null)
				return true;

			return this.defaultValue.Equals(Value);
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(this.name);
			sb.Append(", <");
			sb.Append(this.memberType.FullName);
			sb.Append(">");

			if (this.isDefaultValueDefined)
			{
				sb.Append(", default: ");

				if (this.defaultValue is null)
					sb.Append("null");
				else
					sb.Append(this.defaultValue.ToString());
			}

			if (this.byReference)
				sb.Append(", by reference");

			if (this.nullable)
				sb.Append(", nullable");

			return sb.ToString();
		}

	}
}
