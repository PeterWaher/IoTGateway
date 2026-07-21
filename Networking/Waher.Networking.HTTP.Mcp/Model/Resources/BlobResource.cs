using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content.Binary;
using Waher.Networking.HTTP.Mcp.Model.Server;

namespace Waher.Networking.HTTP.Mcp.Model.Resources
{
	/// <summary>
	/// Contains information about an MCP Server BLOB Resource
	/// </summary>
	public class BlobResource : Resource
	{
		private readonly Func<Task<CustomEncoding>>? readCustom;
		private readonly Func<Task<byte[]>>? readBinary;

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
		public BlobResource(string Name, string Title, string Description, Uri Uri,
			Func<Task<CustomEncoding>> Read, params KeyValuePair<string, object>[] MetaData)
			: base(Name, Title, Description, Uri, MetaData)
		{
			this.readCustom = Read;
			this.readBinary = null;
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
		public BlobResource(string Name, string Title, string Description, Uri Uri,
			Func<Task<CustomEncoding>> Read, string? RequiredPrivilege,
			params KeyValuePair<string, object>[] MetaData)
			: base(Name, Title, Description, Uri, RequiredPrivilege, MetaData)
		{
			this.readCustom = Read;
			this.readBinary = null;
		}

		/// <summary>
		/// Contains information about an MCP Server BLOB Resource
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
		public BlobResource(string Name, string Title, string Description, Uri Uri,
			Func<Task<CustomEncoding>> Read, string? RequiredPrivilege, long? Size, 
			params KeyValuePair<string, object>[] MetaData)
			: base(Name, Title, Description, Uri, RequiredPrivilege, null, Size,
				  MetaData)
		{
			this.readCustom = Read;
			this.readBinary = null;
		}

		/// <summary>
		/// Contains information about an MCP Server BLOB Resource
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
		public BlobResource(string Name, string Title, string Description, Uri Uri,
			Func<Task<CustomEncoding>> Read, string? RequiredPrivilege, long? Size, 
			Icon Icon, params KeyValuePair<string, object>[] MetaData)
			: base(Name, Title, Description, Uri, RequiredPrivilege, null, Size,
				  Icon, MetaData)
		{
			this.readCustom = Read;
			this.readBinary = null;
		}

		/// <summary>
		/// Contains information about an MCP Server BLOB Resource
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
		public BlobResource(string Name, string Title, string Description, Uri Uri,
			Func<Task<CustomEncoding>> Read, string? RequiredPrivilege, long? Size, 
			Icons? Icons, params KeyValuePair<string, object>[] MetaData)
			: base(Name, Title, Description, Uri, RequiredPrivilege, null, Size,
				  Icons, MetaData)
		{
			this.readCustom = Read;
			this.readBinary = null;
		}

		/// <summary>
		/// Contains information about an MCP Server BLOB Resource
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
		public BlobResource(string Name, string Title, string Description, Uri Uri,
			Func<Task<CustomEncoding>> Read, string? RequiredPrivilege, long? Size, 
			Icons? Icons, McpRole[]? Audience, double? Priority, DateTime? LastModified, 
			params KeyValuePair<string, object>[] MetaData)
			: base(Name, Title, Description, Uri, RequiredPrivilege, null, Size,
				  Icons, Audience, Priority, LastModified, MetaData)
		{
			this.readCustom = Read;
			this.readBinary = null;
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
		/// <param name="MetaData">Meta-data associated with resource.</param>
		public BlobResource(string Name, string Title, string Description, Uri Uri,
			Func<Task<byte[]>> Read, params KeyValuePair<string, object>[] MetaData)
			: base(Name, Title, Description, Uri, MetaData)
		{
			this.readCustom = null;
			this.readBinary = Read;
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
		public BlobResource(string Name, string Title, string Description, Uri Uri,
			Func<Task<byte[]>> Read, string? RequiredPrivilege,
			params KeyValuePair<string, object>[] MetaData)
			: base(Name, Title, Description, Uri, RequiredPrivilege, MetaData)
		{
			this.readCustom = null;
			this.readBinary = Read;
		}

		/// <summary>
		/// Contains information about an MCP Server BLOB Resource
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
		public BlobResource(string Name, string Title, string Description, Uri Uri,
			Func<Task<byte[]>> Read, string? RequiredPrivilege, string? ContentType,
			params KeyValuePair<string, object>[] MetaData)
			: base(Name, Title, Description, Uri, RequiredPrivilege, ContentType, MetaData)
		{
			this.readCustom = null;
			this.readBinary = Read;
		}

		/// <summary>
		/// Contains information about an MCP Server BLOB Resource
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
		public BlobResource(string Name, string Title, string Description, Uri Uri,
			Func<Task<byte[]>> Read, string? RequiredPrivilege, string? ContentType,
			long? Size, params KeyValuePair<string, object>[] MetaData)
			: base(Name, Title, Description, Uri, RequiredPrivilege, ContentType, Size,
				  MetaData)
		{
			this.readCustom = null;
			this.readBinary = Read;
		}

		/// <summary>
		/// Contains information about an MCP Server BLOB Resource
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
		public BlobResource(string Name, string Title, string Description, Uri Uri,
			Func<Task<byte[]>> Read, string? RequiredPrivilege, string? ContentType,
			long? Size, Icon Icon, params KeyValuePair<string, object>[] MetaData)
			: base(Name, Title, Description, Uri, RequiredPrivilege, ContentType, Size,
				  Icon, MetaData)
		{
			this.readCustom = null;
			this.readBinary = Read;
		}

		/// <summary>
		/// Contains information about an MCP Server BLOB Resource
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
		public BlobResource(string Name, string Title, string Description, Uri Uri,
			Func<Task<byte[]>> Read, string? RequiredPrivilege, string? ContentType,
			long? Size, Icons? Icons, params KeyValuePair<string, object>[] MetaData)
			: base(Name, Title, Description, Uri, RequiredPrivilege, ContentType, Size,
				  Icons, MetaData)
		{
			this.readCustom = null;
			this.readBinary = Read;
		}

		/// <summary>
		/// Contains information about an MCP Server BLOB Resource
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
		public BlobResource(string Name, string Title, string Description, Uri Uri,
			Func<Task<byte[]>> Read, string? RequiredPrivilege, string? ContentType,
			long? Size, Icons? Icons, McpRole[]? Audience, double? Priority,
			DateTime? LastModified, params KeyValuePair<string, object>[] MetaData)
			: base(Name, Title, Description, Uri, RequiredPrivilege, ContentType, Size,
				  Icons, Audience, Priority, LastModified, MetaData)
		{
			this.readCustom = null;
			this.readBinary = Read;
		}


		/// <summary>
		/// Reads the resource.
		/// </summary>
		/// <param name="MetaData">Associated meta-data, if available.</param>
		/// <returns>Content objects read.</returns>
		public override async Task<IResourceContent[]> Read(
			Dictionary<string, object>? MetaData)
		{
			BlobContent Content;

			if (this.readBinary is null)
			{
				CustomEncoding Encoded = await this.readCustom!();
				Content = new BlobContent(this.Uri, Encoded.Encoded, Encoded.ContentType, 
					MetaData);
			}
			else
			{
				byte[] Encoded = await this.readBinary();
				Content = new BlobContent(this.Uri, Encoded, 
					this.ContentType ?? BinaryCodec.DefaultContentType, MetaData);
			}

			return new IResourceContent[]
			{
				Content
			};
		}
	}
}
