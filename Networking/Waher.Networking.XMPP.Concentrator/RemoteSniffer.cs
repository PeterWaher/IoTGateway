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
	public class RemoteSniffer : ISniffer
	{
		private string id;
		private string fullJID;
		private DateTime expires;
		private ISniffable node;
		private ConcentratorServer concentratorServer;

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
				Xml.Append("<Expired/>");
				this.Send(Xml);

				return true;
			}
			else
				return false;
		}

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Data">Binary Data.</param>
		public void ReceiveBinary(byte[] Data)
		{
			DateTime Now = DateTime.Now;
			if (this.HasExpired(Now))
				return;

			StringBuilder Xml = this.GetHeader(Now);

			Xml.Append("<RxBin>");
			Xml.Append(Convert.ToBase64String(Data));
			Xml.Append("</RxBin>");

			this.Send(Xml);
		}

		private StringBuilder GetHeader(DateTime Now)
		{
			StringBuilder Xml = new StringBuilder();
			Xml.Append("<sniff xmlns='");
			Xml.Append(ConcentratorServer.NamespaceConcentrator);
			Xml.Append("' snifferId='");
			Xml.Append(this.id);
			Xml.Append("' timestamp='");
			Xml.Append(XML.Encode(Now));
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
		/// <param name="Data">Binary Data.</param>
		public void TransmitBinary(byte[] Data)
		{
			DateTime Now = DateTime.Now;
			if (this.HasExpired(Now))
				return;

			StringBuilder Xml = this.GetHeader(Now);

			Xml.Append("<TxBin>");
			Xml.Append(Convert.ToBase64String(Data));
			Xml.Append("</TxBin>");

			this.Send(Xml);
		}

		/// <summary>
		/// Called when text has been received.
		/// </summary>
		/// <param name="Text">Text</param>
		public void ReceiveText(string Text)
		{
			DateTime Now = DateTime.Now;
			if (this.HasExpired(Now))
				return;

			StringBuilder Xml = this.GetHeader(Now);

			Xml.Append("<Rx>");
			Xml.Append(XML.Encode(Text));
			Xml.Append("</Rx>");

			this.Send(Xml);
		}

		/// <summary>
		/// Called when text has been transmitted.
		/// </summary>
		/// <param name="Text">Text</param>
		public void TransmitText(string Text)
		{
			DateTime Now = DateTime.Now;
			if (this.HasExpired(Now))
				return;

			StringBuilder Xml = this.GetHeader(Now);

			Xml.Append("<Tx>");
			Xml.Append(XML.Encode(Text));
			Xml.Append("</Tx>");

			this.Send(Xml);
		}

		/// <summary>
		/// Called to inform the viewer of something.
		/// </summary>
		/// <param name="Comment">Comment.</param>
		public void Information(string Comment)
		{
			DateTime Now = DateTime.Now;
			if (this.HasExpired(Now))
				return;

			StringBuilder Xml = this.GetHeader(Now);

			Xml.Append("<Info>");
			Xml.Append(XML.Encode(Comment));
			Xml.Append("</Info>");

			this.Send(Xml);
		}

		/// <summary>
		/// Called to inform the viewer of a warning state.
		/// </summary>
		/// <param name="Warning">Warning.</param>
		public void Warning(string Warning)
		{
			DateTime Now = DateTime.Now;
			if (this.HasExpired(Now))
				return;

			StringBuilder Xml = this.GetHeader(Now);

			Xml.Append("<Warning>");
			Xml.Append(XML.Encode(Warning));
			Xml.Append("</Warning>");

			this.Send(Xml);
		}

		/// <summary>
		/// Called to inform the viewer of an error state.
		/// </summary>
		/// <param name="Error">Error.</param>
		public void Error(string Error)
		{
			DateTime Now = DateTime.Now;
			if (this.HasExpired(Now))
				return;

			StringBuilder Xml = this.GetHeader(Now);

			Xml.Append("<Error>");
			Xml.Append(XML.Encode(Error));
			Xml.Append("</Error>");

			this.Send(Xml);
		}

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Exception">Exception.</param>
		public void Exception(string Exception)
		{
			DateTime Now = DateTime.Now;
			if (this.HasExpired(Now))
				return;

			StringBuilder Xml = this.GetHeader(Now);

			Xml.Append("<Exception>");
			Xml.Append(XML.Encode(Exception));
			Xml.Append("</Exception>");

			this.Send(Xml);
		}
	}
}
