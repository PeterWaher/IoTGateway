using System;
using Waher.Networking.CoAP.Transport;

namespace Waher.Networking.CoAP
{
	/// <summary>
	/// Represents a registration on an observable resource.
	/// </summary>
	public class ObservationRegistration
	{
		private readonly ClientBase client;
		private readonly CoapEndpoint endpoint;
		private readonly CoapMessage request;
		private uint seqNr = 0;

		/// <summary>
		/// Represents a registration on an observable resource.
		/// </summary>
		/// <param name="Client">UDP Client on which the request was received.</param>
		/// <param name="Endpoint">CoAP Endpoint managing the resource.</param>
		/// <param name="Request">Request message.</param>
		internal ObservationRegistration(ClientBase Client, CoapEndpoint Endpoint, 
			CoapMessage Request)
		{
			this.client = Client;
			this.endpoint = Endpoint;
			this.request = Request;
		}

		/// <summary>
		/// Client on which the request was received.
		/// </summary>
		internal ClientBase Client => this.client;

		/// <summary>
		/// CoAP Endpoint managing the resource.
		/// </summary>
		public CoapEndpoint Endpoint => this.endpoint;

		/// <summary>
		/// Request message.
		/// </summary>
		public CoapMessage Request => this.request;

		/// <summary>
		/// Sequence number.
		/// </summary>
		public uint SequenceNumber => this.seqNr;

		/// <summary>
		/// Increases the sequence number.
		/// </summary>
		internal void IncSeqNr()
		{
			this.seqNr = (this.seqNr + 1) & 0xffffff;
		}
	}
}
