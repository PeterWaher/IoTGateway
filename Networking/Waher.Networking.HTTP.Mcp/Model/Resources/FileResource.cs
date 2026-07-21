using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Binary;
using Waher.Networking.HTTP.Mcp.Model.Server;
using Waher.Runtime.IO;

namespace Waher.Networking.HTTP.Mcp.Model.Resources
{
	/// <summary>
	/// Contains information about an MCP Server File Resource
	/// </summary>
	public class FileResource : Resource
	{
		private readonly string fileName;
		private readonly bool isTextFile;

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
		/// <param name="FileName">Full path of file.</param>
		/// <param name="MetaData">Meta-data associated with resource.</param>
		public FileResource(string Name, string Title, string Description, Uri Uri,
			string FileName, params KeyValuePair<string, object>[] MetaData)
			: base(Name, Title, Description, Uri, null, GetContentType(FileName), MetaData)
		{
			this.fileName = FileName;
			this.isTextFile = this.IsTextFile();
		}

		private static string GetContentType(string FileName)
		{
			if (InternetContent.TryGetContentType(Path.GetExtension(FileName),
				out string? ContentType))
			{
				return ContentType;
			}
			else
				return BinaryCodec.DefaultContentType;
		}

		private bool IsTextFile()
		{
			if (this.ContentType is null)
				return false;
			else if (this.ContentType.StartsWith("text/"))
				return true;
			else if (this.ContentType.StartsWith("application/"))
			{
				string s = this.ContentType;
				int i = s.IndexOf(';', 12);
				if (i > 0)
					s = s[..i].TrimEnd();

				if (s.EndsWith("xml") ||
					s.EndsWith("json") ||
					s.EndsWith("script") ||
					s.EndsWith("tlv") ||
					s.EndsWith("turtle") ||
					s.EndsWith("latex"))
				{
					return true;
				}
				else
				{
					switch (s)
					{
						case "application/x-www-form-urlencoded":
						case "application/link-format":
						case "application/jwt":
						case "application/sparql-query":
							return true;

						default:
							return false;
					}
				}
			}
			else
				return false;
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
		/// <param name="FileName">Full path of file.</param>
		/// <param name="RequiredPrivilege">Privilege required to access the resource, 
		/// if any. If none provided, only default privileges for accessing the MCP server
		/// are required.</param>
		/// <param name="MetaData">Meta-data associated with resource.</param>
		public FileResource(string Name, string Title, string Description, Uri Uri,
			string FileName, string? RequiredPrivilege,
			params KeyValuePair<string, object>[] MetaData)
			: base(Name, Title, Description, Uri, RequiredPrivilege,
				  GetContentType(FileName), MetaData)
		{
			this.fileName = FileName;
			this.isTextFile = this.IsTextFile();
		}

		/// <summary>
		/// Contains information about an MCP Server File Resource
		/// </summary>
		/// <param name="Name">Name of resource.</param>
		/// <param name="Title">A human-readable title for the resource.</param>
		/// <param name="Description">A human-readable description of the resource.
		/// 
		/// This can be used by clients to improve the LLM's understanding of available resources. 
		/// It can be thought of like a "hint" to the model.</param>
		/// <param name="Uri">URI of the resource.</param>
		/// <param name="FileName">Full path of file.</param>
		/// <param name="RequiredPrivilege">Privilege required to access the resource, 
		/// if any. If none provided, only default privileges for accessing the MCP server
		/// are required.</param>
		/// <param name="Size">The size of the raw resource content, in bytes (i.e., before 
		/// base64 encoding or any tokenization), if known. This can be used by Hosts to 
		/// display file sizes and estimate context window usage.</param>
		/// <param name="MetaData">Meta-data associated with resource.</param>
		public FileResource(string Name, string Title, string Description, Uri Uri,
			string FileName, string? RequiredPrivilege, long? Size,
			params KeyValuePair<string, object>[] MetaData)
			: base(Name, Title, Description, Uri, RequiredPrivilege,
				  GetContentType(FileName), Size, MetaData)
		{
			this.fileName = FileName;
			this.isTextFile = this.IsTextFile();
		}

		/// <summary>
		/// Contains information about an MCP Server File Resource
		/// </summary>
		/// <param name="Name">Name of resource.</param>
		/// <param name="Title">A human-readable title for the resource.</param>
		/// <param name="Description">A human-readable description of the resource.
		/// 
		/// This can be used by clients to improve the LLM's understanding of available resources. 
		/// It can be thought of like a "hint" to the model.</param>
		/// <param name="Uri">URI of the resource.</param>
		/// <param name="FileName">Full path of file.</param>
		/// <param name="RequiredPrivilege">Privilege required to access the resource, 
		/// if any. If none provided, only default privileges for accessing the MCP server
		/// are required.</param>
		/// <param name="Size">The size of the raw resource content, in bytes (i.e., before 
		/// base64 encoding or any tokenization), if known. This can be used by Hosts to 
		/// display file sizes and estimate context window usage.</param>
		/// <param name="Icon">Icon associated with the resource.</param>
		/// <param name="MetaData">Meta-data associated with resource.</param>
		public FileResource(string Name, string Title, string Description, Uri Uri,
			string FileName, string? RequiredPrivilege, long? Size, Icon Icon,
			params KeyValuePair<string, object>[] MetaData)
			: base(Name, Title, Description, Uri, RequiredPrivilege,
				  GetContentType(FileName), Size, Icon, MetaData)
		{
			this.fileName = FileName;
			this.isTextFile = this.IsTextFile();
		}

		/// <summary>
		/// Contains information about an MCP Server File Resource
		/// </summary>
		/// <param name="Name">Name of resource.</param>
		/// <param name="Title">A human-readable title for the resource.</param>
		/// <param name="Description">A human-readable description of the resource.
		/// 
		/// This can be used by clients to improve the LLM's understanding of available resources. 
		/// It can be thought of like a "hint" to the model.</param>
		/// <param name="Uri">URI of the resource.</param>
		/// <param name="FileName">Full path of file.</param>
		/// <param name="RequiredPrivilege">Privilege required to access the resource, 
		/// if any. If none provided, only default privileges for accessing the MCP server
		/// are required.</param>
		/// <param name="Size">The size of the raw resource content, in bytes (i.e., before 
		/// base64 encoding or any tokenization), if known. This can be used by Hosts to 
		/// display file sizes and estimate context window usage.</param>
		/// <param name="Icons">Icons associated with the resource.</param>
		/// <param name="MetaData">Meta-data associated with resource.</param>
		public FileResource(string Name, string Title, string Description, Uri Uri,
			string FileName, string? RequiredPrivilege, long? Size, Icons? Icons,
			params KeyValuePair<string, object>[] MetaData)
			: base(Name, Title, Description, Uri, RequiredPrivilege,
				  GetContentType(FileName), Size, Icons, MetaData)
		{
			this.fileName = FileName;
			this.isTextFile = this.IsTextFile();
		}

		/// <summary>
		/// Contains information about an MCP Server File Resource
		/// </summary>
		/// <param name="Name">Name of resource.</param>
		/// <param name="Title">A human-readable title for the resource.</param>
		/// <param name="Description">A human-readable description of the resource.
		/// 
		/// This can be used by clients to improve the LLM's understanding of available resources. 
		/// It can be thought of like a "hint" to the model.</param>
		/// <param name="Uri">URI of the resource.</param>
		/// <param name="FileName">Full path of file.</param>
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
		public FileResource(string Name, string Title, string Description, Uri Uri,
			string FileName, string? RequiredPrivilege, long? Size, Icons? Icons,
			McpRole[]? Audience, double? Priority, DateTime? LastModified,
			params KeyValuePair<string, object>[] MetaData)
			: base(Name, Title, Description, Uri, RequiredPrivilege,
				  GetContentType(FileName), Size, Icons, Audience, Priority, LastModified,
				  MetaData)
		{
			this.fileName = FileName;
			this.isTextFile = this.IsTextFile();
		}

		/// <summary>
		/// Reads the resource.
		/// </summary>
		/// <param name="MetaData">Associated meta-data, if available.</param>
		/// <returns>Content objects read.</returns>
		public override async Task<IResourceContent[]> Read(
			Dictionary<string, object>? MetaData)
		{
			if (this.isTextFile)
			{
				string s = await Files.ReadAllTextAsync(this.fileName);

				return new IResourceContent[]
				{
					new TextContent(this.Uri, s, this.ContentType, MetaData)
				};
			}
			else
			{
				byte[] Bin = await Files.ReadAllBytesAsync(this.fileName);

				return new IResourceContent[]
				{
					new BlobContent(this.Uri, Bin,
						this.ContentType ?? BinaryCodec.DefaultContentType, MetaData)
				};
			}
		}
	}
}
