using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content.Toon;
using Waher.Networking.HTTP.Mcp.Model.Server;

namespace Waher.Networking.HTTP.Mcp.Model.Resources
{
	/// <summary>
	/// Contains information about an MCP Server JSON Resource
	/// </summary>
	public class ToonResource : Resource
	{
		private readonly Func<Task<object?>> read;

		/// <summary>
		/// Contains information about an MCP Text Server Resource
		/// </summary>
		/// <param name="Name">Name of resource.</param>
		/// <param name="Title">A human-readable title for the resource.</param>
		/// <param name="Description">A human-readable description of the resource.
		/// 
		/// This can be used by clients to improve the LLM's understanding of available resources. 
		/// It can be thought of like a "hint" to the model.</param>
		/// <param name="Uri">URI of the resource.</param>
		/// <param name="Read">Method called when resource is read.</param>
		/// <param name="MetaData">Meta-data associated with resource.</param>
		public ToonResource(string Name, string Title, string Description, Uri Uri,
			Func<Task<object?>> Read, params KeyValuePair<string, object>[] MetaData)
			: base(Name, Title, Description, Uri, MetaData)
		{
			this.read = Read;
		}

		/// <summary>
		/// Contains information about an MCP Text Server Resource
		/// </summary>
		/// <param name="Name">Name of resource.</param>
		/// <param name="Title">A human-readable title for the resource.</param>
		/// <param name="Description">A human-readable description of the resource.
		/// 
		/// This can be used by clients to improve the LLM's understanding of available resources. 
		/// It can be thought of like a "hint" to the model.</param>
		/// <param name="Uri">URI of the resource.</param>
		/// <param name="Read">Method called when resource is read.</param>
		/// <param name="RequiredPrivilege">Privilege required to access the resource, 
		/// if any. If none provided, only default privileges for accessing the MCP server
		/// are required.</param>
		/// <param name="MetaData">Meta-data associated with resource.</param>
		public ToonResource(string Name, string Title, string Description, Uri Uri,
			Func<Task<object?>> Read, string? RequiredPrivilege,
			params KeyValuePair<string, object>[] MetaData)
			: base(Name, Title, Description, Uri, RequiredPrivilege, MetaData)
		{
			this.read = Read;
		}

		/// <summary>
		/// Contains information about an MCP Server JSON Resource
		/// </summary>
		/// <param name="Name">Name of resource.</param>
		/// <param name="Title">A human-readable title for the resource.</param>
		/// <param name="Description">A human-readable description of the resource.
		/// 
		/// This can be used by clients to improve the LLM's understanding of available resources. 
		/// It can be thought of like a "hint" to the model.</param>
		/// <param name="Uri">URI of the resource.</param>
		/// <param name="Read">Method called when resource is read.</param>
		/// <param name="RequiredPrivilege">Privilege required to access the resource, 
		/// if any. If none provided, only default privileges for accessing the MCP server
		/// are required.</param>
		/// <param name="Size">The size of the raw resource content, in bytes (i.e., before 
		/// base64 encoding or any tokenization), if known. This can be used by Hosts to 
		/// display file sizes and estimate context window usage.</param>
		/// <param name="MetaData">Meta-data associated with resource.</param>
		public ToonResource(string Name, string Title, string Description, Uri Uri,
			Func<Task<object?>> Read, string? RequiredPrivilege, long? Size, 
			params KeyValuePair<string, object>[] MetaData)
			: base(Name, Title, Description, Uri, RequiredPrivilege, null, Size,
				  MetaData)
		{
			this.read = Read;
		}

		/// <summary>
		/// Contains information about an MCP Server JSON Resource
		/// </summary>
		/// <param name="Name">Name of resource.</param>
		/// <param name="Title">A human-readable title for the resource.</param>
		/// <param name="Description">A human-readable description of the resource.
		/// 
		/// This can be used by clients to improve the LLM's understanding of available resources. 
		/// It can be thought of like a "hint" to the model.</param>
		/// <param name="Uri">URI of the resource.</param>
		/// <param name="Read">Method called when resource is read.</param>
		/// <param name="RequiredPrivilege">Privilege required to access the resource, 
		/// if any. If none provided, only default privileges for accessing the MCP server
		/// are required.</param>
		/// <param name="Size">The size of the raw resource content, in bytes (i.e., before 
		/// base64 encoding or any tokenization), if known. This can be used by Hosts to 
		/// display file sizes and estimate context window usage.</param>
		/// <param name="Icon">Icon associated with the resource.</param>
		/// <param name="MetaData">Meta-data associated with resource.</param>
		public ToonResource(string Name, string Title, string Description, Uri Uri,
			Func<Task<object?>> Read, string? RequiredPrivilege, long? Size, 
			Icon Icon, params KeyValuePair<string, object>[] MetaData)
			: base(Name, Title, Description, Uri, RequiredPrivilege, null, Size,
				  Icon, MetaData)
		{
			this.read = Read;
		}

		/// <summary>
		/// Contains information about an MCP Server JSON Resource
		/// </summary>
		/// <param name="Name">Name of resource.</param>
		/// <param name="Title">A human-readable title for the resource.</param>
		/// <param name="Description">A human-readable description of the resource.
		/// 
		/// This can be used by clients to improve the LLM's understanding of available resources. 
		/// It can be thought of like a "hint" to the model.</param>
		/// <param name="Uri">URI of the resource.</param>
		/// <param name="Read">Method called when resource is read.</param>
		/// <param name="RequiredPrivilege">Privilege required to access the resource, 
		/// if any. If none provided, only default privileges for accessing the MCP server
		/// are required.</param>
		/// <param name="Size">The size of the raw resource content, in bytes (i.e., before 
		/// base64 encoding or any tokenization), if known. This can be used by Hosts to 
		/// display file sizes and estimate context window usage.</param>
		/// <param name="Icons">Icons associated with the resource.</param>
		/// <param name="MetaData">Meta-data associated with resource.</param>
		public ToonResource(string Name, string Title, string Description, Uri Uri,
			Func<Task<object?>> Read, string? RequiredPrivilege, long? Size, 
			Icons? Icons, params KeyValuePair<string, object>[] MetaData)
			: base(Name, Title, Description, Uri, RequiredPrivilege, null, Size,
				  Icons, MetaData)
		{
			this.read = Read;
		}

		/// <summary>
		/// Contains information about an MCP Server JSON Resource
		/// </summary>
		/// <param name="Name">Name of resource.</param>
		/// <param name="Title">A human-readable title for the resource.</param>
		/// <param name="Description">A human-readable description of the resource.
		/// 
		/// This can be used by clients to improve the LLM's understanding of available resources. 
		/// It can be thought of like a "hint" to the model.</param>
		/// <param name="Uri">URI of the resource.</param>
		/// <param name="Read">Method called when resource is read.</param>
		/// <param name="RequiredPrivilege">Privilege required to access the resource, 
		/// if any. If none provided, only default privileges for accessing the MCP server
		/// are required.</param>
		/// <param name="Size">The size of the raw resource content, in bytes (i.e., before 
		/// base64 encoding or any tokenization), if known. This can be used by Hosts to 
		/// display file sizes and estimate context window usage.</param>
		/// <param name="Icons">Icons associated with the resource.</param>
		/// <param name="Audience">Describes who the intended audience of this object or 
		/// data is. It can include multiple entries to indicate content useful for multiple 
		/// audiences(e.g., `["user", "assistant"]`).</param>
		/// <param name="Priority">Describes how important this data is for operating the 
		/// server. A value of 1 means "most important," and indicates that the data is
		/// effectively required, while 0 means "least important," and indicates that
		/// the data is entirely optional.</param>
		/// <param name="LastModified">The moment the resource was last modified.
		/// Examples: last activity timestamp in an open file timestamp when the resource 
		/// was attached, etc.</param>
		/// <param name="MetaData">Meta-data associated with resource.</param>
		public ToonResource(string Name, string Title, string Description, Uri Uri,
			Func<Task<object?>> Read, string? RequiredPrivilege, long? Size, 
			Icons? Icons, McpRole[]? Audience, double? Priority, DateTime? LastModified, 
			params KeyValuePair<string, object>[] MetaData)
			: base(Name, Title, Description, Uri, RequiredPrivilege, null, Size,
				  Icons, Audience, Priority, LastModified, MetaData)
		{
			this.read = Read;
		}

		/// <summary>
		/// Reads the resource.
		/// </summary>
		/// <param name="MetaData">Associated meta-data, if available.</param>
		/// <returns>Content objects read.</returns>
		public override async Task<IResourceContent[]> Read(
			Dictionary<string, object>? MetaData)
		{
			object? Result = await this.read();
			string s = TOON.Encode(Result, false);

			return new IResourceContent[]
			{
				new TextContent(this.Uri,s, ToonEncoder.DefaultContentType,MetaData)
			};
		}
	}
}
