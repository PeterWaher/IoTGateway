using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Xml;
using Waher.Content.Xml;
using Waher.Events;

namespace Waher.Networking.XMPP
{
	/// <summary>
	/// Implements a HTTP Binding mechanism based on BOSH.
	/// 
	/// XEP-0124: Bidirectional-streams Over Synchronous HTTP (BOSH):
	///	https://xmpp.org/extensions/xep-0124.html
	///	
	/// XEP-0206: XMPP Over BOSH:
	/// https://xmpp.org/extensions/xep-0206.html
	/// </summary>
	public class HttpBinding : ITextTransportLayer
    {
		/// <summary>
		/// http://jabber.org/protocol/httpbind
		/// </summary>
		public const string HttpBindNamespace = "http://jabber.org/protocol/httpbind";

		private HttpClient httpClient;
		private XmppClient xmppClient;
		private string url;
		private long rid;

		/// <summary>
		/// Implements a HTTP Binding mechanism based on BOSH.
		/// </summary>
		/// <param name="Url">URL to remote HTTP endpoint.</param>
		/// <param name="Client">XMPP Client.</param>
		public HttpBinding(string Url, XmppClient Client)
		{
			this.url = Url;
			this.xmppClient = Client;

			this.httpClient = new HttpClient()
			{
				Timeout = TimeSpan.FromMilliseconds(30000)
			};

			this.httpClient.DefaultRequestHeaders.ExpectContinue = false;
			this.rid = BitConverter.ToUInt32(XmppClient.GetRandomBytes(4), 0) + 1;
		}

		/// <summary>
		/// Event raised when a packet has been sent.
		/// </summary>
		public event TextEventHandler OnSent;

		/// <summary>
		/// Event received when text data has been received.
		/// </summary>
		public event TextEventHandler OnReceived;

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting
		/// unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			this.xmppClient = null;

			if (this.httpClient != null)
			{
				this.httpClient.Dispose();
				this.httpClient = null;
			}
		}

		private void RaiseOnSent(string Payload)
		{
			TextEventHandler h = this.OnSent;

			if (h != null)
			{
				try
				{
					h(this, Payload);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		private void RaiseOnReceived(string Payload)
		{
			TextEventHandler h = this.OnReceived;

			if (h != null)
			{
				try
				{
					h(this, Payload);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		/// <summary>
		/// Creates a BOSH session.
		/// </summary>
		public async void CreateSession()
		{
			try
			{
				StringBuilder Xml = new StringBuilder();

				Xml.Append("<body content='text/xml; charset=utf-8' from='");
				Xml.Append(XML.Encode(this.xmppClient.BareJID));
				Xml.Append("' hold='1' rid='");
				Xml.Append(this.rid.ToString());
				Xml.Append("' to='");
				Xml.Append(XML.Encode(this.xmppClient.Domain));
				Xml.Append("' ver='1.11' wait='30' xml:lang='");
				Xml.Append(XML.Encode(this.xmppClient.Language));
				Xml.Append("' xmpp:version='1.0' xmlns='");
				Xml.Append(HttpBindNamespace);
				Xml.Append("' xmlns:xmpp='urn:xmpp:xbosh'/>");

				string s = Xml.ToString();
				HttpContent Content = new StringContent(s, Encoding.UTF8, "text/xml");
				XmlDocument ResponseXml;

				this.RaiseOnSent(s);

				HttpResponseMessage Response = await this.httpClient.PostAsync(this.url, Content);
				Response.EnsureSuccessStatusCode();

				Stream Stream = await Response.Content.ReadAsStreamAsync(); // Regardless of status code, we check for XML content.
				XmlElement Body;

				ResponseXml = new XmlDocument();
				ResponseXml.Load(Stream);

				if ((Body = ResponseXml.DocumentElement) == null || Body.LocalName != "body" ||
					Body.NamespaceURI != HttpBindNamespace)
				{
					throw new Exception("Unexpected response returned.");
				}

				// TODO: Parse body to extract session info.

				this.RaiseOnReceived(Body.InnerXml);
			}
			catch (Exception ex)
			{
				this.xmppClient.ConnectionError(ex);
			}
		}

		/// <summary>
		/// Sends a text packet.
		/// </summary>
		/// <param name="Packet">Text packet.</param>
		public void Send(string Packet)
		{
		}

		/// <summary>
		/// Sends a text packet.
		/// </summary>
		/// <param name="Packet">Text packet.</param>
		/// <param name="DeliveryCallback">Optional method to call when packet has been delivered.</param>
		public void Send(string Packet, EventHandler DeliveryCallback)
		{
		}
	}
}
