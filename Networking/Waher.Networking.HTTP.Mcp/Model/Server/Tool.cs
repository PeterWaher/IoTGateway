using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Waher.Networking.HTTP.Mcp.Model.Attributes;
using Waher.Persistence;
using Waher.Runtime.Collections;
using Waher.Script.Functions.ComplexNumbers;
using Waher.Script.Model;

namespace Waher.Networking.HTTP.Mcp.Model.Server
{
	/// <summary>
	/// Contains information about an MCP Server Tool
	/// </summary>
	public class Tool
	{
		private Icons? icons = null;

		/// <summary>
		/// Contains information about an MCP Server Tool
		/// </summary>
		/// <param name="Method">Method to invoke when tool is executed.</param>
		/// <param name="Title">A human-readable title for the tool.</param>
		/// <param name="Description">A human-readable description of the tool.
		/// 
		/// This can be used by clients to improve the LLM's understanding of available tools. 
		/// It can be thought of like a "hint" to the model.</param>
		/// <param name="IconsMethod">Name of method that returns an <see cref="Icon?"/>, 
		/// an an <see cref="Icon[]?"/> or an <see cref="Icons?"/> resource representing 
		/// the tool. If null or empty, the icon of the MCP server will be used.</param>
		/// <param name="CanModifyEnvironment">If the tool can modify the environment. If false, 
		/// the tool is expected to be read-only and not cause any side effects.</param>
		/// <param name="CanDestroyEnvironment">If true, the tool may perform destructive 
		/// updates to its environment. If false, the tool performs only additive updates.</param>
		/// <param name="Idempotent">If true, calling the tool repeatedly with the same 
		/// arguments will have no additional effect on its environment.</param>
		/// <param name="OpenWorldAccess">If true, this tool may interact with an 
		/// "open world" of external entities. If false, the tool's domain of interaction 
		/// is closed.</param>
		/// <param name="MetaData">Meta-data associated with tool.</param>
		public Tool(MethodInfo Method, string Title, string Description, string IconsMethod,
			bool CanModifyEnvironment, bool CanDestroyEnvironment, bool Idempotent,
			bool OpenWorldAccess, params KeyValuePair<string, object>[] MetaData)
		{
			this.Method = Method;
			this.Title = Title;
			this.Description = Description;
			this.IconsMethod = IconsMethod;
			this.CanModifyEnvironment = CanModifyEnvironment;
			this.CanDestroyEnvironment = CanDestroyEnvironment;
			this.Idempotent = Idempotent;
			this.OpenWorldAccess = OpenWorldAccess;
			this.MetaData = MetaData;
		}

		/// <summary>
		/// Method to invoke when tool is executed.
		/// </summary>
		public MethodInfo Method { get; private set; }

		/// <summary>
		/// A human-readable title for the tool.
		/// </summary>
		public string Title { get; private set; }

		/// <summary>
		/// A human-readable description of the tool.
		/// 
		/// This can be used by clients to improve the LLM's understanding of available tools. 
		/// It can be thought of like a "hint" to the model.
		/// </summary>
		public string Description { get; private set; }

		/// <summary>
		/// Name of method that returns an <see cref="Icon?"/>, an an <see cref="Icon[]?"/>
		/// or an <see cref="Icons?"/> resource representing the tool. If null or empty, the 
		/// icon of the MCP server will be used.
		/// </summary>
		public string IconsMethod { get; private set; }

		/// <summary>
		/// If the tool can modify the environment. If false, the tool is expected to be 
		/// read-only and not cause any side effects.
		/// </summary>
		public bool CanModifyEnvironment { get; private set; }

		/// <summary>
		/// If true, the tool may perform destructive updates to its environment.
		/// If false, the tool performs only additive updates.
		/// </summary>
		/// <remarks>
		/// This property is meaningful only when <see cref="CanModifyEnvironment"/> is true.
		/// </remarks>
		public bool CanDestroyEnvironment { get; private set; }

		/// <summary>
		/// If true, calling the tool repeatedly with the same arguments
		/// will have no additional effect on its environment.
		/// </summary>
		/// <remarks>
		/// This property is meaningful only when <see cref="CanModifyEnvironment"/> is true.
		/// </remarks>
		public bool Idempotent { get; private set; }

		/// <summary>
		/// If true, this tool may interact with an "open world" of external
		/// entities.If false, the tool's domain of interaction is closed.
		/// </summary>
		public bool OpenWorldAccess { get; private set; }

		/// <summary>
		/// Meta-data associated with tool.
		/// </summary>
		public KeyValuePair<string, object>[] MetaData { get; private set; }

		/// <summary>
		/// Converts object to a generic representation.
		/// </summary>
		/// <param name="Resource">MCP Server reference.</param>
		/// <returns>Generic representation.</returns>
		public async Task<Dictionary<string, object>> ToJson(HttpMcpServerResource Resource)
		{
			if (this.icons is null)
			{
				if (string.IsNullOrEmpty(this.IconsMethod))
					this.icons = new Icons();
				else
				{
					MethodInfo? MI = Resource.GetType().GetMethod(this.IconsMethod,
						BindingFlags.Static | BindingFlags.Instance |
						BindingFlags.Public | BindingFlags.NonPublic);

					if (MI is null)
						this.icons = new Icons();
					else
					{
						object? Obj = await ScriptNode.WaitPossibleTask(MI.Invoke(Resource, null));

						if (Obj is Icons Typed)
							this.icons = Typed;
						else if (Obj is Icon[] IconArray)
							this.icons = new Icons(IconArray);
						else if (Obj is Icon SingleIcon)
							this.icons = new Icons(SingleIcon);
						else if (Obj is null)
							this.icons = new Icons();
						else
						{
							throw new ArgumentException("Method " + this.IconsMethod +
								"returned an invalid type: " + Obj.GetType().FullName,
								nameof(this.IconsMethod));
						}
					}
				}

				if (this.icons.Empty)
					this.icons = Resource.Icons;
			}

			Dictionary<string, object> Annotations = new Dictionary<string, object>()
			{
				{ "readOnlyHint", this.CanModifyEnvironment },
				{ "destructiveHint", this.CanDestroyEnvironment },
				{ "idempotentHint", this.Idempotent },
				{ "openWorldHint", this.OpenWorldAccess }
			};
			Dictionary<string, object> Result = new Dictionary<string, object>()
			{
				{ "name", this.Method.Name },
				{ "execution", new Dictionary<string,object>()
					{
						{ "taskSupport", "optional" }
					}
				},
				{ "inputSchema", GenerateSchema(this.Method) },
				{ "annotations", Annotations }
			};

			if (!string.IsNullOrEmpty(this.Title))
				Annotations.Add("title", this.Title);

			if (!string.IsNullOrEmpty(this.Title))
				Result.Add("description", this.Description);

			if (!this.icons.Empty)
				Result.Add("icons", this.icons.ToJson());

			if (this.Method.ReturnType != typeof(void))
			{
				McpParameterAttribute ReturnInfo = this.Method.ReturnParameter.GetCustomAttribute<McpParameterAttribute>(true);
				IEnumerable<McpEnumValueAttribute>? EnumValues = this.Method.ReturnType.IsEnum ?
					this.Method.ReturnParameter.GetCustomAttributes<McpEnumValueAttribute>(true) : null;

				Result.Add("outputSchema", GenerateSchema(this.Method.ReturnType, false, null, ReturnInfo, EnumValues));
			}

			if ((this.MetaData?.Length ?? 0) > 0)
			{
				Dictionary<string, object> MetaData = new Dictionary<string, object>();

				foreach (KeyValuePair<string, object> P in this.MetaData!)
					MetaData[P.Key] = P.Value;

				Result["_meta"] = MetaData;
			}

			return Result;
		}

		/// <summary>
		/// Generates an Input Schema for a method, based on its parameters.
		/// </summary>
		/// <param name="Method">Method information.</param>
		/// <returns>Input Schema</returns>
		private static Dictionary<string, object> GenerateSchema(MethodInfo Method)
		{
			Dictionary<string, object> Properties = new Dictionary<string, object>();
			ChunkedList<string> Required = new ChunkedList<string>();
			ParameterInfo[] Parameters = Method.GetParameters();

			foreach (ParameterInfo Parameter in Parameters)
			{
				if (!Parameter.IsOptional && !Parameter.HasDefaultValue)
					Required.Add(Parameter.Name);

				McpParameterAttribute ParameterInfo = Parameter.GetCustomAttribute<McpParameterAttribute>(true);
				IEnumerable<McpEnumValueAttribute>? EnumValues = Parameter.ParameterType.IsEnum ?
					Parameter.GetCustomAttributes<McpEnumValueAttribute>(true) : null;

				Properties[Parameter.Name] = GenerateSchema(Parameter.ParameterType,
					Parameter.HasDefaultValue, Parameter.DefaultValue, ParameterInfo, EnumValues);
			}

			Dictionary<string, object> Result = new Dictionary<string, object>()
			{
				{ "type", "object" },
				{ "properties", Properties },
				{ "required", Required.ToArray() }
			};

			return Result;
		}

		private static object GenerateSchema(Type T, bool HasDefault, object? Default,
			McpParameterAttribute? ParameterInfo, IEnumerable<McpEnumValueAttribute>? EnumValues)
		{
			Dictionary<string, object?> Result = new Dictionary<string, object?>();

			if (T.IsEnum)
			{
				ChunkedList<Dictionary<string, object>>? EnumValuesList;

				if (EnumValues is null)
					EnumValuesList = null;
				else
				{
					EnumValuesList = new ChunkedList<Dictionary<string, object>>();

					foreach (McpEnumValueAttribute EnumValue in EnumValues)
					{
						EnumValuesList.Add(new Dictionary<string, object>()
						{
							{ "const", EnumValue.Value.ToString() },
							{ "title", EnumValue.Title ?? EnumValue.Value.ToString() }
						});
					}
				}

				if (Attribute.IsDefined(T, typeof(FlagsAttribute)))
				{
					Result["type"] = "array";

					if (EnumValuesList is null)
					{
						Result["items"] = new Dictionary<string, object>()
						{
							{ "type", "string" },
							{ "enum", Enum.GetNames(T) }
						};
					}
					else
					{
						Result["items"] = new Dictionary<string, object>()
						{
							{ "anyOf", EnumValuesList.ToArray() }
						};
					}
				}
				else
				{
					Result["type"] = "string";

					if (EnumValuesList is null)
						Result["enum"] = Enum.GetNames(T);
					else
						Result["oneOf"] = EnumValuesList.ToArray();
				}
			}
			else
			{
				switch (Type.GetTypeCode(T))
				{
					case TypeCode.Empty:
						Result["type"] = "null";
						break;

					case TypeCode.Object:
						if (T == typeof(CaseInsensitiveString))
							Result["type"] = "string";
						else if (T == typeof(Uri))
						{
							Result["type"] = "string";
							Result["format"] = "uri";
						}
						else if (T.IsArray)
						{
							Result["type"] = "array";
							Result["items"] = GenerateSchema(T.GetElementType()!, false, null, null, null);
						}
						else
							Result["type"] = "object";
						break;

					case TypeCode.DBNull:
						Result["type"] = "null";
						break;

					case TypeCode.Boolean:
						Result["type"] = "boolean";
						break;

					case TypeCode.Char:
						Result["type"] = "string";
						break;

					case TypeCode.SByte:
						Result["type"] = "integer";
						Result["minimum"] = sbyte.MinValue;
						Result["maximum"] = sbyte.MaxValue;
						break;

					case TypeCode.Byte:
						Result["type"] = "integer";
						Result["minimum"] = byte.MinValue;
						Result["maximum"] = byte.MaxValue;
						break;

					case TypeCode.Int16:
						Result["type"] = "integer";
						Result["minimum"] = short.MinValue;
						Result["maximum"] = short.MaxValue;
						break;

					case TypeCode.UInt16:
						Result["type"] = "integer";
						Result["minimum"] = ushort.MinValue;
						Result["maximum"] = ushort.MaxValue;
						break;

					case TypeCode.Int32:
						Result["type"] = "integer";
						Result["minimum"] = int.MinValue;
						Result["maximum"] = int.MaxValue;
						break;

					case TypeCode.UInt32:
						Result["type"] = "integer";
						Result["minimum"] = uint.MinValue;
						Result["maximum"] = uint.MaxValue;
						break;

					case TypeCode.Int64:
						Result["type"] = "integer";
						Result["minimum"] = long.MinValue;
						Result["maximum"] = long.MaxValue;
						break;

					case TypeCode.UInt64:
						Result["type"] = "integer";
						Result["minimum"] = ulong.MinValue;
						Result["maximum"] = ulong.MaxValue;
						break;

					case TypeCode.Single:
					case TypeCode.Double:
					case TypeCode.Decimal:
						Result["type"] = "number";
						break;

					case TypeCode.DateTime:
						{
							Result["type"] = "string";
							Result["format"] = "date-time";
						}
						break;

					case TypeCode.String:
						Result["type"] = "string";
						break;
				}
			}

			if (HasDefault)
				Result["default"] = Default;

			ParameterInfo?.Annotate(Result);

			return Result;
		}
	}
}
