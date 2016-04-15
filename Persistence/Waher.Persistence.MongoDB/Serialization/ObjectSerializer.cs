using System;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CSharp;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using Waher.Persistence.Attributes;

namespace Waher.Persistence.MongoDB.Serialization
{
	/// <summary>
	/// Serializes a type to BSON, taking into account attributes defined in <see cref="Waher.Persistence.Attributes"/>.
	/// </summary>
	public class ObjectSerializer : IBsonDocumentSerializer
	{
		private Type type;
		private string collectionName;
		private IBsonSerializer customSerializer;

		/// <summary>
		/// Serializes a type to BSON, taking into account attributes defined in <see cref="Waher.Persistence.Attributes"/>.
		/// </summary>
		/// <param name="Type">Type to serialize.</param>
		public ObjectSerializer(Type Type)
		{
			this.type = Type;

			CollectionNameAttribute CollectionNameAttribute = Type.GetCustomAttribute<CollectionNameAttribute>(true);
			if (CollectionNameAttribute == null)
				this.collectionName = null;
			else
				this.collectionName = CollectionNameAttribute.Name;

			StringBuilder CSharp = new StringBuilder();
			Type MemberType;
			FieldInfo FI;
			PropertyInfo PI;
			object DefaultValue;
			string ShortName;
			string Indent;
			int NrDefault = 0;
			bool HasDefaultValue;
			bool Ignore;

			CSharp.AppendLine("using System;");
			CSharp.AppendLine("using System.Text;");
			CSharp.AppendLine("using MongoDB.Bson;");
			CSharp.AppendLine("using MongoDB.Bson.IO;");
			CSharp.AppendLine("using MongoDB.Bson.Serialization;");
			CSharp.AppendLine();
			CSharp.AppendLine("namespace " + Type.Namespace + ".Bson");
			CSharp.AppendLine("{");
			CSharp.AppendLine("\tpublic class BsonSerializer" + Type.Name + " : IBsonSerializer");
			CSharp.AppendLine("\t{");

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

				foreach (Attribute Attr in Member.GetCustomAttributes(true))
				{
					if (Attr is IgnoreMemberAttribute)
					{
						Ignore = true;
						break;
					}
					else if (Attr is DefaultValueAttribute)
					{
						DefaultValue = ((DefaultValueAttribute)Attr).Value;
						NrDefault++;

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
						else
							CSharp.Append(DefaultValue.ToString());

						CSharp.AppendLine(";");
					}
				}

				if (Ignore)
					continue;
			}

			if (NrDefault > 0)
				CSharp.AppendLine();

			CSharp.AppendLine("\t\tpublic BsonSerializer" + Type.Name + "()");
			CSharp.AppendLine("\t\t{");
			CSharp.AppendLine("\t\t}");
			CSharp.AppendLine();
			CSharp.AppendLine("\t\tpublic Type ValueType { get { return typeof(" + Type.FullName + "); } }");
			CSharp.AppendLine();
			CSharp.AppendLine("\t\tpublic object Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)");
			CSharp.AppendLine("\t\t{");
			CSharp.AppendLine("\t\t\tIBsonReader Reader = context.Reader;");
			CSharp.AppendLine("\t\t\tBsonType BsonType;");
			CSharp.AppendLine("\t\t\t" + Type.Name + " Result = new " + Type.Name + "();");
			CSharp.AppendLine();
			CSharp.AppendLine("\t\t\tReader.ReadStartDocument();");
			CSharp.AppendLine("\t\t\twhile ((BsonType = Reader.GetCurrentBsonType()) != BsonType.EndOfDocument)");
			CSharp.AppendLine("\t\t\t{");
			CSharp.AppendLine("\t\t\t\tswitch (Reader.ReadName())");
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

				foreach (Attribute Attr in Member.GetCustomAttributes(true))
				{
					if (Attr is IgnoreMemberAttribute)
					{
						Ignore = true;
						break;
					}
					else if (Attr is ShortNameAttribute)
						ShortName = ((ShortNameAttribute)Attr).Name;
				}

				if (Ignore)
					continue;

				CSharp.AppendLine("\t\t\t\t\tcase \"" + Member.Name + "\":");

				if (!string.IsNullOrEmpty(ShortName))
					CSharp.AppendLine("\t\t\t\t\tcase \"" + ShortName + "\":");

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
						CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (byte)Reader.ReadDouble() != 0;");
						CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
						CSharp.AppendLine();
						CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int32:");
						CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (byte)Reader.ReadInt32() != 0;");
						CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
						CSharp.AppendLine();
						CSharp.AppendLine("\t\t\t\t\t\t\tcase BsonType.Int64:");
						CSharp.AppendLine("\t\t\t\t\t\t\t\tResult." + Member.Name + " = (byte)Reader.ReadInt64() != 0;");
						CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
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
						CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
						CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new Exception(\"Unable to set " + Member.Name + ". Expected an unsigned Int64 value, but was a \" + BsonType.ToString() + \".\");");
						CSharp.AppendLine("\t\t\t\t\t\t}");
						break;

					case TypeCode.DBNull:
					case TypeCode.Empty:
					default:
						throw new Exception("Invalid member type: " + Member.MemberType.ToString());

					case TypeCode.Object:
						throw new NotImplementedException("Sub-objects not implemented yet.");
						// TODO: Implement sub-objects.
				}

				CSharp.AppendLine("\t\t\t\t\t\tbreak;");
				CSharp.AppendLine();
			}

			CSharp.AppendLine("\t\t\t\t}");
			CSharp.AppendLine();
			CSharp.AppendLine("\t\t\t\treturn Result;");
			CSharp.AppendLine("\t\t\t}");
			CSharp.AppendLine();
			CSharp.AppendLine("\t\t\tReader.ReadEndDocument();");
			CSharp.AppendLine();
			CSharp.AppendLine("\t\t\treturn Result;");

			CSharp.AppendLine("\t\t}");
			CSharp.AppendLine();
			CSharp.AppendLine("\t\tpublic void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)");
			CSharp.AppendLine("\t\t{");
			CSharp.AppendLine("\t\t\t" + Type.Name + " Value = (" + Type.Name + ")value;");
			CSharp.AppendLine("\t\t\tIBsonWriter Writer = context.Writer;");
			CSharp.AppendLine("\t\t\tWriter.WriteStartDocument();");

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
				CSharp.Append("Writer.WriteName(\"");

				if (!string.IsNullOrEmpty(ShortName))
					CSharp.Append(Escape(ShortName));
				else
					CSharp.Append(Escape(Member.Name));

				CSharp.AppendLine("\");");

				switch (Type.GetTypeCode(MemberType))
				{
					case TypeCode.Boolean:
						CSharp.Append(Indent);
						CSharp.Append("Writer.WriteBoolean(Value.");
						CSharp.Append(Member.Name);
						CSharp.AppendLine(");");
						break;

					case TypeCode.Byte:
						CSharp.Append(Indent);
						CSharp.Append("Writer.WriteInt32(Value.");
						CSharp.Append(Member.Name);
						CSharp.AppendLine(");");
						break;

					case TypeCode.Char:
						CSharp.Append(Indent);
						CSharp.Append("Writer.WriteInt32((int)Value.");
						CSharp.Append(Member.Name);
						CSharp.AppendLine(");");
						break;

					case TypeCode.DateTime:
						CSharp.Append(Indent);
						CSharp.Append("Writer.WriteDateTime((long)((Value.");
						CSharp.Append(Member.Name);
						CSharp.Append(" - ");
						CSharp.Append(typeof(ObjectSerializer).FullName);
						CSharp.AppendLine(".UnixEpoch).TotalMilliseconds + 0.5));");
						break;

					case TypeCode.Decimal:
						CSharp.Append(Indent);
						CSharp.Append("Writer.WriteDouble((double)Value.");
						CSharp.Append(Member.Name);
						CSharp.AppendLine(");");
						break;

					case TypeCode.Double:
					case TypeCode.Single:
						CSharp.Append(Indent);
						CSharp.Append("Writer.WriteDouble(Value.");
						CSharp.Append(Member.Name);
						CSharp.AppendLine(");");
						break;

					case TypeCode.Int32:
					case TypeCode.Int16:
					case TypeCode.UInt16:
					case TypeCode.SByte:
						CSharp.Append(Indent);
						CSharp.Append("Writer.WriteInt(Value.");
						CSharp.Append(Member.Name);
						CSharp.AppendLine(");");
						break;

					case TypeCode.Int64:
					case TypeCode.UInt32:
						CSharp.Append(Indent);
						CSharp.Append("Writer.WriteInt64(Value.");
						CSharp.Append(Member.Name);
						CSharp.AppendLine(");");
						break;

					case TypeCode.String:
						CSharp.Append(Indent);
						CSharp.Append("Writer.WriteString(Value.");
						CSharp.Append(Member.Name);
						CSharp.AppendLine(");");
						break;

					case TypeCode.UInt64:
						CSharp.Append(Indent);
						CSharp.Append("Writer.WriteInt64((long)Value.");
						CSharp.Append(Member.Name);
						CSharp.AppendLine(");");
						break;

					case TypeCode.DBNull:
					case TypeCode.Empty:
					default:
						throw new Exception("Invalid member type: " + Member.MemberType.ToString());

					case TypeCode.Object:
						throw new NotImplementedException("Sub-objects not implemented yet.");
						// TODO: Implement sub-objects.
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
			CompilerResults CompilerResults = CodeProvider.CompileAssemblyFromSource(
				new CompilerParameters(new string[]
				{
					Type.Assembly.Location,
					typeof(ObjectSerializer).Assembly.Location,
					typeof(BsonType).Assembly.Location
				}), CSharp.ToString());

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
			Type T = A.GetType(Type.Namespace + ".Bson.BsonSerializer" + Type.Name);
			ConstructorInfo CI = T.GetConstructor(new Type[0]);
			this.customSerializer = (IBsonSerializer)CI.Invoke(new object[0]);

			BsonSerializer.RegisterSerializer(Type, this);
		}

		/// <summary>
		/// Escapes a string to be enclosed between double quotes.
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
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
