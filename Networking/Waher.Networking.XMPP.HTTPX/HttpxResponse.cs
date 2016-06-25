using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Networking.HTTP;

namespace Waher.Networking.XMPP.HTTPX
{
	internal class HttpxResponse : TransferEncoding
	{
		private StringBuilder response = new StringBuilder();
		private XmppClient client;
		private string id;
		private string to;
		private int maxChunkSize;
		private bool? chunked = null;
		private int nr = 0;
		private string streamId = null;
		private byte[] chunk = null;
		private int pos;

		public HttpxResponse(XmppClient Client, string Id, string To, int MaxChunkSize)
			: base()
		{
			this.client = Client;
			this.id = Id;
			this.to = To;
			this.maxChunkSize = MaxChunkSize;
		}

		public override void BeforeContent(HttpResponse Response, bool ExpectContent)
		{
			this.response.Append("<resp xmlns='");
			this.response.Append(HttpxClient.Namespace);
			this.response.Append("' version='1.1' statusCode='");
			this.response.Append(Response.StatusCode.ToString());
			this.response.Append("' statusMessage='");
			this.response.Append(XML.Encode(Response.StatusMessage));
			this.response.Append("'><headers xmlns='");
			this.response.Append(HttpxClient.NamespaceHeaders);
			this.response.Append("'>");

			if (Response.ContentLength.HasValue)
				this.chunked = (Response.ContentLength.Value > this.maxChunkSize);
			else if (ExpectContent)
				this.chunked = true;
			else
			{
				if ((Response.StatusCode < 100 || Response.StatusCode > 199) && Response.StatusCode != 204 && Response.StatusCode != 304)
					Response.ContentLength = 0;
			}

			foreach (KeyValuePair<string, string> P in Response.GetHeaders())
			{
				this.response.Append("<header name='");
				this.response.Append(XML.Encode(P.Key));
				this.response.Append("'>");
				this.response.Append(XML.Encode(P.Value));
				this.response.Append("</header>");
			}

			this.response.Append("</headers>");

			if (this.chunked.HasValue)
			{
				if (this.chunked.Value)
				{
					this.streamId = Guid.NewGuid().ToString().Replace("-", string.Empty);
					this.chunk = new byte[this.maxChunkSize];

					this.response.Append("<data><chunkedBase64 streamId='");
					this.response.Append(this.streamId);
					this.response.Append("'/></data>");
					this.ReturnResponse();
				}
				else
					this.response.Append("<data><base64>");
			}
			else
				this.ReturnResponse();
		}

		public override void ContentSent()
		{
			this.ReturnResponse();

			if (this.chunked.HasValue && this.chunked.Value)
				this.SendChunk(true);
		}

		private void ReturnResponse()
		{
			if (this.response != null)
			{
				if (this.chunked.HasValue && !this.chunked.Value)
					this.response.Append("</base64></data>");

				this.response.Append("</resp>");
				this.client.SendIqResult(this.id, this.to, this.response.ToString());
				this.response = null;
			}
		}

		public override bool Decode(byte[] Data, int Offset, int NrRead, out int NrAccepted)
		{
			throw new InternalServerErrorException();	// Will not be called.
		}

		public override void Encode(byte[] Data, int Offset, int NrBytes)
		{
			if (this.chunked.Value)
			{
				int NrLeft = this.maxChunkSize - this.pos;

				while (NrBytes > 0)
				{
					if (NrBytes <= NrLeft)
					{
						Array.Copy(Data, Offset, this.chunk, this.pos, NrBytes);
						this.pos += NrBytes;
						NrBytes = 0;

						if (this.pos >= this.maxChunkSize)
							this.SendChunk(false);
					}
					else
					{
						Array.Copy(Data, Offset, this.chunk, this.pos, NrLeft);
						this.pos += NrLeft;
						Offset += NrLeft;
						NrBytes -= NrLeft;
						this.SendChunk(false);
						NrLeft = this.maxChunkSize;
					}
				}
			}
			else
				this.response.Append(Convert.ToBase64String(Data, Offset, NrBytes, Base64FormattingOptions.None));
		}

		private void SendChunk(bool Last)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<chunk xmlns='");
			Xml.Append(HttpxClient.Namespace);
			Xml.Append("' streamId='");
			Xml.Append(this.streamId);
			Xml.Append("' nr='");
			Xml.Append(this.nr.ToString());

			if (Last)
				Xml.Append("' last='true");

			Xml.Append("'>");

			if (this.pos > 0)
				Xml.Append(Convert.ToBase64String(this.chunk, 0, this.pos, Base64FormattingOptions.None));

			Xml.Append("</chunk>");

			this.client.SendMessage(MessageType.Normal, this.to, Xml.ToString(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

			this.nr++;
			this.pos = 0;
		}

		public override void Flush()
		{
			this.ReturnResponse();

			if (this.pos > 0)
				this.SendChunk(false);
		}
	}
}
