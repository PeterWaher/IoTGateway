using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content.Toon;
using Waher.Networking.HTTP.Mcp.Model.Resources;
using Waher.Networking.HTTP.Mcp.Model.Server;
using Waher.Networking.XMPP.Events;

namespace Waher.Mcp.Xmpp.Resources
{
	/// <summary>
	/// Contains information about a received message.
	/// </summary>
	public class MessageResource : Resource
	{
		private readonly MessageEventArgs? message;
		private readonly string messageId;

		/// <summary>
		/// Contains information about a received message.
		/// </summary>
		/// <param name="MessageId">Message ID</param>
		/// <param name="Message">Received message.</param>
		/// <param name="MetaData">Meta-data associated with resource.</param>
		public MessageResource(string MessageId, MessageEventArgs? Message, params KeyValuePair<string, object>[] MetaData)
			: base(MessageId, MessageId, "Message " + MessageId,
				  CreateMessageUri(MessageId), MetaData)
		{
			this.messageId = MessageId;
			this.message = Message;
		}

		/// <summary>
		/// Creates a Message Resource URI.
		/// </summary>
		/// <param name="MessageId">Message ID of received message.</param>
		/// <returns>URI</returns>
		public static Uri CreateMessageUri(string MessageId)
		{
			return new Uri("mid:" + MessageId);
		}

		/// <summary>
		/// Reads the resource.
		/// </summary>
		/// <param name="MetaData">Associated meta-data, if available.</param>
		/// <returns>Content objects read.</returns>
		public override Task<IResourceContent[]> Read(
			Dictionary<string, object>? MetaData)
		{
			Dictionary<string, object> Contents = new Dictionary<string, object>()
			{
				{ "MessageId", this.messageId }
			};

			if (!(this.message is null))
			{
				Contents["From"] = this.message.From;
				Contents["FromBareJid"] = this.message.FromBareJID;
				Contents["To"] = this.message.To;
				Contents["Body"] = this.message.Body;
				Contents["Type"] = this.message.Type;

				if (!string.IsNullOrEmpty(this.message.Subject))
					Contents["Subject"] = this.message.Subject;

				if (!string.IsNullOrEmpty(this.message.ThreadID))
					Contents["ThreadID"] = this.message.ThreadID;

				if (!string.IsNullOrEmpty(this.message.ParentThreadID))
					Contents["ParentThreadID"] = this.message.ParentThreadID;

				if (!(this.message.Content is null))
					Contents["ContentXml"] = this.message.Content.OuterXml;

				Contents["Ok"] = this.message.Ok;

				if (!this.message.Ok)
				{
					Contents["ErrorCode"] = this.message.ErrorCode;
					Contents["ErrorType"] = this.message.ErrorType;
					Contents["ErrorText"] = this.message.ErrorText;
				}
			}

			string s = TOON.Encode(Contents, false);

			return Task.FromResult(new IResourceContent[]
			{
				new TextContent(this.Uri, s, ToonEncoder.DefaultContentType, MetaData)
			});
		}
	}
}
