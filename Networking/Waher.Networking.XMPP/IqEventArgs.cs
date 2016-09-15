using System;
using System.Collections.Generic;
using System.Xml;
using Waher.Networking.XMPP.StanzaErrors;

namespace Waher.Networking.XMPP
{
	/// <summary>
	/// Event arguments for IQ queries.
	/// </summary>
	public class IqEventArgs : EventArgs
	{
		private XmppClient client;
		private XmppComponent component;
		private IEndToEndEncryption e2eEncryption;
		private XmlElement iq;
		private XmlElement query = null;
		private string id;
		private string to;
		private string toBareJid = null;
		private string from;
		private string fromBareJid = null;

		/// <summary>
		/// Event arguments for IQ queries.
		/// </summary>
		protected IqEventArgs(IqEventArgs e)
		{
			this.client = e.client;
			this.component = e.component;
			this.iq = e.iq;
			this.id = e.id;
			this.to = e.to;
			this.from = e.from;
		}

		/// <summary>
		/// Event arguments for IQ queries.
		/// </summary>
		/// <param name="Client">XMPP Client.</param>
		/// <param name="Iq">IQ element.</param>
		/// <param name="Id">Id attribute of IQ stanza.</param>
		/// <param name="To">To attribute of IQ stanza.</param>
		/// <param name="From">From attribute of IQ stanza.</param>
		public IqEventArgs(XmppClient Client, XmlElement Iq, string Id, string To, string From)
		{
			this.client = Client;
			this.component = null;
			this.e2eEncryption = null;
			this.iq = Iq;
			this.id = Id;
			this.to = To;
			this.from = From;
		}

		/// <summary>
		/// Event arguments for IQ queries.
		/// </summary>
		/// <param name="Component">XMPP Component.</param>
		/// <param name="Iq">IQ element.</param>
		/// <param name="Id">Id attribute of IQ stanza.</param>
		/// <param name="To">To attribute of IQ stanza.</param>
		/// <param name="From">From attribute of IQ stanza.</param>
		public IqEventArgs(XmppComponent Component, XmlElement Iq, string Id, string To, string From)
		{
			this.client = null;
			this.component = Component;
			this.iq = Iq;
			this.id = Id;
			this.to = To;
			this.from = From;
		}

		/// <summary>
		/// Event arguments for IQ queries.
		/// </summary>
		/// <param name="Client">XMPP Client.</param>
		/// <param name="E2eEncryption">End-to-end encryption algorithm used.</param>
		/// <param name="Iq">IQ element.</param>
		/// <param name="Id">Id attribute of IQ stanza.</param>
		/// <param name="To">To attribute of IQ stanza.</param>
		/// <param name="From">From attribute of IQ stanza.</param>
		public IqEventArgs(XmppClient Client, IEndToEndEncryption E2eEncryption, XmlElement Iq, string Id,
			string To, string From)
		{
			this.client = Client;
			this.component = null;
			this.e2eEncryption = E2eEncryption;
			this.iq = Iq;
			this.id = Id;
			this.to = To;
			this.from = From;
		}

		/// <summary>
		/// IQ element.
		/// </summary>
		public XmlElement IQ { get { return this.iq; } }

		/// <summary>
		/// Query element, if found, null otherwise.
		/// </summary>
		public XmlElement Query
		{
			get { return this.query; }
			internal set { this.query = value; }
		}

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
		/// Bare version of the "to" JID.
		/// </summary>
		public string ToBareJid
		{
			get
			{
				if (this.toBareJid == null)
					this.toBareJid = XmppClient.GetBareJID(this.to);

				return this.toBareJid;
			}
		}

		/// <summary>
		/// Bare version of the "from" JID.
		/// </summary>
		public string FromBareJid
		{
			get
			{
				if (this.fromBareJid == null)
					this.fromBareJid = XmppClient.GetBareJID(this.from);

				return this.fromBareJid;
			}
		}

		/// <summary>
		/// If end-to-end encryption was used in the request.
		/// </summary>
		public bool UsesE2eEncryption
		{
			get { return this.e2eEncryption != null; }
		}

		/// <summary>
		/// End-to-end encryption interface, if used in the request.
		/// </summary>
		public IEndToEndEncryption E2eEncryption
		{
			get { return this.e2eEncryption; }
		}

		/// <summary>
		/// Returns a response to the current request.
		/// </summary>
		/// <param name="Xml">XML to embed into the response.</param>
		public void IqResult(string Xml)
		{
			if (this.e2eEncryption != null)
				this.e2eEncryption.SendIqResult(this.client, E2ETransmission.IgnoreIfNotE2E, this.id, this.from, Xml);
			else if (this.client != null)
				this.client.SendIqResult(this.id, this.from, Xml);
			else
				this.component.SendIqResult(this.id, this.to, this.from, Xml);
		}

		/// <summary>
		/// Returns an error response to the current request.
		/// </summary>
		/// <param name="Xml">XML to embed into the response.</param>
		public void IqError(string Xml)
		{
			if (this.e2eEncryption != null)
				this.e2eEncryption.SendIqError(this.client, E2ETransmission.IgnoreIfNotE2E, this.id, this.from, Xml);
			else if (this.client != null)
				this.client.SendIqError(this.id, this.from, Xml);
			else
				this.component.SendIqError(this.id, this.to, this.from, Xml);
		}

		/// <summary>
		/// Returns an error response to the current request.
		/// </summary>
		/// <param name="ex">Internal exception object.</param>
		public void IqError(Exception ex)
		{
			if (this.e2eEncryption != null)
				this.e2eEncryption.SendIqError(this.client, E2ETransmission.IgnoreIfNotE2E, this.id, this.from, ex);
			else if (this.client != null)
				this.client.SendIqError(this.id, this.from, ex);
			else
				this.component.SendIqError(this.id, this.to, this.from, ex);
		}
	}
}
