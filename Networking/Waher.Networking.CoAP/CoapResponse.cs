using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Waher.Networking.CoAP.Options;
using Waher.Networking.CoAP.Transport;

namespace Waher.Networking.CoAP
{
	/// <summary>
	/// CoAP Method handler.
	/// </summary>
	/// <param name="Request">Incoming request.</param>
	/// <param name="Response">Outgoing response.</param>
	public delegate Task CoapMethodHandler(CoapMessage Request, CoapResponse Response);

	/// <summary>
	/// CoAP response to return to a received request.
	/// </summary>
	public class CoapResponse
	{
		private readonly CoapOption[] additionalResponseOptions;
		private readonly ClientBase client;
		private readonly CoapEndpoint endpoint;
		private readonly CoapMessage request;
		private readonly IPEndPoint remoteEndpoint;
		private readonly Notifications notifications;
		private readonly CoapResource resource;
		private bool responded = false;

		/// <summary>
		/// CoAP response to return to a received request.
		/// </summary>
		/// <param name="Client">Client receiving the response.</param>
		/// <param name="Endpoint">CoAP Endpoint.</param>
		/// <param name="RemoteEndpoint">Remote endpoint.</param>
		/// <param name="Request">Request.</param>
		/// <param name="Notifications">How notifications are sent, if at all.</param>
		/// <param name="Resource">CoAP resource.</param>
		/// <param name="AdditionalResponseOptions">Additional response options.</param>
		internal CoapResponse(ClientBase Client, CoapEndpoint Endpoint, IPEndPoint RemoteEndpoint,
			CoapMessage Request, Notifications Notifications, CoapResource Resource, params CoapOption[] AdditionalResponseOptions)
		{
			this.client = Client;
			this.endpoint = Endpoint;
			this.remoteEndpoint = RemoteEndpoint;
			this.request = Request;
			this.notifications = Notifications;
			this.resource = Resource;
			this.additionalResponseOptions = AdditionalResponseOptions;
		}

		/// <summary>
		/// UDP Client through which the message was received.
		/// </summary>
		internal ClientBase Client => this.client;

		/// <summary>
		/// CoAP Endpoint.
		/// </summary>
		public CoapEndpoint Endpoint => this.endpoint;

		/// <summary>
		/// Remote endpoint.
		/// </summary>
		public IPEndPoint RemoteEndpoint => this.remoteEndpoint;

		/// <summary>
		/// If a response has been returned.
		/// </summary>
		public bool Responded
		{
			get => this.responded;
			internal set => this.responded = value;
		}

		/// <summary>
		/// CoAP request message.
		/// </summary>
		public CoapMessage Request => this.request;

		/// <summary>
		/// How notifications are sent, if at all.
		/// </summary>
		public Notifications Notifications => this.notifications;

		/// <summary>
		/// Additional Response Options
		/// </summary>
		public CoapOption[] AdditionalResponseOptions => this.additionalResponseOptions;

		/// <summary>
		/// Returns a response to the caller.
		/// </summary>
		/// <param name="Code">CoAP message code.</param>
		/// <param name="Options">Optional options.</param>
		public Task Respond(CoapCode Code, params CoapOption[] Options)
		{
			return this.Respond(Code, null, 64, Options);
		}

		/// <summary>
		/// Returns a response to the caller.
		/// </summary>
		/// <param name="Code">CoAP message code.</param>
		/// <param name="Payload">Optional payload.</param>
		/// <param name="BlockSize">Block size, in case the <paramref name="Payload"/> needs to be divided into blocks.</param>
		/// <param name="Options">Optional options.</param>
		public Task Respond(CoapCode Code, byte[] Payload, int BlockSize, params CoapOption[] Options)
		{
			int BlockNr = !(this.request.Block2 is null) ? this.request.Block2.Number : 0;
			return this.Respond(Code, Payload, BlockNr, BlockSize, Options);
		}

		/// <summary>
		/// Returns a response to the caller.
		/// </summary>
		/// <param name="Code">CoAP message code.</param>
		/// <param name="Payload">Optional payload.</param>
		/// <param name="Block2Nr">Block index to transmit.</param>
		/// <param name="BlockSize">Block size, in case the <paramref name="Payload"/> needs to be divided into blocks.</param>
		/// <param name="Options">Optional options.</param>
		internal async Task Respond(CoapCode Code, byte[] Payload, int Block2Nr, int BlockSize,
			params CoapOption[] Options)
		{
			await this.endpoint.Transmit(this.client, this.remoteEndpoint, this.client.IsEncrypted,
				this.responded ? (ushort?)null : this.request.MessageId,
				this.ResponseType, Code, this.request.Token, false, Payload, Block2Nr, BlockSize,
				this.resource, null, null, null, null, CoapEndpoint.Merge(Options, this.additionalResponseOptions));

			this.responded = true;
		}

		private CoapMessageType ResponseType
		{
			get
			{
				if (this.responded)
				{
					switch (this.notifications)
					{
						case Notifications.Acknowledged:
							return CoapMessageType.CON;

						case Notifications.Unacknowledged:
							return CoapMessageType.NON;

						case Notifications.None:
						default:
							// Delayed response. Use same service as original request.
							return this.request.Type;
					}
				}
				else
					return CoapMessageType.ACK;
			}
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

			await this.Respond(Code, Data, BlockSize, Options);
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
			return this.endpoint.Transmit(this.client, this.remoteEndpoint, this.client.IsEncrypted,
				this.request.MessageId, CoapMessageType.ACK, Code,
				Code == CoapCode.EmptyMessage ? (ulong?)null : this.request.Token, false,
				null, 0, 64, this.resource, null, null, null, null);
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
		/// <param name="Code">CoAP mesasge code.</param>
		public Task RST(CoapCode Code)
		{
			this.responded = true;
			return this.endpoint.Transmit(this.client, this.remoteEndpoint, this.client.IsEncrypted,
				this.request.MessageId, CoapMessageType.RST, Code,
				Code == CoapCode.EmptyMessage ? (ulong?)null : this.request.Token, false, null, 0, 64,
				this.resource, null, null, null, null);
		}

	}
}
