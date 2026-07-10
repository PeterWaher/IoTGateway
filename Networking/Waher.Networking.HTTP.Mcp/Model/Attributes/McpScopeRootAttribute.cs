using System;

namespace Waher.Networking.HTTP.Mcp.Model.Attributes
{
	/// <summary>
	/// Defines a scope root for an MCP server web resource.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
	public class McpScopeRootAttribute : Attribute
	{
		/// <summary>
		/// Defines a scope root for an MCP server web resource.
		/// </summary>
		/// <param name="ScopeRoot">Scope root for the MCP server.</param>
		public McpScopeRootAttribute(string ScopeRoot)
		{
			this.ScopeRoot = ScopeRoot;
		}

		/// <summary>
		/// Scope root for the MCP server.
		/// </summary>
		public string ScopeRoot { get; }
	}
}
