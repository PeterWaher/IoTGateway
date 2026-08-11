using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Markdown;
using Waher.Networking.HTTP.JsonRpc.Transports;
using Waher.Networking.HTTP.Mcp.Model;
using Waher.Networking.HTTP.Mcp.Model.Server;
using Waher.Networking.Sniffers;
using Waher.Runtime.Collections;
using Waher.Security;

namespace Waher.Networking.HTTP.Mcp
{
	/// <summary>
	/// Manages a set of MCP servers that are joined together and can be accessed as
	/// one web service, for example via STDIO.
	/// </summary>
	public class StdioMcpServer : HttpMcpServerResource
	{
		private readonly HttpMcpServerResource[] resources;
		private readonly bool resourcesRequireAuthentication;
		private readonly bool hasResources = false;

		/// <summary>
		/// Manages a set of MCP servers that are joined together and can be accessed as
		/// one web service, for example via STDIO.
		/// </summary>
		/// <param name="Resources">MCP Server resources to join.</param>
		/// <param name="ResourceName">Name of resource.</param>
		/// <param name="Name">Name of server.</param>
		/// <param name="Title">Title of server.</param>
		/// <param name="Icons">Icons of server.</param>
		/// <param name="WebSiteUri">Website URI of server.</param>
		/// <param name="SnifferSet">Optional sniffer set used to log agent interaction 
		/// with MCP service.</param>
		public StdioMcpServer(HttpMcpServerResource[] Resources, string ResourceName,
			string Name, string Title, Icon[] Icons, Uri? WebSiteUri, ISnifferSet? SnifferSet)
			: base(ResourceName, Name, Title,
				typeof(StdioMcpServer).Assembly.GetName().Version.ToString(),
				GetDescription(Resources),
				GetMarkdownDescription(Resources),
				Icons,
				WebSiteUri,
				GetInstructions(Resources),
				SnifferSet)
		{
			this.resources = Resources;

			foreach (HttpMcpServerResource Resource in Resources)
			{
				foreach (Tool Tool in Resource.GetTools())
					this.RegisterTool(Tool.Method, Tool.Attributes);

				foreach (Prompt Prompt in Resource.GetPrompts())
					this.RegisterPrompt(Prompt.Method, Prompt.Attributes);

				this.hasResources |= Resource.HasResources;
				this.resourcesRequireAuthentication |= Resource.ResourcesRequireAuthentication;
			}
		}

		private static string GetDescription(HttpMcpServerResource[] Resources)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("This MCP Server joins the following MCP servers together, ");
			sb.AppendLine("to provide a single endpoint for them all.");

			foreach (HttpMcpServerResource Resource in Resources)
			{
				sb.AppendLine();
				sb.Append(Resource.Title);
				sb.AppendLine(":");
				sb.AppendLine();
				sb.AppendLine(Resource.Description);
			}

			return sb.ToString();
		}

		private static string GetMarkdownDescription(HttpMcpServerResource[] Resources)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("This MCP Server joins the following MCP servers together, ");
			sb.AppendLine("to provide a single endpoint for them all.");

			foreach (HttpMcpServerResource Resource in Resources)
			{
				sb.AppendLine();
				sb.AppendLine(Resource.Title);
				sb.AppendLine(new string('-', Resource.Title.Length + 3));
				sb.AppendLine();

				sb.AppendLine();
				sb.AppendLine(Resource.MarkdownDescription);
			}

			return sb.ToString();
		}

		private static string GetInstructions(HttpMcpServerResource[] Resources)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("Following are instructions for the different MCP Servers available ");
			sb.AppendLine("via this MCP server.");

			foreach (HttpMcpServerResource Resource in Resources)
			{
				sb.AppendLine();
				sb.Append(Resource.Title);
				sb.AppendLine(":");
				sb.AppendLine();
				sb.AppendLine(Resource.Instructions);
			}

			return sb.ToString();
		}

		/// <summary>
		/// If the MCP server has resource capabilities.
		/// </summary>
		public override bool HasResources => this.hasResources;

		/// <summary>
		/// If resources published by the MCP Server require authentication. If true, 
		/// the client must authenticate before resources can be listed or read.
		/// </summary>
		public override bool ResourcesRequireAuthentication => this.resourcesRequireAuthentication;

		/// <summary>
		/// Gets available resources.
		/// </summary>
		/// <param name="Call">JSON-RPC Call object.</param>
		/// <param name="User">MCP Client user requesting resources.</param>
		/// <param name="Session">MCP Session, if available.</param>
		/// <returns>Array of resources.</returns>
		public override async Task<Resource[]> GetResources(IJsonRpcCall Call, IUser? User, 
			Session? Session)
		{
			ChunkedList<Resource> Result = new ChunkedList<Resource>();

			foreach (HttpMcpServerResource Resource in this.resources)
				Result.AddRange(await Resource.GetResources(Call, User, Session));

			return Result.ToArray();
		}

		/// <summary>
		/// MCP server resource documentation, as an array of key-value pairs.
		/// The Key represents Markdown (true) or plain text (false), and the Value
		/// represents the documentation text. Each entry in the array represents a
		/// paragraph.
		/// </summary>
		public override KeyValuePair<bool, string>[] ResourceDocumentation
		{
			get
			{
				ChunkedList<KeyValuePair<bool, string>> Result = 
					new ChunkedList<KeyValuePair<bool, string>>()
				{
					new KeyValuePair<bool, string>(false,
						"Following are information regarding resources published by the " +
						"different MCP Servers available via this MCP server.")
				};

				foreach (HttpMcpServerResource Resource in this.resources)
				{
					Result.Add(new KeyValuePair<bool, string>(true, "# " + Resource.Title));
					Result.AddRange(Resource.ResourceDocumentation);
				}

				return Result.ToArray();
			}
		}
	}
}
