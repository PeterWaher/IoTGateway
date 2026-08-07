using Waher.Networking.HTTP.Mcp.Model.Attributes;

namespace Waher.Mcp.Xmpp.Responses
{
	/// <summary>
	/// Generic response to an MCP request.
	/// </summary>
	public class GenericResponse
	{
		/// <summary>
		/// Generic response to an MCP request.
		/// </summary>
		/// <param name="Success">Indicates if the operation was successful.</param>
		/// <param name="Message">Message associated with the response.</param>
		public GenericResponse(bool Success, string Message)
		{
			this.Success = Success;
			this.Message = Message;
		}

		/// <summary>
		/// Indicates if the operation was successful.
		/// </summary>
		[McpParameter("Success", "Indicates if the operation was successful.")]
		public bool Success { get; set; }

		/// <summary>
		/// Message associated with the response.
		/// </summary>
		[McpParameter("Message", "Message associated with the response.")]
		public string Message { get; set; }
	}
}
