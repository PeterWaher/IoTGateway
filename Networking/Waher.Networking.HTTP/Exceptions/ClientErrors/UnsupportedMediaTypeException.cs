using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The server is refusing to service the request because the entity of the request is in a format not supported by the requested resource 
	/// for the requested method. 
	/// </summary>
	public class UnsupportedMediaTypeException : HttpException
	{
		private const int Code = 415;
		private const string Msg = "Unsupported Media Type";

		/// <summary>
		/// The server is refusing to service the request because the entity of the request is in a format not supported by the requested resource 
		/// for the requested method. 
		/// </summary>
		public UnsupportedMediaTypeException()
			: base(Code, Msg)
		{
		}

		/// <summary>
		/// The server is refusing to service the request because the entity of the request is in a format not supported by the requested resource 
		/// for the requested method. 
		/// </summary>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		public UnsupportedMediaTypeException(object ContentObject)
			: base(Code, Msg, ContentObject)
		{
		}

		/// <summary>
		/// The server is refusing to service the request because the entity of the request is in a format not supported by the requested resource 
		/// for the requested method. 
		/// </summary>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		public UnsupportedMediaTypeException(byte[] Content, string ContentType)
			: base(Code, Msg, Content, ContentType)
		{
		}
	}
}
