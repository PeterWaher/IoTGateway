using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Waher.Networking.CoAP
{
	/// <summary>
	/// Represents a registration on an observable resource.
	/// </summary>
	public class ObservationRegistration
	{
		private UdpClient client;
		private CoapEndpoint endpoint;
		private CoapMessage request;
		private uint seqNr = 0;

		/// <summary>
		/// Represents a registration on an observable resource.
		/// </summary>
		/// <param name="Client">UDP Client on which the request was received.</param>
		/// <param name="Endpoint">CoAP Endpoint managing the resource.</param>
		/// <param name="Request">Request message.</param>
		public ObservationRegistration(UdpClient Client, CoapEndpoint Endpoint, CoapMessage Request)
		{
			this.client = Client;
			this.endpoint = Endpoint;
			this.request = Request;
		}

		/// <summary>
		/// UDP Client on which the request was received.
		/// </summary>
		public UdpClient Client
		{
			get { return this.client; }
		}

		/// <summary>
		/// CoAP Endpoint managing the resource.
		/// </summary>
		public CoapEndpoint Endpoint
		{
			get { return this.endpoint; }
		}

		/// <summary>
		/// Request message.
		/// </summary>
		public CoapMessage Request
		{
			get { return this.request; }
		}

		/// <summary>
		/// Sequence number.
		/// </summary>
		public uint SequenceNumber
		{
			get { return this.seqNr; }
		}

		/// <summary>
		/// Increases the sequence number.
		/// </summary>
		internal void IncSeqNr()
		{
			this.seqNr = (this.seqNr + 1) & 0xffffff;
		}
	}
}
