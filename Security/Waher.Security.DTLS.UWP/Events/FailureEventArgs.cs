using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Security.DTLS
{
	/// <summary>
	/// Delegate for handshake failure events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void FailureEventHandler(object Sender, FailureEventArgs e);

	/// <summary>
	/// Event arguments for handshake failure events.
	/// </summary>
	public class FailureEventArgs : RemoteEndpointEventArgs
	{
		private string reason;
		private AlertDescription descripton;

		/// <summary>
		/// Event arguments for handshake failure events.
		/// </summary>
		/// <param name="RemoteEndpoint">Remote endpoint.</param>
		/// <param name="Reason">Reason for failing.</param>
		/// <param name="Descripton">Alert description.</param>
		public FailureEventArgs(object RemoteEndpoint, string Reason, AlertDescription Descripton)
			: base(RemoteEndpoint)
		{
			this.reason = Reason;
			this.descripton = Descripton;
		}

		/// <summary>
		/// Reason for failing.
		/// </summary>
		public string Reason
		{
			get { return this.reason; }
		}

		/// <summary>
		/// Alert description.
		/// </summary>
		public AlertDescription Description
		{
			get { return this.descripton; }
		}
	}
}
