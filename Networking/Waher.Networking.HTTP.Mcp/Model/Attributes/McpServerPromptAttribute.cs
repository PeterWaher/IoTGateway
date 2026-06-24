using System;
using System.Collections.Generic;

namespace Waher.Networking.HTTP.Mcp.Model.Attributes
{
	/// <summary>
	/// Defines a method in an <see cref="HttpMcpServerResource"/> implementation as a
	/// recipient of an MCP Server Prompt invocation.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class McpServerPromptAttribute : Attribute
	{
		/// <summary>
		/// Defines a method in an <see cref="HttpMcpServerResource"/> implementation as a
		/// recipient of an MCP Server Prompt invocation.
		/// </summary>
		/// <param name="Title">A human-readable title for the prompt.</param>
		/// <param name="Description">A human-readable description of the prompt.
		/// 
		/// This can be used by clients to improve the LLM's understanding of available prompts. 
		/// It can be thought of like a "hint" to the model.</param>
		/// <param name="IconsMethod">Name of method that returns an <see cref="Icon?"/>, 
		/// an an <see cref="Icon[]?"/> or an <see cref="Icons?"/> resource representing 
		/// the prompt. If null or empty, the icon of the MCP server will be used.</param>
		public McpServerPromptAttribute(string Title, string Description, string IconsMethod)
			: this(Title, Description, IconsMethod, Array.Empty<KeyValuePair<string, object>>())
		{
		}

		/// <summary>
		/// Defines a method in an <see cref="HttpMcpServerResource"/> implementation as a
		/// recipient of an MCP Server Prompt invocation.
		/// </summary>
		/// <param name="Title">A human-readable title for the prompt.</param>
		/// <param name="Description">A human-readable description of the prompt.
		/// 
		/// This can be used by clients to improve the LLM's understanding of available prompts. 
		/// It can be thought of like a "hint" to the model.</param>
		/// <param name="IconsMethod">Name of method that returns an <see cref="Icon?"/>, 
		/// an an <see cref="Icon[]?"/> or an <see cref="Icons?"/> resource representing 
		/// the prompt. If null or empty, the icon of the MCP server will be used.</param>
		/// <param name="MetaDataKey1">Meta-data Key 1</param>
		/// <param name="MetaDataValue1">Meta-data Value 1</param>
		public McpServerPromptAttribute(string Title, string Description, string IconsMethod,
			string MetaDataKey1, object MetaDataValue1)
			: this(Title, Description, IconsMethod, 
				  new KeyValuePair<string, object>(MetaDataKey1, MetaDataValue1))
		{
		}

		/// <summary>
		/// Defines a method in an <see cref="HttpMcpServerResource"/> implementation as a
		/// recipient of an MCP Server Prompt invocation.
		/// </summary>
		/// <param name="Title">A human-readable title for the prompt.</param>
		/// <param name="Description">A human-readable description of the prompt.
		/// 
		/// This can be used by clients to improve the LLM's understanding of available prompts. 
		/// It can be thought of like a "hint" to the model.</param>
		/// <param name="IconsMethod">Name of method that returns an <see cref="Icon?"/>, 
		/// an an <see cref="Icon[]?"/> or an <see cref="Icons?"/> resource representing 
		/// the prompt. If null or empty, the icon of the MCP server will be used.</param>
		/// <param name="MetaDataKey1">Meta-data Key 1</param>
		/// <param name="MetaDataValue1">Meta-data Value 1</param>
		/// <param name="MetaDataKey2">Meta-data Key 2</param>
		/// <param name="MetaDataValue2">Meta-data Value 2</param>
		public McpServerPromptAttribute(string Title, string Description, string IconsMethod,
			string MetaDataKey1, object MetaDataValue1,
			string MetaDataKey2, object MetaDataValue2)
			: this(Title, Description, IconsMethod, 
				  new KeyValuePair<string, object>(MetaDataKey1, MetaDataValue1),
				  new KeyValuePair<string, object>(MetaDataKey2, MetaDataValue2))
		{
		}

		/// <summary>
		/// Defines a method in an <see cref="HttpMcpServerResource"/> implementation as a
		/// recipient of an MCP Server Prompt invocation.
		/// </summary>
		/// <param name="Title">A human-readable title for the prompt.</param>
		/// <param name="Description">A human-readable description of the prompt.
		/// 
		/// This can be used by clients to improve the LLM's understanding of available prompts. 
		/// It can be thought of like a "hint" to the model.</param>
		/// <param name="IconsMethod">Name of method that returns an <see cref="Icon?"/>, 
		/// an an <see cref="Icon[]?"/> or an <see cref="Icons?"/> resource representing 
		/// the prompt. If null or empty, the icon of the MCP server will be used.</param>
		/// <param name="MetaDataKey1">Meta-data Key 1</param>
		/// <param name="MetaDataValue1">Meta-data Value 1</param>
		/// <param name="MetaDataKey2">Meta-data Key 2</param>
		/// <param name="MetaDataValue2">Meta-data Value 2</param>
		/// <param name="MetaDataKey3">Meta-data Key 3</param>
		/// <param name="MetaDataValue3">Meta-data Value 3</param>
		public McpServerPromptAttribute(string Title, string Description, string IconsMethod,
			string MetaDataKey1, object MetaDataValue1,
			string MetaDataKey2, object MetaDataValue2,
			string MetaDataKey3, object MetaDataValue3)
			: this(Title, Description, IconsMethod, 
				  new KeyValuePair<string, object>(MetaDataKey1, MetaDataValue1),
				  new KeyValuePair<string, object>(MetaDataKey2, MetaDataValue2),
				  new KeyValuePair<string, object>(MetaDataKey3, MetaDataValue3))
		{
		}

		/// <summary>
		/// Defines a method in an <see cref="HttpMcpServerResource"/> implementation as a
		/// recipient of an MCP Server Prompt invocation.
		/// </summary>
		/// <param name="Title">A human-readable title for the prompt.</param>
		/// <param name="Description">A human-readable description of the prompt.
		/// 
		/// This can be used by clients to improve the LLM's understanding of available prompts. 
		/// It can be thought of like a "hint" to the model.</param>
		/// <param name="IconsMethod">Name of method that returns an <see cref="Icon?"/>, 
		/// an an <see cref="Icon[]?"/> or an <see cref="Icons?"/> resource representing 
		/// the prompt. If null or empty, the icon of the MCP server will be used.</param>
		/// <param name="MetaDataKey1">Meta-data Key 1</param>
		/// <param name="MetaDataValue1">Meta-data Value 1</param>
		/// <param name="MetaDataKey2">Meta-data Key 2</param>
		/// <param name="MetaDataValue2">Meta-data Value 2</param>
		/// <param name="MetaDataKey3">Meta-data Key 3</param>
		/// <param name="MetaDataValue3">Meta-data Value 3</param>
		/// <param name="MetaDataKey4">Meta-data Key 4</param>
		/// <param name="MetaDataValue4">Meta-data Value 4</param>
		public McpServerPromptAttribute(string Title, string Description, string IconsMethod,
			string MetaDataKey1, object MetaDataValue1,
			string MetaDataKey2, object MetaDataValue2,
			string MetaDataKey3, object MetaDataValue3,
			string MetaDataKey4, object MetaDataValue4)
			: this(Title, Description, IconsMethod, 
				  new KeyValuePair<string, object>(MetaDataKey1, MetaDataValue1),
				  new KeyValuePair<string, object>(MetaDataKey2, MetaDataValue2),
				  new KeyValuePair<string, object>(MetaDataKey3, MetaDataValue3),
				  new KeyValuePair<string, object>(MetaDataKey4, MetaDataValue4))
		{
		}

		/// <summary>
		/// Defines a method in an <see cref="HttpMcpServerResource"/> implementation as a
		/// recipient of an MCP Server Prompt invocation.
		/// </summary>
		/// <param name="Title">A human-readable title for the prompt.</param>
		/// <param name="Description">A human-readable description of the prompt.
		/// 
		/// This can be used by clients to improve the LLM's understanding of available prompts. 
		/// It can be thought of like a "hint" to the model.</param>
		/// <param name="IconsMethod">Name of method that returns an <see cref="Icon?"/>, 
		/// an an <see cref="Icon[]?"/> or an <see cref="Icons?"/> resource representing 
		/// the prompt. If null or empty, the icon of the MCP server will be used.</param>
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
		public McpServerPromptAttribute(string Title, string Description, string IconsMethod,
			string MetaDataKey1, object MetaDataValue1,
			string MetaDataKey2, object MetaDataValue2,
			string MetaDataKey3, object MetaDataValue3,
			string MetaDataKey4, object MetaDataValue4,
			string MetaDataKey5, object MetaDataValue5)
			: this(Title, Description, IconsMethod, 
				  new KeyValuePair<string, object>(MetaDataKey1, MetaDataValue1),
				  new KeyValuePair<string, object>(MetaDataKey2, MetaDataValue2),
				  new KeyValuePair<string, object>(MetaDataKey3, MetaDataValue3),
				  new KeyValuePair<string, object>(MetaDataKey4, MetaDataValue4),
				  new KeyValuePair<string, object>(MetaDataKey5, MetaDataValue5))
		{
		}

		/// <summary>
		/// Defines a method in an <see cref="HttpMcpServerResource"/> implementation as a
		/// recipient of an MCP Server Prompt invocation.
		/// </summary>
		/// <param name="Title">A human-readable title for the prompt.</param>
		/// <param name="Description">A human-readable description of the prompt.
		/// 
		/// This can be used by clients to improve the LLM's understanding of available prompts. 
		/// It can be thought of like a "hint" to the model.</param>
		/// <param name="IconsMethod">Name of method that returns an <see cref="Icon?"/>, 
		/// an an <see cref="Icon[]?"/> or an <see cref="Icons?"/> resource representing 
		/// the prompt. If null or empty, the icon of the MCP server will be used.</param>
		/// <param name="MetaData">Meta-data associated with prompt.</param>
		public McpServerPromptAttribute(string Title, string Description, string IconsMethod,
			params KeyValuePair<string, object>[] MetaData)
		{
			this.Title = Title;
			this.Description = Description;
			this.IconsMethod = IconsMethod;
			this.MetaData = MetaData;
		}

		/// <summary>
		/// A human-readable title for the prompt.
		/// </summary>
		public string Title { get; }

		/// <summary>
		/// A human-readable description of the prompt.
		/// </summary>
		public string Description { get; }

		/// <summary>
		/// Name of method that returns an <see cref="Icon?"/>, an an <see cref="Icon[]?"/>
		/// or an <see cref="Icons?"/> resource representing the prompt. If null or empty, the 
		/// icon of the MCP server will be used.
		/// </summary>
		public string IconsMethod { get; }

		/// <summary>
		/// Meta-data associated with prompt.
		/// </summary>
		public KeyValuePair<string, object>[] MetaData { get; }
	}
}
