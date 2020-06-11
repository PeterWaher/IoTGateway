using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content.Xml;
using Waher.Networking.Sniffers;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Class redirecting sniffer output to a remote client.
	/// </summary>
	public class RemoteSniffer : SnifferBase
	{
		private readonly string id;
		private readonly string fullJID;
		private readonly DateTime expires;
		private readonly ISniffable node;
		private readonly ConcentratorServer concentratorServer;

		/// <summary>
		/// Class redirecting sniffer output to a remote client.
		/// </summary>
		/// <param name="FullJID">Full JID of remote client.</param>
		/// <param name="Expires">When the sniffer should automatically expire.</param>
		/// <param name="Node">Node being sniffed.</param>
		/// <param name="ConcentratorServer">Concentrator server managing nodes.</param>
		public RemoteSniffer(string FullJID, DateTime Expires, ISniffable Node, ConcentratorServer ConcentratorServer)
		{
			this.id = Guid.NewGuid().ToString().Replace("-", string.Empty);
			this.fullJID = FullJID;
			this.expires = Expires;
			this.node = Node;
			this.concentratorServer = ConcentratorServer;
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
		public ISniffable Node => this.node;

		/// <summary>
		/// Concentrator server managing nodes.
		/// </summary>
		public ConcentratorServer ConcentratorServer => this.concentratorServer;

		/// <summary>
		/// If the sniffer has expired.
		/// </summary>
		/// <param name="Now"></param>
		/// <returns></returns>
		private bool HasExpired(DateTime Now)
		{
			if (Now >= this.expires)
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
		public override void ReceiveBinary(DateTime Timestamp, byte[] Data)
		{
			if (this.HasExpired(Timestamp))
				return;

			StringBuilder Xml = this.GetHeader(Timestamp);

			Xml.Append("<rxBin>");
			Xml.Append(Convert.ToBase64String(Data));
			Xml.Append("</rxBin>");

			this.Send(Xml);
		}

		private StringBuilder GetHeader(DateTime Timestamp)
		{
			StringBuilder Xml = new StringBuilder();
			Xml.Append("<sniff xmlns='");
			Xml.Append(ConcentratorServer.NamespaceConcentrator);
			Xml.Append("' snifferId='");
			Xml.Append(this.id);
			Xml.Append("' timestamp='");
			Xml.Append(XML.Encode(Timestamp));
			Xml.Append("'>");

			return Xml;
		}

		private void Send(StringBuilder Xml)
		{
			Xml.Append("</sniff>");
			this.concentratorServer.Client.SendMessage(MessageType.Normal, this.fullJID, Xml.ToString(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
		}

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Data">Binary Data.</param>
		public override void TransmitBinary(DateTime Timestamp, byte[] Data)
		{
			if (this.HasExpired(Timestamp))
				return;

			StringBuilder Xml = this.GetHeader(Timestamp);

			Xml.Append("<txBin>");
			Xml.Append(Convert.ToBase64String(Data));
			Xml.Append("</txBin>");

			this.Send(Xml);
		}

		/// <summary>
		/// Called when text has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text</param>
		public override void ReceiveText(DateTime Timestamp, string Text)
		{
			if (this.HasExpired(Timestamp))
				return;

			StringBuilder Xml = this.GetHeader(Timestamp);

			Xml.Append("<rx>");
			Xml.Append(XML.Encode(Text));
			Xml.Append("</rx>");

			this.Send(Xml);
		}

		/// <summary>
		/// Called when text has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text</param>
		public override void TransmitText(DateTime Timestamp, string Text)
		{
			if (this.HasExpired(Timestamp))
				return;

			StringBuilder Xml = this.GetHeader(Timestamp);

			Xml.Append("<tx>");
			Xml.Append(XML.Encode(Text));
			Xml.Append("</tx>");

			this.Send(Xml);
		}

		/// <summary>
		/// Called to inform the viewer of something.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Comment">Comment.</param>
		public override void Information(DateTime Timestamp, string Comment)
		{
			if (this.HasExpired(Timestamp))
				return;

			StringBuilder Xml = this.GetHeader(Timestamp);

			Xml.Append("<info>");
			Xml.Append(XML.Encode(Comment));
			Xml.Append("</info>");

			this.Send(Xml);
		}

		/// <summary>
		/// Called to inform the viewer of a warning state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Warning">Warning.</param>
		public override void Warning(DateTime Timestamp, string Warning)
		{
			if (this.HasExpired(Timestamp))
				return;

			StringBuilder Xml = this.GetHeader(Timestamp);

			Xml.Append("<warning>");
			Xml.Append(XML.Encode(Warning));
			Xml.Append("</warning>");

			this.Send(Xml);
		}

		/// <summary>
		/// Called to inform the viewer of an error state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Error">Error.</param>
		public override void Error(DateTime Timestamp, string Error)
		{
			if (this.HasExpired(Timestamp))
				return;

			StringBuilder Xml = this.GetHeader(Timestamp);

			Xml.Append("<error>");
			Xml.Append(XML.Encode(Error));
			Xml.Append("</error>");

			this.Send(Xml);
		}

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Exception">Exception.</param>
		public override void Exception(DateTime Timestamp, string Exception)
		{
			if (this.HasExpired(Timestamp))
				return;

			StringBuilder Xml = this.GetHeader(Timestamp);

			Xml.Append("<exception>");
			Xml.Append(XML.Encode(Exception));
			Xml.Append("</exception>");

			this.Send(Xml);
		}
	}
}
