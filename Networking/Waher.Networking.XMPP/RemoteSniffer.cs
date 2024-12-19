using System;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Xml;
using Waher.Networking.Sniffers;

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
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where received data begins.</param>
		/// <param name="Count">Number of bytes received.</param>
		public override Task ReceiveBinary(DateTime Timestamp, byte[] Data, int Offset, int Count)
		{
			if (this.HasExpired(Timestamp))
				return Task.CompletedTask;

			StringBuilder Xml = this.GetHeader(Timestamp);

			Xml.Append("<rxBin>");
			Xml.Append(Convert.ToBase64String(Data, Offset, Count));
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
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where transmitted data begins.</param>
		/// <param name="Count">Number of bytes transmitted.</param>
		public override Task TransmitBinary(DateTime Timestamp, byte[] Data, int Offset, int Count)
		{
			if (this.HasExpired(Timestamp))
				return Task.CompletedTask;

			StringBuilder Xml = this.GetHeader(Timestamp);

			Xml.Append("<txBin>");
			Xml.Append(Convert.ToBase64String(Data, Offset, Count));
			Xml.Append("</txBin>");

			return this.Send(Xml);
		}

		/// <summary>
		/// Called when text has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text</param>
		public override Task ReceiveText(DateTime Timestamp, string Text)
		{
			if (this.HasExpired(Timestamp))
				return Task.CompletedTask;

			StringBuilder Xml = this.GetHeader(Timestamp);

			Xml.Append("<rx>");
			Xml.Append(XML.Encode(Text));
			Xml.Append("</rx>");

			return this.Send(Xml);
		}

		/// <summary>
		/// Called when text has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text</param>
		public override Task TransmitText(DateTime Timestamp, string Text)
		{
			if (this.HasExpired(Timestamp))
				return Task.CompletedTask;

			StringBuilder Xml = this.GetHeader(Timestamp);

			Xml.Append("<tx>");
			Xml.Append(XML.Encode(Text));
			Xml.Append("</tx>");

			return this .Send(Xml);
		}

		/// <summary>
		/// Called to inform the viewer of something.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Comment">Comment.</param>
		public override Task Information(DateTime Timestamp, string Comment)
		{
			if (this.HasExpired(Timestamp))
				return Task.CompletedTask;

			StringBuilder Xml = this.GetHeader(Timestamp);

			Xml.Append("<info>");
			Xml.Append(XML.Encode(Comment));
			Xml.Append("</info>");

			return this.Send(Xml);
		}

		/// <summary>
		/// Called to inform the viewer of a warning state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Warning">Warning.</param>
		public override Task Warning(DateTime Timestamp, string Warning)
		{
			if (this.HasExpired(Timestamp))
				return Task.CompletedTask;

			StringBuilder Xml = this.GetHeader(Timestamp);

			Xml.Append("<warning>");
			Xml.Append(XML.Encode(Warning));
			Xml.Append("</warning>");

			return this.Send(Xml);
		}

		/// <summary>
		/// Called to inform the viewer of an error state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Error">Error.</param>
		public override Task Error(DateTime Timestamp, string Error)
		{
			if (this.HasExpired(Timestamp))
				return Task.CompletedTask;

			StringBuilder Xml = this.GetHeader(Timestamp);

			Xml.Append("<error>");
			Xml.Append(XML.Encode(Error));
			Xml.Append("</error>");

			return this.Send(Xml);
		}

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Exception">Exception.</param>
		public override Task Exception(DateTime Timestamp, string Exception)
		{
			if (this.HasExpired(Timestamp))
				return Task.CompletedTask;

			StringBuilder Xml = this.GetHeader(Timestamp);

			Xml.Append("<exception>");
			Xml.Append(XML.Encode(Exception));
			Xml.Append("</exception>");

			return this.Send(Xml);
		}
	}
}
