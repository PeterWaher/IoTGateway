using System;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Waher.Persistence.Attributes;
using Waher.Persistence.Filters;
using Waher.Runtime.Inventory;
using Waher.Runtime.Threading;

namespace Waher.Persistence.MongoDB.Serialization
{
	/// <summary>
	/// Serializes a type to BSON, taking into account attributes defined in <see cref="Waher.Persistence.Attributes"/>.
	/// </summary>
	public class ObjectSerializer : IObjectSerializer, IBsonDocumentSerializer
	{
		private readonly Dictionary<string, string> shortNamesByFieldName = new Dictionary<string, string>();
		private readonly Dictionary<string, object> defaultValues = new Dictionary<string, object>();
		private readonly Dictionary<string, Type> memberTypes = new Dictionary<string, Type>();
		private readonly Dictionary<string, MemberInfo> members = new Dictionary<string, MemberInfo>();
		private readonly string collectionName;
		private readonly string typeFieldName;
		private readonly IObjectSerializer customSerializer;
		private readonly TypeNameSerialization typeNameSerialization;
		private readonly FieldInfo objectIdFieldInfo = null;
		private readonly PropertyInfo objectIdPropertyInfo = null;
		private readonly bool isNullable;
		private readonly Type type;
		private readonly System.Reflection.TypeInfo typeInfo;
		private readonly MongoDBProvider provider;

		internal ObjectSerializer(Type Type, MongoDBProvider Provider, bool Null)
		{
			this.isNullable = true;
			this.collectionName = null;
			this.typeNameSerialization = TypeNameSerialization.FullName;
			this.isNullable = true;
			this.type = Type;
			this.typeInfo = Type.GetTypeInfo();
			this.provider = Provider;
		}

		/// <summary>
		/// Serializes a type to BSON, taking into account attributes defined in <see cref="Waher.Persistence.Attributes"/>.
		/// </summary>
		/// <param name="Type">Type to serialize.</param>
		/// <param name="Provider">MongoDB Provider object.</param>
		public ObjectSerializer(Type Type, MongoDBProvider Provider)
		{
			string TypeName = Type.Name;

			this.type = Type;
			this.typeInfo = Type.GetTypeInfo();
			this.provider = Provider;

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

			StringBuilder CSharp = new StringBuilder();
			Type MemberType;
			System.Reflection.TypeInfo MemberTypeInfo;
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
			CSharp.AppendLine("using MongoDB.Bson;");
			CSharp.AppendLine("using MongoDB.Bson.IO;");
			CSharp.AppendLine("using MongoDB.Bson.Serialization;");
			CSharp.AppendLine("using Waher.Persistence.Filters;");
			CSharp.AppendLine("using Waher.Persistence.MongoDB;");
			CSharp.AppendLine("using Waher.Persistence.MongoDB.Serialization;");
			CSharp.AppendLine("using Waher.Persistence.Serialization;");
			CSharp.AppendLine("using Waher.Runtime.Inventory;");
			CSharp.AppendLine();
			CSharp.AppendLine("namespace " + this.type.Namespace + ".Bson");
			CSharp.AppendLine("{");
			CSharp.AppendLine("\tpublic class BsonSerializer" + TypeName + this.provider.Id + " : GeneratedObjectSerializerBase");
			CSharp.AppendLine("\t{");
			CSharp.AppendLine("\t\tprivate readonly MongoDBProvider provider;");

			foreach (MemberInfo Member in this.typeInfo.GetMembers(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
			{
				if ((FI = Member as FieldInfo) != null)
				{
					PI = null;
					MemberType = FI.FieldType;
				}
				else if ((PI = Member as PropertyInfo) != null)
				{
					if (PI.GetMethod is null || PI.SetMethod is null)
						continue;

					if (PI.GetIndexParameters().Length > 0)
						continue;

					MemberType = PI.PropertyType;
				}
				else
					continue;

				this.memberTypes[Member.Name] = MemberType;
				this.members[Member.Name] = Member;

				MemberTypeInfo = MemberType.GetTypeInfo();
				Ignore = false;
				ShortName = null;
				ByReference = false;
				Nullable = false;

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

					if (Attr is DefaultValueAttribute)
					{
						DefaultValue = ((DefaultValueAttribute)Attr).Value;
						NrDefault++;

						this.defaultValues[Member.Name] = DefaultValue;

						CSharp.Append("\t\tprivate static readonly ");

						if (DefaultValue is null)
							CSharp.Append("object");
						else
							CSharp.Append(DefaultValue.GetType().FullName);

						CSharp.Append(" default");
						CSharp.Append(Member.Name);
						CSharp.Append(" = ");

						if (DefaultValue is null)
							CSharp.Append("null");
						else if (DefaultValue is string s2)
						{
							if (string.IsNullOrEmpty(s2))
								CSharp.Append("string.Empty");
							else
							{
								CSharp.Append("\"");
								CSharp.Append(Escape(s2));
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
						else if (DefaultValue is CaseInsensitiveString cis)
						{
							if (CaseInsensitiveString.IsNullOrEmpty(cis))
								CSharp.Append("CaseInsensitiveString.Empty");
							else
							{
								CSharp.Append("new CaseInsensitiveString(\"");
								CSharp.Append(Escape(cis.Value));
								CSharp.Append("\")");
							}
						}
						else if (DefaultValue is Enum e)
						{
							Type DefaultValueType = DefaultValue.GetType();
							System.Reflection.TypeInfo DefaultValueTypeInfo = DefaultValueType.GetTypeInfo();

							if (DefaultValueTypeInfo.IsDefined(typeof(FlagsAttribute)))
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

				if (Type.GetTypeCode(MemberType) == TypeCode.Object &&
					!MemberType.IsArray &&
					!ByReference &&
					MemberType != typeof(TimeSpan) &&
					MemberType != typeof(Guid) &&
					MemberType != typeof(DateTimeOffset) &&
					MemberType != typeof(CaseInsensitiveString))
				{
					CSharp.Append("\t\tprivate readonly IObjectSerializer serializer");
					CSharp.Append(Member.Name);
					CSharp.AppendLine(";");
				}
			}

			if (NrDefault > 0)
				CSharp.AppendLine();

			CSharp.AppendLine("\t\tpublic BsonSerializer" + TypeName + this.provider.Id + "(MongoDBProvider Provider)");
			CSharp.AppendLine("\t\t{");
			CSharp.AppendLine("\t\t\tthis.provider = Provider;");


			foreach (MemberInfo Member in this.typeInfo.GetMembers(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
			{
				if ((FI = Member as FieldInfo) != null)
				{
					PI = null;
					MemberType = FI.FieldType;
				}
				else if ((PI = Member as PropertyInfo) != null)
				{
					if (PI.GetMethod is null || PI.SetMethod is null)
						continue;

					if (PI.GetIndexParameters().Length > 0)
						continue;

					MemberType = PI.PropertyType;
				}
				else
					continue;

				MemberTypeInfo = MemberType.GetTypeInfo();
				Ignore = false;
				ByReference = false;
				Nullable = false;

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
					else if (Attr is ByReferenceAttribute)
						ByReference = true;
				}

				if (Ignore)
					continue;

				if (Type.GetTypeCode(MemberType) == TypeCode.Object &&
					!MemberType.IsArray &&
					!ByReference &&
					MemberType != typeof(TimeSpan) &&
					MemberType != typeof(Guid) &&
					MemberType != typeof(DateTimeOffset) &&
					MemberType != typeof(CaseInsensitiveString))
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
			CSharp.AppendLine("\t\tpublic override Type ValueType { get { return typeof(" + this.type.FullName + "); } }");
			CSharp.AppendLine();
			CSharp.AppendLine("\t\tpublic override bool IsNullable { get { return " + (this.isNullable ? "true" : "false") + "; } }");
			CSharp.AppendLine();
			CSharp.AppendLine("\t\tpublic override object Deserialize(IBsonReader Reader, BsonType? DataType, bool Embedded)");
			CSharp.AppendLine("\t\t{");
			CSharp.AppendLine("\t\t\tstring FieldName;");
			CSharp.AppendLine("\t\t\tBsonType FieldType;");
			CSharp.AppendLine("\t\t\t" + TypeName + " Result;");
			CSharp.AppendLine("\t\t\tBsonReaderBookmark Bookmark = Reader.GetBookmark();");
			CSharp.AppendLine("\t\t\tBsonType? DataTypeBak = DataType;");
			CSharp.AppendLine();
			CSharp.AppendLine("\t\t\tif (!DataType.HasValue)");
			CSharp.AppendLine("\t\t\t{");
			CSharp.AppendLine("\t\t\t\tDataType = Reader.ReadBsonType();");
			CSharp.AppendLine("\t\t\t\tswitch (DataType.Value)");
			CSharp.AppendLine("\t\t\t\t{");
			CSharp.AppendLine("\t\t\t\t\tcase BsonType.Null: return null;");
			CSharp.AppendLine("\t\t\t\t\tcase BsonType.Document: break;");
			CSharp.AppendLine("\t\t\t\t\tdefault: throw new Exception(\"Expected object document or null.\");");
			CSharp.AppendLine("\t\t\t\t}");
			CSharp.AppendLine("\t\t\t}");

			if (this.typeNameSerialization != TypeNameSerialization.None)
			{
				CSharp.AppendLine();
				CSharp.AppendLine("\t\t\tReader.ReadStartDocument();");
				CSharp.AppendLine("\t\t\tif (!Reader.FindElement(\"" + Escape(this.typeFieldName) + "\"))");
				CSharp.AppendLine("\t\t\t\tthrow new Exception(\"Type name not available.\");");
				CSharp.AppendLine();
				CSharp.AppendLine("\t\t\tstring TypeName = Reader.ReadString();");

				if (this.typeNameSerialization == TypeNameSerialization.LocalName)
					CSharp.AppendLine("\t\t\tTypeName = \"" + this.type.Namespace + ".\" + TypeName;");

				CSharp.AppendLine();
				CSharp.AppendLine("\t\t\tType DesiredType = Waher.Runtime.Inventory.Types.GetType(TypeName);");
				CSharp.AppendLine("\t\t\tif (DesiredType is null)");
				CSharp.AppendLine("\t\t\t\tDesiredType = typeof(GenericObject);");
				CSharp.AppendLine();
				CSharp.AppendLine("\t\t\tReader.ReturnToBookmark(Bookmark);");
				CSharp.AppendLine();
				CSharp.AppendLine("\t\t\tif (DesiredType != typeof(" + this.type.FullName + "))");
				CSharp.AppendLine("\t\t\t{");
				CSharp.AppendLine("\t\t\t\tIObjectSerializer Serializer2 = this.provider.GetObjectSerializer(DesiredType);");
				CSharp.AppendLine("\t\t\t\treturn Serializer2.Deserialize(Reader, DataType, Embedded);");
				CSharp.AppendLine("\t\t\t}");
			}

			if (this.typeInfo.IsAbstract)
				CSharp.AppendLine("\t\t\tthrow new Exception(\"Unable to create an instance of an abstract class.\");");
			else
			{
				CSharp.AppendLine();
				CSharp.AppendLine("\t\t\tReader.ReadStartDocument();");
				CSharp.AppendLine("\t\t\tResult = new " + this.type.FullName + "();");
				CSharp.AppendLine();
				CSharp.AppendLine("\t\t\twhile (Reader.State == BsonReaderState.Type)");
				CSharp.AppendLine("\t\t\t{");
				CSharp.AppendLine("\t\t\t\tFieldType = Reader.ReadBsonType();");
				CSharp.AppendLine("\t\t\t\tif (FieldType == BsonType.EndOfDocument)");
				CSharp.AppendLine("\t\t\t\t\tbreak;");
				CSharp.AppendLine();
				CSharp.AppendLine("\t\t\t\tFieldName = Reader.ReadName();");
				CSharp.AppendLine();
				CSharp.AppendLine("\t\t\t\tswitch (FieldName)");
				CSharp.AppendLine("\t\t\t\t{");

				foreach (MemberInfo Member in this.typeInfo.GetMembers(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
				{
					if ((FI = Member as FieldInfo) != null)
					{
						PI = null;
						MemberType = FI.FieldType;
					}
					else if ((PI = Member as PropertyInfo) != null)
					{
						if (PI.GetMethod is null || PI.SetMethod is null)
							continue;

						if (PI.GetIndexParameters().Length > 0)
							continue;

						MemberType = PI.PropertyType;
					}
					else
						continue;

					MemberTypeInfo = MemberType.GetTypeInfo();
					Ignore = false;
					ShortName = null;
					ObjectIdField = false;
					ByReference = false;
					Nullable = false;

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

						if (Attr is ObjectIdAttribute)
							ObjectIdField = true;

						if (Attr is ByReferenceAttribute)
							ByReference = true;

						if (Attr is ShortNameAttribute)
							ShortName = ((ShortNameAttribute)Attr).Name;
					}

					if (Ignore)
						continue;

					if (ObjectIdField)
					{
						HasObjectId = true;

						CSharp.AppendLine("\t\t\t\t\tcase \"_id\":");
						CSharp.AppendLine("\t\t\t\t\t\tswitch (FieldType)");
						CSharp.AppendLine("\t\t\t\t\t\t{");
						CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.ObjectId:");
						CSharp.AppendLine("\t\t\t\t\t\t\t\tObjectId ObjectId = Reader.ReadObjectId();");

						if (MemberType == typeof(ObjectId))
							CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = ObjectId;");
						else if (MemberType == typeof(string))
							CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = ObjectId.ToString();");
						else if (MemberType == typeof(byte[]))
							CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = ObjectId.ToByteArray();");
						else if (MemberType == typeof(Guid))
							CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = ObjectIdToGuid(ObjectId);");

						CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
						CSharp.AppendLine();
						CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
						CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Object ID parameter _id must be an Object ID value, but was a \" + FieldType.ToString() + \".\");");
						CSharp.AppendLine("\t\t\t\t\t\t}");
					}
					else
					{
						string MemberName = Member.Name;
						CSharp.AppendLine("\t\t\t\t\tcase \"" + MemberName + "\":");

						if (!string.IsNullOrEmpty(ShortName) && ShortName != MemberName)
							CSharp.AppendLine("\t\t\t\t\tcase \"" + ShortName + "\":");

						if (MemberTypeInfo.IsEnum)
						{
							CSharp.AppendLine("\t\t\t\t\t\tswitch (FieldType)");
							CSharp.AppendLine("\t\t\t\t\t\t{");
							CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Boolean:");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = (" + MemberType.FullName + ")(Reader.ReadBoolean() ? 1 : 0);");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
							CSharp.AppendLine();
							CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Double:");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = (" + MemberType.FullName + ")(int)Reader.ReadDouble();");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
							CSharp.AppendLine();
							CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int32:");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = (" + MemberType.FullName + ")Reader.ReadInt32();");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
							CSharp.AppendLine();
							CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int64:");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = (" + MemberType.FullName + ")Reader.ReadInt64();");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
							CSharp.AppendLine();
							CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.String:");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = (" + MemberType.FullName + ")Enum.Parse(typeof(" + MemberType.FullName + "), Reader.ReadString());");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
							if (Nullable)
							{
								CSharp.AppendLine();
								CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Null:");
								CSharp.AppendLine("\t\t\t\t\t\t\t\tReader.ReadNull();");
								CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = null;");
								CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
							}
							CSharp.AppendLine();
							CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Unable to set " + MemberName + ". Expected an enumeration value, but was a \" + FieldType.ToString() + \".\");");
							CSharp.AppendLine("\t\t\t\t\t\t}");
						}
						else
						{
							switch (Type.GetTypeCode(MemberType))
							{
								case TypeCode.Boolean:
									CSharp.AppendLine("\t\t\t\t\t\tswitch (FieldType)");
									CSharp.AppendLine("\t\t\t\t\t\t{");
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Boolean:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = Reader.ReadBoolean();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Double:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = Reader.ReadDouble() != 0;");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int32:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = Reader.ReadInt32() != 0;");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int64:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = Reader.ReadInt64() != 0;");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									if (Nullable)
									{
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Null:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tReader.ReadNull();");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = null;");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									}
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Unable to set " + MemberName + ". Expected a boolean value, but was a \" + FieldType.ToString() + \".\");");
									CSharp.AppendLine("\t\t\t\t\t\t}");
									break;

								case TypeCode.Byte:
									CSharp.AppendLine("\t\t\t\t\t\tswitch (FieldType)");
									CSharp.AppendLine("\t\t\t\t\t\t{");
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Boolean:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = Reader.ReadBoolean() ? (byte)1 : (byte)0;");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Double:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = (byte)Reader.ReadDouble();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int32:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = (byte)Reader.ReadInt32();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int64:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = (byte)Reader.ReadInt64();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.String:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = byte.Parse(Reader.ReadString());");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									if (Nullable)
									{
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Null:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tReader.ReadNull();");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = null;");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									}
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Unable to set " + MemberName + ". Expected a byte value, but was a \" + FieldType.ToString() + \".\");");
									CSharp.AppendLine("\t\t\t\t\t\t}");
									break;

								case TypeCode.Char:
									CSharp.AppendLine("\t\t\t\t\t\tswitch (FieldType)");
									CSharp.AppendLine("\t\t\t\t\t\t{");
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.String:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tstring " + MemberName + " = Reader.ReadString();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = string.IsNullOrEmpty(" + MemberName + ") ? (char)0 : " + MemberName + "[0];");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Symbol:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tstring " + MemberName + " = Reader.ReadSymbol();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = string.IsNullOrEmpty(" + MemberName + ") ? (char)0 : " + MemberName + "[0];");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Double:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = (char)Reader.ReadDouble();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int32:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = (char)Reader.ReadInt32();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int64:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = (char)Reader.ReadInt64();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									if (Nullable)
									{
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Null:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tReader.ReadNull();");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = null;");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									}
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Unable to set " + MemberName + ". Expected a char value, but was a \" + FieldType.ToString() + \".\");");
									CSharp.AppendLine("\t\t\t\t\t\t}");
									break;

								case TypeCode.DateTime:
									CSharp.AppendLine("\t\t\t\t\t\tswitch (FieldType)");
									CSharp.AppendLine("\t\t\t\t\t\t{");
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.DateTime:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = " + typeof(ObjectSerializer).FullName + ".UnixEpoch.AddMilliseconds(Reader.ReadDateTime());");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.String:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = DateTime.Parse(Reader.ReadString());");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									if (Nullable)
									{
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Null:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tReader.ReadNull();");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = null;");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									}
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Unable to set " + MemberName + ". Expected a DateTime value, but was a \" + FieldType.ToString() + \".\");");
									CSharp.AppendLine("\t\t\t\t\t\t}");
									break;

								case TypeCode.Decimal:
									CSharp.AppendLine("\t\t\t\t\t\tswitch (FieldType)");
									CSharp.AppendLine("\t\t\t\t\t\t{");
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Boolean:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = Reader.ReadBoolean() ? 1 : 0;");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Double:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = (decimal)Reader.ReadDouble();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int32:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = Reader.ReadInt32();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int64:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = Reader.ReadInt64();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.String:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = decimal.Parse(Reader.ReadString());");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									if (Nullable)
									{
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Null:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tReader.ReadNull();");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = null;");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									}
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Unable to set " + MemberName + ". Expected a decimal value, but was a \" + FieldType.ToString() + \".\");");
									CSharp.AppendLine("\t\t\t\t\t\t}");
									break;

								case TypeCode.Double:
									CSharp.AppendLine("\t\t\t\t\t\tswitch (FieldType)");
									CSharp.AppendLine("\t\t\t\t\t\t{");
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Boolean:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = Reader.ReadBoolean() ? 1 : 0;");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Double:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = Reader.ReadDouble();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int32:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = Reader.ReadInt32();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int64:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = Reader.ReadInt64();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.String:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = double.Parse(Reader.ReadString());");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									if (Nullable)
									{
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Null:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tReader.ReadNull();");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = null;");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									}
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Unable to set " + MemberName + ". Expected a double-precision value, but was a \" + FieldType.ToString() + \".\");");
									CSharp.AppendLine("\t\t\t\t\t\t}");
									break;

								case TypeCode.Int16:
									CSharp.AppendLine("\t\t\t\t\t\tswitch (FieldType)");
									CSharp.AppendLine("\t\t\t\t\t\t{");
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Boolean:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = Reader.ReadBoolean() ? (short)1 : (short)0;");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Double:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = (short)Reader.ReadDouble();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int32:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = (short)Reader.ReadInt32();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int64:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = (short)Reader.ReadInt64();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.String:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = short.Parse(Reader.ReadString());");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									if (Nullable)
									{
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Null:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tReader.ReadNull();");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = null;");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									}
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Unable to set " + MemberName + ". Expected an Int16 value, but was a \" + FieldType.ToString() + \".\");");
									CSharp.AppendLine("\t\t\t\t\t\t}");
									break;

								case TypeCode.Int32:
									CSharp.AppendLine("\t\t\t\t\t\tswitch (FieldType)");
									CSharp.AppendLine("\t\t\t\t\t\t{");
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Boolean:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = Reader.ReadBoolean() ? 1 : 0;");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Double:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = (int)Reader.ReadDouble();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int32:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = Reader.ReadInt32();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int64:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = (int)Reader.ReadInt64();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.String:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = int.Parse(Reader.ReadString());");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									if (Nullable)
									{
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Null:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tReader.ReadNull();");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = null;");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									}
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Unable to set " + MemberName + ". Expected an Int32 value, but was a \" + FieldType.ToString() + \".\");");
									CSharp.AppendLine("\t\t\t\t\t\t}");
									break;

								case TypeCode.Int64:
									CSharp.AppendLine("\t\t\t\t\t\tswitch (FieldType)");
									CSharp.AppendLine("\t\t\t\t\t\t{");
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Boolean:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = Reader.ReadBoolean() ? 1 : 0;");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Double:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = (long)Reader.ReadDouble();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int32:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = Reader.ReadInt32();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int64:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = Reader.ReadInt64();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.String:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = long.Parse(Reader.ReadString());");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									if (Nullable)
									{
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Null:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tReader.ReadNull();");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = null;");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									}
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Unable to set " + MemberName + ". Expected an Int64 value, but was a \" + FieldType.ToString() + \".\");");
									CSharp.AppendLine("\t\t\t\t\t\t}");
									break;

								case TypeCode.SByte:
									CSharp.AppendLine("\t\t\t\t\t\tswitch (FieldType)");
									CSharp.AppendLine("\t\t\t\t\t\t{");
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Boolean:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = Reader.ReadBoolean() ? (sbyte)1 : (sbyte)0;");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Double:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = (sbyte)Reader.ReadDouble();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int32:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = (sbyte)Reader.ReadInt32();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int64:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = (sbyte)Reader.ReadInt64();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.String:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = sbyte.Parse(Reader.ReadString());");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									if (Nullable)
									{
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Null:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tReader.ReadNull();");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = null;");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									}
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Unable to set " + MemberName + ". Expected a signed byte value, but was a \" + FieldType.ToString() + \".\");");
									CSharp.AppendLine("\t\t\t\t\t\t}");
									break;

								case TypeCode.Single:
									CSharp.AppendLine("\t\t\t\t\t\tswitch (FieldType)");
									CSharp.AppendLine("\t\t\t\t\t\t{");
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Boolean:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = Reader.ReadBoolean() ? 1 : 0;");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Double:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = (float)Reader.ReadDouble();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int32:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = Reader.ReadInt32();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int64:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = Reader.ReadInt64();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.String:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = float.Parse(Reader.ReadString());");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									if (Nullable)
									{
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Null:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tReader.ReadNull();");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = null;");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									}
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Unable to set " + MemberName + ". Expected a single-precision value, but was a \" + FieldType.ToString() + \".\");");
									CSharp.AppendLine("\t\t\t\t\t\t}");
									break;

								case TypeCode.String:
									CSharp.AppendLine("\t\t\t\t\t\tswitch (FieldType)");
									CSharp.AppendLine("\t\t\t\t\t\t{");
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.String:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = Reader.ReadString();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Symbol:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = Reader.ReadSymbol();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.RegularExpression:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = Reader.ReadRegularExpression().Pattern;");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.JavaScriptWithScope: ");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = Reader.ReadJavaScriptWithScope();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.JavaScript:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = Reader.ReadJavaScript();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Null:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tReader.ReadNull();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = null;");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Unable to set " + MemberName + ". Expected a string value, but was a \" + FieldType.ToString() + \".\");");
									CSharp.AppendLine("\t\t\t\t\t\t}");
									break;

								case TypeCode.UInt16:
									CSharp.AppendLine("\t\t\t\t\t\tswitch (FieldType)");
									CSharp.AppendLine("\t\t\t\t\t\t{");
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Boolean:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = Reader.ReadBoolean() ? (ushort)1 : (ushort)0;");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Double:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = (ushort)Reader.ReadDouble();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int32:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = (ushort)Reader.ReadInt32();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int64:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = (ushort)Reader.ReadInt64();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.String:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = ushort.Parse(Reader.ReadString());");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									if (Nullable)
									{
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Null:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tReader.ReadNull();");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = null;");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									}
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Unable to set " + MemberName + ". Expected an unsigned Int16 value, but was a \" + FieldType.ToString() + \".\");");
									CSharp.AppendLine("\t\t\t\t\t\t}");
									break;

								case TypeCode.UInt32:
									CSharp.AppendLine("\t\t\t\t\t\tswitch (FieldType)");
									CSharp.AppendLine("\t\t\t\t\t\t{");
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Boolean:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = Reader.ReadBoolean() ? (uint)1 : (uint)0;");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Double:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = (uint)Reader.ReadDouble();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int32:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = (uint)Reader.ReadInt32();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int64:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = (uint)Reader.ReadInt64();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.String:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = uint.Parse(Reader.ReadString());");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									if (Nullable)
									{
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Null:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tReader.ReadNull();");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = null;");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									}
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Unable to set " + MemberName + ". Expected an unsigned Int32 value, but was a \" + FieldType.ToString() + \".\");");
									CSharp.AppendLine("\t\t\t\t\t\t}");
									break;

								case TypeCode.UInt64:
									CSharp.AppendLine("\t\t\t\t\t\tswitch (FieldType)");
									CSharp.AppendLine("\t\t\t\t\t\t{");
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Boolean:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = Reader.ReadBoolean() ? (ulong)1 : (ulong)0;");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Double:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = (ulong)Reader.ReadDouble();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int32:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = (ulong)Reader.ReadInt32();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int64:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = (ulong)Reader.ReadInt64();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.String:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = ulong.Parse(Reader.ReadString());");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									if (Nullable)
									{
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Null:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tReader.ReadNull();");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = null;");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									}
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Unable to set " + MemberName + ". Expected an unsigned Int64 value, but was a \" + FieldType.ToString() + \".\");");
									CSharp.AppendLine("\t\t\t\t\t\t}");
									break;

								case TypeCode.Empty:
								default:
									throw new Exception("Invalid member type: " + Member.MemberType.ToString());

								case TypeCode.Object:
									if (MemberType.IsArray)
									{
										Type ElementType = MemberType.GetElementType();

										CSharp.AppendLine("\t\t\t\t\t\tswitch (FieldType)");
										CSharp.AppendLine("\t\t\t\t\t\t{");
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Array:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tList<" + GenericParameterName(ElementType) + "> Elements" + MemberName + " = new List<" + GenericParameterName(ElementType) + ">();");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tIObjectSerializer S = this.provider.GetObjectSerializer(typeof(" + ElementType.FullName + "));");
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\t\tReader.ReadStartArray();");
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\t\twhile (Reader.State != BsonReaderState.EndOfArray)");
										CSharp.AppendLine("\t\t\t\t\t\t\t\t{");
										CSharp.AppendLine("\t\t\t\t\t\t\t\t\tBsonType? ElementType = null;");
										CSharp.AppendLine("\t\t\t\t\t\t\t\t\tif (Reader.State == BsonReaderState.Type)");
										CSharp.AppendLine("\t\t\t\t\t\t\t\t\t{");
										CSharp.AppendLine("\t\t\t\t\t\t\t\t\t\tElementType = Reader.ReadBsonType();");
										CSharp.AppendLine("\t\t\t\t\t\t\t\t\t\tif (ElementType == BsonType.EndOfDocument)");
										CSharp.AppendLine("\t\t\t\t\t\t\t\t\t\t\tbreak;");
										CSharp.AppendLine("\t\t\t\t\t\t\t\t\t}");
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\t\t\tElements" + MemberName + ".Add((" + ElementType.FullName + ")S.Deserialize(Reader, ElementType, Embedded));");
										CSharp.AppendLine("\t\t\t\t\t\t\t\t}");
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\t\tReader.ReadEndArray();");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = Elements" + MemberName + ".ToArray();");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										CSharp.AppendLine();

										if (MemberType == typeof(byte[]))
										{
											CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Binary:");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = Reader.ReadBytes();");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
											CSharp.AppendLine();
										}

										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Null:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tReader.ReadNull();");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = null;");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Array expected for " + MemberName + ".\");");
										CSharp.AppendLine("\t\t\t\t\t\t}");
									}
									else if (ByReference)
									{
										CSharp.AppendLine("\t\t\t\t\t\tswitch (FieldType)");
										CSharp.AppendLine("\t\t\t\t\t\t{");
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.ObjectId:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tObjectId " + MemberName + "ObjectId = Reader.ReadObjectId();");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tTask<" + GenericParameterName(MemberType) + "> " + MemberName + "Task = this.provider.LoadObject<" + GenericParameterName(MemberType) + ">(" + MemberName + "ObjectId);");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tif (!" + MemberName + "Task.Wait(10000))");
										CSharp.AppendLine("\t\t\t\t\t\t\t\t\tthrow new Exception(\"Unable to load referenced object. Database timed out.\");");
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = " + MemberName + "Task.Result;");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Null:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = null;");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Object ID expected for " + MemberName + ".\");");
										CSharp.AppendLine("\t\t\t\t\t\t}");
									}
									else if (MemberType == typeof(TimeSpan))
									{
										CSharp.AppendLine("\t\t\t\t\t\tswitch (FieldType)");
										CSharp.AppendLine("\t\t\t\t\t\t{");
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.String:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = TimeSpan.Parse(Reader.ReadString());");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										if (Nullable)
										{
											CSharp.AppendLine();
											CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Null:");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tReader.ReadNull();");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = null;");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										}
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Unable to set " + MemberName + ". Expected a string value, but was a \" + FieldType.ToString() + \".\");");
										CSharp.AppendLine("\t\t\t\t\t\t}");
									}
									else if (MemberType == typeof(Guid))
									{
										CSharp.AppendLine("\t\t\t\t\t\tswitch (FieldType)");
										CSharp.AppendLine("\t\t\t\t\t\t{");
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.String:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = Guid.Parse(Reader.ReadString());");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.ObjectId:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = ObjectIdToGuid(Reader.ReadObjectId());");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										if (Nullable)
										{
											CSharp.AppendLine();
											CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Null:");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tReader.ReadNull();");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = null;");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										}
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Unable to set " + MemberName + ". Expected a string value, but was a \" + FieldType.ToString() + \".\");");
										CSharp.AppendLine("\t\t\t\t\t\t}");
									}
									else if (MemberType == typeof(DateTimeOffset))
									{
										CSharp.AppendLine("\t\t\t\t\t\tswitch (FieldType)");
										CSharp.AppendLine("\t\t\t\t\t\t{");
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.DateTime:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = " + typeof(ObjectSerializer).FullName + ".UnixEpoch.AddMilliseconds(Reader.ReadDateTime());");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.String:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = DateTimeOffset.Parse(Reader.ReadString());");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										if (Nullable)
										{
											CSharp.AppendLine();
											CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Null:");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tReader.ReadNull();");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = null;");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										}
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Unable to set " + MemberName + ". Expected a DateTime value, but was a \" + FieldType.ToString() + \".\");");
										CSharp.AppendLine("\t\t\t\t\t\t}");
									}
									else if (MemberType == typeof(CaseInsensitiveString))
									{
										CSharp.AppendLine("\t\t\t\t\t\tswitch (FieldType)");
										CSharp.AppendLine("\t\t\t\t\t\t{");
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.String:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = Reader.ReadString();");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Symbol:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = Reader.ReadSymbol();");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.RegularExpression:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = Reader.ReadRegularExpression().Pattern;");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.JavaScriptWithScope: ");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = Reader.ReadJavaScriptWithScope();");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.JavaScript:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = Reader.ReadJavaScript();");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Null:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tReader.ReadNull();");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = null;");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Unable to set " + MemberName + ". Expected a case-insensitive string value, but was a \" + FieldType.ToString() + \".\");");
										CSharp.AppendLine("\t\t\t\t\t\t}");

										CSharp.AppendLine("\t\t\t\t\t\tbreak;");
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\tcase \"" + MemberName + "_L\":");

										if (!string.IsNullOrEmpty(ShortName) && ShortName != MemberName)
											CSharp.AppendLine("\t\t\t\t\tcase \"" + ShortName + "_L\":");

										CSharp.AppendLine("\t\t\t\t\t\tReader.SkipValue();");
									}
									else
									{
										CSharp.AppendLine("\t\t\t\t\t\tswitch (FieldType)");
										CSharp.AppendLine("\t\t\t\t\t\t{");
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Document:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = (" + MemberType.FullName + ")this.serializer" + MemberName + ".Deserialize(Reader, DataType, Embedded);");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Null:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + MemberName + " = null;");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Document expected for " + MemberName + ".\");");
										CSharp.AppendLine("\t\t\t\t\t\t}");
									}
									break;
							}
						}
					}

					CSharp.AppendLine("\t\t\t\t\t\tbreak;");
					CSharp.AppendLine();
				}

				if (!string.IsNullOrEmpty(this.typeFieldName))
				{
					CSharp.AppendLine("\t\t\t\t\tcase \"" + this.typeFieldName + "\":");
					CSharp.AppendLine("\t\t\t\t\t\tswitch (FieldType)");
					CSharp.AppendLine("\t\t\t\t\t\t{");
					CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.String:");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tReader.ReadString();");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
					CSharp.AppendLine();
					CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Type parameter " + this.typeFieldName + " must be a string value, but was a \" + FieldType.ToString() + \".\");");
					CSharp.AppendLine("\t\t\t\t\t\t}");
					CSharp.AppendLine("\t\t\t\t\t\tbreak;");
					CSharp.AppendLine();
				}

				if (!HasObjectId)
				{
					CSharp.AppendLine("\t\t\t\t\tcase \"_id\":");
					CSharp.AppendLine("\t\t\t\t\t\tswitch (FieldType)");
					CSharp.AppendLine("\t\t\t\t\t\t{");
					CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.ObjectId:");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tReader.ReadObjectId();");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
					CSharp.AppendLine();
					CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Object ID parameter _id must be an Object ID value, but was a \" + FieldType.ToString() + \".\");");
					CSharp.AppendLine("\t\t\t\t\t\t}");
					CSharp.AppendLine("\t\t\t\t\t\tbreak;");
					CSharp.AppendLine();
				}

				CSharp.AppendLine("\t\t\t\t\tdefault:");
				CSharp.AppendLine("\t\t\t\t\t\tthrow new Exception(\"Field name not recognized: \" + FieldName);");

				CSharp.AppendLine("\t\t\t\t}");
				CSharp.AppendLine("\t\t\t}");
				CSharp.AppendLine();
				CSharp.AppendLine("\t\t\tReader.ReadEndDocument();");
				CSharp.AppendLine();
				CSharp.AppendLine("\t\t\treturn Result;");
			}

			CSharp.AppendLine("\t\t}");
			CSharp.AppendLine();
			CSharp.AppendLine("\t\tpublic override void Serialize(IBsonWriter Writer, bool WriteTypeCode, bool Embedded, object UntypedValue)");
			CSharp.AppendLine("\t\t{");
			CSharp.AppendLine("\t\t\t" + TypeName + " Value = (" + TypeName + ")UntypedValue;");
			CSharp.AppendLine();
			CSharp.AppendLine("\t\t\tWriter.WriteStartDocument();");

			switch (this.typeNameSerialization)
			{
				case TypeNameSerialization.LocalName:
					CSharp.Append("\t\t\tWriter.WriteName(\"");
					CSharp.Append(Escape(this.typeFieldName));
					CSharp.AppendLine("\");");

					CSharp.Append("\t\t\tWriter.WriteString(\"");
					CSharp.Append(Escape(this.type.Name));
					CSharp.AppendLine("\");");
					break;

				case TypeNameSerialization.FullName:
					CSharp.Append("\t\t\tWriter.WriteName(\"");
					CSharp.Append(Escape(this.typeFieldName));
					CSharp.AppendLine("\");");

					CSharp.Append("\t\t\tWriter.WriteString(\"");
					CSharp.Append(Escape(this.type.FullName));
					CSharp.AppendLine("\");");
					break;
			}

			foreach (MemberInfo Member in this.typeInfo.GetMembers(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
			{
				if ((FI = Member as FieldInfo) != null)
				{
					PI = null;
					MemberType = FI.FieldType;
				}
				else if ((PI = Member as PropertyInfo) != null)
				{
					if (PI.GetMethod is null || PI.SetMethod is null)
						continue;

					if (PI.GetIndexParameters().Length > 0)
						continue;

					MemberType = PI.PropertyType;
				}
				else
					continue;

				MemberTypeInfo = MemberType.GetTypeInfo();
				Ignore = false;
				ShortName = null;
				HasDefaultValue = false;
				DefaultValue = null;
				ObjectIdField = false;
				ByReference = false;
				Nullable = false;

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
					CSharp.AppendLine();

					if (DefaultValue is null)
						CSharp.AppendLine("\t\t\tif (!((object)Value." + Member.Name + " is null))");
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

					CSharp.Append(Indent);
					CSharp.AppendLine("if (ObjectId != null)");

					CSharp.Append(Indent);
					CSharp.AppendLine("{");

					CSharp.Append(Indent);
					CSharp.AppendLine("\tWriter.WriteName(\"_id\");");

					if (MemberType == typeof(ObjectId))
					{
						CSharp.Append(Indent);
						CSharp.AppendLine("\tWriter.WriteObjectId(ObjectId);");
					}
					else if (MemberType == typeof(string) || MemberType == typeof(byte[]))
					{
						CSharp.Append(Indent);
						CSharp.AppendLine("\tWriter.WriteObjectId(new ObjectId(ObjectId));");
					}
					else if (MemberType == typeof(Guid))
					{
						CSharp.Append(Indent);
						CSharp.AppendLine("\tWriter.WriteObjectId(GuidToObjectId(ObjectId));");
					}
					else
						throw new Exception("Invalid Object ID type.");

					CSharp.Append(Indent);
					CSharp.AppendLine("}");
				}
				else
				{
					CSharp.Append("Writer.WriteName(\"");

					if (!string.IsNullOrEmpty(ShortName))
						CSharp.Append(Escape(ShortName));
					else
						CSharp.Append(Escape(Member.Name));

					CSharp.AppendLine("\");");

					if (Nullable)
					{
						Indent2 = Indent + "\t";

						CSharp.Append(Indent);
						CSharp.Append("if (!Value.");
						CSharp.Append(Member.Name);
						CSharp.AppendLine(".HasValue)");
						CSharp.Append(Indent2);
						CSharp.AppendLine("Writer.WriteNull();");
						CSharp.Append(Indent);
						CSharp.AppendLine("else");
						CSharp.AppendLine("{");
					}
					else
						Indent2 = Indent;

					if (MemberTypeInfo.IsEnum)
					{
						CSharp.Append(Indent2);
						CSharp.Append("Writer.WriteInt32((int)Value.");
						CSharp.Append(Member.Name);
						if (Nullable)
							CSharp.Append(".Value");
						CSharp.AppendLine(");");
					}
					else
					{
						switch (Type.GetTypeCode(MemberType))
						{
							case TypeCode.Boolean:
								CSharp.Append(Indent2);
								CSharp.Append("Writer.WriteBoolean(Value.");
								CSharp.Append(Member.Name);
								if (Nullable)
									CSharp.Append(".Value");
								CSharp.AppendLine(");");
								break;

							case TypeCode.Byte:
								CSharp.Append(Indent2);
								CSharp.Append("Writer.WriteInt32(Value.");
								CSharp.Append(Member.Name);
								if (Nullable)
									CSharp.Append(".Value");
								CSharp.AppendLine(");");
								break;

							case TypeCode.Char:
								CSharp.Append(Indent2);
								CSharp.Append("Writer.WriteInt32((int)Value.");
								CSharp.Append(Member.Name);
								if (Nullable)
									CSharp.Append(".Value");
								CSharp.AppendLine(");");
								break;

							case TypeCode.DateTime:
								CSharp.Append(Indent2);
								CSharp.Append("Writer.WriteDateTime((long)((Value.");
								CSharp.Append(Member.Name);
								if (Nullable)
									CSharp.Append(".Value");
								CSharp.Append(".ToUniversalTime() - ");
								CSharp.Append(typeof(ObjectSerializer).FullName);
								CSharp.AppendLine(".UnixEpoch).TotalMilliseconds + 0.5));");
								break;

							case TypeCode.Decimal:
								CSharp.Append(Indent2);
								CSharp.Append("Writer.WriteDouble((double)Value.");
								CSharp.Append(Member.Name);
								if (Nullable)
									CSharp.Append(".Value");
								CSharp.AppendLine(");");
								break;

							case TypeCode.Double:
							case TypeCode.Single:
								CSharp.Append(Indent2);
								CSharp.Append("Writer.WriteDouble(Value.");
								CSharp.Append(Member.Name);
								if (Nullable)
									CSharp.Append(".Value");
								CSharp.AppendLine(");");
								break;

							case TypeCode.Int32:
							case TypeCode.Int16:
							case TypeCode.UInt16:
							case TypeCode.SByte:
								CSharp.Append(Indent2);
								CSharp.Append("Writer.WriteInt32(Value.");
								CSharp.Append(Member.Name);
								if (Nullable)
									CSharp.Append(".Value");
								CSharp.AppendLine(");");
								break;

							case TypeCode.Int64:
							case TypeCode.UInt32:
								CSharp.Append(Indent2);
								CSharp.Append("Writer.WriteInt64(Value.");
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
								CSharp.AppendLine("\tWriter.WriteNull();");
								CSharp.Append(Indent2);
								CSharp.AppendLine("else");
								CSharp.Append(Indent2);
								CSharp.Append("\tWriter.WriteString(Value.");
								CSharp.Append(Member.Name);
								CSharp.AppendLine(");");
								break;

							case TypeCode.UInt64:
								CSharp.Append(Indent2);
								CSharp.Append("Writer.WriteInt64((long)Value.");
								CSharp.Append(Member.Name);
								if (Nullable)
									CSharp.Append(".Value");
								CSharp.AppendLine(");");
								break;

							case TypeCode.Empty:
							default:
								throw new Exception("Invalid member type: " + Member.MemberType.ToString());

							case TypeCode.Object:
								if (MemberType == typeof(byte[]))
								{
									CSharp.Append(Indent2);
									CSharp.AppendLine("if (Value." + Member.Name + " is null)");
									CSharp.Append(Indent2);
									CSharp.AppendLine("\tWriter.WriteNull();");
									CSharp.Append(Indent2);
									CSharp.AppendLine("else");
									CSharp.Append(Indent2);
									CSharp.Append("\tWriter.WriteBytes(Value.");
									CSharp.Append(Member.Name);
									CSharp.AppendLine(");");
								}
								else if (MemberType.IsArray)
								{
									CSharp.Append(Indent2);
									CSharp.Append("Writer.WriteStartArray();");
									CSharp.AppendLine();

									CSharp.Append(Indent2);
									CSharp.Append("foreach (object Item in Value.");
									CSharp.Append(Member.Name);
									CSharp.AppendLine(")");

									CSharp.Append(Indent2);
									CSharp.AppendLine("{");

									CSharp.Append(Indent2);
									CSharp.AppendLine("\tif (Item is null)");
									CSharp.Append(Indent2);
									CSharp.AppendLine("\t\tWriter.WriteNull();");
									CSharp.Append(Indent2);
									CSharp.AppendLine("\telse");
									CSharp.Append(Indent2);
									CSharp.AppendLine("\t{");

									CSharp.Append(Indent2);
									CSharp.AppendLine("\t\tType T = Item.GetType();");
									CSharp.AppendLine("\t\tIObjectSerializer S;");
									CSharp.AppendLine();
									CSharp.Append(Indent2);
									CSharp.AppendLine("\t\tS = this.provider.GetObjectSerializer(T);");
									CSharp.AppendLine();
									CSharp.Append(Indent2);
									CSharp.AppendLine("\t\tS.Serialize(Writer, true, true, Item);");
									CSharp.Append(Indent2);
									CSharp.AppendLine("\t}");

									CSharp.Append(Indent2);
									CSharp.AppendLine("}");
									CSharp.AppendLine();

									CSharp.Append(Indent2);
									CSharp.AppendLine("Writer.WriteEndArray();");
								}
								else if (ByReference)
								{
									CSharp.Append(Indent2);
									CSharp.AppendLine("if (Value." + Member.Name + " is null)");
									CSharp.Append(Indent2);
									CSharp.AppendLine("\tWriter.WriteNull();");
									CSharp.Append(Indent2);
									CSharp.AppendLine("else");
									CSharp.Append(Indent2);
									CSharp.AppendLine("{");
									CSharp.Append(Indent2);
									CSharp.AppendLine("\tObjectSerializer Serializer" + Member.Name + " = this.provider.GetObjectSerializerEx(typeof(" + MemberType.FullName + "));");
									CSharp.Append(Indent2);
									CSharp.AppendLine("\tWriter.WriteObjectId(Serializer" + Member.Name + ".GetObjectId(Value." + Member.Name + ", true).Result);");
									CSharp.Append(Indent2);
									CSharp.AppendLine("}");
								}
								else if (MemberType == typeof(TimeSpan))
								{
									CSharp.Append(Indent2);
									CSharp.Append("Writer.WriteString(Value.");
									CSharp.Append(Member.Name);
									if (Nullable)
										CSharp.Append(".Value");
									CSharp.AppendLine(".ToString());");
								}
								else if (MemberType == typeof(Guid))
								{
									CSharp.Append(Indent2);
									CSharp.Append("if (TryConvertToObjectId(Value.");
									CSharp.Append(Member.Name);
									CSharp.Append(", out ObjectId ");
									CSharp.Append(Member.Name);
									CSharp.AppendLine("ObjId))");
									CSharp.Append(Indent2);
									CSharp.Append("\tWriter.WriteObjectId(");
									CSharp.Append(Member.Name);
									CSharp.AppendLine("ObjId);");
									CSharp.Append(Indent2);
									CSharp.Append("else");
									CSharp.Append(Indent2);
									CSharp.Append("\tWriter.WriteString(Value.");
									CSharp.Append(Member.Name);
									if (Nullable)
										CSharp.Append(".Value");
									CSharp.AppendLine(".ToString());");
								}
								else if (MemberType == typeof(DateTimeOffset))
								{
									CSharp.Append(Indent2);
									CSharp.Append("Writer.WriteString(Value.");
									CSharp.Append(Member.Name);
									if (Nullable)
										CSharp.Append(".Value");
									CSharp.AppendLine(".ToString());");
								}
								else if (MemberType == typeof(CaseInsensitiveString))
								{
									CSharp.Append(Indent2);
									CSharp.Append("if (Value.");
									CSharp.Append(Member.Name);
									CSharp.AppendLine(" is null)");
									CSharp.Append(Indent2);
									CSharp.AppendLine("\tWriter.WriteNull();");
									CSharp.Append(Indent2);
									CSharp.AppendLine("else");
									CSharp.Append(Indent2);
									CSharp.AppendLine("{");
									CSharp.Append(Indent2);
									CSharp.Append("\tWriter.WriteString(Value.");
									CSharp.Append(Member.Name);
									CSharp.AppendLine(".Value);");

									CSharp.Append(Indent2);
									CSharp.Append("\tWriter.WriteName(\"");

									if (!string.IsNullOrEmpty(ShortName))
										CSharp.Append(Escape(ShortName));
									else
										CSharp.Append(Escape(Member.Name));

									CSharp.AppendLine("_L\");");

									CSharp.Append(Indent2);
									CSharp.Append("\tWriter.WriteString(Value.");
									CSharp.Append(Member.Name);
									CSharp.AppendLine(".LowerCase);");
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
			CSharp.AppendLine("\t\t\tWriter.WriteEndDocument();");
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

			CSharp.AppendLine();
			CSharp.AppendLine("\t\tpublic override bool TryGetFieldType(string FieldName, object UntypedObject, out Type FieldType)");
			CSharp.AppendLine("\t\t{");
			CSharp.AppendLine("\t\t\tswitch (FieldName)");
			CSharp.AppendLine("\t\t\t{");

			foreach (KeyValuePair<string, Type> P in this.memberTypes)
			{
				CSharp.AppendLine("\t\t\t\tcase \"" + P.Key + "\":");
				CSharp.AppendLine("\t\t\t\t\tFieldType = typeof(" + GenericParameterName(P.Value) + ");");
				CSharp.AppendLine("\t\t\t\t\treturn true;");
				CSharp.AppendLine();
			}

			CSharp.AppendLine("\t\t\t\tdefault:");
			CSharp.AppendLine("\t\t\t\t\tFieldType = null;");
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
				{ GetLocation(typeof(MultiReadSingleWriteObject)), true },
				{ GetLocation(typeof(MongoClient)), true },
				{ GetLocation(typeof(BsonDocument)), true }
			};

			System.Reflection.TypeInfo LoopInfo;
			Type Loop = Type;
			string s = Path.Combine(Path.GetDirectoryName(GetLocation(typeof(object))), "netstandard.dll");

			if (File.Exists(s))
				Dependencies[s] = true;

			while (Loop != null)
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
					if (FI != null && !((s = GetLocation(FI.FieldType)).EndsWith("mscorlib.dll") || s.EndsWith("System.Runtime.dll") || s.EndsWith("System.Private.CoreLib.dll")))
						Dependencies[s] = true;

					PI = MI2 as PropertyInfo;
					if (PI != null && !((s = GetLocation(PI.PropertyType)).EndsWith("mscorlib.dll") || s.EndsWith("System.Runtime.dll") || s.EndsWith("System.Private.CoreLib.dll")))
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

			CSharpCompilation Compilation = CSharpCompilation.Create("WPMA." + this.type.FullName,
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

			A = AssemblyLoadContext.Default.LoadFromStream(Output, PdbOutput);
			Type T = A.GetType(Type.Namespace + ".Bson.BsonSerializer" + TypeName + this.provider.Id);
			this.customSerializer = (IObjectSerializer)Activator.CreateInstance(T, this.provider);

			BsonSerializer.RegisterSerializer(Type, this);

			IMongoCollection<BsonDocument> Collection = null;
			List<BsonDocument> Indices = null;

			foreach (IndexAttribute CompoundIndexAttribute in this.typeInfo.GetCustomAttributes<IndexAttribute>(true))
			{
				if (Collection is null)
				{
					if (string.IsNullOrEmpty(this.collectionName))
						Collection = this.provider.DefaultCollection;
					else
						Collection = this.provider.GetCollection(this.collectionName);

					IAsyncCursor<BsonDocument> Cursor = Collection.Indexes.List();
					Indices = Cursor.ToList<BsonDocument>();
				}

				Task T2 = CheckIndexExists(Collection, Indices, CompoundIndexAttribute.FieldNames, this);
			}
		}

		internal static async Task CheckIndexExists(IMongoCollection<BsonDocument> Collection, List<BsonDocument> Indices,
			string[] FieldNames, ObjectSerializer Serializer)
		{
			foreach (BsonDocument Index in Indices)
			{
				BsonDocument Key = Index["key"].AsBsonDocument;
				if (Key.ElementCount != FieldNames.Length)
					continue;

				IEnumerator<BsonElement> e1 = Key.Elements.GetEnumerator();
				IEnumerator e2 = FieldNames.GetEnumerator();

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
					return;
			}

			IndexKeysDefinition<BsonDocument> NewIndex = null;

			foreach (string FieldName in FieldNames)
			{
				if (NewIndex is null)
				{
					if (FieldName.StartsWith("-"))
						NewIndex = Builders<BsonDocument>.IndexKeys.Descending(Serializer?.ToShortName(FieldName.Substring(1)) ?? FieldName.Substring(1));
					else
						NewIndex = Builders<BsonDocument>.IndexKeys.Ascending(Serializer?.ToShortName(FieldName) ?? FieldName);
				}
				else
				{
					if (FieldName.StartsWith("-"))
						NewIndex = NewIndex.Descending(Serializer?.ToShortName(FieldName.Substring(1)) ?? FieldName.Substring(1));
					else
						NewIndex = NewIndex.Ascending(Serializer?.ToShortName(FieldName) ?? FieldName);
				}
			}

			await Collection.Indexes.CreateOneAsync(NewIndex);
		}

		private static string GetLocation(Type T)
		{
			System.Reflection.TypeInfo TI = T.GetTypeInfo();
			string s = TI.Assembly.Location;

			if (!string.IsNullOrEmpty(s))
				return s;

			return Path.Combine(Path.GetDirectoryName(GetLocation(typeof(Database))), TI.Module.ScopeName);
		}

		/// <summary>
		/// Escapes a string to be enclosed between double quotes.
		/// </summary>
		/// <param name="s"></param>
		/// <returns>String with special characters escaped.</returns>
		public static string Escape(string s)
		{
			if (s is null || s.IndexOfAny(specialCharacters) < 0)
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
		/// UNIX Epoch, started at 1970-01-01, 00:00:00 (GMT)
		/// </summary>
		public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

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
		/// Provider reference.
		/// </summary>
		protected MongoDBProvider Provider => this.provider;

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <param name="context">The deserialization context.</param>
		/// <param name="args">The deserialization args.</param>
		/// <returns>A deserialized value.</returns>
		public object Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
		{
			return this.Deserialize(context.Reader, null, false);
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="context">The serialization context.</param>
		/// <param name="args">The serialization args.</param>
		/// <param name="value">The value.</param>
		public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
		{
			this.Serialize(context.Writer, true, false, value);
		}

		/// <summary>
		/// Deserializes an object from a binary source.
		/// </summary>
		/// <param name="Reader">Binary deserializer.</param>n
		/// <param name="DataType">Optional datatype. If not provided, will be read from the binary source.</param>
		/// <param name="Embedded">If the object is embedded into another.</param>
		/// <returns>Deserialized object.</returns>
		public virtual object Deserialize(IBsonReader Reader, BsonType? DataType, bool Embedded)
		{
			return this.customSerializer.Deserialize(Reader, DataType, Embedded);
		}

		/// <summary>
		/// Serializes an object to a binary destination.
		/// </summary>
		/// <param name="Writer">Binary destination.</param>
		/// <param name="WriteTypeCode">If a type code is to be output.</param>
		/// <param name="Embedded">If the object is embedded into another.</param>
		/// <param name="Value">The actual object to serialize.</param>
		public virtual void Serialize(IBsonWriter Writer, bool WriteTypeCode, bool Embedded, object Value)
		{
			this.customSerializer.Serialize(Writer, WriteTypeCode, Embedded, Value);
		}

		/// <summary>
		/// Converts a field name to its corresponding short name. If no explicit short name is available, the same field name is returned.
		/// </summary>
		/// <param name="FieldName">Field Name.</param>
		/// <returns>Short name, if found, or the field name itself, if not.</returns>
		public string ToShortName(string FieldName)
		{
			object Value = null;

			return this.ToShortName(FieldName, ref Value);
		}

		/// <summary>
		/// Converts a field name to its corresponding short name. If no explicit short name is available, the same field name is returned.
		/// </summary>
		/// <param name="FieldName">Field Name.</param>
		/// <param name="Value">Field value.</param>
		/// <returns>Short name, if found, or the field name itself, if not.</returns>
		public string ToShortName(string FieldName, ref object Value)
		{
			string Name;
			string Result;
			int i;

			i = FieldName.IndexOf('.');
			if (i < 0)
				Name = FieldName;
			else
				Name = FieldName.Substring(0, i);

			if (this.shortNamesByFieldName.TryGetValue(FieldName, out string s))
				Result = s;
			else if (FieldName == this.ObjectIdMemberName)
			{
				if (Value is string s2)
					Value = new ObjectId(s2);

				Result = "_id";
			}
			else
				Result = Name;

			if (i >= 0)
			{
				if (this.memberTypes.TryGetValue(Name, out Type T))
				{
					IObjectSerializer S2;

					if (T.IsArray)
						S2 = this.provider.GetObjectSerializer(T.GetElementType());
					else
						S2 = this.provider.GetObjectSerializer(T);

					if (S2 is ObjectSerializer S3)
						Result += "." + S3.ToShortName(FieldName.Substring(i + 1), ref Value);
					else
						Result += FieldName.Substring(i);
				}
				else
					Result += FieldName.Substring(i);
			}

			return Result;
		}

		/// <summary>
		/// Mamber name of the field or property holding the Object ID, if any. If there are no such member, this property returns null.
		/// </summary>
		public virtual string ObjectIdMemberName
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
		public virtual bool HasObjectIdField
		{
			get
			{
				return this.objectIdFieldInfo != null || this.objectIdPropertyInfo != null;
			}
		}

		public virtual bool IsNullable => this.isNullable;

		/// <summary>
		/// If the class has an Object ID.
		/// </summary>
		/// <param name="Value">Object reference.</param>
		public virtual bool HasObjectId(object Value)
		{
			object OID;

			if (this.objectIdFieldInfo != null)
				OID = this.objectIdFieldInfo.GetValue(Value);
			else if (this.objectIdPropertyInfo != null)
				OID = this.objectIdPropertyInfo.GetValue(Value);
			else
				return false;

			if (OID is null)
				return false;

			if (OID is string s)
				return !string.IsNullOrEmpty(s);
			else if (OID is Guid Guid)
				return Guid != Guid.Empty;
			else if (OID is ObjectId ObjId)
				return ObjId != ObjectId.Empty;
			else
				return true;
		}

		/// <summary>
		/// Gets the Object ID for a given object.
		/// </summary>
		/// <param name="Value">Object reference.</param>
		/// <param name="InsertIfNotFound">Insert object into database with new Object ID, if no Object ID is set.</param>
		/// <returns>Object ID for <paramref name="Value"/>.</returns>
		public virtual async Task<ObjectId> GetObjectId(object Value, bool InsertIfNotFound)
		{
			object Obj;

			if (this.objectIdFieldInfo != null)
				Obj = this.objectIdFieldInfo.GetValue(Value);
			else if (this.objectIdPropertyInfo != null)
				Obj = this.objectIdPropertyInfo.GetValue(Value);
			else
				throw new NotSupportedException("No Object ID member found in objects of type " + Value.GetType().FullName + ".");

			if (Obj is null ||
				(Obj is string s && string.IsNullOrEmpty(s)) ||
				(Obj is Guid id && id == Guid.Empty) ||
				(Obj is ObjectId ObjId && ObjId == ObjectId.Empty))
			{
				if (!InsertIfNotFound)
					throw new Exception("Object has no Object ID defined.");

				Type ValueType = Value.GetType();
				ObjectSerializer Serializer = this.provider.GetObjectSerializerEx(ValueType);
				string CollectionName = Serializer.CollectionName(Value);
				IMongoCollection<BsonDocument> Collection;

				if (string.IsNullOrEmpty(CollectionName))
					Collection = this.provider.DefaultCollection;
				else
					Collection = this.provider.GetCollection(CollectionName);

				ObjectId ObjectId = ObjectId.GenerateNewId();
				Type MemberType;

				if (this.objectIdFieldInfo != null)
					MemberType = this.objectIdFieldInfo.FieldType;
				else
					MemberType = this.objectIdPropertyInfo.PropertyType;

				if (MemberType == typeof(ObjectId))
					Obj = ObjectId;
				else if (MemberType == typeof(string))
					Obj = ObjectId.ToString();
				else if (MemberType == typeof(byte[]))
					Obj = ObjectId.ToByteArray();
				else if (MemberType == typeof(Guid))
					Obj = GeneratedObjectSerializerBase.ObjectIdToGuid(ObjectId);
				else
					throw new NotSupportedException("Unsupported type for Object ID members: " + MemberType.FullName);

				if (this.objectIdFieldInfo != null)
					this.objectIdFieldInfo.SetValue(Value, Obj);
				else
					this.objectIdPropertyInfo.SetValue(Value, Obj);

				BsonDocument Doc = Value.ToBsonDocument(ValueType, Serializer);
				await Collection.InsertOneAsync(Doc);

				return ObjectId;
			}
			else if (Obj is ObjectId ObjectId)
				return ObjectId;
			else if (Obj is string s2)
				return new ObjectId(s2);
			else if (Obj is byte[] A)
				return new ObjectId(A);
			else if (Obj is Guid Guid)
				return GeneratedObjectSerializerBase.GuidToObjectId(Guid);
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
			if (!this.defaultValues.TryGetValue(FieldName, out object Default))
				return false;

			if ((Value is null) ^ (Default is null))
				return false;

			if (Value is null)
				return true;

			return Default.Equals(Value);
		}

		/// <summary>
		/// Name of collection objects of this type is to be stored in, if available. If not available, this property returns null.
		/// </summary>
		/// <param name="Object">Object in the current context. If null, the default collection name is requested.</param>
		public virtual string CollectionName(object Object)
		{
			return this.collectionName;
		}

		/// <summary>
		/// Tries to get the serialization info for a member.
		/// </summary>
		/// <param name="memberName">Name of the member.</param>
		/// <param name="serializationInfo">The serialization information.</param>
		/// <returns>true if the serialization info exists; otherwise false.</returns>
		public virtual bool TryGetMemberSerializationInfo(string memberName, out BsonSerializationInfo serializationInfo)
		{
			throw new NotImplementedException();
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
			if (this.customSerializer is null)
			{
				Value = null;
				return false;
			}
			else
				return this.customSerializer.TryGetFieldValue(FieldName, Object, out Value);
		}

		/// <summary>
		/// Gets the type of a field or property of an object, given its name.
		/// </summary>
		/// <param name="FieldName">Name of field or property.</param>
		/// <param name="Object">Object.</param>
		/// <param name="FieldType">Corresponding field or property type, if found, or null otherwise.</param>
		/// <returns>If the corresponding field or property was found.</returns>
		public virtual bool TryGetFieldType(string FieldName, object Object, out Type FieldType)
		{
			if (this.customSerializer is null)
			{
				FieldType = null;
				return false;
			}
			else
				return this.customSerializer.TryGetFieldType(FieldName, Object, out FieldType);
		}

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
	}
}
