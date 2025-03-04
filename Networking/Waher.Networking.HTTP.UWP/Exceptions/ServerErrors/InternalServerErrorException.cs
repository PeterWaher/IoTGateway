using System;
using System.Collections.Generic;
using Waher.Events;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The server encountered an unexpected condition which prevented it from fulfilling the request. 
	/// </summary>
	public class InternalServerErrorException : HttpException
	{
		/// <summary>
		/// 500
		/// </summary>
		public const int Code = 500;

		/// <summary>
		/// Internal Server Error
		/// </summary>
		public const string StatusMessage = "Internal Server Error";

		/// <summary>
		/// The server encountered an unexpected condition which prevented it from fulfilling the request. 
		/// </summary>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public InternalServerErrorException(params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, HeaderFields)
		{
		}

		/// <summary>
		/// The server encountered an unexpected condition which prevented it from fulfilling the request. 
		/// </summary>
		/// <param name="Exception">Exception object to return. The object will be encoded before being sent.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public InternalServerErrorException(Exception Exception, params KeyValuePair<string, string>[] HeaderFields)
			: this(Log.UnnestException(Exception).Message, HeaderFields)
		{
		}

		/// <summary>
		/// The server encountered an unexpected condition which prevented it from fulfilling the request. 
		/// </summary>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public InternalServerErrorException(object ContentObject, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, ContentObject, HeaderFields)
		{
		}

		/// <summary>
		/// The server encountered an unexpected condition which prevented it from fulfilling the request. 
		/// </summary>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public InternalServerErrorException(byte[] Content, string ContentType, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, Content, ContentType, HeaderFields)
		{
		}
	}
}
