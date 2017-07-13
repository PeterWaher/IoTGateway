using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

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
		private CoapEndpoint client;
		private IPEndPoint remoteEndpoint;
		private ushort messageId;
		private ulong token;
		private bool responded = false;

		/// <summary>
		/// CoAP response to return to a received request.
		/// </summary>
		/// <param name="Client">CoAP Client.</param>
		/// <param name="RemoteEndpoint">Remote endpoint.</param>
		/// <param name="MessageId">Message ID.</param>
		/// <param name="Token">Token</param>
		public CoapResponse(CoapEndpoint Client, IPEndPoint RemoteEndpoint, ushort MessageId, ulong Token)
		{
			this.client = Client;
			this.remoteEndpoint = RemoteEndpoint;
			this.messageId = MessageId;
			this.token = Token;
		}

		/// <summary>
		/// CoAP Client.
		/// </summary>
		public CoapEndpoint Client
		{
			get { return this.client; }
		}

		/// <summary>
		/// If a response has been returned.
		/// </summary>
		public bool Responded
		{
			get { return this.responded; }
		}

		/// <summary>
		/// Message ID.
		/// </summary>
		public ushort MessageId
		{
			get { return this.messageId; }
		}

		/// <summary>
		/// Token
		/// </summary>
		public ulong Token
		{
			get { return this.token; }
		}

		/// <summary>
		/// Remote endpoint.
		/// </summary>
		public IPEndPoint RemoteEndpoint
		{
			get { return this.remoteEndpoint; }
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
			this.responded = true;
			this.client.Transmit(this.remoteEndpoint, this.messageId, CoapMessageType.ACK, Code, this.token,
				false, Payload, 0, BlockSize, null, null, null, Options);
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
			this.Respond(CoapCode.EmptyMessage, null, 64);
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
			this.client.Transmit(this.remoteEndpoint, this.messageId, CoapMessageType.RST, Code, this.token,
				false, null, 0, 64, null, null, null);
		}

	}
}
