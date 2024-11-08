using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Waher.Networking.CoAP.Options;
using Waher.Networking.CoAP.Transport;

namespace Waher.Networking.CoAP
{
	/// <summary>
	/// Event arguments for CoAP message callbacks.
	/// </summary>
	public class CoapMessageEventArgs : EventArgs
	{
		private readonly ClientBase client;
		private readonly CoapEndpoint endpoint;
		private readonly CoapMessage message;
		private readonly CoapResource resource;
		private bool responded = false;

		/// <summary>
		/// Event arguments for CoAP response callbacks.
		/// </summary>
		/// <param name="Client">UDP Client.</param>
		/// <param name="Endpoint">CoAP Endpoint.</param>
		/// <param name="Message">CoAP message.</param>
		/// <param name="Resource">CoAP resource if relevant.</param>
		internal CoapMessageEventArgs(ClientBase Client, CoapEndpoint Endpoint, CoapMessage Message,
			CoapResource Resource)
		{
			this.client = Client;
			this.endpoint = Endpoint;
			this.message = Message;
			this.resource = Resource;
		}

		/// <summary>
		/// UDP Client through which the message was received.
		/// </summary>
		internal ClientBase Client => this.client;

		/// <summary>
		/// CoAP resource, if relevant.
		/// </summary>
		internal CoapResource Resource => this.resource;

		/// <summary>
		/// CoAP Endpoint.
		/// </summary>
		public CoapEndpoint Endpoint => this.endpoint;

		/// <summary>
		/// CoAP message received.
		/// </summary>
		public CoapMessage Message => this.message;

		/// <summary>
		/// If a response has been returned.
		/// </summary>
		public bool Responded => this.responded;

		/// <summary>
		/// Returns a response to the caller.
		/// </summary>
		/// <param name="Code">CoAP message code.</param>
		/// <param name="Options">Optional options.</param>
		public Task RespondAsync(CoapCode Code, params CoapOption[] Options)
		{
			return this.RespondAsync(Code, null, 64, Options);
		}

		/// <summary>
		/// Returns a response to the caller.
		/// </summary>
		/// <param name="Code">CoAP message code.</param>
		/// <param name="Payload">Optional payload to be encoded.</param>
		/// <param name="BlockSize">Block size, in case the <paramref name="Payload"/> needs to be divided into blocks.</param>
		/// <param name="Options">Optional options.</param>
		public async Task RespondAsync(CoapCode Code, object Payload, int BlockSize, params CoapOption[] Options)
		{
			KeyValuePair<byte[], int> P = await CoapEndpoint.EncodeAsync(Payload);
			byte[] Data = P.Key;
			int ContentFormat = P.Value;

			if (!CoapEndpoint.HasOption(Options, 12))
				Options = CoapEndpoint.Merge(Options, new CoapOptionContentFormat((ulong)ContentFormat));

			await this.RespondAsync(Code, Data, BlockSize, Options);
		}

		/// <summary>
		/// Returns a response to the caller.
		/// </summary>
		/// <param name="Code">CoAP message code.</param>
		/// <param name="Payload">Optional payload.</param>
		/// <param name="BlockSize">Block size, in case the <paramref name="Payload"/> needs to be divided into blocks.</param>
		/// <param name="Options">Optional options.</param>
		public async Task Respond(CoapCode Code, byte[] Payload, int BlockSize, params CoapOption[] Options)
		{
			if (this.message.Type == CoapMessageType.ACK || this.message.Type == CoapMessageType.RST)
				throw new IOException("You cannot respond to ACK or RST messages.");

			int BlockNr = !(this.message.Block2 is null) ? this.message.Block2.Number : 0;

			await this.endpoint.Transmit(this.client, this.message.From, this.client.IsEncrypted,
				this.responded ? (ushort?)null : this.message.MessageId,
				this.responded ? this.message.Type : CoapMessageType.ACK, Code,
				this.message.Token, false, Payload, BlockNr, BlockSize, this.resource, null, null, null, null, Options);

			this.responded = true;
		}

		/// <summary>
		/// Returns an acknowledgement.
		/// </summary>
		public Task ACK()
		{
			return this.ACK(CoapCode.EmptyMessage);
		}

		/// <summary>
		/// Returns an acknowledgement.
		/// </summary>
		/// <param name="Code">CoAP message code.</param>
		public Task ACK(CoapCode Code)
		{
			this.responded = true;
			return this.endpoint.Transmit(this.client, this.message.From, this.client.IsEncrypted,
				this.message.MessageId, CoapMessageType.ACK, Code,
				Code == CoapCode.EmptyMessage ? (ulong?)null : this.message.Token, false, null, 0, 64, this.resource, null, null, null, null);
		}

		/// <summary>
		/// Returns a reset message.
		/// </summary>
		public Task RST()
		{
			return this.RST(CoapCode.EmptyMessage);
		}

		/// <summary>
		/// Returns a reset message.
		/// </summary>
		/// <param name="Code">CoAP message code.</param>
		public Task RST(CoapCode Code)
		{
			if (this.message.Type == CoapMessageType.ACK || this.message.Type == CoapMessageType.RST)
				throw new IOException("You cannot respond to ACK or RST messages.");

			this.responded = true;
			return this.endpoint.Transmit(this.client, this.message.From, this.client.IsEncrypted,
				this.message.MessageId, CoapMessageType.RST, Code,
				Code == CoapCode.EmptyMessage ? (ulong?)null : this.message.Token, false, null, 0, 64, this.resource, null, null, null, null);
		}
	}
}
