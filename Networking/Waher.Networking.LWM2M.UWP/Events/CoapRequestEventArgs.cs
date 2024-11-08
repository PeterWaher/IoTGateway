using System;
using Waher.Networking.CoAP;

namespace Waher.Networking.LWM2M.Events
{
	/// <summary>
	/// Event arguments for CoAP request events.
	/// </summary>
    public class CoapRequestEventArgs : EventArgs
    {
		private readonly CoapMessage request;

		/// <summary>
		/// Event arguments for CoAP request events.
		/// </summary>
		/// <param name="Request">Request</param>
		public CoapRequestEventArgs(CoapMessage Request)
		{
			this.request = Request;
		}

		/// <summary>
		/// Request message.
		/// </summary>
		public CoapMessage Request => this.request;
    }
}
