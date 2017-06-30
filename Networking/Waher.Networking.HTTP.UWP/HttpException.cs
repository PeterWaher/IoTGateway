using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Base class of all HTTP Exceptions.
	/// </summary>
	public class HttpException : Exception
	{
		private KeyValuePair<string, string>[] headerFields;
		private int statusCode;

		/// <summary>
		/// Base class of all HTTP Exceptions.
		/// </summary>
		/// <param name="StatusCode">HTTP Status Code.</param>
		/// <param name="Message">HTTP Status Message.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public HttpException(int StatusCode, string Message, params KeyValuePair<string, string>[] HeaderFields)
			: base(Message)
		{
			this.statusCode = StatusCode;
			this.headerFields = HeaderFields;
		}

		/// <summary>
		/// HTTP Status Code.
		/// </summary>
		public int StatusCode
		{
			get { return this.statusCode; }
		}

		/// <summary>
		/// HTTP Header fields to include in the response.
		/// </summary>
		public KeyValuePair<string, string>[] HeaderFields
		{
			get { return this.headerFields; }
		}

	}
}
