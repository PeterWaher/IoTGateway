using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.Mail
{
	/// <summary>
	/// Mail priority
	/// </summary>
	public enum Priority
	{
		/// <summary>
		/// High (1)
		/// </summary>
		High = 1,

		/// <summary>
		/// Normal (3)
		/// </summary>
		Normal = 3,

		/// <summary>
		/// Low (5)
		/// </summary>
		Low = 5
	}

	/// <summary>
	/// Delegate for mail event handlers.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments</param>
	public delegate void MailEventHandler(object Sender, MailEventArgs e);

	/// <summary>
	/// Event arguments for mail message events
	/// </summary>
	public class MailEventArgs : MessageEventArgs
	{
		private readonly string contentType;
		private readonly string messageId;
		private readonly Priority priority;
		private readonly DateTimeOffset date;
		private readonly string fromMail;
		private readonly string fromHeader;
		private readonly string sender;
		private readonly int size;
		private readonly string mailObjectId;
		private readonly KeyValuePair<string, string>[] headers;
		private readonly EmbeddedObjectReference[] attachments;
		private readonly EmbeddedObjectReference[] inline;
		private readonly MailClient client;
		private readonly string plainText;
		private readonly string html;
		private readonly string markdown;

		/// <summary>
		/// Event arguments for mail message events
		/// </summary>
		/// <param name="e">Message stanza event arguments</param>
		/// <param name="Client">Mail Client</param>
		/// <param name="ContentType">Mail Content-Type</param>
		/// <param name="MessageId">Message-ID, if available.</param>
		/// <param name="Priority">Message Priority</param>
		/// <param name="Date">Message Date</param>
		/// <param name="FromMail">From Address, as provided in the MAIL FROM command.</param>
		/// <param name="FromHeader">From Address, as provided in the mail headers.</param>
		/// <param name="Sender">Sender address, as provided in the mail headers.</param>
		/// <param name="Size">Size of message contents.</param>
		/// <param name="MailObjectId">Mail object ID in broker.</param>
		/// <param name="Headers">Mail headers</param>
		/// <param name="Attachments">Attachments, if available.</param>
		/// <param name="Inline">Inline objects, if available.</param>
		/// <param name="PlainText">Plain-text body, if available.</param>
		/// <param name="Html">HTML body, if available.</param>
		/// <param name="Markdown">Markdown body, if available.</param>
		public MailEventArgs(MailClient Client, MessageEventArgs e, string ContentType, string MessageId, Priority Priority, 
			DateTimeOffset Date, string FromMail, string FromHeader, string Sender, int Size, string MailObjectId, 
			KeyValuePair<string, string>[] Headers, EmbeddedObjectReference[] Attachments, EmbeddedObjectReference[] Inline, 
			string PlainText, string Html, string Markdown)
			: base(e)
		{
			this.client = Client;
			this.contentType = ContentType;
			this.messageId = MessageId;
			this.priority = Priority;
			this.date = Date;
			this.fromMail = FromMail;
			this.fromHeader = FromHeader;
			this.sender = Sender;
			this.size = Size;
			this.mailObjectId = MailObjectId;
			this.headers = Headers;
			this.attachments = Attachments;
			this.inline = Inline;
			this.plainText = PlainText;
			this.html = Html;
			this.markdown = Markdown;
		}

		/// <summary>
		/// Mail Client.
		/// </summary>
		public MailClient Client => this.client;

		/// <summary>
		/// Mail Content-Type
		/// </summary>
		public string ContentType => this.contentType;

		/// <summary>
		/// Message-ID, if available.
		/// </summary>
		public string MessageId => this.messageId;

		/// <summary>
		/// Message Priority
		/// </summary>
		public Priority Priority => this.priority;

		/// <summary>
		/// Message Date
		/// </summary>
		public DateTimeOffset Date => this.date;

		/// <summary>
		/// From Address, as provided in the MAIL FROM command.
		/// </summary>
		public string FromMail => this.fromMail;

		/// <summary>
		/// From Address, as provided in the mail headers.
		/// </summary>
		public string FromHeader => this.fromHeader;

		/// <summary>
		/// Sender address, as provided in the mail headers.
		/// </summary>
		public string Sender => this.sender;

		/// <summary>
		/// Size of message contents.
		/// </summary>
		public int Size => this.size;

		/// <summary>
		/// Mail object ID in broker.
		/// </summary>
		public string MailObjectId => this.mailObjectId;

		/// <summary>
		/// Mail headers
		/// </summary>
		public KeyValuePair<string, string>[] Headers => this.headers;

		/// <summary>
		/// Attachments, if available.
		/// </summary>
		public EmbeddedObjectReference[] Attachments => this.attachments;

		/// <summary>
		/// Inline objects, if available.
		/// </summary>
		public EmbeddedObjectReference[] Inline => this.inline;

		/// <summary>
		/// Plain-text body, if available.
		/// </summary>
		public string PlainText => this.plainText;

		/// <summary>
		/// HTML body, if available.
		/// </summary>
		public string Html => this.html;

		/// <summary>
		/// Markdown body, if available.
		/// </summary>
		public string Markdown => this.markdown;
	}
}
