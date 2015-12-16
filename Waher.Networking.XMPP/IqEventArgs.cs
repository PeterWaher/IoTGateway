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
		private XmlElement iq;
		private XmlElement query = null;
		private string id;
		private string to;
		private string from;

		internal IqEventArgs(XmppClient Client, XmlElement Iq, string Id, string To, string From)
		{
			this.client = Client;
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
		/// Returns a response to the current request.
		/// </summary>
		/// <param name="Xml">XML to embed into the response.</param>
		public void IqResult(string Xml)
		{
			this.client.IqResult(this.id, this.from, Xml);
		}

		/// <summary>
		/// Returns an error response to the current request.
		/// </summary>
		/// <param name="Xml">XML to embed into the response.</param>
		public void IqError(string Xml)
		{
			this.client.IqError(this.id, this.from, Xml);
		}
	}
}
