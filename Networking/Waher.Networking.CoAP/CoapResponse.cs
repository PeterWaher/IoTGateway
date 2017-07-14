using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Waher.Content;
using Waher.Networking.CoAP.Options;

namespace Waher.Networking.CoAP
{
	/// <summary>
	/// CoAP Method handler.
	/// </summary>
	/// <param name="Request">Incoming request.</param>
	/// <param name="Response">Outgoing response.</param>
	public delegate void CoapMethodHandler(CoapMessage Request, CoapResponse Response);

	/// <summary>
	/// CoAP response to return to a received request.
	/// </summary>
	public class CoapResponse
	{
		private UdpClient client;
		private CoapEndpoint endpoint;
		private CoapMessage request;
		private IPEndPoint remoteEndpoint;
		private bool responded = false;

		/// <summary>
		/// CoAP response to return to a received request.
		/// </summary>
		/// <param name="Client">UDP Client.</param>
		/// <param name="Endpoint">CoAP Endpoint.</param>
		/// <param name="RemoteEndpoint">Remote endpoint.</param>
		/// <param name="Request">Request.</param>
		public CoapResponse(UdpClient Client, CoapEndpoint Endpoint, IPEndPoint RemoteEndpoint,
			CoapMessage Request)
		{
			this.client = Client;
			this.endpoint = Endpoint;
			this.remoteEndpoint = RemoteEndpoint;
			this.request = Request;
		}

		/// <summary>
		/// UDP Client through which the message was received.
		/// </summary>
		public UdpClient Client
		{
			get { return this.client; }
		}

		/// <summary>
		/// CoAP Endpoint.
		/// </summary>
		public CoapEndpoint Endpoint
		{
			get { return this.endpoint; }
		}

		/// <summary>
		/// Remote endpoint.
		/// </summary>
		public IPEndPoint RemoteEndpoint
		{
			get { return this.remoteEndpoint; }
		}

		/// <summary>
		/// If a response has been returned.
		/// </summary>
		public bool Responded
		{
			get { return this.responded; }
		}

		/// <summary>
		/// CoAP request message.
		/// </summary>
		public CoapMessage Request
		{
			get { return this.request; }
		}

		/// <summary>
		/// Returns a response to the caller.
		/// </summary>
		/// <param name="Code">CoAP message code.</param>
		/// <param name="Options">Optional options.</param>
		public void Respond(CoapCode Code, params CoapOption[] Options)
		{
			this.Respond(Code, null, 64, Options);
		}

		/// <summary>
		/// Returns a response to the caller.
		/// </summary>
		/// <param name="Code">CoAP message code.</param>
		/// <param name="Payload">Optional payload.</param>
		/// <param name="BlockSize">Block size, in case the <paramref name="Payload"/> needs to be divided into blocks.</param>
		/// <param name="Options">Optional options.</param>
		public void Respond(CoapCode Code, byte[] Payload, int BlockSize, params CoapOption[] Options)
		{
			int BlockNr = this.request.Block2 != null ? this.request.Block2.Number : 0;
			this.Respond(Code, Payload, BlockNr, BlockSize, Options);
		}

		/// <summary>
		/// Returns a response to the caller.
		/// </summary>
		/// <param name="Code">CoAP message code.</param>
		/// <param name="Payload">Optional payload.</param>
		/// <param name="Block2Nr">Block index to transmit.</param>
		/// <param name="BlockSize">Block size, in case the <paramref name="Payload"/> needs to be divided into blocks.</param>
		/// <param name="Options">Optional options.</param>
		internal void Respond(CoapCode Code, byte[] Payload, int Block2Nr, int BlockSize,
			params CoapOption[] Options)
		{
			this.endpoint.Transmit(this.client, this.remoteEndpoint, this.request.MessageId,
				this.responded ? CoapMessageType.CON : CoapMessageType.ACK, Code,
				this.request.Token, false, Payload, Block2Nr, BlockSize, null, null, null, Options);

			this.responded = true;
		}

		/// <summary>
		/// Returns a response to the caller.
		/// </summary>
		/// <param name="Code">CoAP message code.</param>
		/// <param name="Payload">Optional payload to be encoded.</param>
		/// <param name="BlockSize">Block size, in case the <paramref name="Payload"/> needs to be divided into blocks.</param>
		/// <param name="Options">Optional options.</param>
		public void Respond(CoapCode Code, object Payload, int BlockSize, params CoapOption[] Options)
		{
			byte[] Data = CoapEndpoint.Encode(Payload, out int ContentFormat);

			if (!CoapEndpoint.HasOption(Options, 12))
				Options = CoapEndpoint.Merge(Options, new CoapOptionContentFormat((ulong)ContentFormat));

			this.Respond(Code, Data, BlockSize, Options);
		}

		/// <summary>
		/// Returns an acknowledgement.
		/// </summary>
		public void ACK()
		{
			this.ACK(CoapCode.EmptyMessage);
		}

		/// <summary>
		/// Returns an acknowledgement.
		/// </summary>
		/// <param name="Code">CoAP message code.</param>
		public void ACK(CoapCode Code)
		{
			this.responded = true;
			this.endpoint.Transmit(this.client, this.remoteEndpoint, this.request.MessageId,
				CoapMessageType.ACK, Code, null, false, null, 0, 64, null, null, null);
		}

		/// <summary>
		/// Returns a reset message.
		/// </summary>
		public void RST()
		{
			this.RST(CoapCode.EmptyMessage);
		}

		/// <summary>
		/// Returns a reset message.
		/// </summary>
		/// <param name="Code">CoAP mesasge code.</param>
		public void RST(CoapCode Code)
		{
			this.responded = true;
			this.endpoint.Transmit(this.client, this.remoteEndpoint, this.request.MessageId, CoapMessageType.RST,
				Code, null, false, null, 0, 64, null, null, null);
		}

	}
}
