using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The request was directed at a server that is not able to produce a response (for example because a connection reuse).
	/// </summary>
	public class MisdirectedRequestException : HttpException
	{
		private const int Code = 421;
		private const string Msg = "Misdirected Request";

		/// <summary>
		/// The request was directed at a server that is not able to produce a response (for example because a connection reuse).
		/// </summary>
		public MisdirectedRequestException()
			: base(Code, Msg)
		{
		}

		/// <summary>
		/// The request was directed at a server that is not able to produce a response (for example because a connection reuse).
		/// </summary>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		public MisdirectedRequestException(object ContentObject)
			: base(Code, Msg, ContentObject)
		{
		}

		/// <summary>
		/// The request was directed at a server that is not able to produce a response (for example because a connection reuse).
		/// </summary>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		public MisdirectedRequestException(byte[] Content, string ContentType)
			: base(Code, Msg, Content, ContentType)
		{
		}
	}
}
