using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Multipart;
using Waher.Content.Xml;
using Waher.Events;

namespace Waher.Networking.XMPP.Mail
{
	/// <summary>
	/// Client providing support for server mail-extension.
	/// </summary>
	public class MailClient : XmppExtension
	{
		/// <summary>
		/// http://jabber.org/protocol/pubsub
		/// </summary>
		public const string NamespaceMail = "urn:xmpp:smtp";

		/// <summary>
		/// Client providing support for server mail-extension.
		/// </summary>
		/// <param name="Client">XMPP Client to use.</param>
		public MailClient(XmppClient Client)
			: base(Client)
		{
			Client.RegisterMessageHandler("mailInfo", NamespaceMail, this.MailHandler, true);
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			Client.UnregisterMessageHandler("mailInfo", NamespaceMail, this.MailHandler, true);

			base.Dispose();
		}

		/// <summary>
		/// Implemented extensions.
		/// </summary>
		public override string[] Extensions => new string[] { };

		private async Task MailHandler(object Sender, MessageEventArgs e)
		{
			string ContentType = XML.Attribute(e.Content, "contentType");
			string MessageId = XML.Attribute(e.Content, "id");
			int Priority = XML.Attribute(e.Content, "priority", 3);
			DateTimeOffset Date = XML.Attribute(e.Content, "date", DateTimeOffset.Now);
			string FromMail = XML.Attribute(e.Content, "fromMail");
			string FromHeader = XML.Attribute(e.Content, "fromHeader");
			string Sender2 = XML.Attribute(e.Content, "sender");
			int Size = XML.Attribute(e.Content, "size", 0);
			string MailObjectId = XML.Attribute(e.Content, "cid");
			List<KeyValuePair<string, string>> Headers = new List<KeyValuePair<string, string>>();
			List<EmbeddedObjectReference> Attachments = null;
			List<EmbeddedObjectReference> Inline = null;

			foreach (XmlNode N in e.Content.ChildNodes)
			{
				if (N is XmlElement E)
				{
					switch (E.LocalName)
					{
						case "headers":
							foreach (XmlNode N2 in E.ChildNodes)
							{
								switch (N2.LocalName)
								{
									case "header":
										string Key = XML.Attribute((XmlElement)N2, "name");
										string Value = N2.InnerText;

										Headers.Add(new KeyValuePair<string, string>(Key, Value));
										break;
								}
							}
							break;

						case "attachment":
						case "inline":
							string ContentType2 = XML.Attribute(E, "contentType");
							string Description = XML.Attribute(E, "description");
							string FileName = XML.Attribute(E, "fileName");
							string Name = XML.Attribute(E, "name");
							string ContentId = XML.Attribute(E, "id");
							string EmbeddedObjectId = XML.Attribute(E, "cid");
							int Size2 = XML.Attribute(E, "size", 0);

							EmbeddedObjectReference Ref = new EmbeddedObjectReference(ContentType2, Description, FileName, Name,
								ContentId, EmbeddedObjectId, Size);

							if (E.LocalName == "inline")
							{
								if (Inline is null)
									Inline = new List<EmbeddedObjectReference>();

								Inline.Add(Ref);
							}
							else
							{
								if (Attachments is null)
									Attachments = new List<EmbeddedObjectReference>();

								Attachments.Add(Ref);
							}
							break;
					}
				}
			}

			string PlainText = e.Body;
			string Html = null;
			string Markdown = null;

			foreach (XmlNode N in e.Message.ChildNodes)
			{
				if (N is XmlElement E)
				{
					switch (E.LocalName)
					{
						case "content":
							if (E.NamespaceURI == "urn:xmpp:content")
							{
								switch (XML.Attribute(E, "type").ToLower())
								{
									case "text/plain":
										PlainText = E.InnerText;
										break;

									case "text/html":
										Html = E.InnerText;
										break;

									case "text/markdown":
										Markdown = E.InnerText;
										break;
								}
							}
							break;

						case "html":
							if (E.NamespaceURI == "http://jabber.org/protocol/xhtml-im")
							{
								foreach (XmlNode N2 in E.ChildNodes)
								{
									if (N2 is XmlElement E2 && E2.LocalName == "body")
									{
										Html = E2.OuterXml;		// InnerXml introduces xmlns attributes on all child elements.

										int i = Html.IndexOf('>');
										Html = Html.Substring(i + 1);

										i = Html.LastIndexOf('<');
										Html = Html.Substring(0, i);
										break;
									}
								}
							}
							break;
					}
				}
			}

			MailEventHandler h = this.MailReceived;
			if (!(h is null))
			{
				try
				{
					await h(this, new MailEventArgs(this, e, ContentType, MessageId, (Mail.Priority)Priority,
						Date, FromMail, FromHeader, Sender2, Size, MailObjectId, Headers.ToArray(), Attachments?.ToArray(),
						Inline?.ToArray(), PlainText, Html, Markdown));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		/// <summary>
		/// This event is raised when a mail message has been received.
		/// </summary>
		public event MailEventHandler MailReceived = null;

		/// <summary>
		/// Gets a message object from the broker.
		/// </summary>
		/// <param name="ObjectId">ID of the message object to get.</param>
		/// <param name="Callback">Method to call when response has been returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void Get(string ObjectId, MessageObjectEventHandler Callback, object State)
		{
			this.Get(ObjectId, string.Empty, Callback, State);
		}

		/// <summary>
		/// Gets a message object from the broker.
		/// </summary>
		/// <param name="ObjectId">ID of the message object to get.</param>
		/// <param name="ContentType">Content-Type of response, if only part of the mail object is desired.</param>
		/// <param name="Callback">Method to call when response has been returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void Get(string ObjectId, string ContentType, MessageObjectEventHandler Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<get xmlns='");
			Xml.Append(NamespaceMail);
			Xml.Append("' cid='");
			Xml.Append(XML.Encode(ObjectId));

			if (!string.IsNullOrEmpty(ContentType))
			{
				Xml.Append("' type='");
				Xml.Append(XML.Encode(ContentType));
			}

			Xml.Append("'/>");

			this.client.SendIqGet(this.client.Domain, Xml.ToString(), async (sender, e) =>
				{
					XmlElement E;
					string ResponseContentType = null;
					byte[] Data = null;

					if (e.Ok && !((E = e.FirstElement) is null) && E.LocalName == "content" && E.NamespaceURI == NamespaceMail)
					{
						try
						{
							ResponseContentType = XML.Attribute(E, "type");
							Data = Convert.FromBase64String(E.InnerText);
						}
						catch (Exception)
						{
							e.Ok = false;
						}
					}
					else
						e.Ok = false;

					if (!(Callback is null))
					{
						try
						{
							await Callback(this, new MessageObjectEventArgs(e, ResponseContentType, Data));
						}
						catch (Exception ex)
						{
							Log.Critical(ex);
						}
					}
				}, State);
		}

		/// <summary>
		/// Gets a message object from the broker.
		/// </summary>
		/// <param name="ObjectId">ID of the message object to get.</param>
		public Task<MessageObject> GetAsync(string ObjectId)
		{
			return GetAsync(ObjectId, string.Empty);
		}

		/// <summary>
		/// Gets a message object from the broker.
		/// </summary>
		/// <param name="ObjectId">ID of the message object to get.</param>
		/// <param name="ContentType">Content-Type of response, if only part of the mail object is desired.</param>
		public Task<MessageObject> GetAsync(string ObjectId, string ContentType)
		{
			TaskCompletionSource<MessageObject> Result = new TaskCompletionSource<MessageObject>();
			
			this.Get(ObjectId, ContentType, (sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(new MessageObject(e.Data, e.ContentType));
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to get message object."));

				return Task.CompletedTask;

			}, null);

			return Result.Task;
		}

		/// <summary>
		/// Deletes a message object from the broker.
		/// </summary>
		/// <param name="ObjectId">ID of the message object to delete.</param>
		/// <param name="Callback">Method to call when response has been returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void Delete(string ObjectId, IqResultEventHandlerAsync Callback, object State)
		{
			this.client.SendIqSet(this.client.Domain, "<delete xmlns='" + NamespaceMail + "' cid='" + XML.Encode(ObjectId) + "'/>",
				Callback, State);
		}

		/// <summary>
		/// Deletes a message object from the broker.
		/// </summary>
		/// <param name="ObjectId">ID of the message object to delete.</param>
		public Task DeleteAsync(string ObjectId)
		{
			TaskCompletionSource<bool> Result = new TaskCompletionSource<bool>();

			this.Delete(ObjectId, (sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(true);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to delete message object."));

				return Task.CompletedTask;

			}, null);

			return Result.Task;
		}

		/// <summary>
		/// Appends embedded content to a message XML output.
		/// </summary>
		/// <param name="ContentId">Content ID</param>
		/// <param name="ContentType">Content-Type of embedded content.</param>
		/// <param name="ContentData">Binary encoding of content to embed.</param>
		/// <param name="Disposition">Content disposition.</param>
		/// <param name="Name">Name attribute</param>
		/// <param name="FileName">File-name attribute</param>
		/// <param name="Description">Content description.</param>
		/// <param name="Xml">XML output.</param>
		public void AppendContent(string ContentId, string ContentType, byte[] ContentData, ContentDisposition Disposition,
			string Name, string FileName, string Description, StringBuilder Xml)
		{
			Xml.Append("<content xmlns='");
			Xml.Append(NamespaceMail);
			Xml.Append("' cid='");
			Xml.Append(XML.Encode(ContentId));
			Xml.Append("' type='");
			Xml.Append(XML.Encode(ContentType));

			if (Disposition != ContentDisposition.Unknown)
			{
				Xml.Append("' disposition='");
				Xml.Append(XML.Encode(Disposition.ToString()));
			}

			if (!string.IsNullOrEmpty(Name))
			{
				Xml.Append("' name='");
				Xml.Append(XML.Encode(Name));
			}

			if (!string.IsNullOrEmpty(FileName))
			{
				Xml.Append("' fileName='");
				Xml.Append(XML.Encode(FileName));
			}

			if (!string.IsNullOrEmpty(Description))
			{
				Xml.Append("' description='");
				Xml.Append(XML.Encode(Description));
			}

			Xml.Append("'>");
			Xml.Append(Convert.ToBase64String(ContentData));		// TODO: Chunked transfer
			Xml.Append("</content>");
		}

	}
}
