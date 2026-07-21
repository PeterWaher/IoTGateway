using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Networking.HTTP.Mcp.Model.Server;

namespace Waher.Networking.HTTP.Mcp.Model.Resources
{
	/// <summary>
	/// Contains information about an MCP Server Text Resource
	/// </summary>
	public class TextResource : Resource
	{
		private readonly Func<Task<string>> read;

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
		public TextResource(string Name, string Title, string Description, Uri Uri,
			Func<Task<string>> Read, params KeyValuePair<string, object>[] MetaData)
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
		public TextResource(string Name, string Title, string Description, Uri Uri,
			Func<Task<string>> Read, string? RequiredPrivilege,
			params KeyValuePair<string, object>[] MetaData)
			: base(Name, Title, Description, Uri, RequiredPrivilege, MetaData)
		{
			this.read = Read;
		}

		/// <summary>
		/// Contains information about an MCP Server Text Resource
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
		/// <param name="ContentType">Content-Type of resource, if known.</param>
		/// <param name="MetaData">Meta-data associated with resource.</param>
		public TextResource(string Name, string Title, string Description, Uri Uri,
			Func<Task<string>> Read, string? RequiredPrivilege, string? ContentType,
			params KeyValuePair<string, object>[] MetaData)
			: base(Name, Title, Description, Uri, RequiredPrivilege, ContentType, MetaData)
		{
			this.read = Read;
		}

		/// <summary>
		/// Contains information about an MCP Server Text Resource
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
		/// <param name="ContentType">Content-Type of resource, if known.</param>
		/// <param name="Size">The size of the raw resource content, in bytes (i.e., before 
		/// base64 encoding or any tokenization), if known. This can be used by Hosts to 
		/// display file sizes and estimate context window usage.</param>
		/// <param name="MetaData">Meta-data associated with resource.</param>
		public TextResource(string Name, string Title, string Description, Uri Uri,
			Func<Task<string>> Read, string? RequiredPrivilege, string? ContentType,
			long? Size, params KeyValuePair<string, object>[] MetaData)
			: base(Name, Title, Description, Uri, RequiredPrivilege, ContentType, Size,
				  MetaData)
		{
			this.read = Read;
		}

		/// <summary>
		/// Contains information about an MCP Server Text Resource
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
		/// <param name="ContentType">Content-Type of resource, if known.</param>
		/// <param name="Size">The size of the raw resource content, in bytes (i.e., before 
		/// base64 encoding or any tokenization), if known. This can be used by Hosts to 
		/// display file sizes and estimate context window usage.</param>
		/// <param name="Icon">Icon associated with the resource.</param>
		/// <param name="MetaData">Meta-data associated with resource.</param>
		public TextResource(string Name, string Title, string Description, Uri Uri,
			Func<Task<string>> Read, string? RequiredPrivilege, string? ContentType,
			long? Size, Icon Icon, params KeyValuePair<string, object>[] MetaData)
			: base(Name, Title, Description, Uri, RequiredPrivilege, ContentType, Size,
				  Icon, MetaData)
		{
			this.read = Read;
		}

		/// <summary>
		/// Contains information about an MCP Server Text Resource
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
		/// <param name="ContentType">Content-Type of resource, if known.</param>
		/// <param name="Size">The size of the raw resource content, in bytes (i.e., before 
		/// base64 encoding or any tokenization), if known. This can be used by Hosts to 
		/// display file sizes and estimate context window usage.</param>
		/// <param name="Icons">Icons associated with the resource.</param>
		/// <param name="MetaData">Meta-data associated with resource.</param>
		public TextResource(string Name, string Title, string Description, Uri Uri,
			Func<Task<string>> Read, string? RequiredPrivilege, string? ContentType,
			long? Size, Icons? Icons, params KeyValuePair<string, object>[] MetaData)
			: base(Name, Title, Description, Uri, RequiredPrivilege, ContentType, Size,
				  Icons, MetaData)
		{
			this.read = Read;
		}

		/// <summary>
		/// Contains information about an MCP Server Text Resource
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
		/// <param name="ContentType">Content-Type of resource, if known.</param>
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
		public TextResource(string Name, string Title, string Description, Uri Uri,
			Func<Task<string>> Read, string? RequiredPrivilege, string? ContentType,
			long? Size, Icons? Icons, McpRole[]? Audience, double? Priority,
			DateTime? LastModified, params KeyValuePair<string, object>[] MetaData)
			: base(Name, Title, Description, Uri, RequiredPrivilege, ContentType, Size,
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
			string s = await this.read();

			return new IResourceContent[]
			{
				new TextContent(this.Uri, s, this.ContentType, MetaData)
			};
		}
	}
}
