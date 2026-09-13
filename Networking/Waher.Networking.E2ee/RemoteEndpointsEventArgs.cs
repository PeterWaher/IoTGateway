using System;
using Waher.Security;

namespace Waher.Networking.E2ee
{
	/// <summary>
	/// Event arguments for remote endpoint events.
	/// </summary>
	public class RemoteEndpointsEventArgs : EventArgs
	{
		/// <summary>
		/// Event arguments for remote endpoint events.
		/// </summary>
		/// <param name="RemoteId">Identifier of the remote endpoint.</param>
		/// <param name="Endpoints">Remote endpoints.</param>
		/// <param name="Ciphers">Remote ciphers.</param>
		public RemoteEndpointsEventArgs(Guid RemoteId, IE2eEndpoint[] Endpoints,
			IE2eSymmetricCipher[] Ciphers)
		{
			this.RemoteId = RemoteId;
			this.Endpoints = Endpoints;
			this.Ciphers = Ciphers;
			this.Valid = true;
		}

		/// <summary>
		/// Identifier of the remote endpoint.
		/// </summary>
		public Guid RemoteId { get; }

		/// <summary>
		/// Remote endpoints.
		/// </summary>
		public IE2eEndpoint[] Endpoints { get; }

		/// <summary>
		/// Remote ciphers.
		/// </summary>
		public IE2eSymmetricCipher[] Ciphers { get; }

		/// <summary>
		/// Indicates if the remote endpoint representation is valid or if the
		/// connection should be terminated.
		/// </summary>
		public bool Valid { get; set; }
	}
}
