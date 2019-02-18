using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
#if NETSTANDARD1_5
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
#endif
using Waher.Persistence.Files.Serialization.Model;
using Waher.Persistence.Serialization;
using Waher.Persistence.Attributes;
using Waher.Runtime.Inventory;
using Waher.Runtime.Threading;

namespace Waher.Persistence.Files.Serialization
{
	/// <summary>
	/// Serializes a class, taking into account attributes defined in <see cref="Waher.Persistence.Attributes"/>.
	/// </summary>
	public class ObjectSerializer : IObjectSerializer
	{
		/// <summary>
		/// Represents a <see cref="Boolean"/>
		/// </summary>
		public const uint TYPE_BOOLEAN = 0;

		/// <summary>
		/// Represents a <see cref="Byte"/>
		/// </summary>
		public const uint TYPE_BYTE = 1;

		/// <summary>
		/// Represents a <see cref="Int16"/>
		/// </summary>
		public const uint TYPE_INT16 = 2;

		/// <summary>
		/// Represents a <see cref="Int32"/>
		/// </summary>
		public const uint TYPE_INT32 = 3;

		/// <summary>
		/// Represents a <see cref="Int64"/>
		/// </summary>
		public const uint TYPE_INT64 = 4;

		/// <summary>
		/// Represents a <see cref="SByte"/>
		/// </summary>
		public const uint TYPE_SBYTE = 5;

		/// <summary>
		/// Represents a <see cref="UInt16"/>
		/// </summary>
		public const uint TYPE_UINT16 = 6;

		/// <summary>
		/// Represents a <see cref="UInt32"/>
		/// </summary>
		public const uint TYPE_UINT32 = 7;

		/// <summary>
		/// Represents a <see cref="UInt64"/>
		/// </summary>
		public const uint TYPE_UINT64 = 8;

		/// <summary>
		/// Represents a <see cref="Decimal"/>
		/// </summary>
		public const uint TYPE_DECIMAL = 9;

		/// <summary>
		/// Represents a <see cref="Double"/>
		/// </summary>
		public const uint TYPE_DOUBLE = 10;

		/// <summary>
		/// Represents a <see cref="Single"/>
		/// </summary>
		public const uint TYPE_SINGLE = 11;

		/// <summary>
		/// Represents a <see cref="DateTime"/>
		/// </summary>
		public const uint TYPE_DATETIME = 12;

		/// <summary>
		/// Represents a <see cref="TimeSpan"/>
		/// </summary>
		public const uint TYPE_TIMESPAN = 13;

		/// <summary>
		/// Represents a <see cref="Char"/>
		/// </summary>
		public const uint TYPE_CHAR = 14;

		/// <summary>
		/// Represents a <see cref="String"/>
		/// </summary>
		public const uint TYPE_STRING = 15;

		/// <summary>
		/// Represents an enumerated value.
		/// </summary>
		public const uint TYPE_ENUM = 16;

		/// <summary>
		/// Represents a byte array.
		/// </summary>
		public const uint TYPE_BYTEARRAY = 17;

		/// <summary>
		/// Represents a <see cref="Guid"/>
		/// </summary>
		public const uint TYPE_GUID = 18;

		/// <summary>
		/// Represents a <see cref="DateTimeOffset"/>
		/// </summary>
		public const uint TYPE_DATETIMEOFFSET = 19;

		/// <summary>
		/// Represents a <see cref="CaseInsensitiveString"/>
		/// </summary>
		public const uint TYPE_CI_STRING = 20;

		/// <summary>
		/// Represents the smallest possible value for the field type being searched or filtered.
		/// </summary>
		public const uint TYPE_MIN = 27;

		/// <summary>
		/// Represents the largest possible value for the field type being searched or filtered.
		/// </summary>
		public const uint TYPE_MAX = 28;

		/// <summary>
		/// Represents a null value.
		/// </summary>
		public const uint TYPE_NULL = 29;

		/// <summary>
		/// Represents an arary.
		/// </summary>
		public const uint TYPE_ARRAY = 30;

		/// <summary>
		/// Represents an object.
		/// </summary>
		public const uint TYPE_OBJECT = 31;

		/// <summary>
		/// Files provider reference.
		/// </summary>
		protected readonly FilesProvider provider;

		private readonly Type type;
		private readonly System.Reflection.TypeInfo typeInfo;
		private readonly string collectionName;
		private readonly string typeFieldName;
		private readonly string[][] indices;
		private readonly TypeNameSerialization typeNameSerialization;
#if NETSTANDARD1_5
		private readonly FieldInfo objectIdFieldInfo = null;
		private readonly PropertyInfo objectIdPropertyInfo = null;
		private readonly Dictionary<string, object> defaultValues = new Dictionary<string, object>();
		private readonly Dictionary<string, Type> memberTypes = new Dictionary<string, Type>();
		private readonly Dictionary<string, MemberInfo> members = new Dictionary<string, MemberInfo>();
		private readonly IObjectSerializer customSerializer = null;
		private readonly bool compiled;
#endif
		private readonly Member objectIdMember = null;
		private readonly Dictionary<string, Member> membersByName = new Dictionary<string, Member>();
		private readonly Dictionary<ulong, Member> membersByFieldCode = new Dictionary<ulong, Member>();
		private readonly LinkedList<Member> membersOrdered = new LinkedList<Member>();
		private readonly bool isNullable;
		private readonly bool debug;
		private bool indicesCreated = false;

		internal ObjectSerializer(Type Type, FilesProvider Provider)
		{
			this.type = Type;
			this.typeInfo = Type.GetTypeInfo();
			this.provider = Provider;
			this.debug = false;
#if NETSTANDARD1_5
			this.compiled = false;
#endif
			this.isNullable = true;
			this.collectionName = null;
			this.typeNameSerialization = TypeNameSerialization.FullName;
			this.indices = new string[0][];
		}

#if NETSTANDARD1_5
		/// <summary>
		/// Serializes a class, taking into account attributes defined in <see cref="Waher.Persistence.Attributes"/>.
		/// </summary>
		/// <param name="Type">Type to serialize.</param>
		/// <param name="Provider">Database provider.</param>
		/// <param name="Debug">If debug information is to be included for generated code.</param>
		/// <param name="Compiled">If object serializers should be compiled or not.</param>
		public ObjectSerializer(Type Type, FilesProvider Provider, bool Debug, bool Compiled)
#else
		/// <summary>
		/// Serializes a class, taking into account attributes defined in <see cref="Waher.Persistence.Attributes"/>.
		/// </summary>
		/// <param name="Type">Type to serialize.</param>
		/// <param name="Provider">Database provider.</param>
		/// <param name="Debug">If debug information is to be included for generated code.</param>
		public ObjectSerializer(Type Type, FilesProvider Provider, bool Debug)
#endif
		{
			string TypeName = Type.Name;

			this.type = Type;
			this.typeInfo = Type.GetTypeInfo();
			this.provider = Provider;
			this.debug = Debug;

#if NETSTANDARD1_5
			this.compiled = Compiled;
#endif

			if (this.type == typeof(bool) ||
				this.type == typeof(byte) ||
				this.type == typeof(char) ||
				this.type == typeof(DateTime) ||
				this.type == typeof(decimal) ||
				this.type == typeof(double) ||
				this.type == typeof(short) ||
				this.type == typeof(int) ||
				this.type == typeof(long) ||
				this.type == typeof(sbyte) ||
				this.type == typeof(float) ||
				this.type == typeof(ushort) ||
				this.type == typeof(uint) ||
				this.type == typeof(ulong))
			{
				this.isNullable = false;
			}
			else if (this.type == typeof(void) || this.type == typeof(string))
				this.isNullable = true;
			else
				this.isNullable = !this.typeInfo.IsValueType;

			CollectionNameAttribute CollectionNameAttribute = this.typeInfo.GetCustomAttribute<CollectionNameAttribute>(true);
			if (CollectionNameAttribute is null)
				this.collectionName = null;
			else
				this.collectionName = CollectionNameAttribute.Name;

			TypeNameAttribute TypeNameAttribute = this.typeInfo.GetCustomAttribute<TypeNameAttribute>(true);
			if (TypeNameAttribute is null)
			{
				this.typeFieldName = "_type";
				this.typeNameSerialization = TypeNameSerialization.FullName;
			}
			else
			{
				this.typeFieldName = TypeNameAttribute.FieldName;
				this.typeNameSerialization = TypeNameAttribute.TypeNameSerialization;
			}

			if (this.typeInfo.IsAbstract && this.typeNameSerialization == TypeNameSerialization.None)
				throw new Exception("Serializers for abstract classes require type names to be serialized.");

			List<string[]> Indices = new List<string[]>();

			foreach (IndexAttribute IndexAttribute in this.typeInfo.GetCustomAttributes<IndexAttribute>(true))
				Indices.Add(IndexAttribute.FieldNames);

			this.indices = Indices.ToArray();

#if NETSTANDARD1_5
			if (this.compiled)
			{
				StringBuilder CSharp = new StringBuilder();
				IEnumerable<MemberInfo> Members = GetMembers(this.typeInfo);
				Type MemberType;
				System.Reflection.TypeInfo MemberTypeInfo;
				FieldInfo FI;
				PropertyInfo PI;
				MethodInfo MI;
				object DefaultValue;
				//string ShortName;
				string Indent, Indent2;
				int NrDefault = 0;
				bool HasDefaultValue;
				bool Ignore;
				bool ObjectIdField;
				bool ByReference;
				bool Nullable;
				bool HasObjectId = false;

				CSharp.AppendLine("using System;");
				CSharp.AppendLine("using System.Collections.Generic;");
				CSharp.AppendLine("using System.Reflection;");
				CSharp.AppendLine("using System.Text;");
				CSharp.AppendLine("using System.Threading.Tasks;");
				CSharp.AppendLine("using Waher.Persistence;");
				CSharp.AppendLine("using Waher.Persistence.Files;");
				CSharp.AppendLine("using Waher.Persistence.Files.Serialization;");
				CSharp.AppendLine("using Waher.Persistence.Filters;");
				CSharp.AppendLine("using Waher.Persistence.Serialization;");
				CSharp.AppendLine("using Waher.Runtime.Inventory;");
				CSharp.AppendLine();
				CSharp.AppendLine("namespace " + Type.Namespace + ".Binary");
				CSharp.AppendLine("{");
				CSharp.AppendLine("\tpublic class BinarySerializer" + TypeName + this.provider.Id + " : GeneratedObjectSerializerBase");
				CSharp.AppendLine("\t{");
				CSharp.AppendLine("\t\tprivate FilesProvider provider;");

				foreach (MemberInfo Member in Members)
				{
					if (!((FI = Member as FieldInfo) is null))
					{
						if (!FI.IsPublic || FI.IsStatic)
							continue;

						PI = null;
						MemberType = FI.FieldType;
					}
					else if (!((PI = Member as PropertyInfo) is null))
					{
						if ((MI = PI.GetMethod) is null || !MI.IsPublic || MI.IsStatic)
							continue;

						if ((MI = PI.SetMethod) is null || !MI.IsPublic || MI.IsStatic)
							continue;

						if (PI.GetIndexParameters().Length > 0)
							continue;

						MemberType = PI.PropertyType;
					}
					else
						continue;

					this.memberTypes[Member.Name] = MemberType;
					this.members[Member.Name] = Member;

					Ignore = false;
					//ShortName = null;
					ByReference = false;
					Nullable = false;

					MemberTypeInfo = MemberType.GetTypeInfo();
					if (MemberTypeInfo.IsGenericType)
					{
						Type GT = MemberType.GetGenericTypeDefinition();
						if (GT == typeof(Nullable<>))
						{
							Nullable = true;
							MemberType = MemberType.GenericTypeArguments[0];
							MemberTypeInfo = MemberType.GetTypeInfo();
						}
					}

					foreach (Attribute Attr in Member.GetCustomAttributes(true))
					{
						if (Attr is IgnoreMemberAttribute)
						{
							Ignore = true;
							break;
						}

						if (Attr is DefaultValueAttribute DefaultValueAttribute)
						{
							DefaultValue = DefaultValueAttribute.Value;
							NrDefault++;

							this.defaultValues[Member.Name] = DefaultValue;

							CSharp.Append("\t\tprivate static readonly ");

							if (DefaultValue is null)
								CSharp.Append("object");
							else
								CSharp.Append(MemberType.FullName);

							CSharp.Append(" default");
							CSharp.Append(Member.Name);
							CSharp.Append(" = ");

							if (DefaultValue is null)
								CSharp.Append("null");
							else
							{
								if (DefaultValue.GetType() != MemberType)
								{
									CSharp.Append('(');
									CSharp.Append(MemberType.FullName);
									CSharp.Append(')');
								}

								if (DefaultValue is string s2)
								{
									if (string.IsNullOrEmpty(s2))
									{
										if (MemberType == typeof(CaseInsensitiveString))
											CSharp.Append("CaseInsensitiveString.Empty");
										else
											CSharp.Append("string.Empty");
									}
									else
									{
										CSharp.Append("\"");
										CSharp.Append(Escape(DefaultValue.ToString()));
										CSharp.Append("\"");
									}
								}
								else if (DefaultValue is DateTime TP)
								{
									if (TP == DateTime.MinValue)
										CSharp.Append("DateTime.MinValue");
									else if (TP == DateTime.MaxValue)
										CSharp.Append("DateTime.MaxValue");
									else
									{
										CSharp.Append("new DateTime(");
										CSharp.Append(TP.Ticks.ToString());
										CSharp.Append(", DateTimeKind.");
										CSharp.Append(TP.Kind.ToString());
										CSharp.Append(")");
									}
								}
								else if (DefaultValue is TimeSpan TS)
								{
									if (TS == TimeSpan.MinValue)
										CSharp.Append("TimeSpan.MinValue");
									else if (TS == TimeSpan.MaxValue)
										CSharp.Append("TimeSpan.MaxValue");
									else
									{
										CSharp.Append("new TimeSpan(");
										CSharp.Append(TS.Ticks.ToString());
										CSharp.Append(")");
									}
								}
								else if (DefaultValue is Guid Guid)
								{
									if (Guid.Equals(Guid.Empty))
										CSharp.Append("Guid.Empty");
									else
									{
										CSharp.Append("new Guid(\"");
										CSharp.Append(Guid.ToString());
										CSharp.Append("\")");
									}
								}
								else if (DefaultValue is Enum e)
								{
									Type DefaultValueType = DefaultValue.GetType();

									if (DefaultValueType.GetTypeInfo().IsDefined(typeof(FlagsAttribute), false))
									{
										bool First = true;

										foreach (object Value in Enum.GetValues(DefaultValueType))
										{
											if (!e.HasFlag((Enum)Value))
												continue;

											if (First)
												First = false;
											else
												CSharp.Append(" | ");

											CSharp.Append(DefaultValueType.FullName);
											CSharp.Append('.');
											CSharp.Append(Value.ToString());
										}

										if (First)
											CSharp.Append('0');
									}
									else
									{
										CSharp.Append(DefaultValue.GetType().FullName);
										CSharp.Append('.');
										CSharp.Append(DefaultValue.ToString());
									}
								}
								else if (DefaultValue is bool b)
								{
									if (b)
										CSharp.Append("true");
									else
										CSharp.Append("false");
								}
								else if (DefaultValue is long)
								{
									CSharp.Append(DefaultValue.ToString());
									CSharp.Append("L");
								}
								else if (DefaultValue is char ch)
								{
									CSharp.Append('\'');

									if (ch == '\'')
										CSharp.Append('\\');

									CSharp.Append(ch);
									CSharp.Append('\'');
								}
								else
									CSharp.Append(DefaultValue.ToString());
							}

							CSharp.AppendLine(";");
						}

						if (Attr is ByReferenceAttribute)
							ByReference = true;

						if (Attr is ObjectIdAttribute)
						{
							this.objectIdFieldInfo = FI;
							this.objectIdPropertyInfo = PI;
						}
					}

					if (Ignore)
						continue;

					if (FilesProvider.GetFieldDataTypeCode(Type) == TYPE_OBJECT)
					{
						CSharp.Append("\t\tprivate IObjectSerializer serializer");
						CSharp.Append(Member.Name);
						CSharp.AppendLine(";");
					}
				}

				if (NrDefault > 0)
					CSharp.AppendLine();

				CSharp.AppendLine();
				CSharp.AppendLine("\t\tpublic BinarySerializer" + TypeName + this.provider.Id + "(FilesProvider Provider)");
				CSharp.AppendLine("\t\t{");
				CSharp.AppendLine("\t\t\tthis.provider = Provider;");

				MemberInfo ObjectIdMember = null;
				Type ObjectIdMemberType = null;

				foreach (MemberInfo Member in Members)
				{
					if (!((FI = Member as FieldInfo) is null))
					{
						if (!FI.IsPublic || FI.IsStatic)
							continue;

						PI = null;
						MemberType = FI.FieldType;
					}
					else if (!((PI = Member as PropertyInfo) is null))
					{
						if ((MI = PI.GetMethod) is null || !MI.IsPublic || MI.IsStatic)
							continue;

						if ((MI = PI.SetMethod) is null || !MI.IsPublic || MI.IsStatic)
							continue;

						if (PI.GetIndexParameters().Length > 0)
							continue;

						MemberType = PI.PropertyType;
					}
					else
						continue;

					Ignore = false;
					ByReference = false;
					Nullable = false;
					HasObjectId = false;
					//ShortName = null;

					MemberTypeInfo = MemberType.GetTypeInfo();
					if (MemberTypeInfo.IsGenericType)
					{
						Type GT = MemberType.GetGenericTypeDefinition();
						if (GT == typeof(Nullable<>))
						{
							Nullable = true;
							MemberType = MemberType.GenericTypeArguments[0];
							MemberTypeInfo = MemberType.GetTypeInfo();
						}
					}

					foreach (Attribute Attr in Member.GetCustomAttributes(true))
					{
						if (Attr is IgnoreMemberAttribute)
						{
							Ignore = true;
							break;
						}

						if (Attr is ByReferenceAttribute)
							ByReference = true;
						else if (Attr is ObjectIdAttribute)
						{
							ObjectIdMember = Member;
							ObjectIdMemberType = MemberType;
							HasObjectId = true;
						}
						//else if (Attr is ShortNameAttribute ShortNameAttribute)
						//	ShortName = ShortNameAttribute.Name;
					}

					if (Ignore || HasObjectId)
						continue;

					if (FilesProvider.GetFieldDataTypeCode(Type) == TYPE_OBJECT)
					{
						CSharp.Append("\t\t\tthis.serializer");
						CSharp.Append(Member.Name);
						CSharp.Append(" = this.provider.GetObjectSerializer(typeof(");
						this.AppendType(MemberType, CSharp);
						CSharp.AppendLine("));");
					}
				}

				CSharp.AppendLine("\t\t}");
				CSharp.AppendLine();
				CSharp.AppendLine("\t\tpublic override Type ValueType { get { return typeof(" + Type.FullName + "); } }");
				CSharp.AppendLine();
				CSharp.AppendLine("\t\tpublic override bool IsNullable { get { return " + (this.isNullable ? "true" : "false") + "; } }");
				CSharp.AppendLine();
				CSharp.AppendLine("\t\tpublic override object Deserialize(BinaryDeserializer Reader, uint? DataType, bool Embedded)");
				CSharp.AppendLine("\t\t{");
				CSharp.AppendLine("\t\t\tuint FieldDataType;");
				CSharp.AppendLine("\t\t\tulong FieldCode;");
				CSharp.AppendLine("\t\t\t" + TypeName + " Result;");
				CSharp.AppendLine("\t\t\tStreamBookmark Bookmark = Reader.GetBookmark();");
				CSharp.AppendLine("\t\t\tuint? DataTypeBak = DataType;");
				CSharp.AppendLine("\t\t\tGuid ObjectId = Embedded ? Guid.Empty : Reader.ReadGuid();");
				CSharp.AppendLine("\t\t\tulong ContentLen = Embedded ? 0 : Reader.ReadVariableLengthUInt64();");
				CSharp.AppendLine();
				CSharp.AppendLine("\t\t\tif (!DataType.HasValue)");
				CSharp.AppendLine("\t\t\t{");
				CSharp.AppendLine("\t\t\t\tDataType = Reader.ReadBits(6);");
				CSharp.AppendLine("\t\t\t\tif (DataType.Value == " + TYPE_NULL + ")");
				CSharp.AppendLine("\t\t\t\t\treturn null;");
				CSharp.AppendLine("\t\t\t}");
				CSharp.AppendLine();

				CSharp.AppendLine("\t\t\tFieldCode = Reader.ReadVariableLengthUInt64();");

				if (this.typeNameSerialization != TypeNameSerialization.None)
				{
					CSharp.AppendLine("\t\t\tstring TypeName = this.provider.GetFieldName(\"" + Escape(this.collectionName) + "\", FieldCode);");

					if (this.typeNameSerialization == TypeNameSerialization.LocalName)
						CSharp.AppendLine("\t\t\tTypeName = \"" + Type.Namespace + ".\" + TypeName;");

					CSharp.AppendLine();
					CSharp.AppendLine("\t\t\tType DesiredType = Waher.Runtime.Inventory.Types.GetType(TypeName);");
					CSharp.AppendLine("\t\t\tif (DesiredType is null)");
					CSharp.AppendLine("\t\t\t\tDesiredType = typeof(GenericObject);");
					CSharp.AppendLine();
					CSharp.AppendLine("\t\t\tif (DesiredType != typeof(" + Type.FullName + "))");
					CSharp.AppendLine("\t\t\t{");
					CSharp.AppendLine("\t\t\t\tIObjectSerializer Serializer2 = this.provider.GetObjectSerializer(DesiredType);");
					CSharp.AppendLine("\t\t\t\tReader.SetBookmark(Bookmark);");
					CSharp.AppendLine("\t\t\t\treturn Serializer2.Deserialize(Reader, DataTypeBak, Embedded);");
					CSharp.AppendLine("\t\t\t}");
				}

				if (this.typeInfo.IsAbstract)
					CSharp.AppendLine("\t\t\tthrow new Exception(\"Unable to create an instance of an abstract class.\");");
				else
				{
					CSharp.AppendLine();

					CSharp.AppendLine("\t\t\tif (Embedded)");
					CSharp.AppendLine("\t\t\t\tReader.ReadVariableLengthUInt64();	// Collection name");

					CSharp.AppendLine();

					CSharp.AppendLine("\t\t\tif (DataType.Value != " + TYPE_OBJECT + ")");
					CSharp.AppendLine("\t\t\t\tthrow new Exception(\"Object expected.\");");

					CSharp.AppendLine();
					CSharp.AppendLine("\t\t\tResult = new " + Type.FullName + "();");
					CSharp.AppendLine();

					if (!(ObjectIdMember is null))
					{
						if (ObjectIdMemberType == typeof(Guid))
							CSharp.AppendLine("\t\t\tResult." + ObjectIdMember.Name + " = ObjectId;");
						else if (ObjectIdMemberType == typeof(string))
							CSharp.AppendLine("\t\t\tResult." + ObjectIdMember.Name + " = ObjectId.ToString();");
						else if (ObjectIdMemberType == typeof(byte[]))
							CSharp.AppendLine("\t\t\tResult." + ObjectIdMember.Name + " = ObjectId.ToByteArray();");
						else
							throw new Exception("Type not supported for Object ID fields: " + ObjectIdMemberType.FullName);

						CSharp.AppendLine();
					}

					CSharp.AppendLine("\t\t\twhile ((FieldCode = Reader.ReadVariableLengthUInt64()) != 0)");
					CSharp.AppendLine("\t\t\t{");
					CSharp.AppendLine("\t\t\t\tFieldDataType = Reader.ReadBits(6);");
					CSharp.AppendLine();
					CSharp.AppendLine("\t\t\t\tswitch (FieldCode)");
					CSharp.AppendLine("\t\t\t\t{");

					foreach (MemberInfo Member in Members)
					{
						if (!((FI = Member as FieldInfo) is null))
						{
							if (!FI.IsPublic || FI.IsStatic)
								continue;

							PI = null;
							MemberType = FI.FieldType;
						}
						else if (!((PI = Member as PropertyInfo) is null))
						{
							if ((MI = PI.GetMethod) is null || !MI.IsPublic || MI.IsStatic)
								continue;

							if ((MI = PI.SetMethod) is null || !MI.IsPublic || MI.IsStatic)
								continue;

							if (PI.GetIndexParameters().Length > 0)
								continue;

							MemberType = PI.PropertyType;
						}
						else
							continue;

						Ignore = false;
						//ShortName = null;
						ByReference = false;
						Nullable = false;

						MemberTypeInfo = MemberType.GetTypeInfo();
						if (MemberTypeInfo.IsGenericType)
						{
							Type GT = MemberType.GetGenericTypeDefinition();
							if (GT == typeof(Nullable<>))
							{
								Nullable = true;
								MemberType = MemberType.GenericTypeArguments[0];
								MemberTypeInfo = MemberType.GetTypeInfo();
							}
						}

						foreach (Attribute Attr in Member.GetCustomAttributes(true))
						{
							if (Attr is IgnoreMemberAttribute || Attr is ObjectIdAttribute)
							{
								Ignore = true;
								break;
							}

							if (Attr is ByReferenceAttribute)
								ByReference = true;

							//if (Attr is ShortNameAttribute ShortNameAttribute)
							//	ShortName = ShortNameAttribute.Name;
						}

						if (Ignore)
							continue;

						//if (!string.IsNullOrEmpty(ShortName) && ShortName != Member.Name)
						//	CSharp.AppendLine("\t\t\t\t\tcase " + this.provider.GetFieldCode(this.collectionName, ShortName) + ":");

						CSharp.AppendLine("\t\t\t\t\tcase " + this.provider.GetFieldCode(this.collectionName, Member.Name) + ":");

						if (MemberTypeInfo.IsEnum)
						{
							CSharp.AppendLine("\t\t\t\t\t\tswitch (FieldDataType)");
							CSharp.AppendLine("\t\t\t\t\t\t{");
							CSharp.AppendLine("\t\t\t\t\t\t\tcase " + TYPE_BOOLEAN + ":");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (" + MemberType.FullName + ")(Reader.ReadBoolean() ? 1 : 0);");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
							CSharp.AppendLine();
							CSharp.AppendLine("\t\t\t\t\t\t\tcase " + TYPE_BYTE + ":");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (" + MemberType.FullName + ")Reader.ReadByte();");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
							CSharp.AppendLine();
							CSharp.AppendLine("\t\t\t\t\t\t\tcase " + TYPE_INT16 + ":");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (" + MemberType.FullName + ")Reader.ReadInt16();");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
							CSharp.AppendLine();
							CSharp.AppendLine("\t\t\t\t\t\t\tcase " + TYPE_INT32 + ":");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (" + MemberType.FullName + ")Reader.ReadInt32();");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
							CSharp.AppendLine();
							CSharp.AppendLine("\t\t\t\t\t\t\tcase " + TYPE_INT64 + ":");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (" + MemberType.FullName + ")Reader.ReadInt64();");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
							CSharp.AppendLine();
							CSharp.AppendLine("\t\t\t\t\t\t\tcase " + TYPE_SBYTE + ":");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (" + MemberType.FullName + ")Reader.ReadSByte();");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
							CSharp.AppendLine();
							CSharp.AppendLine("\t\t\t\t\t\t\tcase " + TYPE_UINT16 + ":");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (" + MemberType.FullName + ")Reader.ReadUInt16();");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
							CSharp.AppendLine();
							CSharp.AppendLine("\t\t\t\t\t\t\tcase " + TYPE_UINT32 + ":");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (" + MemberType.FullName + ")Reader.ReadUInt32();");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
							CSharp.AppendLine();
							CSharp.AppendLine("\t\t\t\t\t\t\tcase " + TYPE_UINT64 + ":");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (" + MemberType.FullName + ")Reader.ReadUInt64();");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
							CSharp.AppendLine();
							CSharp.AppendLine("\t\t\t\t\t\t\tcase " + TYPE_DECIMAL + ":");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (" + MemberType.FullName + ")(int)Reader.ReadDecimal();");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
							CSharp.AppendLine();
							CSharp.AppendLine("\t\t\t\t\t\t\tcase " + TYPE_DOUBLE + ":");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (" + MemberType.FullName + ")(int)Reader.ReadDouble();");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
							CSharp.AppendLine();
							CSharp.AppendLine("\t\t\t\t\t\t\tcase " + TYPE_SINGLE + ":");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (" + MemberType.FullName + ")(int)Reader.ReadSingle();");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
							CSharp.AppendLine();
							CSharp.AppendLine("\t\t\t\t\t\t\tcase " + TYPE_STRING + ":");
							CSharp.AppendLine("\t\t\t\t\t\t\tcase " + TYPE_CI_STRING + ":");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (" + MemberType.FullName + ")Enum.Parse(typeof(" + MemberType.FullName + "), Reader.ReadString());");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
							CSharp.AppendLine();
							CSharp.AppendLine("\t\t\t\t\t\t\tcase " + TYPE_ENUM + ":");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (" + MemberType.FullName + ")Reader.ReadEnum(typeof(" + MemberType.FullName + "));");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");

							if (Nullable)
							{
								CSharp.AppendLine();
								CSharp.AppendLine("\t\t\t\t\t\t\tcase " + TYPE_NULL + ":");
								CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = null;");
								CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
							}

							CSharp.AppendLine();
							CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Unable to set " + Member.Name + ". Expected an enumeration value, but was a \" + FilesProvider.GetFieldDataTypeName(FieldDataType) + \".\");");
							CSharp.AppendLine("\t\t\t\t\t\t}");
						}
						else
						{
							switch (Type.GetTypeCode(MemberType))
							{
								case TypeCode.Boolean:
									if (Nullable)
										CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = ReadNullableBoolean(Reader, FieldDataType);");
									else
										CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = ReadBoolean(Reader, FieldDataType);");
									break;

								case TypeCode.Byte:
									if (Nullable)
										CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = ReadNullableByte(Reader, FieldDataType);");
									else
										CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = ReadByte(Reader, FieldDataType);");
									break;

								case TypeCode.Char:
									if (Nullable)
										CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = ReadNullableChar(Reader, FieldDataType);");
									else
										CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = ReadChar(Reader, FieldDataType);");
									break;

								case TypeCode.DateTime:
									if (Nullable)
										CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = ReadNullableDateTime(Reader, FieldDataType);");
									else
										CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = ReadDateTime(Reader, FieldDataType);");
									break;

								case TypeCode.Decimal:
									if (Nullable)
										CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = ReadNullableDecimal(Reader, FieldDataType);");
									else
										CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = ReadDecimal(Reader, FieldDataType);");
									break;

								case TypeCode.Double:
									if (Nullable)
										CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = ReadNullableDouble(Reader, FieldDataType);");
									else
										CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = ReadDouble(Reader, FieldDataType);");
									break;

								case TypeCode.Int16:
									if (Nullable)
										CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = ReadNullableInt16(Reader, FieldDataType);");
									else
										CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = ReadInt16(Reader, FieldDataType);");
									break;

								case TypeCode.Int32:
									if (Nullable)
										CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = ReadNullableInt32(Reader, FieldDataType);");
									else
										CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = ReadInt32(Reader, FieldDataType);");
									break;

								case TypeCode.Int64:
									if (Nullable)
										CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = ReadNullableInt64(Reader, FieldDataType);");
									else
										CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = ReadInt64(Reader, FieldDataType);");
									break;

								case TypeCode.SByte:
									if (Nullable)
										CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = ReadNullableSByte(Reader, FieldDataType);");
									else
										CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = ReadSByte(Reader, FieldDataType);");
									break;

								case TypeCode.Single:
									if (Nullable)
										CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = ReadNullableSingle(Reader, FieldDataType);");
									else
										CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = ReadSingle(Reader, FieldDataType);");
									break;

								case TypeCode.String:
									CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = ReadString(Reader, FieldDataType);");
									break;

								case TypeCode.UInt16:
									if (Nullable)
										CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = ReadNullableUInt16(Reader, FieldDataType);");
									else
										CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = ReadUInt16(Reader, FieldDataType);");
									break;

								case TypeCode.UInt32:
									if (Nullable)
										CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = ReadNullableUInt32(Reader, FieldDataType);");
									else
										CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = ReadUInt32(Reader, FieldDataType);");
									break;

								case TypeCode.UInt64:
									if (Nullable)
										CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = ReadNullableUInt64(Reader, FieldDataType);");
									else
										CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = ReadUInt64(Reader, FieldDataType);");
									break;

								case TypeCode.Empty:
								default:
									throw new Exception("Invalid member type: " + MemberType.FullName);

								case TypeCode.Object:
									if (MemberType.IsArray)
									{
										if (MemberType == typeof(byte[]))
											CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = Reader.ReadByteArray();");
										else
										{
											MemberType = MemberType.GetElementType();
											CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = ReadArray<" + GenericParameterName(MemberType) + ">(this.provider, Reader, FieldDataType);");
										}
									}
									else if (ByReference)
									{
										CSharp.AppendLine("\t\t\t\t\t\tswitch (FieldDataType)");
										CSharp.AppendLine("\t\t\t\t\t\t{");
										CSharp.AppendLine("\t\t\t\t\t\t\tcase " + TYPE_GUID + ":");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tGuid " + Member.Name + "ObjectId = Reader.ReadGuid();");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tTask<" + MemberType.FullName + "> " + Member.Name + "Task = this.provider.LoadObject<" + GenericParameterName(MemberType) + ">(" + Member.Name + "ObjectId, (EmbeddedValue) => Result." + Member.Name + " = (" + MemberType.FullName + ")EmbeddedValue);");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tif (!" + Member.Name + "Task.Wait(10000))");
										CSharp.AppendLine("\t\t\t\t\t\t\t\t\tthrow new TimeoutException(\"Unable to load referenced object. Database timed out.\");");
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = " + Member.Name + "Task.Result;");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tcase " + TYPE_NULL + ":");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = null;");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Object ID expected for " + Member.Name + ".\");");
										CSharp.AppendLine("\t\t\t\t\t\t}");
									}
									else if (MemberType == typeof(TimeSpan))
									{
										if (Nullable)
											CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = ReadNullableTimeSpan(Reader, FieldDataType);");
										else
											CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = ReadTimeSpan(Reader, FieldDataType);");
									}
									else if (MemberType == typeof(Guid))
									{
										if (Nullable)
											CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = ReadNullableGuid(Reader, FieldDataType);");
										else
											CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = ReadGuid(Reader, FieldDataType);");
									}
									else if (MemberType == typeof(DateTimeOffset))
									{
										if (Nullable)
											CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = ReadNullableDateTimeOffset(Reader, FieldDataType);");
										else
											CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = ReadDateTimeOffset(Reader, FieldDataType);");
									}
									else
									{
										CSharp.AppendLine("\t\t\t\t\t\tswitch (FieldDataType)");
										CSharp.AppendLine("\t\t\t\t\t\t{");
										CSharp.AppendLine("\t\t\t\t\t\t\tcase " + TYPE_OBJECT + ":");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (" + MemberType.FullName + ")this.serializer" + Member.Name + ".Deserialize(Reader, FieldDataType, true);");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tcase " + TYPE_NULL + ":");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = null;");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");

										if (MemberTypeInfo.IsAssignableFrom(typeof(bool)))
										{
											CSharp.AppendLine();
											CSharp.AppendLine("\t\t\t\t\t\t\tcase " + TYPE_BOOLEAN + ":");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (" + MemberType.FullName + ")ReadBoolean(Reader, FieldDataType);");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										}

										if (MemberTypeInfo.IsAssignableFrom(typeof(byte)))
										{
											CSharp.AppendLine();
											CSharp.AppendLine("\t\t\t\t\t\t\tcase " + TYPE_BYTE + ":");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (" + MemberType.FullName + ")ReadByte(Reader, FieldDataType);");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										}

										if (MemberTypeInfo.IsAssignableFrom(typeof(short)))
										{
											CSharp.AppendLine();
											CSharp.AppendLine("\t\t\t\t\t\t\tcase " + TYPE_INT16 + ":");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (" + MemberType.FullName + ")ReadInt16(Reader, FieldDataType);");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										}

										if (MemberTypeInfo.IsAssignableFrom(typeof(int)))
										{
											CSharp.AppendLine();
											CSharp.AppendLine("\t\t\t\t\t\t\tcase " + TYPE_INT32 + ":");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (" + MemberType.FullName + ")ReadInt32(Reader, FieldDataType);");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										}

										if (MemberTypeInfo.IsAssignableFrom(typeof(long)))
										{
											CSharp.AppendLine();
											CSharp.AppendLine("\t\t\t\t\t\t\tcase " + TYPE_INT64 + ":");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (" + MemberType.FullName + ")ReadInt64(Reader, FieldDataType);");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										}

										if (MemberTypeInfo.IsAssignableFrom(typeof(sbyte)))
										{
											CSharp.AppendLine();
											CSharp.AppendLine("\t\t\t\t\t\t\tcase " + TYPE_SBYTE + ":");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (" + MemberType.FullName + ")ReadSByte(Reader, FieldDataType);");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										}

										if (MemberTypeInfo.IsAssignableFrom(typeof(ushort)))
										{
											CSharp.AppendLine();
											CSharp.AppendLine("\t\t\t\t\t\t\tcase " + TYPE_UINT16 + ":");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (" + MemberType.FullName + ")ReadUInt16(Reader, FieldDataType);");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										}

										if (MemberTypeInfo.IsAssignableFrom(typeof(uint)))
										{
											CSharp.AppendLine();
											CSharp.AppendLine("\t\t\t\t\t\t\tcase " + TYPE_UINT32 + ":");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (" + MemberType.FullName + ")ReadUInt32(Reader, FieldDataType);");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										}

										if (MemberTypeInfo.IsAssignableFrom(typeof(ulong)))
										{
											CSharp.AppendLine();
											CSharp.AppendLine("\t\t\t\t\t\t\tcase " + TYPE_UINT64 + ":");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (" + MemberType.FullName + ")ReadUInt64(Reader, FieldDataType);");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										}

										if (MemberTypeInfo.IsAssignableFrom(typeof(decimal)))
										{
											CSharp.AppendLine();
											CSharp.AppendLine("\t\t\t\t\t\t\tcase " + TYPE_DECIMAL + ":");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (" + MemberType.FullName + ")ReadDecimal(Reader, FieldDataType);");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										}

										if (MemberTypeInfo.IsAssignableFrom(typeof(double)))
										{
											CSharp.AppendLine();
											CSharp.AppendLine("\t\t\t\t\t\t\tcase " + TYPE_DOUBLE + ":");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (" + MemberType.FullName + ")ReadDouble(Reader, FieldDataType);");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										}

										if (MemberTypeInfo.IsAssignableFrom(typeof(float)))
										{
											CSharp.AppendLine();
											CSharp.AppendLine("\t\t\t\t\t\t\tcase " + TYPE_SINGLE + ":");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (" + MemberType.FullName + ")ReadSingle(Reader, FieldDataType);");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										}

										if (MemberTypeInfo.IsAssignableFrom(typeof(DateTime)))
										{
											CSharp.AppendLine();
											CSharp.AppendLine("\t\t\t\t\t\t\tcase " + TYPE_DATETIME + ":");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (" + MemberType.FullName + ")ReadDateTime(Reader, FieldDataType);");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										}

										if (MemberTypeInfo.IsAssignableFrom(typeof(DateTimeOffset)))
										{
											CSharp.AppendLine();
											CSharp.AppendLine("\t\t\t\t\t\t\tcase " + TYPE_DATETIMEOFFSET + ":");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (" + MemberType.FullName + ")ReadDateTimeOffset(Reader, FieldDataType);");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										}

										if (MemberTypeInfo.IsAssignableFrom(typeof(TimeSpan)))
										{
											CSharp.AppendLine();
											CSharp.AppendLine("\t\t\t\t\t\t\tcase " + TYPE_TIMESPAN + ":");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (" + MemberType.FullName + ")ReadTimeSpan(Reader, FieldDataType);");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										}

										if (MemberTypeInfo.IsAssignableFrom(typeof(char)))
										{
											CSharp.AppendLine();
											CSharp.AppendLine("\t\t\t\t\t\t\tcase " + TYPE_CHAR + ":");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (" + MemberType.FullName + ")ReadChar(Reader, FieldDataType);");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										}

										if (MemberTypeInfo.IsAssignableFrom(typeof(string)) ||
											MemberTypeInfo.IsAssignableFrom(typeof(CaseInsensitiveString)))
										{
											CSharp.AppendLine();
											CSharp.AppendLine("\t\t\t\t\t\t\tcase " + TYPE_STRING + ":");
											CSharp.AppendLine("\t\t\t\t\t\t\tcase " + TYPE_CI_STRING + ":");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (" + MemberType.FullName + ")ReadString(Reader, FieldDataType);");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										}

										if (MemberTypeInfo.IsAssignableFrom(typeof(byte[])))
										{
											CSharp.AppendLine();
											CSharp.AppendLine("\t\t\t\t\t\t\tcase " + TYPE_BYTEARRAY + ":");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (" + MemberType.FullName + ")ReadByteArray(Reader, FieldDataType);");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										}

										if (MemberTypeInfo.IsAssignableFrom(typeof(Guid)))
										{
											CSharp.AppendLine();
											CSharp.AppendLine("\t\t\t\t\t\t\tcase " + TYPE_GUID + ":");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (" + MemberType.FullName + ")ReadGuid(Reader, FieldDataType);");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										}

										if (MemberTypeInfo.IsAssignableFrom(typeof(Enum)))
										{
											CSharp.AppendLine();
											CSharp.AppendLine("\t\t\t\t\t\t\tcase " + TYPE_ENUM + ":");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (" + MemberType.FullName + ")ReadString(Reader, FieldDataType);");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										}

										if (MemberTypeInfo.IsAssignableFrom(typeof(Array)))
										{
											CSharp.AppendLine();
											CSharp.AppendLine("\t\t\t\t\t\t\tcase " + TYPE_ARRAY + ":");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (" + MemberType.FullName + ")ReadArray<Waher.Persistence.Serialization.GenericObject>(this.provider, Reader, FieldDataType);");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										}

										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Object expected for " + Member.Name + ". Data Type read: \" + FilesProvider.GetFieldDataTypeName(FieldDataType));");
										CSharp.AppendLine("\t\t\t\t\t\t}");
									}
									break;
							}
						}

						CSharp.AppendLine("\t\t\t\t\t\tbreak;");
						CSharp.AppendLine();
					}

					CSharp.AppendLine("\t\t\t\t\tdefault:");
					CSharp.Append("\t\t\t\t\t\tthrow new Exception(\"Field name not recognized: \" + this.provider.GetFieldName(\"");
					if (!string.IsNullOrEmpty(this.collectionName))
						CSharp.Append(Escape(this.collectionName));
					CSharp.AppendLine("\", FieldCode));");

					CSharp.AppendLine("\t\t\t\t}");
					CSharp.AppendLine("\t\t\t}");
					CSharp.AppendLine();
					CSharp.AppendLine("\t\t\treturn Result;");
				}

				CSharp.AppendLine("\t\t}");
				CSharp.AppendLine();
				CSharp.AppendLine("\t\tpublic override void Serialize(BinarySerializer Writer, bool WriteTypeCode, bool Embedded, object UntypedValue)");
				CSharp.AppendLine("\t\t{");
				CSharp.AppendLine("\t\t\t" + TypeName + " Value = (" + TypeName + ")UntypedValue;");
				CSharp.AppendLine("\t\t\tBinarySerializer WriterBak = Writer;");
				CSharp.AppendLine();
				CSharp.AppendLine("\t\t\tif (!Embedded)");

				if (this.debug)
					CSharp.AppendLine("\t\t\t\tWriter = new BinarySerializer(Writer.CollectionName, Writer.Encoding, true);");
				else
					CSharp.AppendLine("\t\t\t\tWriter = new BinarySerializer(Writer.CollectionName, Writer.Encoding, false);");

				CSharp.AppendLine();

				if (this.isNullable)
				{
					CSharp.AppendLine("\t\t\tif (WriteTypeCode)");
					CSharp.AppendLine("\t\t\t{");
					CSharp.AppendLine("\t\t\t\tif (Value is null)");
					CSharp.AppendLine("\t\t\t\t{");
					CSharp.AppendLine("\t\t\t\t\tWriter.WriteBits(" + TYPE_NULL + ", 6);");
					CSharp.AppendLine("\t\t\t\t\treturn;");
					CSharp.AppendLine("\t\t\t\t}");
					CSharp.AppendLine("\t\t\t\telse");
					CSharp.AppendLine("\t\t\t\t\tWriter.WriteBits(" + TYPE_OBJECT + ", 6);");
					CSharp.AppendLine("\t\t\t}");
					CSharp.AppendLine("\t\t\telse if (Value is null)");
					CSharp.AppendLine("\t\t\t\tthrow new NullReferenceException(\"Value cannot be null.\");");
				}
				else
				{
					CSharp.AppendLine("\t\t\tif (WriteTypeCode)");
					CSharp.AppendLine("\t\t\t\tWriter.WriteBits(" + TYPE_OBJECT + ", 6);");
				}

				CSharp.AppendLine();

				if (this.typeNameSerialization == TypeNameSerialization.None)
					CSharp.AppendLine("\t\t\tWriter.WriteVariableLengthUInt64(0);");
				else
				{
					if (this.debug)
						CSharp.AppendLine("\t\t\tConsole.Out.WriteLine();");

					CSharp.Append("\t\t\tWriter.WriteVariableLengthUInt64(");

					if (this.typeNameSerialization == TypeNameSerialization.LocalName)
						CSharp.Append(this.provider.GetFieldCode(this.collectionName, this.type.Name));
					else
						CSharp.Append(this.provider.GetFieldCode(this.collectionName, this.type.FullName));

					CSharp.AppendLine(");");
				}

				CSharp.AppendLine("\t\t\tif (Embedded)");
				CSharp.Append("\t\t\t\tWriter.WriteVariableLengthUInt64(");
				CSharp.Append(this.provider.GetFieldCode(null, string.IsNullOrEmpty(this.collectionName) ? this.provider.DefaultCollectionName : this.collectionName));
				CSharp.AppendLine(");");

				foreach (MemberInfo Member in Members)
				{
					if (!((FI = Member as FieldInfo) is null))
					{
						if (!FI.IsPublic || FI.IsStatic)
							continue;

						PI = null;
						MemberType = FI.FieldType;
					}
					else if (!((PI = Member as PropertyInfo) is null))
					{
						if ((MI = PI.GetMethod) is null || !MI.IsPublic || MI.IsStatic)
							continue;

						if ((MI = PI.SetMethod) is null || !MI.IsPublic || MI.IsStatic)
							continue;

						if (PI.GetIndexParameters().Length > 0)
							continue;

						MemberType = PI.PropertyType;
					}
					else
						continue;

					Ignore = false;
					//ShortName = null;
					HasDefaultValue = false;
					DefaultValue = null;
					ObjectIdField = false;
					ByReference = false;
					Nullable = false;

					MemberTypeInfo = MemberType.GetTypeInfo();
					if (MemberTypeInfo.IsGenericType)
					{
						Type GT = MemberType.GetGenericTypeDefinition();
						if (GT == typeof(Nullable<>))
						{
							Nullable = true;
							MemberType = MemberType.GenericTypeArguments[0];
							MemberTypeInfo = MemberType.GetTypeInfo();
						}
					}

					foreach (Attribute Attr in Member.GetCustomAttributes(true))
					{
						if (Attr is IgnoreMemberAttribute)
						{
							Ignore = true;
							break;
						}
						else if (Attr is DefaultValueAttribute DefaultValueAttribute)
						{
							HasDefaultValue = true;
							DefaultValue = DefaultValueAttribute.Value;
						}
						//else if (Attr is ShortNameAttribute ShortNameAttribute)
						//	ShortName = ShortNameAttribute.Name;
						else if (Attr is ObjectIdAttribute)
							ObjectIdField = true;
						else if (Attr is ByReferenceAttribute)
							ByReference = true;
					}

					if (Ignore)
						continue;

					CSharp.AppendLine();

					if (HasDefaultValue)
					{
						if (DefaultValue is null)
							CSharp.AppendLine("\t\t\tif (!((object)Value." + Member.Name + " is null))");
						else if (MemberType == typeof(string) && DefaultValue is string s2 && string.IsNullOrEmpty(s2))
							CSharp.AppendLine("\t\t\tif (!string.IsNullOrEmpty(Value." + Member.Name + "))");
						else if (MemberType == typeof(CaseInsensitiveString) && DefaultValue is CaseInsensitiveString s3 && CaseInsensitiveString.IsNullOrEmpty(s3))
							CSharp.AppendLine("\t\t\tif (!CaseInsensitiveString.IsNullOrEmpty(Value." + Member.Name + "))");
						else
							CSharp.AppendLine("\t\t\tif (!default" + Member.Name + ".Equals(Value." + Member.Name + "))");

						CSharp.AppendLine("\t\t\t{");
						Indent = "\t\t\t\t";
					}
					else
						Indent = "\t\t\t";

					CSharp.Append(Indent);

					if (ObjectIdField)
					{
						CSharp.Append(MemberType.FullName);
						CSharp.Append(" ObjectId = Value.");
						CSharp.Append(Member.Name);
						CSharp.AppendLine(";");
					}
					else
					{
						if (this.debug)
						{
							CSharp.AppendLine("Console.Out.WriteLine();");
							CSharp.Append(Indent);
							CSharp.Append("Console.Out.WriteLine(\"");
							CSharp.Append(Escape(Member.Name));
							CSharp.AppendLine("\");");
							CSharp.Append(Indent);
						}

						CSharp.Append("Writer.WriteVariableLengthUInt64(");
						//if (string.IsNullOrEmpty(ShortName))
						CSharp.Append(this.provider.GetFieldCode(this.collectionName, Member.Name));
						//else
						//	CSharp.Append(this.provider.GetFieldCode(this.collectionName, ShortName));
						CSharp.AppendLine(");");

						if (Nullable)
						{
							Indent2 = Indent + "\t";

							CSharp.Append(Indent);
							CSharp.Append("if (!Value.");
							CSharp.Append(Member.Name);
							CSharp.AppendLine(".HasValue)");
							CSharp.Append(Indent2);
							CSharp.AppendLine("Writer.WriteBits(" + TYPE_NULL + ", 6);");
							CSharp.Append(Indent);
							CSharp.AppendLine("else");
							CSharp.AppendLine("{");
						}
						else
							Indent2 = Indent;

						if (MemberTypeInfo.IsEnum)
						{
							if (MemberTypeInfo.IsDefined(typeof(FlagsAttribute), false))
							{
								CSharp.Append(Indent2);
								CSharp.Append("Writer.WriteBits(");
								CSharp.Append(TYPE_INT32);
								CSharp.AppendLine(", 6);");

								CSharp.Append(Indent2);
								CSharp.Append("Writer.Write((int)Value.");
								CSharp.Append(Member.Name);
								if (Nullable)
									CSharp.Append(".Value");
								CSharp.AppendLine(");");
							}
							else
							{
								CSharp.Append(Indent2);
								CSharp.Append("Writer.WriteBits(");
								CSharp.Append(TYPE_ENUM);
								CSharp.AppendLine(", 6);");

								CSharp.Append(Indent2);
								CSharp.Append("Writer.Write(Value.");
								CSharp.Append(Member.Name);
								if (Nullable)
									CSharp.Append(".Value");
								CSharp.AppendLine(");");
							}
						}
						else
						{
							switch (Type.GetTypeCode(MemberType))
							{
								case TypeCode.Boolean:
									CSharp.Append(Indent2);
									CSharp.Append("Writer.WriteBits(");
									CSharp.Append(TYPE_BOOLEAN);
									CSharp.AppendLine(", 6);");

									CSharp.Append(Indent2);
									CSharp.Append("Writer.Write(Value.");
									CSharp.Append(Member.Name);
									if (Nullable)
										CSharp.Append(".Value");
									CSharp.AppendLine(");");
									break;

								case TypeCode.Byte:
									CSharp.Append(Indent2);
									CSharp.Append("Writer.WriteBits(");
									CSharp.Append(TYPE_BYTE);
									CSharp.AppendLine(", 6);");

									CSharp.Append(Indent2);
									CSharp.Append("Writer.Write(Value.");
									CSharp.Append(Member.Name);
									if (Nullable)
										CSharp.Append(".Value");
									CSharp.AppendLine(");");
									break;

								case TypeCode.Char:
									CSharp.Append(Indent2);
									CSharp.Append("Writer.WriteBits(");
									CSharp.Append(TYPE_CHAR);
									CSharp.AppendLine(", 6);");

									CSharp.Append(Indent2);
									CSharp.Append("Writer.Write(Value.");
									CSharp.Append(Member.Name);
									if (Nullable)
										CSharp.Append(".Value");
									CSharp.AppendLine(");");
									break;

								case TypeCode.DateTime:
									CSharp.Append(Indent2);
									CSharp.Append("Writer.WriteBits(");
									CSharp.Append(TYPE_DATETIME);
									CSharp.AppendLine(", 6);");

									CSharp.Append(Indent2);
									CSharp.Append("Writer.Write(Value.");
									CSharp.Append(Member.Name);
									if (Nullable)
										CSharp.Append(".Value");
									CSharp.AppendLine(");");
									break;

								case TypeCode.Decimal:
									CSharp.Append(Indent2);
									CSharp.Append("Writer.WriteBits(");
									CSharp.Append(TYPE_DECIMAL);
									CSharp.AppendLine(", 6);");

									CSharp.Append(Indent2);
									CSharp.Append("Writer.Write(Value.");
									CSharp.Append(Member.Name);
									if (Nullable)
										CSharp.Append(".Value");
									CSharp.AppendLine(");");
									break;

								case TypeCode.Double:
									CSharp.Append(Indent2);
									CSharp.Append("Writer.WriteBits(");
									CSharp.Append(TYPE_DOUBLE);
									CSharp.AppendLine(", 6);");

									CSharp.Append(Indent2);
									CSharp.Append("Writer.Write(Value.");
									CSharp.Append(Member.Name);
									if (Nullable)
										CSharp.Append(".Value");
									CSharp.AppendLine(");");
									break;

								case TypeCode.Single:
									CSharp.Append(Indent2);
									CSharp.Append("Writer.WriteBits(");
									CSharp.Append(TYPE_SINGLE);
									CSharp.AppendLine(", 6);");

									CSharp.Append(Indent2);
									CSharp.Append("Writer.Write(Value.");
									CSharp.Append(Member.Name);
									if (Nullable)
										CSharp.Append(".Value");
									CSharp.AppendLine(");");
									break;

								case TypeCode.Int32:
									CSharp.Append(Indent2);
									CSharp.Append("Writer.WriteBits(");
									CSharp.Append(TYPE_INT32);
									CSharp.AppendLine(", 6);");

									CSharp.Append(Indent2);
									CSharp.Append("Writer.Write(Value.");
									CSharp.Append(Member.Name);
									if (Nullable)
										CSharp.Append(".Value");
									CSharp.AppendLine(");");
									break;

								case TypeCode.Int16:
									CSharp.Append(Indent2);
									CSharp.Append("Writer.WriteBits(");
									CSharp.Append(TYPE_INT16);
									CSharp.AppendLine(", 6);");

									CSharp.Append(Indent2);
									CSharp.Append("Writer.Write(Value.");
									CSharp.Append(Member.Name);
									if (Nullable)
										CSharp.Append(".Value");
									CSharp.AppendLine(");");
									break;

								case TypeCode.UInt16:
									CSharp.Append(Indent2);
									CSharp.Append("Writer.WriteBits(");
									CSharp.Append(TYPE_UINT16);
									CSharp.AppendLine(", 6);");

									CSharp.Append(Indent2);
									CSharp.Append("Writer.Write(Value.");
									CSharp.Append(Member.Name);
									if (Nullable)
										CSharp.Append(".Value");
									CSharp.AppendLine(");");
									break;

								case TypeCode.SByte:
									CSharp.Append(Indent2);
									CSharp.Append("Writer.WriteBits(");
									CSharp.Append(TYPE_SBYTE);
									CSharp.AppendLine(", 6);");

									CSharp.Append(Indent2);
									CSharp.Append("Writer.Write(Value.");
									CSharp.Append(Member.Name);
									if (Nullable)
										CSharp.Append(".Value");
									CSharp.AppendLine(");");
									break;

								case TypeCode.Int64:
									CSharp.Append(Indent2);
									CSharp.Append("Writer.WriteBits(");
									CSharp.Append(TYPE_INT64);
									CSharp.AppendLine(", 6);");

									CSharp.Append(Indent2);
									CSharp.Append("Writer.Write(Value.");
									CSharp.Append(Member.Name);
									if (Nullable)
										CSharp.Append(".Value");
									CSharp.AppendLine(");");
									break;

								case TypeCode.UInt32:
									CSharp.Append(Indent2);
									CSharp.Append("Writer.WriteBits(");
									CSharp.Append(TYPE_UINT32);
									CSharp.AppendLine(", 6);");

									CSharp.Append(Indent2);
									CSharp.Append("Writer.Write(Value.");
									CSharp.Append(Member.Name);
									if (Nullable)
										CSharp.Append(".Value");
									CSharp.AppendLine(");");
									break;

								case TypeCode.String:
									CSharp.Append(Indent2);
									CSharp.Append("if (Value.");
									CSharp.Append(Member.Name);
									CSharp.AppendLine(" is null)");

									CSharp.Append(Indent2);
									CSharp.Append("\tWriter.WriteBits(");
									CSharp.Append(TYPE_NULL);
									CSharp.AppendLine(", 6);");

									CSharp.Append(Indent2);
									CSharp.AppendLine("else");
									CSharp.Append(Indent2);
									CSharp.AppendLine("{");

									CSharp.Append(Indent2);
									CSharp.Append("\tWriter.WriteBits(");
									CSharp.Append(TYPE_STRING);
									CSharp.AppendLine(", 6);");

									CSharp.Append(Indent2);
									CSharp.Append("\tWriter.Write(Value.");
									CSharp.Append(Member.Name);
									CSharp.AppendLine(");");

									CSharp.Append(Indent2);
									CSharp.AppendLine("}");
									break;

								case TypeCode.UInt64:
									CSharp.Append(Indent2);
									CSharp.Append("Writer.WriteBits(");
									CSharp.Append(TYPE_UINT64);
									CSharp.AppendLine(", 6);");

									CSharp.Append(Indent2);
									CSharp.Append("Writer.Write(Value.");
									CSharp.Append(Member.Name);
									if (Nullable)
										CSharp.Append(".Value");
									CSharp.AppendLine(");");
									break;

								case TypeCode.Empty:
									CSharp.Append(Indent2);
									CSharp.Append("Writer.WriteBits(");
									CSharp.Append(TYPE_NULL);
									CSharp.AppendLine(", 6);");
									break;

								default:
									throw new Exception("Invalid member type: " + MemberType.FullName);

								case TypeCode.Object:
									if (MemberType.IsArray)
									{
										if (MemberType == typeof(byte[]))
										{
											CSharp.Append(Indent2);
											CSharp.Append("Writer.WriteBits(");
											CSharp.Append(TYPE_BYTEARRAY);
											CSharp.AppendLine(", 6);");

											CSharp.Append(Indent2);
											CSharp.Append("Writer.Write(Value.");
											CSharp.Append(Member.Name);
											CSharp.AppendLine(");");
										}
										else
										{
											MemberType = MemberType.GetElementType();
											CSharp.Append(Indent2);
											CSharp.Append("WriteArray<");
											CSharp.Append(GenericParameterName(MemberType));
											CSharp.Append(">(this.provider, Writer, Value.");
											CSharp.Append(Member.Name);
											CSharp.AppendLine(");");
										}
									}
									else if (ByReference)
									{
										CSharp.Append(Indent2);
										CSharp.AppendLine("if (Value." + Member.Name + " is null)");
										CSharp.Append(Indent2);
										CSharp.AppendLine("\tWriter.WriteBits(" + TYPE_NULL + ", 6);");
										CSharp.Append(Indent2);
										CSharp.AppendLine("else");
										CSharp.Append(Indent2);
										CSharp.AppendLine("{");
										CSharp.Append(Indent2);
										CSharp.AppendLine("\tWriter.WriteBits(" + TYPE_GUID + ", 6);");
										CSharp.Append(Indent2);
										CSharp.AppendLine("\tObjectSerializer Serializer" + Member.Name + " = this.provider.GetObjectSerializerEx(typeof(" + MemberType.FullName + "));");
										CSharp.Append(Indent2);
										CSharp.AppendLine("\tTask<Guid> " + Member.Name + "Task = Serializer" + Member.Name + ".GetObjectId(Value." + Member.Name + ", true);");
										CSharp.Append(Indent2);
										CSharp.AppendLine("\tif (!" + Member.Name + "Task.Wait(" + this.provider.TimeoutMilliseconds.ToString() + "))");
										CSharp.Append(Indent2);
										CSharp.AppendLine("\t\tthrow new TimeoutException(\"Unable to load referenced object. Database timed out.\");");
										CSharp.Append(Indent2);
										CSharp.AppendLine("\tWriter.Write(" + Member.Name + "Task.Result);");
										CSharp.Append(Indent2);
										CSharp.AppendLine("}");
									}
									else if (MemberType == typeof(TimeSpan))
									{
										CSharp.Append(Indent2);
										CSharp.Append("Writer.WriteBits(");
										CSharp.Append(TYPE_TIMESPAN);
										CSharp.AppendLine(", 6);");

										CSharp.Append(Indent2);
										CSharp.Append("Writer.Write(Value.");
										CSharp.Append(Member.Name);
										if (Nullable)
											CSharp.Append(".Value");
										CSharp.AppendLine(");");
									}
									else if (MemberType == typeof(DateTimeOffset))
									{
										CSharp.Append(Indent2);
										CSharp.Append("Writer.WriteBits(");
										CSharp.Append(TYPE_DATETIMEOFFSET);
										CSharp.AppendLine(", 6);");

										CSharp.Append(Indent2);
										CSharp.Append("Writer.Write(Value.");
										CSharp.Append(Member.Name);
										if (Nullable)
											CSharp.Append(".Value");
										CSharp.AppendLine(");");
									}
									else if (MemberType == typeof(Guid))
									{
										CSharp.Append(Indent2);
										CSharp.Append("Writer.WriteBits(");
										CSharp.Append(TYPE_GUID);
										CSharp.AppendLine(", 6);");

										CSharp.Append(Indent2);
										CSharp.Append("Writer.Write(Value.");
										CSharp.Append(Member.Name);
										if (Nullable)
											CSharp.Append(".Value");
										CSharp.AppendLine(");");
									}
									else if (MemberType == typeof(CaseInsensitiveString))
									{
										CSharp.Append(Indent2);
										CSharp.Append("if (Value.");
										CSharp.Append(Member.Name);
										CSharp.AppendLine(" is null)");

										CSharp.Append(Indent2);
										CSharp.Append("\tWriter.WriteBits(");
										CSharp.Append(TYPE_NULL);
										CSharp.AppendLine(", 6);");

										CSharp.Append(Indent2);
										CSharp.AppendLine("else");
										CSharp.Append(Indent2);
										CSharp.AppendLine("{");

										CSharp.Append(Indent2);
										CSharp.Append("\tWriter.WriteBits(");
										CSharp.Append(TYPE_CI_STRING);
										CSharp.AppendLine(", 6);");

										CSharp.Append(Indent2);
										CSharp.Append("\tWriter.Write(Value.");
										CSharp.Append(Member.Name);
										CSharp.AppendLine(".Value);");

										CSharp.Append(Indent2);
										CSharp.AppendLine("}");
									}
									else
									{
										CSharp.Append(Indent2);
										CSharp.Append("this.serializer");
										CSharp.Append(Member.Name);
										CSharp.Append(".Serialize(Writer, true, true, Value.");
										CSharp.Append(Member.Name);
										if (Nullable)
											CSharp.Append(".Value");
										CSharp.AppendLine(");");
									}
									break;
							}
						}

						if (Nullable)
						{
							CSharp.Append(Indent);
							CSharp.AppendLine("}");
						}
					}

					if (HasDefaultValue)
						CSharp.AppendLine("\t\t\t}");
				}

				CSharp.AppendLine();
				if (this.debug)
					CSharp.AppendLine("\t\t\tConsole.Out.WriteLine();");

				CSharp.AppendLine("\t\t\tWriter.WriteVariableLengthUInt64(0);");

				CSharp.AppendLine();
				CSharp.AppendLine("\t\t\tif (!Embedded)");
				CSharp.AppendLine("\t\t\t{");
				if (this.debug)
					CSharp.AppendLine("\t\t\t\tConsole.Out.WriteLine();");

				if (ObjectIdMemberType is null)
					CSharp.AppendLine("\t\t\t\tWriterBak.Write(Waher.Persistence.Files.ObjectBTreeFile.CreateDatabaseGUID());");
				else
				{
					if (ObjectIdMemberType == typeof(Guid))
					{
						CSharp.AppendLine("\t\t\t\tif (!ObjectId.Equals(Guid.Empty))");
						CSharp.AppendLine("\t\t\t\t\tWriterBak.Write(ObjectId);");
					}
					else if (ObjectIdMemberType == typeof(string))
					{
						CSharp.AppendLine("\t\t\t\tif (!string.IsNullOrEmpty(ObjectId))");
						CSharp.AppendLine("\t\t\t\t\tWriterBak.Write(new Guid(ObjectId));");
					}
					else if (ObjectIdMemberType == typeof(byte[]))
					{
						CSharp.AppendLine("\t\t\t\tif (!(ObjectId is null))");
						CSharp.AppendLine("\t\t\t\t\tWriterBak.Write(new Guid(ObjectId));");
					}
					else
						throw new Exception("Invalid Object ID type.");

					CSharp.AppendLine("\t\t\t\telse");
					CSharp.AppendLine("\t\t\t\t{");
					CSharp.AppendLine("\t\t\t\t\tGuid NewObjectId = Waher.Persistence.Files.ObjectBTreeFile.CreateDatabaseGUID();");
					CSharp.AppendLine("\t\t\t\t\tWriterBak.Write(NewObjectId);");

					if (ObjectIdMemberType == typeof(Guid))
						CSharp.AppendLine("\t\t\t\t\tValue." + ObjectIdMember.Name + " = NewObjectId;");
					else if (ObjectIdMemberType == typeof(string))
						CSharp.AppendLine("\t\t\t\t\tValue." + ObjectIdMember.Name + " = NewObjectId.ToString();");
					else if (ObjectIdMemberType == typeof(byte[]))
						CSharp.AppendLine("\t\t\t\t\tValue." + ObjectIdMember.Name + " = NewObjectId.ToByteArray();");

					CSharp.AppendLine("\t\t\t\t}");
				}

				CSharp.AppendLine();
				CSharp.AppendLine("\t\t\t\tbyte[] Bin = Writer.GetSerialization();");
				CSharp.AppendLine();
				CSharp.AppendLine("\t\t\t\tWriterBak.WriteVariableLengthUInt64((ulong)Bin.Length);");
				CSharp.AppendLine("\t\t\t\tWriterBak.WriteRaw(Bin);");

				CSharp.AppendLine("\t\t\t}");
				CSharp.AppendLine("\t\t}");

				CSharp.AppendLine();
				CSharp.AppendLine("\t\tpublic override bool TryGetFieldValue(string FieldName, object UntypedObject, out object Value)");
				CSharp.AppendLine("\t\t{");
				CSharp.AppendLine("\t\t\t" + TypeName + " Object = (" + TypeName + ")UntypedObject;");
				CSharp.AppendLine();
				CSharp.AppendLine("\t\t\tswitch (FieldName)");
				CSharp.AppendLine("\t\t\t{");

				foreach (string MemberName in this.members.Keys)
				{
					CSharp.AppendLine("\t\t\t\tcase \"" + MemberName + "\":");
					CSharp.AppendLine("\t\t\t\t\tValue = Object." + MemberName + ";");
					CSharp.AppendLine("\t\t\t\t\treturn true;");
					CSharp.AppendLine();
				}

				CSharp.AppendLine("\t\t\t\tdefault:");
				CSharp.AppendLine("\t\t\t\t\tValue = null;");
				CSharp.AppendLine("\t\t\t\t\treturn false;");
				CSharp.AppendLine("\t\t\t}");
				CSharp.AppendLine("\t\t}");

				CSharp.AppendLine("\t}");
				CSharp.AppendLine("}");

				string CSharpCode = CSharp.ToString();

				Dictionary<string, bool> Dependencies = new Dictionary<string, bool>()
				{
					{ GetLocation(typeof(object)), true },
					{ Path.Combine(Path.GetDirectoryName(GetLocation(typeof(object))), "System.Runtime.dll"), true },
					{ Path.Combine(Path.GetDirectoryName(GetLocation(typeof(Encoding))), "System.Text.Encoding.dll"), true },
					{ Path.Combine(Path.GetDirectoryName(GetLocation(typeof(MemoryStream))), "System.IO.dll"), true },
					{ Path.Combine(Path.GetDirectoryName(GetLocation(typeof(MemoryStream))), "System.Runtime.Extensions.dll"), true },
					{ Path.Combine(Path.GetDirectoryName(GetLocation(typeof(Task))), "System.Threading.Tasks.dll"), true },
					{ GetLocation(typeof(Types)), true },
					{ GetLocation(typeof(Database)), true },
					{ GetLocation(typeof(ObjectSerializer)), true },
					{ GetLocation(typeof(MultiReadSingleWriteObject)), true }
				};

				if (this.debug)
					Dependencies[GetLocation(typeof(Console))] = true;

				System.Reflection.TypeInfo LoopInfo;
				Type Loop = Type;
				string s = Path.Combine(Path.GetDirectoryName(GetLocation(typeof(object))), "netstandard.dll");

				if (File.Exists(s))
					Dependencies[s] = true;

				while (!(Loop is null))
				{
					LoopInfo = Loop.GetTypeInfo();
					Dependencies[GetLocation(Loop)] = true;

					foreach (Type Interface in LoopInfo.ImplementedInterfaces)
					{
						s = GetLocation(Interface);
						Dependencies[s] = true;
					}

					foreach (MemberInfo MI2 in LoopInfo.DeclaredMembers)
					{
						FI = MI2 as FieldInfo;
						if (!(FI is null) && !((s = GetLocation(FI.FieldType)).EndsWith("mscorlib.dll") || s.EndsWith("System.Runtime.dll") || s.EndsWith("System.Private.CoreLib.dll")))
							Dependencies[s] = true;

						PI = MI2 as PropertyInfo;
						if (!(PI is null) && !((s = GetLocation(PI.PropertyType)).EndsWith("mscorlib.dll") || s.EndsWith("System.Runtime.dll") || s.EndsWith("System.Private.CoreLib.dll")))
							Dependencies[s] = true;
					}

					Loop = LoopInfo.BaseType;
					if (Loop == typeof(object))
						break;
				}

				List<MetadataReference> References = new List<MetadataReference>();

				foreach (string Location in Dependencies.Keys)
				{
					if (!string.IsNullOrEmpty(Location))
						References.Add(MetadataReference.CreateFromFile(Location));
				}

				CSharpCompilation Compilation = CSharpCompilation.Create("WPFA." + this.type.FullName,
					new SyntaxTree[] { CSharpSyntaxTree.ParseText(CSharpCode) },
					References, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

				MemoryStream Output = new MemoryStream();
				MemoryStream PdbOutput = new MemoryStream();

				EmitResult CompilerResults = Compilation.Emit(Output, pdbStream: PdbOutput);

				if (!CompilerResults.Success)
				{
					StringBuilder sb = new StringBuilder();

					foreach (Diagnostic Error in CompilerResults.Diagnostics)
					{
						sb.AppendLine();
						sb.Append(Error.Location.ToString());
						sb.Append(": ");
						sb.Append(Error.GetMessage());
					}

					sb.AppendLine();
					sb.AppendLine();
					sb.AppendLine("Code generated:");
					sb.AppendLine();
					sb.AppendLine(CSharpCode);

					throw new Exception("Unable to serialize objects of type " + Type.FullName +
						". When generating serialization class, the following compiler errors were reported:\r\n" + sb.ToString());
				}
				Output.Position = 0;
				Assembly A;

				try
				{
					A = AssemblyLoadContext.Default.LoadFromStream(Output, PdbOutput);
					Type T = A.GetType(Type.Namespace + ".Binary.BinarySerializer" + TypeName + this.provider.Id);
					this.customSerializer = (IObjectSerializer)Activator.CreateInstance(T, this.provider);
				}
				catch (FileLoadException)
				{
					this.customSerializer = new ObjectSerializer(Type, Provider, Debug, false);
				}
			}
			else
			{
#endif
				Member Member;
				FieldInfo FI;
				PropertyInfo PI;
				MethodInfo MI;
				object DefaultValue;
				int NrDefault = 0;
				bool Ignore;

				foreach (MemberInfo MemberInfo in GetMembers(this.typeInfo))
				{
					if (!((FI = MemberInfo as FieldInfo) is null))
					{
						if (!FI.IsPublic || FI.IsStatic)
							continue;

						PI = null;
						Member = new FieldMember(FI, this.provider.GetFieldCode(this.collectionName, FI.Name));
					}
					else if (!((PI = MemberInfo as PropertyInfo) is null))
					{
						if ((MI = PI.GetMethod) is null || !MI.IsPublic || MI.IsStatic)
							continue;

						if ((MI = PI.SetMethod) is null || !MI.IsPublic || MI.IsStatic)
							continue;

						if (PI.GetIndexParameters().Length > 0)
							continue;

						Member = new PropertyMember(PI, this.provider.GetFieldCode(this.collectionName, PI.Name));
					}
					else
						continue;

					Ignore = false;

					foreach (Attribute Attr in MemberInfo.GetCustomAttributes(true))
					{
						if (Attr is IgnoreMemberAttribute)
						{
							Ignore = true;
							break;
						}
						else if (Attr is DefaultValueAttribute DefaultValueAttribute)
						{
							DefaultValue = DefaultValueAttribute.Value;
							NrDefault++;

							if (DefaultValue is string s && Member.MemberType == typeof(CaseInsensitiveString))
								DefaultValue = new CaseInsensitiveString(s);

							if (!(DefaultValue is null) && DefaultValue.GetType() != Member.MemberType)
								DefaultValue = Convert.ChangeType(DefaultValue, Member.MemberType);

							Member.DefaultValue = DefaultValue;
						}
						else if (Attr is ByReferenceAttribute)
							Member.ByReference = true;
						else if (Attr is ObjectIdAttribute)
						{
							this.objectIdMember = Member;
							Ignore = true;
						}
					}

					if (Ignore)
						continue;

					this.membersByName[Member.Name] = Member;
					this.membersByFieldCode[Member.FieldCode] = Member;
					this.membersOrdered.AddLast(Member);

					if (Member.IsNestedObject)
						Member.NestedSerializer = this.provider.GetObjectSerializer(Member.MemberType);
				}
#if NETSTANDARD1_5
			}
#endif
		}

#if NETSTANDARD1_5
		private static string GetLocation(Type T)
		{
			System.Reflection.TypeInfo TI = T.GetTypeInfo();
			string s = TI.Assembly.Location;

			if (!string.IsNullOrEmpty(s))
				return s;

			return Path.Combine(Path.GetDirectoryName(GetLocation(typeof(Database))), TI.Module.ScopeName);
		}
#endif

		private static string GenericParameterName(Type Type)
		{
			if (Type.GetTypeInfo().IsGenericType)
			{
				Type GT = Type.GetGenericTypeDefinition();
				if (GT == typeof(Nullable<>))
				{
					Type = Type.GenericTypeArguments[0];
					return Type.FullName + "?";
				}
			}

			return Type.FullName;
		}

		/// <summary>
		/// Escapes a string to be enclosed between double quotes.
		/// </summary>
		/// <param name="s"></param>
		/// <returns>String with special characters escaped.</returns>
		public static string Escape(string s)
		{
			if (s is null)
				return string.Empty;

			if (s.IndexOfAny(specialCharacters) < 0)
				return s;

			return s.Replace("\\", "\\\\").
				Replace("\n", "\\n").
				Replace("\r", "\\r").
				Replace("\t", "\\t").
				Replace("\f", "\\f").
				Replace("\b", "\\b").
				Replace("\a", "\\a").
				Replace("\"", "\\\"");
		}

		private static readonly char[] specialCharacters = new char[] { '\\', '\n', '\r', '\t', '\f', '\b', '\a', '"' };

		/// <summary>
		/// Name of collection objects of this type is to be stored in, if available. If not available, this property returns null.
		/// </summary>
		/// <param name="Object">Object in the current context. If null, the default collection name is requested.</param>
		public virtual string CollectionName(object Object)
		{
			return this.collectionName;
		}

		/// <summary>
		/// Gets the type of the value.
		/// </summary>
		public Type ValueType
		{
			get
			{
				return this.type;
			}
		}

		/// <summary>
		/// Array of indices defined for the underlying type.
		/// </summary>
		public string[][] Indices
		{
			get { return this.indices; }
		}

		/// <summary>
		/// If index files have been checked and created.
		/// </summary>
		internal bool IndicesCreated
		{
			get { return this.indicesCreated; }
			set { this.indicesCreated = value; }
		}

		/// <summary>
		/// If the type is nullable.
		/// </summary>
		public bool IsNullable
		{
			get { return this.isNullable; }
		}

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <param name="Reader">Binary deserializer.</param>
		/// <param name="DataType">Data type of object.</param>
		/// <param name="Embedded">If the object is embedded in another object.</param>
		/// <returns>A deserialized value.</returns>
		public virtual object Deserialize(BinaryDeserializer Reader, uint? DataType, bool Embedded)
		{
#if NETSTANDARD1_5
			if (this.compiled)
				return this.customSerializer.Deserialize(Reader, DataType, Embedded);
			else
			{
#endif
				uint FieldDataType;
				ulong FieldCode;
				object Result;
				StreamBookmark Bookmark = Reader.GetBookmark();
				uint? DataTypeBak = DataType;
				Guid ObjectId = Embedded ? Guid.Empty : Reader.ReadGuid();
				ulong ContentLen = Embedded ? 0 : Reader.ReadVariableLengthUInt64();

				if (!DataType.HasValue)
				{
					DataType = Reader.ReadBits(6);
					if (DataType.Value == TYPE_NULL)
						return null;
				}

				FieldCode = Reader.ReadVariableLengthUInt64();
				if (this.typeNameSerialization != TypeNameSerialization.None)
				{
					string TypeName = this.provider.GetFieldName(this.collectionName, FieldCode);
					if (this.typeNameSerialization == TypeNameSerialization.LocalName)
						TypeName = this.type.Namespace + "." + TypeName;

					Type DesiredType = Waher.Runtime.Inventory.Types.GetType(TypeName);
					if (DesiredType is null)
						DesiredType = typeof(GenericObject);

					if (DesiredType != this.type)
					{
						IObjectSerializer Serializer2 = this.provider.GetObjectSerializer(DesiredType);
						Reader.SetBookmark(Bookmark);
						return Serializer2.Deserialize(Reader, DataTypeBak, Embedded);
					}
				}

				if (this.typeInfo.IsAbstract)
					throw new Exception("Unable to create an instance of an abstract class.");

				if (Embedded)
					Reader.ReadVariableLengthUInt64();  // Collection name

				if (DataType.Value != TYPE_OBJECT)
					throw new Exception("Object expected.");

				Result = Activator.CreateInstance(this.type);

				if (!(this.objectIdMember is null))
				{
					switch (this.objectIdMember.MemberFieldDataTypeCode)
					{
						case TYPE_GUID:
							this.objectIdMember.Set(Result, ObjectId);
							break;

						case TYPE_STRING:
							this.objectIdMember.Set(Result, ObjectId.ToString());
							break;

						case TYPE_BYTEARRAY:
							this.objectIdMember.Set(Result, ObjectId.ToByteArray());
							break;

						default:
							throw new Exception("Type not supported for Object ID fields: " + this.objectIdMember.MemberType.FullName);
					}
				}

				while ((FieldCode = Reader.ReadVariableLengthUInt64()) != 0)
				{
					FieldDataType = Reader.ReadBits(6);
					if (this.membersByFieldCode.TryGetValue(FieldCode, out Member Member))
					{
						switch (Member.MemberFieldDataTypeCode)
						{
							case TYPE_BOOLEAN:
								if (Member.Nullable)
									Member.Set(Result, GeneratedObjectSerializerBase.ReadNullableBoolean(Reader, FieldDataType));
								else
									Member.Set(Result, GeneratedObjectSerializerBase.ReadBoolean(Reader, FieldDataType));
								break;

							case TYPE_BYTE:
								if (Member.Nullable)
									Member.Set(Result, GeneratedObjectSerializerBase.ReadNullableByte(Reader, FieldDataType));
								else
									Member.Set(Result, GeneratedObjectSerializerBase.ReadByte(Reader, FieldDataType));
								break;

							case TYPE_CHAR:
								if (Member.Nullable)
									Member.Set(Result, GeneratedObjectSerializerBase.ReadNullableChar(Reader, FieldDataType));
								else
									Member.Set(Result, GeneratedObjectSerializerBase.ReadChar(Reader, FieldDataType));
								break;

							case TYPE_DATETIME:
								if (Member.Nullable)
									Member.Set(Result, GeneratedObjectSerializerBase.ReadNullableDateTime(Reader, FieldDataType));
								else
									Member.Set(Result, GeneratedObjectSerializerBase.ReadDateTime(Reader, FieldDataType));
								break;

							case TYPE_DATETIMEOFFSET:
								if (Member.Nullable)
									Member.Set(Result, GeneratedObjectSerializerBase.ReadNullableDateTimeOffset(Reader, FieldDataType));
								else
									Member.Set(Result, GeneratedObjectSerializerBase.ReadDateTimeOffset(Reader, FieldDataType));
								break;

							case TYPE_DECIMAL:
								if (Member.Nullable)
									Member.Set(Result, GeneratedObjectSerializerBase.ReadNullableDecimal(Reader, FieldDataType));
								else
									Member.Set(Result, GeneratedObjectSerializerBase.ReadDecimal(Reader, FieldDataType));
								break;

							case TYPE_DOUBLE:
								if (Member.Nullable)
									Member.Set(Result, GeneratedObjectSerializerBase.ReadNullableDouble(Reader, FieldDataType));
								else
									Member.Set(Result, GeneratedObjectSerializerBase.ReadDouble(Reader, FieldDataType));
								break;

							case TYPE_INT16:
								if (Member.Nullable)
									Member.Set(Result, GeneratedObjectSerializerBase.ReadNullableInt16(Reader, FieldDataType));
								else
									Member.Set(Result, GeneratedObjectSerializerBase.ReadInt16(Reader, FieldDataType));
								break;

							case TYPE_INT32:
								if (Member.IsEnum)
								{
									if (Member.Nullable)
										Member.Set(Result, GeneratedObjectSerializerBase.ReadNullableEnum(Reader, FieldDataType, Member.MemberType));
									else
										Member.Set(Result, Enum.ToObject(Member.MemberType, GeneratedObjectSerializerBase.ReadInt32(Reader, FieldDataType)));
								}
								else
								{
									if (Member.Nullable)
										Member.Set(Result, GeneratedObjectSerializerBase.ReadNullableInt32(Reader, FieldDataType));
									else
										Member.Set(Result, GeneratedObjectSerializerBase.ReadInt32(Reader, FieldDataType));
								}
								break;

							case TYPE_INT64:
								if (Member.Nullable)
									Member.Set(Result, GeneratedObjectSerializerBase.ReadNullableInt64(Reader, FieldDataType));
								else
									Member.Set(Result, GeneratedObjectSerializerBase.ReadInt64(Reader, FieldDataType));
								break;

							case TYPE_SBYTE:
								if (Member.Nullable)
									Member.Set(Result, GeneratedObjectSerializerBase.ReadNullableSByte(Reader, FieldDataType));
								else
									Member.Set(Result, GeneratedObjectSerializerBase.ReadSByte(Reader, FieldDataType));
								break;

							case TYPE_SINGLE:
								if (Member.Nullable)
									Member.Set(Result, GeneratedObjectSerializerBase.ReadNullableSingle(Reader, FieldDataType));
								else
									Member.Set(Result, GeneratedObjectSerializerBase.ReadSingle(Reader, FieldDataType));
								break;

							case TYPE_STRING:
								Member.Set(Result, GeneratedObjectSerializerBase.ReadString(Reader, FieldDataType));
								break;

							case TYPE_CI_STRING:
								Member.Set(Result, new CaseInsensitiveString(GeneratedObjectSerializerBase.ReadString(Reader, FieldDataType)));
								break;

							case TYPE_UINT16:
								if (Member.Nullable)
									Member.Set(Result, GeneratedObjectSerializerBase.ReadNullableUInt16(Reader, FieldDataType));
								else
									Member.Set(Result, GeneratedObjectSerializerBase.ReadUInt16(Reader, FieldDataType));
								break;

							case TYPE_UINT32:
								if (Member.Nullable)
									Member.Set(Result, GeneratedObjectSerializerBase.ReadNullableUInt32(Reader, FieldDataType));
								else
									Member.Set(Result, GeneratedObjectSerializerBase.ReadUInt32(Reader, FieldDataType));
								break;

							case TYPE_UINT64:
								if (Member.Nullable)
									Member.Set(Result, GeneratedObjectSerializerBase.ReadNullableUInt64(Reader, FieldDataType));
								else
									Member.Set(Result, GeneratedObjectSerializerBase.ReadUInt64(Reader, FieldDataType));
								break;

							case TYPE_TIMESPAN:
								if (Member.Nullable)
									Member.Set(Result, GeneratedObjectSerializerBase.ReadNullableTimeSpan(Reader, FieldDataType));
								else
									Member.Set(Result, GeneratedObjectSerializerBase.ReadTimeSpan(Reader, FieldDataType));
								break;

							case TYPE_GUID:
								if (Member.Nullable)
									Member.Set(Result, GeneratedObjectSerializerBase.ReadNullableGuid(Reader, FieldDataType));
								else
									Member.Set(Result, GeneratedObjectSerializerBase.ReadGuid(Reader, FieldDataType));
								break;

							case TYPE_NULL:
							default:
								throw new Exception("Invalid member type: " + Member.MemberType.FullName);

							case TYPE_ARRAY:
								Member.Set(Result, GeneratedObjectSerializerBase.ReadArray(Member.MemberType.GetElementType(), this.provider, Reader, FieldDataType));
								break;

							case TYPE_BYTEARRAY:
								Member.Set(Result, Reader.ReadByteArray());
								break;

							case TYPE_ENUM:
								switch (FieldDataType)
								{
									case TYPE_BOOLEAN:
										Member.Set(Result, Enum.ToObject(Member.MemberType, Reader.ReadBoolean() ? 1 : 0));
										break;

									case TYPE_BYTE:
										Member.Set(Result, Enum.ToObject(Member.MemberType, (int)Reader.ReadByte()));
										break;

									case TYPE_INT16:
										Member.Set(Result, Enum.ToObject(Member.MemberType, (int)Reader.ReadInt16()));
										break;

									case TYPE_INT32:
										Member.Set(Result, Enum.ToObject(Member.MemberType, Reader.ReadInt32()));
										break;

									case TYPE_INT64:
										Member.Set(Result, Enum.ToObject(Member.MemberType, Reader.ReadInt64()));
										break;

									case TYPE_SBYTE:
										Member.Set(Result, Enum.ToObject(Member.MemberType, (int)Reader.ReadSByte()));
										break;

									case TYPE_UINT16:
										Member.Set(Result, Enum.ToObject(Member.MemberType, (int)Reader.ReadUInt16()));
										break;

									case TYPE_UINT32:
										Member.Set(Result, Enum.ToObject(Member.MemberType, Reader.ReadUInt32()));
										break;

									case TYPE_UINT64:
										Member.Set(Result, Enum.ToObject(Member.MemberType, Reader.ReadUInt64()));
										break;

									case TYPE_DECIMAL:
										Member.Set(Result, Enum.ToObject(Member.MemberType, (int)Reader.ReadDecimal()));
										break;

									case TYPE_DOUBLE:
										Member.Set(Result, Enum.ToObject(Member.MemberType, (int)Reader.ReadDouble()));
										break;

									case TYPE_SINGLE:
										Member.Set(Result, Enum.ToObject(Member.MemberType, (int)Reader.ReadSingle()));
										break;

									case TYPE_STRING:
									case TYPE_CI_STRING:
										Member.Set(Result, Enum.Parse(Member.MemberType, Reader.ReadString()));
										break;

									case TYPE_ENUM:
										Member.Set(Result, Reader.ReadEnum(Member.MemberType));
										break;

									case TYPE_NULL:
										Member.Set(Result, null);
										break;

									default:
										throw new Exception("Unable to set " + Member.Name + ". Expected an enumeration value, but was a " +
											FilesProvider.GetFieldDataTypeName(FieldDataType) + ".");
								}
								break;

							case TYPE_OBJECT:
								if (Member.ByReference)
								{
									switch (FieldDataType)
									{
										case TYPE_GUID:
											Guid RefObjectId = Reader.ReadGuid();
											Task<object> SetTask = this.provider.LoadObject(Member.MemberType, RefObjectId,
												(EmbeddedValue) => Member.Set(Result, EmbeddedValue));

											if (!SetTask.Wait(10000))
												throw new Exception("Unable to load referenced object. Database timed out.");

											Member.Set(Result, SetTask.Result);
											break;

										case TYPE_NULL:
											Member.Set(Result, null);
											break;

										default:
											throw new Exception("Object ID expected for " + Member.Name + ".");
									}
								}
								else
								{
									switch (FieldDataType)
									{
										case TYPE_OBJECT:
											Member.Set(Result, Member.NestedSerializer.Deserialize(Reader, FieldDataType, true));
											break;

										case TYPE_NULL:
											Member.Set(Result, null);
											break;

										case TYPE_BOOLEAN:
											Member.Set(Result, Reader.ReadBoolean());
											break;

										case TYPE_BYTE:
											Member.Set(Result, Reader.ReadByte());
											break;

										case TYPE_INT16:
											Member.Set(Result, Reader.ReadInt16());
											break;

										case TYPE_INT32:
											Member.Set(Result, Reader.ReadInt32());
											break;

										case TYPE_INT64:
											Member.Set(Result, Reader.ReadInt64());
											break;

										case TYPE_SBYTE:
											Member.Set(Result, Reader.ReadSByte());
											break;

										case TYPE_UINT16:
											Member.Set(Result, Reader.ReadUInt16());
											break;

										case TYPE_UINT32:
											Member.Set(Result, Reader.ReadUInt32());
											break;

										case TYPE_UINT64:
											Member.Set(Result, Reader.ReadUInt64());
											break;

										case TYPE_DECIMAL:
											Member.Set(Result, Reader.ReadDecimal());
											break;

										case TYPE_DOUBLE:
											Member.Set(Result, Reader.ReadDouble());
											break;

										case TYPE_SINGLE:
											Member.Set(Result, Reader.ReadSingle());
											break;

										case TYPE_DATETIME:
											Member.Set(Result, Reader.ReadDateTime());
											break;

										case TYPE_DATETIMEOFFSET:
											Member.Set(Result, Reader.ReadDateTimeOffset());
											break;

										case TYPE_TIMESPAN:
											Member.Set(Result, Reader.ReadTimeSpan());
											break;

										case TYPE_CHAR:
											Member.Set(Result, Reader.ReadChar());
											break;

										case TYPE_STRING:
											Member.Set(Result, Reader.ReadString());
											break;

										case TYPE_CI_STRING:
											Member.Set(Result, new CaseInsensitiveString(Reader.ReadString()));
											break;

										case TYPE_BYTEARRAY:
											Member.Set(Result, Reader.ReadByteArray());
											break;

										case TYPE_GUID:
											Member.Set(Result, Reader.ReadGuid());
											break;

										case TYPE_ENUM:
											Member.Set(Result, Reader.ReadString());
											break;

										case TYPE_ARRAY:
											Member.Set(Result, GeneratedObjectSerializerBase.ReadArray(typeof(Waher.Persistence.Serialization.GenericObject), this.provider, Reader, FieldDataType));
											break;

										default:
											throw new Exception("Object expected for " + Member.Name + ". Data Type read: " + FilesProvider.GetFieldDataTypeName(FieldDataType));
									}
								}
								break;
						}
					}
					else
						throw new Exception("Field name not recognized: " + this.provider.GetFieldName(this.collectionName, FieldCode));
				}

				return Result;
#if NETSTANDARD1_5
			}
#endif
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Writer">Binary serializer.</param>
		/// <param name="WriteTypeCode">If a type code is to be written.</param>
		/// <param name="Embedded">If the object is embedded in another object.</param>
		/// <param name="Value">Value to serialize.</param>
		public virtual void Serialize(BinarySerializer Writer, bool WriteTypeCode, bool Embedded, object Value)
		{
#if NETSTANDARD1_5
			if (this.compiled)
				this.customSerializer.Serialize(Writer, WriteTypeCode, Embedded, Value);
			else
			{
#endif
				BinarySerializer WriterBak = Writer;

				if (!Embedded)
					Writer = new BinarySerializer(Writer.CollectionName, Writer.Encoding, this.debug);

				if (WriteTypeCode)
				{
					if (Value is null)
					{
						Writer.WriteBits(TYPE_NULL, 6);
						return;
					}
					else
						Writer.WriteBits(TYPE_OBJECT, 6);
				}
				else if (Value is null)
					throw new NullReferenceException("Value cannot be null.");

				if (this.typeNameSerialization == TypeNameSerialization.None)
					Writer.WriteVariableLengthUInt64(0);
				else
				{
					if (this.typeNameSerialization == TypeNameSerialization.LocalName)
						Writer.WriteVariableLengthUInt64(this.provider.GetFieldCode(this.collectionName, this.type.Name));
					else
						Writer.WriteVariableLengthUInt64(this.provider.GetFieldCode(this.collectionName, this.type.FullName));
				}

				if (Embedded)
				{
					if (string.IsNullOrEmpty(this.collectionName))
						Writer.WriteVariableLengthUInt64(this.provider.GetFieldCode(null, this.provider.DefaultCollectionName));
					else
						Writer.WriteVariableLengthUInt64(this.provider.GetFieldCode(null, this.collectionName));
				}

				foreach (Member Member in this.membersOrdered)
				{
					if (Member.HasDefaultValue(Value))
						continue;

					Writer.WriteVariableLengthUInt64(Member.FieldCode);

					object MemberValue = Member.Get(Value);
					if (MemberValue is null)
						Writer.WriteBits(TYPE_NULL, 6);
					else
					{
						switch (Member.MemberFieldDataTypeCode)
						{
							case TYPE_BOOLEAN:
								Writer.WriteBits(TYPE_BOOLEAN, 6);
								Writer.Write((bool)MemberValue);
								break;

							case TYPE_BYTE:
								Writer.WriteBits(TYPE_BYTE, 6);
								Writer.Write((byte)MemberValue);
								break;

							case TYPE_CHAR:
								Writer.WriteBits(TYPE_CHAR, 6);
								Writer.Write((char)MemberValue);
								break;

							case TYPE_DATETIME:
								Writer.WriteBits(TYPE_DATETIME, 6);
								Writer.Write((DateTime)MemberValue);
								break;

							case TYPE_DATETIMEOFFSET:
								Writer.WriteBits(TYPE_DATETIMEOFFSET, 6);
								Writer.Write((DateTimeOffset)MemberValue);
								break;

							case TYPE_DECIMAL:
								Writer.WriteBits(TYPE_DECIMAL, 6);
								Writer.Write((decimal)MemberValue);
								break;

							case TYPE_DOUBLE:
								Writer.WriteBits(TYPE_DOUBLE, 6);
								Writer.Write((double)MemberValue);
								break;

							case TYPE_INT16:
								Writer.WriteBits(TYPE_INT16, 6);
								Writer.Write((short)MemberValue);
								break;

							case TYPE_INT32:
								Writer.WriteBits(TYPE_INT32, 6);
								Writer.Write((int)MemberValue);
								break;

							case TYPE_INT64:
								Writer.WriteBits(TYPE_INT64, 6);
								Writer.Write((long)MemberValue);
								break;

							case TYPE_SBYTE:
								Writer.WriteBits(TYPE_SBYTE, 6);
								Writer.Write((sbyte)MemberValue);
								break;

							case TYPE_SINGLE:
								Writer.WriteBits(TYPE_SINGLE, 6);
								Writer.Write((float)MemberValue);
								break;

							case TYPE_STRING:
								Writer.WriteBits(TYPE_STRING, 6);
								Writer.Write((string)MemberValue);
								break;

							case TYPE_CI_STRING:
								Writer.WriteBits(TYPE_CI_STRING, 6);
								Writer.Write(((CaseInsensitiveString)MemberValue).Value);
								break;

							case TYPE_UINT16:
								Writer.WriteBits(TYPE_UINT16, 6);
								Writer.Write((ushort)MemberValue);
								break;

							case TYPE_UINT32:
								Writer.WriteBits(TYPE_UINT32, 6);
								Writer.Write((uint)MemberValue);
								break;

							case TYPE_UINT64:
								Writer.WriteBits(TYPE_UINT64, 6);
								Writer.Write((ulong)MemberValue);
								break;

							case TYPE_TIMESPAN:
								Writer.WriteBits(TYPE_TIMESPAN, 6);
								Writer.Write((TimeSpan)MemberValue);
								break;

							case TYPE_GUID:
								Writer.WriteBits(TYPE_GUID, 6);
								Writer.Write((Guid)MemberValue);
								break;

							case TYPE_NULL:
							default:
								throw new Exception("Invalid member type: " + Member.MemberType.FullName);

							case TYPE_ARRAY:
								GeneratedObjectSerializerBase.WriteArray(Member.MemberType.GetElementType(), this.provider, Writer, (Array)MemberValue);
								break;

							case TYPE_BYTEARRAY:
								Writer.WriteBits(TYPE_BYTEARRAY, 6);
								Writer.Write((byte[])MemberValue);
								break;

							case TYPE_ENUM:
								if (Member.HasFlags)
								{
									Writer.WriteBits(TYPE_INT32, 6);
									Writer.Write(Convert.ToInt32(MemberValue));
								}
								else
								{
									Writer.WriteBits(TYPE_ENUM, 6);
									Writer.Write((Enum)MemberValue);
								}
								break;

							case TYPE_OBJECT:
								if (Member.ByReference)
								{
									if (Member.NestedSerializer is ObjectSerializer ObjectSerializer)
									{
										Writer.WriteBits(TYPE_GUID, 6);

										Task<Guid> WriteTask = ObjectSerializer.GetObjectId(MemberValue, true);
										FilesProvider.Wait(WriteTask, this.provider.TimeoutMilliseconds);
										Writer.Write(WriteTask.Result);
									}
									else
										throw new Exception("Objects of type " + Member.MemberType.FullName + " cannot be stored by reference.");
								}
								else
									Member.NestedSerializer.Serialize(Writer, true, true, MemberValue);
								break;
						}
					}
				}

				Writer.WriteVariableLengthUInt64(0);

				if (!Embedded)
				{
					if (this.objectIdMember is null)
						WriterBak.Write(Waher.Persistence.Files.ObjectBTreeFile.CreateDatabaseGUID());
					else
					{
						object ObjectId = this.objectIdMember.Get(Value);
						int ObjectIdType = 0;
						bool HasGuid = false;

						if (this.objectIdMember.MemberFieldDataTypeCode == TYPE_GUID)
						{
							ObjectIdType = 1;
							if (!(ObjectId is null) && !ObjectId.Equals(Guid.Empty))
							{
								WriterBak.Write((Guid)ObjectId);
								HasGuid = true;
							}
						}
						else if (this.objectIdMember.MemberFieldDataTypeCode == TYPE_STRING)
						{
							string s = (string)ObjectId;
							ObjectIdType = 2;
							if (!string.IsNullOrEmpty(s))
							{
								WriterBak.Write(new Guid(s));
								HasGuid = true;
							}
						}
						else if (this.objectIdMember.MemberFieldDataTypeCode == TYPE_BYTEARRAY)
						{
							byte[] bin = (byte[])ObjectId;
							ObjectIdType = 3;
							if (!(bin is null))
							{
								WriterBak.Write(new Guid(bin));
								HasGuid = true;
							}
						}
						else
							throw new Exception("Invalid Object ID type.");

						if (!HasGuid)
						{
							Guid NewObjectId = Waher.Persistence.Files.ObjectBTreeFile.CreateDatabaseGUID();
							WriterBak.Write(NewObjectId);

							switch (ObjectIdType)
							{
								case 1:
									this.objectIdMember.Set(Value, NewObjectId);
									break;

								case 2:
									this.objectIdMember.Set(Value, NewObjectId.ToString());
									break;

								case 3:
									this.objectIdMember.Set(Value, NewObjectId.ToByteArray());
									break;
							}
						}
					}

					byte[] Bin = Writer.GetSerialization();

					WriterBak.WriteVariableLengthUInt64((ulong)Bin.Length);
					WriterBak.WriteRaw(Bin);
				}
#if NETSTANDARD1_5
			}
#endif
		}

		/// <summary>
		/// Mamber name of the field or property holding the Object ID, if any. If there are no such member, this property returns null.
		/// </summary>
		public virtual string ObjectIdMemberName
		{
			get
			{
#if NETSTANDARD1_5
				if (this.compiled)
				{
					if (!(this.objectIdFieldInfo is null))
						return this.objectIdFieldInfo.Name;
					else if (!(this.objectIdPropertyInfo is null))
						return this.objectIdPropertyInfo.Name;
					else
						return null;
				}
				else
				{
#endif
					if (!(this.objectIdMember is null))
						return this.objectIdMember.Name;
					else
						return null;
#if NETSTANDARD1_5
				}
#endif
			}
		}

		/// <summary>
		/// If the class has an Object ID field.
		/// </summary>
		public virtual bool HasObjectIdField
		{
			get
			{
#if NETSTANDARD1_5
				if (this.compiled)
					return !(this.objectIdFieldInfo is null) || this.objectIdPropertyInfo != null;
				else
#endif
					return !(this.objectIdMember is null);
			}
		}

		/// <summary>
		/// If the class has an Object ID.
		/// </summary>
		/// <param name="Value">Object reference.</param>
		public virtual bool HasObjectId(object Value)
		{
			object ObjectId;

#if NETSTANDARD1_5
			if (this.compiled)
			{
				if (!(this.objectIdFieldInfo is null))
					ObjectId = this.objectIdFieldInfo.GetValue(Value);
				else if (!(this.objectIdPropertyInfo is null))
					ObjectId = this.objectIdPropertyInfo.GetValue(Value);
				else
					return false;
			}
			else
			{
#endif
				if (!(this.objectIdMember is null))
					ObjectId = this.objectIdMember.Get(Value);
				else
					return false;
#if NETSTANDARD1_5
			}
#endif

			if (ObjectId is null)
				return false;

			if (ObjectId is Guid && ObjectId.Equals(Guid.Empty))
				return false;

			return true;
		}

		/// <summary>
		/// Tries to set the object id of an object.
		/// </summary>
		/// <param name="Value">Object reference.</param>
		/// <param name="ObjectId">Object ID</param>
		/// <returns>If the object has an Object ID field or property that could be set.</returns>
		public virtual bool TrySetObjectId(object Value, Guid ObjectId)
		{
			Type MemberType;
			object Obj;

#if NETSTANDARD1_5
			if (this.compiled)
			{
				if (!(this.objectIdFieldInfo is null))
					MemberType = this.objectIdFieldInfo.FieldType;
				else if (!(this.objectIdPropertyInfo is null))
					MemberType = this.objectIdPropertyInfo.PropertyType;
				else
					return false;
			}
			else
			{
#endif
				if (!(this.objectIdMember is null))
					MemberType = this.objectIdMember.MemberType;
				else
					return false;
#if NETSTANDARD1_5
			}
#endif

			if (MemberType == typeof(Guid))
				Obj = ObjectId;
			else if (MemberType == typeof(string))
				Obj = ObjectId.ToString();
			else if (MemberType == typeof(byte[]))
				Obj = ObjectId.ToByteArray();
			else
				return false;

#if NETSTANDARD1_5
			if (this.compiled)
			{
				if (!(this.objectIdFieldInfo is null))
					this.objectIdFieldInfo.SetValue(Value, Obj);
				else
					this.objectIdPropertyInfo.SetValue(Value, Obj);
			}
			else
#endif
				this.objectIdMember.Set(Value, Obj);

			return true;
		}

		/// <summary>
		/// Gets the Object ID for a given object.
		/// </summary>
		/// <param name="Value">Object reference.</param>
		/// <param name="InsertIfNotFound">Insert object into database with new Object ID, if no Object ID is set.</param>
		/// <returns>Object ID for <paramref name="Value"/>.</returns>
		/// <exception cref="NotSupportedException">Thrown, if the corresponding class does not have an Object ID property, 
		/// or if the corresponding property type is not supported.</exception>
		public virtual async Task<Guid> GetObjectId(object Value, bool InsertIfNotFound)
		{
			object Obj;

#if NETSTANDARD1_5
			if (this.compiled)
			{
				if (!(this.objectIdFieldInfo is null))
					Obj = this.objectIdFieldInfo.GetValue(Value);
				else if (!(this.objectIdPropertyInfo is null))
					Obj = this.objectIdPropertyInfo.GetValue(Value);
				else
					throw new NotSupportedException("No Object ID member found in objects of type " + Value.GetType().FullName + ".");
			}
			else
			{
#endif
				if (!(this.objectIdMember is null))
					Obj = this.objectIdMember.Get(Value);
				else
					throw new NotSupportedException("No Object ID member found in objects of type " + Value.GetType().FullName + ".");
#if NETSTANDARD1_5
			}
#endif

			if (Obj is null || (Obj is Guid && Obj.Equals(Guid.Empty)))
			{
				if (!InsertIfNotFound)
					throw new Exception("Object has no Object ID defined.");

				Type ValueType = Value.GetType();
				ObjectSerializer Serializer = this.provider.GetObjectSerializerEx(ValueType);

				ObjectBTreeFile File = await this.provider.GetFile(this.collectionName);
				Guid ObjectId;
				Type T;

				if (await File.TryBeginWrite(0))
				{
					try
					{
						ObjectId = await File.SaveNewObjectLocked(Value, Serializer);
					}
					finally
					{
						await File.EndWrite();
					}

					foreach (IndexBTreeFile Index in File.Indices)
						await Index.SaveNewObject(ObjectId, Value, Serializer);
				}
				else
				{
					Tuple<Guid, Storage.BlockInfo> Rec = await File.PrepareObjectIdForSaveLocked(Value, Serializer);

					ObjectId = Rec.Item1;
					File.QueueForSave(Value, Serializer);
				}

#if NETSTANDARD1_5
				if (this.compiled)
				{
					if (!(this.objectIdFieldInfo is null))
						T = this.objectIdFieldInfo.FieldType;
					else
						T = this.objectIdPropertyInfo.PropertyType;
				}
				else
#endif
					T = this.objectIdMember.MemberType;

				if (T == typeof(Guid))
					Obj = ObjectId;
				else if (T == typeof(string))
					Obj = ObjectId.ToString();
				else if (T == typeof(byte[]))
					Obj = ObjectId.ToByteArray();
				else
					throw new NotSupportedException("Unsupported type for Object ID members: " + Obj.GetType().FullName);

#if NETSTANDARD1_5
				if (this.compiled)
				{
					if (!(this.objectIdFieldInfo is null))
						this.objectIdFieldInfo.SetValue(Value, Obj);
					else
						this.objectIdPropertyInfo.SetValue(Value, Obj);
				}
				else
#endif
					this.objectIdMember.Set(Value, Obj);

				return ObjectId;
			}
			else if (Obj is Guid Guid)
				return Guid;
			else if (Obj is string s)
				return new Guid(s);
			else if (Obj is byte[] Bin)
				return new Guid(Bin);
			else
				throw new NotSupportedException("Unsupported type for Object ID members: " + Obj.GetType().FullName);
		}

		/// <summary>
		/// Checks if a given field value corresponds to the default value for the corresponding field.
		/// </summary>
		/// <param name="FieldName">Name of field.</param>
		/// <param name="Value">Field value.</param>
		/// <returns>If the field value corresponds to the default value of the corresponding field.</returns>
		public virtual bool IsDefaultValue(string FieldName, object Value)
		{
			object Default;

#if NETSTANDARD1_5
			if (this.compiled)
			{
				if (!this.defaultValues.TryGetValue(FieldName, out Default))
					return false;
			}
			else
			{
#endif
				if (!this.membersByName.TryGetValue(FieldName, out Member Member))
					return false;

				Default = Member.DefaultValue;
#if NETSTANDARD1_5
			}
#endif
			if ((Value is null) ^ (Default is null))
				return false;

			if (Value is null)
				return true;

			return Default.Equals(Value);
		}

		/// <summary>
		/// Gets the value of a field or property of an object, given its name.
		/// </summary>
		/// <param name="FieldName">Name of field or property.</param>
		/// <param name="Object">Object.</param>
		/// <param name="Value">Corresponding field or property value, if found, or null otherwise.</param>
		/// <returns>If the corresponding field or property was found.</returns>
		public virtual bool TryGetFieldValue(string FieldName, object Object, out object Value)
		{
#if NETSTANDARD1_5
			if (this.compiled)
				return this.customSerializer.TryGetFieldValue(FieldName, Object, out Value);
			else
			{
#endif
				if (this.membersByName.TryGetValue(FieldName, out Member Member))
				{
					Value = Member.Get(Object);
					return true;
				}
				else
				{
					Value = null;
					return false;
				}
#if NETSTANDARD1_5
			}
#endif
		}

		/// <summary>
		/// Gets members from a type, including inherited members.
		/// </summary>
		/// <param name="T">Type information.</param>
		/// <returns>Enumerated set of members.</returns>
		public static IEnumerable<MemberInfo> GetMembers(System.Reflection.TypeInfo T)
		{
			while (!(T is null))
			{
				foreach (MemberInfo MI in T.DeclaredMembers)
					yield return MI;

				if (!(T.BaseType is null))
					T = T.BaseType.GetTypeInfo();
				else
					T = null;
			}
		}

		private void AppendType(Type T, StringBuilder sb)
		{
			if (T.IsConstructedGenericType)
			{
				Type T2 = T.GetGenericTypeDefinition();
				string s = T2.FullName;
				int i = s.IndexOf('`');

				if (i > 0)
					s = s.Substring(0, i);

				sb.Append(s);
				sb.Append('<');

				bool First = true;

				foreach (Type Arg in T.GenericTypeArguments)
				{
					if (First)
						First = false;
					else
						sb.Append(',');

					this.AppendType(Arg, sb);
				}

				sb.Append('>');
			}
			else if (T.HasElementType)
			{
				if (T.IsArray)
				{
					this.AppendType(T.GetElementType(), sb);
					sb.Append("[]");
				}
				else if (T.IsPointer)
				{
					this.AppendType(T.GetElementType(), sb);
					sb.Append('*');
				}
				else
					sb.Append(T.FullName);
			}
			else
				sb.Append(T.FullName);
		}

	}
}
