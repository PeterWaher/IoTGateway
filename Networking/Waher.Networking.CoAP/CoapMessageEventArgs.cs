using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Waher.Networking.CoAP.Options;
using Waher.Networking.CoAP.Transport;

namespace Waher.Networking.CoAP
{
	/// <summary>
	/// Delegate for CoAP message events
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void CoapMessageEventHandler(object Sender, CoapMessageEventArgs e);

	/// <summary>
	/// Event arguments for CoAP message callbacks.
	/// </summary>
	public class CoapMessageEventArgs : EventArgs
	{
		private ClientBase client;
		private CoapEndpoint endpoint;
		private CoapMessage message;
		private CoapResource resource;
		private bool responded = false;

		/// <summary>
		/// Event arguments for CoAP response callbacks.
		/// </summary>
		/// <param name="Client">UDP Client.</param>
		/// <param name="Endpoint">CoAP Endpoint.</param>
		/// <param name="Message">CoAP message.</param>
		internal CoapMessageEventArgs(ClientBase Client, CoapEndpoint Endpoint, CoapMessage Message, CoapResource Resource)
		{
			this.client = Client;
			this.endpoint = Endpoint;
			this.message = Message;
			this.resource = Resource;
		}

		/// <summary>
		/// UDP Client through which the message was received.
		/// </summary>
		internal ClientBase Client
		{
			get { return this.client; }
		}

		/// <summary>
		/// CoAP resource, if relevant.
		/// </summary>
		internal CoapResource Resource
		{
			get { return this.resource; }
		}

		/// <summary>
		/// CoAP Endpoint.
		/// </summary>
		public CoapEndpoint Endpoint
		{
			get { return this.endpoint; }
		}

		/// <summary>
		/// CoAP message received.
		/// </summary>
		public CoapMessage Message
		{
			get { return this.message; }
		}

		/// <summary>
		/// If a response has been returned.
		/// </summary>
		public bool Responded
		{
			get { return this.responded; }
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
		/// Returns a response to the caller.
		/// </summary>
		/// <param name="Code">CoAP message code.</param>
		/// <param name="Payload">Optional payload.</param>
		/// <param name="BlockSize">Block size, in case the <paramref name="Payload"/> needs to be divided into blocks.</param>
		/// <param name="Options">Optional options.</param>
		public void Respond(CoapCode Code, byte[] Payload, int BlockSize, params CoapOption[] Options)
		{
			if (this.message.Type == CoapMessageType.ACK || this.message.Type == CoapMessageType.RST)
				throw new IOException("You cannot respond to ACK or RST messages.");

			int BlockNr = this.message.Block2 != null ? this.message.Block2.Number : 0;

			this.endpoint.Transmit(this.client, this.message.From, this.client.IsEncrypted,
				this.responded ? (ushort?)null : this.message.MessageId,
				this.responded ? this.message.Type : CoapMessageType.ACK, Code, 
				this.message.Token, false, Payload, BlockNr, BlockSize, this.resource, null, null, null, null, Options);

			this.responded = true;
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
			this.endpoint.Transmit(this.client, this.message.From, this.client.IsEncrypted,
				this.message.MessageId, CoapMessageType.ACK, 
				Code, null, false, null, 0, 64, this.resource, null, null, null, null);
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
		/// <param name="Code">CoAP message code.</param>
		public void RST(CoapCode Code)
		{
			if (this.message.Type == CoapMessageType.ACK || this.message.Type == CoapMessageType.RST)
				throw new IOException("You cannot respond to ACK or RST messages.");

			this.responded = true;
			this.endpoint.Transmit(this.client, this.message.From, this.client.IsEncrypted,
				this.message.MessageId, CoapMessageType.RST, 
				Code, null, false, null, 0, 64, this.resource, null, null, null, null);
		}
	}
}
