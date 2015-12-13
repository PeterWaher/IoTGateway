using System;
using System.Collections.Generic;
using System.Xml;
using Waher.Networking.XMPP.StanzaErrors;

namespace Waher.Networking.XMPP
{
	/// <summary>
	/// Error Type
	/// </summary>
	public enum ErrorType
	{
		/// <summary>
		/// No error
		/// </summary>
		None,

		/// <summary>
		/// Retry after providing credentials
		/// </summary>
		Auth,

		/// <summary>
		/// Do not retry (the error cannot be remedied)
		/// </summary>
		Cancel,

		/// <summary>
		/// Proceed (the condition was only a warning)
		/// </summary>
		Continue,

		/// <summary>
		/// Retry after changing the data sent
		/// </summary>
		Modify,

		/// <summary>
		/// Retry after waiting (the error is temporary)
		/// </summary>
		Wait,

		/// <summary>
		/// Undefined error type
		/// </summary>
		Undefined
	}

	/// <summary>
	/// Event arguments for responses to IQ queries.
	/// </summary>
	public class IqResultEventArgs : EventArgs
	{
		private XmlElement response;
		private XmlElement errorElement = null;
		private ErrorType errorType = ErrorType.None;
		private XmppException stanzaError = null;
		private string errorText = string.Empty;
		private object state;
		private string id;
		private string to;
		private string from;
		private int errorCode;
		private bool ok;

		internal IqResultEventArgs(XmlElement Response, string Id, string To, string From, bool Ok, object State)
		{
			XmlElement E;

			this.response = Response;
			this.id = Id;
			this.to = To;
			this.from = From;
			this.ok = Ok;
			this.state = State;
			this.errorCode = 0;

			if (!Ok)
			{
				foreach (XmlNode N in Response.ChildNodes)
				{
					E = N as XmlElement;
					if (E == null)
						continue;

					if (N.LocalName == "error" && N.NamespaceURI == Response.NamespaceURI)
					{
						this.errorElement = E;
						this.errorCode = XmppClient.XmlAttribute(E, "code", 0);

						switch (XmppClient.XmlAttribute(E, "type"))
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

						this.stanzaError = XmppClient.GetStanzaExceptionObject(E);
						this.errorText = this.stanzaError.Message;
					}
				}
			}
		}

		/// <summary>
		/// IQ Response element.
		/// </summary>
		public XmlElement Response { get { return this.response; } }

		/// <summary>
		/// State object passed to the original request.
		/// </summary>
		public object State { get { return this.state; } }

		/// <summary>
		/// ID of the request.
		/// </summary>
		public string Id { get { return this.id; } }

		/// <summary>
		/// To address attribute
		/// </summary>
		public string To { get { return this.to; } }

		/// <summary>
		/// From address attribute
		/// </summary>
		public string From { get { return this.from; } }

		/// <summary>
		/// If the response is an OK result response (true), or an error response (false).
		/// </summary>
		public bool Ok { get { return this.ok; } }

		/// <summary>
		/// Error Code
		/// </summary>
		public int ErrorCode { get { return this.errorCode; } }

		/// <summary>
		/// Error Type
		/// </summary>
		public ErrorType ErrorType { get { return this.errorType; } }

		/// <summary>
		/// Error element.
		/// </summary>
		public XmlElement ErrorElement { get { return this.errorElement; } }

		/// <summary>
		/// Any error specific text.
		/// </summary>
		public string ErrorText { get { return this.errorText; } }

		/// <summary>
		/// Any stanza error returned.
		/// </summary>
		public XmppException StanzaError { get { return this.stanzaError; } }
	}
}
