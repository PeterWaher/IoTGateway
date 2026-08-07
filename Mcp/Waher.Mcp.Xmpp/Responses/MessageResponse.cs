using Waher.Networking.HTTP.Mcp.Model.Attributes;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Events;

namespace Waher.Mcp.Xmpp.Responses
{
	/// <summary>
	/// Message response.
	/// </summary>
	public class MessageResponse : GenericResponse
	{
		/// <summary>
		/// Message response.
		/// </summary>
		/// <param name="ErrorMessage">Error message.</param>
		public MessageResponse(string ErrorMessage)
			: base(false, ErrorMessage)
		{
		}

		/// <summary>
		/// Message response.
		/// </summary>
		/// <param name="Message">Message.</param>
		public MessageResponse(MessageEventArgs Message)
			: base(true, "Message retrieved.")
		{
			this.From = Message.From;
			this.FromBareJid = Message.FromBareJID;
			this.To = Message.To;
			this.Body = Message.Body;
			this.Type = Message.Type;
			this.Subject = Message.Subject;
			this.ThreadID = Message.ThreadID;
			this.ParentThreadID = Message.ParentThreadID;
			this.ContentXml = Message.Content?.OuterXml;
			this.ErrorMessage = !Message.Ok;

			if (this.ErrorMessage.Value)
			{
				this.ErrorMessageCode = Message.ErrorCode;
				this.ErrorMessageType = Message.ErrorType;
				this.ErrorMessageText = Message.ErrorText;
			}
		}

		/// <summary>
		/// Full JID of sender of message.
		/// </summary>
		[McpStringParameter("From", "Full JID of sender of message.")]
		public string? From;

		/// <summary>
		/// Bare JID of sender of message.
		/// </summary>
		[McpStringParameter("From (Bare JID)", "Bare JID of sender of message.")]
		public string? FromBareJid;

		/// <summary>
		/// Full JID of recipient of message.
		/// </summary>
		[McpStringParameter("To", "Full JID of recipient of message.")]
		public string? To;

		/// <summary>
		/// Plain text body of message.
		/// </summary>
		[McpStringParameter("Body", "Plain text body of message.")]
		public string? Body;

		/// <summary>
		/// Type of message.
		/// </summary>
		[McpParameter("Type", "Type of message.")]
		public MessageType? Type;

		/// <summary>
		/// Subject of message, if available.
		/// </summary>
		[McpStringParameter("Subject", "Subject of message, if available.")]
		public string? Subject;

		/// <summary>
		/// Thread ID of message, if available.
		/// </summary>
		[McpStringParameter("Thread ID", "Thread ID of message, if available.")]
		public string? ThreadID;

		/// <summary>
		/// Parent thread ID of message, if available.
		/// </summary>
		[McpStringParameter("Parent Thread ID", "Parent thread ID of message, if available.")]
		public string? ParentThreadID;

		/// <summary>
		/// XML content embedded in message, if available.
		/// </summary>
		[McpStringParameter("Content XML", "XML content embedded in message, if available.")]
		public string? ContentXml;

		/// <summary>
		/// If the message retrieved is an error message.
		/// </summary>
		[McpParameter("Error Message", "If the message retrieved is an error message.")]
		public bool? ErrorMessage;

		/// <summary>
		/// Error code of error message, if available.
		/// </summary>
		[McpIntegerParameter("Error Message Code", "Error code of error message, if available.")]
		public int? ErrorMessageCode;

		/// <summary>
		/// Error type of error message, if available.
		/// </summary>
		[McpStringParameter("Error Message Type", "Error type of error message, if available.")]
		public ErrorType? ErrorMessageType;

		/// <summary>error
		/// Error text of error message, if available.
		/// </summary>
		[McpStringParameter("Error Message Text", "Error text of error message, if available.")]
		public string? ErrorMessageText;
	}
}
