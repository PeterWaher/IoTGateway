using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
#if NETSTANDARD2_0
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
#endif
using Waher.Events;
using Waher.Persistence.Serialization.Model;
using Waher.Persistence.Attributes;
using Waher.Runtime.Inventory;
using Waher.Runtime.Threading;
using Waher.Script.Abstraction.Elements;

namespace Waher.Persistence.Serialization
{
	/// <summary>
	/// Serializes a class, taking into account attributes defined in <see cref="Attributes"/>.
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
		/// Variable length INT16
		/// </summary>
		public const uint TYPE_VARINT16 = 21;

		/// <summary>
		/// Variable length INT32
		/// </summary>
		public const uint TYPE_VARINT32 = 22;

		/// <summary>
		/// Variable length INT64
		/// </summary>
		public const uint TYPE_VARINT64 = 23;

		/// <summary>
		/// Variable length UINT16
		/// </summary>
		public const uint TYPE_VARUINT16 = 24;

		/// <summary>
		/// Variable length UINT32
		/// </summary>
		public const uint TYPE_VARUINT32 = 25;

		/// <summary>
		/// Variable length UINT64
		/// </summary>
		public const uint TYPE_VARUINT64 = 26;

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

		private static readonly Type[] obsoleteMethodTypes = new Type[] { typeof(Dictionary<string, object>) };

		/// <summary>
		/// Serialization context.
		/// </summary>
		private ISerializerContext context;

		private Type type;
		private System.Reflection.TypeInfo typeInfo;
		private string collectionName;
		private string[][] indices;
		private TypeNameSerialization typeNameSerialization;
#if NETSTANDARD2_0
		private FieldInfo objectIdFieldInfo = null;
		private PropertyInfo objectIdPropertyInfo = null;
		private MemberInfo objectIdMemberInfo = null;
		private Type objectIdMemberType = null;
		private readonly Dictionary<string, object> defaultValues = new Dictionary<string, object>();
		private readonly Dictionary<string, Type> memberTypes = new Dictionary<string, Type>();
		private readonly Dictionary<string, MemberInfo> members = new Dictionary<string, MemberInfo>();
		private IObjectSerializer customSerializer = null;
		private bool compiled;
#endif
		private Member objectIdMember = null;
		private readonly Dictionary<string, Member> membersByName = new Dictionary<string, Member>();
		private readonly Dictionary<ulong, Member> membersByFieldCode = new Dictionary<ulong, Member>();
		private readonly LinkedList<Member> membersOrdered = new LinkedList<Member>();
		private PropertyInfo archiveProperty = null;
		private FieldInfo archiveField = null;
		private MethodInfo obsoleteMethod = null;
		private object tag = null;
		private int archiveDays = 0;
		private bool archive = false;
		private bool archiveDynamic = false;
		private bool isNullable;
		private bool normalized;
		private bool hasByRef = false;
		private bool backupCollection = true;
		private bool prepared = false;
		private string noBackupReason = null;

		internal ObjectSerializer(ISerializerContext Context, Type Type)    // Note order.
		{
			this.type = Type;
			this.typeInfo = Type.GetTypeInfo();
			this.context = Context;
			this.normalized = Context.NormalizedNames;
#if NETSTANDARD2_0
			this.compiled = false;
#endif
			this.isNullable = true;
			this.collectionName = null;
			this.typeNameSerialization = TypeNameSerialization.FullName;
			this.indices = new string[0][];
		}

		internal ObjectSerializer()
		{
		}

		/// <summary>
		/// Serialization context.
		/// </summary>
		public ISerializerContext Context => this.context;

#if NETSTANDARD2_0
		/// <summary>
		/// Initializes a serializer that serializes a class, taking into account attributes defined in <see cref="Waher.Persistence.Attributes"/>.
		/// </summary>
		/// <param name="Type">Type to serialize.</param>
		/// <param name="Context">Serialization context.</param>
		/// <param name="Compiled">If object serializers should be compiled or not.</param>
		public static async Task<ObjectSerializer> Create(Type Type, ISerializerContext Context, bool Compiled)
		{
			ObjectSerializer Result = new ObjectSerializer();
			await Result.Prepare(Type, Context, Compiled);
			return Result;
		}
#else
		/// <summary>
		/// Initializes a serializer that serializes a class, taking into account attributes defined in <see cref="Waher.Persistence.Attributes"/>.
		/// </summary>
		/// <param name="Type">Type to serialize.</param>
		/// <param name="Context">Serialization context.</param>
		public static async Task<ObjectSerializer> Create(Type Type, ISerializerContext Context)
		{
			ObjectSerializer Result = new ObjectSerializer();
			await Result.Prepare(Type, Context);
			return Result;
		}
#endif

#if NETSTANDARD2_0
		/// <summary>
		/// Prepares a serializer that serializes a class, taking into account attributes defined in <see cref="Waher.Persistence.Attributes"/>.
		/// </summary>
		/// <param name="Type">Type to serialize.</param>
		/// <param name="Context">Serialization context.</param>
		/// <param name="Compiled">If object serializers should be compiled or not.</param>
		internal async Task Prepare(Type Type, ISerializerContext Context, bool Compiled)
#else
		/// <summary>
		/// Prepares a serializer that serializes a class, taking into account attributes defined in <see cref="Waher.Persistence.Attributes"/>.
		/// </summary>
		/// <param name="Type">Type to serialize.</param>
		/// <param name="Context">Serialization context.</param>
		internal async Task Prepare(Type Type, ISerializerContext Context)
#endif
		{
			string TypeName = Type.Name.Replace("`", "_GT_");

			this.type = Type;
			this.typeInfo = Type.GetTypeInfo();
			this.context = Context;
			this.normalized = Context.NormalizedNames;

#if NETSTANDARD2_0
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

			NoBackupAttribute NoBackupAttribute = this.typeInfo.GetCustomAttribute<NoBackupAttribute>(true);
			if (NoBackupAttribute is null)
			{
				this.backupCollection = true;
				this.noBackupReason = null;
			}
			else
			{
				this.backupCollection = false;
				this.noBackupReason = NoBackupAttribute.Reason;
			}

			TypeNameAttribute TypeNameAttribute = this.typeInfo.GetCustomAttribute<TypeNameAttribute>(true);
			if (TypeNameAttribute is null)
				this.typeNameSerialization = TypeNameSerialization.FullName;
			else
				this.typeNameSerialization = TypeNameAttribute.TypeNameSerialization;

			ObsoleteMethodAttribute ObsoleteMethodAttribute = this.typeInfo.GetCustomAttribute<ObsoleteMethodAttribute>(true);
			if (!(ObsoleteMethodAttribute is null))
			{
				this.obsoleteMethod = this.type.GetRuntimeMethod(ObsoleteMethodAttribute.MethodName, obsoleteMethodTypes);
				if (this.obsoleteMethod is null)
				{
					StringBuilder sb = new StringBuilder();

					sb.Append("Obsolete method ");
					sb.Append(ObsoleteMethodAttribute.MethodName);
					sb.Append(" does not exist on ");
					AppendType(this.type, sb);

					throw new SerializationException(sb.ToString(), this.type);
				}

				ParameterInfo[] Parameters = this.obsoleteMethod.GetParameters();
				if (Parameters.Length != 1 || Parameters[0].ParameterType != typeof(Dictionary<string, object>))
				{
					StringBuilder sb = new StringBuilder();

					sb.Append("Obsolete method ");
					sb.Append(ObsoleteMethodAttribute.MethodName);
					sb.Append(" on ");
					AppendType(this.type, sb);
					sb.Append(" has invalid arguments.");

					throw new SerializationException(sb.ToString(), this.type);
				}
			}

			ArchivingTimeAttribute ArchivingTimeAttribute = this.typeInfo.GetCustomAttribute<ArchivingTimeAttribute>(true);
			if (ArchivingTimeAttribute is null)
				this.archive = false;
			else
			{
				this.archive = true;
				if (!string.IsNullOrEmpty(ArchivingTimeAttribute.PropertyName))
				{
					this.archiveProperty = this.type.GetRuntimeProperty(ArchivingTimeAttribute.PropertyName);

					if (this.archiveProperty is null)
					{
						this.archiveField = this.type.GetRuntimeField(ArchivingTimeAttribute.PropertyName);
						this.archiveProperty = null;

						if (this.archiveField is null)
							throw new SerializationException("Archiving time property or field not found: " + ArchivingTimeAttribute.PropertyName, this.type);
						else if (this.archiveField.FieldType != typeof(int))
							throw new SerializationException("Invalid field type for the archiving time: " + this.archiveField.Name, this.type);
					}
					else if (this.archiveProperty.PropertyType != typeof(int))
						throw new SerializationException("Invalid property type for the archiving time: " + this.archiveProperty.Name, this.type);
					else
						this.archiveField = null;

					this.archiveDynamic = !(this.archiveProperty is null && this.archiveField is null);
				}
				else
					this.archiveDays = ArchivingTimeAttribute.Days;
			}

			if (this.typeInfo.IsAbstract && this.typeNameSerialization == TypeNameSerialization.None)
				throw new SerializationException("Serializers for abstract classes require type names to be serialized.", this.type);

			List<string[]> Indices = new List<string[]>();
			Dictionary<string, bool> IndexFields = new Dictionary<string, bool>();

			foreach (IndexAttribute IndexAttribute in this.typeInfo.GetCustomAttributes<IndexAttribute>(true))
			{
				Indices.Add(IndexAttribute.FieldNames);

				foreach (string FieldName in IndexAttribute.FieldNames)
					IndexFields[FieldName] = true;
			}

			this.indices = Indices.ToArray();

#if NETSTANDARD2_0
			if (this.compiled)
			{
				StringBuilder CSharp = new StringBuilder();
				StringBuilder sb = new StringBuilder();
				IEnumerable<MemberInfo> Members = GetMembers(this.typeInfo);
				Dictionary<string, string> ShortNames = new Dictionary<string, string>();
				Dictionary<Type, bool> MemberTypes = new Dictionary<Type, bool>();
				Type MemberType;
				System.Reflection.TypeInfo MemberTypeInfo;
				FieldInfo FI;
				PropertyInfo PI;
				MethodInfo MI;
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
				CSharp.AppendLine("using Waher.Persistence;");
				CSharp.AppendLine("using Waher.Persistence.Filters;");
				CSharp.AppendLine("using Waher.Persistence.Serialization;");
				CSharp.AppendLine("using Waher.Runtime.Inventory;");
				CSharp.AppendLine();
				CSharp.Append("namespace ");
				CSharp.Append(Type.Namespace);
				CSharp.AppendLine(".Binary");
				CSharp.AppendLine("{");
				CSharp.Append("\tpublic class Serializer");
				CSharp.Append(TypeName);
				CSharp.Append(this.context.Id);
				CSharp.AppendLine(" : GeneratedObjectSerializerBase");
				CSharp.AppendLine("\t{");
				CSharp.AppendLine("\t\tprivate ISerializerContext context;");

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
					ShortName = null;
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

					MemberTypes[MemberType] = true;

					foreach (object Attr in Member.GetCustomAttributes(true))
					{
						if (Attr is IgnoreMemberAttribute)
						{
							Ignore = true;
							break;
						}
						else if (Attr is DefaultValueAttribute DefaultValueAttribute)
						{
							if (IndexFields.ContainsKey(Member.Name))
							{
								sb.Clear();

								sb.Append("Default value for ");
								AppendType(Type, sb);
								sb.Append('.');
								sb.Append(Member.Name);
								sb.Append(" ignored, as field is used to index objects.");

								Log.Notice(sb.ToString());
							}
							else
							{
								DefaultValue = DefaultValueAttribute.Value;
								NrDefault++;

								this.defaultValues[Member.Name] = DefaultValue;

								CSharp.Append("\t\tprivate static readonly ");

								if (DefaultValue is null)
									CSharp.Append("object");
								else
									AppendType(MemberType, CSharp);

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
										AppendType(MemberType, CSharp);
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

												AppendType(DefaultValueType, CSharp);
												CSharp.Append('.');
												CSharp.Append(Value.ToString());
											}

											if (First)
												CSharp.Append('0');
										}
										else
										{
											AppendType(DefaultValue.GetType(), CSharp);
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
						}
						else if (Attr is ByReferenceAttribute)
						{
							ByReference = true;
							this.hasByRef = true;
						}
						else if (Attr is ObjectIdAttribute)
						{
							this.objectIdFieldInfo = FI;
							this.objectIdPropertyInfo = PI;
							this.objectIdMemberInfo = Member;
							this.objectIdMemberType = MemberType;
							HasObjectId = true;
							Ignore = true;
						}
					}

					if (Ignore)
						continue;

					if (GetFieldDataTypeCode(Type) == TYPE_OBJECT)
					{
						CSharp.Append("\t\tprivate IObjectSerializer serializer");
						CSharp.Append(Member.Name);
						CSharp.AppendLine(";");
					}
				}

				if (NrDefault > 0)
					CSharp.AppendLine();

				CSharp.AppendLine();
				CSharp.Append("\t\tpublic Serializer");
				CSharp.Append(TypeName);
				CSharp.Append(this.context.Id);
				CSharp.AppendLine("(ISerializerContext Context)");
				CSharp.AppendLine("\t\t{");
				CSharp.AppendLine("\t\t\tthis.context = Context;");
				CSharp.AppendLine("\t\t}");
				CSharp.AppendLine();

				bool InitWritten = false;

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

					MemberTypeInfo = MemberType.GetTypeInfo();
					if (MemberTypeInfo.IsGenericType)
					{
						Type GT = MemberType.GetGenericTypeDefinition();
						if (GT == typeof(Nullable<>))
						{
							MemberType = MemberType.GenericTypeArguments[0];
							MemberTypeInfo = MemberType.GetTypeInfo();
						}
					}

					foreach (object Attr in Member.GetCustomAttributes(true))
					{
						if (Attr is IgnoreMemberAttribute || Attr is ObjectIdAttribute)
						{
							Ignore = true;
							break;
						}
					}

					if (Ignore)
						continue;

					if (GetFieldDataTypeCode(Type) == TYPE_OBJECT)
					{
						if (!InitWritten)
						{
							CSharp.AppendLine("\t\tpublic override async Task Init()");
							CSharp.AppendLine("\t\t{");

							InitWritten = true;
						}

						CSharp.Append("\t\t\tthis.serializer");
						CSharp.Append(Member.Name);
						CSharp.Append(" = await this.context.GetObjectSerializer(typeof(");
						AppendType(MemberType, CSharp);
						CSharp.AppendLine("));");
					}
				}

				if (!InitWritten)
				{
					CSharp.AppendLine("\t\tpublic override Task Init()");
					CSharp.AppendLine("\t\t{");
					CSharp.AppendLine("\t\t\treturn Task.CompletedTask;");
				}
				CSharp.AppendLine("\t\t}");
				CSharp.AppendLine();
				CSharp.Append("\t\tpublic override Type ValueType { get { return typeof(");
				AppendType(Type, CSharp);
				CSharp.AppendLine("); } }");
				CSharp.AppendLine();
				CSharp.Append("\t\tpublic override bool IsNullable { get { return ");
				CSharp.Append((this.isNullable ? "true" : "false"));
				CSharp.AppendLine("; } }");
				CSharp.AppendLine();
				CSharp.AppendLine("\t\tpublic override async Task<object> Deserialize(IDeserializer Reader, uint? DataType, bool Embedded)");
				CSharp.AppendLine("\t\t{");
				CSharp.AppendLine("\t\t\tuint FieldDataType;");

				if (this.normalized)
					CSharp.AppendLine("\t\t\tulong FieldCode;");
				else
					CSharp.AppendLine("\t\t\tstring FieldName;");

				CSharp.Append("\t\t\t");
				AppendType(Type, CSharp);
				CSharp.AppendLine(" Result;");
				CSharp.AppendLine("\t\t\tStreamBookmark Bookmark = Reader.GetBookmark();");
				CSharp.AppendLine("\t\t\tuint? DataTypeBak = DataType;");
				CSharp.AppendLine("\t\t\tGuid ObjectId = Embedded ? Guid.Empty : Reader.ReadGuid();");
				CSharp.AppendLine("\t\t\tulong ContentLen = Embedded ? 0 : Reader.ReadVariableLengthUInt64();");
				CSharp.AppendLine("\t\t\tDictionary<string, object> Obsolete = null;");
				CSharp.AppendLine();
				CSharp.AppendLine("\t\t\tif (!DataType.HasValue)");
				CSharp.AppendLine("\t\t\t{");
				CSharp.AppendLine("\t\t\t\tDataType = Reader.ReadBits(6);");
				CSharp.Append("\t\t\t\tif (DataType.Value == ");
				CSharp.Append(TYPE_NULL);
				CSharp.AppendLine(")");
				CSharp.AppendLine("\t\t\t\t\treturn null;");
				CSharp.AppendLine("\t\t\t}");
				CSharp.AppendLine();
				CSharp.AppendLine("\t\t\tif (Embedded && Reader.BitOffset > 0 && Reader.ReadBit())");
				CSharp.AppendLine("\t\t\t\tObjectId = Reader.ReadGuid();");
				CSharp.AppendLine();

				if (this.normalized)
					CSharp.AppendLine("\t\t\tFieldCode = Reader.ReadVariableLengthUInt64();");
				else
					CSharp.AppendLine("\t\t\tFieldName = Reader.ReadString();");

				if (this.typeNameSerialization != TypeNameSerialization.None)
				{
					if (this.normalized)
					{
						CSharp.Append("\t\t\tstring TypeName = await this.context.GetFieldName(\"");
						CSharp.Append(Escape(this.collectionName));
						CSharp.AppendLine("\", FieldCode);");
					}
					else
						CSharp.AppendLine("\t\t\tstring TypeName = FieldName;");

					if (this.typeNameSerialization == TypeNameSerialization.LocalName)
					{
						CSharp.AppendLine("\t\t\tif (TypeName.IndexOf('.') < 0)");
						CSharp.Append("\t\t\t\tTypeName = \"");
						CSharp.Append(Type.Namespace);
						CSharp.AppendLine(".\" + TypeName;");
					}

					CSharp.AppendLine();
					CSharp.AppendLine("\t\t\tType DesiredType = Waher.Runtime.Inventory.Types.GetType(TypeName);");
					CSharp.AppendLine("\t\t\tif (DesiredType is null)");
					CSharp.AppendLine("\t\t\t\tDesiredType = typeof(GenericObject);");
					CSharp.AppendLine();
					CSharp.Append("\t\t\tif (DesiredType != typeof(");
					AppendType(Type, CSharp);
					CSharp.AppendLine("))");
					CSharp.AppendLine("\t\t\t{");
					CSharp.AppendLine("\t\t\t\tIObjectSerializer Serializer2 = await this.context.GetObjectSerializer(DesiredType);");
					CSharp.AppendLine("\t\t\t\tReader.SetBookmark(Bookmark);");
					CSharp.AppendLine("\t\t\t\treturn await Serializer2.Deserialize(Reader, DataTypeBak, Embedded);");
					CSharp.AppendLine("\t\t\t}");
				}

				if (this.typeInfo.IsAbstract)
				{
					CSharp.AppendLine();
					CSharp.Append("\t\t\tthrow new SerializationException(\"Unable to create an instance of the abstract class ");
					AppendType(this.type, CSharp);
					CSharp.AppendLine(".\", this.ValueType);");
				}
				else
				{
					CSharp.AppendLine();

					CSharp.AppendLine("\t\t\tif (Embedded)");
					if (this.normalized)
						CSharp.AppendLine("\t\t\t\tReader.SkipVariableLengthInteger();	// Collection name");
					else
						CSharp.AppendLine("\t\t\t\tReader.SkipString();");

					CSharp.AppendLine();

					CSharp.Append("\t\t\tif (DataType.Value != ");
					CSharp.Append(TYPE_OBJECT);
					CSharp.AppendLine(")");
					CSharp.AppendLine("\t\t\t\tthrow new SerializationException(\"Object expected.\", this.ValueType);");

					CSharp.AppendLine();
					CSharp.Append("\t\t\tResult = new ");
					AppendType(Type, CSharp);
					CSharp.AppendLine("();");
					CSharp.AppendLine();

					if (HasObjectId)
					{
						if (this.objectIdMemberType == typeof(Guid))
						{
							CSharp.Append("\t\t\tResult.");
							CSharp.Append(this.objectIdMemberInfo.Name);
							CSharp.AppendLine(" = ObjectId;");
						}
						else if (this.objectIdMemberType == typeof(string))
						{
							CSharp.Append("\t\t\tResult.");
							CSharp.Append(this.objectIdMemberInfo.Name);
							CSharp.AppendLine(" = ObjectId.ToString();");
						}
						else if (this.objectIdMemberType == typeof(byte[]))
						{
							CSharp.Append("\t\t\tResult.");
							CSharp.Append(this.objectIdMemberInfo.Name);
							CSharp.AppendLine(" = ObjectId.ToByteArray();");
						}
						else
						{
							sb.Clear();

							sb.Append("Type not supported for Object ID fields: ");
							AppendType(this.objectIdMemberType, sb);

							throw new SerializationException(sb.ToString(), this.type);
						}

						CSharp.AppendLine();
					}

					if (this.normalized)
						CSharp.AppendLine("\t\t\twhile ((FieldCode = Reader.ReadVariableLengthUInt64()) != 0)");
					else
						CSharp.AppendLine("\t\t\twhile (!string.IsNullOrEmpty(FieldName = Reader.ReadString()))");

					CSharp.AppendLine("\t\t\t{");
					CSharp.AppendLine("\t\t\t\tFieldDataType = Reader.ReadBits(6);");
					CSharp.AppendLine();

					if (this.normalized)
						CSharp.AppendLine("\t\t\t\tswitch (FieldCode)");
					else
						CSharp.AppendLine("\t\t\t\tswitch (FieldName)");

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
						ShortName = null;
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

						foreach (object Attr in Member.GetCustomAttributes(true))
						{
							if (Attr is IgnoreMemberAttribute || Attr is ObjectIdAttribute)
							{
								Ignore = true;
								break;
							}
							else if (Attr is ByReferenceAttribute)
								ByReference = true;
							else if (Attr is ShortNameAttribute ShortNameAttribute)
								ShortName = ShortNameAttribute.Name;
						}

						if (Ignore)
							continue;

						if (this.normalized)
						{
							CSharp.Append("\t\t\t\t\tcase ");
							CSharp.Append(await this.context.GetFieldCode(this.collectionName, Member.Name));
							CSharp.AppendLine(":");
						}
						else
						{
							CSharp.Append("\t\t\t\t\tcase \"");
							CSharp.Append(Member.Name);
							CSharp.AppendLine("\":");

							if (!string.IsNullOrEmpty(ShortName) && ShortName != Member.Name)
							{
								CSharp.Append("\t\t\t\t\tcase \"");
								CSharp.Append(ShortName);
								CSharp.AppendLine("\":");
							}
						}

						if (MemberTypeInfo.IsEnum)
						{
							CSharp.AppendLine("\t\t\t\t\t\tswitch (FieldDataType)");
							CSharp.AppendLine("\t\t\t\t\t\t{");
							CSharp.Append("\t\t\t\t\t\t\tcase ");
							CSharp.Append(TYPE_BOOLEAN);
							CSharp.AppendLine(":");
							CSharp.Append("\t\t\t\t\t\t\t\tResult.");
							CSharp.Append(Member.Name);
							CSharp.Append(" = (");
							AppendType(MemberType, CSharp);
							CSharp.AppendLine(")(Reader.ReadBoolean() ? 1 : 0);");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
							CSharp.AppendLine();
							CSharp.Append("\t\t\t\t\t\t\tcase ");
							CSharp.Append(TYPE_BYTE);
							CSharp.AppendLine(":");
							CSharp.Append("\t\t\t\t\t\t\t\tResult.");
							CSharp.Append(Member.Name);
							CSharp.Append(" = (");
							AppendType(MemberType, CSharp);
							CSharp.AppendLine(")Reader.ReadByte();");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
							CSharp.AppendLine();
							CSharp.Append("\t\t\t\t\t\t\tcase ");
							CSharp.Append(TYPE_INT16);
							CSharp.AppendLine(":");
							CSharp.Append("\t\t\t\t\t\t\t\tResult.");
							CSharp.Append(Member.Name);
							CSharp.Append(" = (");
							AppendType(MemberType, CSharp);
							CSharp.AppendLine(")Reader.ReadInt16();");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
							CSharp.AppendLine();
							CSharp.Append("\t\t\t\t\t\t\tcase ");
							CSharp.Append(TYPE_INT32);
							CSharp.AppendLine(":");
							CSharp.Append("\t\t\t\t\t\t\t\tResult.");
							CSharp.Append(Member.Name);
							CSharp.Append(" = (");
							AppendType(MemberType, CSharp);
							CSharp.AppendLine(")Reader.ReadInt32();");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
							CSharp.AppendLine();
							CSharp.Append("\t\t\t\t\t\t\tcase ");
							CSharp.Append(TYPE_INT64);
							CSharp.AppendLine(":");
							CSharp.Append("\t\t\t\t\t\t\t\tResult.");
							CSharp.Append(Member.Name);
							CSharp.Append(" = (");
							AppendType(MemberType, CSharp);
							CSharp.AppendLine(")Reader.ReadInt64();");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
							CSharp.AppendLine();
							CSharp.Append("\t\t\t\t\t\t\tcase ");
							CSharp.Append(TYPE_SBYTE);
							CSharp.AppendLine(":");
							CSharp.Append("\t\t\t\t\t\t\t\tResult.");
							CSharp.Append(Member.Name);
							CSharp.Append(" = (");
							AppendType(MemberType, CSharp);
							CSharp.AppendLine(")Reader.ReadSByte();");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
							CSharp.AppendLine();
							CSharp.Append("\t\t\t\t\t\t\tcase ");
							CSharp.Append(TYPE_UINT16);
							CSharp.AppendLine(":");
							CSharp.Append("\t\t\t\t\t\t\t\tResult.");
							CSharp.Append(Member.Name);
							CSharp.Append(" = (");
							AppendType(MemberType, CSharp);
							CSharp.AppendLine(")Reader.ReadUInt16();");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
							CSharp.AppendLine();
							CSharp.Append("\t\t\t\t\t\t\tcase ");
							CSharp.Append(TYPE_UINT32);
							CSharp.AppendLine(":");
							CSharp.Append("\t\t\t\t\t\t\t\tResult.");
							CSharp.Append(Member.Name);
							CSharp.Append(" = (");
							AppendType(MemberType, CSharp);
							CSharp.AppendLine(")Reader.ReadUInt32();");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
							CSharp.AppendLine();
							CSharp.Append("\t\t\t\t\t\t\tcase ");
							CSharp.Append(TYPE_UINT64);
							CSharp.AppendLine(":");
							CSharp.Append("\t\t\t\t\t\t\t\tResult.");
							CSharp.Append(Member.Name);
							CSharp.Append(" = (");
							AppendType(MemberType, CSharp);
							CSharp.AppendLine(")Reader.ReadUInt64();");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
							CSharp.AppendLine();
							CSharp.Append("\t\t\t\t\t\t\tcase ");
							CSharp.Append(TYPE_VARINT16);
							CSharp.AppendLine(":");
							CSharp.Append("\t\t\t\t\t\t\t\tResult.");
							CSharp.Append(Member.Name);
							CSharp.Append(" = (");
							AppendType(MemberType, CSharp);
							CSharp.AppendLine(")Reader.ReadVariableLengthInt16();");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
							CSharp.AppendLine();
							CSharp.Append("\t\t\t\t\t\t\tcase ");
							CSharp.Append(TYPE_VARINT32);
							CSharp.AppendLine(":");
							CSharp.Append("\t\t\t\t\t\t\t\tResult.");
							CSharp.Append(Member.Name);
							CSharp.Append(" = (");
							AppendType(MemberType, CSharp);
							CSharp.AppendLine(")Reader.ReadVariableLengthInt32();");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
							CSharp.AppendLine();
							CSharp.Append("\t\t\t\t\t\t\tcase ");
							CSharp.Append(TYPE_VARINT64);
							CSharp.AppendLine(":");
							CSharp.Append("\t\t\t\t\t\t\t\tResult.");
							CSharp.Append(Member.Name);
							CSharp.Append(" = (");
							AppendType(MemberType, CSharp);
							CSharp.AppendLine(")Reader.ReadVariableLengthInt64();");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
							CSharp.AppendLine();
							CSharp.Append("\t\t\t\t\t\t\tcase ");
							CSharp.Append(TYPE_VARUINT16);
							CSharp.AppendLine(":");
							CSharp.Append("\t\t\t\t\t\t\t\tResult.");
							CSharp.Append(Member.Name);
							CSharp.Append(" = (");
							AppendType(MemberType, CSharp);
							CSharp.AppendLine(")Reader.ReadVariableLengthUInt16();");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
							CSharp.AppendLine();
							CSharp.Append("\t\t\t\t\t\t\tcase ");
							CSharp.Append(TYPE_VARUINT32);
							CSharp.AppendLine(":");
							CSharp.Append("\t\t\t\t\t\t\t\tResult.");
							CSharp.Append(Member.Name);
							CSharp.Append(" = (");
							AppendType(MemberType, CSharp);
							CSharp.AppendLine(")Reader.ReadVariableLengthUInt32();");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
							CSharp.AppendLine();
							CSharp.Append("\t\t\t\t\t\t\tcase ");
							CSharp.Append(TYPE_VARUINT64);
							CSharp.AppendLine(":");
							CSharp.Append("\t\t\t\t\t\t\t\tResult.");
							CSharp.Append(Member.Name);
							CSharp.Append(" = (");
							AppendType(MemberType, CSharp);
							CSharp.AppendLine(")Reader.ReadVariableLengthUInt64();");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
							CSharp.AppendLine();
							CSharp.Append("\t\t\t\t\t\t\tcase ");
							CSharp.Append(TYPE_DECIMAL);
							CSharp.AppendLine(":");
							CSharp.Append("\t\t\t\t\t\t\t\tResult.");
							CSharp.Append(Member.Name);
							CSharp.Append(" = (");
							AppendType(MemberType, CSharp);
							CSharp.AppendLine(")(int)Reader.ReadDecimal();");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
							CSharp.AppendLine();
							CSharp.Append("\t\t\t\t\t\t\tcase ");
							CSharp.Append(TYPE_DOUBLE);
							CSharp.AppendLine(":");
							CSharp.Append("\t\t\t\t\t\t\t\tResult.");
							CSharp.Append(Member.Name);
							CSharp.Append(" = (");
							AppendType(MemberType, CSharp);
							CSharp.AppendLine(")(int)Reader.ReadDouble();");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
							CSharp.AppendLine();
							CSharp.Append("\t\t\t\t\t\t\tcase ");
							CSharp.Append(TYPE_SINGLE);
							CSharp.AppendLine(":");
							CSharp.Append("\t\t\t\t\t\t\t\tResult.");
							CSharp.Append(Member.Name);
							CSharp.Append(" = (");
							AppendType(MemberType, CSharp);
							CSharp.AppendLine(")(int)Reader.ReadSingle();");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
							CSharp.AppendLine();
							CSharp.Append("\t\t\t\t\t\t\tcase ");
							CSharp.Append(TYPE_STRING);
							CSharp.AppendLine(":");
							CSharp.Append("\t\t\t\t\t\t\tcase ");
							CSharp.Append(TYPE_CI_STRING);
							CSharp.AppendLine(":");
							CSharp.Append("\t\t\t\t\t\t\t\tResult.");
							CSharp.Append(Member.Name);
							CSharp.Append(" = (");
							AppendType(MemberType, CSharp);
							CSharp.Append(")Enum.Parse(typeof(");
							AppendType(MemberType, CSharp);
							CSharp.AppendLine("), Reader.ReadString());");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
							CSharp.AppendLine();
							CSharp.Append("\t\t\t\t\t\t\tcase ");
							CSharp.Append(TYPE_ENUM);
							CSharp.AppendLine(":");
							CSharp.Append("\t\t\t\t\t\t\t\tResult.");
							CSharp.Append(Member.Name);
							CSharp.Append(" = (");
							AppendType(MemberType, CSharp);
							CSharp.Append(")Reader.ReadEnum(typeof(");
							AppendType(MemberType, CSharp);
							CSharp.AppendLine("));");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
							CSharp.AppendLine();
							CSharp.Append("\t\t\t\t\t\t\tcase ");
							CSharp.Append(TYPE_NULL);
							CSharp.AppendLine(":");
							CSharp.Append("\t\t\t\t\t\t\t\tResult.");
							CSharp.Append(Member.Name);
							CSharp.AppendLine(Nullable ? " = null;" : " = default;");
							CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
							CSharp.AppendLine();
							CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
							CSharp.Append("\t\t\t\t\t\t\t\tthrow new SerializationException(\"Unable to set ");
							CSharp.Append(Member.Name);
							CSharp.AppendLine(". Expected an enumeration value, but was a \" + ObjectSerializer.GetFieldDataTypeName(FieldDataType) + \".\", this.ValueType);");
							CSharp.AppendLine("\t\t\t\t\t\t}");
						}
						else
						{
							switch (Type.GetTypeCode(MemberType))
							{
								case TypeCode.Boolean:
									if (Nullable)
									{
										CSharp.Append("\t\t\t\t\t\tResult.");
										CSharp.Append(Member.Name);
										CSharp.AppendLine(" = ReadNullableBoolean(Reader, FieldDataType);");
									}
									else
									{
										CSharp.Append("\t\t\t\t\t\tResult.");
										CSharp.Append(Member.Name);
										CSharp.AppendLine(" = ReadBoolean(Reader, FieldDataType);");
									}
									break;

								case TypeCode.Byte:
									if (Nullable)
									{
										CSharp.Append("\t\t\t\t\t\tResult.");
										CSharp.Append(Member.Name);
										CSharp.AppendLine(" = ReadNullableByte(Reader, FieldDataType);");
									}
									else
									{
										CSharp.Append("\t\t\t\t\t\tResult.");
										CSharp.Append(Member.Name);
										CSharp.AppendLine(" = ReadByte(Reader, FieldDataType);");
									}
									break;

								case TypeCode.Char:
									if (Nullable)
									{
										CSharp.Append("\t\t\t\t\t\tResult.");
										CSharp.Append(Member.Name);
										CSharp.AppendLine(" = ReadNullableChar(Reader, FieldDataType);");
									}
									else
									{
										CSharp.Append("\t\t\t\t\t\tResult.");
										CSharp.Append(Member.Name);
										CSharp.AppendLine(" = ReadChar(Reader, FieldDataType);");
									}
									break;

								case TypeCode.DateTime:
									if (Nullable)
									{
										CSharp.Append("\t\t\t\t\t\tResult.");
										CSharp.Append(Member.Name);
										CSharp.AppendLine(" = ReadNullableDateTime(Reader, FieldDataType);");
									}
									else
									{
										CSharp.Append("\t\t\t\t\t\tResult.");
										CSharp.Append(Member.Name);
										CSharp.AppendLine(" = ReadDateTime(Reader, FieldDataType);");
									}
									break;

								case TypeCode.Decimal:
									if (Nullable)
									{
										CSharp.Append("\t\t\t\t\t\tResult.");
										CSharp.Append(Member.Name);
										CSharp.AppendLine(" = ReadNullableDecimal(Reader, FieldDataType);");
									}
									else
									{
										CSharp.Append("\t\t\t\t\t\tResult.");
										CSharp.Append(Member.Name);
										CSharp.AppendLine(" = ReadDecimal(Reader, FieldDataType);");
									}
									break;

								case TypeCode.Double:
									if (Nullable)
									{
										CSharp.Append("\t\t\t\t\t\tResult.");
										CSharp.Append(Member.Name);
										CSharp.AppendLine(" = ReadNullableDouble(Reader, FieldDataType);");
									}
									else
									{
										CSharp.Append("\t\t\t\t\t\tResult.");
										CSharp.Append(Member.Name);
										CSharp.AppendLine(" = ReadDouble(Reader, FieldDataType);");
									}
									break;

								case TypeCode.Int16:
									if (Nullable)
									{
										CSharp.Append("\t\t\t\t\t\tResult.");
										CSharp.Append(Member.Name);
										CSharp.AppendLine(" = ReadNullableInt16(Reader, FieldDataType);");
									}
									else
									{
										CSharp.Append("\t\t\t\t\t\tResult.");
										CSharp.Append(Member.Name);
										CSharp.AppendLine(" = ReadInt16(Reader, FieldDataType);");
									}
									break;

								case TypeCode.Int32:
									if (Nullable)
									{
										CSharp.Append("\t\t\t\t\t\tResult.");
										CSharp.Append(Member.Name);
										CSharp.AppendLine(" = ReadNullableInt32(Reader, FieldDataType);");
									}
									else
									{
										CSharp.Append("\t\t\t\t\t\tResult.");
										CSharp.Append(Member.Name);
										CSharp.AppendLine(" = ReadInt32(Reader, FieldDataType);");
									}
									break;

								case TypeCode.Int64:
									if (Nullable)
									{
										CSharp.Append("\t\t\t\t\t\tResult.");
										CSharp.Append(Member.Name);
										CSharp.AppendLine(" = ReadNullableInt64(Reader, FieldDataType);");
									}
									else
									{
										CSharp.Append("\t\t\t\t\t\tResult.");
										CSharp.Append(Member.Name);
										CSharp.AppendLine(" = ReadInt64(Reader, FieldDataType);");
									}
									break;

								case TypeCode.SByte:
									if (Nullable)
									{
										CSharp.Append("\t\t\t\t\t\tResult.");
										CSharp.Append(Member.Name);
										CSharp.AppendLine(" = ReadNullableSByte(Reader, FieldDataType);");
									}
									else
									{
										CSharp.Append("\t\t\t\t\t\tResult.");
										CSharp.Append(Member.Name);
										CSharp.AppendLine(" = ReadSByte(Reader, FieldDataType);");
									}
									break;

								case TypeCode.Single:
									if (Nullable)
									{
										CSharp.Append("\t\t\t\t\t\tResult.");
										CSharp.Append(Member.Name);
										CSharp.AppendLine(" = ReadNullableSingle(Reader, FieldDataType);");
									}
									else
									{
										CSharp.Append("\t\t\t\t\t\tResult.");
										CSharp.Append(Member.Name);
										CSharp.AppendLine(" = ReadSingle(Reader, FieldDataType);");
									}
									break;

								case TypeCode.String:
									CSharp.Append("\t\t\t\t\t\tResult.");
									CSharp.Append(Member.Name);
									CSharp.AppendLine(" = ReadString(Reader, FieldDataType);");
									break;

								case TypeCode.UInt16:
									if (Nullable)
									{
										CSharp.Append("\t\t\t\t\t\tResult.");
										CSharp.Append(Member.Name);
										CSharp.AppendLine(" = ReadNullableUInt16(Reader, FieldDataType);");
									}
									else
									{
										CSharp.Append("\t\t\t\t\t\tResult.");
										CSharp.Append(Member.Name);
										CSharp.AppendLine(" = ReadUInt16(Reader, FieldDataType);");
									}
									break;

								case TypeCode.UInt32:
									if (Nullable)
									{
										CSharp.Append("\t\t\t\t\t\tResult.");
										CSharp.Append(Member.Name);
										CSharp.AppendLine(" = ReadNullableUInt32(Reader, FieldDataType);");
									}
									else
									{
										CSharp.Append("\t\t\t\t\t\tResult.");
										CSharp.Append(Member.Name);
										CSharp.AppendLine(" = ReadUInt32(Reader, FieldDataType);");
									}
									break;

								case TypeCode.UInt64:
									if (Nullable)
									{
										CSharp.Append("\t\t\t\t\t\tResult.");
										CSharp.Append(Member.Name);
										CSharp.AppendLine(" = ReadNullableUInt64(Reader, FieldDataType);");
									}
									else
									{
										CSharp.Append("\t\t\t\t\t\tResult.");
										CSharp.Append(Member.Name);
										CSharp.AppendLine(" = ReadUInt64(Reader, FieldDataType);");
									}
									break;

								case TypeCode.Empty:
								default:
									sb.Clear();

									sb.AppendLine("Invalid member type: ");
									AppendType(MemberType, sb);

									throw new SerializationException(sb.ToString(), this.type);

								case TypeCode.Object:
									if (MemberType.IsArray)
									{
										if (MemberType == typeof(byte[]))
										{
											CSharp.Append("\t\t\t\t\t\tResult.");
											CSharp.Append(Member.Name);
											CSharp.AppendLine(" = ReadByteArray(Reader, FieldDataType);");
										}
										else if (MemberType == typeof(KeyValuePair<string, object>[]))
										{
											CSharp.Append("\t\t\t\t\t\tResult.");
											CSharp.Append(Member.Name);
											CSharp.AppendLine(" = await ReadTagArray(this.context, Reader, FieldDataType);");
										}
										else if (MemberType == typeof(KeyValuePair<string, IElement>[]))
										{
											CSharp.Append("\t\t\t\t\t\tResult.");
											CSharp.Append(Member.Name);
											CSharp.AppendLine(" = await ReadTagElementArray(this.context, Reader, FieldDataType);");
										}
										else
										{
											MemberType = MemberType.GetElementType();
											CSharp.Append("\t\t\t\t\t\tResult.");
											CSharp.Append(Member.Name);
											CSharp.Append(" = await ReadArray<");
											AppendType(MemberType, CSharp);
											CSharp.AppendLine(">(this.context, Reader, FieldDataType);");
										}
									}
									else if (ByReference)
									{
										CSharp.AppendLine("\t\t\t\t\t\tswitch (FieldDataType)");
										CSharp.AppendLine("\t\t\t\t\t\t{");
										CSharp.Append("\t\t\t\t\t\t\tcase ");
										CSharp.Append(TYPE_GUID);
										CSharp.AppendLine(":");
										CSharp.Append("\t\t\t\t\t\t\t\tGuid ");
										CSharp.Append(Member.Name);
										CSharp.AppendLine("ObjectId = Reader.ReadGuid();");
										CSharp.Append("\t\t\t\t\t\t\t\tResult.");
										CSharp.Append(Member.Name);
										CSharp.Append(" = await this.context.TryLoadObject<");
										AppendType(MemberType, CSharp);
										CSharp.Append(">(");
										CSharp.Append(Member.Name);
										CSharp.Append("ObjectId, (EmbeddedValue) => Result.");
										CSharp.Append(Member.Name);
										CSharp.Append(" = (");
										AppendType(MemberType, CSharp);
										CSharp.AppendLine(")EmbeddedValue);");
										CSharp.Append("\t\t\t\t\t\t\t\tif (Result.");
										CSharp.Append(Member.Name);
										CSharp.AppendLine(" is null)");
										CSharp.AppendLine("\t\t\t\t\t\t\t\t\tthrow new KeyNotFoundException(\"Referenced object not found.\");");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										CSharp.AppendLine();
										CSharp.Append("\t\t\t\t\t\t\tcase ");
										CSharp.Append(TYPE_NULL);
										CSharp.AppendLine(":");
										CSharp.Append("\t\t\t\t\t\t\t\tResult.");
										CSharp.Append(Member.Name);
										CSharp.AppendLine(Nullable ? " = null;" : " = default;");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
										CSharp.Append("\t\t\t\t\t\t\t\tthrow new SerializationException(\"Object ID expected for ");
										CSharp.Append(Member.Name);
										CSharp.AppendLine(".\", this.ValueType);");
										CSharp.AppendLine("\t\t\t\t\t\t}");
									}
									else if (MemberType == typeof(TimeSpan))
									{
										if (Nullable)
										{
											CSharp.Append("\t\t\t\t\t\tResult.");
											CSharp.Append(Member.Name);
											CSharp.AppendLine(" = ReadNullableTimeSpan(Reader, FieldDataType);");
										}
										else
										{
											CSharp.Append("\t\t\t\t\t\tResult.");
											CSharp.Append(Member.Name);
											CSharp.AppendLine(" = ReadTimeSpan(Reader, FieldDataType);");
										}
									}
									else if (MemberType == typeof(Guid))
									{
										if (Nullable)
										{
											CSharp.Append("\t\t\t\t\t\tResult.");
											CSharp.Append(Member.Name);
											CSharp.AppendLine(" = ReadNullableGuid(Reader, FieldDataType);");
										}
										else
										{
											CSharp.Append("\t\t\t\t\t\tResult.");
											CSharp.Append(Member.Name);
											CSharp.AppendLine(" = ReadGuid(Reader, FieldDataType);");
										}
									}
									else if (MemberType == typeof(DateTimeOffset))
									{
										if (Nullable)
										{
											CSharp.Append("\t\t\t\t\t\tResult.");
											CSharp.Append(Member.Name);
											CSharp.AppendLine(" = ReadNullableDateTimeOffset(Reader, FieldDataType);");
										}
										else
										{
											CSharp.Append("\t\t\t\t\t\tResult.");
											CSharp.Append(Member.Name);
											CSharp.AppendLine(" = ReadDateTimeOffset(Reader, FieldDataType);");
										}
									}
									else
									{
										CSharp.AppendLine("\t\t\t\t\t\tswitch (FieldDataType)");
										CSharp.AppendLine("\t\t\t\t\t\t{");
										CSharp.Append("\t\t\t\t\t\t\tcase ");
										CSharp.Append(TYPE_OBJECT);
										CSharp.AppendLine(":");
										CSharp.Append("\t\t\t\t\t\t\t\tResult.");
										CSharp.Append(Member.Name);
										CSharp.Append(" = (");
										AppendType(MemberType, CSharp);
										CSharp.Append(")await this.serializer");
										CSharp.Append(Member.Name);
										CSharp.AppendLine(".Deserialize(Reader, FieldDataType, true);");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										CSharp.AppendLine();
										CSharp.Append("\t\t\t\t\t\t\tcase ");
										CSharp.Append(TYPE_NULL);
										CSharp.AppendLine(":");
										CSharp.Append("\t\t\t\t\t\t\t\tResult.");
										CSharp.Append(Member.Name);
										CSharp.AppendLine(Nullable ? " = null;" : " = default;");
										CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");

										if (MemberTypeInfo.IsAssignableFrom(typeof(bool)))
										{
											CSharp.AppendLine();
											CSharp.Append("\t\t\t\t\t\t\tcase ");
											CSharp.Append(TYPE_BOOLEAN);
											CSharp.AppendLine(":");
											CSharp.Append("\t\t\t\t\t\t\t\tResult.");
											CSharp.Append(Member.Name);
											CSharp.Append(" = (");
											AppendType(MemberType, CSharp);
											CSharp.AppendLine(")ReadBoolean(Reader, FieldDataType);");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										}

										if (MemberTypeInfo.IsAssignableFrom(typeof(byte)))
										{
											CSharp.AppendLine();
											CSharp.Append("\t\t\t\t\t\t\tcase ");
											CSharp.Append(TYPE_BYTE);
											CSharp.AppendLine(":");
											CSharp.Append("\t\t\t\t\t\t\t\tResult.");
											CSharp.Append(Member.Name);
											CSharp.Append(" = (");
											AppendType(MemberType, CSharp);
											CSharp.AppendLine(")ReadByte(Reader, FieldDataType);");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										}

										if (MemberTypeInfo.IsAssignableFrom(typeof(short)))
										{
											CSharp.AppendLine();
											CSharp.Append("\t\t\t\t\t\t\tcase ");
											CSharp.Append(TYPE_INT16);
											CSharp.AppendLine(":");
											CSharp.Append("\t\t\t\t\t\t\tcase ");
											CSharp.Append(TYPE_VARINT16);
											CSharp.AppendLine(":");
											CSharp.Append("\t\t\t\t\t\t\t\tResult.");
											CSharp.Append(Member.Name);
											CSharp.Append(" = (");
											AppendType(MemberType, CSharp);
											CSharp.AppendLine(")ReadInt16(Reader, FieldDataType);");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										}

										if (MemberTypeInfo.IsAssignableFrom(typeof(int)))
										{
											CSharp.AppendLine();
											CSharp.Append("\t\t\t\t\t\t\tcase ");
											CSharp.Append(TYPE_INT32);
											CSharp.AppendLine(":");
											CSharp.Append("\t\t\t\t\t\t\tcase ");
											CSharp.Append(TYPE_VARINT32);
											CSharp.AppendLine(":");
											CSharp.Append("\t\t\t\t\t\t\t\tResult.");
											CSharp.Append(Member.Name);
											CSharp.Append(" = (");
											AppendType(MemberType, CSharp);
											CSharp.AppendLine(")ReadInt32(Reader, FieldDataType);");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										}

										if (MemberTypeInfo.IsAssignableFrom(typeof(long)))
										{
											CSharp.AppendLine();
											CSharp.Append("\t\t\t\t\t\t\tcase ");
											CSharp.Append(TYPE_INT64);
											CSharp.AppendLine(":");
											CSharp.Append("\t\t\t\t\t\t\tcase ");
											CSharp.Append(TYPE_VARINT64);
											CSharp.AppendLine(":");
											CSharp.Append("\t\t\t\t\t\t\t\tResult.");
											CSharp.Append(Member.Name);
											CSharp.Append(" = (");
											AppendType(MemberType, CSharp);
											CSharp.AppendLine(")ReadInt64(Reader, FieldDataType);");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										}

										if (MemberTypeInfo.IsAssignableFrom(typeof(sbyte)))
										{
											CSharp.AppendLine();
											CSharp.Append("\t\t\t\t\t\t\tcase ");
											CSharp.Append(TYPE_SBYTE);
											CSharp.AppendLine(":");
											CSharp.Append("\t\t\t\t\t\t\t\tResult.");
											CSharp.Append(Member.Name);
											CSharp.Append(" = (");
											AppendType(MemberType, CSharp);
											CSharp.AppendLine(")ReadSByte(Reader, FieldDataType);");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										}

										if (MemberTypeInfo.IsAssignableFrom(typeof(ushort)))
										{
											CSharp.AppendLine();
											CSharp.Append("\t\t\t\t\t\t\tcase ");
											CSharp.Append(TYPE_UINT16);
											CSharp.AppendLine(":");
											CSharp.Append("\t\t\t\t\t\t\tcase ");
											CSharp.Append(TYPE_VARUINT16);
											CSharp.AppendLine(":");
											CSharp.Append("\t\t\t\t\t\t\t\tResult.");
											CSharp.Append(Member.Name);
											CSharp.Append(" = (");
											AppendType(MemberType, CSharp);
											CSharp.AppendLine(")ReadUInt16(Reader, FieldDataType);");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										}

										if (MemberTypeInfo.IsAssignableFrom(typeof(uint)))
										{
											CSharp.AppendLine();
											CSharp.Append("\t\t\t\t\t\t\tcase ");
											CSharp.Append(TYPE_UINT32);
											CSharp.AppendLine(":");
											CSharp.Append("\t\t\t\t\t\t\tcase ");
											CSharp.Append(TYPE_VARUINT32);
											CSharp.AppendLine(":");
											CSharp.Append("\t\t\t\t\t\t\t\tResult.");
											CSharp.Append(Member.Name);
											CSharp.Append(" = (");
											AppendType(MemberType, CSharp);
											CSharp.AppendLine(")ReadUInt32(Reader, FieldDataType);");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										}

										if (MemberTypeInfo.IsAssignableFrom(typeof(ulong)))
										{
											CSharp.AppendLine();
											CSharp.Append("\t\t\t\t\t\t\tcase ");
											CSharp.Append(TYPE_UINT64);
											CSharp.AppendLine(":");
											CSharp.Append("\t\t\t\t\t\t\tcase ");
											CSharp.Append(TYPE_VARUINT64);
											CSharp.AppendLine(":");
											CSharp.Append("\t\t\t\t\t\t\t\tResult.");
											CSharp.Append(Member.Name);
											CSharp.Append(" = (");
											AppendType(MemberType, CSharp);
											CSharp.AppendLine(")ReadUInt64(Reader, FieldDataType);");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										}

										if (MemberTypeInfo.IsAssignableFrom(typeof(decimal)))
										{
											CSharp.AppendLine();
											CSharp.Append("\t\t\t\t\t\t\tcase ");
											CSharp.Append(TYPE_DECIMAL);
											CSharp.AppendLine(":");
											CSharp.Append("\t\t\t\t\t\t\t\tResult.");
											CSharp.Append(Member.Name);
											CSharp.Append(" = (");
											AppendType(MemberType, CSharp);
											CSharp.AppendLine(")ReadDecimal(Reader, FieldDataType);");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										}

										if (MemberTypeInfo.IsAssignableFrom(typeof(double)))
										{
											CSharp.AppendLine();
											CSharp.Append("\t\t\t\t\t\t\tcase ");
											CSharp.Append(TYPE_DOUBLE);
											CSharp.AppendLine(":");
											CSharp.Append("\t\t\t\t\t\t\t\tResult.");
											CSharp.Append(Member.Name);
											CSharp.Append(" = (");
											AppendType(MemberType, CSharp);
											CSharp.AppendLine(")ReadDouble(Reader, FieldDataType);");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										}

										if (MemberTypeInfo.IsAssignableFrom(typeof(float)))
										{
											CSharp.AppendLine();
											CSharp.Append("\t\t\t\t\t\t\tcase ");
											CSharp.Append(TYPE_SINGLE);
											CSharp.AppendLine(":");
											CSharp.Append("\t\t\t\t\t\t\t\tResult.");
											CSharp.Append(Member.Name);
											CSharp.Append(" = (");
											AppendType(MemberType, CSharp);
											CSharp.AppendLine(")ReadSingle(Reader, FieldDataType);");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										}

										if (MemberTypeInfo.IsAssignableFrom(typeof(DateTime)))
										{
											CSharp.AppendLine();
											CSharp.Append("\t\t\t\t\t\t\tcase ");
											CSharp.Append(TYPE_DATETIME);
											CSharp.AppendLine(":");
											CSharp.Append("\t\t\t\t\t\t\t\tResult.");
											CSharp.Append(Member.Name);
											CSharp.Append(" = (");
											AppendType(MemberType, CSharp);
											CSharp.AppendLine(")ReadDateTime(Reader, FieldDataType);");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										}

										if (MemberTypeInfo.IsAssignableFrom(typeof(DateTimeOffset)))
										{
											CSharp.AppendLine();
											CSharp.Append("\t\t\t\t\t\t\tcase ");
											CSharp.Append(TYPE_DATETIMEOFFSET);
											CSharp.AppendLine(":");
											CSharp.Append("\t\t\t\t\t\t\t\tResult.");
											CSharp.Append(Member.Name);
											CSharp.Append(" = (");
											AppendType(MemberType, CSharp);
											CSharp.AppendLine(")ReadDateTimeOffset(Reader, FieldDataType);");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										}

										if (MemberTypeInfo.IsAssignableFrom(typeof(TimeSpan)))
										{
											CSharp.AppendLine();
											CSharp.Append("\t\t\t\t\t\t\tcase ");
											CSharp.Append(TYPE_TIMESPAN);
											CSharp.AppendLine(":");
											CSharp.Append("\t\t\t\t\t\t\t\tResult.");
											CSharp.Append(Member.Name);
											CSharp.Append(" = (");
											AppendType(MemberType, CSharp);
											CSharp.AppendLine(")ReadTimeSpan(Reader, FieldDataType);");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										}

										if (MemberTypeInfo.IsAssignableFrom(typeof(char)))
										{
											CSharp.AppendLine();
											CSharp.Append("\t\t\t\t\t\t\tcase ");
											CSharp.Append(TYPE_CHAR);
											CSharp.AppendLine(":");
											CSharp.Append("\t\t\t\t\t\t\t\tResult.");
											CSharp.Append(Member.Name);
											CSharp.Append(" = (");
											AppendType(MemberType, CSharp);
											CSharp.AppendLine(")ReadChar(Reader, FieldDataType);");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										}

										if (MemberTypeInfo.IsAssignableFrom(typeof(string)) ||
											MemberTypeInfo.IsAssignableFrom(typeof(CaseInsensitiveString)))
										{
											CSharp.AppendLine();
											CSharp.Append("\t\t\t\t\t\t\tcase ");
											CSharp.Append(TYPE_STRING);
											CSharp.AppendLine(":");
											CSharp.Append("\t\t\t\t\t\t\tcase ");
											CSharp.Append(TYPE_CI_STRING);
											CSharp.AppendLine(":");
											CSharp.Append("\t\t\t\t\t\t\t\tResult.");
											CSharp.Append(Member.Name);
											CSharp.Append(" = (");
											AppendType(MemberType, CSharp);
											CSharp.AppendLine(")ReadString(Reader, FieldDataType);");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										}

										if (MemberTypeInfo.IsAssignableFrom(typeof(byte[])))
										{
											CSharp.AppendLine();
											CSharp.Append("\t\t\t\t\t\t\tcase ");
											CSharp.Append(TYPE_BYTEARRAY);
											CSharp.AppendLine(":");
											CSharp.Append("\t\t\t\t\t\t\t\tResult.");
											CSharp.Append(Member.Name);
											CSharp.Append(" = (");
											AppendType(MemberType, CSharp);
											CSharp.AppendLine(")ReadByteArray(Reader, FieldDataType);");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										}

										if (MemberTypeInfo.IsAssignableFrom(typeof(Guid)))
										{
											CSharp.AppendLine();
											CSharp.Append("\t\t\t\t\t\t\tcase ");
											CSharp.Append(TYPE_GUID);
											CSharp.AppendLine(":");
											CSharp.Append("\t\t\t\t\t\t\t\tResult.");
											CSharp.Append(Member.Name);
											CSharp.Append(" = (");
											AppendType(MemberType, CSharp);
											CSharp.AppendLine(")ReadGuid(Reader, FieldDataType);");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										}

										if (MemberTypeInfo.IsAssignableFrom(typeof(Enum)))
										{
											CSharp.AppendLine();
											CSharp.Append("\t\t\t\t\t\t\tcase ");
											CSharp.Append(TYPE_ENUM);
											CSharp.AppendLine(":");
											CSharp.Append("\t\t\t\t\t\t\t\tResult.");
											CSharp.Append(Member.Name);
											CSharp.Append(" = (");
											AppendType(MemberType, CSharp);
											CSharp.AppendLine(")ReadString(Reader, FieldDataType);");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										}

										if (MemberTypeInfo.IsAssignableFrom(typeof(Array)))
										{
											CSharp.AppendLine();
											CSharp.Append("\t\t\t\t\t\t\tcase ");
											CSharp.Append(TYPE_ARRAY);
											CSharp.AppendLine(":");
											CSharp.Append("\t\t\t\t\t\t\t\tResult.");
											CSharp.Append(Member.Name);
											CSharp.Append(" = (");
											AppendType(MemberType, CSharp);
											CSharp.AppendLine(")await ReadArray<Waher.Persistence.Serialization.GenericObject>(this.context, Reader, FieldDataType);");
											CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
										}

										CSharp.AppendLine();
										CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
										CSharp.Append("\t\t\t\t\t\t\t\tthrow new SerializationException(\"Object expected for ");
										CSharp.Append(Member.Name);
										CSharp.AppendLine(". Data Type read: \" + ObjectSerializer.GetFieldDataTypeName(FieldDataType), this.ValueType);");
										CSharp.AppendLine("\t\t\t\t\t\t}");
									}
									break;
							}
						}

						CSharp.AppendLine("\t\t\t\t\t\tbreak;");
						CSharp.AppendLine();
					}

					CSharp.AppendLine("\t\t\t\t\tdefault:");

					if (this.normalized)
					{
						CSharp.Append("\t\t\t\t\t\tstring FieldName = await this.context.GetFieldName(\"");
						if (!string.IsNullOrEmpty(this.collectionName))
							CSharp.Append(Escape(this.collectionName));
						CSharp.AppendLine("\", FieldCode);");
					}

					CSharp.AppendLine("\t\t\t\t\t\tobject FieldValue;");
					CSharp.AppendLine();
					CSharp.AppendLine("\t\t\t\t\t\tswitch (FieldDataType)");
					CSharp.AppendLine("\t\t\t\t\t\t{");
					CSharp.Append("\t\t\t\t\t\t\tcase ");
					CSharp.Append(TYPE_OBJECT);
					CSharp.AppendLine(":");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tFieldValue = await ReadEmbeddedObject(this.context, Reader, FieldDataType);");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
					CSharp.AppendLine();
					CSharp.Append("\t\t\t\t\t\t\tcase ");
					CSharp.Append(TYPE_NULL);
					CSharp.AppendLine(":");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tFieldValue = null;");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
					CSharp.AppendLine();
					CSharp.Append("\t\t\t\t\t\t\tcase ");
					CSharp.Append(TYPE_BOOLEAN);
					CSharp.AppendLine(":");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tFieldValue = ReadBoolean(Reader, FieldDataType);");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
					CSharp.AppendLine();
					CSharp.Append("\t\t\t\t\t\t\tcase ");
					CSharp.Append(TYPE_BYTE);
					CSharp.AppendLine(":");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tFieldValue = ReadByte(Reader, FieldDataType);");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
					CSharp.AppendLine();
					CSharp.Append("\t\t\t\t\t\t\tcase ");
					CSharp.Append(TYPE_INT16);
					CSharp.AppendLine(":");
					CSharp.Append("\t\t\t\t\t\t\tcase ");
					CSharp.Append(TYPE_VARINT16);
					CSharp.AppendLine(":");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tFieldValue = ReadInt16(Reader, FieldDataType);");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
					CSharp.AppendLine();
					CSharp.Append("\t\t\t\t\t\t\tcase ");
					CSharp.Append(TYPE_INT32);
					CSharp.AppendLine(":");
					CSharp.Append("\t\t\t\t\t\t\tcase ");
					CSharp.Append(TYPE_VARINT32);
					CSharp.AppendLine(":");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tFieldValue = ReadInt32(Reader, FieldDataType);");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
					CSharp.AppendLine();
					CSharp.Append("\t\t\t\t\t\t\tcase ");
					CSharp.Append(TYPE_INT64);
					CSharp.AppendLine(":");
					CSharp.Append("\t\t\t\t\t\t\tcase ");
					CSharp.Append(TYPE_VARINT64);
					CSharp.AppendLine(":");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tFieldValue = ReadInt64(Reader, FieldDataType);");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
					CSharp.AppendLine();
					CSharp.Append("\t\t\t\t\t\t\tcase ");
					CSharp.Append(TYPE_SBYTE);
					CSharp.AppendLine(":");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tFieldValue = ReadSByte(Reader, FieldDataType);");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
					CSharp.AppendLine();
					CSharp.Append("\t\t\t\t\t\t\tcase ");
					CSharp.Append(TYPE_UINT16);
					CSharp.AppendLine(":");
					CSharp.Append("\t\t\t\t\t\t\tcase ");
					CSharp.Append(TYPE_VARUINT16);
					CSharp.AppendLine(":");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tFieldValue = ReadUInt16(Reader, FieldDataType);");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
					CSharp.AppendLine();
					CSharp.Append("\t\t\t\t\t\t\tcase ");
					CSharp.Append(TYPE_UINT32);
					CSharp.AppendLine(":");
					CSharp.Append("\t\t\t\t\t\t\tcase ");
					CSharp.Append(TYPE_VARUINT32);
					CSharp.AppendLine(":");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tFieldValue = ReadUInt32(Reader, FieldDataType);");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
					CSharp.AppendLine();
					CSharp.Append("\t\t\t\t\t\t\tcase ");
					CSharp.Append(TYPE_UINT64);
					CSharp.AppendLine(":");
					CSharp.Append("\t\t\t\t\t\t\tcase ");
					CSharp.Append(TYPE_VARUINT64);
					CSharp.AppendLine(":");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tFieldValue = ReadUInt64(Reader, FieldDataType);");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
					CSharp.AppendLine();
					CSharp.Append("\t\t\t\t\t\t\tcase ");
					CSharp.Append(TYPE_DECIMAL);
					CSharp.AppendLine(":");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tFieldValue = ReadDecimal(Reader, FieldDataType);");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
					CSharp.AppendLine();
					CSharp.Append("\t\t\t\t\t\t\tcase ");
					CSharp.Append(TYPE_DOUBLE);
					CSharp.AppendLine(":");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tFieldValue = ReadDouble(Reader, FieldDataType);");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
					CSharp.AppendLine();
					CSharp.Append("\t\t\t\t\t\t\tcase ");
					CSharp.Append(TYPE_SINGLE);
					CSharp.AppendLine(":");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tFieldValue = ReadSingle(Reader, FieldDataType);");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
					CSharp.AppendLine();
					CSharp.Append("\t\t\t\t\t\t\tcase ");
					CSharp.Append(TYPE_DATETIME);
					CSharp.AppendLine(":");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tFieldValue = ReadDateTime(Reader, FieldDataType);");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
					CSharp.AppendLine();
					CSharp.Append("\t\t\t\t\t\t\tcase ");
					CSharp.Append(TYPE_DATETIMEOFFSET);
					CSharp.AppendLine(":");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tFieldValue = ReadDateTimeOffset(Reader, FieldDataType);");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
					CSharp.AppendLine();
					CSharp.Append("\t\t\t\t\t\t\tcase ");
					CSharp.Append(TYPE_TIMESPAN);
					CSharp.AppendLine(":");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tFieldValue = ReadTimeSpan(Reader, FieldDataType);");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
					CSharp.AppendLine();
					CSharp.Append("\t\t\t\t\t\t\tcase ");
					CSharp.Append(TYPE_CHAR);
					CSharp.AppendLine(":");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tFieldValue = ReadChar(Reader, FieldDataType);");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
					CSharp.AppendLine();
					CSharp.Append("\t\t\t\t\t\t\tcase ");
					CSharp.Append(TYPE_STRING);
					CSharp.AppendLine(":");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tFieldValue = ReadString(Reader, FieldDataType);");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
					CSharp.AppendLine();
					CSharp.Append("\t\t\t\t\t\t\tcase ");
					CSharp.Append(TYPE_CI_STRING);
					CSharp.AppendLine(":");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tFieldValue = ReadString(Reader, FieldDataType);");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
					CSharp.AppendLine();
					CSharp.Append("\t\t\t\t\t\t\tcase ");
					CSharp.Append(TYPE_BYTEARRAY);
					CSharp.AppendLine(":");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tFieldValue = ReadByteArray(Reader, FieldDataType);");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
					CSharp.AppendLine();
					CSharp.Append("\t\t\t\t\t\t\tcase ");
					CSharp.Append(TYPE_GUID);
					CSharp.AppendLine(":");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tFieldValue = ReadGuid(Reader, FieldDataType);");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
					CSharp.AppendLine();
					CSharp.Append("\t\t\t\t\t\t\tcase ");
					CSharp.Append(TYPE_ENUM);
					CSharp.AppendLine(":");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tFieldValue = ReadString(Reader, FieldDataType);");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
					CSharp.AppendLine();
					CSharp.Append("\t\t\t\t\t\t\tcase ");
					CSharp.Append(TYPE_ARRAY);
					CSharp.AppendLine(":");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tFieldValue = await ReadArray<Waher.Persistence.Serialization.GenericObject>(this.context, Reader, FieldDataType);");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tbreak;");
					CSharp.AppendLine();
					CSharp.AppendLine("\t\t\t\t\t\t\tdefault:");
					CSharp.AppendLine("\t\t\t\t\t\t\t\tthrow new SerializationException(\"Value expected for \" + FieldName + \". Data Type read: \" + ObjectSerializer.GetFieldDataTypeName(FieldDataType), this.ValueType);");
					CSharp.AppendLine("\t\t\t\t\t\t}");
					CSharp.AppendLine();
					CSharp.AppendLine("\t\t\t\t\t\tif (Obsolete is null)");
					CSharp.AppendLine("\t\t\t\t\t\t\tObsolete = new Dictionary<string, object>();");
					CSharp.AppendLine();
					CSharp.AppendLine("\t\t\t\t\t\tObsolete[FieldName] = FieldValue;");
					CSharp.AppendLine("\t\t\t\t\t\tbreak;");
					CSharp.AppendLine("\t\t\t\t}");
					CSharp.AppendLine("\t\t\t}");
					CSharp.AppendLine();

					if (!(this.obsoleteMethod is null))
					{
						CSharp.AppendLine("\t\t\tif (!(Obsolete is null))");
						CSharp.Append("\t\t\t\tResult.");
						CSharp.Append(this.obsoleteMethod.Name);
						CSharp.AppendLine("(Obsolete);");
						CSharp.AppendLine();
					}

					CSharp.AppendLine("\t\t\treturn Result;");
				}

				CSharp.AppendLine("\t\t}");
				CSharp.AppendLine();
				CSharp.AppendLine("\t\tpublic override async Task Serialize(ISerializer Writer, bool WriteTypeCode, bool Embedded, object UntypedValue, object State)");
				CSharp.AppendLine("\t\t{");
				CSharp.AppendLine("\t\t\tType T = UntypedValue?.GetType();");
				CSharp.Append("\t\t\tif (!(T is null) && T != typeof(");
				AppendType(this.type, CSharp);
				CSharp.AppendLine("))");
				CSharp.AppendLine("\t\t\t{");
				CSharp.AppendLine("\t\t\t\tIObjectSerializer Serializer = await this.context.GetObjectSerializer(T);");
				CSharp.AppendLine("\t\t\t\tawait Serializer.Serialize(Writer, WriteTypeCode, Embedded, UntypedValue, State);");
				CSharp.AppendLine("\t\t\t\treturn;");
				CSharp.AppendLine("\t\t\t}");
				CSharp.AppendLine();
				CSharp.Append("\t\t\t");
				AppendType(Type, CSharp);
				CSharp.Append(" Value = (");
				AppendType(Type, CSharp);
				CSharp.AppendLine(")UntypedValue;");
				CSharp.AppendLine("\t\t\tISerializer WriterBak = Writer;");
				CSharp.AppendLine();
				CSharp.AppendLine("\t\t\tif (!Embedded)");
				CSharp.AppendLine("\t\t\t\tWriter = Writer.CreateNew();");

				CSharp.AppendLine();

				if (this.isNullable)
				{
					CSharp.AppendLine("\t\t\tif (WriteTypeCode)");
					CSharp.AppendLine("\t\t\t{");
					CSharp.AppendLine("\t\t\t\tif (Value is null)");
					CSharp.AppendLine("\t\t\t\t{");
					CSharp.Append("\t\t\t\t\tWriter.WriteBits(");
					CSharp.Append(TYPE_NULL);
					CSharp.AppendLine(", 6);");
					CSharp.AppendLine("\t\t\t\t\treturn;");
					CSharp.AppendLine("\t\t\t\t}");
					CSharp.AppendLine("\t\t\t\telse");
					CSharp.Append("\t\t\t\t\tWriter.WriteBits(");
					CSharp.Append(TYPE_OBJECT);
					CSharp.AppendLine(", 6);");
					CSharp.AppendLine("\t\t\t}");
					CSharp.AppendLine("\t\t\telse if (Value is null)");
					CSharp.AppendLine("\t\t\t\tthrow new NullReferenceException(\"Value cannot be null.\");");
				}
				else
				{
					CSharp.AppendLine("\t\t\tif (WriteTypeCode)");
					CSharp.Append("\t\t\t\tWriter.WriteBits(");
					CSharp.Append(TYPE_OBJECT);
					CSharp.AppendLine(", 6);");
				}

				CSharp.AppendLine();

				if (HasObjectId)
				{
					CSharp.AppendLine("\t\t\tif (Embedded && Writer.BitOffset > 0)");
					CSharp.AppendLine("\t\t\t{");

					if (this.objectIdMemberType == typeof(Guid))
					{
						CSharp.Append("\t\t\t\tbool WriteObjectId = !Value.");
						CSharp.Append(this.objectIdMemberInfo.Name);
						CSharp.AppendLine(".Equals(Guid.Empty);");
					}
					else if (this.objectIdMemberType == typeof(string))
					{
						CSharp.Append("\t\t\t\tbool WriteObjectId = !string.IsNullOrEmpty(Value.");
						CSharp.Append(this.objectIdMemberInfo.Name);
						CSharp.AppendLine(");");
					}
					else if (this.objectIdMemberType == typeof(byte[]))
					{
						CSharp.Append("\t\t\t\tbool WriteObjectId = !(Value.");
						CSharp.Append(this.objectIdMemberInfo.Name);
						CSharp.AppendLine(" is null);");
					}

					CSharp.AppendLine("\t\t\t\tWriter.WriteBit(WriteObjectId);");
					CSharp.AppendLine("\t\t\t\tif (WriteObjectId)");

					if (this.objectIdMemberType == typeof(Guid))
					{
						CSharp.Append("\t\t\t\t\tWriter.Write(Value.");
						CSharp.Append(this.objectIdMemberInfo.Name);
						CSharp.AppendLine(");");
					}
					else
					{
						CSharp.Append("\t\t\t\t\tWriter.Write(new Guid(Value.");
						CSharp.Append(this.objectIdMemberInfo.Name);
						CSharp.AppendLine("));");
					}

					CSharp.AppendLine("\t\t\t}");
					CSharp.AppendLine();
				}

				if (this.typeNameSerialization == TypeNameSerialization.None)
					CSharp.AppendLine("\t\t\tWriter.WriteVariableLengthUInt64(0);");    // Same as Writer.Write("") for non-normalized case.
				else
				{
					if (this.normalized)
					{
						CSharp.Append("\t\t\tWriter.WriteVariableLengthUInt64(");

						if (this.typeNameSerialization == TypeNameSerialization.LocalName)
							CSharp.Append(await this.context.GetFieldCode(this.collectionName, this.type.Name));
						else
							CSharp.Append(await this.context.GetFieldCode(this.collectionName, this.type.FullName));

						CSharp.AppendLine(");");
					}
					else
					{
						CSharp.Append("\t\t\tWriter.Write(\"");

						if (this.typeNameSerialization == TypeNameSerialization.LocalName)
							CSharp.Append(this.type.Name);
						else
							CSharp.Append(this.type.FullName);

						CSharp.AppendLine("\");");
					}
				}

				CSharp.AppendLine("\t\t\tif (Embedded)");

				if (this.normalized)
				{
					CSharp.Append("\t\t\t\tWriter.WriteVariableLengthUInt64(");
					CSharp.Append(await this.context.GetFieldCode(null, string.IsNullOrEmpty(this.collectionName) ? this.context.DefaultCollectionName : this.collectionName));
					CSharp.AppendLine(");");
				}
				else
				{
					CSharp.Append("\t\t\t\tWriter.Write(\"");
					CSharp.Append(string.IsNullOrEmpty(this.collectionName) ? this.context.DefaultCollectionName : this.collectionName);
					CSharp.AppendLine("\");");
				}

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
					ShortName = null;
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

					foreach (object Attr in Member.GetCustomAttributes(true))
					{
						if (Attr is IgnoreMemberAttribute)
						{
							Ignore = true;
							break;
						}
						else if (Attr is DefaultValueAttribute DefaultValueAttribute)
						{
							if (!IndexFields.ContainsKey(Member.Name))
							{
								HasDefaultValue = true;
								DefaultValue = DefaultValueAttribute.Value;
							}
						}
						else if (Attr is ShortNameAttribute ShortNameAttribute)
							ShortName = ShortNameAttribute.Name;
						else if (Attr is ObjectIdAttribute)
							ObjectIdField = true;
						else if (Attr is ByReferenceAttribute)
							ByReference = true;
					}

					if (Ignore)
						continue;

					CSharp.AppendLine();

					if (ObjectIdField)
					{
						CSharp.Append("\t\t\t");
						AppendType(MemberType, CSharp);
						CSharp.Append(" ObjectId = Value.");
						CSharp.Append(Member.Name);
						CSharp.AppendLine(";");
					}
					else
					{
						if (HasDefaultValue)
						{
							if (DefaultValue is null)
							{
								CSharp.Append("\t\t\tif (!((object)Value.");
								CSharp.Append(Member.Name);
								CSharp.AppendLine(" is null))");
							}
							else if (MemberType == typeof(string) && DefaultValue is string s2 && string.IsNullOrEmpty(s2))
							{
								CSharp.Append("\t\t\tif (!string.IsNullOrEmpty(Value.");
								CSharp.Append(Member.Name);
								CSharp.AppendLine("))");
							}
							else if (MemberType == typeof(CaseInsensitiveString) && DefaultValue is CaseInsensitiveString s3 && CaseInsensitiveString.IsNullOrEmpty(s3))
							{
								CSharp.Append("\t\t\tif (!CaseInsensitiveString.IsNullOrEmpty(Value.");
								CSharp.Append(Member.Name);
								CSharp.AppendLine("))");
							}
							else
							{
								CSharp.Append("\t\t\tif (!default");
								CSharp.Append(Member.Name);
								CSharp.Append(".Equals(Value.");
								CSharp.Append(Member.Name);
								CSharp.AppendLine("))");
							}

							CSharp.AppendLine("\t\t\t{");
							Indent = "\t\t\t\t";
						}
						else
							Indent = "\t\t\t";

						CSharp.Append(Indent);

						if (this.normalized)
						{
							CSharp.Append("Writer.WriteVariableLengthUInt64(");
							CSharp.Append(await this.context.GetFieldCode(this.collectionName, Member.Name));
							CSharp.AppendLine(");");
						}
						else
						{
							CSharp.Append("Writer.Write(\"");

							if (string.IsNullOrEmpty(ShortName))
								CSharp.Append(Member.Name);
							else
								CSharp.Append(ShortName);

							CSharp.AppendLine("\");");
						}

						if (Nullable)
						{
							Indent2 = Indent + "\t";

							CSharp.Append(Indent);
							CSharp.Append("if (!Value.");
							CSharp.Append(Member.Name);
							CSharp.AppendLine(".HasValue)");
							CSharp.Append(Indent2);
							CSharp.Append("Writer.WriteBits(");
							CSharp.Append(TYPE_NULL);
							CSharp.AppendLine(", 6);");
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

								case TypeCode.Int16:
									CSharp.Append(Indent2);
									CSharp.Append("if (Value.");
									CSharp.Append(Member.Name);
									if (Nullable)
										CSharp.Append(".Value");
									CSharp.Append(" > Int16VarSizeMinLimit && Value.");
									CSharp.Append(Member.Name);
									if (Nullable)
										CSharp.Append(".Value");
									CSharp.AppendLine(" < Int16VarSizeMaxLimit)");
									CSharp.Append(Indent2);
									CSharp.AppendLine("{");

									CSharp.Append(Indent2);
									CSharp.Append("\tWriter.WriteBits(");
									CSharp.Append(TYPE_VARINT16);
									CSharp.AppendLine(", 6);");

									CSharp.Append(Indent2);
									CSharp.Append("\tWriter.WriteVariableLengthInt16(Value.");
									CSharp.Append(Member.Name);
									if (Nullable)
										CSharp.Append(".Value");
									CSharp.AppendLine(");");

									CSharp.Append(Indent2);
									CSharp.AppendLine("}");
									CSharp.Append(Indent2);
									CSharp.AppendLine("else");
									CSharp.Append(Indent2);
									CSharp.AppendLine("{");

									CSharp.Append(Indent2);
									CSharp.Append("\tWriter.WriteBits(");
									CSharp.Append(TYPE_INT16);
									CSharp.AppendLine(", 6);");

									CSharp.Append(Indent2);
									CSharp.Append("\tWriter.Write(Value.");
									CSharp.Append(Member.Name);
									if (Nullable)
										CSharp.Append(".Value");
									CSharp.AppendLine(");");

									CSharp.Append(Indent2);
									CSharp.AppendLine("}");
									break;

								case TypeCode.Int32:
									CSharp.Append(Indent2);
									CSharp.Append("if (Value.");
									CSharp.Append(Member.Name);
									if (Nullable)
										CSharp.Append(".Value");
									CSharp.Append(" > Int32VarSizeMinLimit && Value.");
									CSharp.Append(Member.Name);
									if (Nullable)
										CSharp.Append(".Value");
									CSharp.AppendLine(" < Int32VarSizeMaxLimit)");
									CSharp.Append(Indent2);
									CSharp.AppendLine("{");

									CSharp.Append(Indent2);
									CSharp.Append("\tWriter.WriteBits(");
									CSharp.Append(TYPE_VARINT32);
									CSharp.AppendLine(", 6);");

									CSharp.Append(Indent2);
									CSharp.Append("\tWriter.WriteVariableLengthInt32(Value.");
									CSharp.Append(Member.Name);
									if (Nullable)
										CSharp.Append(".Value");
									CSharp.AppendLine(");");

									CSharp.Append(Indent2);
									CSharp.AppendLine("}");
									CSharp.Append(Indent2);
									CSharp.AppendLine("else");
									CSharp.Append(Indent2);
									CSharp.AppendLine("{");

									CSharp.Append(Indent2);
									CSharp.Append("\tWriter.WriteBits(");
									CSharp.Append(TYPE_INT32);
									CSharp.AppendLine(", 6);");

									CSharp.Append(Indent2);
									CSharp.Append("\tWriter.Write(Value.");
									CSharp.Append(Member.Name);
									if (Nullable)
										CSharp.Append(".Value");
									CSharp.AppendLine(");");

									CSharp.Append(Indent2);
									CSharp.AppendLine("}");
									break;

								case TypeCode.Int64:
									CSharp.Append(Indent2);
									CSharp.Append("if (Value.");
									CSharp.Append(Member.Name);
									if (Nullable)
										CSharp.Append(".Value");
									CSharp.Append(" > Int64VarSizeMinLimit && Value.");
									CSharp.Append(Member.Name);
									if (Nullable)
										CSharp.Append(".Value");
									CSharp.AppendLine(" < Int64VarSizeMaxLimit)");
									CSharp.Append(Indent2);
									CSharp.AppendLine("{");

									CSharp.Append(Indent2);
									CSharp.Append("\tWriter.WriteBits(");
									CSharp.Append(TYPE_VARINT64);
									CSharp.AppendLine(", 6);");

									CSharp.Append(Indent2);
									CSharp.Append("\tWriter.WriteVariableLengthInt64(Value.");
									CSharp.Append(Member.Name);
									if (Nullable)
										CSharp.Append(".Value");
									CSharp.AppendLine(");");

									CSharp.Append(Indent2);
									CSharp.AppendLine("}");
									CSharp.Append(Indent2);
									CSharp.AppendLine("else");
									CSharp.Append(Indent2);
									CSharp.AppendLine("{");

									CSharp.Append(Indent2);
									CSharp.Append("\tWriter.WriteBits(");
									CSharp.Append(TYPE_INT64);
									CSharp.AppendLine(", 6);");

									CSharp.Append(Indent2);
									CSharp.Append("\tWriter.Write(Value.");
									CSharp.Append(Member.Name);
									if (Nullable)
										CSharp.Append(".Value");
									CSharp.AppendLine(");");

									CSharp.Append(Indent2);
									CSharp.AppendLine("}");
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

								case TypeCode.UInt16:
									CSharp.Append(Indent2);
									CSharp.Append("if (Value.");
									CSharp.Append(Member.Name);
									if (Nullable)
										CSharp.Append(".Value");
									CSharp.AppendLine(" < UInt16VarSizeLimit)");
									CSharp.Append(Indent2);
									CSharp.AppendLine("{");

									CSharp.Append(Indent2);
									CSharp.Append("\tWriter.WriteBits(");
									CSharp.Append(TYPE_VARUINT16);
									CSharp.AppendLine(", 6);");

									CSharp.Append(Indent2);
									CSharp.Append("\tWriter.WriteVariableLengthUInt16(Value.");
									CSharp.Append(Member.Name);
									if (Nullable)
										CSharp.Append(".Value");
									CSharp.AppendLine(");");

									CSharp.Append(Indent2);
									CSharp.AppendLine("}");
									CSharp.Append(Indent2);
									CSharp.AppendLine("else");
									CSharp.Append(Indent2);
									CSharp.AppendLine("{");

									CSharp.Append(Indent2);
									CSharp.Append("\tWriter.WriteBits(");
									CSharp.Append(TYPE_UINT16);
									CSharp.AppendLine(", 6);");

									CSharp.Append(Indent2);
									CSharp.Append("\tWriter.Write(Value.");
									CSharp.Append(Member.Name);
									if (Nullable)
										CSharp.Append(".Value");
									CSharp.AppendLine(");");

									CSharp.Append(Indent2);
									CSharp.AppendLine("}");
									break;

								case TypeCode.UInt32:
									CSharp.Append(Indent2);
									CSharp.Append("if (Value.");
									CSharp.Append(Member.Name);
									if (Nullable)
										CSharp.Append(".Value");
									CSharp.AppendLine(" < UInt32VarSizeLimit)");
									CSharp.Append(Indent2);
									CSharp.AppendLine("{");

									CSharp.Append(Indent2);
									CSharp.Append("\tWriter.WriteBits(");
									CSharp.Append(TYPE_VARUINT32);
									CSharp.AppendLine(", 6);");

									CSharp.Append(Indent2);
									CSharp.Append("\tWriter.WriteVariableLengthUInt32(Value.");
									CSharp.Append(Member.Name);
									if (Nullable)
										CSharp.Append(".Value");
									CSharp.AppendLine(");");

									CSharp.Append(Indent2);
									CSharp.AppendLine("}");
									CSharp.Append(Indent2);
									CSharp.AppendLine("else");
									CSharp.Append(Indent2);
									CSharp.AppendLine("{");

									CSharp.Append(Indent2);
									CSharp.Append("\tWriter.WriteBits(");
									CSharp.Append(TYPE_UINT32);
									CSharp.AppendLine(", 6);");

									CSharp.Append(Indent2);
									CSharp.Append("\tWriter.Write(Value.");
									CSharp.Append(Member.Name);
									if (Nullable)
										CSharp.Append(".Value");
									CSharp.AppendLine(");");

									CSharp.Append(Indent2);
									CSharp.AppendLine("}");
									break;

								case TypeCode.UInt64:
									CSharp.Append(Indent2);
									CSharp.Append("if (Value.");
									CSharp.Append(Member.Name);
									if (Nullable)
										CSharp.Append(".Value");
									CSharp.AppendLine(" < UInt64VarSizeLimit)");
									CSharp.Append(Indent2);
									CSharp.AppendLine("{");

									CSharp.Append(Indent2);
									CSharp.Append("\tWriter.WriteBits(");
									CSharp.Append(TYPE_VARUINT64);
									CSharp.AppendLine(", 6);");

									CSharp.Append(Indent2);
									CSharp.Append("\tWriter.WriteVariableLengthUInt64(Value.");
									CSharp.Append(Member.Name);
									if (Nullable)
										CSharp.Append(".Value");
									CSharp.AppendLine(");");

									CSharp.Append(Indent2);
									CSharp.AppendLine("}");
									CSharp.Append(Indent2);
									CSharp.AppendLine("else");
									CSharp.Append(Indent2);
									CSharp.AppendLine("{");
									
									CSharp.Append(Indent2);
									CSharp.Append("\tWriter.WriteBits(");
									CSharp.Append(TYPE_UINT64);
									CSharp.AppendLine(", 6);");

									CSharp.Append(Indent2);
									CSharp.Append("\tWriter.Write(Value.");
									CSharp.Append(Member.Name);
									if (Nullable)
										CSharp.Append(".Value");
									CSharp.AppendLine(");");
									
									CSharp.Append(Indent2);
									CSharp.AppendLine("}");
									break;

								case TypeCode.Empty:
									CSharp.Append(Indent2);
									CSharp.Append("Writer.WriteBits(");
									CSharp.Append(TYPE_NULL);
									CSharp.AppendLine(", 6);");
									break;

								default:
									sb.Clear();

									sb.Append("Invalid member type: ");
									AppendType(MemberType, CSharp);

									throw new SerializationException(sb.ToString(), this.type);

								case TypeCode.Object:
									if (MemberType.IsArray)
									{
										if (MemberType == typeof(byte[]))
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
											CSharp.Append(TYPE_BYTEARRAY);
											CSharp.AppendLine(", 6);");

											CSharp.Append(Indent2);
											CSharp.Append("\tWriter.Write(Value.");
											CSharp.Append(Member.Name);
											CSharp.AppendLine(");");
											CSharp.AppendLine("}");
										}
										else
										{
											MemberType = MemberType.GetElementType();

											CSharp.Append(Indent2);

											if (MemberType == typeof(KeyValuePair<string, object>))
												CSharp.Append("await WriteTagArray");
											else if (MemberType == typeof(KeyValuePair<string, IElement>))
												CSharp.Append("await WriteTagElementArray");
											else
											{
												CSharp.Append("await WriteArray<");
												AppendType(MemberType, CSharp);
												CSharp.Append(">");
											}

											CSharp.Append("(this.context, Writer, Value.");
											CSharp.Append(Member.Name);
											CSharp.AppendLine(", State);");
										}
									}
									else if (ByReference)
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
										CSharp.Append(TYPE_GUID);
										CSharp.AppendLine(", 6);");
										CSharp.Append(Indent2);
										CSharp.Append("\tObjectSerializer Serializer");
										CSharp.Append(Member.Name);
										CSharp.Append(" = (ObjectSerializer)await this.context.GetObjectSerializer(typeof(");
										AppendType(MemberType, CSharp);
										CSharp.AppendLine("));");
										CSharp.Append(Indent2);
										CSharp.Append("\tGuid ");
										CSharp.Append(Member.Name);
										CSharp.Append("Id = await Serializer");
										CSharp.Append(Member.Name);
										CSharp.Append(".GetObjectId(Value.");
										CSharp.Append(Member.Name);
										CSharp.AppendLine(", true, State);");
										CSharp.Append(Indent2);
										CSharp.Append("\tWriter.Write(");
										CSharp.Append(Member.Name);
										CSharp.AppendLine("Id);");
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
										CSharp.Append("await this.serializer");
										CSharp.Append(Member.Name);
										CSharp.Append(".Serialize(Writer, true, true, Value.");
										CSharp.Append(Member.Name);
										if (Nullable)
											CSharp.Append(".Value");
										CSharp.AppendLine(", State);");
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
				CSharp.AppendLine("\t\t\tWriter.WriteVariableLengthUInt64(0);");    // Same as Writer.Write("") for non-normalized case.

				CSharp.AppendLine();
				CSharp.AppendLine("\t\t\tif (!Embedded)");
				CSharp.AppendLine("\t\t\t{");

				if (this.objectIdMemberType is null)
					CSharp.AppendLine("\t\t\t\tWriterBak.Write(this.context.CreateGuid());");
				else
				{
					if (this.objectIdMemberType == typeof(Guid))
					{
						CSharp.AppendLine("\t\t\t\tif (!ObjectId.Equals(Guid.Empty))");
						CSharp.AppendLine("\t\t\t\t\tWriterBak.Write(ObjectId);");
					}
					else if (this.objectIdMemberType == typeof(string))
					{
						CSharp.AppendLine("\t\t\t\tif (!string.IsNullOrEmpty(ObjectId))");
						CSharp.AppendLine("\t\t\t\t\tWriterBak.Write(new Guid(ObjectId));");
					}
					else if (this.objectIdMemberType == typeof(byte[]))
					{
						CSharp.AppendLine("\t\t\t\tif (!(ObjectId is null))");
						CSharp.AppendLine("\t\t\t\t\tWriterBak.Write(new Guid(ObjectId));");
					}
					else
						throw new SerializationException("Invalid Object ID type.", this.type);

					CSharp.AppendLine("\t\t\t\telse");
					CSharp.AppendLine("\t\t\t\t{");
					CSharp.AppendLine("\t\t\t\t\tGuid NewObjectId = this.context.CreateGuid();");
					CSharp.AppendLine("\t\t\t\t\tWriterBak.Write(NewObjectId);");

					if (this.objectIdMemberType == typeof(Guid))
					{
						CSharp.Append("\t\t\t\t\tValue.");
						CSharp.Append(this.objectIdMemberInfo.Name);
						CSharp.AppendLine(" = NewObjectId;");
					}
					else if (this.objectIdMemberType == typeof(string))
					{
						CSharp.Append("\t\t\t\t\tValue.");
						CSharp.Append(this.objectIdMemberInfo.Name);
						CSharp.AppendLine(" = NewObjectId.ToString();");
					}
					else if (this.objectIdMemberType == typeof(byte[]))
					{
						CSharp.Append("\t\t\t\t\tValue.");
						CSharp.Append(this.objectIdMemberInfo.Name);
						CSharp.AppendLine(" = NewObjectId.ToByteArray();");
					}

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
				CSharp.AppendLine("\t\tpublic override Task<object> TryGetFieldValue(string FieldName, object UntypedObject)");
				CSharp.AppendLine("\t\t{");
				CSharp.Append("\t\t\t");
				AppendType(Type, CSharp);
				CSharp.Append(" Object = (");
				AppendType(Type, CSharp);
				CSharp.AppendLine(")UntypedObject;");
				CSharp.AppendLine();
				CSharp.AppendLine("\t\t\tswitch (FieldName)");
				CSharp.AppendLine("\t\t\t{");

				foreach (string MemberName in this.members.Keys)
				{
					CSharp.Append("\t\t\t\tcase \"");
					CSharp.Append(MemberName);
					CSharp.AppendLine("\":");

					if (ShortNames.TryGetValue(MemberName, out ShortName))
					{
						CSharp.Append("\t\t\t\tcase \"");
						CSharp.Append(ShortName);
						CSharp.AppendLine("\":");
					}

					CSharp.Append("\t\t\t\t\treturn Task.FromResult<object>(Object.");
					CSharp.Append(MemberName);
					CSharp.AppendLine(");");
					CSharp.AppendLine();
				}

				CSharp.AppendLine("\t\t\t\tdefault:");
				CSharp.AppendLine("\t\t\t\t\treturn Task.FromResult<object>(null);");
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
					{ Path.Combine(Path.GetDirectoryName(GetLocation(typeof(Dictionary<string, object>))), "System.Collections.dll"), true },
					{ GetLocation(typeof(Types)), true },
					{ GetLocation(typeof(Database)), true },
					{ GetLocation(typeof(ObjectSerializer)), true },
					{ GetLocation(typeof(MultiReadSingleWriteObject)), true },
					{ GetLocation(typeof(IElement)), true }
				};

				foreach (Type T in MemberTypes.Keys)
					Dependencies[GetLocation(T)] = true;

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

				sb.Clear();

				sb.Append("WPSA.");
				AppendType(this.type, sb);
				sb.Append(this.context.Id);

				CSharpCompilation Compilation = CSharpCompilation.Create(sb.ToString(),
					new SyntaxTree[] { CSharpSyntaxTree.ParseText(CSharpCode) },
					References, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

				MemoryStream Output = new MemoryStream();
				MemoryStream PdbOutput = new MemoryStream();

				EmitResult CompilerResults = Compilation.Emit(Output, pdbStream: PdbOutput);

				if (!CompilerResults.Success)
				{
					sb.Clear();

					sb.Append("Unable to serialize objects of type ");
					AppendType(Type, sb);
					sb.AppendLine(". When generating serialization class, the following compiler errors were reported:");

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

					throw new SerializationException(sb.ToString(), this.type);
				}
				Output.Position = 0;
				Assembly A;

				try
				{
					A = AssemblyLoadContext.Default.LoadFromStream(Output, PdbOutput);

					sb.Clear();
					sb.Append(Type.Namespace);
					sb.Append(".Binary.Serializer");
					sb.Append(TypeName);
					sb.Append(this.context.Id);

					Type T = A.GetType(sb.ToString());
					this.customSerializer = (IObjectSerializer)Activator.CreateInstance(T, this.context);
				}
				catch (FileLoadException ex)
				{
					this.customSerializer = await this.context.GetObjectSerializer(Type); // Created in other thread?
					if (this.customSerializer is null)
					{
						sb.Clear();
						AppendType(Type, sb);

						Log.Warning(ex.Message, sb.ToString());
						this.customSerializer = await Create(Type, Context, false);
					}
				}
			}
			else
			{
#endif
				Member Member;
				MethodInfo MI;
				object DefaultValue;
				string ShortName;
				int NrDefault = 0;
				bool Ignore;

				foreach (MemberInfo MemberInfo in GetMembers(this.typeInfo))
				{
					if (MemberInfo is FieldInfo FI)
					{
						if (!FI.IsPublic || FI.IsStatic)
							continue;

						Member = new FieldMember(FI, this.normalized ? await this.context.GetFieldCode(this.collectionName, FI.Name) : 0);
					}
					else if (MemberInfo is PropertyInfo PI)
					{
						if ((MI = PI.GetMethod) is null || !MI.IsPublic || MI.IsStatic)
							continue;

						if ((MI = PI.SetMethod) is null || !MI.IsPublic || MI.IsStatic)
							continue;

						if (PI.GetIndexParameters().Length > 0)
							continue;

						Member = new PropertyMember(PI, this.normalized ? await this.context.GetFieldCode(this.collectionName, PI.Name) : 0);
					}
					else
						continue;

					Ignore = false;
					ShortName = null;

					foreach (object Attr in MemberInfo.GetCustomAttributes(true))
					{
						if (Attr is IgnoreMemberAttribute)
						{
							Ignore = true;
							break;
						}
						else if (Attr is DefaultValueAttribute DefaultValueAttribute)
						{
							if (!IndexFields.ContainsKey(MemberInfo.Name))
							{
								DefaultValue = DefaultValueAttribute.Value;
								NrDefault++;

								if (DefaultValue is string s && Member.MemberType == typeof(CaseInsensitiveString))
									DefaultValue = (CaseInsensitiveString)s;

								if (!(DefaultValue is null) && DefaultValue.GetType() != Member.MemberType)
									DefaultValue = Convert.ChangeType(DefaultValue, Member.MemberType);

								Member.DefaultValue = DefaultValue;
							}
						}
						else if (Attr is ByReferenceAttribute)
						{
							Member.ByReference = true;
							this.hasByRef = true;
						}
						else if (Attr is ObjectIdAttribute)
						{
							this.objectIdMember = Member;
							Ignore = true;
						}
						else if (Attr is ShortNameAttribute ShortNameAttribute)
							ShortName = ShortNameAttribute.Name;
					}

					if (Ignore)
						continue;

					this.membersByName[Member.Name] = Member;
					this.membersOrdered.AddLast(Member);

					if (this.normalized)
						this.membersByFieldCode[Member.FieldCode] = Member;

					if (!string.IsNullOrEmpty(ShortName))
						this.membersByName[ShortName] = Member;
				}
#if NETSTANDARD2_0
			}
#endif
			this.prepared = true;

			foreach (Member Member2 in this.membersOrdered)
			{
				if (GetFieldDataTypeCode(Member2.MemberType) == TYPE_OBJECT)
				{
					Member2.NestedSerializer = await this.context.GetObjectSerializer(Member2.MemberType);
					if (Member2.NestedSerializer is ObjectSerializer Nested2 && Nested2.HasByReference)
						this.hasByRef = true;
				}
			}
		}

		/// <summary>
		/// If the serializer is successfully prepared, and can be used.
		/// </summary>
		public bool Prepared
		{
			get => this.prepared;
			protected set => this.prepared = value;
		}

#if NETSTANDARD2_0
		private static string GetLocation(Type T)
		{
			System.Reflection.TypeInfo TI = T.GetTypeInfo();
			string s = TI.Assembly.Location;

			if (!string.IsNullOrEmpty(s))
				return s;

			return Path.Combine(Path.GetDirectoryName(GetLocation(typeof(Database))), TI.Module.ScopeName);
		}
#endif

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
				Replace("\v", "\\v").
				Replace("\"", "\\\"");
		}

		private static readonly char[] specialCharacters = new char[] { '\\', '\n', '\r', '\t', '\f', '\b', '\a', '\v', '"' };

		/// <summary>
		/// Name of collection objects of this type is to be stored in, if available. If not available, this property returns null.
		/// </summary>
		/// <param name="Object">Object in the current context. If null, the default collection name is requested.</param>
		/// <returns>Collection name.</returns>
		public virtual Task<string> CollectionName(object Object)
		{
			return Task.FromResult<string>(this.collectionName);
		}

		/// <summary>
		/// Internal reference to constant collection name.
		/// </summary>
		internal string CollectionNameConstant => this.collectionName;

		/// <summary>
		/// Gets the type of the value.
		/// </summary>
		public Type ValueType => this.type;

		/// <summary>
		/// Array of indices defined for the underlying type.
		/// </summary>
		public string[][] Indices => this.indices;

		/// <summary>
		/// If the type is nullable.
		/// </summary>
		public bool IsNullable => this.isNullable;

		/// <summary>
		/// If objects serialized by the serializer include subobjects by reference.
		/// </summary>
		public bool HasByReference => this.hasByRef;

		/// <summary>
		/// Property that can be used by the creator to associate the serializer with data.
		/// </summary>
		public object Tag
		{
			get => this.tag;
			set => this.tag = value;
		}

		/// <summary>
		/// If the corresponding collection should be backed up or not.
		/// </summary>
		public bool BackupCollection => this.backupCollection;

		/// <summary>
		/// A reason for not backing up the corresponding collection.
		/// </summary>
		public string NoBackupReason => this.noBackupReason;

		/// <summary>
		/// Number of days to archive objects of this type. If equal to <see cref="int.MaxValue"/>, no limit is defined.
		/// </summary>
		public virtual int GetArchivingTimeDays(object Object)
		{
			if (!this.archive)
				return 0;

			if (!(this.archiveProperty is null))
				return (int)this.archiveProperty.GetValue(Object);

			if (!(this.archiveField is null))
				return (int)this.archiveField.GetValue(Object);

			return this.archiveDays;
		}

		/// <summary>
		/// If objects of this type can be archived.
		/// </summary>
		public bool ArchiveObjects
		{
			get => this.archive;
			internal set => this.archive = value;
		}

		/// <summary>
		/// If each object contains the information for how long time it can be archived.
		/// </summary>
		public bool ArchiveTimeDynamic
		{
			get => this.archiveDynamic;
			internal set => this.archiveDynamic = value;
		}

		/// <summary>
		/// If names are normalized or not.
		/// </summary>
		public bool NormalizedNames => this.normalized;

#if NETSTANDARD2_0
		/// <summary>
		/// Initializes the serializer before first-time use.
		/// </summary>
		public async Task Init()
		{
			if (this.compiled)
				await this.customSerializer.Init();
		}
#else
		/// <summary>
		/// Initializes the serializer before first-time use.
		/// </summary>
		public Task Init()
		{
			return Task.CompletedTask;
		}
#endif

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <param name="Reader">Deserializer.</param>
		/// <param name="DataType">Data type of object.</param>
		/// <param name="Embedded">If the object is embedded in another object.</param>
		/// <returns>A deserialized value.</returns>
		public virtual async Task<object> Deserialize(IDeserializer Reader, uint? DataType, bool Embedded)
		{
#if NETSTANDARD2_0
			if (this.compiled)
				return await this.customSerializer.Deserialize(Reader, DataType, Embedded);
			else
			{
#endif
				uint FieldDataType;
				ulong FieldCode;
				string FieldName;
				object Result;
				Dictionary<string, object> Obsolete = null;
				StreamBookmark Bookmark = Reader.GetBookmark();
				uint? DataTypeBak = DataType;
				Guid ObjectId = Embedded ? Guid.Empty : Reader.ReadGuid();
				ulong ContentLen = Embedded ? 0 : Reader.ReadVariableLengthUInt64();
				Member Member;

				if (!DataType.HasValue)
				{
					DataType = Reader.ReadBits(6);
					if (DataType.Value == TYPE_NULL)
						return null;
				}

				if (Embedded && Reader.BitOffset > 0 && Reader.ReadBit())
					ObjectId = Reader.ReadGuid();

				if (this.normalized)
				{
					FieldCode = Reader.ReadVariableLengthUInt64();
					FieldName = null;
				}
				else
				{
					FieldCode = 0;
					FieldName = Reader.ReadString();
				}

				if (this.typeNameSerialization != TypeNameSerialization.None)
				{
					string TypeName;

					if (this.normalized)
						TypeName = await this.context.GetFieldName(this.collectionName, FieldCode);
					else
						TypeName = FieldName;

					if (this.typeNameSerialization == TypeNameSerialization.LocalName && TypeName.IndexOf('.') < 0)
						TypeName = this.type.Namespace + "." + TypeName;

					Type DesiredType = Types.GetType(TypeName) ?? typeof(GenericObject);

					if (DesiredType != this.type)
					{
						IObjectSerializer Serializer2 = await this.context.GetObjectSerializer(DesiredType);
						Reader.SetBookmark(Bookmark);
						return await Serializer2.Deserialize(Reader, DataTypeBak, Embedded);
					}
				}

				if (this.typeInfo.IsAbstract)
				{
					StringBuilder sb = new StringBuilder();

					sb.Append("Unable to create an instance of the abstract class ");
					AppendType(this.type, sb);
					sb.Append(".");

					throw new SerializationException(sb.ToString(), this.type);
				}

				if (Embedded)
				{
					if (this.normalized)
						Reader.SkipVariableLengthInteger();  // Collection name
					else
						Reader.SkipString();
				}

				if (DataType.Value != TYPE_OBJECT)
					throw new SerializationException("Object expected.", this.type);

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
							StringBuilder sb = new StringBuilder();

							sb.Append("Type not supported for Object ID fields: ");
							AppendType(this.objectIdMember.MemberType, sb);

							throw new SerializationException(sb.ToString(), this.type);
					}
				}

				while (true)
				{
					if (this.normalized)
					{
						FieldCode = Reader.ReadVariableLengthUInt64();
						if (FieldCode == 0)
							break;
					}
					else
					{
						FieldName = Reader.ReadString();
						if (string.IsNullOrEmpty(FieldName))
							break;
					}

					FieldDataType = Reader.ReadBits(6);

					if (this.normalized)
					{
						if (!this.membersByFieldCode.TryGetValue(FieldCode, out Member))
							Member = null;
					}
					else
					{
						if (!this.membersByName.TryGetValue(FieldName, out Member))
							Member = null;
					}

					if (Member is null)
					{
						if (this.normalized)
							FieldName = await this.context.GetFieldName(this.collectionName, FieldCode);

						object FieldValue;

						switch (FieldDataType)
						{
							case TYPE_OBJECT:
								FieldValue = await GeneratedObjectSerializerBase.ReadEmbeddedObject(this.context, Reader, FieldDataType);
								break;

							case TYPE_NULL:
								FieldValue = null;
								break;

							case TYPE_BOOLEAN:
								FieldValue = GeneratedObjectSerializerBase.ReadBoolean(Reader, FieldDataType);
								break;

							case TYPE_BYTE:
								FieldValue = GeneratedObjectSerializerBase.ReadByte(Reader, FieldDataType);
								break;

							case TYPE_INT16:
							case TYPE_VARINT16:
								FieldValue = GeneratedObjectSerializerBase.ReadInt16(Reader, FieldDataType);
								break;

							case TYPE_INT32:
							case TYPE_VARINT32:
								FieldValue = GeneratedObjectSerializerBase.ReadInt32(Reader, FieldDataType);
								break;

							case TYPE_INT64:
							case TYPE_VARINT64:
								FieldValue = GeneratedObjectSerializerBase.ReadInt64(Reader, FieldDataType);
								break;

							case TYPE_SBYTE:
								FieldValue = GeneratedObjectSerializerBase.ReadSByte(Reader, FieldDataType);
								break;

							case TYPE_UINT16:
							case TYPE_VARUINT16:
								FieldValue = GeneratedObjectSerializerBase.ReadUInt16(Reader, FieldDataType);
								break;

							case TYPE_UINT32:
							case TYPE_VARUINT32:
								FieldValue = GeneratedObjectSerializerBase.ReadUInt32(Reader, FieldDataType);
								break;

							case TYPE_UINT64:
							case TYPE_VARUINT64:
								FieldValue = GeneratedObjectSerializerBase.ReadUInt64(Reader, FieldDataType);
								break;

							case TYPE_DECIMAL:
								FieldValue = GeneratedObjectSerializerBase.ReadDecimal(Reader, FieldDataType);
								break;

							case TYPE_DOUBLE:
								FieldValue = GeneratedObjectSerializerBase.ReadDouble(Reader, FieldDataType);
								break;

							case TYPE_SINGLE:
								FieldValue = GeneratedObjectSerializerBase.ReadSingle(Reader, FieldDataType);
								break;

							case TYPE_DATETIME:
								FieldValue = GeneratedObjectSerializerBase.ReadDateTime(Reader, FieldDataType);
								break;

							case TYPE_DATETIMEOFFSET:
								FieldValue = GeneratedObjectSerializerBase.ReadDateTimeOffset(Reader, FieldDataType);
								break;

							case TYPE_TIMESPAN:
								FieldValue = GeneratedObjectSerializerBase.ReadTimeSpan(Reader, FieldDataType);
								break;

							case TYPE_CHAR:
								FieldValue = GeneratedObjectSerializerBase.ReadChar(Reader, FieldDataType);
								break;

							case TYPE_STRING:
								FieldValue = GeneratedObjectSerializerBase.ReadString(Reader, FieldDataType);
								break;

							case TYPE_CI_STRING:
								FieldValue = GeneratedObjectSerializerBase.ReadCaseInsensitiveString(Reader, FieldDataType);
								break;

							case TYPE_BYTEARRAY:
								FieldValue = GeneratedObjectSerializerBase.ReadByteArray(Reader, FieldDataType);
								break;

							case TYPE_GUID:
								FieldValue = GeneratedObjectSerializerBase.ReadGuid(Reader, FieldDataType);
								break;

							case TYPE_ENUM:
								FieldValue = GeneratedObjectSerializerBase.ReadString(Reader, FieldDataType);
								break;

							case TYPE_ARRAY:
								FieldValue = await GeneratedObjectSerializerBase.ReadArray<GenericObject>(this.context, Reader, FieldDataType);
								break;

							default:
								StringBuilder sb = new StringBuilder();

								sb.Append("Value expected for ");
								sb.Append(FieldName);
								sb.Append(". Data Type read: ");
								sb.Append(GetFieldDataTypeName(FieldDataType));

								throw new SerializationException(sb.ToString(), this.type);
						}

						if (Obsolete is null)
							Obsolete = new Dictionary<string, object>();

						Obsolete[FieldName] = FieldValue;
					}
					else
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
							case TYPE_VARINT16:
								if (Member.Nullable)
									Member.Set(Result, GeneratedObjectSerializerBase.ReadNullableInt16(Reader, FieldDataType));
								else
									Member.Set(Result, GeneratedObjectSerializerBase.ReadInt16(Reader, FieldDataType));
								break;

							case TYPE_INT32:
							case TYPE_VARINT32:
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
							case TYPE_VARINT64:
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
								Member.Set(Result, GeneratedObjectSerializerBase.ReadCaseInsensitiveString(Reader, FieldDataType));
								break;

							case TYPE_UINT16:
							case TYPE_VARUINT16:
								if (Member.Nullable)
									Member.Set(Result, GeneratedObjectSerializerBase.ReadNullableUInt16(Reader, FieldDataType));
								else
									Member.Set(Result, GeneratedObjectSerializerBase.ReadUInt16(Reader, FieldDataType));
								break;

							case TYPE_UINT32:
							case TYPE_VARUINT32:
								if (Member.Nullable)
									Member.Set(Result, GeneratedObjectSerializerBase.ReadNullableUInt32(Reader, FieldDataType));
								else
									Member.Set(Result, GeneratedObjectSerializerBase.ReadUInt32(Reader, FieldDataType));
								break;

							case TYPE_UINT64:
							case TYPE_VARUINT64:
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
								StringBuilder sb = new StringBuilder();

								sb.Append("Invalid member type: ");
								AppendType(Member.MemberType, sb);

								throw new SerializationException(sb.ToString(), this.type);

							case TYPE_ARRAY:
								Member.Set(Result, await GeneratedObjectSerializerBase.ReadArray(Member.MemberType.GetElementType(), this.context, Reader, FieldDataType));
								break;

							case TYPE_BYTEARRAY:
								Member.Set(Result, GeneratedObjectSerializerBase.ReadByteArray(Reader, FieldDataType));
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

									case TYPE_VARINT16:
										Member.Set(Result, Enum.ToObject(Member.MemberType, (int)Reader.ReadVariableLengthInt16()));
										break;

									case TYPE_INT32:
										Member.Set(Result, Enum.ToObject(Member.MemberType, Reader.ReadInt32()));
										break;

									case TYPE_VARINT32:
										Member.Set(Result, Enum.ToObject(Member.MemberType, Reader.ReadVariableLengthInt32()));
										break;

									case TYPE_INT64:
										Member.Set(Result, Enum.ToObject(Member.MemberType, Reader.ReadInt64()));
										break;

									case TYPE_VARINT64:
										Member.Set(Result, Enum.ToObject(Member.MemberType, Reader.ReadVariableLengthInt64()));
										break;

									case TYPE_SBYTE:
										Member.Set(Result, Enum.ToObject(Member.MemberType, (int)Reader.ReadSByte()));
										break;

									case TYPE_UINT16:
										Member.Set(Result, Enum.ToObject(Member.MemberType, (int)Reader.ReadUInt16()));
										break;

									case TYPE_VARUINT16:
										Member.Set(Result, Enum.ToObject(Member.MemberType, (int)Reader.ReadVariableLengthUInt16()));
										break;

									case TYPE_UINT32:
										Member.Set(Result, Enum.ToObject(Member.MemberType, Reader.ReadUInt32()));
										break;

									case TYPE_VARUINT32:
										Member.Set(Result, Enum.ToObject(Member.MemberType, Reader.ReadVariableLengthUInt32()));
										break;

									case TYPE_UINT64:
										Member.Set(Result, Enum.ToObject(Member.MemberType, Reader.ReadUInt64()));
										break;

									case TYPE_VARUINT64:
										Member.Set(Result, Enum.ToObject(Member.MemberType, Reader.ReadVariableLengthUInt64()));
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
										sb = new StringBuilder();

										sb.Append("Unable to set ");
										sb.Append(Member.Name);
										sb.Append(". Expected an enumeration value, but was a ");
										sb.Append(GetFieldDataTypeName(FieldDataType));
										sb.Append(".");

										throw new SerializationException(sb.ToString(), this.type);
								}
								break;

							case TYPE_OBJECT:
								if (Member.ByReference)
								{
									switch (FieldDataType)
									{
										case TYPE_GUID:
											Guid RefObjectId = Reader.ReadGuid();
											object Value = await this.context.TryLoadObject(Member.MemberType, RefObjectId,
												(EmbeddedValue) => Member.Set(Result, EmbeddedValue))
												?? throw new KeyNotFoundException("Referenced object not found.");

											Member.Set(Result, Value);
											break;

										case TYPE_NULL:
											Member.Set(Result, null);
											break;

										default:
											sb = new StringBuilder();

											sb.Append("Object ID expected for ");
											sb.Append(Member.Name);
											sb.Append(".");

											throw new SerializationException(sb.ToString(), this.type);
									}
								}
								else
								{
									switch (FieldDataType)
									{
										case TYPE_OBJECT:
											Member.Set(Result, await Member.NestedSerializer.Deserialize(Reader, FieldDataType, true));
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

										case TYPE_VARINT16:
											Member.Set(Result, Reader.ReadVariableLengthInt16());
											break;

										case TYPE_INT32:
											Member.Set(Result, Reader.ReadInt32());
											break;

										case TYPE_VARINT32:
											Member.Set(Result, Reader.ReadVariableLengthInt32());
											break;

										case TYPE_INT64:
											Member.Set(Result, Reader.ReadInt64());
											break;

										case TYPE_VARINT64:
											Member.Set(Result, Reader.ReadVariableLengthInt64());
											break;

										case TYPE_SBYTE:
											Member.Set(Result, Reader.ReadSByte());
											break;

										case TYPE_UINT16:
											Member.Set(Result, Reader.ReadUInt16());
											break;

										case TYPE_VARUINT16:
											Member.Set(Result, Reader.ReadVariableLengthUInt16());
											break;

										case TYPE_UINT32:
											Member.Set(Result, Reader.ReadUInt32());
											break;

										case TYPE_VARUINT32:
											Member.Set(Result, Reader.ReadVariableLengthUInt32());
											break;

										case TYPE_UINT64:
											Member.Set(Result, Reader.ReadUInt64());
											break;

										case TYPE_VARUINT64:
											Member.Set(Result, Reader.ReadVariableLengthUInt64());
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
											Member.Set(Result, Reader.ReadCaseInsensitiveString());
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
											Member.Set(Result, await GeneratedObjectSerializerBase.ReadArray(typeof(GenericObject), this.context, Reader, FieldDataType));
											break;

										default:
											sb = new StringBuilder();

											sb.Append("Object expected for ");
											sb.Append(Member.Name);
											sb.Append(". Data Type read: ");
											sb.Append(GetFieldDataTypeName(FieldDataType));

											throw new SerializationException(sb.ToString(), this.type);
									}
								}
								break;
						}
					}
				}

				if (!(this.obsoleteMethod is null) && (!(Obsolete is null)))
					this.obsoleteMethod.Invoke(Result, new object[] { Obsolete });

				return Result;
#if NETSTANDARD2_0
			}
#endif
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="Writer">Serializer.</param>
		/// <param name="WriteTypeCode">If a type code is to be written.</param>
		/// <param name="Embedded">If the object is embedded in another object.</param>
		/// <param name="Value">Value to serialize.</param>
		/// <param name="State">State object, passed on in recursive calls.</param>
		public virtual async Task Serialize(ISerializer Writer, bool WriteTypeCode, bool Embedded, object Value, object State)
		{
#if NETSTANDARD2_0
			if (this.compiled)
				await this.customSerializer.Serialize(Writer, WriteTypeCode, Embedded, Value, State);
			else
			{
#endif
				Type T = Value?.GetType();
				if (!(T is null) && T != this.type)
				{
					IObjectSerializer Serializer = await this.context.GetObjectSerializer(T);
					await Serializer.Serialize(Writer, WriteTypeCode, Embedded, Value, State);
					return;
				}

				ISerializer WriterBak = Writer;

				if (!Embedded)
					Writer = Writer.CreateNew();

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
					Writer.WriteVariableLengthUInt64(0);    // Same as Writer.Write("") for non-normalized serialization.
				else
				{
					if (this.normalized)
					{
						if (this.typeNameSerialization == TypeNameSerialization.LocalName)
							Writer.WriteVariableLengthUInt64(await this.context.GetFieldCode(this.collectionName, this.type.Name));
						else
							Writer.WriteVariableLengthUInt64(await this.context.GetFieldCode(this.collectionName, this.type.FullName));
					}
					else
					{
						if (this.typeNameSerialization == TypeNameSerialization.LocalName)
							Writer.Write(this.type.Name);
						else
							Writer.Write(this.type.FullName);
					}
				}

				if (Embedded)
				{
					if (this.normalized)
					{
						if (string.IsNullOrEmpty(this.collectionName))
							Writer.WriteVariableLengthUInt64(await this.context.GetFieldCode(null, this.context.DefaultCollectionName));
						else
							Writer.WriteVariableLengthUInt64(await this.context.GetFieldCode(null, this.collectionName));
					}
					else
					{
						if (string.IsNullOrEmpty(this.collectionName))
							Writer.Write(this.context.DefaultCollectionName);
						else
							Writer.Write(this.collectionName);
					}
				}

				foreach (Member Member in this.membersOrdered)
				{
					if (Member.HasDefaultValue(Value))
						continue;

					if (this.normalized)
						Writer.WriteVariableLengthUInt64(Member.FieldCode);
					else
						Writer.Write(Member.Name);

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
								short i16 = (short)MemberValue;
								if (i16 > GeneratedObjectSerializerBase.Int16VarSizeMinLimit && 
									i16 < GeneratedObjectSerializerBase.Int16VarSizeMaxLimit)
								{
									Writer.WriteBits(TYPE_VARINT16, 6);
									Writer.WriteVariableLengthInt16(i16);
								}
								else
								{
									Writer.WriteBits(TYPE_INT16, 6);
									Writer.Write(i16);
								}
								break;

							case TYPE_INT32:
								int i32 = (int)MemberValue;
								if (i32 > GeneratedObjectSerializerBase.Int32VarSizeMinLimit &&
									i32 < GeneratedObjectSerializerBase.Int32VarSizeMaxLimit)
								{
									Writer.WriteBits(TYPE_VARINT32, 6);
									Writer.WriteVariableLengthInt32(i32);
								}
								else
								{
									Writer.WriteBits(TYPE_INT32, 6);
									Writer.Write(i32);
								}
								break;

							case TYPE_INT64:
								long i64 = (long)MemberValue;
								if (i64 > GeneratedObjectSerializerBase.Int64VarSizeMinLimit &&
									i64 < GeneratedObjectSerializerBase.Int64VarSizeMaxLimit)
								{
									Writer.WriteBits(TYPE_VARINT64, 6);
									Writer.WriteVariableLengthInt64(i64);
								}
								else
								{
									Writer.WriteBits(TYPE_INT64, 6);
									Writer.Write(i64);
								}
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
								ushort ui16 = (ushort)MemberValue;
								if (ui16 < GeneratedObjectSerializerBase.UInt16VarSizeLimit)
								{
									Writer.WriteBits(TYPE_VARUINT16, 6);
									Writer.WriteVariableLengthUInt16(ui16);
								}
								else
								{
									Writer.WriteBits(TYPE_UINT16, 6);
									Writer.Write(ui16);
								}
								break;

							case TYPE_UINT32:
								uint ui32 = (uint)MemberValue;
								if (ui32 < GeneratedObjectSerializerBase.UInt32VarSizeLimit)
								{
									Writer.WriteBits(TYPE_VARUINT32, 6);
									Writer.WriteVariableLengthUInt32(ui32);
								}
								else
								{
									Writer.WriteBits(TYPE_UINT32, 6);
									Writer.Write(ui32);
								}
								break;

							case TYPE_UINT64:
							case TYPE_VARUINT64:
								ulong ui64 = (ulong)MemberValue;
								if (ui64 < GeneratedObjectSerializerBase.UInt64VarSizeLimit)
								{
									Writer.WriteBits(TYPE_VARUINT64, 6);
									Writer.WriteVariableLengthUInt64(ui64);
								}
								else
								{
									Writer.WriteBits(TYPE_UINT64, 6);
									Writer.Write(ui64);
								}
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
								StringBuilder sb = new StringBuilder();

								sb.Append("Invalid member type: ");
								AppendType(Member.MemberType, sb);

								throw new SerializationException(sb.ToString(), this.type);

							case TYPE_ARRAY:
								if (Member.MemberType == typeof(KeyValuePair<string, object>[]))
									await GeneratedObjectSerializerBase.WriteTagArray(this.context, Writer, (KeyValuePair<string, object>[])MemberValue, State);
								else if (Member.MemberType == typeof(KeyValuePair<string, IElement>[]))
									await GeneratedObjectSerializerBase.WriteTagElementArray(this.context, Writer, (KeyValuePair<string, IElement>[])MemberValue, State);
								else
									await GeneratedObjectSerializerBase.WriteArray(Member.MemberType.GetElementType(), this.context, Writer, (Array)MemberValue, State);
								break;

							case TYPE_BYTEARRAY:
								if (MemberValue is null)
									Writer.WriteBits(TYPE_NULL, 6);
								else
								{
									Writer.WriteBits(TYPE_BYTEARRAY, 6);
									Writer.Write((byte[])MemberValue);
								}
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
										Writer.Write(await GetObjectId(MemberValue, true, State));
									}
									else
									{
										sb = new StringBuilder();

										sb.Append("Objects of type ");
										AppendType(Member.MemberType, sb);
										sb.Append(" cannot be stored by reference.");

										throw new SerializationException(sb.ToString(), this.type);
									}
								}
								else
									await Member.NestedSerializer.Serialize(Writer, true, true, MemberValue, State);
								break;
						}
					}
				}

				Writer.WriteVariableLengthUInt64(0);    // Same as Writer.Write("") for non-normalized serialization

				if (!Embedded)
				{
					if (this.objectIdMember is null)
						WriterBak.Write(this.context.CreateGuid());
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
							throw new SerializationException("Invalid Object ID type.", this.type);

						if (!HasGuid)
						{
							Guid NewObjectId = this.context.CreateGuid();
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
#if NETSTANDARD2_0
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
#if NETSTANDARD2_0
				if (this.compiled)
				{
					if (!(this.objectIdMemberInfo is null))
						return this.objectIdMemberInfo.Name;
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
#if NETSTANDARD2_0
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
#if NETSTANDARD2_0
				if (this.compiled)
					return !(this.objectIdMemberInfo is null);
				else
#endif
					return !(this.objectIdMember is null);
			}
		}

		/// <summary>
		/// If the class has an Object ID.
		/// </summary>
		/// <param name="Value">Object reference.</param>
		public virtual Task<bool> HasObjectId(object Value)
		{
			object ObjectId;

#if NETSTANDARD2_0
			if (this.compiled)
			{
				if (!(this.objectIdFieldInfo is null))
					ObjectId = this.objectIdFieldInfo.GetValue(Value);
				else if (!(this.objectIdPropertyInfo is null))
					ObjectId = this.objectIdPropertyInfo.GetValue(Value);
				else
					return Task.FromResult(false);
			}
			else
			{
#endif
				if (!(this.objectIdMember is null))
					ObjectId = this.objectIdMember.Get(Value);
				else
					return Task.FromResult(false);
#if NETSTANDARD2_0
			}
#endif

			if (ObjectId is null)
				return Task.FromResult(false);

			if (ObjectId is Guid && ObjectId.Equals(Guid.Empty))
				return Task.FromResult(false);

			return Task.FromResult(true);
		}

		/// <summary>
		/// Tries to set the object id of an object.
		/// </summary>
		/// <param name="Value">Object reference.</param>
		/// <param name="ObjectId">Object ID</param>
		/// <returns>If the object has an Object ID field or property that could be set.</returns>
		public virtual Task<bool> TrySetObjectId(object Value, Guid ObjectId)
		{
			Type MemberType;
			object Obj;

#if NETSTANDARD2_0
			if (this.compiled)
			{
				if (!(this.objectIdFieldInfo is null))
					MemberType = this.objectIdFieldInfo.FieldType;
				else if (!(this.objectIdPropertyInfo is null))
					MemberType = this.objectIdPropertyInfo.PropertyType;
				else
					return Task.FromResult(false);
			}
			else
			{
#endif
				if (!(this.objectIdMember is null))
					MemberType = this.objectIdMember.MemberType;
				else
					return Task.FromResult(false);
#if NETSTANDARD2_0
			}
#endif

			if (MemberType == typeof(Guid))
				Obj = ObjectId;
			else if (MemberType == typeof(string))
				Obj = ObjectId.ToString();
			else if (MemberType == typeof(byte[]))
				Obj = ObjectId.ToByteArray();
			else
				return Task.FromResult(false);

#if NETSTANDARD2_0
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

			return Task.FromResult(true);
		}

		/// <summary>
		/// Gets the Object ID for a given object.
		/// </summary>
		/// <param name="Value">Object reference.</param>
		/// <param name="InsertIfNotFound">Insert object into database with new Object ID, if no Object ID is set.</param>
		/// <param name="State">State object, passed on in recursive calls.</param>
		/// <returns>Object ID for <paramref name="Value"/>.</returns>
		/// <exception cref="NotSupportedException">Thrown, if the corresponding class does not have an Object ID property, 
		/// or if the corresponding property type is not supported.</exception>
		public virtual async Task<Guid> GetObjectId(object Value, bool InsertIfNotFound, object State)
		{
			object Obj;

#if NETSTANDARD2_0
			if (this.compiled)
			{
				if (!(this.objectIdFieldInfo is null))
					Obj = this.objectIdFieldInfo.GetValue(Value);
				else if (!(this.objectIdPropertyInfo is null))
					Obj = this.objectIdPropertyInfo.GetValue(Value);
				else
				{
					StringBuilder sb = new StringBuilder();

					sb.Append("No Object ID member found in objects of type ");
					AppendType(Value.GetType(), sb);
					sb.Append(".");

					throw new NotSupportedException(sb.ToString());
				}
			}
			else
			{
#endif
				if (!(this.objectIdMember is null))
					Obj = this.objectIdMember.Get(Value);
				else
				{
					StringBuilder sb = new StringBuilder();

					sb.Append("No Object ID member found in objects of type ");
					AppendType(Value.GetType(), sb);
					sb.Append(".");

					throw new NotSupportedException(sb.ToString());
				}
#if NETSTANDARD2_0
			}
#endif

			if (Obj is null || (Obj is Guid && Obj.Equals(Guid.Empty)))
			{
				if (!InsertIfNotFound)
					throw new SerializationException("Object has no Object ID defined.", this.type);

				Guid ObjectId = await this.context.SaveNewObject(Value, State);
				Type T;

#if NETSTANDARD2_0
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
				{
					StringBuilder sb = new StringBuilder();

					sb.Append("Unsupported type for Object ID members: ");
					AppendType(Obj.GetType(), sb);

					throw new NotSupportedException(sb.ToString());
				}

#if NETSTANDARD2_0
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
			{
				StringBuilder sb = new StringBuilder();

				sb.Append("Unsupported type for Object ID members: ");
				AppendType(Obj.GetType(), sb);

				throw new NotSupportedException(sb.ToString());
			}
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

#if NETSTANDARD2_0
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
#if NETSTANDARD2_0
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
		/// <returns>Corresponding field or property value, if found, or null otherwise.</returns>
		public virtual Task<object> TryGetFieldValue(string FieldName, object Object)
		{
#if NETSTANDARD2_0
			if (this.compiled)
				return this.customSerializer.TryGetFieldValue(FieldName, Object);
			else
			{
#endif
				if (this.membersByName.TryGetValue(FieldName, out Member Member))
					return Task.FromResult<object>(Member.Get(Object));
				else
					return Task.FromResult<object>(null);
#if NETSTANDARD2_0
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
			LinkedList<MemberInfo> Result = new LinkedList<MemberInfo>();
			Dictionary<string, bool> Names = new Dictionary<string, bool>();

			while (!(T is null))
			{
				foreach (MemberInfo MI in T.DeclaredMembers)
				{
#if NETSTANDARD2_0
					if (MI.MemberType != MemberTypes.Property && MI.MemberType != MemberTypes.Field)
						continue;
#endif
					if (!Names.ContainsKey(MI.Name))
					{
						Result.AddLast(MI);
						Names[MI.Name] = true;  // To avoid repetition of virtual members.
					}
				}

				if (!(T.BaseType is null))
					T = T.BaseType.GetTypeInfo();
				else
					T = null;
			}

			return Result;
		}

		private static void AppendType(Type T, StringBuilder sb)
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

					AppendType(Arg, sb);
				}

				sb.Append('>');
			}
			else if (T.HasElementType)
			{
				if (T.IsArray)
				{
					AppendType(T.GetElementType(), sb);
					sb.Append("[]");
				}
				else if (T.IsPointer)
				{
					AppendType(T.GetElementType(), sb);
					sb.Append('*');
				}
				else
					sb.Append(T.FullName);
			}
			else
				sb.Append(T.FullName);
		}

		#region Static interface

		/// <summary>
		/// Returns the type name corresponding to a given field data type code.
		/// </summary>
		/// <param name="FieldDataType">Field data type code.</param>
		/// <returns>Corresponding data type name.</returns>
		public static string GetFieldDataTypeName(uint FieldDataType)
		{
			return GetFieldDataType(FieldDataType).FullName;
		}

		/// <summary>
		/// Returns the type corresponding to a given field data type code.
		/// </summary>
		/// <param name="FieldDataTypeCode">Field data type code.</param>
		/// <returns>Corresponding data type.</returns>
		public static Type GetFieldDataType(uint FieldDataTypeCode)
		{
			switch (FieldDataTypeCode)
			{
				case TYPE_BOOLEAN: return typeof(bool);
				case TYPE_BYTE: return typeof(byte);
				case TYPE_INT16: return typeof(short);
				case TYPE_INT32: return typeof(int);
				case TYPE_INT64: return typeof(long);
				case TYPE_SBYTE: return typeof(sbyte);
				case TYPE_UINT16: return typeof(ushort);
				case TYPE_UINT32: return typeof(uint);
				case TYPE_UINT64: return typeof(ulong);
				case TYPE_VARINT16: return typeof(short);
				case TYPE_VARINT32: return typeof(int);
				case TYPE_VARINT64: return typeof(long);
				case TYPE_VARUINT16: return typeof(ushort);
				case TYPE_VARUINT32: return typeof(uint);
				case TYPE_VARUINT64: return typeof(ulong);
				case TYPE_DECIMAL: return typeof(decimal);
				case TYPE_DOUBLE: return typeof(double);
				case TYPE_SINGLE: return typeof(float);
				case TYPE_DATETIME: return typeof(DateTime);
				case TYPE_DATETIMEOFFSET: return typeof(DateTimeOffset);
				case TYPE_TIMESPAN: return typeof(TimeSpan);
				case TYPE_CHAR: return typeof(char);
				case TYPE_STRING: return typeof(string);
				case TYPE_CI_STRING: return typeof(CaseInsensitiveString);
				case TYPE_ENUM: return typeof(Enum);
				case TYPE_BYTEARRAY: return typeof(byte[]);
				case TYPE_GUID: return typeof(Guid);
				case TYPE_ARRAY: return typeof(Array);
				case TYPE_NULL:
				case TYPE_OBJECT: return typeof(object);
				default:
					StringBuilder sb = new StringBuilder();

					sb.Append("Unrecognized data type code: ");
					sb.Append(FieldDataTypeCode.ToString());

					throw new Exception(sb.ToString());
			}
		}

		/// <summary>
		/// Returns the type code corresponding to a given field data type.
		/// </summary>
		/// <param name="Value">Field data value.</param>
		/// <returns>Corresponding data type code.</returns>
		public static uint GetFieldDataTypeCode(object Value)
		{
			if (Value is null)
				return TYPE_NULL;
			else
				return GetFieldDataTypeCode(Value.GetType());
		}

		/// <summary>
		/// Returns the type code corresponding to a given field data type.
		/// </summary>
		/// <param name="FieldDataType">Field data type.</param>
		/// <returns>Corresponding data type code.</returns>
		public static uint GetFieldDataTypeCode(Type FieldDataType)
		{
			System.Reflection.TypeInfo TI = FieldDataType.GetTypeInfo();
			if (TI.IsEnum)
			{
				if (TI.IsDefined(typeof(FlagsAttribute), false))
					return TYPE_INT32;
				else
					return TYPE_ENUM;
			}

			if (FieldDataType == typeof(bool))
				return TYPE_BOOLEAN;
			else if (FieldDataType == typeof(byte))
				return TYPE_BYTE;
			else if (FieldDataType == typeof(short))
				return TYPE_INT16;
			else if (FieldDataType == typeof(int))
				return TYPE_INT32;
			else if (FieldDataType == typeof(long))
				return TYPE_INT64;
			else if (FieldDataType == typeof(sbyte))
				return TYPE_SBYTE;
			else if (FieldDataType == typeof(ushort))
				return TYPE_UINT16;
			else if (FieldDataType == typeof(uint))
				return TYPE_UINT32;
			else if (FieldDataType == typeof(ulong))
				return TYPE_UINT64;
			else if (FieldDataType == typeof(decimal))
				return TYPE_DECIMAL;
			else if (FieldDataType == typeof(double))
				return TYPE_DOUBLE;
			else if (FieldDataType == typeof(float))
				return TYPE_SINGLE;
			else if (FieldDataType == typeof(DateTime))
				return TYPE_DATETIME;
			else if (FieldDataType == typeof(DateTimeOffset))
				return TYPE_DATETIMEOFFSET;
			else if (FieldDataType == typeof(char))
				return TYPE_CHAR;
			else if (FieldDataType == typeof(string))
				return TYPE_STRING;
			else if (FieldDataType == typeof(CaseInsensitiveString))
				return TYPE_CI_STRING;
			else if (FieldDataType == typeof(TimeSpan))
				return TYPE_TIMESPAN;
			else if (FieldDataType == typeof(byte[]))
				return TYPE_BYTEARRAY;
			else if (FieldDataType == typeof(Guid))
				return TYPE_GUID;
			else if (FieldDataType == typeof(CaseInsensitiveString))
				return TYPE_CI_STRING;
			else if (TI.IsArray)
			{
				if (FieldDataType == typeof(KeyValuePair<string, object>[]) ||
					FieldDataType == typeof(KeyValuePair<string, IElement>[]))
				{
					return TYPE_OBJECT;
				}
				else return TYPE_ARRAY;
			}
			else if (FieldDataType == typeof(void))
				return TYPE_NULL;
			else
				return TYPE_OBJECT;
		}

		#endregion

		#region Generic interface

		/// <summary>
		/// Reads a generic array.
		/// </summary>
		/// <param name="Reader"></param>
		/// <returns></returns>
		protected async Task<Array> ReadGenericArray(IDeserializer Reader)
		{
			ulong NrElements = Reader.ReadVariableLengthUInt64();
			uint ElementDataType = Reader.ReadBits(6);

			switch (ElementDataType)
			{
				case TYPE_BOOLEAN: return await this.ReadArray<bool>(Reader, NrElements, ElementDataType);
				case TYPE_BYTE: return await this.ReadArray<byte>(Reader, NrElements, ElementDataType);
				case TYPE_INT16:
				case TYPE_VARINT16:
					return await this.ReadArray<short>(Reader, NrElements, ElementDataType);
				case TYPE_INT32:
				case TYPE_VARINT32:
					return await this.ReadArray<int>(Reader, NrElements, ElementDataType);
				case TYPE_INT64:
				case TYPE_VARINT64:
					return await this.ReadArray<long>(Reader, NrElements, ElementDataType);
				case TYPE_SBYTE: return await this.ReadArray<sbyte>(Reader, NrElements, ElementDataType);
				case TYPE_UINT16:
				case TYPE_VARUINT16:
					return await this.ReadArray<ushort>(Reader, NrElements, ElementDataType);
				case TYPE_UINT32:
				case TYPE_VARUINT32:
					return await this.ReadArray<uint>(Reader, NrElements, ElementDataType);
				case TYPE_UINT64:
				case TYPE_VARUINT64:
					return await this.ReadArray<ulong>(Reader, NrElements, ElementDataType);
				case TYPE_DECIMAL: return await this.ReadArray<decimal>(Reader, NrElements, ElementDataType);
				case TYPE_DOUBLE: return await this.ReadArray<double>(Reader, NrElements, ElementDataType);
				case TYPE_SINGLE: return await this.ReadArray<float>(Reader, NrElements, ElementDataType);
				case TYPE_DATETIME: return await this.ReadArray<DateTime>(Reader, NrElements, ElementDataType);
				case TYPE_DATETIMEOFFSET: return await this.ReadArray<DateTimeOffset>(Reader, NrElements, ElementDataType);
				case TYPE_TIMESPAN: return await this.ReadArray<TimeSpan>(Reader, NrElements, ElementDataType);
				case TYPE_CHAR: return await this.ReadArray<char>(Reader, NrElements, ElementDataType);
				case TYPE_STRING:
				case TYPE_ENUM: return await this.ReadArray<string>(Reader, NrElements, ElementDataType);
				case TYPE_CI_STRING: return await this.ReadArray<CaseInsensitiveString>(Reader, NrElements, ElementDataType);
				case TYPE_BYTEARRAY: return await this.ReadArray<byte[]>(Reader, NrElements, ElementDataType);
				case TYPE_GUID: return await this.ReadArray<Guid>(Reader, NrElements, ElementDataType);
				case TYPE_ARRAY: return await this.ReadArrayOfArrays(Reader, NrElements);
				case TYPE_OBJECT: return await this.ReadArrayOfObjects(Reader, NrElements, ElementDataType);
				case TYPE_NULL: return await this.ReadArrayOfNullableElements(Reader, NrElements);
				default:
					StringBuilder sb = new StringBuilder();

					sb.Append("Unrecognized data type: ");
					sb.Append(ElementDataType.ToString());

					throw new Exception(sb.ToString());
			}
		}

		private async Task<T[]> ReadArray<T>(IDeserializer Reader, ulong NrElements, uint ElementDataType)
		{
			List<T> Elements = new List<T>();
			IObjectSerializer S = await this.Context.GetObjectSerializer(typeof(T));

			while (NrElements > 0)
			{
				if (await S.Deserialize(Reader, ElementDataType, true) is T Item)
				{
					Elements.Add(Item);
					NrElements--;
				}
			}

			return Elements.ToArray();
		}

		private async Task<Array[]> ReadArrayOfArrays(IDeserializer Reader, ulong NrElements)
		{
			List<Array> Elements = new List<Array>();

			while (NrElements-- > 0)
				Elements.Add(await this.ReadGenericArray(Reader));

			return Elements.ToArray();
		}

		private async Task<GenericObject[]> ReadArrayOfObjects(IDeserializer Reader, ulong NrElements, uint ElementDataType)
		{
			List<GenericObject> Elements = new List<GenericObject>();

			while (NrElements-- > 0)
				Elements.Add((GenericObject)await this.Deserialize(Reader, ElementDataType, true));

			return Elements.ToArray();
		}

		private async Task<object[]> ReadArrayOfNullableElements(IDeserializer Reader, ulong NrElements)
		{
			List<object> Elements = new List<object>();
			uint ElementDataType;

			while (NrElements-- > 0)
			{
				ElementDataType = Reader.ReadBits(6);

				switch (ElementDataType)
				{
					case TYPE_BOOLEAN:
						Elements.Add(Reader.ReadBoolean());
						break;

					case TYPE_BYTE:
						Elements.Add(Reader.ReadByte());
						break;

					case TYPE_INT16:
						Elements.Add(Reader.ReadInt16());
						break;

					case TYPE_INT32:
						Elements.Add(Reader.ReadInt32());
						break;

					case TYPE_INT64:
						Elements.Add(Reader.ReadInt64());
						break;

					case TYPE_SBYTE:
						Elements.Add(Reader.ReadSByte());
						break;

					case TYPE_UINT16:
						Elements.Add(Reader.ReadUInt16());
						break;

					case TYPE_UINT32:
						Elements.Add(Reader.ReadUInt32());
						break;

					case TYPE_UINT64:
						Elements.Add(Reader.ReadUInt64());
						break;

					case TYPE_VARINT16:
						Elements.Add(Reader.ReadVariableLengthInt16());
						break;

					case TYPE_VARINT32:
						Elements.Add(Reader.ReadVariableLengthInt32());
						break;

					case TYPE_VARINT64:
						Elements.Add(Reader.ReadVariableLengthInt64());
						break;

					case TYPE_VARUINT16:
						Elements.Add(Reader.ReadVariableLengthUInt16());
						break;

					case TYPE_VARUINT32:
						Elements.Add(Reader.ReadVariableLengthUInt32());
						break;

					case TYPE_VARUINT64:
						Elements.Add(Reader.ReadVariableLengthUInt64());
						break;

					case TYPE_DECIMAL:
						Elements.Add(Reader.ReadDecimal());
						break;

					case TYPE_DOUBLE:
						Elements.Add(Reader.ReadDouble());
						break;

					case TYPE_SINGLE:
						Elements.Add(Reader.ReadSingle());
						break;

					case TYPE_DATETIME:
						Elements.Add(Reader.ReadDateTime());
						break;

					case TYPE_DATETIMEOFFSET:
						Elements.Add(Reader.ReadDateTimeOffset());
						break;

					case TYPE_TIMESPAN:
						Elements.Add(Reader.ReadTimeSpan());
						break;

					case TYPE_CHAR:
						Elements.Add(Reader.ReadChar());
						break;

					case TYPE_STRING:
					case TYPE_ENUM:
						Elements.Add(Reader.ReadString());
						break;

					case TYPE_CI_STRING:
						Elements.Add(Reader.ReadCaseInsensitiveString());
						break;

					case TYPE_BYTEARRAY:
						Elements.Add(Reader.ReadByteArray());
						break;

					case TYPE_GUID:
						Elements.Add(Reader.ReadGuid());
						break;

					case TYPE_ARRAY:
						Elements.Add(await this.ReadGenericArray(Reader));
						break;

					case TYPE_OBJECT:
						Elements.Add(await this.Deserialize(Reader, ElementDataType, true));
						break;

					case TYPE_NULL:
						Elements.Add(null);
						break;

					default:
						StringBuilder sb = new StringBuilder();

						sb.Append("Unrecognized data type: ");
						sb.Append(ElementDataType.ToString());

						throw new Exception(sb.ToString());
				}
			}

			return Elements.ToArray();
		}

		#endregion
	}
}
