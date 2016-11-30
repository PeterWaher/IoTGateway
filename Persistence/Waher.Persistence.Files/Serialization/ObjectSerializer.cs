using System;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CSharp;
using Waher.Persistence.Attributes;
using Waher.Persistence.Filters;

namespace Waher.Persistence.Files.Serialization
{
	/// <summary>
	/// Serializes a class, taking into account attributes defined in <see cref="Waher.Persistence.Attributes"/>.
	/// </summary>
	public class ObjectSerializer : IObjectSerializer
	{
		public const uint TYPE_BOOLEAN = 0;
		public const uint TYPE_BYTE = 1;
		public const uint TYPE_INT16 = 2;
		public const uint TYPE_INT32 = 3;
		public const uint TYPE_INT64 = 4;
		public const uint TYPE_SBYTE = 5;
		public const uint TYPE_UINT16 = 6;
		public const uint TYPE_UINT32 = 7;
		public const uint TYPE_UINT64 = 8;
		public const uint TYPE_DECIMAL = 9;
		public const uint TYPE_DOUBLE = 10;
		public const uint TYPE_SINGLE = 11;
		public const uint TYPE_DATETIME = 12;
		public const uint TYPE_TIMESPAN = 13;
		public const uint TYPE_CHAR = 14;
		public const uint TYPE_STRING = 15;
		public const uint TYPE_ENUM = 16;
		public const uint TYPE_BYTEARRAY = 17;
		public const uint TYPE_GUID = 18;
		public const uint TYPE_NULL = 29;
		public const uint TYPE_ARRAY = 30;
		public const uint TYPE_OBJECT = 31;

		private Dictionary<string, string> shortNamesByFieldName = new Dictionary<string, string>();
		private Dictionary<string, object> defaultValues = new Dictionary<string, object>();
		private Dictionary<string, Type> memberTypes = new Dictionary<string, Type>();
		private Dictionary<string, MemberInfo> members = new Dictionary<string, MemberInfo>();
		private Type type;
		private string collectionName;
		private string typeFieldName;
		private TypeNameSerialization typeNameSerialization;
		private FieldInfo objectIdFieldInfo = null;
		private PropertyInfo objectIdPropertyInfo = null;
		private IObjectSerializer customSerializer = null;
		private FilesProvider provider;
		private bool isNullable;
		private bool debug;

		/// <summary>
		/// Serializes a class, taking into account attributes defined in <see cref="Waher.Persistence.Attributes"/>.
		/// </summary>
		/// <param name="Type">Type to serialize.</param>
		/// <param name="Provider">Database provider.</param>
		/// <param name="Debug">If debug information is to be included for generated code.</param>
		public ObjectSerializer(Type Type, FilesProvider Provider, bool Debug)
		{
			string TypeName = Type.Name;

			this.type = Type;
			this.provider = Provider;
			this.debug = Debug;

			switch (Type.GetTypeCode(this.type))
			{
				case TypeCode.Boolean:
				case TypeCode.Byte:
				case TypeCode.Char:
				case TypeCode.DateTime:
				case TypeCode.Decimal:
				case TypeCode.Double:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.SByte:
				case TypeCode.Single:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
					this.isNullable = false;
					break;

				case TypeCode.DBNull:
				case TypeCode.Empty:
				case TypeCode.String:
					this.isNullable = true;
					break;

				case TypeCode.Object:
					this.isNullable = !this.type.IsValueType;
					break;
			}

			CollectionNameAttribute CollectionNameAttribute = Type.GetCustomAttribute<CollectionNameAttribute>(true);
			if (CollectionNameAttribute == null)
				this.collectionName = null;
			else
				this.collectionName = CollectionNameAttribute.Name;

			TypeNameAttribute TypeNameAttribute = Type.GetCustomAttribute<TypeNameAttribute>(true);
			if (TypeNameAttribute == null)
			{
				this.typeFieldName = "_type";
				this.typeNameSerialization = TypeNameSerialization.FullName;
			}
			else
			{
				this.typeFieldName = TypeNameAttribute.FieldName;
				this.typeNameSerialization = TypeNameAttribute.TypeNameSerialization;
			}

			if (Type.IsAbstract && this.typeNameSerialization == TypeNameSerialization.None)
				throw new Exception("Serializers for abstract classes require type names to be serialized.");

			StringBuilder CSharp = new StringBuilder();
			Type MemberType;
			FieldInfo FI;
			PropertyInfo PI;
			object DefaultValue;
			string ShortName;
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
			CSharp.AppendLine("using Waher.Persistence.Filters;");
			CSharp.AppendLine("using Waher.Persistence.Files;");
			CSharp.AppendLine("using Waher.Persistence.Files.Serialization;");
			CSharp.AppendLine("using Waher.Script;");
			CSharp.AppendLine();
			CSharp.AppendLine("namespace " + Type.Namespace + ".Binary");
			CSharp.AppendLine("{");
			CSharp.AppendLine("\tpublic class BinarySerializer" + TypeName + this.provider.Id + " : GeneratedObjectSerializerBase");
			CSharp.AppendLine("\t{");
			CSharp.AppendLine("\t\tprivate FilesProvider provider;");

			foreach (MemberInfo Member in Type.GetMembers(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
			{
				if ((FI = Member as FieldInfo) != null)
				{
					PI = null;
					MemberType = FI.FieldType;
				}
				else if ((PI = Member as PropertyInfo) != null)
				{
					if (PI.GetMethod == null || PI.SetMethod == null)
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
				ShortName = null;
				ByReference = false;
				Nullable = false;

				if (MemberType.IsGenericType)
				{
					Type GT = MemberType.GetGenericTypeDefinition();
					if (GT == typeof(Nullable<>))
					{
						Nullable = true;
						MemberType = MemberType.GenericTypeArguments[0];
					}
				}

				foreach (Attribute Attr in Member.GetCustomAttributes(true))
				{
					if (Attr is IgnoreMemberAttribute)
					{
						Ignore = true;
						break;
					}

					if (Attr is DefaultValueAttribute)
					{
						DefaultValue = ((DefaultValueAttribute)Attr).Value;
						NrDefault++;

						this.defaultValues[Member.Name] = DefaultValue;

						CSharp.Append("\t\tprivate static readonly ");

						if (DefaultValue == null)
							CSharp.Append("object");
						else
							CSharp.Append(MemberType.FullName);

						CSharp.Append(" default");
						CSharp.Append(Member.Name);
						CSharp.Append(" = ");

						if (DefaultValue == null)
							CSharp.Append("null");
						else
						{
							if (DefaultValue.GetType() != MemberType)
							{
								CSharp.Append('(');
								CSharp.Append(MemberType.FullName);
								CSharp.Append(')');
							}

							if (DefaultValue is string)
							{
								if (string.IsNullOrEmpty((string)DefaultValue))
									CSharp.Append("string.Empty");
								else
								{
									CSharp.Append("\"");
									CSharp.Append(Escape(DefaultValue.ToString()));
									CSharp.Append("\"");
								}
							}
							else if (DefaultValue is DateTime)
							{
								DateTime TP = (DateTime)DefaultValue;
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
							else if (DefaultValue is TimeSpan)
							{
								TimeSpan TS = (TimeSpan)DefaultValue;
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
							else if (DefaultValue is Guid)
							{
								Guid Guid = (Guid)DefaultValue;
								if (Guid.Equals(Guid.Empty))
									CSharp.Append("Guid.Empty");
								else
								{
									CSharp.Append("new Guid(\"");
									CSharp.Append(Guid.ToString());
									CSharp.Append("\")");
								}
							}
							else if (DefaultValue is Enum)
							{
								Type DefaultValueType = DefaultValue.GetType();

								if (DefaultValueType.IsDefined(typeof(FlagsAttribute), false))
								{
									Enum e = (Enum)DefaultValue;
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
							else if (DefaultValue is bool)
							{
								if ((bool)DefaultValue)
									CSharp.Append("true");
								else
									CSharp.Append("false");
							}
							else if (DefaultValue is long)
							{
								CSharp.Append(DefaultValue.ToString());
								CSharp.Append("L");
							}
							else if (DefaultValue is char)
							{
								char ch = (char)DefaultValue;

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

					if (Attr is ShortNameAttribute)
					{
						ShortName = ((ShortNameAttribute)Attr).Name;
						this.shortNamesByFieldName[Member.Name] = ShortName;
					}
				}

				if (Ignore)
					continue;

				if (Type.GetTypeCode(MemberType) == TypeCode.Object && !MemberType.IsArray &&
					!ByReference && MemberType != typeof(TimeSpan) && MemberType != typeof(TimeSpan) && MemberType != typeof(Guid))
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

			foreach (MemberInfo Member in Type.GetMembers(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
			{
				if ((FI = Member as FieldInfo) != null)
				{
					PI = null;
					MemberType = FI.FieldType;
				}
				else if ((PI = Member as PropertyInfo) != null)
				{
					if (PI.GetMethod == null || PI.SetMethod == null)
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
				ShortName = null;

				if (MemberType.IsGenericType)
				{
					Type GT = MemberType.GetGenericTypeDefinition();
					if (GT == typeof(Nullable<>))
					{
						Nullable = true;
						MemberType = MemberType.GenericTypeArguments[0];
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
					else if (Attr is ShortNameAttribute)
						ShortName = ((ShortNameAttribute)Attr).Name;
				}

				if (Ignore || HasObjectId)
					continue;

				if (Type.GetTypeCode(MemberType) == TypeCode.Object && !MemberType.IsArray &&
					!ByReference && MemberType != typeof(TimeSpan) && MemberType != typeof(string) && MemberType != typeof(Guid))
				{
					CSharp.Append("\t\t\tthis.serializer");
					CSharp.Append(Member.Name);
					CSharp.Append(" = this.provider.GetObjectSerializer(typeof(");
					CSharp.Append(MemberType.FullName);
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
			CSharp.AppendLine("\t\t\tBookmark Bookmark = Reader.GetBookmark();");
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
				CSharp.AppendLine("\t\t\tstring TypeName = this.provider.GetFieldName(\"" + this.collectionName + "\", FieldCode);");

				if (this.typeNameSerialization == TypeNameSerialization.LocalName)
					CSharp.AppendLine("\t\t\tTypeName = \"" + Type.Namespace + ".\" + TypeName;");

				CSharp.AppendLine();
				CSharp.AppendLine("\t\t\tType DesiredType = Waher.Script.Types.GetType(TypeName);");
				CSharp.AppendLine("\t\t\tif (DesiredType == null)");
				CSharp.AppendLine("\t\t\t\tDesiredType = typeof(GenericObject);");
				CSharp.AppendLine();
				CSharp.AppendLine("\t\t\tif (DesiredType != typeof(" + Type.FullName + "))");
				CSharp.AppendLine("\t\t\t{");
				CSharp.AppendLine("\t\t\t\tIObjectSerializer Serializer2 = this.provider.GetObjectSerializer(DesiredType);");
				CSharp.AppendLine("\t\t\t\tReader.SetBookmark(Bookmark);");
				CSharp.AppendLine("\t\t\t\treturn Serializer2.Deserialize(Reader, DataTypeBak, Embedded);");
				CSharp.AppendLine("\t\t\t}");
			}

			if (Type.IsAbstract)
				CSharp.AppendLine("\t\t\tthrow new Exception(\"Unable to create an instance of an abstract class.\");");
			else
			{
				CSharp.AppendLine();

				if (this.debug)
					CSharp.AppendLine("\t\t\tConsole.Out.WriteLine(DataType.Value);");

				CSharp.AppendLine("\t\t\tif (DataType.Value != " + TYPE_OBJECT + ")");
				CSharp.AppendLine("\t\t\t\tthrow new Exception(\"Object expected.\");");

				CSharp.AppendLine();
				CSharp.AppendLine("\t\t\tResult = new " + Type.FullName + "();");
				CSharp.AppendLine();

				if (ObjectIdMember != null)
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

				foreach (MemberInfo Member in Type.GetMembers(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
				{
					if ((FI = Member as FieldInfo) != null)
					{
						PI = null;
						MemberType = FI.FieldType;
					}
					else if ((PI = Member as PropertyInfo) != null)
					{
						if (PI.GetMethod == null || PI.SetMethod == null)
							continue;

						if (PI.GetIndexParameters().Length > 0)
							continue;

						MemberType = PI.PropertyType;
					}
					else
						continue;

					Ignore = false;
					ShortName = null;
					ByReference = false;
					Nullable = false;

					if (MemberType.IsGenericType)
					{
						Type GT = MemberType.GetGenericTypeDefinition();
						if (GT == typeof(Nullable<>))
						{
							Nullable = true;
							MemberType = MemberType.GenericTypeArguments[0];
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

						if (Attr is ShortNameAttribute)
							ShortName = ((ShortNameAttribute)Attr).Name;
					}

					if (Ignore)
						continue;

					if (!string.IsNullOrEmpty(ShortName) && ShortName != Member.Name)
						CSharp.AppendLine("\t\t\t\t\tcase " + this.provider.GetFieldCode(this.collectionName, ShortName) + ":");

					CSharp.AppendLine("\t\t\t\t\tcase " + this.provider.GetFieldCode(this.collectionName, Member.Name) + ":");

					if (MemberType.IsEnum)
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
									CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = this.ReadNullableBoolean(Reader, FieldDataType);");
								else
									CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = this.ReadBoolean(Reader, FieldDataType);");
								break;

							case TypeCode.Byte:
								if (Nullable)
									CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = this.ReadNullableByte(Reader, FieldDataType);");
								else
									CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = this.ReadByte(Reader, FieldDataType);");
								break;

							case TypeCode.Char:
								if (Nullable)
									CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = this.ReadNullableChar(Reader, FieldDataType);");
								else
									CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = this.ReadChar(Reader, FieldDataType);");
								break;

							case TypeCode.DateTime:
								if (Nullable)
									CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = this.ReadNullableDateTime(Reader, FieldDataType);");
								else
									CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = this.ReadDateTime(Reader, FieldDataType);");
								break;

							case TypeCode.Decimal:
								if (Nullable)
									CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = this.ReadNullableDecimal(Reader, FieldDataType);");
								else
									CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = this.ReadDecimal(Reader, FieldDataType);");
								break;

							case TypeCode.Double:
								if (Nullable)
									CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = this.ReadNullableDouble(Reader, FieldDataType);");
								else
									CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = this.ReadDouble(Reader, FieldDataType);");
								break;

							case TypeCode.Int16:
								if (Nullable)
									CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = this.ReadNullableInt16(Reader, FieldDataType);");
								else
									CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = this.ReadInt16(Reader, FieldDataType);");
								break;

							case TypeCode.Int32:
								if (Nullable)
									CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = this.ReadNullableInt32(Reader, FieldDataType);");
								else
									CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = this.ReadInt32(Reader, FieldDataType);");
								break;

							case TypeCode.Int64:
								if (Nullable)
									CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = this.ReadNullableInt64(Reader, FieldDataType);");
								else
									CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = this.ReadInt64(Reader, FieldDataType);");
								break;

							case TypeCode.SByte:
								if (Nullable)
									CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = this.ReadNullableSByte(Reader, FieldDataType);");
								else
									CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = this.ReadSByte(Reader, FieldDataType);");
								break;

							case TypeCode.Single:
								if (Nullable)
									CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = this.ReadNullableSingle(Reader, FieldDataType);");
								else
									CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = this.ReadSingle(Reader, FieldDataType);");
								break;

							case TypeCode.String:
								CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = this.ReadString(Reader, FieldDataType);");
								break;

							case TypeCode.UInt16:
								if (Nullable)
									CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = this.ReadNullableUInt16(Reader, FieldDataType);");
								else
									CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = this.ReadUInt16(Reader, FieldDataType);");
								break;

							case TypeCode.UInt32:
								if (Nullable)
									CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = this.ReadNullableUInt32(Reader, FieldDataType);");
								else
									CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = this.ReadUInt32(Reader, FieldDataType);");
								break;

							case TypeCode.UInt64:
								if (Nullable)
									CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = this.ReadNullableUInt64(Reader, FieldDataType);");
								else
									CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = this.ReadUInt64(Reader, FieldDataType);");
								break;

							case TypeCode.DBNull:
							case TypeCode.Empty:
							default:
								throw new Exception("Invalid member type: " + Member.MemberType.ToString());

							case TypeCode.Object:
								if (MemberType.IsArray)
								{
									if (MemberType == typeof(byte[]))
										CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = Reader.ReadByteArray();");
									else
									{
										MemberType = MemberType.GetElementType();
										CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = this.ReadArray<" + GenericParameterName(MemberType) + ">(this.provider, Reader, FieldDataType);");
									}
								}
								else if (ByReference)
								{
									CSharp.AppendLine("\t\t\t\t\t\tswitch (FieldDataType)");
									CSharp.AppendLine("\t\t\t\t\t\t{");
									CSharp.AppendLine("\t\t\t\t\t\t\tcase " + TYPE_GUID + ":");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tGuid ObjectId = Reader.ReadGuid();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tTask<" + MemberType.FullName + "> Task = this.provider.LoadObject<" + GenericParameterName(MemberType) + ">(ObjectId);");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tif (!Task.Wait(10000))");
									CSharp.AppendLine("\t\t\t\t\t\t\t\t\tthrow new Exception(\"Unable to load referenced object. Database timed out.\");");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = Task.Result;");
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
										CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = this.ReadNullableTimeSpan(Reader, FieldDataType);");
									else
										CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = this.ReadTimeSpan(Reader, FieldDataType);");
								}
								else if (MemberType == typeof(Guid))
								{
									if (Nullable)
										CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = this.ReadNullableGuid(Reader, FieldDataType);");
									else
										CSharp.AppendLine("\t\t\t\t\t\tResult." + Member.Name + " = this.ReadGuid(Reader, FieldDataType);");
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
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Embedded object expected for " + Member.Name + ". Data Type read: \" + FilesProvider.GetFieldDataTypeName(FieldDataType));");
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
				CSharp.AppendLine("\t\t\t\tif (Value == null)");
				CSharp.AppendLine("\t\t\t\t{");
				CSharp.AppendLine("\t\t\t\t\tWriter.WriteBits(" + TYPE_NULL + ", 6);");
				CSharp.AppendLine("\t\t\t\t\treturn;");
				CSharp.AppendLine("\t\t\t\t}");
				CSharp.AppendLine("\t\t\t\telse");
				CSharp.AppendLine("\t\t\t\t\tWriter.WriteBits(" + TYPE_OBJECT + ", 6);");
				CSharp.AppendLine("\t\t\t}");
				CSharp.AppendLine("\t\t\telse if (Value == null)");
				CSharp.AppendLine("\t\t\t\tthrow new NullReferenceException(\"Value cannot be null.\");");
			}
			else
			{
				CSharp.AppendLine("\t\t\tif (WriteTypeCode)");
				CSharp.AppendLine("\t\t\t\tWriter.WriteBits(" + TYPE_OBJECT + ", 6);");
			}

			CSharp.AppendLine();

			if (this.typeNameSerialization != TypeNameSerialization.None)
			{
				if (this.debug)
					CSharp.AppendLine("\t\t\tConsole.Out.WriteLine();");

				CSharp.Append("\t\t\tWriter.WriteVariableLengthUInt64(");

				if (this.typeNameSerialization == TypeNameSerialization.LocalName)
					CSharp.Append(this.provider.GetFieldCode(this.collectionName, this.type.Name));
				else
					CSharp.Append(this.provider.GetFieldCode(this.collectionName, this.type.FullName));

				CSharp.Append(");");
			}
			else
				CSharp.Append("\t\t\tWriter.WriteVariableLengthUInt64(0);");

			foreach (MemberInfo Member in Type.GetMembers(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
			{
				if ((FI = Member as FieldInfo) != null)
				{
					PI = null;
					MemberType = FI.FieldType;
				}
				else if ((PI = Member as PropertyInfo) != null)
				{
					if (PI.GetMethod == null || PI.SetMethod == null)
						continue;

					if (PI.GetIndexParameters().Length > 0)
						continue;

					MemberType = PI.PropertyType;
				}
				else
					continue;

				Ignore = false;
				ShortName = null;
				HasDefaultValue = false;
				DefaultValue = null;
				ObjectIdField = false;
				ByReference = false;
				Nullable = false;

				if (MemberType.IsGenericType)
				{
					Type GT = MemberType.GetGenericTypeDefinition();
					if (GT == typeof(Nullable<>))
					{
						Nullable = true;
						MemberType = MemberType.GenericTypeArguments[0];
					}
				}

				foreach (Attribute Attr in Member.GetCustomAttributes(true))
				{
					if (Attr is IgnoreMemberAttribute)
					{
						Ignore = true;
						break;
					}
					else if (Attr is DefaultValueAttribute)
					{
						HasDefaultValue = true;
						DefaultValue = ((DefaultValueAttribute)Attr).Value;
					}
					else if (Attr is ShortNameAttribute)
						ShortName = ((ShortNameAttribute)Attr).Name;
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
					if (DefaultValue == null)
						CSharp.AppendLine("\t\t\tif (((object)Value." + Member.Name + ") != null)");
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
					if (string.IsNullOrEmpty(ShortName))
						CSharp.Append(this.provider.GetFieldCode(this.collectionName, Member.Name));
					else
						CSharp.Append(this.provider.GetFieldCode(this.collectionName, ShortName));
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

					if (MemberType.IsEnum)
					{
						if (MemberType.IsDefined(typeof(FlagsAttribute), false))
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
								CSharp.AppendLine(" == null)");

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

							case TypeCode.DBNull:
							case TypeCode.Empty:
								CSharp.Append(Indent2);
								CSharp.Append("Writer.WriteBits(");
								CSharp.Append(TYPE_NULL);
								CSharp.AppendLine(", 6);");
								break;

							default:
								throw new Exception("Invalid member type: " + Member.MemberType.ToString());

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
										CSharp.Append("this.WriteArray<" + GenericParameterName(MemberType) + ">(this.provider, Writer, Value." + Member.Name + ");");
									}
								}
								else if (ByReference)
								{
									CSharp.Append(Indent2);
									CSharp.AppendLine("if (Value." + Member.Name + " == null)");
									CSharp.Append(Indent2);
									CSharp.AppendLine("\tWriter.WriteBits(" + TYPE_NULL + ");");
									CSharp.Append(Indent2);
									CSharp.AppendLine("else");
									CSharp.Append(Indent2);
									CSharp.AppendLine("{");
									CSharp.Append(Indent2);
									CSharp.AppendLine("\tWriter.WriteBits(" + TYPE_GUID + ");");
									CSharp.Append(Indent2);
									CSharp.AppendLine("\tIObjectSerializer Serializer" + Member.Name + " = this.provider.GetObjectSerializer(typeof(" + MemberType.FullName + "));");
									CSharp.Append(Indent2);
									CSharp.AppendLine("\tWriter.Write(Serializer" + Member.Name + ".GetObjectId(Value." + Member.Name + ", true));");
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

			if (ObjectIdMemberType == null)
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
					CSharp.AppendLine("\t\t\t\tif (ObjectId != null)");
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
			CSharp.AppendLine("\t}");
			CSharp.AppendLine("}");

			string CSharpCode = CSharp.ToString();
			CSharpCodeProvider CodeProvider = new CSharpCodeProvider();
			string[] Assemblies = new string[]
			{
				Type.Assembly.Location,
				typeof(Database).Assembly.Location,
				typeof(Waher.Script.Types).Assembly.Location,
				typeof(ObjectSerializer).Assembly.Location
			};
			CompilerParameters Options;

			/*if (this.debug)
			{
				System.IO.File.WriteAllText(Type.Name + ".cs", CSharpCode);
				Options = new CompilerParameters(Assemblies, Type.Name + ".obj", true);
			}
			else*/
			Options = new CompilerParameters(Assemblies);

			CompilerResults CompilerResults = CodeProvider.CompileAssemblyFromSource(Options, CSharpCode);

			if (CompilerResults.Errors.Count > 0)
			{
				StringBuilder sb = new StringBuilder();

				foreach (CompilerError Error in CompilerResults.Errors)
				{
					sb.AppendLine();
					sb.Append(Error.Line.ToString());
					sb.Append(": ");
					sb.Append(Error.ErrorText);
				}

				sb.AppendLine();
				sb.AppendLine();
				sb.AppendLine("Code generated:");
				sb.AppendLine();
				sb.AppendLine(CSharp.ToString());

				throw new Exception("Unable to serialize objects of type " + Type.FullName +
					". When generating serialization class, the following compiler errors were reported:\r\n" + sb.ToString());
			}

			Assembly A = CompilerResults.CompiledAssembly;
			Type T = A.GetType(Type.Namespace + ".Binary.BinarySerializer" + TypeName + this.provider.Id);
			ConstructorInfo CI = T.GetConstructor(new Type[] { typeof(FilesProvider) });
			this.customSerializer = (IObjectSerializer)CI.Invoke(new object[] { this.provider });

			/*BsonSerializer.RegisterSerializer(Type, this);

			IMongoCollection<BsonDocument> Collection = null;
			List<BsonDocument> Indices = null;

			foreach (IndexAttribute CompoundIndexAttribute in Type.GetCustomAttributes<IndexAttribute>(true))
			{
				bool IndexFound = false;

				if (Collection == null)
				{
					if (string.IsNullOrEmpty(this.collectionName))
						Collection = this.provider.DefaultCollection;
					else
						Collection = this.provider.GetCollection(this.collectionName);

					IAsyncCursor<BsonDocument> Cursor = Collection.Indexes.List();
					Indices = Cursor.ToList<BsonDocument>();
				}

				foreach (BsonDocument Index in Indices)
				{
					BsonDocument Key = Index["key"].AsBsonDocument;
					if (Key.ElementCount != CompoundIndexAttribute.FieldNames.Length)
						continue;

					IEnumerator<BsonElement> e1 = Key.Elements.GetEnumerator();
					IEnumerator e2 = CompoundIndexAttribute.FieldNames.GetEnumerator();

					bool Found = true;

					while (e1.MoveNext() && e2.MoveNext())
					{
						if (e1.Current.Name != (string)e2.Current)
						{
							Found = false;
							break;
						}
					}

					if (Found)
					{
						IndexFound = true;
						break;
					}
				}

				if (!IndexFound)
				{
					IndexKeysDefinition<BsonDocument> Index = null;

					foreach (string FieldName in CompoundIndexAttribute.FieldNames)
					{
						if (Index == null)
						{
							if (FieldName.StartsWith("-"))
								Index = Builders<BsonDocument>.IndexKeys.Descending(this.ToShortName(FieldName.Substring(1)));
							else
								Index = Builders<BsonDocument>.IndexKeys.Ascending(this.ToShortName(FieldName));
						}
						else
						{
							if (FieldName.StartsWith("-"))
								Index = Index.Descending(this.ToShortName(FieldName.Substring(1)));
							else
								Index = Index.Ascending(this.ToShortName(FieldName));
						}
					}

					Collection.Indexes.CreateOneAsync(Index);
				}
			}*/
		}

		private static string GenericParameterName(Type Type)
		{
			if (Type.IsGenericType)
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
		public string CollectionName
		{
			get { return this.collectionName; }
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
		public object Deserialize(BinaryDeserializer Reader, uint? DataType, bool Embedded)
		{
			return this.customSerializer.Deserialize(Reader, DataType, Embedded);
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Writer">Binary serializer.</param>
		/// <param name="WriteTypeCode">If a type code is to be written.</param>
		/// <param name="Embedded">If the object is embedded in another object.</param>
		/// <param name="Value">Value to serialize.</param>
		public void Serialize(BinarySerializer Writer, bool WriteTypeCode, bool Embedded, object Value)
		{
			this.customSerializer.Serialize(Writer, WriteTypeCode, Embedded, Value);
		}

		/// <summary>
		/// Mamber name of the field or property holding the Object ID, if any. If there are no such member, this property returns null.
		/// </summary>
		public string ObjectIdMemberName
		{
			get
			{
				if (this.objectIdFieldInfo != null)
					return this.objectIdFieldInfo.Name;
				else if (this.objectIdPropertyInfo != null)
					return this.objectIdPropertyInfo.Name;
				else
					return null;
			}
		}

		/// <summary>
		/// If the class has an Object ID field.
		/// </summary>
		public bool HasObjectIdField
		{
			get
			{
				return this.objectIdFieldInfo != null || this.objectIdPropertyInfo != null;
			}
		}

		/// <summary>
		/// If the class has an Object ID.
		/// </summary>
		/// <param name="Value">Object reference.</param>
		public bool HasObjectId(object Value)
		{
			object ObjectId;

			if (this.objectIdFieldInfo != null)
				ObjectId = this.objectIdFieldInfo.GetValue(Value);
			else if (this.objectIdPropertyInfo != null)
				ObjectId = this.objectIdPropertyInfo.GetValue(Value);
			else
				return false;

			if (ObjectId == null)
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
		public bool TrySetObjectId(object Value, Guid ObjectId)
		{
			Type MemberType;
			object Obj;

			if (this.objectIdFieldInfo != null)
				MemberType = this.objectIdFieldInfo.FieldType;
			else if (this.objectIdPropertyInfo != null)
				MemberType = this.objectIdPropertyInfo.PropertyType;
			else
				return false;

			if (MemberType == typeof(Guid))
				Obj = ObjectId;
			else if (MemberType == typeof(string))
				Obj = ObjectId.ToString();
			else if (MemberType == typeof(byte[]))
				Obj = ObjectId.ToByteArray();
			else
				return false;

			if (this.objectIdFieldInfo != null)
				this.objectIdFieldInfo.SetValue(Value, Obj);
			else
				this.objectIdPropertyInfo.SetValue(Value, Obj);

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
		public Guid GetObjectId(object Value, bool InsertIfNotFound)
		{
			object Obj;

			if (this.objectIdFieldInfo != null)
				Obj = this.objectIdFieldInfo.GetValue(Value);
			else if (this.objectIdPropertyInfo != null)
				Obj = this.objectIdPropertyInfo.GetValue(Value);
			else
				throw new NotSupportedException("No Object ID member found in objects of type " + Value.GetType().FullName + ".");

			if (Obj == null)
			{
				if (!InsertIfNotFound)
					throw new Exception("Object has no Object ID defined.");

				Type ValueType = Value.GetType();
				IObjectSerializer Serializer = this.provider.GetObjectSerializer(ValueType);
				string CollectionName = this.collectionName;

				throw new NotImplementedException();    // TODO: Insert object
			}
			else if (Obj is Guid)
				return (Guid)Obj;
			else if (Obj is string)
				return new Guid((string)Obj);
			else if (Obj is byte[])
				return new Guid((byte[])Obj);
			else
				throw new NotSupportedException("Unsupported type for Object ID members: " + Obj.GetType().FullName);
		}

		/// <summary>
		/// Checks if a given field value corresponds to the default value for the corresponding field.
		/// </summary>
		/// <param name="FieldName">Name of field.</param>
		/// <param name="Value">Field value.</param>
		/// <returns>If the field value corresponds to the default value of the corresponding field.</returns>
		public bool IsDefaultValue(string FieldName, object Value)
		{
			object Default;

			if (!this.defaultValues.TryGetValue(FieldName, out Default))
				return false;

			if ((Value == null) ^ (Default == null))
				return false;

			if (Value == null)
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
		public bool TryGetFieldValue(string FieldName, object Object, out object Value)
		{
			MemberInfo MI;
			FieldInfo FI;
			PropertyInfo PI;

			if (!this.members.TryGetValue(FieldName, out MI))
			{
				Value = null;
				return false;
			}

			if ((PI = MI as PropertyInfo) != null)
			{
				Value = PI.GetValue(Object);
				return true;
			}

			if ((FI = MI as FieldInfo) != null)
			{
				Value = FI.GetValue(Object);
				return true;
			}

			Value = null;
			return false;
		}

	}
}
