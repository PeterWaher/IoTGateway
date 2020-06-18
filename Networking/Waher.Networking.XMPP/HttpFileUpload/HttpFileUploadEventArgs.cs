using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.HttpFileUpload
{
	/// <summary>
	/// Delegate for HTTP File Upload callback methods.
	/// </summary>
	/// <param name="Sender">Sender.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task HttpFileUploadEventHandler(object Sender, HttpFileUploadEventArgs e);
	
	/// <summary>
	/// Event arguments for HTTP File Upload callback methods.
	/// </summary>
	public class HttpFileUploadEventArgs : IqResultEventArgs
	{
		private readonly KeyValuePair<string, string>[] putHeaders = null;
		private readonly string putUrl = null;
		private readonly string getUrl = null;

		/// <summary>
		/// Event arguments for HTTP File Upload callback methods.
		/// </summary>
		/// <param name="e">IQ response.</param>
		/// <param name="GetUrl">GET URL.</param>
		/// <param name="PutUrl">PUT URL.</param>
		/// <param name="PutHeaders">HTTP Headers for PUT request.</param>
		public HttpFileUploadEventArgs(IqResultEventArgs e, string GetUrl,
			string PutUrl, KeyValuePair<string, string>[] PutHeaders)
			: base(e)
		{
			this.getUrl = GetUrl;
			this.putUrl = PutUrl;
			this.putHeaders = PutHeaders;
		}

		/// <summary>
		/// GET URL.
		/// </summary>
		public string GetUrl => this.getUrl;

		/// <summary>
		/// PUT URL.
		/// </summary>
		public string PutUrl => this.putUrl;

		/// <summary>
		/// HTTP Headers for PUT request.
		/// </summary>
		public KeyValuePair<string, string>[] PutHeaders => this.putHeaders;

	}
}
