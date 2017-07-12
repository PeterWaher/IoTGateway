using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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
		private CoapEndpoint client;
		private CoapMessage message;
		private bool responded = false;

		/// <summary>
		/// Event arguments for CoAP response callbacks.
		/// </summary>
		/// <param name="Client">CoAP Client.</param>
		/// <param name="Message">CoAP message.</param>
		public CoapMessageEventArgs(CoapEndpoint Client, CoapMessage Message)
		{
			this.client = Client;
			this.message = Message;
		}

		/// <summary>
		/// CoAP Client.
		/// </summary>
		public CoapEndpoint Client
		{
			get { return this.client; }
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
		/// <param name="Payload">Optional payload.</param>
		/// <param name="BlockSize">Block size, in case the <paramref name="Payload"/> needs to be divided into blocks.</param>
		/// <param name="Options">Optional options.</param>
		public void Respond(CoapCode Code, byte[] Payload, int BlockSize, params CoapOption[] Options)
		{
			if (this.message.Type == CoapMessageType.ACK || this.message.Type == CoapMessageType.RST)
				throw new IOException("You cannot respond to ACK or RST messages.");

			this.responded = true;
			this.client.Transmit(this.message.From, this.message.MessageId, CoapMessageType.ACK, Code, this.message.Token, 
				false, Payload, 0, BlockSize, null, null, null, Options);
		}

		/// <summary>
		/// Returns an acknowledgement.
		/// </summary>
		public void ACK()
		{
			this.Respond(CoapCode.EmptyMessage, null, 64);
		}

		/// <summary>
		/// Returns an acknowledgement.
		/// </summary>
		public void RST()
		{
			if (this.message.Type == CoapMessageType.ACK || this.message.Type == CoapMessageType.RST)
				throw new IOException("You cannot respond to ACK or RST messages.");

			this.responded = true;
			this.client.Transmit(this.message.From, this.message.MessageId, CoapMessageType.RST, CoapCode.EmptyMessage, this.message.Token,
				false, null, 0, 64, null, null, null);
		}
	}
}
