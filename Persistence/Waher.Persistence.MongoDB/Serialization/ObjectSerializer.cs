using System;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CSharp;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Waher.Persistence.Attributes;
using Waher.Persistence.Filters;

namespace Waher.Persistence.MongoDB.Serialization
{
	/// <summary>
	/// Serializes a type to BSON, taking into account attributes defined in <see cref="Waher.Persistence.Attributes"/>.
	/// </summary>
	public class ObjectSerializer : IBsonDocumentSerializer
	{
		private Dictionary<string, string> shortNamesByFieldName = new Dictionary<string, string>();
		private Dictionary<string, object> defaultValues = new Dictionary<string, object>();
		private Dictionary<string, Type> memberTypes = new Dictionary<string, Type>();
		private Type type;
		private string collectionName;
		private string typeFieldName;
		private IBsonSerializer customSerializer;
		private TypeNameSerialization typeNameSerialization;
		private MongoDBProvider provider;
		private FieldInfo objectIdFieldInfo = null;
		private PropertyInfo objectIdPropertyInfo = null;

		/// <summary>
		/// Serializes a type to BSON, taking into account attributes defined in <see cref="Waher.Persistence.Attributes"/>.
		/// </summary>
		/// <param name="Type">Type to serialize.</param>
		/// <param name="Provider">MongoDB Provider object.</param>
		public ObjectSerializer(Type Type, MongoDBProvider Provider)
		{
			string TypeName = Type.Name;

			this.type = Type;
			this.provider = Provider;

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
			CSharp.AppendLine("using MongoDB.Bson;");
			CSharp.AppendLine("using MongoDB.Bson.IO;");
			CSharp.AppendLine("using MongoDB.Bson.Serialization;");
			CSharp.AppendLine("using Waher.Persistence.Filters;");
			CSharp.AppendLine("using Waher.Persistence.MongoDB;");
			CSharp.AppendLine("using Waher.Persistence.MongoDB.Serialization;");
			CSharp.AppendLine("using Waher.Script;");
			CSharp.AppendLine();
			CSharp.AppendLine("namespace " + Type.Namespace + ".Bson");
			CSharp.AppendLine("{");
			CSharp.AppendLine("\tpublic class BsonSerializer" + TypeName + " : IBsonSerializer");
			CSharp.AppendLine("\t{");
			CSharp.AppendLine("\t\tprivate MongoDBProvider provider;");

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
							CSharp.Append(DefaultValue.GetType().FullName);

						CSharp.Append(" default");
						CSharp.Append(Member.Name);
						CSharp.Append(" = ");

						if (DefaultValue == null)
							CSharp.Append("null");
						else if (DefaultValue is string)
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
						else if (DefaultValue is Enum)
						{
							Type DefaultValueType = DefaultValue.GetType();

							if (DefaultValueType.IsDefined(typeof(FlagsAttribute)))
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

				if (Type.GetTypeCode(MemberType) == TypeCode.Object && !MemberType.IsArray &&
					!ByReference && MemberType != typeof(TimeSpan) && MemberType != typeof(TimeSpan))
				{
					CSharp.Append("\t\tprivate static readonly ObjectSerializer serializer");
					CSharp.Append(Member.Name);
					CSharp.AppendLine(";");
				}
			}

			if (NrDefault > 0)
				CSharp.AppendLine();

			CSharp.AppendLine("\t\tpublic BsonSerializer" + TypeName + "(MongoDBProvider Provider)");
			CSharp.AppendLine("\t\t{");
			CSharp.AppendLine("\t\t\tthis.provider = Provider;");


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
					else if (Attr is ByReferenceAttribute)
						ByReference = true;
				}

				if (Ignore)
					continue;

				if (Type.GetTypeCode(MemberType) == TypeCode.Object && !MemberType.IsArray &&
					!ByReference && MemberType != typeof(TimeSpan) && MemberType != typeof(string))
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
			CSharp.AppendLine("\t\tpublic Type ValueType { get { return typeof(" + Type.FullName + "); } }");
			CSharp.AppendLine();
			CSharp.AppendLine("\t\tpublic object Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)");
			CSharp.AppendLine("\t\t{");
			CSharp.AppendLine("\t\t\tIBsonReader Reader = context.Reader;");
			CSharp.AppendLine("\t\t\tBsonType BsonType;");
			CSharp.AppendLine("\t\t\tstring FieldName;");
			CSharp.AppendLine("\t\t\t" + TypeName + " Result;");

			if (this.typeNameSerialization != TypeNameSerialization.None)
			{
				CSharp.AppendLine("\t\t\tBsonReaderBookmark Bookmark = Reader.GetBookmark();");
				CSharp.AppendLine();
				CSharp.AppendLine("\t\t\tReader.ReadStartDocument();");
				CSharp.AppendLine("\t\t\tif (!Reader.FindElement(\"" + Escape(this.typeFieldName) + "\"))");
				CSharp.AppendLine("\t\t\t\tthrow new Exception(\"Type name not available.\");");
				CSharp.AppendLine();
				CSharp.AppendLine("\t\t\tstring TypeName = Reader.ReadString();");

				if (this.typeNameSerialization == TypeNameSerialization.LocalName)
					CSharp.AppendLine("\t\t\tTypeName = \"" + Type.Namespace + ".\" + TypeName;");

				CSharp.AppendLine();
				CSharp.AppendLine("\t\t\tType DesiredType = Waher.Script.Types.GetType(TypeName);");
				CSharp.AppendLine("\t\t\tif (DesiredType == null)");
				CSharp.AppendLine("\t\t\t\tthrow new Exception(\"Class of type \" + TypeName + \" not found.\");");
				CSharp.AppendLine();
				CSharp.AppendLine("\t\t\tReader.ReturnToBookmark(Bookmark);");
				CSharp.AppendLine();
				CSharp.AppendLine("\t\t\tif (DesiredType != typeof(" + Type.FullName + "))");
				CSharp.AppendLine("\t\t\t{");
				CSharp.AppendLine("\t\t\t\tObjectSerializer Serializer2 = this.provider.GetObjectSerializer(DesiredType);");
				CSharp.AppendLine("\t\t\t\treturn Serializer2.Deserialize(context, args);");
				CSharp.AppendLine("\t\t\t}");
			}

			if (Type.IsAbstract)
				CSharp.AppendLine("\t\t\tthrow new Exception(\"Unable to create an instance of an abstract class.\");");
			else
			{
				CSharp.AppendLine();
				CSharp.AppendLine("\t\t\tReader.ReadStartDocument();");
				CSharp.AppendLine("\t\t\tResult = new " + Type.FullName + "();");
				CSharp.AppendLine();
				CSharp.AppendLine("\t\t\twhile (Reader.State == BsonReaderState.Type)");
				CSharp.AppendLine("\t\t\t{");
				CSharp.AppendLine("\t\t\t\tBsonType = Reader.ReadBsonType();");
				CSharp.AppendLine("\t\t\t\tif (BsonType == BsonType.EndOfDocument)");
				CSharp.AppendLine("\t\t\t\t\tbreak;");
				CSharp.AppendLine();
				CSharp.AppendLine("\t\t\t\tFieldName = Reader.ReadName();");
				CSharp.AppendLine();
				CSharp.AppendLine("\t\t\t\tswitch (FieldName)");
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
						CSharp.AppendLine("\t\t\t\t\t\tswitch (BsonType)");
						CSharp.AppendLine("\t\t\t\t\t\t{");
						CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.ObjectId:");
						CSharp.AppendLine("\t\t\t\t\t\t\t\tObjectId ObjectId = Reader.ReadObjectId();");

						if (MemberType == typeof(ObjectId))
							CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = ObjectId;");
						else if (MemberType == typeof(string))
							CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = ObjectId.ToString();");
						else if (MemberType == typeof(byte[]))
							CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = ObjectId.ToByteArray();");

						CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
						CSharp.AppendLine();
						CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
						CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Object ID parameter _id must be an Object ID value, but was a \" + BsonType.ToString() + \".\");");
						CSharp.AppendLine("\t\t\t\t\t\t}");
						CSharp.AppendLine("\t\t\t\t\t\tbreak;");
						CSharp.AppendLine();
					}
					else
					{
						CSharp.AppendLine("\t\t\t\t\tcase \"" + Member.Name + "\":");

						if (!string.IsNullOrEmpty(ShortName) && ShortName != Member.Name)
							CSharp.AppendLine("\t\t\t\t\tcase \"" + ShortName + "\":");

						if (MemberType.IsEnum)
						{
							CSharp.AppendLine("\t\t\t\t\t\tswitch (BsonType)");
							CSharp.AppendLine("\t\t\t\t\t\t{");
							CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Boolean:");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (" + MemberType.FullName + ")(Reader.ReadBoolean() ? 1 : 0);");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
							CSharp.AppendLine();
							CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Double:");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (" + MemberType.FullName + ")(int)Reader.ReadDouble();");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
							CSharp.AppendLine();
							CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int32:");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (" + MemberType.FullName + ")Reader.ReadInt32();");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
							CSharp.AppendLine();
							CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int64:");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (" + MemberType.FullName + ")Reader.ReadInt64();");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
							CSharp.AppendLine();
							CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.String:");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (" + MemberType.FullName + ")Enum.Parse(typeof(" + MemberType.FullName + "), Reader.ReadString());");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
							if (Nullable)
							{
								CSharp.AppendLine();
								CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Null:");
								CSharp.AppendLine("\t\t\t\t\t\t\t\tReader.ReadNull();");
								CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = null;");
								CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
							}
							CSharp.AppendLine();
							CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Unable to set " + Member.Name + ". Expected an enumeration value, but was a \" + BsonType.ToString() + \".\");");
							CSharp.AppendLine("\t\t\t\t\t\t}");
						}
						else
						{
							switch (Type.GetTypeCode(MemberType))
							{
								case TypeCode.Boolean:
									CSharp.AppendLine("\t\t\t\t\t\tswitch (BsonType)");
									CSharp.AppendLine("\t\t\t\t\t\t{");
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Boolean:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = Reader.ReadBoolean();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Double:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = Reader.ReadDouble() != 0;");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int32:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = Reader.ReadInt32() != 0;");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int64:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = Reader.ReadInt64() != 0;");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									if (Nullable)
									{
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Null:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tReader.ReadNull();");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = null;");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									}
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Unable to set " + Member.Name + ". Expected a boolean value, but was a \" + BsonType.ToString() + \".\");");
									CSharp.AppendLine("\t\t\t\t\t\t}");
									break;

								case TypeCode.Byte:
									CSharp.AppendLine("\t\t\t\t\t\tswitch (BsonType)");
									CSharp.AppendLine("\t\t\t\t\t\t{");
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Boolean:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = Reader.ReadBoolean() ? (byte)1 : (byte)0;");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Double:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (byte)Reader.ReadDouble();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int32:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (byte)Reader.ReadInt32();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int64:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (byte)Reader.ReadInt64();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.String:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = byte.Parse(Reader.ReadString());");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									if (Nullable)
									{
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Null:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tReader.ReadNull();");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = null;");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									}
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Unable to set " + Member.Name + ". Expected a byte value, but was a \" + BsonType.ToString() + \".\");");
									CSharp.AppendLine("\t\t\t\t\t\t}");
									break;

								case TypeCode.Char:
									CSharp.AppendLine("\t\t\t\t\t\tswitch (BsonType)");
									CSharp.AppendLine("\t\t\t\t\t\t{");
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.String:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tstring " + Member.Name + " = Reader.ReadString();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = string.IsNullOrEmpty(" + Member.Name + ") ? (char)0 : " + Member.Name + "[0];");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Symbol:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tstring " + Member.Name + " = Reader.ReadSymbol();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = string.IsNullOrEmpty(" + Member.Name + ") ? (char)0 : " + Member.Name + "[0];");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Double:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (char)Reader.ReadDouble();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int32:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (char)Reader.ReadInt32();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int64:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (char)Reader.ReadInt64();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									if (Nullable)
									{
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Null:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tReader.ReadNull();");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = null;");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									}
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Unable to set " + Member.Name + ". Expected a char value, but was a \" + BsonType.ToString() + \".\");");
									CSharp.AppendLine("\t\t\t\t\t\t}");
									break;

								case TypeCode.DateTime:
									CSharp.AppendLine("\t\t\t\t\t\tswitch (BsonType)");
									CSharp.AppendLine("\t\t\t\t\t\t{");
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.DateTime:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = " + typeof(ObjectSerializer).FullName + ".UnixEpoch.AddMilliseconds(Reader.ReadDateTime());");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.String:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = DateTime.Parse(Reader.ReadString());");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									if (Nullable)
									{
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Null:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tReader.ReadNull();");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = null;");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									}
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Unable to set " + Member.Name + ". Expected a DateTime value, but was a \" + BsonType.ToString() + \".\");");
									CSharp.AppendLine("\t\t\t\t\t\t}");
									break;

								case TypeCode.Decimal:
									CSharp.AppendLine("\t\t\t\t\t\tswitch (BsonType)");
									CSharp.AppendLine("\t\t\t\t\t\t{");
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Boolean:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = Reader.ReadBoolean() ? 1 : 0;");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Double:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (decimal)Reader.ReadDouble();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int32:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = Reader.ReadInt32();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int64:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = Reader.ReadInt64();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.String:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = decimal.Parse(Reader.ReadString());");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									if (Nullable)
									{
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Null:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tReader.ReadNull();");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = null;");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									}
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Unable to set " + Member.Name + ". Expected a decimal value, but was a \" + BsonType.ToString() + \".\");");
									CSharp.AppendLine("\t\t\t\t\t\t}");
									break;

								case TypeCode.Double:
									CSharp.AppendLine("\t\t\t\t\t\tswitch (BsonType)");
									CSharp.AppendLine("\t\t\t\t\t\t{");
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Boolean:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = Reader.ReadBoolean() ? 1 : 0;");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Double:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = Reader.ReadDouble();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int32:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = Reader.ReadInt32();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int64:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = Reader.ReadInt64();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.String:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = double.Parse(Reader.ReadString());");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									if (Nullable)
									{
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Null:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tReader.ReadNull();");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = null;");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									}
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Unable to set " + Member.Name + ". Expected a double-precision value, but was a \" + BsonType.ToString() + \".\");");
									CSharp.AppendLine("\t\t\t\t\t\t}");
									break;

								case TypeCode.Int16:
									CSharp.AppendLine("\t\t\t\t\t\tswitch (BsonType)");
									CSharp.AppendLine("\t\t\t\t\t\t{");
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Boolean:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = Reader.ReadBoolean() ? (short)1 : (short)0;");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Double:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (short)Reader.ReadDouble();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int32:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (short)Reader.ReadInt32();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int64:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (short)Reader.ReadInt64();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.String:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = short.Parse(Reader.ReadString());");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									if (Nullable)
									{
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Null:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tReader.ReadNull();");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = null;");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									}
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Unable to set " + Member.Name + ". Expected an Int16 value, but was a \" + BsonType.ToString() + \".\");");
									CSharp.AppendLine("\t\t\t\t\t\t}");
									break;

								case TypeCode.Int32:
									CSharp.AppendLine("\t\t\t\t\t\tswitch (BsonType)");
									CSharp.AppendLine("\t\t\t\t\t\t{");
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Boolean:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = Reader.ReadBoolean() ? 1 : 0;");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Double:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (int)Reader.ReadDouble();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int32:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = Reader.ReadInt32();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int64:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (int)Reader.ReadInt64();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.String:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = int.Parse(Reader.ReadString());");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									if (Nullable)
									{
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Null:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tReader.ReadNull();");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = null;");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									}
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Unable to set " + Member.Name + ". Expected an Int32 value, but was a \" + BsonType.ToString() + \".\");");
									CSharp.AppendLine("\t\t\t\t\t\t}");
									break;

								case TypeCode.Int64:
									CSharp.AppendLine("\t\t\t\t\t\tswitch (BsonType)");
									CSharp.AppendLine("\t\t\t\t\t\t{");
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Boolean:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = Reader.ReadBoolean() ? 1 : 0;");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Double:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (long)Reader.ReadDouble();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int32:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = Reader.ReadInt32();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int64:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = Reader.ReadInt64();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.String:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = long.Parse(Reader.ReadString());");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									if (Nullable)
									{
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Null:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tReader.ReadNull();");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = null;");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									}
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Unable to set " + Member.Name + ". Expected an Int64 value, but was a \" + BsonType.ToString() + \".\");");
									CSharp.AppendLine("\t\t\t\t\t\t}");
									break;

								case TypeCode.SByte:
									CSharp.AppendLine("\t\t\t\t\t\tswitch (BsonType)");
									CSharp.AppendLine("\t\t\t\t\t\t{");
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Boolean:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = Reader.ReadBoolean() ? (sbyte)1 : (sbyte)0;");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Double:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (sbyte)Reader.ReadDouble();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int32:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (sbyte)Reader.ReadInt32();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int64:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (sbyte)Reader.ReadInt64();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.String:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = sbyte.Parse(Reader.ReadString());");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									if (Nullable)
									{
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Null:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tReader.ReadNull();");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = null;");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									}
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Unable to set " + Member.Name + ". Expected a signed byte value, but was a \" + BsonType.ToString() + \".\");");
									CSharp.AppendLine("\t\t\t\t\t\t}");
									break;

								case TypeCode.Single:
									CSharp.AppendLine("\t\t\t\t\t\tswitch (BsonType)");
									CSharp.AppendLine("\t\t\t\t\t\t{");
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Boolean:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = Reader.ReadBoolean() ? 1 : 0;");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Double:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (float)Reader.ReadDouble();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int32:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = Reader.ReadInt32();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int64:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = Reader.ReadInt64();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.String:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = float.Parse(Reader.ReadString());");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									if (Nullable)
									{
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Null:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tReader.ReadNull();");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = null;");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									}
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Unable to set " + Member.Name + ". Expected a single-precision value, but was a \" + BsonType.ToString() + \".\");");
									CSharp.AppendLine("\t\t\t\t\t\t}");
									break;

								case TypeCode.String:
									CSharp.AppendLine("\t\t\t\t\t\tswitch (BsonType)");
									CSharp.AppendLine("\t\t\t\t\t\t{");
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.String:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = Reader.ReadString();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Symbol:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = Reader.ReadSymbol();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.RegularExpression:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = Reader.ReadRegularExpression().Pattern;");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.JavaScriptWithScope: ");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = Reader.ReadJavaScriptWithScope();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.JavaScript:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = Reader.ReadJavaScript();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Null:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tReader.ReadNull();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = null;");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Unable to set " + Member.Name + ". Expected a string value, but was a \" + BsonType.ToString() + \".\");");
									CSharp.AppendLine("\t\t\t\t\t\t}");
									break;

								case TypeCode.UInt16:
									CSharp.AppendLine("\t\t\t\t\t\tswitch (BsonType)");
									CSharp.AppendLine("\t\t\t\t\t\t{");
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Boolean:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = Reader.ReadBoolean() ? (ushort)1 : (ushort)0;");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Double:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (ushort)Reader.ReadDouble();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int32:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (ushort)Reader.ReadInt32();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int64:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (ushort)Reader.ReadInt64();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.String:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = ushort.Parse(Reader.ReadString());");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									if (Nullable)
									{
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Null:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tReader.ReadNull();");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = null;");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									}
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Unable to set " + Member.Name + ". Expected an unsigned Int16 value, but was a \" + BsonType.ToString() + \".\");");
									CSharp.AppendLine("\t\t\t\t\t\t}");
									break;

								case TypeCode.UInt32:
									CSharp.AppendLine("\t\t\t\t\t\tswitch (BsonType)");
									CSharp.AppendLine("\t\t\t\t\t\t{");
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Boolean:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = Reader.ReadBoolean() ? (uint)1 : (uint)0;");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Double:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (uint)Reader.ReadDouble();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int32:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (uint)Reader.ReadInt32();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int64:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (uint)Reader.ReadInt64();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.String:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = uint.Parse(Reader.ReadString());");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									if (Nullable)
									{
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Null:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tReader.ReadNull();");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = null;");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									}
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Unable to set " + Member.Name + ". Expected an unsigned Int32 value, but was a \" + BsonType.ToString() + \".\");");
									CSharp.AppendLine("\t\t\t\t\t\t}");
									break;

								case TypeCode.UInt64:
									CSharp.AppendLine("\t\t\t\t\t\tswitch (BsonType)");
									CSharp.AppendLine("\t\t\t\t\t\t{");
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Boolean:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = Reader.ReadBoolean() ? (ulong)1 : (ulong)0;");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Double:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (ulong)Reader.ReadDouble();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int32:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (ulong)Reader.ReadInt32();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int64:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (ulong)Reader.ReadInt64();");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.String:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = ulong.Parse(Reader.ReadString());");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									if (Nullable)
									{
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Null:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tReader.ReadNull();");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = null;");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
									}
									CSharp.AppendLine();
									CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
									CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Unable to set " + Member.Name + ". Expected an unsigned Int64 value, but was a \" + BsonType.ToString() + \".\");");
									CSharp.AppendLine("\t\t\t\t\t\t}");
									break;

								case TypeCode.DBNull:
								case TypeCode.Empty:
								default:
									throw new Exception("Invalid member type: " + Member.MemberType.ToString());

								case TypeCode.Object:
									if (MemberType.IsArray)
									{
										MemberType = MemberType.GetElementType();

										CSharp.AppendLine("\t\t\t\t\t\tswitch (BsonType)");
										CSharp.AppendLine("\t\t\t\t\t\t{");
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Array:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tList<" + MemberType.FullName + "> Elements" + Member.Name + " = new List<" + MemberType.FullName + ">();");

										if (MemberType.IsClass && MemberType != typeof(string))
											CSharp.AppendLine("\t\t\t\t\t\t\t\tIBsonSerializer S = this.provider.GetObjectSerializer(typeof(" + MemberType.FullName + "));");
										else
											CSharp.AppendLine("\t\t\t\t\t\t\t\tIBsonSerializer S = BsonSerializer.LookupSerializer(typeof(" + MemberType.FullName + "));");

										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\t\tReader.ReadStartArray();");
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\t\twhile (Reader.State != BsonReaderState.EndOfArray)");
										CSharp.AppendLine("\t\t\t\t\t\t\t\t{");
										CSharp.AppendLine("\t\t\t\t\t\t\t\t\tif (Reader.State == BsonReaderState.Type)");
										CSharp.AppendLine("\t\t\t\t\t\t\t\t\t{");
										CSharp.AppendLine("\t\t\t\t\t\t\t\t\t\tBsonType TempType = Reader.ReadBsonType();");
										CSharp.AppendLine("\t\t\t\t\t\t\t\t\t\tif (TempType == BsonType.EndOfDocument)");
										CSharp.AppendLine("\t\t\t\t\t\t\t\t\t\t\tbreak;");
										CSharp.AppendLine("\t\t\t\t\t\t\t\t\t}");
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\t\t\tElements" + Member.Name + ".Add((" + MemberType.FullName + ")S.Deserialize(context, args));");
										CSharp.AppendLine("\t\t\t\t\t\t\t\t\tif (Reader.ReadBsonType() == BsonType.EndOfDocument)");
										CSharp.AppendLine("\t\t\t\t\t\t\t\t\t\tbreak;");
										CSharp.AppendLine("\t\t\t\t\t\t\t\t}");
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\t\tReader.ReadEndArray();");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = Elements" + Member.Name + ".ToArray();");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Null:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tReader.ReadNull();");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = null;");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
										CSharp.AppendLine("\t\t\t\t\t\t\tthrow new Exception(\"Array expected for " + Member.Name + ".\");");
										CSharp.AppendLine("\t\t\t\t\t\t}");
									}
									else if (ByReference)
									{
										CSharp.AppendLine("\t\t\t\t\t\tswitch (BsonType)");
										CSharp.AppendLine("\t\t\t\t\t\t{");
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.ObjectId:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tObjectId " + Member.Name + "ObjectId = Reader.ReadObjectId();");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tTask<" + MemberType.FullName + "> " + Member.Name + "Task = this.provider.LoadObject<" + MemberType.FullName + ">(" + Member.Name + "ObjectId);");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tif (!" + Member.Name + "Task.Wait(10000))");
										CSharp.AppendLine("\t\t\t\t\t\t\t\t\tthrow new Exception(\"Unable to load referenced object. Database timed out.\");");
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = " + Member.Name + "Task.Result;");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Null:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = null;");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Object ID expected for " + Member.Name + ".\");");
										CSharp.AppendLine("\t\t\t\t\t\t}");
									}
									else if (MemberType == typeof(TimeSpan))
									{
										CSharp.AppendLine("\t\t\t\t\t\tswitch (BsonType)");
										CSharp.AppendLine("\t\t\t\t\t\t{");
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.String:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = TimeSpan.Parse(Reader.ReadString());");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										if (Nullable)
										{
											CSharp.AppendLine();
											CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Null:");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tReader.ReadNull();");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = null;");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										}
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Unable to set " + Member.Name + ". Expected a string value, but was a \" + BsonType.ToString() + \".\");");
										CSharp.AppendLine("\t\t\t\t\t\t}");
									}
									else
									{
										CSharp.AppendLine("\t\t\t\t\t\tswitch (BsonType)");
										CSharp.AppendLine("\t\t\t\t\t\t{");
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Document:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = this.serializer" + Member.Name + ".Deserialize(context, args);");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Null:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = null;");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Document expected for " + Member.Name + ".\");");
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
					CSharp.AppendLine("\t\t\t\t\t\tswitch (BsonType)");
					CSharp.AppendLine("\t\t\t\t\t\t{");
					CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.String:");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tReader.ReadString();");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
					CSharp.AppendLine();
					CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Type parameter " + this.typeFieldName + " must be a string value, but was a \" + BsonType.ToString() + \".\");");
					CSharp.AppendLine("\t\t\t\t\t\t}");
					CSharp.AppendLine("\t\t\t\t\t\tbreak;");
					CSharp.AppendLine();
				}

				if (!HasObjectId)
				{
					CSharp.AppendLine("\t\t\t\t\tcase \"_id\":");
					CSharp.AppendLine("\t\t\t\t\t\tswitch (BsonType)");
					CSharp.AppendLine("\t\t\t\t\t\t{");
					CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.ObjectId:");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tReader.ReadObjectId();");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
					CSharp.AppendLine();
					CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Object ID parameter _id must be an Object ID value, but was a \" + BsonType.ToString() + \".\");");
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
			CSharp.AppendLine("\t\tpublic void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)");
			CSharp.AppendLine("\t\t{");
			CSharp.AppendLine("\t\t\t" + TypeName + " Value = (" + TypeName + ")value;");
			CSharp.AppendLine("\t\t\tIBsonWriter Writer = context.Writer;");
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
					CSharp.AppendLine();

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

					if (MemberType.IsEnum)
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
								CSharp.Append("if ( Value.");
								CSharp.Append(Member.Name);
								CSharp.AppendLine(" == null)");
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

							case TypeCode.DBNull:
							case TypeCode.Empty:
							default:
								throw new Exception("Invalid member type: " + Member.MemberType.ToString());

							case TypeCode.Object:
								if (MemberType.IsArray)
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
									CSharp.AppendLine("\tif (Item == null)");
									CSharp.Append(Indent2);
									CSharp.AppendLine("\t\tWriter.WriteNull();");
									CSharp.Append(Indent2);
									CSharp.AppendLine("\telse");
									CSharp.Append(Indent2);
									CSharp.AppendLine("\t{");

									CSharp.Append(Indent2);
									CSharp.AppendLine("\t\tType T = Item.GetType();");
									CSharp.AppendLine("\t\tIBsonSerializer S;");
									CSharp.AppendLine();
									CSharp.Append(Indent2);
									CSharp.AppendLine("\t\tif (T.IsClass && T != typeof(string))");
									CSharp.Append(Indent2);
									CSharp.AppendLine("\t\t\tS = this.provider.GetObjectSerializer(T);");
									CSharp.Append(Indent2);
									CSharp.AppendLine("\t\telse");
									CSharp.Append(Indent2);
									CSharp.AppendLine("\t\t\tS = BsonSerializer.LookupSerializer(T);");
									CSharp.AppendLine();
									CSharp.Append(Indent2);
									CSharp.AppendLine("\t\tS.Serialize(context, args, Item);");
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
									CSharp.AppendLine("if (Value." + Member.Name + " == null)");
									CSharp.Append(Indent2);
									CSharp.AppendLine("\tWriter.WriteNull();");
									CSharp.Append(Indent2);
									CSharp.AppendLine("else");
									CSharp.Append(Indent2);
									CSharp.AppendLine("{");
									CSharp.Append(Indent2);
									CSharp.AppendLine("\tObjectSerializer Serializer" + Member.Name + " = this.provider.GetObjectSerializer(typeof(" + MemberType.FullName + "));");
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
								else
								{
									CSharp.Append(Indent2);
									CSharp.Append("this.serializer");
									CSharp.Append(Member.Name);
									CSharp.Append(".Serialize(context, args, Value.");
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
			CSharp.AppendLine("\t}");
			CSharp.AppendLine("}");

			CSharpCodeProvider CodeProvider = new CSharpCodeProvider();
			Dictionary<string, bool> Dependencies = new Dictionary<string, bool>()
			{
				{ typeof(IEnumerable).Assembly.Location.Replace("mscorlib.dll", "System.Runtime.dll"), true },
				{ typeof(Database).Assembly.Location, true },
				{ typeof(ObjectSerializer).Assembly.Location, true }
			};

			Type Loop = Type;

			while (Loop != null)
			{
				Dependencies[Loop.Assembly.Location] = true;

				foreach (Type Interface in Loop.GetInterfaces())
					Dependencies[Interface.Assembly.Location] = true;

				Loop = Loop.BaseType;
				if (Loop == typeof(object))
					break;
			}

			string[] Assemblies = new string[Dependencies.Count];
			CompilerParameters Options;

			Dependencies.Keys.CopyTo(Assemblies, 0);
			Options = new CompilerParameters(Assemblies);

			CompilerResults CompilerResults = CodeProvider.CompileAssemblyFromSource(Options, CSharp.ToString());

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
			Type T = A.GetType(Type.Namespace + ".Bson.BsonSerializer" + TypeName);
			ConstructorInfo CI = T.GetConstructor(new Type[] { typeof(MongoDBProvider) });
			this.customSerializer = (IBsonSerializer)CI.Invoke(new object[] { this.provider });

			BsonSerializer.RegisterSerializer(Type, this);

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
			}
		}

		/// <summary>
		/// Escapes a string to be enclosed between double quotes.
		/// </summary>
		/// <param name="s"></param>
		/// <returns>String with special characters escaped.</returns>
		public static string Escape(string s)
		{
			if (s == null || s.IndexOfAny(specialCharacters) < 0)
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
		/// Deserializes a value.
		/// </summary>
		/// <param name="context">The deserialization context.</param>
		/// <param name="args">The deserialization args.</param>
		/// <returns>A deserialized value.</returns>
		public object Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
		{
			return this.customSerializer.Deserialize(context, args);
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="context">The serialization context.</param>
		/// <param name="args">The serialization args.</param>
		/// <param name="value">The value.</param>
		public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
		{
			this.customSerializer.Serialize(context, args, value);
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
				if (Value is string)
					Value = new ObjectId((string)Value);

				Result = "_id";
			}
			else
				Result = Name;

			if (i >= 0)
			{
				if (this.memberTypes.TryGetValue(Name, out Type T))
				{
					ObjectSerializer S2;

					if (T.IsArray)
						S2 = this.provider.GetObjectSerializer(T.GetElementType());
					else
						S2 = this.provider.GetObjectSerializer(T);

					Result += "." + S2.ToShortName(FieldName.Substring(i + 1), ref Value);
				}
				else
					Result += FieldName.Substring(i);
			}

			return Result;
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
			if (this.objectIdFieldInfo != null)
				return this.objectIdFieldInfo.GetValue(Value) != null;
			else if (this.objectIdPropertyInfo != null)
				return this.objectIdPropertyInfo.GetValue(Value) != null;
			else
				return false;
		}

		/// <summary>
		/// Gets the Object ID for a given object.
		/// </summary>
		/// <param name="Value">Object reference.</param>
		/// <param name="InsertIfNotFound">Insert object into database with new Object ID, if no Object ID is set.</param>
		/// <returns>Object ID for <paramref name="Value"/>.</returns>
		public async Task<ObjectId> GetObjectId(object Value, bool InsertIfNotFound)
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
				ObjectSerializer Serializer = this.provider.GetObjectSerializer(ValueType);
				string CollectionName = Serializer.CollectionName;
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
			else if (Obj is ObjectId)
				return (ObjectId)Obj;
			else if (Obj is string)
				return new ObjectId((string)Obj);
			else if (Obj is byte[])
				return new ObjectId((byte[])Obj);
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
			if (!this.defaultValues.TryGetValue(FieldName, out object Default))
				return false;

			if ((Value == null) ^ (Default == null))
				return false;

			if (Value == null)
				return true;

			return Default.Equals(Value);
		}

		/// <summary>
		/// Tries to get the serialization info for a member.
		/// </summary>
		/// <param name="memberName">Name of the member.</param>
		/// <param name="serializationInfo">The serialization information.</param>
		/// <returns>true if the serialization info exists; otherwise false.</returns>
		public bool TryGetMemberSerializationInfo(string memberName, out BsonSerializationInfo serializationInfo)
		{
			throw new NotImplementedException();
		}
	}
}
