using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using Waher.Networking.HTTP.JsonRpc;
using Waher.Networking.HTTP.Mcp.Model.Attributes;
using Waher.Persistence;
using Waher.Runtime.Collections;
using Waher.Script.Model;
using Waher.Security;

namespace Waher.Networking.HTTP.Mcp.Model.Server
{
	/// <summary>
	/// Contains information about an MCP Server Tool
	/// </summary>
	public class Tool
	{
		private const string McpToolResultTitle = "Result";
		private const string McpToolResultDescription = "Result returned after executing the tool.";

		private readonly JsonRpcMethodInfo methodInfo;
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
		public Tool(MethodInfo Method, string Title, string Description, 
			string IconsMethod, bool CanModifyEnvironment, bool CanDestroyEnvironment, 
			bool Idempotent, bool OpenWorldAccess, 
			params KeyValuePair<string, object>[] MetaData)
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
			this.HasReturnValue = Method.ReturnType != typeof(void);

			this.methodInfo = new JsonRpcMethodInfo(Method, false,
				JsonRpcWebService.GetRequiredPrivileges(Method));

			this.RequiresAuthentication = this.methodInfo.RequiresAuthentication;
			this.RequiredPrivileges = this.methodInfo.RequiredPrivileges;
		}

		/// <summary>
		/// Method to invoke when tool is executed.
		/// </summary>
		public MethodInfo Method { get; }

		/// <summary>
		/// A human-readable title for the tool.
		/// </summary>
		public string Title { get; }

		/// <summary>
		/// A human-readable description of the tool.
		/// 
		/// This can be used by clients to improve the LLM's understanding of available tools. 
		/// It can be thought of like a "hint" to the model.
		/// </summary>
		public string Description { get; }

		/// <summary>
		/// Name of method that returns an <see cref="Icon?"/>, an an <see cref="Icon[]?"/>
		/// or an <see cref="Icons?"/> resource representing the tool. If null or empty, the 
		/// icon of the MCP server will be used.
		/// </summary>
		public string IconsMethod { get; }

		/// <summary>
		/// If the tool can modify the environment. If false, the tool is expected to be 
		/// read-only and not cause any side effects.
		/// </summary>
		public bool CanModifyEnvironment { get; }

		/// <summary>
		/// If true, the tool may perform destructive updates to its environment.
		/// If false, the tool performs only additive updates.
		/// </summary>
		/// <remarks>
		/// This property is meaningful only when <see cref="CanModifyEnvironment"/> is true.
		/// </remarks>
		public bool CanDestroyEnvironment { get; }

		/// <summary>
		/// If true, calling the tool repeatedly with the same arguments
		/// will have no additional effect on its environment.
		/// </summary>
		/// <remarks>
		/// This property is meaningful only when <see cref="CanModifyEnvironment"/> is true.
		/// </remarks>
		public bool Idempotent { get; }

		/// <summary>
		/// If true, this tool may interact with an "open world" of external
		/// entities.If false, the tool's domain of interaction is closed.
		/// </summary>
		public bool OpenWorldAccess { get; }

		/// <summary>
		/// Meta-data associated with tool.
		/// </summary>
		public KeyValuePair<string, object>[] MetaData { get; }

		/// <summary>
		/// If the tool returns a value.
		/// </summary>
		public bool HasReturnValue { get; }

		/// <summary>
		/// If authentication of the user is required.
		/// </summary>
		public bool RequiresAuthentication { get; }

		/// <summary>
		/// Privileges required by the user that calls the method.
		/// </summary>
		public string[] RequiredPrivileges { get; }

		/// <summary>
		/// Checks if a user is authorized to call the method.
		/// </summary>
		/// <param name="User">User to check.</param>
		/// <returns>True if the user is authorized, false otherwise.</returns>
		public bool IsAuthorized(IUser? User)
		{
			return this.methodInfo.IsAuthorized(User);
		}

		/// <summary>
		/// Asserts user is authorized to call the method. If not, a 
		/// <see cref="ForbiddenException"/> is thrown.
		/// </summary>
		/// <param name="ObjectId">Object ID to use in log events.</param>
		/// <param name="User">User accessing method.</param>
		public void AssertAuthorized(string ObjectId, IUser? User)
		{
			this.methodInfo.AssertAuthorized(ObjectId, User);
		}

		/// <summary>
		/// Converts object to a generic representation.
		/// </summary>
		/// <param name="Resource">MCP Server reference.</param>
		/// <returns>Generic representation.</returns>
		public async Task<Dictionary<string, object>> ToJson(HttpMcpServerResource Resource)
		{
			this.icons ??= await GetIcons(Resource, this.IconsMethod);

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
				Result.Add("title", this.Title);

			if (!string.IsNullOrEmpty(this.Title))
				Result.Add("description", this.Description);

			if (!this.icons.Empty)
				Result.Add("icons", this.icons.ToJson());

			if (this.HasReturnValue)
			{
				McpParameterAttribute ReturnInfo = this.Method.ReturnParameter.GetCustomAttribute<McpParameterAttribute>(true);
				IEnumerable<McpEnumValueAttribute>? EnumValues = this.Method.ReturnType.IsEnum ?
					this.Method.ReturnParameter.GetCustomAttributes<McpEnumValueAttribute>(true) : null;

				Result.Add("outputSchema", GenerateOutputSchema(this.Method.ReturnType,
					ReturnInfo, EnumValues));
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

		internal static async Task<Icons> GetIcons(HttpMcpServerResource Resource, string IconsMethod)
		{
			Icons Icons;

			if (string.IsNullOrEmpty(IconsMethod))
				Icons = new Icons();
			else
			{
				MethodInfo? MI = Resource.GetType().GetMethod(IconsMethod,
					BindingFlags.Static | BindingFlags.Instance |
					BindingFlags.Public | BindingFlags.NonPublic);

				if (MI is null)
					Icons = new Icons();
				else
				{
					object? Obj = await ScriptNode.WaitPossibleTask(MI.Invoke(Resource, null));

					if (Obj is Icons Typed)
						Icons = Typed;
					else if (Obj is Icon[] IconArray)
						Icons = new Icons(IconArray);
					else if (Obj is Icon SingleIcon)
						Icons = new Icons(SingleIcon);
					else if (Obj is null)
						Icons = new Icons();
					else
					{
						throw new ArgumentException("Method " + IconsMethod +
							"returned an invalid type: " + Obj.GetType().FullName,
							nameof(IconsMethod));
					}
				}
			}

			if (Icons.Empty)
				Icons = Resource.Icons;

			return Icons;
		}

		/// <summary>
		/// Generates an Input Schema for a method, based on its parameters.
		/// </summary>
		/// <param name="Method">Method information.</param>
		/// <returns>Input Schema</returns>
		private static Dictionary<string, object> GenerateSchema(MethodInfo Method)
		{
			ParameterInfo[] Parameters = Method.GetParameters();
			Dictionary<string, object> Result = new Dictionary<string, object>()
			{
				{ "type", "object" }
			};

			if (Parameters.Length == 0)
				Result["additionalProperties"] = false;
			else
			{
				Dictionary<string, object> Properties = new Dictionary<string, object>();
				ChunkedList<string> Required = new ChunkedList<string>();

				foreach (ParameterInfo Parameter in Parameters)
				{
					Type ParameterType = Parameter.ParameterType;

					if (ParameterType == typeof(HttpRequest) ||
						ParameterType == typeof(HttpResponse))
					{
						continue;
					}

					if (!Parameter.IsOptional && !Parameter.HasDefaultValue)
						Required.Add(Parameter.Name);

					McpParameterAttribute ParameterInfo = Parameter.GetCustomAttribute<McpParameterAttribute>(true);
					IEnumerable<McpEnumValueAttribute>? EnumValues = ParameterType.IsEnum ?
						Parameter.GetCustomAttributes<McpEnumValueAttribute>(true) : null;

					Properties[Parameter.Name] = GenerateSchema(ParameterType,
						Parameter.HasDefaultValue, Parameter.DefaultValue, ParameterInfo,
						EnumValues);
				}

				Result["properties"] = Properties;
				Result["required"] = Required.ToArray();
			}

			return Result;
		}

		private static Dictionary<string, object?> GenerateOutputSchema(Type ReturnType,
			McpParameterAttribute? ParameterInfo, IEnumerable<McpEnumValueAttribute>? EnumValues)
		{
			Dictionary<string, object?> Result = new Dictionary<string, object?>()
			{
				{ "type", "object" },
				{ "result", GenerateSchema(ReturnType, false, null, ParameterInfo, EnumValues) },
				{ "title", McpToolResultTitle },
				{ "description", McpToolResultDescription },
			};

			if (ReturnType == typeof(void))
				Result["required"] = Array.Empty<string>();
			else
				Result["required"] = new string[] { "result" };

			return Result;
		}

		/// <summary>
		/// Generates an Input/Output Schema element for a given type.
		/// </summary>
		/// <param name="T">Type to generate information about.</param>
		/// <param name="HasDefault">Indicates if the type is associated with a default value.</param>
		/// <param name="Default">The default value.</param>
		/// <param name="ParameterInfo">Parameter information.</param>
		/// <param name="EnumValues">Enumeration values.</param>
		/// <returns>Schema element.</returns>
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
						else if (T == typeof(Dictionary<string, object?>))
						{
							Result["type"] = "object";
							//Result["additionalProperties"] = true;// new Dictionary<string, object>();
						}
						else
						{
							if (T.IsGenericType)
							{
								Type GenericType = T.GetGenericTypeDefinition();

								if (GenericType == typeof(Nullable<>) ||
									GenericType == typeof(Task<>))
								{
									return GenerateSchema(T.GenericTypeArguments[0], true, Default,
										ParameterInfo, EnumValues);
								}
							}

							Dictionary<string, object?> Properties = new Dictionary<string, object?>();

							Result["type"] = "object";
							Result["properties"] = Properties;

							foreach (FieldInfo FI in T.GetFields(BindingFlags.Public | BindingFlags.Instance))
							{
								Type FieldType = FI.FieldType;
								McpParameterAttribute FieldInfo = FI.GetCustomAttribute<McpParameterAttribute>(true);
								IEnumerable<McpEnumValueAttribute>? EnumValues2 = FieldType.IsEnum ?
									FI.GetCustomAttributes<McpEnumValueAttribute>(true) : null;

								Properties[FI.Name] = GenerateSchema(FieldType, false, null, FieldInfo, EnumValues2);
							}

							foreach (PropertyInfo PI in T.GetProperties(BindingFlags.Public | BindingFlags.Instance))
							{
								Type PropertyType = PI.PropertyType;
								McpParameterAttribute PropertyInfo = PI.GetCustomAttribute<McpParameterAttribute>(true);
								IEnumerable<McpEnumValueAttribute>? EnumValues2 = PropertyType.IsEnum ?
									PI.GetCustomAttributes<McpEnumValueAttribute>(true) : null;

								Properties[PI.Name] = GenerateSchema(PropertyType, false, null, PropertyInfo, EnumValues2);
							}
						}
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

		/// <summary>
		/// Tries to build a request for the method, based on the provided named parameters.
		/// </summary>
		/// <param name="Parameters">Named parameters.</param>
		/// <param name="Request">HTTP Request object.</param>
		/// <param name="Response">HTTP Response object.</param>
		/// <param name="MetaData">Additional Meta-Data available for the request.</param>
		/// <param name="Reason">Reason for not being able to create request.</param>
		/// <param name="Arguments">Ordered set of typed arguments, to be used in a
		/// call to the method.</param>
		/// <returns>If able to prepare a request to the method.</returns>
		public bool TryBuildRequest(Dictionary<string, object?> Parameters,
			HttpRequest Request, HttpResponse Response,
			Dictionary<string, object?>? MetaData,
			[NotNullWhen(false)] out string? Reason,
			[NotNullWhen(true)] out object?[]? Arguments)
		{
			if (!this.methodInfo.TryBuildRequest(Parameters, MetaData, out Reason, out Arguments))
				return false;

			if (this.methodInfo.RequestArgument.HasValue)
				Arguments[this.methodInfo.RequestArgument.Value] = Request;

			if (this.methodInfo.ResponseArgument.HasValue)
				Arguments[this.methodInfo.ResponseArgument.Value] = Response;

			return true;
		}

	}
}
