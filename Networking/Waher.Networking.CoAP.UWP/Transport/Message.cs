using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Waher.Events;
using Waher.Networking.CoAP.Options;
using Waher.Security.DTLS;

namespace Waher.Networking.CoAP.Transport
{
	internal class Message
	{
		public CoapResponseEventHandler callback;
		public CoapEndpoint endpoint;
		public IDtlsCredentials credentials;
		public object state;
		public IPEndPoint destination;
		public CoapMessageType messageType;
		public CoapCode messageCode;
		public CoapOption[] options;
		public CoapResource resource;
		public byte[] payload;
		public int blockSize;
		public int blockNr;
		public MemoryStream payloadResponseStream = null;
		public ushort messageID;
		public ulong token;
		public byte[] encoded;
		public int timeoutMilliseconds;
		public int retryCount = 0;
		public bool acknowledged;
		public bool responseReceived = false;

		internal void ResponseReceived(ClientBase Client, CoapMessage Response)
		{
			this.responseReceived = true;

			if (this.callback != null)
			{
				try
				{
					this.callback(this.endpoint, new CoapResponseEventArgs(Client, this.endpoint,
						Response.Type != CoapMessageType.RST && (int)Response.Code >= 0x40 && (int)Response.Code <= 0x5f,
						this.state, Response, null));
				}
				catch (Exception ex)
				{
					this.endpoint.Exception(ex);
					Log.Critical(ex);
				}
			}
		}

		internal byte[] BlockReceived(ClientBase Client, CoapMessage IncomingMessage)
		{
			if (this.payloadResponseStream is null)
				this.payloadResponseStream = new MemoryStream();

			if (IncomingMessage.Payload != null)
				this.payloadResponseStream.Write(IncomingMessage.Payload, 0, IncomingMessage.Payload.Length);

			if (IncomingMessage.Block2.More)
			{
				List<CoapOption> Options = new List<CoapOption>();

				if (this.options != null)
				{
					foreach (CoapOption Option in this.options)
					{
						if (!(Option is CoapOptionBlock2))
							Options.Add(Option);
					}
				}

				Options.Add(new CoapOptionBlock2(IncomingMessage.Block2.Number + 1, false, IncomingMessage.Block2.Size));

				this.endpoint.Transmit(Client, this.destination, Client.IsEncrypted, null,
					this.messageType == CoapMessageType.ACK ? CoapMessageType.CON : this.messageType,
					this.messageCode, this.token, true, null, 0, this.blockSize, this.resource, this.callback, this.state,
					this.payloadResponseStream, this.credentials, Options.ToArray());

				return null;
			}
			else
			{
				byte[] Result = this.payloadResponseStream.ToArray();
				this.payloadResponseStream.Dispose();
				this.payloadResponseStream = null;

				return Result;
			}
		}

		internal string GetUri()
		{
			string Host = null;
			int? Port = null;
			string Path = null;

			if (this.options != null)
			{
				foreach (CoapOption Option in this.options)
				{
					switch (Option.OptionNumber)
					{
						case 3:
							Host = ((CoapOptionUriHost)Option).Value;
							break;

						case 7:
							Port = (int)((CoapOptionUriPort)Option).Value;
							break;

						case 11:
							if (Path is null)
								Path = "/";
							else
								Path += "/";

							Path += ((CoapOptionUriPath)Option).Value;
							break;
					}
				}
			}

			return CoapEndpoint.GetUri(Host, Port, Path, null);
		}
	}
}
