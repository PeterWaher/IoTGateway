using System;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Xml;
using Waher.Networking.Sniffers;
using Waher.Networking.Sniffers.Model;

namespace Waher.Networking.XMPP
{
    /// <summary>
    /// Class redirecting sniffer output to a remote client.
    /// </summary>
    public class RemoteSniffer : SnifferBase
	{
		private readonly string id;
		private readonly string fullJID;
		private readonly string @namespace;
		private readonly DateTime expires;
		private readonly ICommunicationLayer node;
		private readonly XmppClient client;

		/// <summary>
		/// Class redirecting sniffer output to a remote client.
		/// </summary>
		/// <param name="FullJID">Full JID of remote client.</param>
		/// <param name="Expires">When the sniffer should automatically expire.</param>
		/// <param name="Node">Node being sniffed.</param>
		/// <param name="Client">XMPP Client transmitting messages.</param>
		/// <param name="Namespace">Namespace used when creating sniffer.</param>
		public RemoteSniffer(string FullJID, DateTime Expires, ICommunicationLayer Node, XmppClient Client, string Namespace)
		{
			this.id = Guid.NewGuid().ToString().Replace("-", string.Empty);
			this.fullJID = FullJID;
			this.expires = Expires.ToUniversalTime();
			this.node = Node;
			this.client = Client;
			this.@namespace = Namespace;
		}

		/// <summary>
		/// ID of sniffer session.
		/// </summary>
		public string Id => this.id;

		/// <summary>
		/// FUll JID of remote client doing the sniffing.
		/// </summary>
		public string FullJID => this.fullJID;

		/// <summary>
		/// When the sniffer should automatically expire.
		/// </summary>
		public DateTime Expires => this.expires;

		/// <summary>
		/// Node being sniffed.
		/// </summary>
		public ICommunicationLayer Node => this.node;

		/// <summary>
		/// XMPP Client transmitting messages.
		/// </summary>
		public XmppClient Client => this.client;

		/// <summary>
		/// Namespace used when creating sniffer.
		/// </summary>
		public string Namespace => this.@namespace;

		/// <summary>
		/// If the sniffer has expired.
		/// </summary>
		/// <param name="Now"></param>
		/// <returns>If sniffer has expired.</returns>
		private bool HasExpired(DateTime Now)
		{
			if (Now.ToUniversalTime() >= this.expires)
			{
				this.node.Remove(this);

				StringBuilder Xml = this.GetHeader(Now);
				Xml.Append("<expired/>");
				this.Send(Xml);

				return true;
			}
			else
				return false;
		}

		/// <summary>
		/// Processes a binary reception event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		public override Task Process(SnifferRxBinary Event)
		{
			if (this.HasExpired(Event.Timestamp))
				return Task.CompletedTask;

			StringBuilder Xml = this.GetHeader(Event.Timestamp);

			Xml.Append("<rxBin>");
			Xml.Append(Convert.ToBase64String(Event.Data, Event.Offset, Event.Count));
			Xml.Append("</rxBin>");

			return this.Send(Xml);
		}

		private StringBuilder GetHeader(DateTime Timestamp)
		{
			StringBuilder Xml = new StringBuilder();
			Xml.Append("<sniff xmlns='");
			Xml.Append(this.@namespace);
			Xml.Append("' snifferId='");
			Xml.Append(this.id);
			Xml.Append("' timestamp='");
			Xml.Append(XML.Encode(Timestamp));
			Xml.Append("'>");

			return Xml;
		}

		private Task Send(StringBuilder Xml)
		{
			Xml.Append("</sniff>");
			this.client.SendMessage(MessageType.Normal, this.fullJID, Xml.ToString(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Processes a binary transmission event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		public override Task Process(SnifferTxBinary Event)
		{
			if (this.HasExpired(Event.Timestamp))
				return Task.CompletedTask;

			StringBuilder Xml = this.GetHeader(Event.Timestamp);

			Xml.Append("<txBin>");
			Xml.Append(Convert.ToBase64String(Event.Data, Event.Offset, Event.Count));
			Xml.Append("</txBin>");

			return this.Send(Xml);
		}

		/// <summary>
		/// Processes a text reception event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		public override Task Process(SnifferRxText Event)
		{
			if (this.HasExpired(Event.Timestamp))
				return Task.CompletedTask;

			StringBuilder Xml = this.GetHeader(Event.Timestamp);

			Xml.Append("<rx>");
			Xml.Append(XML.Encode(Event.Text));
			Xml.Append("</rx>");

			return this.Send(Xml);
		}

		/// <summary>
		/// Processes a text transmission event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		public override Task Process(SnifferTxText Event)
		{
			if (this.HasExpired(Event.Timestamp))
				return Task.CompletedTask;

			StringBuilder Xml = this.GetHeader(Event.Timestamp);

			Xml.Append("<tx>");
			Xml.Append(XML.Encode(Event.Text));
			Xml.Append("</tx>");

			return this.Send(Xml);
		}

		/// <summary>
		/// Processes an information event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		public override Task Process(SnifferInformation Event)
		{
			if (this.HasExpired(Event.Timestamp))
				return Task.CompletedTask;

			StringBuilder Xml = this.GetHeader(Event.Timestamp);

			Xml.Append("<info>");
			Xml.Append(XML.Encode(Event.Text));
			Xml.Append("</info>");

			return this.Send(Xml);
		}

		/// <summary>
		/// Processes a warning event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		public override Task Process(SnifferWarning Event)
		{
			if (this.HasExpired(Event.Timestamp))
				return Task.CompletedTask;

			StringBuilder Xml = this.GetHeader(Event.Timestamp);

			Xml.Append("<warning>");
			Xml.Append(XML.Encode(Event.Text));
			Xml.Append("</warning>");

			return this.Send(Xml);
		}

		/// <summary>
		/// Processes an error event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		public override Task Process(SnifferError Event)
		{
			if (this.HasExpired(Event.Timestamp))
				return Task.CompletedTask;

			StringBuilder Xml = this.GetHeader(Event.Timestamp);

			Xml.Append("<error>");
			Xml.Append(XML.Encode(Event.Text));
			Xml.Append("</error>");

			return this.Send(Xml);
		}

		/// <summary>
		/// Processes an exception event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		public override Task Process(SnifferException Event)
		{
			if (this.HasExpired(Event.Timestamp))
				return Task.CompletedTask;

			StringBuilder Xml = this.GetHeader(Event.Timestamp);

			Xml.Append("<exception>");
			Xml.Append(XML.Encode(Event.Text));
			Xml.Append("</exception>");

			return this.Send(Xml);
		}
	}
}
