using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The server detected an infinite loop while processing the request.
	/// </summary>
	public class LoopDetectedException : HttpException
	{
		private const int Code = 508;
		private const string Msg = "Loop Detected";

		/// <summary>
		/// The server detected an infinite loop while processing the request.
		/// </summary>
		public LoopDetectedException()
			: base(Code, Msg)
		{
		}

		/// <summary>
		/// The server detected an infinite loop while processing the request.
		/// </summary>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		public LoopDetectedException(object ContentObject)
			: base(Code, Msg, ContentObject)
		{
		}

		/// <summary>
		/// The server detected an infinite loop while processing the request.
		/// </summary>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		public LoopDetectedException(byte[] Content, string ContentType)
			: base(Code, Msg, Content, ContentType)
		{
		}
	}
}
