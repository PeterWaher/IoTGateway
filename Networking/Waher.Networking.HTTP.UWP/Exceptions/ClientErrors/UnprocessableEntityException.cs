using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The request was well-formed but was unable to be followed due to semantic errors.
	/// </summary>
	public class UnprocessableEntityException : HttpException
	{
		private const int Code = 422;
		private const string Msg = "Unprocessable Entity";

		/// <summary>
		/// The request was well-formed but was unable to be followed due to semantic errors.
		/// </summary>
		public UnprocessableEntityException()
			: base(Code, Msg)
		{
		}

		/// <summary>
		/// The request was well-formed but was unable to be followed due to semantic errors.
		/// </summary>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		public UnprocessableEntityException(object ContentObject)
			: base(Code, Msg, ContentObject)
		{
		}

		/// <summary>
		/// The request was well-formed but was unable to be followed due to semantic errors.
		/// </summary>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		public UnprocessableEntityException(byte[] Content, string ContentType)
			: base(Code, Msg, Content, ContentType)
		{
		}
	}
}
