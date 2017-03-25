using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.CoAP
{
	/// <summary>
	/// Delegate for CoAP response callbacks
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void CoapResponseEventHandler(object Sender, CoapResponseEventArgs e);

	/// <summary>
	/// Event arguments for CoAP response callbacks.
	/// </summary>
	public class CoapResponseEventArgs : EventArgs
	{
		private object state;
		private bool ok;

		/// <summary>
		/// Event arguments for CoAP response callbacks.
		/// </summary>
		/// <param name="Ok">If the request was successful or not.</param>
		/// <param name="State">State object passed to the original request.</param>
		public CoapResponseEventArgs(bool Ok, object State)
		{
			this.ok = Ok;
			this.state = State;
		}

		/// <summary>
		/// If the request was successful or not.
		/// </summary>
		public bool Ok
		{
			get { return this.ok; }
		}

		/// <summary>
		/// State object passed to the original request.
		/// </summary>
		public object State
		{
			get { return this.state; }
		}
	}
}
