using System;

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
		/// <param name="CanModifyEnvironment">If the tool can modify the environment. If false, 
		/// the tool is expected to be read-only and not cause any side effects.</param>
		/// <param name="CanDestroyEnvironment">If true, the tool may perform destructive 
		/// updates to its environment. If false, the tool performs only additive updates.</param>
		/// <param name="Idempotent">If true, calling the tool repeatedly with the same 
		/// arguments will have no additional effect on its environment.</param>
		/// <param name="OpenWorldAccess">If true, this tool may interact with an 
		/// "open world" of external entities. If false, the tool's domain of interaction 
		/// is closed.</param>
		public McpServerToolAttribute(string Title, string Description,
			bool CanModifyEnvironment, bool CanDestroyEnvironment, bool Idempotent,
			bool OpenWorldAccess)
		{
			this.Title = Title;
			this.Description = Description;
			this.CanModifyEnvironment = CanModifyEnvironment;
			this.CanDestroyEnvironment = CanDestroyEnvironment;
			this.Idempotent = Idempotent;
			this.OpenWorldAccess = OpenWorldAccess;
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
	}
}
