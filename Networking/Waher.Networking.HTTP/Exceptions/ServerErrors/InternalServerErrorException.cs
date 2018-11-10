using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The server encountered an unexpected condition which prevented it from fulfilling the request. 
	/// </summary>
	public class InternalServerErrorException : HttpException
	{
		private const int Code = 500;
		private const string Msg = "Internal Server Error";

		/// <summary>
		/// The server encountered an unexpected condition which prevented it from fulfilling the request. 
		/// </summary>
		public InternalServerErrorException()
			: base(Code, Msg)
		{
		}

		/// <summary>
		/// The server encountered an unexpected condition which prevented it from fulfilling the request. 
		/// </summary>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		public InternalServerErrorException(object ContentObject)
			: base(Code, Msg, ContentObject)
		{
		}

		/// <summary>
		/// The server encountered an unexpected condition which prevented it from fulfilling the request. 
		/// </summary>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		public InternalServerErrorException(byte[] Content, string ContentType)
			: base(Code, Msg, Content, ContentType)
		{
		}
	}
}
