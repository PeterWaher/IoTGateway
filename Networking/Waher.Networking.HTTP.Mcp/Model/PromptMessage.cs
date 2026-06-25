using System.Collections.Generic;

namespace Waher.Networking.HTTP.Mcp.Model
{
	/// <summary>
	/// Describes a message returned as part of a prompt.
	/// </summary>
	public class PromptMessage
	{
		/// <summary>
		/// Describes a message returned as part of a prompt.
		/// </summary>
		/// <param name="Role">Role of recipient of message.</param>
		/// <param name="Content">Unencoded content of the message.</param>
		public PromptMessage(McpRole Role, object Content)
		{
			this.Role = Role;
			this.Content = Content;

			if (Content is Dictionary<string, object?> Encoded)
			{
				this.Encoded = Encoded;
				this.IsEncoded = true;
			}
			else
			{
				this.Encoded = null;
				this.IsEncoded = false;
			}
		}

		/// <summary>
		/// Describes a message returned as part of a prompt.
		/// </summary>
		/// <param name="Role">Role of recipient of message.</param>
		/// <param name="Content">Encoded content of the message.</param>
		public PromptMessage(McpRole Role, Dictionary<string, object?> Content)
		{
			this.Role = Role;
			this.Content = Content;
			this.Encoded = Content;
			this.IsEncoded = true;
		}

		/// <summary>
		/// Role of recipient of message.
		/// </summary>
		public McpRole Role { get; }

		/// <summary>
		/// Message content.
		/// </summary>
		public object Content { get; }

		/// <summary>
		/// If the content has been encoded.
		/// </summary>
		public bool IsEncoded { get; }

		/// <summary>
		/// Encoded content.
		/// </summary>
		public Dictionary<string, object?>? Encoded { get; }
	}
}
