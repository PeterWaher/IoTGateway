using System;
using System.Collections.Generic;
using System.Xml;
using Waher.Content.Xml;

namespace Waher.Networking.XMPP.Events
{
	/// <summary>
	/// Event arguments for message events.
	/// </summary>
	public class MessageEventArgs : EventArgs
	{
		private readonly KeyValuePair<string, string>[] bodies;
		private readonly KeyValuePair<string, string>[] subjects;
		private IEndToEndEncryption e2eEncryption = null;
		private readonly XmlElement message;
		private XmlElement content;
		private readonly XmlElement errorElement = null;
		private readonly ErrorType errorType = ErrorType.None;
		private readonly XmppException stanzaError = null;
		private readonly string errorText = string.Empty;
		private readonly XmppClient client;
		private readonly XmppComponent component;
		private readonly MessageType type;
		private readonly string threadId;
		private readonly string parentThreadId;
		private string from;
		private string fromBareJid;
		private string to;
		private string id;
		private readonly string body;
		private readonly string subject;
        private string e2eReference = null;
        private readonly int errorCode;
		private readonly bool ok;

		/// <summary>
		/// Event arguments for message events.
		/// </summary>
		/// <param name="e">Values are taken from this object.</param>
		public MessageEventArgs(MessageEventArgs e)
		{
			this.bodies = e?.bodies;
			this.subjects = e?.subjects;
			this.message = e?.message;
			this.content = e?.content;
			this.errorElement = e?.errorElement;
			this.errorType = e?.errorType ?? ErrorType.Undefined;
			this.stanzaError = e?.stanzaError;
			this.errorText = e?.errorText;
			this.client = e?.client;
			this.component = e?.component;
			this.type = e?.type ?? MessageType.Normal;
			this.threadId = e?.threadId;
			this.parentThreadId = e?.parentThreadId;
			this.from = e?.from;
			this.fromBareJid = e?.fromBareJid;
			this.to = e?.to;
			this.id = e?.id;
			this.body = e?.body;
			this.subject = e?.subject;
			this.errorCode = e?.errorCode ?? 0;
			this.ok = e?.ok ?? false;
            this.e2eEncryption = e?.e2eEncryption;
            this.e2eReference = e?.e2eReference;
        }

        /// <summary>
        /// Event arguments for message events.
        /// </summary>
        /// <param name="Client">Client</param>
        /// <param name="Message">Message element.</param>
        public MessageEventArgs(XmppClient Client, XmlElement Message)
			: this(Client, null, Message)
		{
		}

		/// <summary>
		/// Event arguments for message events.
		/// </summary>
		/// <param name="Component">Component</param>
		/// <param name="Message">Message element.</param>
		public MessageEventArgs(XmppComponent Component, XmlElement Message)
			: this(null, Component, Message)
		{
		}

		private MessageEventArgs(XmppClient Client, XmppComponent Component, XmlElement Message)
		{
			XmlElement E;

			this.message = Message;
			this.client = Client;
			this.component = Component;
			this.from = XML.Attribute(Message, "from");
			this.to = XML.Attribute(Message, "to");
			this.id = XML.Attribute(Message, "id");
			this.ok = true;
			this.errorCode = 0;

			this.fromBareJid = XmppClient.GetBareJID(this.from);

			switch (XML.Attribute(Message, "type").ToLower())
			{
				case "chat":
					this.type = MessageType.Chat;
					break;

				case "error":
					this.type = MessageType.Error;
					this.ok = false;
					break;

				case "groupchat":
					this.type = MessageType.GroupChat;
					break;

				case "headline":
					this.type = MessageType.Headline;
					break;

				case "normal":
				default:
					this.type = MessageType.Normal;
					break;
			}

			SortedDictionary<string, string> Bodies = new SortedDictionary<string, string>();
			SortedDictionary<string, string> Subjects = new SortedDictionary<string, string>();

			foreach (XmlNode N in Message.ChildNodes)
			{
				E = N as XmlElement;
				if (E is null)
					continue;

				if (E.NamespaceURI == Message.NamespaceURI)
				{
					switch (E.LocalName)
					{
						case "body":
							if (string.IsNullOrEmpty(this.body))
								this.body = N.InnerText;

							string Language = XML.Attribute(E, "xml:lang");
							Bodies[Language] = N.InnerText;
							break;

						case "subject":
							if (string.IsNullOrEmpty(this.subject))
								this.subject = N.InnerText;

							Language = XML.Attribute(E, "xml:lang");
							Subjects[Language] = N.InnerText;
							break;

						case "thread":
							this.threadId = N.InnerText;
							this.parentThreadId = XML.Attribute(E, "parent");
							break;

						case "error":
							this.errorElement = E;
							this.errorCode = XML.Attribute(E, "code", 0);
							this.ok = false;

							switch (XML.Attribute(E, "type"))
							{
								case "auth":
									this.errorType = ErrorType.Auth;
									break;

								case "cancel":
									this.errorType = ErrorType.Cancel;
									break;

								case "continue":
									this.errorType = ErrorType.Continue;
									break;

								case "modify":
									this.errorType = ErrorType.Modify;
									break;

								case "wait":
									this.errorType = ErrorType.Wait;
									break;

								default:
									this.errorType = ErrorType.Undefined;
									break;
							}

							this.stanzaError = XmppClient.GetExceptionObject(E);
							this.errorText = this.stanzaError.Message;
							break;
					}
				}
				else if (this.content is null)
					this.content = E;
			}

			this.bodies = new KeyValuePair<string, string>[Bodies.Count];
			Bodies.CopyTo(this.bodies, 0);

			this.subjects = new KeyValuePair<string, string>[Subjects.Count];
			Subjects.CopyTo(this.subjects, 0);
		}

		/// <summary>
		/// The message stanza.
		/// </summary>
		public XmlElement Message => this.message;

		/// <summary>
		/// Content of the message. For messages that are processed by registered message handlers, this value points to the element inside
		/// the message stanza, that the handler is registered to handle. For other types of messages, it represents the first custom element
		/// in the message. If no such elements are found, this value is null.
		/// </summary>
		public XmlElement Content
		{
			get => this.content;
			internal set => this.content = value;
		}

		/// <summary>
		/// Type of message received.
		/// </summary>
		public MessageType Type => this.type;

		/// <summary>
		/// From where the message was received.
		/// </summary>
		public string From
		{
			get => this.from;
			set
			{
				this.from = value;
				this.fromBareJid = XmppClient.GetBareJID(value);
			}
		}

		/// <summary>
		/// Bare JID of resource sending the message.
		/// </summary>
		public string FromBareJID => this.fromBareJid;

		/// <summary>
		/// To whom the message was sent.
		/// </summary>
		public string To
		{
			get => this.to;
			set => this.to = value;
		}

		/// <summary>
		/// ID attribute of message stanza.
		/// </summary>
		public string Id
		{
			get => this.id;
			set => this.id = value;
		}

		/// <summary>
		/// Human readable subject.
		/// </summary>
		public string Subject => this.subject;

		/// <summary>
		/// Human readable body.
		/// </summary>
		public string Body => this.body;

		/// <summary>
		/// Thread ID.
		/// </summary>
		public string ThreadID => this.threadId;

		/// <summary>
		/// Parent Thraed ID.
		/// </summary>
		public string ParentThreadID => this.parentThreadId;

		/// <summary>
		/// If the response is an OK result response (true), or an error response (false).
		/// </summary>
		public bool Ok => this.ok;

		/// <summary>
		/// Error Code
		/// </summary>
		public int ErrorCode => this.errorCode;

		/// <summary>
		/// Error Type
		/// </summary>
		public ErrorType ErrorType => this.errorType;

		/// <summary>
		/// Error element.
		/// </summary>
		public XmlElement ErrorElement => this.errorElement;

		/// <summary>
		/// Any error specific text.
		/// </summary>
		public string ErrorText => this.errorText;

		/// <summary>
		/// Any stanza error returned.
		/// </summary>
		public XmppException StanzaError => this.stanzaError;

		/// <summary>
		/// Available set of (language,body) pairs.
		/// </summary>
		public KeyValuePair<string, string>[] Bodies => this.bodies;

		/// <summary>
		/// Available set of (language,subject) pairs.
		/// </summary>
		public KeyValuePair<string, string>[] Subjects => this.subjects;

		/// <summary>
		/// If end-to-end encryption was used in the request.
		/// </summary>
		public bool UsesE2eEncryption
		{
			get { return !(this.e2eEncryption is null); }
		}

		/// <summary>
		/// End-to-end encryption interface, if used in the request.
		/// </summary>
		public IEndToEndEncryption E2eEncryption
		{
			get => this.e2eEncryption;
			set => this.e2eEncryption = value;
		}

        /// <summary>
        /// Reference to End-to-end encryption endpoint used.
        /// </summary>
        public string E2eReference
        {
            get => this.e2eReference;
            set => this.e2eReference = value;
        }

    }
}
