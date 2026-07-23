using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Waher.Networking.HTTP.Mcp.Model.Resources;
using Waher.Security;

namespace Waher.Networking.HTTP.Mcp.Model.Server
{
	/// <summary>
	/// Contains information about an MCP Server Resource
	/// </summary>
	public abstract class Resource : Annotations
	{
		/// <summary>
		/// Contains information about an MCP Server Resource
		/// </summary>
		/// <param name="Name">Name of resource.</param>
		/// <param name="Title">A human-readable title for the resource.</param>
		/// <param name="Description">A human-readable description of the resource.
		/// 
		/// This can be used by clients to improve the LLM's understanding of available resources. 
		/// It can be thought of like a "hint" to the model.</param>
		/// <param name="Uri">URI of the resource.</param>
		/// <param name="MetaData">Meta-data associated with resource.</param>
		public Resource(string Name, string Title, string Description, Uri Uri,
			params KeyValuePair<string, object>[] MetaData)
			: this(Name, Title, Description, Uri, null, null, null, null,
				  null, null, null, MetaData)
		{
		}

		/// <summary>
		/// Contains information about an MCP Server Resource
		/// </summary>
		/// <param name="Name">Name of resource.</param>
		/// <param name="Title">A human-readable title for the resource.</param>
		/// <param name="Description">A human-readable description of the resource.
		/// 
		/// This can be used by clients to improve the LLM's understanding of available resources. 
		/// It can be thought of like a "hint" to the model.</param>
		/// <param name="Uri">URI of the resource.</param>
		/// <param name="RequiredPrivilege">Privilege required to access the resource, 
		/// if any. If none provided, only default privileges for accessing the MCP server
		/// are required.</param>
		/// <param name="MetaData">Meta-data associated with resource.</param>
		public Resource(string Name, string Title, string Description, Uri Uri,
			string? RequiredPrivilege, params KeyValuePair<string, object>[] MetaData)
			: this(Name, Title, Description, Uri, RequiredPrivilege, null, null, null, 
				  null, null, null, MetaData)
		{
		}

		/// <summary>
		/// Contains information about an MCP Server Resource
		/// </summary>
		/// <param name="Name">Name of resource.</param>
		/// <param name="Title">A human-readable title for the resource.</param>
		/// <param name="Description">A human-readable description of the resource.
		/// 
		/// This can be used by clients to improve the LLM's understanding of available resources. 
		/// It can be thought of like a "hint" to the model.</param>
		/// <param name="Uri">URI of the resource.</param>
		/// <param name="RequiredPrivilege">Privilege required to access the resource, 
		/// if any. If none provided, only default privileges for accessing the MCP server
		/// are required.</param>
		/// <param name="ContentType">Content-Type of resource, if known.</param>
		/// <param name="MetaData">Meta-data associated with resource.</param>
		public Resource(string Name, string Title, string Description, Uri Uri,
			string? RequiredPrivilege, string? ContentType, 
			params KeyValuePair<string, object>[] MetaData)
			: this(Name, Title, Description, Uri, RequiredPrivilege, ContentType, 
				  null, null, null, null, null, MetaData)
		{
		}

		/// <summary>
		/// Contains information about an MCP Server Resource
		/// </summary>
		/// <param name="Name">Name of resource.</param>
		/// <param name="Title">A human-readable title for the resource.</param>
		/// <param name="Description">A human-readable description of the resource.
		/// 
		/// This can be used by clients to improve the LLM's understanding of available resources. 
		/// It can be thought of like a "hint" to the model.</param>
		/// <param name="Uri">URI of the resource.</param>
		/// <param name="RequiredPrivilege">Privilege required to access the resource, 
		/// if any. If none provided, only default privileges for accessing the MCP server
		/// are required.</param>
		/// <param name="ContentType">Content-Type of resource, if known.</param>
		/// <param name="Size">The size of the raw resource content, in bytes (i.e., before 
		/// base64 encoding or any tokenization), if known. This can be used by Hosts to 
		/// display file sizes and estimate context window usage.</param>
		/// <param name="MetaData">Meta-data associated with resource.</param>
		public Resource(string Name, string Title, string Description, Uri Uri,
			string? RequiredPrivilege, string? ContentType, long? Size, 
			params KeyValuePair<string, object>[] MetaData)
			: this(Name, Title, Description, Uri, RequiredPrivilege, ContentType, Size, 
				  null, null, null, null, MetaData)
		{
		}

		/// <summary>
		/// Contains information about an MCP Server Resource
		/// </summary>
		/// <param name="Name">Name of resource.</param>
		/// <param name="Title">A human-readable title for the resource.</param>
		/// <param name="Description">A human-readable description of the resource.
		/// 
		/// This can be used by clients to improve the LLM's understanding of available resources. 
		/// It can be thought of like a "hint" to the model.</param>
		/// <param name="Uri">URI of the resource.</param>
		/// <param name="RequiredPrivilege">Privilege required to access the resource, 
		/// if any. If none provided, only default privileges for accessing the MCP server
		/// are required.</param>
		/// <param name="ContentType">Content-Type of resource, if known.</param>
		/// <param name="Size">The size of the raw resource content, in bytes (i.e., before 
		/// base64 encoding or any tokenization), if known. This can be used by Hosts to 
		/// display file sizes and estimate context window usage.</param>
		/// <param name="Icon">Icon associated with the resource.</param>
		/// <param name="MetaData">Meta-data associated with resource.</param>
		public Resource(string Name, string Title, string Description, Uri Uri,
			string? RequiredPrivilege, string? ContentType, long? Size, Icon Icon,
			params KeyValuePair<string, object>[] MetaData)
			: this(Name, Title, Description, Uri, RequiredPrivilege, ContentType, Size, 
				  new Icons(Icon), null, null, null, MetaData)
		{
		}

		/// <summary>
		/// Contains information about an MCP Server Resource
		/// </summary>
		/// <param name="Name">Name of resource.</param>
		/// <param name="Title">A human-readable title for the resource.</param>
		/// <param name="Description">A human-readable description of the resource.
		/// 
		/// This can be used by clients to improve the LLM's understanding of available resources. 
		/// It can be thought of like a "hint" to the model.</param>
		/// <param name="Uri">URI of the resource.</param>
		/// <param name="RequiredPrivilege">Privilege required to access the resource, 
		/// if any. If none provided, only default privileges for accessing the MCP server
		/// are required.</param>
		/// <param name="ContentType">Content-Type of resource, if known.</param>
		/// <param name="Size">The size of the raw resource content, in bytes (i.e., before 
		/// base64 encoding or any tokenization), if known. This can be used by Hosts to 
		/// display file sizes and estimate context window usage.</param>
		/// <param name="Icons">Icons associated with the resource.</param>
		/// <param name="MetaData">Meta-data associated with resource.</param>
		public Resource(string Name, string Title, string Description, Uri Uri,
			string? RequiredPrivilege, string? ContentType, long? Size, Icons? Icons, 
			params KeyValuePair<string, object>[] MetaData)
			: this(Name, Title, Description, Uri, RequiredPrivilege, ContentType, 
				  Size, Icons, null, null, null, MetaData)
		{
		}

		/// <summary>
		/// Contains information about an MCP Server Resource
		/// </summary>
		/// <param name="Name">Name of resource.</param>
		/// <param name="Title">A human-readable title for the resource.</param>
		/// <param name="Description">A human-readable description of the resource.
		/// 
		/// This can be used by clients to improve the LLM's understanding of available resources. 
		/// It can be thought of like a "hint" to the model.</param>
		/// <param name="Uri">URI of the resource.</param>
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
		public Resource(string Name, string Title, string Description, Uri Uri,
			string? RequiredPrivilege, string? ContentType, long? Size, Icons? Icons,
			McpRole[]? Audience, double? Priority, DateTime? LastModified,
			params KeyValuePair<string, object>[] MetaData)
			: base(Audience, Priority, LastModified)
		{
			this.Name = Name;
			this.Title = Title;
			this.Description = Description;
			this.Uri = Uri;
			this.RequiredPrivilege = RequiredPrivilege;
			this.ContentType = ContentType;
			this.Size = Size;
			this.Icons = Icons ?? new Icons(HttpMcpServerResource.GetDefaultIcons());
			this.MetaData = MetaData;
		}

		/// <summary>
		/// Name of resource.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// A human-readable title for the prompt.
		/// </summary>
		public string Title { get; }

		/// <summary>
		/// A human-readable description of the prompt.
		/// 
		/// This can be used by clients to improve the LLM's understanding of available prompts. 
		/// It can be thought of like a "hint" to the model.
		/// </summary>
		public string Description { get; }

		/// <summary>
		/// URI of the resource.
		/// </summary>
		public Uri Uri { get; }

		/// <summary>
		/// Privilege required to access the resource, if any. If null, no additional
		/// privilege is required.
		/// </summary>
		public string? RequiredPrivilege { get; }

		/// <summary>
		/// Content-Type of resource, if known.
		/// </summary>
		public string? ContentType { get; }

		/// <summary>
		/// The size of the raw resource content, in bytes (i.e., before base64 encoding 
		/// or any tokenization), if known. This can be used by Hosts to display file sizes 
		/// and estimate context window usage.
		/// </summary>
		public long? Size { get; }

		/// <summary>
		/// Icons of resource.
		/// </summary>
		public Icons Icons { get; }

		/// <summary>
		/// Meta-data associated with prompt.
		/// </summary>
		public KeyValuePair<string, object>[] MetaData { get; }

		/// <summary>
		/// Checks if a user is authorized to call the method.
		/// </summary>
		/// <param name="User">User to check.</param>
		/// <param name="MissingPrivilege">Missing privilege, if not authorized.</param>
		/// <returns>True if the user is authorized, false otherwise.</returns>
		public bool IsAuthorized(IUser? User, [NotNullWhen(false)] out string? MissingPrivilege)
		{
			if (string.IsNullOrEmpty(this.RequiredPrivilege))
			{
				MissingPrivilege = null;
				return true;
			}

			if (!(User?.HasPrivilege(this.RequiredPrivilege) ?? false))
			{
				MissingPrivilege = this.RequiredPrivilege;
				return false;
			}

			MissingPrivilege = null;
			return true;
		}

		/// <summary>
		/// Converts object to a generic representation.
		/// </summary>
		/// <returns>Generic representation.</returns>
		public Dictionary<string, object> ToJson()
		{
			Dictionary<string, object> Result = new Dictionary<string, object>()
			{
				{ "name", this.Name },
				{ "uri", this.Uri.OriginalString }
			};

			if (!string.IsNullOrEmpty(this.Title))
				Result.Add("title", this.Title);

			if (!string.IsNullOrEmpty(this.Description))
				Result.Add("description", this.Description);

			if (!string.IsNullOrEmpty(this.ContentType))
				Result.Add("mimeType", this.ContentType);

			if (this.Size.HasValue)
				Result.Add("size", this.Size.Value);

			if (!this.Icons.Empty)
				Result.Add("icons", this.Icons.ToJson());

			if (this.HasAnnotations)
			{
				Dictionary<string, object> Annotations = new Dictionary<string, object>();
				this.Annotate(Annotations);
				Result["annotations"] = Annotations;
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
		/// Reads the resource.
		/// </summary>
		/// <param name="MetaData">Associated meta-data, if available.</param>
		/// <returns>Content objects read.</returns>
		public abstract Task<IResourceContent[]> Read(Dictionary<string, object>? MetaData);
	}
}
