using System;
using System.Collections.Generic;
using Waher.Content;
using Waher.Networking.HTTP.HeaderFields;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Base class of all HTTP Exceptions.
	/// </summary>
	public class HttpException : Exception
	{
		private readonly KeyValuePair<string, string>[] headerFields;
		private readonly int statusCode;
		private readonly byte[] content = null;
		private readonly string contentType = null;
		private object contentObject = null;

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
		/// Base class of all HTTP Exceptions.
		/// </summary>
		/// <param name="StatusCode">HTTP Status Code.</param>
		/// <param name="Message">HTTP Status Message.</param>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public HttpException(int StatusCode, string Message, object ContentObject, params KeyValuePair<string, string>[] HeaderFields)
			: base(Message)
		{
			this.statusCode = StatusCode;
			this.headerFields = HeaderFields;
			this.contentObject = ContentObject;
		}

		/// <summary>
		/// Base class of all HTTP Exceptions.
		/// </summary>
		/// <param name="StatusCode">HTTP Status Code.</param>
		/// <param name="Message">HTTP Status Message.</param>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public HttpException(int StatusCode, string Message, byte[] Content, string ContentType, params KeyValuePair<string, string>[] HeaderFields)
			: base(Message)
		{
			this.statusCode = StatusCode;
			this.headerFields = HeaderFields;
			this.content = Content;
			this.contentType = ContentType;
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

		/// <summary>
		/// Any content object to return. The object will be encoded before being sent.
		/// </summary>
		public object ContentObject
		{
			get
			{
				if (!(this.contentObject is null))
					return this.contentObject;

				if (this.content is null)
					return null;

				try
				{
					HttpFieldContentType ContentType = new HttpFieldContentType("Content-Type", this.contentType);
					this.contentObject = InternetContent.Decode(ContentType.Type, this.content, ContentType.Encoding, null, null);
				}
				catch (Exception)
				{
					this.contentObject = this.content;
				}

				return this.contentObject;
			}
		}

		/// <summary>
		/// Any encoded content to return.
		/// </summary>
		public byte[] Content => this.content;

		/// <summary>
		/// The content type of <see cref="Content"/>, if provided.
		/// </summary>
		public string ContentType => this.contentType;

	}
}
