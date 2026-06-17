using System;
using System.Collections.Generic;
using Waher.Networking.HTTP.Mcp.Model;

namespace Waher.Networking.HTTP.Mcp
{
	/// <summary>
	/// Defines a method in an <see cref="HttpMcpServerResource"/> implementation as a
	/// recipient of an MCP Server Tool invocation.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class McpServerToolAttribute : Attribute
	{
		/// <summary>
		/// Defines a method in an <see cref="HttpMcpServerResource"/> implementation as a
		/// recipient of an MCP Server Tool invocation.
		/// </summary>
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
		public McpServerToolAttribute(string Title, string Description, string IconsMethod,
			bool CanModifyEnvironment, bool CanDestroyEnvironment, bool Idempotent,
			bool OpenWorldAccess)
			: this(Title, Description, IconsMethod, CanModifyEnvironment, CanDestroyEnvironment,
				  Idempotent, OpenWorldAccess, Array.Empty<KeyValuePair<string, object>>())
		{
		}

		/// <summary>
		/// Defines a method in an <see cref="HttpMcpServerResource"/> implementation as a
		/// recipient of an MCP Server Tool invocation.
		/// </summary>
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
		/// <param name="MetaDataKey1">Meta-data Key 1</param>
		/// <param name="MetaDataValue1">Meta-data Value 1</param>
		public McpServerToolAttribute(string Title, string Description, string IconsMethod,
			bool CanModifyEnvironment, bool CanDestroyEnvironment, bool Idempotent,
			bool OpenWorldAccess, string MetaDataKey1, object MetaDataValue1)
			: this(Title, Description, IconsMethod, CanModifyEnvironment, CanDestroyEnvironment,
				  Idempotent, OpenWorldAccess,
				  new KeyValuePair<string, object>(MetaDataKey1, MetaDataValue1))
		{
		}

		/// <summary>
		/// Defines a method in an <see cref="HttpMcpServerResource"/> implementation as a
		/// recipient of an MCP Server Tool invocation.
		/// </summary>
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
		/// <param name="MetaDataKey1">Meta-data Key 1</param>
		/// <param name="MetaDataValue1">Meta-data Value 1</param>
		/// <param name="MetaDataKey2">Meta-data Key 2</param>
		/// <param name="MetaDataValue2">Meta-data Value 2</param>
		public McpServerToolAttribute(string Title, string Description, string IconsMethod,
			bool CanModifyEnvironment, bool CanDestroyEnvironment, bool Idempotent,
			bool OpenWorldAccess, string MetaDataKey1, object MetaDataValue1,
			string MetaDataKey2, object MetaDataValue2)
			: this(Title, Description, IconsMethod, CanModifyEnvironment, CanDestroyEnvironment,
				  Idempotent, OpenWorldAccess,
				  new KeyValuePair<string, object>(MetaDataKey1, MetaDataValue1),
				  new KeyValuePair<string, object>(MetaDataKey2, MetaDataValue2))
		{
		}

		/// <summary>
		/// Defines a method in an <see cref="HttpMcpServerResource"/> implementation as a
		/// recipient of an MCP Server Tool invocation.
		/// </summary>
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
		/// <param name="MetaDataKey1">Meta-data Key 1</param>
		/// <param name="MetaDataValue1">Meta-data Value 1</param>
		/// <param name="MetaDataKey2">Meta-data Key 2</param>
		/// <param name="MetaDataValue2">Meta-data Value 2</param>
		/// <param name="MetaDataKey3">Meta-data Key 3</param>
		/// <param name="MetaDataValue3">Meta-data Value 3</param>
		public McpServerToolAttribute(string Title, string Description, string IconsMethod,
			bool CanModifyEnvironment, bool CanDestroyEnvironment, bool Idempotent,
			bool OpenWorldAccess, string MetaDataKey1, object MetaDataValue1,
			string MetaDataKey2, object MetaDataValue2,
			string MetaDataKey3, object MetaDataValue3)
			: this(Title, Description, IconsMethod, CanModifyEnvironment, CanDestroyEnvironment,
				  Idempotent, OpenWorldAccess,
				  new KeyValuePair<string, object>(MetaDataKey1, MetaDataValue1),
				  new KeyValuePair<string, object>(MetaDataKey2, MetaDataValue2),
				  new KeyValuePair<string, object>(MetaDataKey3, MetaDataValue3))
		{
		}

		/// <summary>
		/// Defines a method in an <see cref="HttpMcpServerResource"/> implementation as a
		/// recipient of an MCP Server Tool invocation.
		/// </summary>
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
		/// <param name="MetaDataKey1">Meta-data Key 1</param>
		/// <param name="MetaDataValue1">Meta-data Value 1</param>
		/// <param name="MetaDataKey2">Meta-data Key 2</param>
		/// <param name="MetaDataValue2">Meta-data Value 2</param>
		/// <param name="MetaDataKey3">Meta-data Key 3</param>
		/// <param name="MetaDataValue3">Meta-data Value 3</param>
		/// <param name="MetaDataKey4">Meta-data Key 4</param>
		/// <param name="MetaDataValue4">Meta-data Value 4</param>
		public McpServerToolAttribute(string Title, string Description, string IconsMethod,
			bool CanModifyEnvironment, bool CanDestroyEnvironment, bool Idempotent,
			bool OpenWorldAccess, string MetaDataKey1, object MetaDataValue1,
			string MetaDataKey2, object MetaDataValue2,
			string MetaDataKey3, object MetaDataValue3,
			string MetaDataKey4, object MetaDataValue4)
			: this(Title, Description, IconsMethod, CanModifyEnvironment, CanDestroyEnvironment,
				  Idempotent, OpenWorldAccess,
				  new KeyValuePair<string, object>(MetaDataKey1, MetaDataValue1),
				  new KeyValuePair<string, object>(MetaDataKey2, MetaDataValue2),
				  new KeyValuePair<string, object>(MetaDataKey3, MetaDataValue3),
				  new KeyValuePair<string, object>(MetaDataKey4, MetaDataValue4))
		{
		}

		/// <summary>
		/// Defines a method in an <see cref="HttpMcpServerResource"/> implementation as a
		/// recipient of an MCP Server Tool invocation.
		/// </summary>
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
		/// <param name="MetaDataKey1">Meta-data Key 1</param>
		/// <param name="MetaDataValue1">Meta-data Value 1</param>
		/// <param name="MetaDataKey2">Meta-data Key 2</param>
		/// <param name="MetaDataValue2">Meta-data Value 2</param>
		/// <param name="MetaDataKey3">Meta-data Key 3</param>
		/// <param name="MetaDataValue3">Meta-data Value 3</param>
		/// <param name="MetaDataKey4">Meta-data Key 4</param>
		/// <param name="MetaDataValue4">Meta-data Value 4</param>
		/// <param name="MetaDataKey5">Meta-data Key 5</param>
		/// <param name="MetaDataValue5">Meta-data Value 5</param>
		public McpServerToolAttribute(string Title, string Description, string IconsMethod,
			bool CanModifyEnvironment, bool CanDestroyEnvironment, bool Idempotent,
			bool OpenWorldAccess, string MetaDataKey1, object MetaDataValue1,
			string MetaDataKey2, object MetaDataValue2,
			string MetaDataKey3, object MetaDataValue3,
			string MetaDataKey4, object MetaDataValue4,
			string MetaDataKey5, object MetaDataValue5)
			: this(Title, Description, IconsMethod, CanModifyEnvironment, CanDestroyEnvironment,
				  Idempotent, OpenWorldAccess,
				  new KeyValuePair<string, object>(MetaDataKey1, MetaDataValue1),
				  new KeyValuePair<string, object>(MetaDataKey2, MetaDataValue2),
				  new KeyValuePair<string, object>(MetaDataKey3, MetaDataValue3),
				  new KeyValuePair<string, object>(MetaDataKey4, MetaDataValue4),
				  new KeyValuePair<string, object>(MetaDataKey5, MetaDataValue5))
		{
		}

		/// <summary>
		/// Defines a method in an <see cref="HttpMcpServerResource"/> implementation as a
		/// recipient of an MCP Server Tool invocation.
		/// </summary>
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
		public McpServerToolAttribute(string Title, string Description, string IconsMethod,
			bool CanModifyEnvironment, bool CanDestroyEnvironment, bool Idempotent,
			bool OpenWorldAccess, params KeyValuePair<string, object>[] MetaData)
		{
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
	}
}
