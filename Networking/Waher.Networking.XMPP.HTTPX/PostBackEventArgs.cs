using System;
using System.IO;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.HTTPX
{
	/// <summary>
	/// Delegate for post-back events or callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event, or caller of callback method.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task PostBackEventHandler(object Sender, PostBackEventArgs e);

	/// <summary>
	/// Event arguments for post-back events or callbacks.
	/// </summary>
	public class PostBackEventArgs : EventArgs
	{
		private readonly Stream data;
		private readonly object state;
		private readonly string from;
		private readonly string to;
		private readonly string endpointReference;
		private readonly string symmetricCipherReference;

		/// <summary>
		/// Event arguments for post-back events or callbacks.
		/// </summary>
		/// <param name="Data">Posted binary data.</param>
		/// <param name="State">State object related to the request,</param>
		/// <param name="From">From whom the response came.</param>
		/// <param name="To">To whom the response is made.</param>
		/// <param name="EndpointReference">Endpoint Reference</param>
		/// <param name="SymmetricCipherReference">Symmetric Cipher Reference</param>
		public PostBackEventArgs(Stream Data, object State, string From, string To, string EndpointReference, string SymmetricCipherReference)
		{
			this.data = Data;
			this.state = State;
			this.from = From;
			this.to = To;
			this.endpointReference = EndpointReference;
			this.symmetricCipherReference = SymmetricCipherReference;
		}

		/// <summary>
		/// Data stream
		/// </summary>
		public Stream Data => this.data;

		/// <summary>
		/// State object
		/// </summary>
		public object State => this.state;

		/// <summary>
		/// From whom the response came.
		/// </summary>
		public string From => this.from;

		/// <summary>
		/// To whom the response is made.
		/// </summary>
		public string To => this.to;

		/// <summary>
		/// Endpoint reference (if end-to-end encryption has been used).
		/// </summary>
		public string EndpointReference => this.endpointReference;

		/// <summary>
		/// Symmetric Cipher reference (if end-to-end encryption has been used).
		/// </summary>
		public string SymmetricCipherReference => this.symmetricCipherReference;
	}
}
