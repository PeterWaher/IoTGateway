using Waher.Networking.CoAP.Transport;

namespace Waher.Networking.CoAP
{
	/// <summary>
	/// Event arguments for CoAP response callbacks.
	/// </summary>
	public class CoapResponseEventArgs : CoapMessageEventArgs
	{
		private readonly object state;
		private readonly bool ok;

		/// <summary>
		/// Event arguments for CoAP response callbacks.
		/// </summary>
		/// <param name="Client">UDP Client.</param>
		/// <param name="Endpoint">CoAP Endpoint.</param>
		/// <param name="Ok">If the request was successful or not.</param>
		/// <param name="State">State object passed to the original request.</param>
		/// <param name="Message">Response message.</param>
		/// <param name="Resource">Resource</param>
		internal CoapResponseEventArgs(ClientBase Client, CoapEndpoint Endpoint, bool Ok, object State, CoapMessage Message, CoapResource Resource)
			: base(Client, Endpoint, Message, Resource)
		{
			this.ok = Ok;
			this.state = State;
		}

		/// <summary>
		/// If the request was successful or not.
		/// </summary>
		public bool Ok => this.ok;

		/// <summary>
		/// State object passed to the original request.
		/// </summary>
		public object State => this.state;
	}
}
